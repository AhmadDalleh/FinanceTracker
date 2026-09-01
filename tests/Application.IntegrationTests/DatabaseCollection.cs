using Xunit;

namespace Application.IntegrationTests;

[CollectionDefinition(Name)]
public class DatabaseCollection : ICollectionFixture<PostgresContainerFixture>
{
    public const string Name = "Database collection";
}
