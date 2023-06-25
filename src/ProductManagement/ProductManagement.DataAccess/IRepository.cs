using System.Linq.Expressions;

namespace ProductManagement.DataAccess
{
    public interface IRepository<TEntity> where TEntity : class
    {
        public IQueryable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, params Expression<Func<TEntity, object>>[] includes);
        public Task<TEntity> AddAsync(TEntity entity);
        public Task<TEntity> UpdateAsync(TEntity entity);
        public Task<bool> ExistsAsync(int id);
        public Task RemoveAsync(TEntity entity);
    }
}