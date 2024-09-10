namespace SqlForgery.Tests.Database.Entities;

internal class Company : EntityBase
{
    public Company()
    {
        Categories = new HashSet<Category>();
    }
    public ICollection<Category> Categories { get; set; }
}
