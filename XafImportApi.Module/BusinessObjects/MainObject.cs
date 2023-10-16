using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace XafImportApi.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class MainObject : BaseObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public MainObject(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        RefObject5 refProp5;
        RefObject4 refProp4;
        RefObject3 refProp3;
        RefObject2 refProp2;
        RefObject1 refProp1;
        bool active;
        DateTime date;
        string name;

        [Size(SizeAttribute.DefaultStringMappingFieldSize)]
        public string Name
        {
            get => name;
            set => SetPropertyValue(nameof(Name), ref name, value);
        }

        public DateTime Date
        {
            get => date;
            set => SetPropertyValue(nameof(Date), ref date, value);
        }

        public bool Active
        {
            get => active;
            set => SetPropertyValue(nameof(Active), ref active, value);
        }

        public RefObject1 RefProp1
        {
            get => refProp1;
            set => SetPropertyValue(nameof(RefProp1), ref refProp1, value);
        }

        public RefObject2 RefProp2
        {
            get => refProp2;
            set => SetPropertyValue(nameof(RefProp2), ref refProp2, value);
        }

        public RefObject3 RefProp3
        {
            get => refProp3;
            set => SetPropertyValue(nameof(RefProp3), ref refProp3, value);
        }

        public RefObject4 RefProp4
        {
            get => refProp4;
            set => SetPropertyValue(nameof(RefProp4), ref refProp4, value);
        }
        
        public RefObject5 RefProp5

        {
            get => refProp5;
            set => SetPropertyValue(nameof(RefProp5), ref refProp5, value);
        }
    }
}