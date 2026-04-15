using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class RoomIndexViewModel
{
    public string? SearchTerm { get; set; }
    public string? RoomType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public IReadOnlyList<Room> Rooms { get; set; } = [];
}
