using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SqlForgery.Tests.Database.Entities;

namespace SqlForgery.Tests.Database;

internal sealed class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) {}

    internal DbSet<Company> Compaines { get; set; }
    internal DbSet<Category> Categories { get; set; }
    internal DbSet<Item> Items { get; set; }
    internal DbSet<Excerpt> Excerpts { get; set; }
    internal DbSet<Price> Prices { get; set; }
    internal DbSet<UnitOfMeasure> UnitsOfMeasure { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureEntity<Company>(modelBuilder, e =>
        {
            e.ToTable("Companies");
        });

        ConfigureEntity<Category>(modelBuilder, e =>
        {
            e.Property(x => x.Name).IsRequired();

            e.HasOne(x => x.Company).WithMany(x => x.Categories)
                .HasForeignKey(x => x.CompanyId)
                .HasConstraintName($"FK_{nameof(Category)}_{nameof(Company)}");

            e.HasOne(x => x.ParentCategory).WithMany(x => x.ChildCategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .IsRequired(false)
                .HasConstraintName($"FK_{nameof(Category)}_{nameof(Category)}");
        });

        ConfigureEntity<Excerpt>(modelBuilder, e =>
        {
            e.HasKey(x => new { x.CompositeId, x.ElementId });

            e.HasOne(x => x.Composite).WithMany(x => x.CompositeExcerpts)
                .HasForeignKey(x => x.CompositeId).IsRequired()
                .HasConstraintName($"FK_Composite{nameof(Item)}_{nameof(Excerpt)}");

            e.HasOne(x => x.Element).WithMany(x => x.ElementExcerpts)
                .HasForeignKey(x => x.ElementId).IsRequired()
                .HasConstraintName($"FK_Element{nameof(Item)}_{nameof(Excerpt)}");
        }, customKey: true);

        ConfigureEntity<UnitOfMeasure>(modelBuilder, e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).IsRequired();
            e.Property(x => x.Symbol).IsRequired();

        });

        ConfigureEntity<Price>(modelBuilder, e =>
        {
            e.Property(x => x.UnitValue).IsRequired();

            e.HasOne(x => x.Item).WithOne(x => x.Price)
                .HasForeignKey<Price>(x => x.ItemId)
                .HasConstraintName($"FK_{nameof(Item)}_{nameof(Price)}");
        });

        ConfigureEntity<Item>(modelBuilder, e =>
        {
            e.Property(x => x.Name).IsRequired();

            e.HasOne(x => x.Category).WithMany(x => x.Items)
                .HasForeignKey(x => x.CategoryId).IsRequired()
                .HasConstraintName($"FK_{nameof(Category)}_{nameof(Item)}");

            e.HasOne(x => x.UnitOfMeasure).WithMany(x => x.Items)
                .HasForeignKey(x => x.UnitOfMeasureId).IsRequired()
                .HasConstraintName($"FK_{nameof(UnitOfMeasure)}_{nameof(Item)}");

            e.OwnsMany(x => x.Orders, nav => nav.ToJson());
            e.OwnsOne(x => x.Details, nav => nav.ToJson());
        });

        base.OnModelCreating(modelBuilder);
    }

    private void ConfigureEntity<T>(ModelBuilder mb, Action<EntityTypeBuilder<T>>? customConfiguration = null, bool customKey = false) where T : EntityBase
    {
        var name = typeof(T).Name;
        mb.Entity<T>(e =>
        {
            e.ToTable(name);
            
            if (!customKey)
                e.HasKey(x => x.Id);
            
            customConfiguration?.Invoke(e);
        });
    }

    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries();

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added && entry.Entity is EntityBase added)
            {
                added.Id = added.Id == Guid.Empty ? Guid.NewGuid() : added.Id;
                added.CreatedAt = now;
                added.ModifiedAt = now;

            }
            else if (entry.State == EntityState.Modified && entry.Entity is EntityBase modded)
            {
                modded.ModifiedAt = now;
            }
        }

        return base.SaveChanges();
    }

    public void Save()
    {
        SaveChanges();
        ChangeTracker.Clear();
    }
}
