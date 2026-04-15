using System.ComponentModel.DataAnnotations;
using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class BookRoomViewModel : IValidatableObject
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public string GuestName { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Check-in date")]
    public DateTime CheckInDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Check-out date")]
    public DateTime CheckOutDate { get; set; } = DateTime.Today.AddDays(2);

    public decimal TotalPrice { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CheckInDate.Date < DateTime.Today)
        {
            yield return new ValidationResult(
                "Check-in date cannot be in the past.",
                new[] { nameof(CheckInDate) });
        }

        if (CheckOutDate.Date <= CheckInDate.Date)
        {
            yield return new ValidationResult(
                "Check-out date must be later than check-in date.",
                new[] { nameof(CheckOutDate) });
        }
    }
}
