using Application.Common.Interfaces;

namespace Application.IntegrationTests;

public class TestCurrentUserService : ICurrentUserService
{
    public TestCurrentUserService(string? userId)
    {
        UserId = userId;
    }

    public string? UserId { get; }
}
