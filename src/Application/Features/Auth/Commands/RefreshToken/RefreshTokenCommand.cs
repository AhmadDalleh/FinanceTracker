using MediatR;

namespace Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommand : IRequest<AuthResultDto>
{
    public required string RefreshToken { get; init; }
}
