using Comments.Application;

namespace Comments.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User> GetOrCreateUserAsync(string userName, string email, string? homePage, string ipAddress, string userAgent);
    Task<bool> UserExistsAsync(string userName, string email);
}