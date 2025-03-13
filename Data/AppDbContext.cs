using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CourierApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CourierApp.Data
{
        public class AppDbContext : DbContext
        {
            public DbSet<Weather> Weather { get; set; }
            public DbSet<Phenomenon> Phenomenon { get; set; }

            // Constructor for runtime (used with DI)
            public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

            // Parameterless constructor for design-time
            public AppDbContext() {}

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseSqlite(@"Data Source=..\..\..\MyDatabase.db");
                }
            }

            // Optional: Configure relationships
            //protected override void OnModelCreating(ModelBuilder modelBuilder)
            //{
            //    modelBuilder.Entity<Weather>()
            //        .HasOne(w => w.Phenomenon)
            //        .WithMany()
            //        .HasForeignKey(w => w.PhenomenonID);
            //}
        }

}
