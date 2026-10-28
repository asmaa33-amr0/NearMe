using System.ComponentModel.DataAnnotations;

namespace NearMe.DTOs
{
    public enum AccountType
    {
        User = 1,
        Provider = 2
    }

    public class RegisterDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, EnumDataType(typeof(AccountType))]
        public AccountType AccountType { get; set; }
    }

    public class LoginDto
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}