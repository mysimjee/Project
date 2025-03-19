
using Microsoft.EntityFrameworkCore;

using user_management.Databases;
using user_management.Models;

namespace user_management.Services
{
    public class ViewerService
    {
        private readonly AppDbContext _context;

        public ViewerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateViewerAsync(int userId, Viewer updatedViewer)
        {
            var viewer = await _context.Users
                .OfType<Viewer>()
                .FirstOrDefaultAsync(v => v.UserId == userId);

            if (viewer == null)
                return false;

            viewer.FirstName = updatedViewer.FirstName;
            viewer.LastName = updatedViewer.LastName;
            viewer.DateOfBirth = updatedViewer.DateOfBirth;
            viewer.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Viewer?> GetViewerByIdAsync(int userId)
        {
            return await _context.Users
                .OfType<Viewer>()
                .Include(cc => cc.Role)
                .Include(cc => cc.AccountStatus)
                .FirstOrDefaultAsync(v => v.UserId == userId);
        }
    }
}