using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Common.Security;
using Application.Features.Auth.Commands.ResetPassword;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly ResetPasswordCommandHandler _handler;
    private readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    public ResetPasswordCommandHandlerTests()
    {
        _dateTimeProvider.Setup(p => p.UtcNow).Returns(_now);
        _handler = new ResetPasswordCommandHandler(
            _userRepository.Object,
            _tokenRepository.Object,
            _context.Object,
            _passwordHasher.Object,
            _dateTimeProvider.Object);
    }

    private static User CreateUser() => new() { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "old-hash" };

    [Fact]
    public async Task Handle_WithValidToken_UpdatesPasswordAndMarksTokenUsed()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = PasswordResetTokenHasher.Hash("raw-token"),
            ExpiresAt = _now.AddHours(1),
            CreatedAt = _now
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenRepository.Setup(r => r.GetValidTokenByHashAsync(token.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(token);
        _passwordHasher.Setup(h => h.Hash("NewPassword1")).Returns("new-hash");

        await _handler.Handle(
            new ResetPasswordCommand { Email = "user@example.com", Token = "raw-token", NewPassword = "NewPassword1" },
            CancellationToken.None);

        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Equal(_now, token.UsedAt);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsInvalidTokenException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        _tokenRepository.Setup(r => r.GetValidTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((PasswordResetToken?)null);

        await Assert.ThrowsAsync<InvalidTokenException>(() => _handler.Handle(
            new ResetPasswordCommand { Email = "unknown@example.com", Token = "raw-token", NewPassword = "NewPassword1" },
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithNoMatchingToken_ThrowsInvalidTokenException()
    {
        var user = CreateUser();
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenRepository.Setup(r => r.GetValidTokenByHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((PasswordResetToken?)null);

        await Assert.ThrowsAsync<InvalidTokenException>(() => _handler.Handle(
            new ResetPasswordCommand { Email = "user@example.com", Token = "bad-token", NewPassword = "NewPassword1" },
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ThrowsInvalidTokenException()
    {
        var user = CreateUser();
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = PasswordResetTokenHasher.Hash("raw-token"),
            ExpiresAt = _now.AddHours(-1),
            CreatedAt = _now.AddHours(-2)
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenRepository.Setup(r => r.GetValidTokenByHashAsync(token.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(token);

        await Assert.ThrowsAsync<InvalidTokenException>(() => _handler.Handle(
            new ResetPasswordCommand { Email = "user@example.com", Token = "raw-token", NewPassword = "NewPassword1" },
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithTokenBelongingToDifferentUser_ThrowsInvalidTokenException()
    {
        var user = CreateUser();
        var otherUserToken = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            TokenHash = PasswordResetTokenHasher.Hash("raw-token"),
            ExpiresAt = _now.AddHours(1),
            CreatedAt = _now
        };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _tokenRepository.Setup(r => r.GetValidTokenByHashAsync(otherUserToken.TokenHash, It.IsAny<CancellationToken>())).ReturnsAsync(otherUserToken);

        await Assert.ThrowsAsync<InvalidTokenException>(() => _handler.Handle(
            new ResetPasswordCommand { Email = "user@example.com", Token = "raw-token", NewPassword = "NewPassword1" },
            CancellationToken.None));
    }
}
