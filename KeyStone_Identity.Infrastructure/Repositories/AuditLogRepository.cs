using KeyStone_Identity.Core.DTOs.Request;
using KeyStone_Identity.Core.DTOs.Response;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
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

        public async Task<List<AuditHistoryResponse>> GetAuditHistory(AuditHistoryRequest filter)
        {
            IQueryable<AuditLog> query = _dbContext.AuditLogs;

            if (filter.StartDate.HasValue)
            {
                query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) >= filter.StartDate.Value);
            }
            if (filter.EndDate.HasValue)
            {
                query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) <= filter.EndDate.Value);
            }
            if(filter.Username != null)
            {
                query = query.Where(x => EF.Functions.Like(x.Identifier.ToLower(), $"%{filter.Username.ToLower()}%"));
            }
            return await query
                .Select(x => new AuditHistoryResponse
                {
                    Identifier = x.Identifier,
                    EventType = x.EventType,
                    Description = x.Description,
                    IpAddress = x.IpAddress,
                    UserAgent = x.UserAgent,
                    Success = x.Success,
                    CreatedAt = x.CreatedAt,
                }).ToListAsync();

        }
    }
}
