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
using static DevExpress.Drawing.Printing.Internal.DXPageSizeInfo;

namespace XafImportApi.Module.Import
{
    public enum PropertyKind
    {
        Primitive, Reference, Enum,

    }
    public class PropertyInfo
    {
        public string Name { get; set; }
        public string PropertyType { get; set; }
        public PropertyKind PropertyKind { get; set; }
        public string ReferecePropertyLookup { get; set; }
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
        public ImportResult()
        {

        }
    }
    public class ImportService
    {
        public ImportResult Import(IObjectSpace objectSpace, RowDef rowDef)
        {
            ImportResult importResult = new ImportResult();
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
            //Dictionary<PropertyInfo, XPCollection> Collections = new Dictionary<PropertyInfo, XPCollection>();
            Dictionary<PropertyInfo, List<XPCollection>> Collections = new Dictionary<PropertyInfo, List<XPCollection>>();
            List<XPCollection> AllCollections = new List<XPCollection>();
            foreach (KeyValuePair<int, PropertyInfo> RefProp in RefProperties)
            {
                List<XPCollection> value = BuildCollection(RefProp, rowDef, XpOs.Session, TypesInfo, PageSize, Pages);
                foreach (XPCollection xPCollection in value)
                {
                    AllCollections.Add(xPCollection);
                }
                Collections.Add(RefProp.Value, value);
                //Collections.Add(RefProp.Value, BuildCollection(RefProp, rowDef, XpOs.Session, TypesInfo, PageSize, Pages));
            }
            //XpOs.Session.BulkLoad(Collections.Select(c => c.Value).ToArray());
            XpOs.Session.BulkLoad(AllCollections.ToArray());
    
            foreach (List<object> Row in rowDef.Rows)
            {
                var Instance=objectSpace.CreateObject(TypesInfo.FindTypeInfo(rowDef.ObjectType).Type) as XPCustomObject;
                for (int i = 0; i < Row.Count; i++)
                {
                    if(rowDef.Properties[i].PropertyKind==PropertyKind.Primitive)
                        Instance.SetMemberValue(rowDef.Properties[i].Name, Row[i]);
                    else
                    {
                        CurrentOperator = new BinaryOperator(rowDef.Properties[i].ReferecePropertyLookup, Row[i]);
                        PropertyInfo propertyInfo = rowDef.Properties[i];

                        List<XPCollection> xPCollections = Collections[propertyInfo];
                        object CurrentValue = GetValueFromCollection(CurrentOperator, xPCollections);
                        Instance.SetMemberValue(rowDef.Properties[i].Name, CurrentValue);
                       
                    }
                }
                ImportedObjects++;
            }
            DateTime EndTime = DateTime.Now;
            importResult.TotalImportTime = EndTime - startTime;
            var Result=  Validator.RuleSet.ValidateAllTargets(objectSpace, objectSpace.ModifiedObjects,"Save");
            if(Result.ValidationOutcome== ValidationOutcome.Valid)
            {
                objectSpace.CommitChanges();
            }
         
            return importResult;

        }

        private static object GetValueFromCollection(BinaryOperator CurrentOperator, List<XPCollection> xPCollections)
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
                operators.Add(new InOperator(RefProperties.Value.ReferecePropertyLookup, values));
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

