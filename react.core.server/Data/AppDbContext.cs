using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using react.core.Server.Models;


namespace react.core.Server.Data
{
    public class AppDbContext : DbContext
    {
        DbSet<User> Users { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           
        }

    }
}
