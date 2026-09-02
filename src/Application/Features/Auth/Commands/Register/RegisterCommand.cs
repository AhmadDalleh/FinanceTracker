using MediatR;

namespace Application.Features.Auth.Commands.Register;

public record RegisterCommand : IRequest<AuthResultDto>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
