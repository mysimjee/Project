using Microsoft.EntityFrameworkCore; 
using FluentValidation;

using user_management.Models;
using user_management.Databases;

namespace user_management.Validators
{

    public class LoginValidator : AbstractValidator<User>
    {
        private readonly AppDbContext _context;

        public LoginValidator(AppDbContext context)
        {
            _context = context;
            
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .Length(5, 50).WithMessage("Username must be between 5 and 50 characters.")
                .MustAsync(BeExistingUsername)
                .When(x => !string.IsNullOrWhiteSpace(x.Username)) // Only enforce when Username is not empty or whitespace
                .WithMessage("Account with that username does not exist.");
            
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop) // Stops validation once a rule fails
                .MustAsync(BeExistingEmail)
                .When(x => !string.IsNullOrWhiteSpace(x.Email)) // Only enforce if email is provided (not empty/whitespace)
                .WithMessage("Account with that email does not exist.");
            
            RuleFor(x => x.PasswordHash)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");
            
            RuleFor(x => new { x.Username, x.Email }) // Validate based on Username or Email
                .MustAsync((user, cancellationToken) => BeValidAccountStatusId(user.Username, user.Email, cancellationToken))
                .When(x => !string.IsNullOrWhiteSpace(x.Username) || !string.IsNullOrWhiteSpace(x.Email)) // Apply only if either is not empty
                .WithMessage("Account is not in a valid state to log in.");



        }
        
        private async Task<bool> BeExistingUsername(string username, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);
        }

        private async Task<bool> BeExistingEmail(string email, CancellationToken cancellationToken)
        {
            return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
        }
        
        private async Task<bool> BeValidAccountStatusId(string username, string email, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email, cancellationToken);

            if (user == null)
                return false; 

            // Check if AccountStatusId is invalid (6, 8, or 9)
            return !new[] { 6, 8, 9 }.Contains(user.AccountStatusId);
        }

    }
    public class RegisterValidator : AbstractValidator<User>
{
    private readonly AppDbContext _context;

    public RegisterValidator(AppDbContext context)
    {
        _context = context;

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(5, 50).WithMessage("Username must be between 5 and 50 characters.")
            .MustAsync(BeUniqueUsername).WithMessage("Username already exists.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Please provide a valid email address.")
            .Length(5, 100).WithMessage("Email must be between 5 and 100 characters.")
            .MustAsync(BeUniqueEmail).WithMessage("Email already exists.");


        RuleFor(x => x.PasswordHash)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Length(5, 15).WithMessage("Phone number must be between 5 and 15 characters.")
            .Matches(@"^\d+$").WithMessage("Phone number must contain only digits.");


        RuleFor(x => x.RecoveryEmail)
            .EmailAddress()
            .WithMessage("Please provide a valid email address.");           

        RuleFor(x => x.Country)
            .NotEmpty().WithMessage("Country is required.")
            .Length(2, 50).WithMessage("Country must be between 2 and 50 characters.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.")
            .Length(2, 50).WithMessage("State must be between 2 and 50 characters.");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("Zip code is required.")
            .Length(5, 10).WithMessage("Zip code must be between 5 and 10 characters.")
            .Matches(@"^\d+$").WithMessage("Zip code must contain only digits.");

        RuleFor(x => x.RoleId)
            .GreaterThan(0).WithMessage("Role ID must be greater than 0.");

        RuleFor(x => x.AccountStatusId)
            .GreaterThan(0).WithMessage("Account status ID must be greater than 0.");
    }


    private async Task<bool> BeUniqueUsername(string username, CancellationToken cancellationToken)
    {
            var accountWithExistingEmail =  await _context.Users.Where(u => u.Email == username).FirstOrDefaultAsync(cancellationToken);
            return accountWithExistingEmail == null;
    }

    private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
    {
            var accountWithExistingUsername =  await _context.Users.Where(u => u.Username == email).FirstOrDefaultAsync(cancellationToken);
            return accountWithExistingUsername == null;
    }
}
}
