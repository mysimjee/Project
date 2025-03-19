
using Microsoft.EntityFrameworkCore;
using user_management.Databases;
using user_management.Models;

namespace user_management.Services
{
    public class ContentCreatorService(AppDbContext context)
    {
        public async Task<bool> UpdateContentCreatorAsync(int userId, ContentCreator updatedCreator)
        {
            var contentCreator = await context.Users
                .OfType<ContentCreator>()
                .FirstOrDefaultAsync(cc => cc.UserId == userId);

            if (contentCreator == null)
                return false; 

            contentCreator.FirstName = updatedCreator.FirstName;
            contentCreator.LastName = updatedCreator.LastName;
            contentCreator.DateOfBirth = updatedCreator.DateOfBirth;
            contentCreator.Nrc = updatedCreator.Nrc;
            contentCreator.Biography = updatedCreator.Biography;
            contentCreator.SocialLinks = updatedCreator.SocialLinks;
            contentCreator.PortfolioUrl = updatedCreator.PortfolioUrl;
            contentCreator.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ContentCreator?> GetContentCreatorByIdAsync(int userId)
        {
            return await context.Users
                .OfType<ContentCreator>()
                .Include(cc => cc.Role)
                .Include(cc => cc.AccountStatus)                
                .FirstOrDefaultAsync(cc => cc.UserId == userId);
        }        
    }
}
