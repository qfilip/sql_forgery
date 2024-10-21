using SqlForgery.Tests.Database.Abstractions;
using SqlForgery.Tests.Database.Records;

namespace SqlForgery.Tests.Database.Entities;

internal class Excerpt : IAuditable
{
    public AuditRecord AuditRecord { get; set; } = new();
    public double Quantity { get; set; }

    // relational
    public Guid CompositeId { get; set; }
    public Item? Composite { get; set; }

    public Guid ElementId { get; set; }
    public Item? Element { get; set; }
}
