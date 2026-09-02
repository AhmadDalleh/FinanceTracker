namespace Application.Features.Auth;

public class AuthResultDto
{
    public required string Token { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}
