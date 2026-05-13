namespace Zenatur.Tms.Application.Auth;

public interface IUserService
{
    Task EnsureExistsAsync(ExternalLoginPayload payload, CancellationToken ct = default);
}
