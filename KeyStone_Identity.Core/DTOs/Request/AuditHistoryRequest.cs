using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.DTOs.Request
{
    public class AuditHistoryRequest
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public string? Username { get; set; }

    }
}
