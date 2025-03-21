namespace user_management.Models;

public class VerifyEmailRequest
{
    public string Email { get; set; }
    public string Code { get; set; }
}
