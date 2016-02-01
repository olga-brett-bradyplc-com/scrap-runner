namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System.Threading.Tasks;
    using SQLite.Net.Async;

    public interface ISqliteDatabase
    {
        SQLiteAsyncConnection Connection { get; }

        Task InitializeAsync();
    }
}
