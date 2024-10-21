using SqlForgery.Tests.Database.Abstractions;
using SqlForgery.Tests.Database.Records;

namespace SqlForgery.Tests.Database.Entities;

internal class UnitOfMeasure : IPkey<Guid>, IAuditable
{
    public Guid Id { get; set; }
    public AuditRecord AuditRecord { get; set; } = new();
    public UnitOfMeasure()
    {
        Items = new HashSet<Item>();
    }
    public string? Name { get; set; }
    public string? Symbol { get; set; }

    // relational
    public ICollection<Item> Items { get; set; }
}
