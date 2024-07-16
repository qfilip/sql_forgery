namespace SqlForgery.Tests.Database.Entities;

internal class Item : EntityBase
{
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
}