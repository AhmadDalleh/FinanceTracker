using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetValidTokenByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.RevokedAt == null, cancellationToken);

    public async Task AddAsync(RefreshToken token, CancellationToken cancellationToken) =>
        await _context.RefreshTokens.AddAsync(token, cancellationToken);
}
