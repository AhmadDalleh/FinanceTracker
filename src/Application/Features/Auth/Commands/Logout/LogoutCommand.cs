using MediatR;

namespace Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest
{
    public required string RefreshToken { get; init; }
}
