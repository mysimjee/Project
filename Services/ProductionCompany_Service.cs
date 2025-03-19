
using Microsoft.EntityFrameworkCore;
using user_management.Databases;
using user_management.Models;



namespace user_management.Services
{
public class ProductionCompanyService(AppDbContext context)
{
    public async Task<bool> UpdateProductionCompanyAsync(int userId, ProductionCompany updatedCompany)
        {
            var productionCompany = await context.Users
                .OfType<ProductionCompany>()
                .FirstOrDefaultAsync(pc => pc.UserId == userId);

            if (productionCompany == null)
                return false;

            productionCompany.CompanyName = updatedCompany.CompanyName;
            productionCompany.CompanyWebsite = updatedCompany.CompanyWebsite;
            productionCompany.FoundingDate = updatedCompany.FoundingDate;
            productionCompany.Biography = updatedCompany.Biography;
            productionCompany.SocialLinks = updatedCompany.SocialLinks;
            productionCompany.PortfolioUrl = updatedCompany.PortfolioUrl;
            productionCompany.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();
            return true;
        }

        public async Task<ProductionCompany?> GetProductionCompanyByIdAsync(int userId)
        {
            return await context.Users
                .OfType<ProductionCompany>()
                .Include(cc => cc.Role)
                .Include(cc => cc.AccountStatus)
                .FirstOrDefaultAsync(pc => pc.UserId == userId);
        }
    }
}