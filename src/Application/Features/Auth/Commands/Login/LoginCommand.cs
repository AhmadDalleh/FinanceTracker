using MediatR;

namespace Application.Features.Auth.Commands.Login;

public record LoginCommand : IRequest<AuthResultDto>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
