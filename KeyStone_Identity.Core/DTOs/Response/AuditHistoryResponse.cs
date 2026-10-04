using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.DTOs.Response
{
    public class AuditHistoryResponse
    {
        public string Identifier { get; set; }
        public string EventType { get; set; }
        public string? Description { get; set; }
        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public bool Success { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
