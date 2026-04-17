using System.Security.Claims;
using ColafHotel.Data;
using ColafHotel.Helpers;
using ColafHotel.Models;
using ColafHotel.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ColafHotel.Controllers;

[Authorize]
public class ReservationController(AppDbContext context) : Controller
{
    [Authorize(Roles = Roles.Guest)]
    [HttpGet]
    public async Task<IActionResult> Book(int roomId)
    {
        var room = await context.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && r.IsAvailable);
        if (room is null)
        {
            return NotFound();
        }

        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        return View(new BookRoomViewModel
        {
            RoomId = room.RoomId,
            RoomName = room.RoomName,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            GuestName = user.FullName,
            PaymentOption = PaymentOptions.PayOnStay,
            PaymentStatus = PaymentStatuses.DueOnStay,
            TotalPrice = room.PricePerNight
        });
    }

    [Authorize(Roles = Roles.Guest)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(BookRoomViewModel model)
    {
        var room = await context.Rooms.FirstOrDefaultAsync(r => r.RoomId == model.RoomId && r.IsAvailable);
        var user = await GetCurrentUserAsync();

        if (room is null || user is null)
        {
            return NotFound();
        }

        model.RoomName = room.RoomName;
        model.RoomType = room.RoomType;
        model.PricePerNight = room.PricePerNight;
        model.GuestName = user.FullName;
        model.TotalPrice = CalculateTotalPrice(room.PricePerNight, model.CheckInDate, model.CheckOutDate);

        if (await HasOverlappingReservationAsync(room.RoomId, model.CheckInDate, model.CheckOutDate))
        {
            ModelState.AddModelError(string.Empty, "That room is already booked for the selected dates.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var reservation = new Reservation
        {
            UserId = user.UserId,
            RoomId = room.RoomId,
            CheckInDate = model.CheckInDate.Date,
            CheckOutDate = model.CheckOutDate.Date,
            TotalPrice = model.TotalPrice,
            PaymentOption = model.PaymentOption,
            PaymentStatus = model.PaymentOption == PaymentOptions.PayNow
                ? PaymentStatuses.Pending
                : PaymentStatuses.DueOnStay,
            PaymentUpdatedAt = DateTime.UtcNow,
            Status = ReservationStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        };

        context.Reservations.Add(reservation);
        await context.SaveChangesAsync();

        context.ReservationPaymentLogs.Add(new ReservationPaymentLog
        {
            ReservationId = reservation.ReservationId,
            PaymentStatus = reservation.PaymentStatus,
            Message = reservation.PaymentOption == PaymentOptions.PayNow
                ? "Guest selected advance payment. Waiting for payment confirmation."
                : "Guest selected to pay on stay. Payment will be collected during check-in.",
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation submitted successfully.";
        return RedirectToAction(nameof(MyBookings));
    }

    [Authorize(Roles = Roles.Guest)]
    public async Task<IActionResult> MyBookings()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var reservations = await context.Reservations
            .Include(r => r.Room)
            .Include(r => r.PaymentLogs.OrderByDescending(log => log.CreatedAt))
            .Where(r => r.UserId == user.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return View(reservations);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Manage()
    {
        var viewModel = new ManageReservationsViewModel
        {
            Reservations = await context.Reservations
                .Include(r => r.User)
                .Include(r => r.Room)
                .Include(r => r.PaymentLogs.OrderByDescending(log => log.CreatedAt))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(),
            PaymentStatuses = GetAllowedPaymentStatuses()
        };

        return View(viewModel);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Manage(UpdateReservationStatusViewModel model)
    {
        var reservation = await context.Reservations.FindAsync(model.ReservationId);
        if (reservation is null)
        {
            return NotFound();
        }

        if (!new[]
            {
                ReservationStatuses.Pending,
                ReservationStatuses.Confirmed,
                ReservationStatuses.Cancelled
            }.Contains(model.Status))
        {
            TempData["ErrorMessage"] = "Invalid reservation status.";
            return RedirectToAction(nameof(Manage));
        }

        if (!GetAllowedPaymentStatuses().Contains(model.PaymentStatus))
        {
            TempData["ErrorMessage"] = "Invalid payment status.";
            return RedirectToAction(nameof(Manage));
        }

        var paymentStatusChanged = !string.Equals(
            reservation.PaymentStatus,
            model.PaymentStatus,
            StringComparison.Ordinal);
        var paymentReferenceChanged = !string.Equals(
            reservation.PaymentReference ?? string.Empty,
            model.PaymentReference?.Trim() ?? string.Empty,
            StringComparison.Ordinal);
        var paymentNote = model.PaymentNote?.Trim();

        reservation.Status = model.Status;
        reservation.PaymentStatus = model.PaymentStatus;
        reservation.PaymentReference = string.IsNullOrWhiteSpace(model.PaymentReference)
            ? null
            : model.PaymentReference.Trim();
        if (paymentStatusChanged || paymentReferenceChanged || !string.IsNullOrWhiteSpace(paymentNote))
        {
            reservation.PaymentUpdatedAt = DateTime.UtcNow;
            context.ReservationPaymentLogs.Add(new ReservationPaymentLog
            {
                ReservationId = reservation.ReservationId,
                PaymentStatus = reservation.PaymentStatus,
                Message = BuildPaymentLogMessage(reservation.PaymentStatus, reservation.PaymentReference, paymentNote),
                CreatedAt = reservation.PaymentUpdatedAt.Value
            });
        }

        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation status updated.";
        return RedirectToAction(nameof(Manage));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == id);
        if (reservation is null)
        {
            return NotFound();
        }

        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole(Roles.Admin);
        if (!isAdmin && reservation.UserId != currentUserId)
        {
            return Forbid();
        }

        reservation.Status = ReservationStatuses.Cancelled;
        await context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Reservation cancelled.";
        return isAdmin
            ? RedirectToAction(nameof(Manage))
            : RedirectToAction(nameof(MyBookings));
    }

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        return userId > 0
            ? await context.Users.FirstOrDefaultAsync(u => u.UserId == userId)
            : null;
    }

    private int GetCurrentUserId()
    {
        var sessionUserId = HttpContext.Session.GetInt32(SessionKeys.UserId);
        if (sessionUserId.HasValue)
        {
            return sessionUserId.Value;
        }

        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : 0;
    }

    private async Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkInDate, DateTime checkOutDate)
    {
        return await context.Reservations.AnyAsync(r =>
            r.RoomId == roomId &&
            r.Status != ReservationStatuses.Cancelled &&
            checkInDate.Date < r.CheckOutDate.Date &&
            checkOutDate.Date > r.CheckInDate.Date);
    }

    private static decimal CalculateTotalPrice(decimal pricePerNight, DateTime checkInDate, DateTime checkOutDate)
    {
        var nights = Math.Max((checkOutDate.Date - checkInDate.Date).Days, 1);
        return nights * pricePerNight;
    }

    private static IReadOnlyList<string> GetAllowedPaymentStatuses() =>
    [
        PaymentStatuses.Pending,
        PaymentStatuses.DueOnStay,
        PaymentStatuses.Paid,
        PaymentStatuses.Refunded
    ];

    private static string BuildPaymentLogMessage(string paymentStatus, string? paymentReference, string? paymentNote)
    {
        var message = $"Payment status updated to {paymentStatus}.";
        if (!string.IsNullOrWhiteSpace(paymentReference))
        {
            message += $" Reference: {paymentReference}.";
        }

        if (!string.IsNullOrWhiteSpace(paymentNote))
        {
            message += $" Note: {paymentNote}.";
        }

        return message;
    }
}
