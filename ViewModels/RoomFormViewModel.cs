using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ColafHotel.ViewModels;

public class RoomFormViewModel
{
    public int RoomId { get; set; }

    [Required, StringLength(120)]
    [Display(Name = "Room name")]
    public string RoomName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Room type")]
    public string RoomType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Price per night")]
    [Range(0.01, 1000000)]
    [DataType(DataType.Currency)]
    public decimal PricePerNight { get; set; }

    [Display(Name = "Room image")]
    public IFormFile? ImageFile { get; set; }

    [Display(Name = "Available for booking")]
    public bool IsAvailable { get; set; } = true;

    public string? ExistingImagePath { get; set; }
}
