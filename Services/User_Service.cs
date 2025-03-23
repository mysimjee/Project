
using Microsoft.EntityFrameworkCore;


using Microsoft.AspNetCore.SignalR;
using user_management.Databases;
using user_management.Exceptions;
using user_management.Hubs;
using user_management.Models;


namespace user_management.Services
{
public class UserService(
    AppDbContext context,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UserService> logger,
    IHubContext<NotificationHub> hubContext)
{

    public User? CurrentUser { get; set; }


    public async Task<bool> LoginAsync()
    {
        // Find the user by username or email
        var existingUser = await context.Users
            .FirstOrDefaultAsync(u => u.Username == CurrentUser!.Username || u.Email == CurrentUser!.Email);
        
        var today = DateTime.UtcNow.Date;


        int failedAttemptsToday = await context.LoginHistories
            .Where(lh => lh.UserId == existingUser!.UserId &&
                         lh.LoginTimestamp >= today &&
                         !lh.LoginSuccessful)
            .CountAsync();
        
        var loginHistory = new LoginHistory
        {
            UserId = existingUser!.UserId,
            LoginTimestamp = DateTime.UtcNow,
            IpAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            DeviceInfo = httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString(), 
            FailedAttempts = failedAttemptsToday,
            LoginSuccessful = false
        };
        
        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(CurrentUser!.PasswordHash, existingUser.PasswordHash);

        if (!isPasswordValid)
        {   logger.LogWarning("Invalid Password.");
            context.LoginHistories.Add(loginHistory);
            await context.SaveChangesAsync();
            return false;
        }
        
        loginHistory.LoginSuccessful = true;

        // Update account status to Active
        var activeStatus = await context.AccountStatuses.FindAsync(3);

        if (activeStatus == null)
        {
            logger.LogError("Failed to retrieve active status.");
            throw new FailToRetrieveAccountInfoException("Failed to retrieve active status.");
        }

        existingUser.AccountStatusId = activeStatus.AccountStatusId;
        existingUser.UpdatedAt = DateTime.UtcNow;
        logger.LogInformation("Account Status Updated to Active.");
        
        context.LoginHistories.Add(loginHistory);
        await context.SaveChangesAsync();
        
        CurrentUser = existingUser;

        if (loginHistory.LoginSuccessful)
        {
            await hubContext.Clients.Group("Admins").SendAsync("ReceiveUserNotification", $"User {existingUser.Username} has been login.");
        }
        return true;
    }

    // Logout User - This will clear the CurrentUser property
public async Task<bool> LogoutAsync()
{

    var user = await context.Users.FirstOrDefaultAsync(u => u.Username == CurrentUser!.Username || u.Email == CurrentUser!.Email);
    

    if (user == null)
    {
        throw new FailToRetrieveAccountInfoException("User not found.");

    }
    if (user.AccountStatusId == 7)
    {
        return false;
    }

    // Find the "Logged Out" status in the database
    var loggedOutStatus = await context.AccountStatuses.FindAsync(7);

    if (loggedOutStatus == null)
    {
        throw new FailToUpdateException("Failed to retrieve 'Logged Out' status.");
    }

    // Update user account status to "Logged Out"
    user.AccountStatusId = loggedOutStatus.AccountStatusId;
    user.UpdatedAt = DateTime.UtcNow;
    
    
    var result = await context.SaveChangesAsync();
    switch (result)
    {
        case 0:
            throw new FailToUpdateException("Failed to update logout status.");
        case > 0:
            await hubContext.Clients.Group("Admins").SendAsync("ReceiveUserNotification", $"User {user.Username} has been logout!");
            break;
    }

    return result > 0;
}

    public async Task<LoginHistory?> GetLoginStatusAsync()
    {
        return await context.LoginHistories
            .Where(lh => lh.UserId == CurrentUser!.UserId)
            .OrderByDescending(lh => lh.LoginTimestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<List<LoginHistory>> GetLoginHistoryAsync(int userId, int skip, int limit)
    {
        return await context.LoginHistories
            .Where(lh => lh.UserId == userId)
            .OrderByDescending(lh => lh.LoginTimestamp)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }
    
    public async Task<bool> DeactivateAccountAsync()
    {
            var user = await context.Users.FindAsync(CurrentUser!.UserId);
            if (user == null) return false;

            var inactiveStatus = await context.AccountStatuses.FindAsync(8);
            
            if (inactiveStatus == null)
                throw new FailToDeactivateException("Failed to retrieve inactive status.");
  

            user.AccountStatus = inactiveStatus;
            user.UpdatedAt = DateTime.UtcNow;

            int result = await context.SaveChangesAsync();
            if (result > 0)
            {
                await hubContext.Clients.Group("Admins").SendAsync("ReceiveUserNotification", $"User {user.Username} has been deactivated!");
            }
            return result > 0;
    }
    
    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await context.SaveChangesAsync();
        await hubContext.Clients.Group("Admins").SendAsync("ReceiveUserNotification", $"User {user.Username} has reset password!");
        return true;
    }

public async Task<User?> UpdateAccountAsync(User updatedUser)
{
    var user = await context.Users.FindAsync(CurrentUser?.UserId);
    if (user == null)
        throw new FailToRetrieveAccountInfoException("User not found.");

    // Update username if changed and not taken
    if (!string.IsNullOrEmpty(updatedUser.Username) && updatedUser.Username != user.Username)
    {
        bool usernameExists = await context.Users.AnyAsync(u => u.Username == updatedUser.Username);
        if (usernameExists)
            throw new UsernameAlreadyExistException("Username is already taken.");

        user.Username = updatedUser.Username;
    }

    // Update email if changed and not taken
    if (!string.IsNullOrEmpty(updatedUser.Email) && updatedUser.Email != user.Email)
    {
        bool emailExists = await context.Users.AnyAsync(u => u.Email == updatedUser.Email);
        if (emailExists)
            throw new EmailAlreadyExistException("An account with this email already registered.");

        user.Email = updatedUser.Email;
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(updatedUser.PasswordHash) && updatedUser.PasswordHash != user.PasswordHash)
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash);
    }

    user.UpdatedAt = DateTime.UtcNow;

    user.RoleId = updatedUser.RoleId;
    user.ZipCode = updatedUser.ZipCode;
    user.PhoneNumber = updatedUser.PhoneNumber;
    user.Country = updatedUser.Country;
    user.RecoveryEmail = updatedUser.RecoveryEmail;
    user.ProfileImgPath = updatedUser.ProfileImgPath;
    
    int result = await context.SaveChangesAsync();
    
    switch (result)
    {
        case 0:
            throw new FailToUpdateException();
        case > 0:
            await hubContext.Clients.Group("Admins").SendAsync("ReceiveUserNotification", $"User: {user.Username} has been updated!");
            break;
    }

    return user;
}


}
}