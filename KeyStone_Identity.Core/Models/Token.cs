using KeyStone_Identity.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class Token
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public TokenTypes Type { get; set; }
        public string TokenHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
    }
}
