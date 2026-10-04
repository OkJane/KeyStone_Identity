using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly KeyStone_Identity_DbContext _dbContext;

        public RoleRepository(KeyStone_Identity_DbContext dbContext)
        {    
            _dbContext = dbContext;
        }
        public async Task<Role> GetRoleByRoleName(string roleName)
        {
            return await _dbContext.Roles.FirstOrDefaultAsync(x => x.Name == roleName);
        }

        public async Task<Role> GetRoleByUserId(long userId)
        {
            return await _dbContext.UserRoles.Where(x => x.UserId == userId).Select(x => x.Role).FirstOrDefaultAsync();
        }
    }
}
