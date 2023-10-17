using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using DevExpress.XtraRichEdit.Export.OpenDocument;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using XafImportApi.Module.BusinessObjects;
using XafImportApi.Module.Import;

namespace XafImportApi.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class ImportController : ViewController
    {
        ParametrizedAction importObjects;
        SimpleAction deleteImportedData;
        private const int MaxValue = 100;
        SimpleAction ImportData;

        public SimpleAction DeleteImportedData { get => deleteImportedData; set => deleteImportedData = value; }
        public ParametrizedAction ImportObjects { get => importObjects; set => importObjects = value; }

        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public ImportController()
        {
            InitializeComponent();
          

            DeleteImportedData = new SimpleAction(this, "Delete objects", "View");
            DeleteImportedData.Execute += DeleteImportedData_Execute;


            ImportObjects = new ParametrizedAction(this, "Import Objects", "View", typeof(int));
            ImportObjects.Execute += ImportObjects_Execute;
            
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        private void ImportObjects_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            var parameterValue = (int)e.ParameterCurrentValue;
            ImportService importService = new ImportService();
            RowDef rowDef = CreateTestData(parameterValue);
            //var rowsCount = rowDef.Rows.Count;
            //File.WriteAllText("Test.Data.txt", System.Text.Json.JsonSerializer.Serialize<RowDef>(rowDef));
            //var rowDef = System.Text.Json.JsonSerializer.Deserialize<RowDef>(File.ReadAllText("Test.Data.txt"));

            var Row=CreateRow(10);
            Row[0]=null;
            rowDef.Rows.Add(Row);



            var Result= importService.Import(this.Application.CreateObjectSpace(typeof(MainObject)), rowDef, ImportMode.InsertUpdate);
            Debug.WriteLine($"Import executed in :{Result.TotalImportTime.TotalSeconds}");
        }
        private void DeleteImportedData_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            this.View.ObjectSpace.Delete(this.View.ObjectSpace.GetObjects<MainObject>());
            this.View.ObjectSpace.CommitChanges();
        }
        private void ImportData_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
          
        }

        private static RowDef CreateTestData(int objects)
        {
            RowDef rowDef = new RowDef();
            rowDef.ObjectType = typeof(MainObject).FullName;
            rowDef.Properties.Add(0, new PropertyInfo() { PropertyName = "Name", PropertyType = typeof(string).FullName, PropertyKind = PropertyKind.Primitive });
            rowDef.Properties.Add(1, new PropertyInfo() { PropertyName = "Date", PropertyType = typeof(DateTime).FullName, PropertyKind = PropertyKind.Primitive });
            rowDef.Properties.Add(2, new PropertyInfo() { PropertyName = "Active", PropertyType = typeof(bool).FullName, PropertyKind = PropertyKind.Primitive });
            rowDef.Properties.Add(3, new PropertyInfo() { PropertyName = "RefProp1", PropertyType = typeof(RefObject1).FullName, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = "Code" });
            rowDef.Properties.Add(4, new PropertyInfo() { PropertyName = "RefProp2", PropertyType = typeof(RefObject2).FullName, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = "Code" });
            rowDef.Properties.Add(5, new PropertyInfo() { PropertyName = "RefProp3", PropertyType = typeof(RefObject3).FullName, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = "Code" });
            rowDef.Properties.Add(6, new PropertyInfo() { PropertyName = "RefProp4", PropertyType = typeof(RefObject4).FullName, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = "Code" });
            rowDef.Properties.Add(7, new PropertyInfo() { PropertyName = "RefProp5", PropertyType = typeof(RefObject5).FullName, PropertyKind = PropertyKind.Reference, ReferencePropertyLookup = "Code" });
            rowDef.Properties.Add(8, new PropertyInfo() { PropertyName = "Code", PropertyType = typeof(string).FullName, PropertyKind = PropertyKind.Primitive,IsBusinessKey=true });
            Random rnd = new Random();
            for (int i = 1; i <= objects; i++)
            {
                List<object> row = CreateRow(i);
                rowDef.Rows.Add(row);
            }

            return rowDef;
        }

        private static List<object> CreateRow(int i)
        {
            List<object> row = new List<object>();
            for (int j = 0; j < 9; j++)
            {

                if (j == 0)
                    row.Add("Name" + i);
                if (j == 1)
                    row.Add(DateTime.Now);
                if (j == 2)
                    row.Add(true);
                if (j == 3)
                    row.Add(i.ToString());
                if (j == 4)
                    row.Add(i.ToString());
                if (j == 5)
                    row.Add(i.ToString());
                if (j == 6)
                    row.Add(i.ToString());
                if (j == 7)
                    row.Add(i.ToString());
                if (j == 8)
                    row.Add(i.ToString());



            }

            return row;
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
    }
}
