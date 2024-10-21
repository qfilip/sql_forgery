using SqlForgery.Tests.Database.Abstractions;
using SqlForgery.Tests.Database.Records;

namespace SqlForgery.Tests.Database.Entities;

internal class Price : IPkey<Guid>, IAuditable
{
    public Guid Id { get; set; }
    public AuditRecord AuditRecord { get; set; } = new();
    public double UnitValue { get; set; }

    public Guid? ItemId { get; set; }
    public Item? Item { get; set; }
}
