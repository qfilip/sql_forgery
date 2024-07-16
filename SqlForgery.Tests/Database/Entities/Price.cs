namespace SqlForgery.Tests.Database.Entities;

internal class Price : EntityBase
{
    public double UnitValue { get; set; }

    public Guid? ItemId { get; set; }
    public virtual Item? Item { get; set; }
}
