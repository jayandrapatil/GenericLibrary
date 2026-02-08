using MyDbLib.Api;
using MyDbLib.Api.Exceptions;
using MyDbLib.Api.Interfaces;
using MyDbLib.Core.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace MyDbLib.Tests;

public class DbCrudHelperTests
{
    [Fact]
    public async Task InsertAsync_ShouldThrow_WhenTableNameIsNull()
    {
        var data = new { Name = "Test" };

        await Assert.ThrowsAsync<DbLibException>(() =>
            DbCrudHelper.InsertAsync((IDbDriver)null!, null!, data));
    }

    [Fact]
    public void Insert_ShouldThrow_WhenTableNameIsNull()
    {
        var data = new { Name = "Test" };

        Assert.Throws<DbLibException>(() =>
            DbCrudHelper.Insert((IDbDriver)null!, null!, data));
    }

    [Fact]
    public async Task InsertAsync_ShouldThrow_WhenDataIsNull()
    {
        await Assert.ThrowsAsync<DbLibException>(() =>
            DbCrudHelper.InsertAsync((IDbDriver)null!, "Users", null!));
    }

    [Fact]
    public async Task InsertAsync_ShouldThrow_WhenTableNameIsEmpty()
    {
        var data = new { Name = "Test" };

        await Assert.ThrowsAsync<DbLibException>(() =>
            DbCrudHelper.InsertAsync((IDbDriver)null!, "", data));
    }

    [Fact]
    public void BuildInsert_ShouldGenerateCorrectSql()
    {
        var data = new
        {
            Username = "Jay",
            Email = "jay@test.com"
        };

        var result = DbCrudHelper.BuildInsert("Users", data);

        Assert.Contains("INSERT INTO Users", result.sql);
        Assert.Contains("Username", result.sql);
        Assert.Contains("Email", result.sql);
    }

    [Fact]
    public void BuildUpdate_ShouldGenerateCorrectSql()
    {
        var data = new
        {
            Username = "Jay",
            Email = "jay@test.com"
        };

        var where = new { Id = 1 };

        var result = DbCrudHelper.BuildUpdate("Users", data, where);

        Assert.Contains("UPDATE Users", result.sql);
        Assert.Contains("Username = @Username", result.sql);
        Assert.Contains("Email = @Email", result.sql);
        Assert.Contains("WHERE Id = @w_Id", result.sql);
    }

    [Fact]
    public void BuildDelete_ShouldGenerateCorrectSql()
    {
        var where = new { Id = 1 };

        var result = DbCrudHelper.BuildDelete("Users", where);

        Assert.Contains("DELETE FROM Users", result.sql);
        Assert.Contains("WHERE Id = @Id", result.sql);
    }

}
