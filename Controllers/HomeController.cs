using ColafHotel.Data;
using ColafHotel.Helpers;
using ColafHotel.Models;
using ColafHotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ColafHotel.Controllers;

public class HomeController(AppDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true && User.IsInRole(Roles.Admin))
        {
            return RedirectToAction(nameof(Dashboard));
        }

        var featuredRooms = await context.Rooms
            .Where(room => room.IsAvailable)
            .Take(12)
            .ToListAsync();

        var viewModel = new HomeIndexViewModel
        {
            FeaturedRooms = featuredRooms
                .OrderBy(room => room.PricePerNight)
                .Take(3)
                .ToList()
        };

        return View(viewModel);
    }

    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Dashboard()
    {
        var reservations = await context.Reservations
            .Include(reservation => reservation.Room)
            .Include(reservation => reservation.User)
            .OrderByDescending(reservation => reservation.CreatedAt)
            .ToListAsync();

        var rooms = await context.Rooms.ToListAsync();
        var today = DateTime.Today;
        var upcomingWindow = today.AddDays(7);
        var activeReservations = reservations
            .Where(reservation => reservation.Status != ReservationStatuses.Cancelled)
            .ToList();

        var viewModel = new AdminDashboardViewModel
        {
            TotalRooms = rooms.Count,
            AvailableRooms = rooms.Count(room => room.IsAvailable),
            TotalReservations = reservations.Count,
            PendingReservations = reservations.Count(reservation => reservation.Status == ReservationStatuses.Pending),
            ConfirmedReservations = reservations.Count(reservation => reservation.Status == ReservationStatuses.Confirmed),
            CancelledReservations = reservations.Count(reservation => reservation.Status == ReservationStatuses.Cancelled),
            PendingPayments = reservations.Count(reservation => reservation.PaymentStatus == PaymentStatuses.Pending),
            DueOnStayPayments = reservations.Count(reservation => reservation.PaymentStatus == PaymentStatuses.DueOnStay),
            PaidReservations = reservations.Count(reservation => reservation.PaymentStatus == PaymentStatuses.Paid),
            ProjectedRevenue = activeReservations.Sum(reservation => reservation.TotalPrice),
            CollectedRevenue = reservations
                .Where(reservation => reservation.PaymentStatus == PaymentStatuses.Paid)
                .Sum(reservation => reservation.TotalPrice),
            UpcomingCheckIns = reservations.Count(reservation =>
                reservation.Status == ReservationStatuses.Confirmed &&
                reservation.CheckInDate.Date >= today &&
                reservation.CheckInDate.Date <= upcomingWindow),
            RecentReservations = reservations
                .Take(6)
                .ToList(),
            TopRooms = activeReservations
                .Where(reservation => reservation.Room is not null)
                .GroupBy(reservation => new
                {
                    reservation.RoomId,
                    reservation.Room!.RoomName,
                    reservation.Room.RoomType
                })
                .Select(group => new DashboardRoomPerformanceViewModel
                {
                    RoomId = group.Key.RoomId,
                    RoomName = group.Key.RoomName,
                    RoomType = group.Key.RoomType,
                    BookingCount = group.Count(),
                    Revenue = group.Sum(reservation => reservation.TotalPrice)
                })
                .OrderByDescending(room => room.BookingCount)
                .ThenByDescending(room => room.Revenue)
                .Take(5)
                .ToList()
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
