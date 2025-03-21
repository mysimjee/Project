
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.SignalR;
using user_management.Databases;
using user_management.Hubs;
using user_management.Models;


namespace user_management.Services;

public class EmailService(AppDbContext context, ILogger<EmailService> logger, IHubContext<NotificationHub> hubContext)
{
    private readonly string _senderEmail = "zakiayayacoob@gmail.com";
    private readonly string _emailPassword = "qdmj hvyc okrn bphv";
    private readonly string _host = "smtp.gmail.com";
    private readonly int _port = 587;

    private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates a 6-digit code
        }

        public async Task<bool> SendEmailVerificationCodeAsync(string email)
        {
            
            try
            {
                string verificationCode = GenerateVerificationCode();
    
                VerificationCode codeEntry = new VerificationCode
                {
                    Email = email,
                    Code = verificationCode,
                    ExpirationDate = DateTime.UtcNow.AddMinutes(10), // Code expires in 10 minutes
                    IsUsed = false
                };

                context.VerificationCodes.Add(codeEntry);
                await context.SaveChangesAsync();
                
                var smtpClient = new SmtpClient(_host)
                {
                    Port = _port,
                    Credentials = new NetworkCredential(_senderEmail, _emailPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail),
                    Subject = "Email Verification Code",
                    Body = $"Your verification code is: {verificationCode}",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
                smtpClient.Dispose();
                logger.LogInformation("Verification email sent to {0}", email);
                smtpClient.Dispose();
                
                await hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", $"verification code sent to {email}.");

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Error sending email: {0}", ex.Message);
                return false;
            }
        }
        
        public async Task<bool> ValidateVerificationCodeAsync(string email, string code)
        {
            var codeEntry = await context.VerificationCodes
                .Where(vc => vc.Email == email && vc.Code == code && !vc.IsUsed)
                .FirstOrDefaultAsync();

            if (codeEntry == null || codeEntry.ExpirationDate < DateTime.UtcNow)
            {
                return false; 
            }

            codeEntry.IsUsed = true;
            await context.SaveChangesAsync();
            await hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", $"verification code validated to {email}.");

            return true; // Valid code
        }
        
        public async Task<bool> VerifyEmailAsync(string email, string code)
        {
            var verificationEntry = await context.VerificationCodes
                .Where(vc => vc.Email == email && vc.Code == code && !vc.IsUsed)
                .FirstOrDefaultAsync();

            if (verificationEntry == null || verificationEntry.ExpirationDate < DateTime.UtcNow)
                return false; // Invalid or expired code

            // Mark code as used
            verificationEntry.IsUsed = true;

            // Find the user and update account status
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return false; // User not found

            user.AccountStatusId = 3; // Set account status to "Verified"
            await context.SaveChangesAsync();
    
            await hubContext.Clients.Group("Admins").SendAsync("ReceiveNotification", $"User {user.Email} has been verified.");

            return true; // Email successfully verified
        }
        
        public async Task<bool> SendEmailAsync(MailMessage mailMessage)
        {
            try
            {
                var smtpClient = new SmtpClient(_host)
                {
                    Port = _port,
                    Credentials = new NetworkCredential(_senderEmail, _emailPassword),
                    EnableSsl = true,
                };

                mailMessage.From = new MailAddress(_senderEmail);
                await smtpClient.SendMailAsync(mailMessage);
                logger.LogInformation("Email sent successfully to {0}", string.Join(", ", mailMessage.To.Select(t => t.Address)));
                smtpClient.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Error sending email: {0}", ex.Message);
                return false;
            }
        }

}