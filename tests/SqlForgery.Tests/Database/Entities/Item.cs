using SqlForgery.Tests.Database.Abstractions;
using SqlForgery.Tests.Database.JsonEntities;
using SqlForgery.Tests.Database.Records;

namespace SqlForgery.Tests.Database.Entities;

internal class Item : IPkey<Guid>, IAuditable
{
    public Guid Id { get; set; }
    public AuditRecord AuditRecord { get; set; } = new();
    public Item()
    {
        CompositeExcerpts = new HashSet<Excerpt>();
        ElementExcerpts = new HashSet<Excerpt>();
    }

    public string? Name { get; set; }

    // relational
    public Guid CategoryId { get; set; }
    public Category? Category { get; set; }

    public Guid UnitOfMeasureId { get; set; }
    public UnitOfMeasure? UnitOfMeasure { get; set; }

    public Guid PriceId { get; set; }
    public Price? Price { get; set; }

    public ICollection<Excerpt> ElementExcerpts { get; set; }
    public ICollection<Excerpt> CompositeExcerpts { get; set; }

    // json
    public ICollection<ItemOrder> Orders { get; set; } = new List<ItemOrder>();
    public ItemDetails? Details { get; set; }
}