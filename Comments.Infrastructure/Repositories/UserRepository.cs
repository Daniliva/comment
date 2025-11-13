using Comments.Core.Entities;
using Comments.Core.Interfaces;
using Comments.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Comments.Infrastructure.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetOrCreateUserAsync(string userName, string email, string? homePage, string ipAddress, string userAgent)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName && u.Email == email);

            if (existingUser != null)
            {
                existingUser.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existingUser;
            }

            var newUser = new User
            {
                UserName = userName,
                Email = email,
                HomePage = homePage,
                UserIP = ipAddress,
                UserAgent = userAgent,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            return newUser;
        }

        public async Task<bool> UserExistsAsync(string userName, string email)
        {
            return await _context.Users
                .AnyAsync(u => u.UserName == userName && u.Email == email);
        }
    }
}