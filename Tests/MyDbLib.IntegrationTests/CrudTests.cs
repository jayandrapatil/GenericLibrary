using Microsoft.Extensions.DependencyInjection;
using MyDbLib.Api;                // IDbDriver
using MyDbLib.Core.Extensions;    // AddMyDbLibCore()
using MyDbLib.Core.Helpers;       // DbCrudHelper (if inside Helpers)
using MyDbLib.Providers.SqlServer;


public class CrudTests
{
    private readonly IDbDriver _sql;

    public CrudTests()
    {
        var services = new ServiceCollection();

        services.AddMyDbLibCore();

        services.AddMyDbLibSqlServer(
            name: "SQLServer",
            connectionString: "Server=localhost;Database=ProjectDB;User Id=sa;Password=admin;Encrypt=False;"
        );

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IDbDriverFactory>();

        _sql = factory.Get("SQLServer");
    }

    [Fact]
    public async Task Insert_Update_Delete_Should_Work()
    {
        int id = await DbCrudHelper.InsertAndGetIdAsync(_sql, "Users",
            new { Username = "TestUser", Email = "test@test.com" });

        Assert.True(id > 0);

        int up = await DbCrudHelper.UpdateAsync(_sql, "Users",
            new { Email = "updated@test.com" },
            new { Id = id });

        Assert.Equal(1, up);

        int del = await DbCrudHelper.DeleteAsync(_sql, "Users", new { Id = id });

        Assert.Equal(1, del);
    }
}
