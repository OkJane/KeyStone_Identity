using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        public readonly KeyStone_Identity_DbContext _DBcontext;
        public UserRoleRepository(KeyStone_Identity_DbContext DBcontext)
        {
            _DBcontext = DBcontext;
        }
        public async Task<UserRole> Insert(UserRole userRole)
        {
            await _DBcontext.UserRoles.AddAsync(userRole);
            await _DBcontext.SaveChangesAsync();
            return userRole;
        }
    }
}
