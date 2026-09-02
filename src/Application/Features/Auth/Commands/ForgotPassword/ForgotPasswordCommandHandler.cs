using Application.Common.Interfaces;
using Application.Common.Security;
using Domain.Entities;
using MediatR;

namespace Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private const int TokenValidityHours = 1;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _tokenRepository;
    private readonly IApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetTokenRepository tokenRepository,
        IApplicationDbContext context,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _context = context;
        _emailSender = emailSender;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            return;
        }

        var rawToken = SecureTokenGenerator.GenerateUrlSafeToken();
        var token = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = SecureTokenHasher.Hash(rawToken),
            ExpiresAt = _dateTimeProvider.UtcNow.AddHours(TokenValidityHours),
            CreatedAt = _dateTimeProvider.UtcNow
        };

        await _tokenRepository.AddAsync(token, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _emailSender.SendPasswordResetEmailAsync(user.Email, rawToken, cancellationToken);
    }
}
