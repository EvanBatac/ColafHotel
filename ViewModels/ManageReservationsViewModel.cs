using ColafHotel.Models;

namespace ColafHotel.ViewModels;

public class ManageReservationsViewModel
{
    public IReadOnlyList<Reservation> Reservations { get; set; } = [];
}
