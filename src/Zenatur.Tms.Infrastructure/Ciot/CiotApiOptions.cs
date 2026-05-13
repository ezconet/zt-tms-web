namespace Zenatur.Tms.Infrastructure.Ciot;

public sealed class CiotApiOptions
{
    public const string SectionName = "CiotApi";

    public string BaseUrl        { get; set; } = "";
    public string ApiKey         { get; set; } = "";
    public int    TimeoutSeconds { get; set; } = 30;
    public int    RetryCount     { get; set; } = 3;
}
