using ProductManagement.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.BusinessLogic
{
    public class Service<T> : IService<T> where T : class
    {
        private readonly IUnitOfWork _unitOfWork;

        public Service(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IQueryable<T> Get()
        {
            return _unitOfWork.GetRepository<T>().Get();
        }

        public async Task<T> AddAsync(T entity)
        {
            var addedEntity = await _unitOfWork.GetRepository<T>().AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();
            return addedEntity;
        }

        public async Task UpdateAsync(T entity)
        {
            await _unitOfWork.GetRepository<T>().UpdateAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            await _unitOfWork.GetRepository<T>().RemoveAsync(entity);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
