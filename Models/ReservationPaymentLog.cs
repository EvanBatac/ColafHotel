using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ColafHotel.Models
{
    public class ReservationPaymentLog
    {
        [Key]
        public int ReservationPaymentLogId { get; set; }

        [Required]
        public int ReservationId { get; set; }

        [ForeignKey("ReservationId")]
        public virtual Reservation? Reservation { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
