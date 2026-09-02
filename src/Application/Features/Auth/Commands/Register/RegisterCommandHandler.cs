using Application.Common.Interfaces;
using Application.Common.Security;
using Domain.Entities;
using MediatR;
using ValidationException = Application.Common.Exceptions.ValidationException;

namespace Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResultDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<AuthResultDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var emailTaken = await _userRepository.ExistsByEmailAsync(email, cancellationToken);
        if (emailTaken)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "An account with this email already exists.")
            });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        await _userRepository.AddAsync(user, cancellationToken);

        var (refreshToken, rawRefreshToken) = RefreshTokenFactory.Create(user.Id, _dateTimeProvider.UtcNow);
        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        var (token, expiresAt) = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResultDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            RefreshToken = rawRefreshToken,
            UserId = user.Id,
            Email = user.Email
        };
    }
}
