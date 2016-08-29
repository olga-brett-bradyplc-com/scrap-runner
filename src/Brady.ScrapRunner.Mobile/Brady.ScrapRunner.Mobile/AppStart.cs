using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brady.ScrapRunner.Mobile.ViewModels;
using MvvmCross.Core.ViewModels;

namespace Brady.ScrapRunner.Mobile
{
    public class AppStart : MvxNavigatingObject, IMvxAppStart
    {
        public AppStart() { }

        public void Start(object hint = null)
        {
            ShowViewModel<SignInViewModel>();
        }
    }
}
