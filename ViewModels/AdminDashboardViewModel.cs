using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalRooms { get; set; }
    public int AvailableRooms { get; set; }
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ConfirmedReservations { get; set; }
    public int CancelledReservations { get; set; }
    public int PendingPayments { get; set; }
    public int DueOnStayPayments { get; set; }
    public int PaidReservations { get; set; }
    public decimal ProjectedRevenue { get; set; }
    public decimal CollectedRevenue { get; set; }
    public int UpcomingCheckIns { get; set; }
    public IReadOnlyList<Reservation> RecentReservations { get; set; } = [];
    public IReadOnlyList<DashboardRoomPerformanceViewModel> TopRooms { get; set; } = [];
}

public class DashboardRoomPerformanceViewModel
{
    public int RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string RoomType { get; set; } = string.Empty;
    public int BookingCount { get; set; }
    public decimal Revenue { get; set; }
}
