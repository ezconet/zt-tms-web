namespace Zenatur.Tms.Infrastructure.LegacyBridge;

public sealed class LegacyBridgeOptions
{
    public const string SectionName = "LegacyBridge";

    public string BaseUrl        { get; set; } = "";
    public string ApiKey         { get; set; } = "";
    public int    TimeoutSeconds { get; set; } = 5; // agressivo — fonte secundária
}
