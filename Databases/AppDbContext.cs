using Microsoft.EntityFrameworkCore;
using user_management.Helpers;
using user_management.Models;

namespace user_management.Databases
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<AccountStatus> AccountStatuses { get; set; }
        public DbSet<LoginHistory> LoginHistories { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<ContentCreator> ContentCreators { get; set; }
        public DbSet<ProductionCompany> ProductionCompanies { get; set; }
        public DbSet<Viewer> Viewers { get; set; }
        public DbSet<PlatformAdmin> PlatformAdmins { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=/Users/mohamedsimjee/RiderProjects/user_management/Databases/SQLLiteDatabase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed roles data using numeric IDs mapped to specific role types
            modelBuilder.Entity<Role>().HasData(DataSeeder.RoleList);
            
            // Seed AccountStatuses data
            modelBuilder.Entity<AccountStatus>().HasData(DataSeeder.AccountStatusList);

        }
    }

   
}