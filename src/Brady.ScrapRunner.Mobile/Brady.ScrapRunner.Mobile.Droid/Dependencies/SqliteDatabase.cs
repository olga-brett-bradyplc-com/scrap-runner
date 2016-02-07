using Brady.ScrapRunner.Mobile.Droid.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(SqliteDatabase))]
namespace Brady.ScrapRunner.Mobile.Droid.Dependencies
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Models;
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

        public async Task InitializeAsync()
        {
            // @TODO: Call CreateTableAsync<Model> here for each SQLite table.
            await Connection.CreateTableAsync<EmployeeMasterModel>();
            await Connection.CreateTableAsync<CustomerDirectionModel>();
            await Connection.CreateTableAsync<TripModel>();

            // Clear the data in these tables at startup:
            await Connection.DeleteAllAsync<EmployeeMasterModel>();
            await Connection.DeleteAllAsync<CustomerDirectionModel>();
            await Connection.DeleteAllAsync<TripModel>();
        }
    }
}