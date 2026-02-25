namespace BusinessAnalytics.API.Models.DTOs;

public record AnalyticsPoint(string Label, decimal Value, bool IsPartial = false);
