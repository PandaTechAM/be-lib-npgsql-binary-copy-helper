using Microsoft.EntityFrameworkCore;

namespace NpgsqlBinaryCopyHelperTests;

public class Context: DbContext
{
    public Context(DbContextOptions<Context> options) : base(options)
    {
    }

    public DbSet<SomeModel> SomeModels { get; set; } = null!;
}