using System;
using System.Collections.Generic;
using System.Text;
using KariyerNet.Application.Interfaces;
using KariyerNet.Domain.Entities;
using KariyerNet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KariyerNet.Infrastructure.Repositories
{
      public class UserRepository: IUserRepository  
    {
        private readonly AppDbContext _context;
        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

    }
}
