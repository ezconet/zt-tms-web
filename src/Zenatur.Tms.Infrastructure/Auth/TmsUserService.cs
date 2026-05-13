using Microsoft.EntityFrameworkCore;
using Zenatur.Tms.Application.Auth;
using Zenatur.Tms.Infrastructure.Persistence;
using Zenatur.Tms.Infrastructure.Persistence.Entities;

namespace Zenatur.Tms.Infrastructure.Auth;

public sealed class TmsUserService : IUserService
{
    private readonly TmsDbContext _db;

    public TmsUserService(TmsDbContext db) => _db = db;

    public async Task EnsureExistsAsync(ExternalLoginPayload payload, CancellationToken ct = default)
    {
        var exists = await _db.Users.AnyAsync(u => u.ExternalId == payload.ExternalId, ct);
        if (!exists)
        {
            _db.Users.Add(TmsUser.Create(payload.ExternalId, payload.Email));
            await _db.SaveChangesAsync(ct);
        }
    }
}
