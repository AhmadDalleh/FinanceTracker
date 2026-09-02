using Application.Common.Interfaces;
using Application.Features.Auth.Commands.ForgotPassword;
using Domain.Entities;
using Moq;
using Xunit;

namespace Application.UnitTests.Features.Auth.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IPasswordResetTokenRepository> _tokenRepository = new();
    private readonly Mock<IApplicationDbContext> _context = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _dateTimeProvider.Setup(p => p.UtcNow).Returns(DateTimeOffset.UtcNow);
        _handler = new ForgotPasswordCommandHandler(
            _userRepository.Object,
            _tokenRepository.Object,
            _context.Object,
            _emailSender.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task Handle_WithRegisteredEmail_CreatesTokenAndSendsEmail()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "user@example.com", PasswordHash = "hashed" };
        _userRepository.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);

        await _handler.Handle(new ForgotPasswordCommand { Email = "user@example.com" }, CancellationToken.None);

        _tokenRepository.Verify(r => r.AddAsync(
            It.Is<PasswordResetToken>(t => t.UserId == user.Id && !string.IsNullOrWhiteSpace(t.TokenHash)),
            It.IsAny<CancellationToken>()), Times.Once);
        _context.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(e => e.SendPasswordResetEmailAsync(
            "user@example.com",
            It.Is<string>(token => !string.IsNullOrWhiteSpace(token)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_DoesNotCreateTokenOrSendEmail()
    {
        _userRepository.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await _handler.Handle(new ForgotPasswordCommand { Email = "unknown@example.com" }, CancellationToken.None);

        _tokenRepository.Verify(r => r.AddAsync(It.IsAny<PasswordResetToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendPasswordResetEmailAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
