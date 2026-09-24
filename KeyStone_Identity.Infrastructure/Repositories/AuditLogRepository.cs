using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly KeyStone_Identity_DbContext _dbContext;
        public AuditLogRepository(KeyStone_Identity_DbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task CreateLogEntry(AuditLog logEntry)
        {
            await _dbContext.AuditLogs.AddAsync(logEntry);
            await _dbContext.SaveChangesAsync();
        }
    }
}
