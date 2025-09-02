namespace Infrastructure;

public class DbInitializer
{
    public void Initialize(AppDbContext context)
    {

        context.Database.EnsureDeleted();

        context.Database.EnsureCreated();
    }
}
