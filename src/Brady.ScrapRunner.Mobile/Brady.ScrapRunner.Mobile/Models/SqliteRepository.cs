using SQLite.Net;

namespace Brady.ScrapRunner.Mobile.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Interfaces;
    using MvvmCross.Plugins.Sqlite;
    using SQLite.Net.Async;

    public class SqliteRepository<T> : IRepository<T> where T : class, new()
    {
        private readonly SQLiteAsyncConnection _connection;
        private readonly SQLiteConnectionWithLock _connectionWithLock;

        public SqliteRepository(IMvxSqliteConnectionFactory sqliteConnectionFactory)
        {
            _connectionWithLock = sqliteConnectionFactory.GetConnectionWithLock(MobileConstants.Database);
            _connection = new SQLiteAsyncConnection(() => _connectionWithLock);
        }

        public Task<List<T>> AllAsync()
        {
            return _connection.Table<T>().ToListAsync();
        }

        public Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate)
        {
            return _connection.Table<T>()
                .Where(predicate)
                .ToListAsync();
        }

        public Task<List<T>> ToListAsync<TValue>(Expression<Func<T, bool>> predicate = null, 
            Expression<Func<T, TValue>> orderBy = null)
        {
            var query = _connection.Table<T>();
            if (predicate != null)
                query = query.Where(predicate);
            if (orderBy != null)
                query = query.OrderBy(orderBy);
            return query.ToListAsync();
        }

        public Task<T> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return _connection.FindAsync(predicate);
        }

        public AsyncTableQuery<T> AsQueryable()
        {
            return _connection.Table<T>();
        }

        public Task<int> InsertAsync(T entity)
        {
            return _connection.InsertAsync(entity);
        }

        public Task<int> InsertRangeAsync(IEnumerable<T> entities)
        {
            return _connection.InsertAllAsync(entities);
        }

        public Task<int> InsertOrReplaceAsync(T entity)
        {
            return _connection.InsertOrReplaceAsync(entity);
        }

        public Task<int> InsertOrReplaceRangeAsync(IEnumerable<T> entities)
        {
            return _connection.InsertOrReplaceAllAsync(entities);
        }

        public Task<int> UpdateAsync(T entity)
        {
            return _connection.UpdateAsync(entity);
        }

        public Task<int> UpdateRangeAsync(IEnumerable<T> entities)
        {
            return _connection.UpdateAllAsync(entities);
        }

        public Task<int> DeleteAsync(T entity)
        {
            return _connection.DeleteAsync(entity);
        }
    }
}