using DevExpress.Data.Linq.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using NUnit.Framework;
using System;
using XafFunctionalTest.Module;
using XafFunctionalTest.Module.BusinessObjects;
using XafFunctionalTest.Module.Controllers;
using XafFunctionalTest.Module.DatabaseUpdate;

namespace FunctionalTest
{
    public class XafSecurityTest
    {

   
        SecurityStrategyComplex security;
        SecuredObjectSpaceProvider secureObjectSpaceProvider;
        TestApplication application;
        XPObjectSpaceProvider nonSecureObjectSpaceProvider;
        MemoryDataStoreProvider dataStoreProvider;




        [SetUp]
        public virtual void SetUp()
        {

            //HACK   https://docs.devexpress.com/eXpressAppFramework/112769/getting-started/in-depth-tutorial-winforms-webforms/security-system/access-the-security-system-in-code

            dataStoreProvider = new MemoryDataStoreProvider();


            //Create an instance of the test application (this application is a core application and is not bound to any U.I platform)
            application = new TestApplication();

            nonSecureObjectSpaceProvider = new XPObjectSpaceProvider(dataStoreProvider);


            XafFunctionalTestModule xafFunctionalTestModule = new XafFunctionalTestModule();
            application.Modules.Add(xafFunctionalTestModule);

            application.Setup("TestApplication", nonSecureObjectSpaceProvider);


            //Create an object space provider that is not secure
            var UpdaterObjectSpace = nonSecureObjectSpaceProvider.CreateUpdatingObjectSpace(true);



            //Create a instance of the updater from the agnostic module
            Updater updater = new Updater(UpdaterObjectSpace, null);
            updater.UpdateDatabaseBeforeUpdateSchema();
            updater.UpdateDatabaseAfterUpdateSchema();




        }

        private SecurityStrategyComplex Login(string userName, string password)
        {
            var LoginObjectSpace = nonSecureObjectSpaceProvider.CreateObjectSpace();
            AuthenticationStandard authentication = new AuthenticationStandard();
            this.security = new SecurityStrategyComplex(typeof(ApplicationUser), typeof(PermissionPolicyRole), authentication);
            security.RegisterXPOAdapterProviders();

            authentication.SetLogonParameters(new AuthenticationStandardLogonParameters(userName, password));
            security.Logon(LoginObjectSpace);
            secureObjectSpaceProvider = new SecuredObjectSpaceProvider(this.security, dataStoreProvider);

         
            application.Security = security;
            return security;
        }

        [Test]
        public void UserShouldNotBeAbleToCreateCustomer()
        {

            SecurityStrategyComplex security = Login("User", "");


          

            IObjectSpace objectSpace = secureObjectSpaceProvider.CreateObjectSpace();
            SecurityStrategy SecurityFromApp = application.GetSecurityStrategy();


            //HACK assert exception 
            var Exception = Assert.Throws<UserFriendlyObjectLayerSecurityException>
             (
               () =>
               {
                   var Customer = objectSpace.CreateObject<Customer>();
                   objectSpace.CommitChanges();

               }

             );

            //HACK assert exception message
            Assert.AreEqual(Exception.Message, "Saving the 'XafFunctionalTest.Module.BusinessObjects.Customer' object is prohibited by security rules.");


          
        }
        [Test]
        public void CheckIsCreateOperationIsAllow()
        {

            Login("User", "");
            SecurityStrategy SecurityFromApp = application.GetSecurityStrategy();

            Assert.AreEqual(false,SecurityFromApp.CanCreate<Customer>());
            Assert.AreEqual(false, SecurityFromApp.CanWrite<Customer>());
            Assert.AreEqual(false, SecurityFromApp.CanDelete<Customer>());
            //HACK navigation permissions should be explicitly denied otherwise it will return true
            Assert.AreEqual(false, SecurityFromApp.CanNavigate("Application/NavigationItems/Items/Default/Items/Customer_ListView"));


        }
        [Test]
        public void CheckIsCreateOperationIsAllowForAdmin()
        {

            Login("Admin", "");
            SecurityStrategy SecurityFromApp = application.GetSecurityStrategy();

            Assert.AreEqual(false, SecurityFromApp.CanCreate<Customer>());
            Assert.AreEqual(false, SecurityFromApp.CanWrite<Customer>());
            Assert.AreEqual(false, SecurityFromApp.CanDelete<Customer>());
            //HACK navigation permissions should be explicitly denied otherwise it will return true
            Assert.AreEqual(false, SecurityFromApp.CanNavigate("Application/NavigationItems/Items/Default/Items/Customer_ListView"));


        }
        [Test]
        public void AdminShouldBeAbleToCreateCustomer()
        {

            Login("Admin", "");

            IObjectSpace objectSpace = secureObjectSpaceProvider.CreateObjectSpace();

            var Customer = objectSpace.CreateObject<Customer>();
            objectSpace.CommitChanges();

            var CustomerFromDatabase=objectSpace.FindObject<Customer>(null);

            Assert.NotNull(CustomerFromDatabase);
        }



        [Test]
        public void SecurityInActions()
        {
            //HACK enable security for actions
            //https://community.devexpress.com/blogs/xaf/archive/2020/05/04/xaf-permissions-for-ui-actions-and-security-system-for-non-xaf-apps-powered-by-entity-framework-core-3-v20-1.aspx
            //https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.EnableSecurityForActions

            //HACK security strategy https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy._members
           
            SecurityStrategyComplex security = Login("User", "");
            SecurityStrategy.EnableSecurityForActions = true;

            //the action CustomerController.CustomerActionId was added to the secure actions in the method CreateDefaultRole of the updater class in the agnostic module

            bool actual = security.CanExecute(CustomerController.CustomerActionId);
            Assert.AreEqual(false, actual);



        }
        [Test]
        public void CheckIfAnActionIsEnableDependingOnTheObjectState()
        {
            //HACK https://supportcenter.devexpress.com/ticket/details/t853927/how-to-unit-test-action-s-enabled-disabled-state-based-on-target-criteria-and-selection
            
            //Login
            SecurityStrategyComplex security = Login("Admin", "");

            IObjectSpace objectSpace = secureObjectSpaceProvider.CreateObjectSpace();


            var Customer = objectSpace.CreateObject<Customer>();
            Customer.Active = false;
            
            //Controllers
            var CustomerController = new CustomerController();

            //HACK xaf list of controllers https://docs.devexpress.com/eXpressAppFramework/113016/ui-construction/controllers-and-actions/built-in-controllers-and-actions
            //HACK ActionsCriteriaViewController https://docs.devexpress.com/eXpressAppFramework/113141/ui-construction/controllers-and-actions/built-in-controllers-and-actions/built-in-controllers-and-actions-in-the-system-module#actionscriteriaviewcontroller
            ActionsCriteriaViewController actionsCriteriaViewController = new ActionsCriteriaViewController();

            DetailView DetailView = new DetailView(objectSpace, Customer, application, true);

            //HACK frames and windows https://docs.devexpress.com/eXpressAppFramework/112608/ui-construction/windows-and-frames
            //HACK frames https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Frame

            //Set view and controllers to the frame
            Frame frame = new Frame(this.application, TemplateContext.View, actionsCriteriaViewController, CustomerController);
            frame.SetView(DetailView);
            
            
            var CustomerAction = CustomerController.Actions[CustomerController.CustomerActionId] as SimpleAction;

            Assert.AreEqual(false, CustomerAction.Enabled.ResultValue);
        }

    }
}