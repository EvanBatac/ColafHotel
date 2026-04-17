using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class ManageReservationsViewModel
{
    public IReadOnlyList<Reservation> Reservations { get; set; } = [];
    public IReadOnlyList<string> PaymentStatuses { get; set; } = [];
}
