namespace Zenatur.Tms.Infrastructure.Persistence.Entities;

public class TmsUser
{
    public int     Id         { get; private set; }
    public string  ExternalId { get; private set; } = default!;
    public string  Email      { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private TmsUser() { }

    public static TmsUser Create(string externalId, string email) => new()
    {
        ExternalId = externalId,
        Email      = email,
        CreatedAt  = DateTime.UtcNow
    };
}
