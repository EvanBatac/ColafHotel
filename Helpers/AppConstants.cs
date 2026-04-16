namespace ColafHotel.Helpers;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Guest = "Guest";
}

public static class ReservationStatuses
{
    public const string Pending = "Pending";
    public const string Confirmed = "Confirmed";
    public const string Cancelled = "Cancelled";
}

public static class PaymentOptions
{
    public const string PayNow = "Pay in Advance";
    public const string PayOnStay = "Pay on Stay";
}

public static class RoomTypes
{
    public const string Single = "Single";
    public const string Double = "Double";
    public const string Suite = "Suite";
}

public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string Role = "Role";
    public const string FullName = "FullName";
    public const string ProfilePicturePath = "ProfilePicturePath";
}
