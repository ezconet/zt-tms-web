namespace Zenatur.Tms.Application.Auth;

public interface ITokenValidator
{
    ExternalLoginPayload? Validate(string token);
}
