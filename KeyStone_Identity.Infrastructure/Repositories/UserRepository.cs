using KeyStone_Identity.Core.Interfaces;
using KeyStone_Identity.Core.Models;
using KeyStone_Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace KeyStone_Identity.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly KeyStone_Identity_DbContext _databaseContext;
        public UserRepository(KeyStone_Identity_DbContext dbContext)
        {
         this._databaseContext = dbContext;   
        }

        public async Task<bool> UserExists(string username, string emailAddress)
        {
            return await _databaseContext.Users.Where(x => x.UserName == username || x.EmailAddress == emailAddress).AnyAsync();
        }

        public async Task<User> GetUser(string username)
        {
            var mail = new MailAddress(username);
            if(mail.Address == username)
            {
                return await RetrieveUserByEmailAddress(username);
            }
            else
            {
                return await RetrieveUserByUserName(username);
            }
        }

        public async Task<User> RetrieveUserByUserName(string username)
        {
            return await _databaseContext.Users.FirstOrDefaultAsync(x => x.UserName.Trim().ToLower() == username.Trim().ToLower());
        }

        public async Task<User> RetrieveUserByEmailAddress(string emailAddress)
        {
            return await _databaseContext.Users.FirstOrDefaultAsync(x => x.EmailAddress.Trim().ToLower() == emailAddress.Trim().ToLower());
        }

        public async Task<bool> EmailExists(string emailAddress)
        {
            return await _databaseContext.Users.Where(x => x.EmailAddress == emailAddress ).AnyAsync();
        }

        public async Task<User> Upsert(User user)
        {
            try
            {
                var existingUser = await _databaseContext.Users.Where(x => x.ID == user.ID).AnyAsync();
                if (!existingUser)
                {
                    await _databaseContext.Users.AddAsync(user);
                }
                else
                {
                    _databaseContext.Users.Update(user);
                }
                await _databaseContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message} \n Exception: {ex.ToString()} \n Inner Exception: {ex?.InnerException}");
            }
        }
    }
}
