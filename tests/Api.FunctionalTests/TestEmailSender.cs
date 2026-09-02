using Application.Common.Interfaces;

namespace Api.FunctionalTests;

public class TestEmailSender : IEmailSender
{
    public string? LastEmail { get; private set; }
    public string? LastResetToken { get; private set; }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken)
    {
        LastEmail = email;
        LastResetToken = resetToken;
        return Task.CompletedTask;
    }

    public void Reset()
    {
        LastEmail = null;
        LastResetToken = null;
    }
}
