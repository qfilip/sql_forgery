using SqlForgery.Tests.Database.Entities;

namespace SqlForgery.Tests.Utils;

internal sealed class FakingFunctions
{
    private static DateTime Now = new DateTime(2000, 1, 1);
    private static string MakeName() => $"test-{Guid.NewGuid().ToString("n").Substring(0, 6)}";
    public static T MakeEntity<T>(Func<T> retn) where T : EntityBase
    {
        var e = retn();
        e.Id = e.Id != Guid.Empty ? e.Id : Guid.NewGuid();
        e.CreatedAt = Now;
        e.ModifiedAt = Now;

        return e;
    }
    internal static IDictionary<Type, Delegate> Get()
    {
        return new Dictionary<Type, Delegate>()
        {
            {
                typeof(Category),
                () => MakeEntity(() => new Category { Name = MakeName() })
            },
            {
                typeof(UnitOfMeasure),
                () => MakeEntity(() => new UnitOfMeasure
                {
                    Name = MakeName(),
                    Symbol = "uom"
                })
            },
            {
                typeof(Price),
                () => MakeEntity(() => new Price { UnitValue = 1 })
            },
            {
                typeof(Excerpt),
                () => MakeEntity(() => new Excerpt { Quantity = 1 })
            },
            {
                typeof(Item),
                () => MakeEntity(() => new Item { Name = MakeName() })
            }
        };
    }
}
