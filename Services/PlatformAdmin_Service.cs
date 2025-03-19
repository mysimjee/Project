
using Microsoft.EntityFrameworkCore;
using user_management.Databases;
using user_management.Models;


namespace user_management.Services
{
    public class PlatformAdminService
    {
        private readonly AppDbContext _context;

        public PlatformAdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdatePlatformAdminAsync(int userId, PlatformAdmin updatedAdmin)
        {
            var platformAdmin = await _context.Users
                .OfType<PlatformAdmin>()
                .FirstOrDefaultAsync(pa => pa.UserId == userId);

            if (platformAdmin == null)
                return false;

            platformAdmin.FirstName = updatedAdmin.FirstName;
            platformAdmin.LastName = updatedAdmin.LastName;
            platformAdmin.DateOfBirth = updatedAdmin.DateOfBirth;
            platformAdmin.EmploymentDate = updatedAdmin.EmploymentDate;
            platformAdmin.JobRole = updatedAdmin.JobRole;
            platformAdmin.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PlatformAdmin?> GetPlatformAdminByIdAsync(int userId)
        {
            return await _context.Users
                .OfType<PlatformAdmin>()      
                .Include(cc => cc.Role)
                .Include(cc => cc.AccountStatus)
                .FirstOrDefaultAsync(pa => pa.UserId == userId);
        }
        
}
}