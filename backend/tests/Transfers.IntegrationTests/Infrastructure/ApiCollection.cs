namespace Transfers.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<TransfersApiFactory>
{
    public const string Name = "Transfers API";
}
