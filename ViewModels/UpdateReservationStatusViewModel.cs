using System.ComponentModel.DataAnnotations;

namespace ColafHotel.ViewModels;

public class UpdateReservationStatusViewModel
{
    [Required]
    public int ReservationId { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
}
