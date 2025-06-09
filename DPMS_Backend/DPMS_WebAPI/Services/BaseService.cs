using DPMS_WebAPI.Interfaces.Services;
using DPMS_WebAPI.Interfaces;
using DPMS_WebAPI.ViewModels;
using System.Linq.Expressions;


public abstract class BaseService<T> : IService<T> where T : class
{
    protected readonly IUnitOfWork _unitOfWork;
    protected abstract IRepository<T> Repository { get; }
    protected BaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await Repository.GetAllAsync();
    }

    public virtual async Task<PagedResponse<T>> GetPagedAsync(QueryParams queryParams , params Expression<Func<T, object>>[]? includes  )
    {
        return await Repository.GetPagedAsync(queryParams , includes);
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await Repository.FindAsync(predicate);
    }

    public virtual async Task<T?> GetByIdAsync(object id)
    {
        return await Repository.GetByIdAsync(id);
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        try
        {
            var result = await Repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error adding entity {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        try
        {
            Repository.Update(entity);
            await _unitOfWork.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error updating entity {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public virtual async Task<bool> DeleteAsync(object id)
    {
        try
        {
            var entity = await Repository.GetByIdAsync(id);
            if (entity == null) return false;

            await Repository.DeleteAsync(id);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error deleting entity {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public virtual async Task<bool> BulkAddAsync(IEnumerable<T> entities)
    {
        try
        {
            await Repository.BulkAddAsync(entities);

            return await _unitOfWork.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error bulk adding entities {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public virtual async Task<bool> BulkDeleteAsync(IEnumerable<T> entities)
    {
        try
        {
            await Repository.BulkDeleteAsync(entities);
            return await _unitOfWork.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error bulk deleting entities {typeof(T).Name}: {ex.Message}", ex);
        }
    }

    public async Task<T?> GetDetailAsync(object id, params Expression<Func<T, object>>[] includes)
    {
       try
        {
            var result = await Repository.GetDetailAsync(id, includes);
            return result;
        }
        catch (Exception ex)    
        {
            throw new Exception();
        }
    }
}
