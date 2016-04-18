namespace Brady.ScrapRunner.Mobile.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public void ShowMenu()
        {
            ShowViewModel<SignInViewModel>();
            ShowViewModel<MenuViewModel>();
        }

        public void Init(object hint)
        {
            // TODO
        }

        public override void Start()
        {
            // TODO   
        }
    }
}
