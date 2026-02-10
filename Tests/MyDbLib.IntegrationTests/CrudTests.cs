using Microsoft.Extensions.DependencyInjection;
using MyDbLib.Api;
using MyDbLib.Api.Interfaces;
using MyDbLib.Core.Extensions;
using MyDbLib.Core.Helpers;
using MyDbLib.Providers.SqlServer;
using Xunit;

namespace MyDbLib.IntegrationTests
{
    public class CrudTests
    {
        private readonly IDbDriver _sql;

        public CrudTests()
        {
            var services = new ServiceCollection();

            services.AddMyDbLibCore();

            services.AddMyDbLibSqlServer(
                name: "SQLServer",
                connectionString:
                    "Server=localhost;Database=ProjectDB;User Id=sa;Password=admin;Encrypt=False;"
            );

            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IDbDriverFactory>();

            _sql = factory.Get("SQLServer");
        }

        [Fact]
        public async Task Insert_Update_Delete_Should_Work()
        {
            int id = await DbCrudHelper.InsertAndGetIdAsync(
                _sql,
                "Users",
                new { Username = "TestUser", Email = "test@test.com" });

            Assert.True(id > 0);

            int updated = await DbCrudHelper.UpdateAsync(
                _sql,
                "Users",
                new { Email = "updated@test.com" },
                new { Id = id });

            Assert.Equal(1, updated);

            int deleted = await DbCrudHelper.DeleteAsync(
                _sql,
                "Users",
                new { Id = id });

            Assert.Equal(1, deleted);
        }
    }
}
