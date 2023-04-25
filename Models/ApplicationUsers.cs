using System.ComponentModel.DataAnnotations;

namespace Sciencetopia.Models
{
    public class ApplicationUser
    {
        [Key]
        public int? Id { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? PasswordHash { get; set; }
    }
}
