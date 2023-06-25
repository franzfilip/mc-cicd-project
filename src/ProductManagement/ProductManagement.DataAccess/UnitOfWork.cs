using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using ProductManagement.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductManagement.DataAccess
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ProductManagementDbContext _context;
        private IDbContextTransaction _transaction;
        private readonly IDictionary<Type, object> _repositories;

        public UnitOfWork(ProductManagementDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            if (_repositories.TryGetValue(typeof(TEntity), out var repository))
            {
                return (IRepository<TEntity>)repository;
            }

            var newRepository = new Repository<TEntity>(_context);
            _repositories[typeof(TEntity)] = newRepository;
            return newRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException("Cannot begin a new transaction while an existing transaction is still active.");
            }

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await _transaction.RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _transaction.Rollback();
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            _context.Dispose();
        }
    }

}
