using Brady.ScrapRunner.Mobile.Droid.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(SqliteDatabase))]
namespace Brady.ScrapRunner.Mobile.Droid.Dependencies
{
    using System;
    using Interfaces;
    using SQLite.Net;
    using SQLite.Net.Async;
    using SQLite.Net.Platform.XamarinAndroid;

    public class SqliteDatabase : ISqliteDatabase
    {
        public SqliteDatabase()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var path = System.IO.Path.Combine(folder, "ScrapRunner.db");
            var connectionFactory = new Func<SQLiteConnectionWithLock>(() => 
                new SQLiteConnectionWithLock(new SQLitePlatformAndroid(), 
                new SQLiteConnectionString(path, storeDateTimeAsTicks: false)));
            Connection = new SQLiteAsyncConnection(connectionFactory);
        }

        public SQLiteAsyncConnection Connection { get; }
    }
}