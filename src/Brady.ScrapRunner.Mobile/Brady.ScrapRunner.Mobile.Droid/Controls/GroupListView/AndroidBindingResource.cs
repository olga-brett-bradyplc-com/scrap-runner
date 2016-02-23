using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCross.Platform;
using MvvmCross.Platform.Droid;

namespace Brady.ScrapRunner.Mobile.Droid.Controls.GroupListView
{
    /// <summary>
    /// Taken from https://github.com/deapsquatter/MvvmCross.DeapExtensions
    /// </summary>
    public class AndroidBindingResource
    {
        public static readonly AndroidBindingResource Instance = new AndroidBindingResource();

        private AndroidBindingResource()
        {
            var setup = Mvx.Resolve<IMvxAndroidGlobals>();
            var resourceTypeName = setup.ExecutableNamespace + ".Resource";
            Type resourceType = setup.ExecutableAssembly.GetType(resourceTypeName);
            if (resourceType == null)
                throw new Exception("Unable to find resource type - " + resourceTypeName);
            try
            {
                BindableListGroupItemTemplateId =
                    (int)
                        resourceType.GetNestedType("Styleable")
                        .GetField("MvxListView_GroupItemTemplate")
                        .GetValue(null);
            }
            catch (Exception)
            {
                throw new Exception(
                    "Error finding resource ids for MvvmCross.DeapBinding - please make sure ResourcesToCopy are linked into the executable");
            }
        }

        public int BindableListGroupItemTemplateId { get; private set; }
    }
}