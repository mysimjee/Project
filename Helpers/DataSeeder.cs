using user_management.Models;
namespace user_management.Helpers;

public static class DataSeeder
{
    public static DateTime StaticDate = new DateTime(2025, 1, 1);

    public static List<AccountStatus> AccountStatusList =
    [
        new AccountStatus
        {
            AccountStatusId = 1, // Initial State
            StatusName = "Initial State",
            Description = "User initiates the authentication process.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 2, // Registered
            StatusName = "Registered",
            Description =
                "The user has completed the registration process but hasn’t taken any further action yet.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 3, // Active
            StatusName = "Active",
            Description = "User account is active and valid for authorization process.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 4, // Logged-In
            StatusName = "Logged-In",
            Description = "User is currently logged into the system and has access.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 5, // Inactive
            StatusName = "Inactive",
            Description = "User account is invalid for authorization process until reactivated.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 6, // Unregistered
            StatusName = "Unregistered",
            Description = "User account is removed from the system.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 7, // Logged-Out
            StatusName = "Logged-Out",
            Description = "The user has logged out and is no longer accessing the system.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 8, // Deactivated
            StatusName = "Deactivated",
            Description = "The user's account is disabled therefore cannot access the system.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 9, // Banned
            StatusName = "Banned",
            Description =
                "The user cannot access the system due to being banned for violating terms or rules.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new AccountStatus
        {
            AccountStatusId = 10, // Final State
            StatusName = "Final State",
            Description = "User credential is either authenticated or unauthenticated based on user actions.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        }
    ];
    public static List<Role> RoleList =
    [
        new Role
        {
            RoleId = 1, // ContentCreator
            RoleName = "ContentCreator",
            Description = "Content creators who upload and manage content.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new Role
        {
            RoleId = 2, // ProductionCompany
            RoleName = "ProductionCompany",
            Description = "Companies producing content for the platform.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new Role
        {
            RoleId = 3, // PlatformAdmin
            RoleName = "PlatformAdmin",
            Description = "Administrators with full control over the platform.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        },
        new Role
        {
            RoleId = 4, // Viewer
            RoleName = "Viewer",
            Description = "Users who can view content on the platform.",
            CreatedAt = StaticDate,
            UpdatedAt = StaticDate
        }
    ];
}

