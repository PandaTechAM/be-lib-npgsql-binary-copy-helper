namespace NpgsqlBinaryCopyHelperTests;

[Collection("Shared Postgres")]
public class Tests : IAsyncLifetime
{
    private readonly Func<Task> _resetState;

    public Tests(PostgresFixture fixture)
    {
        _resetState = fixture.ResetStateAsync;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _resetState();

    [Fact]
    public void TestCopy()
    {
        //Arrange

        //Act

        //Assert
    }
}