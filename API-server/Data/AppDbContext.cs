using Microsoft.EntityFrameworkCore;
using API_server.Models;

namespace API_server.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Weather> Weather { get; set; }
        public DbSet<Phenomenon> Phenomenon { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    }
}
