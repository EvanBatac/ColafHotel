using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColafHotel.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }

        [Required, StringLength(120)]
        public string RoomName { get; set; } = string.Empty;

        [Required, StringLength(40)]
        public string RoomType { get; set; } = "Single"; // Single, Double, Suite

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, 1000000)]
        public decimal PricePerNight { get; set; }

        [StringLength(255)]
        public string? ImagePath { get; set; }

        public bool IsAvailable { get; set; } = true;

        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}
