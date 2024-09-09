namespace SqlForgery.Tests.Database.Entities;

internal class Category : EntityBase
{
    public Category()
    {
        ChildCategories = new HashSet<Category>();
        Items = new HashSet<Item>();
    }

    public string? Name { get; set; }

    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public ICollection<Category> ChildCategories { get; set; }
    public ICollection<Item> Items { get; set; }
}
