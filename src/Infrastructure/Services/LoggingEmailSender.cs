using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

// No email provider configured yet — logs instead. Swap for a real IEmailSender (SendGrid, SES, etc.) in DependencyInjection.cs when one is chosen.
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;
    private readonly IConfiguration _configuration;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken)
    {
        var clientBaseUrl = _configuration["App:ClientBaseUrl"]?.TrimEnd('/');
        var resetLink = $"{clientBaseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

        _logger.LogInformation(
            "Password reset requested for {Email}. Reset link (no email provider configured, logging instead): {ResetLink}",
            email,
            resetLink);

        return Task.CompletedTask;
    }
}
