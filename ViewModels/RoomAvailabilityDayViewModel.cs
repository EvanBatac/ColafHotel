namespace ColafHotel.ViewModels;

public class RoomAvailabilityDayViewModel
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsPast { get; set; }
    public bool IsUnavailable { get; set; }
}
