using DevExpress.Data.Linq.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;

using NUnit.Framework;
using XafFunctionalTest.Module.BusinessObjects;

namespace FunctionalTest
{
    public class ValidationsTests
    {
        private IObjectSpace objectSpace;
        [SetUp]
        public virtual void SetUp()
        {
            XPObjectSpaceProvider objectSpaceProvider =
            new XPObjectSpaceProvider(new MemoryDataStoreProvider());
            TestApplication application = new TestApplication();
            ModuleBase testModule = new ModuleBase();
            testModule.AdditionalExportedTypes.Add(typeof(Customer));
            application.Modules.Add(testModule);
           
            application.Setup("TestApplication", objectSpaceProvider);
             objectSpace = objectSpaceProvider.CreateObjectSpace();
        }

       
       
        [Test]
        public void TestValidationRule()
        {
            Customer Customer = objectSpace.CreateObject<Customer>();
            RuleSet ruleSet = new RuleSet();
            RuleSetValidationResult result;

            result = ruleSet.ValidateTarget(objectSpace, Customer, DefaultContexts.Save);

           

            Assert.AreEqual(ValidationState.Invalid,
                result.GetResultItem(CustomerValidationRulesId.CustomerNameIsRequired).State);

            Customer.Name = "Mary Tellitson";
            result = ruleSet.ValidateTarget(objectSpace, Customer, DefaultContexts.Save);
            Assert.AreEqual(ValidationState.Valid,
                result.GetResultItem(CustomerValidationRulesId.CustomerNameIsRequired).State);

          
        }
    }
}