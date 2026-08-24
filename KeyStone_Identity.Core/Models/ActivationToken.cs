using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class ActivationToken
    {
        public long ID { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }

    }
}
