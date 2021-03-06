﻿using MvvmCross.Localization;
using System.ComponentModel;
using System.Collections.Generic;
using Brady.ScrapRunner.Mobile.Enums;

namespace Brady.ScrapRunner.Mobile.ViewModels
{
    using FluentValidation;
    using FluentValidation.Results;
    using MvvmCross.Core.ViewModels;

    public class BaseViewModel : MvxViewModel
    {
        public BaseViewModel()
        {
            ShouldAlwaysRaiseInpcOnUserInterfaceThread(false);
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
        public IMvxLanguageBinder TextSource
        {
            get { return new MvxLanguageBinder("", GetType().Name); }
        }
    }
}