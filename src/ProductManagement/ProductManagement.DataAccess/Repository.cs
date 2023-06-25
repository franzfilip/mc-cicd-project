using Microsoft.EntityFrameworkCore;
using ProductManagement.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.DataAccess
{
    public class Repository<TEntity>: IRepository<TEntity> where TEntity : class
    {
        protected readonly ProductManagementDbContext _context;
        public Repository(ProductManagementDbContext context)
        {
            this._context = context;
        }

        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.AsQueryable();
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            if (entity is null)
            {
                return null;
            }

            await _context.AddAsync(entity);
            return entity;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Entry(entity).State = EntityState.Modified;
            return entity;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Set<TEntity>().FindAsync(id) != null;
        }

        public async Task RemoveAsync(TEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _context.Set<TEntity>().Remove(entity);
        }
    }
}
