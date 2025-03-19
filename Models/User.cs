using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AutoMapper;

namespace user_management.Models
{
    public interface IUser
    {
        int UserId { get; set; }
        string Username { get; set; }
        string Email { get; set; }
        string PhoneNumber { get; set; }
        string ProfileImgPath { get; set; }
        string RecoveryEmail { get; set; }
        string Country { get; set; }
        string State { get; set; }
        string ZipCode { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
    }

    public class User : IUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; } = 0;

        [StringLength(50, MinimumLength = 5)]
        public string Username { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 5)]
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(15, MinimumLength = 5)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string ProfileImgPath { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 5)]
        public string RecoveryEmail { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 2)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50, MinimumLength = 2)]
        public string State { get; set; } = string.Empty;

        [StringLength(10, MinimumLength = 5)]
        public string ZipCode { get; set; } = string.Empty;

        [ForeignKey("RoleId")]
        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AccountStatusId")]
        public int AccountStatusId { get; set; }
        public virtual AccountStatus AccountStatus { get; set; }
    
        public List<LoginHistory> LoginHistory { get; set; } = new List<LoginHistory>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; } = 0;

        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class AccountStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountStatusId { get; set; } = 0;

        [MaxLength(50)]
        public string StatusName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class LoginHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginId { get; set; } = 0;

        [ForeignKey("UserId")]
        public int UserId { get; set; } = 0;

        public DateTime LoginTimestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string? IpAddress { get; set; } = string.Empty;

        public string? DeviceInfo { get; set; } = string.Empty;

        public int FailedAttempts { get; set; } = 0;

        public bool LoginSuccessful { get; set; } = false;
    }

    public class RolePermission
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RolePermissionId { get; set; } = 0;

        [ForeignKey("RoleId")]
        public int RoleId { get; set; } = 0;

        [ForeignKey("PermissionId")]
        public int PermissionId { get; set; } = 0;

        [MaxLength(50)]
        public string PermissionName { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ContentCreator : User, IUser
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [CustomValidation(typeof(ContentCreator), nameof(ValidateAge))]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string Nrc { get; set; } = string.Empty;

        public string Biography { get; set; } = string.Empty;

        public string SocialLinks { get; set; } = string.Empty;

        public string PortfolioUrl { get; set; } = string.Empty;

        public static ValidationResult ValidateAge(DateTime dateOfBirth, ValidationContext context)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-age)) age--;

            return age >= 18 ? ValidationResult.Success : new ValidationResult("Content creator must be at least 18 years old.");
        }
    }

    public class ProductionCompany : User, IUser
    {
        [MaxLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        public string CompanyWebsite { get; set; } = string.Empty;

        public DateTime FoundingDate { get; set; } = DateTime.UtcNow;

        public string Biography { get; set; } = string.Empty;

        public string SocialLinks { get; set; } = string.Empty;

        public string PortfolioUrl { get; set; } = string.Empty;
    }

    public class Viewer : User, IUser
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [CustomValidation(typeof(Viewer), nameof(ValidateAge))]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        public static ValidationResult ValidateAge(DateTime dateOfBirth, ValidationContext context)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-age)) age--;

            return age >= 18 ? ValidationResult.Success : new ValidationResult("Viewer must be at least 18 years old.");
        }
    }

    public class PlatformAdmin : User, IUser
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [CustomValidation(typeof(PlatformAdmin), nameof(ValidateAge))]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;

        public DateTime? EmploymentDate { get; set; } = null;

        [MaxLength(50)]
        public string JobRole { get; set; } = string.Empty;

        public static ValidationResult ValidateAge(DateTime dateOfBirth, ValidationContext context)
        {
            var age = DateTime.Today.Year - dateOfBirth.Year;
            if (dateOfBirth > DateTime.Today.AddYears(-age)) age--;

            return age >= 18 ? ValidationResult.Success : new ValidationResult("Platform admin must be at least 18 years old.");
        }
    }

    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, ContentCreator>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).FirstName : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).LastName : null))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).DateOfBirth : default(DateTime)))
                .ForMember(dest => dest.Nrc, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).Nrc : null))
                .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).Biography : null))
                .ForMember(dest => dest.SocialLinks, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).SocialLinks : null))
                .ForMember(dest => dest.PortfolioUrl, opt => opt.MapFrom(src => (src is ContentCreator) ? ((ContentCreator)src).PortfolioUrl : null));

            CreateMap<ContentCreator, User>();

            CreateMap<User, ProductionCompany>()
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => (src is ProductionCompany) ? ((ProductionCompany)src).CompanyName : null))
                .ForMember(dest => dest.CompanyWebsite, opt => opt.MapFrom(src => (src is ProductionCompany) ? ((ProductionCompany)src).CompanyWebsite : null))
                .ForMember(dest => dest.FoundingDate, opt => opt.MapFrom(src => (src is ProductionCompany) ? ((ProductionCompany)src).FoundingDate : default(DateTime)))
                .ForMember(dest => dest.Biography, opt => opt.MapFrom(src => (src is ProductionCompany) ? ((ProductionCompany)src).Biography : null))
                .ForMember(dest => dest.SocialLinks, opt => opt.MapFrom(src => (src is ProductionCompany) ? ((ProductionCompany)src).SocialLinks : null))
                .ForMember(dest => dest.PortfolioUrl, opt => opt.MapFrom(src => (src is ProductionCompany) ? ((ProductionCompany)src).PortfolioUrl : null));

            CreateMap<ProductionCompany, User>();

            CreateMap<User, Viewer>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => (src is Viewer) ? ((Viewer)src).FirstName : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => (src is Viewer) ? ((Viewer)src).LastName : null))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => (src is Viewer) ? ((Viewer)src).DateOfBirth : default(DateTime)));

            CreateMap<Viewer, User>();

            CreateMap<User, PlatformAdmin>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => (src is PlatformAdmin) ? ((PlatformAdmin)src).FirstName : null))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => (src is PlatformAdmin) ? ((PlatformAdmin)src).LastName : null))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => (src is PlatformAdmin) ? ((PlatformAdmin)src).DateOfBirth : default(DateTime)))
                .ForMember(dest => dest.EmploymentDate, opt => opt.MapFrom(src => (src is PlatformAdmin) ? ((PlatformAdmin)src).EmploymentDate : null))
                .ForMember(dest => dest.JobRole, opt => opt.MapFrom(src => (src is PlatformAdmin) ? ((PlatformAdmin)src).JobRole : null));

            CreateMap<PlatformAdmin, User>();
        }
    }
}
