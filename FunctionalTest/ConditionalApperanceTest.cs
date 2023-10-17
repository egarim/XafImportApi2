using DevExpress.Data.Linq.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
using XafFunctionalTest.Module.BusinessObjects;
using XafFunctionalTest.Module.Controllers;

namespace FunctionalTest
{
    public class ConditionalAppearanceTest
    {
        private Customer Customer;
        private TestAppearanceTarget target;
        private AppearanceController controller;
        private DetailView detailView;
        private IObjectSpace objectSpace;
        TestApplication application;
        [SetUp]
        public virtual void SetUp()
        {
            XPObjectSpaceProvider objectSpaceProvider = new XPObjectSpaceProvider(new MemoryDataStoreProvider());

            application = new TestApplication();
            ModuleBase testModule = new ModuleBase();
            testModule.AdditionalExportedTypes.Add(typeof(Customer));
            application.Modules.Add(testModule);
            application.Modules.Add(new ConditionalAppearanceModule());
            application.Setup("TestApplication", objectSpaceProvider);
            IObjectSpace objectSpace = objectSpaceProvider.CreateObjectSpace();
            Customer = objectSpace.CreateObject<Customer>();
            target = new TestAppearanceTarget();
            controller = new AppearanceController();
            detailView = application.CreateDetailView(objectSpace, Customer);
            controller.SetView(detailView);



        }

        [Test]
        public void TestBusinessObject()
        {

            //Customer.Active = true;
            //controller.RefreshItemAppearance(detailView, "ViewItem", "MaxCredit", target, Customer);
            //Assert.IsTrue(target.Enabled);
            //Customer.Active = false;
            //controller.RefreshItemAppearance(detailView, "ViewItem", "MaxCredit", target, Customer);
            //Assert.IsFalse(target.Enabled);
            //Assert.AreEqual(Color.Red ,target.BackColor);





        }
        [Test]
        public void TestPopup()
        {

            ////this.application.MainWindow.SetView(this.detailView);
            //var CustomerController = this.application.CreateController<CustomerController>();
            //List<Controller> controllers = new List<Controller>();
            //controllers.Add(CustomerController);
            //this.application.CreateWindow(TemplateContext.ApplicationWindow,controllers,false,true,this.detailView);
            //CustomerController.SetView(this.detailView);
            //var paramsw= CustomerController.ShowPopup1.GetPopupWindowParams();


            //CustomerController.ShowPopup1.DoExecute(this.application.MainWindow);

            //var test=CustomerController.Frame.Template;



        }


    }
}