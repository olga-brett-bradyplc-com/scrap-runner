using System;

namespace Brady.ScrapRunner.Mobile.Interfaces
{
    public interface IClientSettings
    {
        Uri ServiceBaseUri { get; set; }

        string UserName { get; set; }

        string Password { get; set; }
    }
}