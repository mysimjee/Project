
using Microsoft.EntityFrameworkCore;
using user_management.Databases;
using user_management.Models;

namespace user_management.Services
{
    public class AccountStatusService
    {
        private readonly AppDbContext _context;

        public AccountStatusService(AppDbContext context)
        {
            _context = context;
        }

        // Get account status by ID
        public async Task<AccountStatus> GetStatusInfoAsync(int accountStatusId)
        {
            return await _context.AccountStatuses.FindAsync(accountStatusId);
        }

        // Create a new account status
        public async Task<AccountStatus> CreateStatusAsync(string statusName, string description)
        {
            var status = new AccountStatus
            {
                StatusName = statusName,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.AccountStatuses.Add(status);
            await _context.SaveChangesAsync();
            return status;
        }

        // Update an existing account status
        public async Task<AccountStatus> UpdateStatusAsync(int accountStatusId, string statusName, string description)
        {
            var status = await _context.AccountStatuses.FindAsync(accountStatusId);
            if (status == null) return null;

            status.StatusName = statusName;
            status.Description = description;
            status.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return status;
        }

        // Delete an account status
        public async Task<bool> DeleteStatusAsync(int accountStatusId)
        {
            var status = await _context.AccountStatuses.FindAsync(accountStatusId);
            if (status == null) return false;

            _context.AccountStatuses.Remove(status);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
