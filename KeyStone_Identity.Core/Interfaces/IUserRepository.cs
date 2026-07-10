using KeyStone_Identity.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KeyStone_Identity.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> UserExists(string username, string emailAddress);
        Task<bool> EmailExists(string emailAddress);
        Task<User> Upsert(User user);
        Task<User> RetrieveUserByUserName(string username);
        Task<User> RetrieveUserByEmailAddress(string emailAddress);
        Task<User> GetUserById(long userID);
    }
}
