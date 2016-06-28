using System;
using Brady.ScrapRunner.Mobile.Interfaces;

namespace Brady.ScrapRunner.Mobile.Services
{
    /// This is a temporary concrete implemention to store varying Uris, developer by developer.
    /// It is not even clear wheter we need IClientSettings in the long term.
    public class DemoClientSettings : IClientSettings
    {

        private Uri _serviceBaseUri = new Uri("https://DESKTOP-3AD7F8J:7776");
        private string _username = "admin";
        private string _password = "mem_2014";

        public Uri ServiceBaseUri 
        {
            get { return _serviceBaseUri; }
            set { _serviceBaseUri = value; }
        }

        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }
    }
}