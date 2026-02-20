namespace BusinessAnalytics.API.Models.DTOs;

public record AnalyticsPoint(string Label, decimal TotalAmount, bool IsPartial = false);
