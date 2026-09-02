namespace Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken);
}
