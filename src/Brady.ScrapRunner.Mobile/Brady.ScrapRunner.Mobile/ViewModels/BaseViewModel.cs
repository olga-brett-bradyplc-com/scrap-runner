using MvvmCross.Localization;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using FluentValidation;
    using FluentValidation.Results;
    using MvvmCross.Core.ViewModels;

    public class BaseViewModel : MvxViewModel
    {
        public BaseViewModel()
        {
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _subTitle;
        public string SubTitle
        {
            get { return _subTitle; }
            set { SetProperty(ref _subTitle, value); }
        }

        protected ValidationResult Validate<TValidator, TType>(TType type) where TValidator : AbstractValidator<TType>, new()
        {
            var validator = new TValidator();
            return validator.Validate(type);
        }
    }
}