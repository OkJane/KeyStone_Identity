using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KeyStone_Identity_DbContext _dbContext;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(KeyStone_Identity_DbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No active transaction.");

            await _transaction.CommitAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }
    }
}
