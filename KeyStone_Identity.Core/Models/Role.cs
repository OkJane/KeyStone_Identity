using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
