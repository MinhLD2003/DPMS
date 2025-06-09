using DPMS_WebAPI.ViewModels;
using System.Linq.Expressions;

namespace DPMS_WebAPI.Interfaces.Services
{
    public interface IService<T> where T : class
    {
        Task<PagedResponse<T>> GetPagedAsync(QueryParams queryParams , params Expression<Func<T, object>>[]? includes);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T?> GetByIdAsync(object id);
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<bool> DeleteAsync(object id);
        Task<bool>BulkAddAsync(IEnumerable<T> entities);
        Task<bool> BulkDeleteAsync(IEnumerable<T> entities);
        Task<T?> GetDetailAsync(object id, params Expression<Func<T, object>>[] includes);
    }
}
