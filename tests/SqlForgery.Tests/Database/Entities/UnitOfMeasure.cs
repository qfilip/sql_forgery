namespace SqlForgery.Tests.Database.Entities;

internal class UnitOfMeasure : EntityBase
{
    public UnitOfMeasure()
    {
        Items = new HashSet<Item>();
    }
    public string? Name { get; set; }
    public string? Symbol { get; set; }

    // relational
    public ICollection<Item> Items { get; set; }
}
