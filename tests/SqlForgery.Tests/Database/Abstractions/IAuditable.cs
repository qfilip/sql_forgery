using SqlForgery.Tests.Database.Records;

namespace SqlForgery.Tests.Database.Abstractions;

internal interface IAuditable
{
    public AuditRecord AuditRecord { get; set; }
}
