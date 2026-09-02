using Application.Common.Interfaces;
using Application.Common.Security;
using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogoutCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationDbContext context,
        IDateTimeProvider dateTimeProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = SecureTokenHasher.Hash(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetValidTokenByHashAsync(tokenHash, cancellationToken);

        if (existingToken is null)
        {
            return;
        }

        existingToken.RevokedAt = _dateTimeProvider.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
