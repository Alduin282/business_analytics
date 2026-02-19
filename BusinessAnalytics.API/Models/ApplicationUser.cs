using Microsoft.AspNetCore.Identity;

namespace BusinessAnalytics.API.Models;

public class ApplicationUser : IdentityUser
{
    /// <summary>IANA timezone ID, e.g. "Europe/Moscow". Set at registration.</summary>
    public string TimeZoneId { get; set; } = "UTC";
}
