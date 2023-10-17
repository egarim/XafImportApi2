using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Pdf.Native.BouncyCastle.Utilities;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XafImportApi.Module.BusinessObjects;
using static DevExpress.Drawing.Printing.Internal.DXPageSizeInfo;

namespace XafImportApi.Module.Import
{
    public enum PropertyKind
    {
        Primitive, Reference, Enum,

    }
    public class PropertyInfo
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public PropertyKind PropertyKind { get; set; }
        public string ReferencePropertyLookup { get; set; }
        public bool IsBusinessKey { get; set; }
    }
    public class RowDef
    {
        public string ObjectType { get; set; }
        public Dictionary<int, PropertyInfo> Properties { get; set; } =new Dictionary<int, PropertyInfo>();
        public List<List<object>> Rows { get; set; } = new List<List<object>>();
    }

    public class ExceptionData
    {
        public int RowNumber { get; set; }
        public Exception Exception { get; set; }
    }
    public class ImportResult
    {
        public TimeSpan TotalImportTime { get; set; }
        public int RowsInserted { get; set; }
        public int RowsUpdated { get; set; }
        List<ExceptionData> ExceptionData { get; set; } = new List<ExceptionData>();
        List<ExceptionData> ValidationErrors { get; set; } = new List<ExceptionData>();
        public ImportResult()
        {

        }
    }
    public enum ImportMode
    {
        InsertUpdate,
        Insert,
        Update
    }
    
    public class ImportService
    {
        List<XPCollection> objectsToUpdate = null;
        public ImportResult Import(IObjectSpace objectSpace, RowDef rowDef, ImportMode importMode)
        {
            ImportResult importResult = new ImportResult();
            try
            {
                DateTime startTime = DateTime.Now;
                int ImportedObjects = 0;
                int CurrentPropertyIndex = 0;
                int CurrentRowNumber = 0;
                BinaryOperator CurrentOperator = null;

                List<KeyValuePair<int, PropertyInfo>> RefProperties = rowDef.Properties.Where(p => p.Value.PropertyKind == PropertyKind.Reference).ToList();
                var XpOs = objectSpace as XPObjectSpace;
                ITypesInfo TypesInfo = XpOs.TypesInfo;
                var PageSize = 1000;
                var Pages = GetPageSize(rowDef, PageSize);
               

                Dictionary<PropertyInfo, List<XPCollection>> Collections = new Dictionary<PropertyInfo, List<XPCollection>>();
                List<XPCollection> AllCollections = new List<XPCollection>();

                var Key=rowDef.Properties.FirstOrDefault(p=> p.Value.IsBusinessKey);

                KeyValuePair<int, PropertyInfo> KeyProperty = new KeyValuePair<int, PropertyInfo>(Key.Key, new PropertyInfo() { PropertyName = Key.Value.PropertyName, PropertyType = rowDef.ObjectType, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = Key.Value.PropertyName });
                objectsToUpdate = BuildCollection(KeyProperty, rowDef, XpOs.Session, TypesInfo, PageSize, Pages);

                foreach (KeyValuePair<int, PropertyInfo> RefProp in RefProperties)
                {
                    List<XPCollection> value = BuildCollection(RefProp, rowDef, XpOs.Session, TypesInfo, PageSize, Pages);
                    foreach (XPCollection xPCollection in value)
                    {
                        AllCollections.Add(xPCollection);
                    }
                    Collections.Add(RefProp.Value, value);
                  
                }
                if(objectsToUpdate!=null)
                {
                    AllCollections.AddRange(objectsToUpdate);
                }

                XpOs.Session.BulkLoad(AllCollections.ToArray());

                Dictionary<int,object> IndexObject=new Dictionary<int, object>();
                foreach (List<object> Row in rowDef.Rows)
                {
                    var KeyValue = Row[Key.Key];
                    var Instance = GetObjectInstance(objectSpace, rowDef.ObjectType, TypesInfo,importMode,KeyValue, KeyProperty.Value.PropertyName) as XPCustomObject;

                    IndexObject.Add(CurrentRowNumber, Instance);
                    for (int i = 0; i < Row.Count; i++)
                    {

#if DEBUG

                        Debug.WriteLine($"Current Index:{i} Current Property{rowDef.Properties[i].PropertyName}");
#endif


                        if (rowDef.Properties[i].PropertyKind == PropertyKind.Primitive)
                            Instance.SetMemberValue(rowDef.Properties[i].PropertyName, Row[i]);
                        else
                        {
                            CurrentOperator = new BinaryOperator(rowDef.Properties[i].ReferencePropertyLookup, Row[i]);
                            PropertyInfo propertyInfo = rowDef.Properties[i];

                            List<XPCollection> xPCollections = Collections[propertyInfo];
                            object CurrentValue = GetValueFromCollection(CurrentOperator, xPCollections);
                            Instance.SetMemberValue(rowDef.Properties[i].PropertyName, CurrentValue);

                        }
                    }

                    CurrentRowNumber++;
                    ImportedObjects++;
                }

                var Result = Validator.RuleSet.ValidateAllTargets(objectSpace, objectSpace.ModifiedObjects, "Save");
                if (Result.ValidationOutcome == ValidationOutcome.Valid)
                {

                    objectSpace.CommitChanges();
                }
                else
                {
                    IEnumerable<RuleSetValidationResultItem> Errors = Result.Results.Where(r => r.ValidationOutcome == ValidationOutcome.Error);
                    var Mo = objectSpace.ModifiedObjects.Count;
                    foreach (object item in Errors.Select(e => e.Target))
                    {
                        var Invalid= IndexObject.FirstOrDefault(Io => Io.Value == item);
                    }
                    var Mo2 = objectSpace.ModifiedObjects.Count;
                    objectSpace.CommitChanges();
                }

                DateTime EndTime = DateTime.Now;
                importResult.TotalImportTime = EndTime - startTime;
            }
            catch (Exception ex)
            {

                throw;
            }
           
            return importResult;

        }

        protected virtual object GetObjectInstance(IObjectSpace objectSpace, string ObjectType, ITypesInfo TypesInfo, ImportMode importMode,object keyValue,string keyPropertyName)
        {
            var CurrentOperator = new BinaryOperator(keyPropertyName, keyValue);

            if (importMode== ImportMode.Insert)
            {
                return objectSpace.CreateObject(TypesInfo.FindTypeInfo(ObjectType).Type);
            }
            if (importMode == ImportMode.InsertUpdate)
            {
                if(this.objectsToUpdate!=null)
                {
                  
                   var returnValue= GetValueFromCollection(CurrentOperator, this.objectsToUpdate);
                    if (returnValue != null)
                        return returnValue;
                    else 
                    {
                        return objectSpace.CreateObject(TypesInfo.FindTypeInfo(ObjectType).Type);
                    }
                }
                else
                {
                    return objectSpace.CreateObject(TypesInfo.FindTypeInfo(ObjectType).Type);
                }
               
            }
            if (importMode == ImportMode.Update)
            {
                return GetValueFromCollection(CurrentOperator, this.objectsToUpdate);
            }
            return null;

        }

        private object GetValueFromCollection(BinaryOperator CurrentOperator, List<XPCollection> xPCollections)
        {
           
            foreach (XPCollection xPCollection in xPCollections)
            {
                xPCollection.Filter = CurrentOperator;
                //Debug.WriteLine(CurrentOperator.ToString());

                if (xPCollection.Count > 0)
                {
                    //Debug.WriteLine("Value:" + xPCollection[0]);
                    return xPCollection[0];

                }
               
            }
            return null;

           
        }

        List<XPCollection> BuildCollection(KeyValuePair<int, PropertyInfo> RefProperties, RowDef rowDef, Session session, ITypesInfo TypesInfo, int pageSize, int pages)
        {
            List<XPCollection> collections = new List<XPCollection>();
            var CriteriaOperators = BuildCriteriaPages(RefProperties, rowDef, pageSize, pages);
            foreach (var Criteria in CriteriaOperators)
            {

                Type type = TypesInfo.FindTypeInfo(RefProperties.Value.PropertyType).Type;
                Debug.WriteLine($"Collection type:{type.FullName} Criteria:{Criteria}");
                var Collection = new XPCollection(session, type, false);
                Collection.TopReturnedObjects = 0;
                Collection.Criteria = Criteria;
                //return Collection;
                var tp = TypesInfo.FindTypeInfo(RefProperties.Value.PropertyType).Type;
                var CurrentCollection = new XPCollection(PersistentCriteriaEvaluationBehavior.BeforeTransaction, session, tp, Criteria);
                //var CurrentCollection = new XPCollection(session, tp,false);
                //CurrentCollection.Criteria = Criteria;
                collections.Add(CurrentCollection);
            }
            return collections;
        
        }
        CriteriaOperator BuildCriteria(KeyValuePair<int, PropertyInfo> RefProperties, RowDef rowDef, int pageSize, int pages)
        {
            List<CriteriaOperator> operators = BuildCriteriaPages(RefProperties, rowDef, pageSize, pages);
            return CriteriaOperator.Or(operators);

        }

        private List<CriteriaOperator> BuildCriteriaPages(KeyValuePair<int, PropertyInfo> RefProperties, RowDef rowDef, int pageSize, int pages)
        {
            List<CriteriaOperator> operators = new List<CriteriaOperator>();
            for (int i = 0; i < pages; i++)
            {
                List<object> values = GetValues(rowDef, i + 1, pageSize, RefProperties.Key);
                operators.Add(new InOperator(RefProperties.Value.ReferencePropertyLookup, values));
            }

            return operators;
        }

        int GetPageSize(RowDef rowDef, int pageSize)
        {
            return (int)Math.Ceiling((double)rowDef.Rows.Count() / pageSize);
        }
        List<Object> GetValues(RowDef rowDef, int page, int pageSize, int ColumIndex)
        {
            Debug.WriteLine($"Page: {page} ColumIndex: {ColumIndex}");
            return rowDef.Rows
                .Select(innerList => innerList[ColumIndex])
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
    }
}

