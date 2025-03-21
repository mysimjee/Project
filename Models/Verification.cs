using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace user_management.Models
{
    public class VerificationCode
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Code { get; set; }
        
        [Required]
        public DateTime ExpirationDate { get; set; }
        
        public bool IsUsed { get; set; } = false;
    } 
}

