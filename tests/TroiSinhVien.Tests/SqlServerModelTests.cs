using Microsoft.EntityFrameworkCore;
using TroiSinhVien.Data;

namespace TroiSinhVien.Tests;

public sealed class SqlServerModelTests
{
    [Fact]
    public void Create_script_uses_sql_server_compatible_filtered_indexes()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=unused;User Id=unused;Password=unused;TrustServerCertificate=True")
            .Options;

        using var db = new ApplicationDbContext(options);
        var script = db.Database.GenerateCreateScript();

        Assert.Contains("CREATE TABLE [AspNetUsers]", script);
        Assert.Contains("[IsDeleted] = 0", script);
        Assert.DoesNotContain("FALSE", script.ToUpperInvariant());
    }
}
