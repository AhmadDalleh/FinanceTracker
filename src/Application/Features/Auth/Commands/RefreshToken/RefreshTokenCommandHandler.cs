using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var now = _dateTimeProvider.UtcNow;
        var tokenHash = SecureTokenHasher.Hash(request.RefreshToken);
        var existingToken = await _refreshTokenRepository.GetValidTokenByHashAsync(tokenHash, cancellationToken);

        if (existingToken is null || existingToken.ExpiresAt < now)
        {
            throw new InvalidTokenException();
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId, cancellationToken);
        if (user is null)
        {
            throw new InvalidTokenException();
        }

        existingToken.RevokedAt = now;

        var (newToken, rawNewToken) = RefreshTokenFactory.Create(user.Id, now);
        await _refreshTokenRepository.AddAsync(newToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto
        {
            Token = accessToken,
            ExpiresAt = expiresAt,
            RefreshToken = rawNewToken,
            UserId = user.Id,
            Email = user.Email
        };
    }
}
