using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PasswordResetToken?> GetValidTokenByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.UsedAt == null, cancellationToken);

    public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken) =>
        await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
}
