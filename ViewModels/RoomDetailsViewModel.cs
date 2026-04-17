using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class RoomDetailsViewModel
{
    public Room Room { get; set; } = new();
    public DateTime CalendarMonth { get; set; }
    public IReadOnlyList<RoomAvailabilityDayViewModel> AvailabilityDays { get; set; } = [];
    public IReadOnlyList<DateTime> UpcomingBookedDates { get; set; } = [];
}
