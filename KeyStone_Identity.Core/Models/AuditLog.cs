using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class AuditLog
    {
        public Guid Id { get; set; }

        public long? UserId { get; set; }
        public string Identifier { get; set; }

        public string EventType { get; set; } = null!;

        public string? Description { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public bool Success { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
