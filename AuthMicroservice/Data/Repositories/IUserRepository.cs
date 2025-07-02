// // Data/Repositories/IUserRepository.cs
using AuthMicroservice.Data.Entities;

namespace AuthMicroservice.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<List<User>> GetAllAsync();
    }
}