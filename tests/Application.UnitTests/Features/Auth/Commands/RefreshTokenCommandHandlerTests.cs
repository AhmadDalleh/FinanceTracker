using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Features.Auth.Commands.RefreshToken;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly RefreshTokenCommandHandler _handler;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public RefreshTokenCommandHandlerTests()
    {
        _dateTimeProvider.Setup(p => p.UtcNow).Returns(_now);
        _handler = new RefreshTokenCommandHandler(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _context.Object,
            _jwtTokenGenerator.Object,
            _dateTimeProvider.Object);
    }

    private static User CreateUser() => new() { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hash" };

    [Fact]
    public async Task Handle_WithValidToken_RotatesTokenAndReturnsNewAccessToken()
    {
        var user = CreateUser();
        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = SecureTokenHasher.Hash("raw-refresh-token"),
            ExpiresAt = _now.AddDays(1),
            CreatedAt = _now.AddDays(-6)
        };
        _refreshTokenRepository.Setup(r => r.GetValidTokenByHashAsync(existingToken.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);
        _userRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _jwtTokenGenerator.Setup(g => g.GenerateToken(user)).Returns(("new-access-token", _now.AddMinutes(15)));

        var result = await _handler.Handle(
            new RefreshTokenCommand { RefreshToken = "raw-refresh-token" },
            CancellationToken.None);

        Assert.Equal("new-access-token", result.Token);
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.NotEqual("raw-refresh-token", result.RefreshToken);
        Assert.Equal(_now, existingToken.RevokedAt);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ThrowsInvalidTokenException()
    {
        _refreshTokenRepository.Setup(r => r.GetValidTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        await Assert.ThrowsAsync<InvalidTokenException>(() => _handler.Handle(
            new RefreshTokenCommand { RefreshToken = "bad-token" },
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ThrowsInvalidTokenException()
    {
        var user = CreateUser();
        var existingToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = SecureTokenHasher.Hash("raw-refresh-token"),
            ExpiresAt = _now.AddDays(-1),
            CreatedAt = _now.AddDays(-8)
        };
        _refreshTokenRepository.Setup(r => r.GetValidTokenByHashAsync(existingToken.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(existingToken);

        await Assert.ThrowsAsync<InvalidTokenException>(() => _handler.Handle(
            new RefreshTokenCommand { RefreshToken = "raw-refresh-token" },
            CancellationToken.None));
    }
}
