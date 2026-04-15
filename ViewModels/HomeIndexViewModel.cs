using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class HomeIndexViewModel
{
    public IReadOnlyList<Room> FeaturedRooms { get; set; } = [];
}
