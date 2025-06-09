using DPMS_WebAPI.ViewModels;
using System.Linq.Expressions;

public interface IRepository<T> where T : class
{
    Task<PagedResponse<T>> GetPagedAsync(QueryParams queryParams , params Expression<Func<T, object>>[]? includes);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(object id);
    Task<T> AddAsync(T entity);
    void Update(T entity);
    Task DeleteAsync(object id);
    void Delete(T entity);
    Task BulkAddAsync(IEnumerable<T> entities);
    Task BulkDeleteAsync(IEnumerable<T> entities);
    Task<T?> GetDetailAsync(object id, params Expression<Func<T, object>>[] includes);

}
