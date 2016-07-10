// Helpers/Settings.cs

using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Brady.ScrapRunner.Mobile.Helpers
{
  /// <summary>
  /// This is the Settings static class that can be used in your Core solution or in any
  /// of your client applications. All settings are laid out the same exact way with getters
  /// and setters. 
  /// </summary>
  public static class PhoneSettings
  {
        private static ISettings AppSettings
        {
            get
            {
                return CrossSettings.Current;
            }
        }

        #region Setting Constants

        private const string RemoteServerUrlKey = "remote_server_url_key";
        private static readonly string RemoteServerUrlDefault = string.Empty;

        private const string PreferredLanguageKey = "preferred_language_key";
        private static readonly string PreferredLanguageDefault = string.Empty;

        private const string LastContainerUpdateKey = "last_container_update_key";
        private static readonly DateTime? LastContainerUpdateDefault = (DateTime?) null;

        #endregion

        public static string ServerSettings
        {
            get { return AppSettings.GetValueOrDefault<string>(RemoteServerUrlKey, RemoteServerUrlDefault); }
            set { AppSettings.AddOrUpdateValue<string>(RemoteServerUrlKey, value); }
        }
        public static string LanguageSettings
        {
            get { return AppSettings.GetValueOrDefault<string>(PreferredLanguageKey, PreferredLanguageDefault); }
            set { AppSettings.AddOrUpdateValue<string>(PreferredLanguageKey, value); }
        }

        public static DateTime? ContainerSettings
        {
            get { return AppSettings.GetValueOrDefault<DateTime?>(LastContainerUpdateKey, LastContainerUpdateDefault); }
            set
            {
                if( value == null )
                    AppSettings.Remove(LastContainerUpdateKey);
                else
                    AppSettings.AddOrUpdateValue<DateTime?>(LastContainerUpdateKey, value);
            }
        }
    }
}