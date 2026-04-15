using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ColafHotel.ViewModels;

public class EditProfileViewModel
{
    [Required, StringLength(120)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6)]
    [Display(Name = "New password")]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword))]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    public string? ConfirmNewPassword { get; set; }

    [Display(Name = "Profile picture")]
    public IFormFile? ProfilePicture { get; set; }

    public string? ExistingProfilePicturePath { get; set; }
}
