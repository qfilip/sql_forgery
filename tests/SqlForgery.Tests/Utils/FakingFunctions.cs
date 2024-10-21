using SqlForgery.Tests.Database.Abstractions;
using SqlForgery.Tests.Database.Entities;

namespace SqlForgery.Tests.Utils;

internal sealed class FakingFunctions
{
    private static DateTime Now = new DateTime(2000, 1, 1);
    private static string MakeName() => $"test-{Guid.NewGuid().ToString("n").Substring(0, 6)}";
    public static T MakeEntity<T>(Func<T> retn) where T : IPkey<Guid>, IAuditable
    {
        var e = retn();
        e.Id = e.Id != Guid.Empty ? e.Id : Guid.NewGuid();

        e.AuditRecord = new()
        {
            CreatedAt = Now,
            ModifiedAt = Now
        };

        return e;
    }

    public static T MakeEntityWithCustomKey<T>(Func<T> retn) where T : IAuditable
    {
        var e = retn();

        e.AuditRecord = new()
        {
            CreatedAt = Now,
            ModifiedAt = Now
        };

        return e;
    }

    internal static IDictionary<Type, Delegate> Get()
    {
        return new Dictionary<Type, Delegate>()
        {
            {
                typeof(Company),
                () => MakeEntity(() => new Company())
            },
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
                () => MakeEntityWithCustomKey(() => new Excerpt { Quantity = 1 })
            },
            {
                typeof(Item),
                () => MakeEntity(() => new Item { Name = MakeName() })
            }
        };
    }
}
