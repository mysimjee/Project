using Microsoft.EntityFrameworkCore;

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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=/Users/mohamedsimjee/RiderProjects/user_management/Databases/SQLLiteDatabase.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Use a fixed date (static value) to avoid dynamic values like DateTime.UtcNow
            DateTime staticDate = new DateTime(2025, 1, 1); // Example static date

            // Seed roles data using numeric IDs mapped to specific role types
            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    RoleId = 1, // ContentCreator
                    RoleName = "ContentCreator",
                    Description = "Content creators who upload and manage content.",
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Role
                {
                    RoleId = 2, // ProductionCompany
                    RoleName = "ProductionCompany",
                    Description = "Companies producing content for the platform.",
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Role
                {
                    RoleId = 3, // PlatformAdmin
                    RoleName = "PlatformAdmin",
                    Description = "Administrators with full control over the platform.",
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                },
                new Role
                {
                    RoleId = 4, // Viewer
                    RoleName = "Viewer",
                    Description = "Users who can view content on the platform.",
                    CreatedAt = staticDate,
                    UpdatedAt = staticDate
                }
            );
            
              // Seed AccountStatuses data
              modelBuilder.Entity<AccountStatus>().HasData(
                  new AccountStatus
                  {
                      AccountStatusId = 1, // Initial State
                      StatusName = "Initial State",
                      Description = "User initiates the authentication process.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 2, // Registered
                      StatusName = "Registered",
                      Description =
                          "The user has completed the registration process but hasn’t taken any further action yet.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 3, // Active
                      StatusName = "Active",
                      Description = "User account is active and valid for authorization process.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 4, // Logged-In
                      StatusName = "Logged-In",
                      Description = "User is currently logged into the system and has access.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 5, // Inactive
                      StatusName = "Inactive",
                      Description = "User account is invalid for authorization process until reactivated.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 6, // Unregistered
                      StatusName = "Unregistered",
                      Description = "User account is removed from the system.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 7, // Logged-Out
                      StatusName = "Logged-Out",
                      Description = "The user has logged out and is no longer accessing the system.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 8, // Deactivated
                      StatusName = "Deactivated",
                      Description = "The user's account is disabled therefore cannot access the system.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 9, // Banned
                      StatusName = "Banned",
                      Description =
                          "The user cannot access the system due to being banned for violating terms or rules.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  },
                  new AccountStatus
                  {
                      AccountStatusId = 10, // Final State
                      StatusName = "Final State",
                      Description = "User credential is either authenticated or unauthenticated based on user actions.",
                      CreatedAt = staticDate,
                      UpdatedAt = staticDate
                  });

        }
    }

   
}