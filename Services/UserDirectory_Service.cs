
using Microsoft.EntityFrameworkCore;

using AutoMapper;
using Serilog;
using user_management.Databases;
using user_management.Models;
using user_management.Exceptions;


namespace user_management.Services
{
    public class UserDirectory
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper; // Inject AutoMapper
        private readonly ILogger<UserDirectory> _logger;

        private readonly Dictionary<int, UserService> _userServices;
        public int TotalUsers { get; set; }
        private int Cursor { get; set; } 


        public UserDirectory(AppDbContext context, IHttpContextAccessor httpContextAccessor, IMapper mapper, ILogger<UserDirectory> logger)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _userServices = new Dictionary<int, UserService>();
            TotalUsers = _context.Users.Count();
        }

public async Task<int> GetUserIdByUsernameAsync(string username)
{
    var user = await _context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

    if (user == null)
    {
        _logger.LogError("User with username {0} not found.", username);
        return 0; 
    }
    return user.UserId;
}

public async Task<bool> AddUserAsync(User user)
        {

            // Check if the user already exists
            var accountWithExistingEmail =  await _context.Users.Where(u => u.Email == user.Email).FirstOrDefaultAsync();
            if (accountWithExistingEmail != null)
            {
                _logger.LogError("User with this email already exists.");
                throw new EmailAlreadyExistException("User with this email already exists.");
            }

            var accountWithExistingUsername =  await _context.Users.Where(u => u.Username == user.Username).FirstOrDefaultAsync();
            if (accountWithExistingUsername != null)
            {
                _logger.LogError("User with this username already exists.");
                throw new UsernameAlreadyExistException("User with this username already exists.");
            }
   
            // Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.PasswordHash = hashedPassword;
            _logger.LogInformation("Hashed Password: {0}", hashedPassword);

            // Add user to the appropriate DbSet based on RoleId
            _logger.LogInformation("User RoleId: {0}", user.RoleId);
            switch (user.RoleId)
            {
            case 1:
                _context.ContentCreators.Add(_mapper.Map<ContentCreator>(user));
                break;
            case 2:
                _context.ProductionCompanies.Add(_mapper.Map<ProductionCompany>(user));
                break;
            case 3:
                _context.PlatformAdmins.Add(_mapper.Map<PlatformAdmin>(user));
                break;
            case 4:
                _context.Viewers.Add(_mapper.Map<Viewer>(user));
                break;
            default:
                _context.Users.Add(user);
                break;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User Added: {0}", user);
            return true;
        }

        public async Task<bool> RemoveUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<AccountStatus?> ChangeAccountStatusAsync(int userId, int accountStatusId)
        {
            var user = await _context.Users.FindAsync(userId);
            var accountStatus = await _context.AccountStatuses.FindAsync(accountStatusId);
            if (user == null) return accountStatus;
            if (accountStatus == null) return accountStatus;
            user.AccountStatusId = accountStatus.AccountStatusId;
            await _context.SaveChangesAsync();
            return accountStatus;
        }

public async Task<Role?> ChangeRoleAsync(int userId, int roleId)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
    var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);

    if (user == null || role == null)
    {
        return null;
    }

    // Manually update the RoleId before removing
    user.RoleId = roleId;
    await _context.SaveChangesAsync(); // Ensure RoleId is saved

    // Remove the existing user
    _context.Users.Remove(user);
    await _context.SaveChangesAsync(); // Save to remove the old user

    // Create a new user with the correct type
    User newUser = roleId switch
    {
        1 => new ContentCreator(),
        2 => new ProductionCompany(),
        3 => new PlatformAdmin(),
        4 => new Viewer(),
        _ => new User()
    };

    // Copy shared properties
    newUser.UserId = user.UserId;
    newUser.Username = user.Username;
    newUser.Email = user.Email;
    newUser.PasswordHash = user.PasswordHash;
    newUser.RoleId = user.RoleId;
    newUser.CreatedAt = user.CreatedAt;
    newUser.UpdatedAt = DateTime.UtcNow;

    // Add and save the new user
         switch (roleId)
            {
            case 1:
                _context.ContentCreators.Add(_mapper.Map<ContentCreator>(user));
                break;
            case 2:
                _context.ProductionCompanies.Add(_mapper.Map<ProductionCompany>(user));
                break;
            case 3:
                _context.PlatformAdmins.Add(_mapper.Map<PlatformAdmin>(user));
                break;
            case 4:
                _context.Viewers.Add(_mapper.Map<Viewer>(user));
                break;
            default:
                _context.Users.Add(user);
                break;
            }
    await _context.SaveChangesAsync();

    return role;
}





        public async Task<List<User>> GetUsersAsync(int limit)
        {
            return await _context.Users
                .Include(cc => cc.Role)
                .Include(cc => cc.AccountStatus)
                .Include(cc => cc.LoginHistory)
                .OrderBy(u => u.UserId)
                .Skip(Cursor)
                .Take(limit)
                .ToListAsync();
        }

        public int SetCursor(int cursor)
        {
            this.Cursor = cursor;
            return this.Cursor;
        }

        public int GetCursor()
        {
            return this.Cursor;
        }

        private UserService GetUserService(int userId)
        {
            var serviceProvider = new ServiceCollection()
                .AddLogging(loggingBuilder => loggingBuilder.AddSerilog())
                .BuildServiceProvider();
            var userLogger = serviceProvider.GetService<ILogger<UserService>>();
            if (!_userServices.TryGetValue(userId, out UserService? value))
            {
                value = new UserService(_context, _mapper, _httpContextAccessor, userLogger!);
                _userServices[userId] = value;
            }
            return value;
        }

public async Task<List<User>> GetUsersByPropertyAsync(string propertyName, object value, int limit)
{
    var query = _context.Users
        .Include(u => u.Role)
        .Include(u => u.AccountStatus)
        .Include(u => u.LoginHistory)
        .AsQueryable();

    switch (propertyName.ToLower())
    {
        case "username":
            query = query.Where(u => u.Username.Contains((string)value));
            break;
        case "email":
            query = query.Where(u => u.Email.Contains((string)value));
            break;
        case "roleid":
            query = query.Where(u => u.RoleId == (int)value);
            break;
        case "userid":
            query = query.Where(u => u.UserId == (int)value);
            break;
        case "accountstatusid":
            query = query.Where(u => u.AccountStatus.AccountStatusId == (int)value);
            break;
        default:
            throw new ArgumentException("Invalid property name.");
    }

    return await query.Take(limit).ToListAsync();
}
        


    }
}
