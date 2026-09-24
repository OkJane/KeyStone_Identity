using KeyStone_Identity.Core.Enums;
using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IRequestContext _requestContext;
        public AuditLogService(IAuditLogRepository auditLogRepository, IRequestContext requestContext)
        {
            _auditLogRepository = auditLogRepository;
            _requestContext = requestContext;
        }
        public async Task LogAsync(AuditEventType eventType, long? userID, bool Success, string? identifier = null, string? decription = null)
        {
            var logEntry = new AuditLog
            {
                UserId = userID,
                EventType = eventType.ToString(),
                Success = Success,
                Description = decription,
                Identifier = identifier,
                IpAddress = _requestContext.IPAddress,
                UserAgent = _requestContext.UserAgent,
                CreatedAt = DateTime.UtcNow

            };
            await _auditLogRepository.CreateLogEntry(logEntry);
        }
    }
}
