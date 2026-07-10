using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class Client
    {
        public long Id { get; set; }
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string ClientSecret { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
