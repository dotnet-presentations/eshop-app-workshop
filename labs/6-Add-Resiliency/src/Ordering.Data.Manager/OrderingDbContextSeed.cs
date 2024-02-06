namespace eShop.Ordering.Data.Manager;

public class OrderingDbContextSeed : IDbSeeder<OrderingDbContext>
{
    public async Task SeedAsync(OrderingDbContext context)
    {

        if (!context.CardTypes.Any())
        {
            context.CardTypes.AddRange(GetPredefinedCardTypes());

            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
    }

    private static IEnumerable<CardType> GetPredefinedCardTypes()
    {
        return Enumeration.GetAll<CardType>();
    }
}
