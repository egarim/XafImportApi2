using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XafImportApi.Module;
using XafImportApi.Module.BusinessObjects;

namespace FunctionalTest
{

    public class TestImportService : TestModuleBase
    {
        protected override ModuleBase GetModule()
        {
            return new XafImportApiModule();
        }
        [SetUp]
        public override void SetUp()
        {
            base.SetUp();
        }
        [Test]
        public void ImportAccounts()
        {
            var Start = DateTime.Now;
            Login("Admin", "");
            var ImportController=application.CreateController<XafImportApi.Module.Controllers.ImportController>();
          

            var Os = this.secureObjectSpaceProvider.CreateObjectSpace();//this.application.CreateObjectSpace(typeof(ImportJob));
            var ImportAccounts=Os.CreateObject<MainObject>();
            //ImportAccounts.ImportFileType = ImportFileType.Account;
            //ImportAccounts.Data = Os.CreateObject<FileData>();
            //ImportAccounts.Data.LoadFromStream("Accounts.csv", System.IO.File.OpenRead("TestData/CuentasContables.csv"));
            var detailView = application.CreateDetailView(Os, ImportAccounts);
            ImportController.SetView(detailView);
            ImportController.ImportObjects.DoExecute(1);
            ImportController.ImportObjects.DoExecute(1);
            //ImportController.Import.DoExecute();
            //Debug.WriteLine((DateTime.Now - Start).TotalSeconds);
            //Assert.IsTrue(Os.GetObjectsCount(typeof(Account), null) == 761);
        }
    }
}
