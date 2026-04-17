using System.ComponentModel.DataAnnotations;

namespace ColafHotel.ViewModels;

public class UpdateReservationStatusViewModel
{
    [Required]
    public int ReservationId { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;

    [Required]
    public string PaymentStatus { get; set; } = string.Empty;

    [StringLength(100)]
    public string? PaymentReference { get; set; }

    [StringLength(255)]
    public string? PaymentNote { get; set; }
}
