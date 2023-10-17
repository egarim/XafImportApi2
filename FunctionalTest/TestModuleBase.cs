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
using System.Collections.Generic;
using XafImportApi.Module.BusinessObjects;
using XafImportApi.Module.DatabaseUpdate;
using static System.Net.Mime.MediaTypeNames;

namespace FunctionalTest
{
    public abstract class TestModuleBase
    {


        protected SecurityStrategyComplex security;
        protected SecuredObjectSpaceProvider secureObjectSpaceProvider;
        protected TestApplication application;
        protected XPObjectSpaceProvider nonSecureObjectSpaceProvider;
        protected IXpoDataStoreProvider dataStoreProvider;

        //TODO create object space with the application

        protected abstract ModuleBase GetModule();

        [SetUp]
        public virtual void SetUp()
        {

            //HACK   https://docs.devexpress.com/eXpressAppFramework/112769/getting-started/in-depth-tutorial-winforms-webforms/security-system/access-the-security-system-in-code

            dataStoreProvider = GetDataStore();


            //Create an instance of the test application (this application is a core application and is not bound to any U.I platform)
            application = new TestApplication();

            nonSecureObjectSpaceProvider = new XPObjectSpaceProvider(dataStoreProvider);


            var ModuleToTest = GetModule();
            application.Modules.Add(ModuleToTest);

            List<IObjectSpaceProvider> objectSpaceProviders = new();
            objectSpaceProviders.Add(nonSecureObjectSpaceProvider);
            objectSpaceProviders.Add(secureObjectSpaceProvider);


            application.Setup("TestApplication", nonSecureObjectSpaceProvider);
            //application.Setup("TestApplication", objectSpaceProviders);

            //Create an object space provider that is not secure
            var UpdaterObjectSpace = nonSecureObjectSpaceProvider.CreateUpdatingObjectSpace(true);



            //Create a instance of the updater from the agnostic module
            Updater updater = new Updater(UpdaterObjectSpace, null);
            updater.UpdateDatabaseBeforeUpdateSchema();
            updater.UpdateDatabaseAfterUpdateSchema();




        }

        protected virtual IXpoDataStoreProvider GetDataStore()
        {
            return new MemoryDataStoreProvider();
        }

        protected SecurityStrategyComplex Login(string userName, string password)
        {
            var LoginObjectSpace = nonSecureObjectSpaceProvider.CreateObjectSpace();
            AuthenticationStandard authentication = new AuthenticationStandard();
            security = new SecurityStrategyComplex(typeof(ApplicationUser), typeof(PermissionPolicyRole), authentication);
            security.RegisterXPOAdapterProviders();

            authentication.SetLogonParameters(new AuthenticationStandardLogonParameters(userName, password));
            security.Logon(LoginObjectSpace);
            secureObjectSpaceProvider = new SecuredObjectSpaceProvider(security, dataStoreProvider);

            application.SetObjectSpaceProvider(secureObjectSpaceProvider);
            application.Security = security;
            return security;
        }

       

    }
}