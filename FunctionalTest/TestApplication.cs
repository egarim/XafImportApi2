using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Layout;
using System;

namespace FunctionalTest
{
    public class TestApplication : XafApplication
    {
        IObjectSpaceProvider provider;
        public void SetObjectSpaceProvider(IObjectSpaceProvider objectSpaceProvider)
        {
            this.provider = objectSpaceProvider;
        }
        protected override LayoutManager CreateLayoutManagerCore(bool simple)
        {
            return null;

        }
        public TestApplication()
        {
            this.CustomCheckCompatibility += TestApplication_CustomCheckCompatibility;
        }

        private void TestApplication_CustomCheckCompatibility(object? sender, CustomCheckCompatibilityEventArgs e)
        {
            e.Handled = true;
        }

        public new IObjectSpace CreateObjectSpace(Type type)
        {
            return this.CreateObjectSpaceCore(type);
        }
        public new IObjectSpace CreateObjectSpace()
        {
            return this.CreateObjectSpaceCore(null);
        }
        protected override IObjectSpace CreateObjectSpaceCore(Type objectType)
        {
            return provider.CreateObjectSpace();
        }

    }
}
