namespace Brady.ScrapRunner.Mobile.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using SQLite.Net.Async;

    public interface IRepository<T> where T : class, new()
    {
        Task<List<T>> AllAsync();
        Task<List<T>> ToListAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> ToListAsync<TValue>(Expression<Func<T, bool>> predicate, Expression<Func<T, TValue>> orderBy);
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);
        AsyncTableQuery<T> AsQueryable();
        Task<int> InsertAsync(T entity);
        Task<int> InsertRangeAsync(IEnumerable<T> entities);
        Task<int> InsertOrReplaceAsync(T entity);
        Task<int> UpdateAsync(T entity);
        Task<int> DeleteAsync(T entity);
    }

}
