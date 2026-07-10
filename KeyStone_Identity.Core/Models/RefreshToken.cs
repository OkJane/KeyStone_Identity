using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
    }
}
