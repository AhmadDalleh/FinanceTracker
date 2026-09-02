using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Features.Auth.Commands.Logout;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class LogoutCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly LogoutCommandHandler _handler;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public LogoutCommandHandlerTests()
    {
        _dateTimeProvider.Setup(p => p.UtcNow).Returns(_now);
        _handler = new LogoutCommandHandler(_refreshTokenRepository.Object, _context.Object, _dateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WithValidToken_RevokesToken()
    {
        var token = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = SecureTokenHasher.Hash("raw-token"),
            ExpiresAt = _now.AddDays(1),
            CreatedAt = _now.AddDays(-1)
        };
        _refreshTokenRepository.Setup(r => r.GetValidTokenByHashAsync(token.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(token);

        await _handler.Handle(new LogoutCommand { RefreshToken = "raw-token" }, CancellationToken.None);

        Assert.Equal(_now, token.RevokedAt);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_DoesNotThrowOrSave()
    {
        _refreshTokenRepository.Setup(r => r.GetValidTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((RefreshToken?)null);

        await _handler.Handle(new LogoutCommand { RefreshToken = "unknown-token" }, CancellationToken.None);

        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
