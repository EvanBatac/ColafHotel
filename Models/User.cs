using System.ComponentModel.DataAnnotations;

namespace ColafHotel.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(160)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Role { get; set; } = "Guest"; // Admin or Guest

        [StringLength(255)]
        public string? ProfilePicturePath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
