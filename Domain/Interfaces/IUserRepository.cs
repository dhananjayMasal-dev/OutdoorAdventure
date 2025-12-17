using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetUserByIdAsync(int id);
        Task AddUserAsync(User user);
        Task SaveChangesAsync();
        Task<User?> GetUserByEmailAsync(string email);
    }
}
