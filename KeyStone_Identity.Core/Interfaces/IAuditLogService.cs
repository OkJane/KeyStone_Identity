using KeyStone_Identity.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IAuditLogService
    {
        Task LogAsync(AuditEventType eventType, long? userID, bool Success, string? identifier = null, string ? decription = null);
    }
}
