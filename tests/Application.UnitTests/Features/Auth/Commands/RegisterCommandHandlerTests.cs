using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Register;
using Domain.Entities;
using Moq;
using Xunit;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.UnitTests.Features.Auth.Commands;

public class RegisterCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(
            _userRepository.Object,
            _context.Object,
            _passwordHasher.Object,
            _jwtTokenGenerator.Object);
    }

    [Fact]
    public async Task Handle_WithNewEmail_CreatesUserAndReturnsToken()
    {
        _userRepository.Setup(r => r.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("Password1")).Returns("hashed");
        var expiresAt = DateTimeOffset.UtcNow.AddHours(8);
        _jwtTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>())).Returns(("token", expiresAt));

        var result = await _handler.Handle(
            new RegisterCommand { Email = "user@example.com", Password = "Password1" },
            CancellationToken.None);

        Assert.Equal("token", result.Token);
        Assert.Equal("user@example.com", result.Email);
        _userRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "user@example.com" && u.PasswordHash == "hashed"),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NormalizesEmailToLowercaseAndTrimmed()
    {
        _userRepository.Setup(r => r.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash(It.IsAny<string>())).Returns("hashed");
        _jwtTokenGenerator.Setup(g => g.GenerateToken(It.IsAny<User>())).Returns(("token", DateTimeOffset.UtcNow));

        var result = await _handler.Handle(
            new RegisterCommand { Email = " User@Example.com ", Password = "Password1" },
            CancellationToken.None);

        Assert.Equal("user@example.com", result.Email);
        _userRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "user@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmailAlreadyExists_ThrowsValidationException()
    {
        _userRepository.Setup(r => r.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(
            new RegisterCommand { Email = "user@example.com", Password = "Password1" },
            CancellationToken.None));
    }
}
