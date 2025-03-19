
using Microsoft.EntityFrameworkCore;

using AutoMapper;

using user_management.Databases;
using user_management.Exceptions;
using user_management.Models;


namespace user_management.Services
{
public class UserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly IMapper _mapper; // Inject AutoMapper

    // Store the current logged-in user
    public User? CurrentUser { get; set; }

    public UserService(AppDbContext context, IMapper mapper, ILogger<UserService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

   

    // Login User - This will check credentials and set the CurrentUser property
    public async Task<bool> LoginAsync()
    {
        var username = CurrentUser.Username;
        var email = CurrentUser.Email;

        // Find the user by username or email
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        _logger.LogInformation("Existing User: {ExistingUser}", existingUser.ToString());

        if (existingUser == null)
        {
            _logger.LogWarning("Account With This Username or Email Does Not Exist.");
            throw new WrongCredentialException("Account With This Username or Email Does Not Exist.");
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(CurrentUser.PasswordHash, existingUser.PasswordHash);

        if (!isPasswordValid)
        {   _logger.LogWarning("Invalid Password.");
            return false;
        }

        // Update account status to Active
        var activeStatus = await _context.AccountStatuses.FindAsync(1);

        if (activeStatus == null)
        {
            _logger.LogError("Failed to retrieve active status.");
            throw new FailToRetrieveAccountInfoException("Failed to retrieve active status.");
        }

        existingUser.AccountStatusId = activeStatus.AccountStatusId;
        existingUser.UpdatedAt = DateTime.UtcNow;
        _logger.LogInformation("Account Status Updated to Active.");

        // Log the login history
        var loginHistory = new LoginHistory
        {
            UserId = existingUser.UserId,
            LoginTimestamp = DateTime.UtcNow,
            IpAddress = "127.0.0.1", // Example IP Address
            DeviceInfo = "Web Browser", // Example Device Info
            FailedAttempts = 0,
            LoginSuccessful = true
        };

        _context.LoginHistories.Add(loginHistory);
        await _context.SaveChangesAsync();

        // Set the CurrentUser property to the logged-in user
        CurrentUser = existingUser;

        return true;
    }

    // Logout User - This will clear the CurrentUser property
public async Task<bool> LogoutAsync()
{
    var username = CurrentUser.Username;
    var email = CurrentUser.Email;
    
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

    _logger.LogInformation("Existing User: {User}", user.ToString());
    
    if (user == null)
        throw new FailToRetrieveAccountInfoException("User not found.");

    if (user.AccountStatusId == 7)
    {
        return false;
    }

    // Find the "Logged Out" status in the database
    var loggedOutStatus = await _context.AccountStatuses.FindAsync(7);

    if (loggedOutStatus == null)
        throw new FailToUpdateException("Failed to retrieve 'Logged Out' status.");

    // Update user account status to "Logged Out"
    user.AccountStatusId = loggedOutStatus.AccountStatusId;
    user.UpdatedAt = DateTime.UtcNow;

    // Log the logout history
    var logoutHistory = new LoginHistory
    {
        UserId = user.UserId,
        LoginTimestamp = DateTime.UtcNow,
        IpAddress = "127.0.0.1",
        DeviceInfo = "Web Browser", 
        FailedAttempts = 0,
        LoginSuccessful = false
    };

    _context.LoginHistories.Add(logoutHistory);

    int result = await _context.SaveChangesAsync();
    if (result == 0)
        throw new FailToUpdateException("Failed to update logout status.");
    
    return result > 0;
}

    public async Task<LoginHistory> GetLoginStatusAsync()
    {
        return await _context.LoginHistories
            .Where(lh => lh.UserId == CurrentUser.UserId)
            .OrderByDescending(lh => lh.LoginTimestamp)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> DeactivateAccountAsync()
    {
            var user = await _context.Users.FindAsync(CurrentUser.UserId);
            if (user == null) return false;

            var inactiveStatus = await _context.AccountStatuses.FindAsync(8);
            
            if (inactiveStatus == null)
                throw new FailToDeactivateException("Failed to retrieve inactive status.");
  

            user.AccountStatus = inactiveStatus;
            user.UpdatedAt = DateTime.UtcNow;

            int result = await _context.SaveChangesAsync();
            return result > 0;
    }

public async Task<User?> UpdateAccountAsync(User updatedUser)
{
    var user = await _context.Users.FindAsync(CurrentUser?.UserId);
    if (user == null)
        throw new FailToRetrieveAccountInfoException("User not found.");

    // Update username if changed and not taken
    if (!string.IsNullOrEmpty(updatedUser.Username) && updatedUser.Username != user.Username)
    {
        bool usernameExists = await _context.Users.AnyAsync(u => u.Username == updatedUser.Username);
        if (usernameExists)
            throw new UsernameAlreadyExistException("Username is already taken.");

        user.Username = updatedUser.Username;
    }

    // Update email if changed and not taken
    if (!string.IsNullOrEmpty(updatedUser.Email) && updatedUser.Email != user.Email)
    {
        bool emailExists = await _context.Users.AnyAsync(u => u.Email == updatedUser.Email);
        if (emailExists)
            throw new EmailAlreadyExistException("Email is already registered.");

        user.Email = updatedUser.Email;
    }

    // Update password if provided
    if (!string.IsNullOrEmpty(updatedUser.PasswordHash))
    {
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updatedUser.PasswordHash);
    }

    user.UpdatedAt = DateTime.UtcNow;

    int result = await _context.SaveChangesAsync();
    if (result == 0)
        throw new FailToUpdateException("Failed to update user account.");

    return user;
}


}
}