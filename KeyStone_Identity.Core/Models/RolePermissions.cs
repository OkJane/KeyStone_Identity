using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Models
{
    public class RolePermissions
    {
        public int ID { get; set; }
        public long RoleID { get; set; }
        public int PermissionID { get; set; }
        public Role Role { get; set; }
        public Permissions Permission { get; set; } = null!;
    }
}
