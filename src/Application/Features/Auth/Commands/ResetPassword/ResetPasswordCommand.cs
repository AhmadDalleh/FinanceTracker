using MediatR;

namespace Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}
