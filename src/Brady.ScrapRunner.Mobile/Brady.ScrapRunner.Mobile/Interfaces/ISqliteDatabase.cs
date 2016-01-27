namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using SQLite.Net.Async;

    public interface ISqliteDatabase
    {
        SQLiteAsyncConnection Connection { get; }
    }
}
