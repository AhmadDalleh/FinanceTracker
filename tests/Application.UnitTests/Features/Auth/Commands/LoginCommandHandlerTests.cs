using Application.Common.Exceptions;
using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Login;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _dateTimeProvider.Setup(p => p.UtcNow).Returns(DateTimeOffset.UtcNow);
        _handler = new LoginCommandHandler(
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _context.Object,
            _passwordHasher.Object,
            _jwtTokenGenerator.Object,
            _dateTimeProvider.Object);
    }

    private static User CreateUser() => new()
    {
        Id = Guid.NewGuid(),
        Email = "user@example.com",
        PasswordHash = "hashed-password"
    };

    [Fact]
    public async Task Handle_WithCorrectCredentials_ReturnsToken()
    {
        var user = CreateUser();
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("hashed-password", "Password1")).Returns(true);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        _jwtTokenGenerator.Setup(g => g.GenerateToken(user)).Returns(("token", expiresAt));

        var result = await _handler.Handle(
            new LoginCommand { Email = "user@example.com", Password = "Password1" },
            CancellationToken.None);

        Assert.Equal("token", result.Token);
        Assert.Equal(user.Id, result.UserId);
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ThrowsInvalidCredentialsException()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(
            new LoginCommand { Email = "unknown@example.com", Password = "Password1" },
            CancellationToken.None));
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ThrowsInvalidCredentialsException()
    {
        var user = CreateUser();
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("hashed-password", "WrongPassword")).Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() => _handler.Handle(
            new LoginCommand { Email = "user@example.com", Password = "WrongPassword" },
            CancellationToken.None));
    }
}
