using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> GetRoleByRoleName (string roleName);
        Task<Role> GetRoleByUserId (long userId);
    }
}
