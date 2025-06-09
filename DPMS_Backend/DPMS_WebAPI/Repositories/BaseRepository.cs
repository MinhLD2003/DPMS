using DPMS_WebAPI.Interfaces.Repositories;
using DPMS_WebAPI.Models;
using DPMS_WebAPI.Utils;
using DPMS_WebAPI.ViewModels;
using Google;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DPMS_WebAPI.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DPMSContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(DPMSContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<PagedResponse<T>> GetPagedAsync(QueryParams queryParams, params Expression<Func<T, object>>[]?includes)
        {
            IQueryable<T> query = _dbSet;

            // Apply includes for related entities if provided
            if (includes != null && includes.Any())
            {
                query = query.ApplyIncludes(includes);
            }

            // Apply filtering
            query = query.ApplyFiltering(queryParams.Filters);

            // Apply sorting
            query = query.ApplySorting(queryParams.SortBy, queryParams.SortDirection);

            // Apply pagination and return results
            return query.ToPagedResponse(queryParams.PageNumber, queryParams.PageSize);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual void Update(T entity)
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public virtual async Task DeleteAsync(object id)
        {
            T? entityToDelete = await _dbSet.FindAsync(id);
            if (entityToDelete != null)
                Delete(entityToDelete);
        }

        public virtual void Delete(T entity)
        {
            if (_context.Entry(entity).State == EntityState.Detached)
                _dbSet.Attach(entity);

            _dbSet.Remove(entity);
        }

        public async Task BulkAddAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities cannot be null or empty.");

            await _dbSet.AddRangeAsync(entities);
        }

        public async Task BulkDeleteAsync(IEnumerable<T> entities)
        {
            if (entities == null || !entities.Any())
                throw new ArgumentException("Entities cannot be null or empty.");

            _dbSet.RemoveRange(entities);
        }

        /// <summary>
        /// Get entity with related entities
        /// </summary>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        public virtual async Task<T?> GetDetailAsync(object id, params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet;
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));
        }
    }
}
