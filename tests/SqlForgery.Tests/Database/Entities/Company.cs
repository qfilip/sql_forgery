using SqlForgery.Tests.Database.Abstractions;
using SqlForgery.Tests.Database.Records;

namespace SqlForgery.Tests.Database.Entities;

internal class Company : IPkey<Guid>, IAuditable
{
    public Guid Id { get; set; }
    public AuditRecord AuditRecord { get; set; } = new();
    public Company()
    {
        Categories = new HashSet<Category>();
    }
    public ICollection<Category> Categories { get; set; }
}
