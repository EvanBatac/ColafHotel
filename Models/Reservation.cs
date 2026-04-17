using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColafHotel.Helpers;

namespace ColafHotel.Models
{
    public class Reservation : IValidatableObject
    {
        [Key]
        public int ReservationId { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required]
        public int RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled

        [Required]
        [StringLength(50)]
        public string PaymentOption { get; set; } = PaymentOptions.PayOnStay;

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = PaymentStatuses.DueOnStay;

        [StringLength(100)]
        public string? PaymentReference { get; set; }

        public DateTime? PaymentUpdatedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ReservationPaymentLog> PaymentLogs { get; set; } = new List<ReservationPaymentLog>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (CheckOutDate.Date <= CheckInDate.Date)
            {
                yield return new ValidationResult(
                    "Check-out date must be later than check-in date.",
                    new[] { nameof(CheckOutDate) });
            }

            if (PaymentStatus is not PaymentStatuses.Pending
                and not PaymentStatuses.DueOnStay
                and not PaymentStatuses.Paid
                and not PaymentStatuses.Refunded)
            {
                yield return new ValidationResult(
                    "Please choose a valid payment status.",
                    new[] { nameof(PaymentStatus) });
            }
        }
    }
}
