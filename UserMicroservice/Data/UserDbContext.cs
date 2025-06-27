using Microsoft.EntityFrameworkCore;
using UserMicroservice.Data.Entities;

namespace UserMicroservice.Data{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
    }
}
