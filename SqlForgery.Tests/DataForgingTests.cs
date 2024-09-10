using Microsoft.EntityFrameworkCore;
using SqlForgery.Tests.Database;
using SqlForgery.Tests.Database.Entities;
using SqlForgery.Tests.Utils;
using Xunit;

namespace SqlForgery.Tests;

public class DataForgingTests
{
    private readonly TestDbContext _dbContext;
    private readonly Forger _sut;
    public DataForgingTests()
    {
        _dbContext = new ContextCreator().GetTestDbContext();
        _sut = new(_dbContext, FakingFunctions.Get());
    }

    [Fact]
    public void Relation_SelfReference_DoesntCauseStakOverflow()
    {
        var item = _sut.Fake<Category>();
        _dbContext.Save();
    }

    [Fact]
    public void Relation_OneToOne_MapsCorrectly()
    {
        // arrange
        var price = _sut.Fake<Price>();

        // act
        _dbContext.Save();

        // assert
        _dbContext.Prices
            .Include(x => x.Item)
            .Single(x => x.Id == price.Id);

        Assert.NotNull(price.Item);
    }

    [Fact]
    public void Relation_OneToOne_WithModifier_MapsCorrectly()
    {
        // arrange
        var name = "test"; 
        var price = _sut.Fake<Price>(x =>
        {
            x.Item = _sut.Fake<Item>(x => x.Name = name);
        });

        // act
        _dbContext.Save();

        // assert
        var entity = _dbContext.Prices
            .Include(x => x.Item)
            .Single(x => x.Id == price.Id);

        Assert.NotNull(entity.Item);
        Assert.Equal(name, entity.Item.Name);
    }

    [Fact]
    public void Relation_OneToMany_DoesntCreateMany()
    {
        // arrange
        var unitOfMeasure = _sut.Fake<UnitOfMeasure>();

        // act
        _dbContext.Save();

        // assert
        var entity = _dbContext.UnitsOfMeasure
            .Include(x => x.Items)
            .First(x => x.Id == unitOfMeasure.Id);

        Assert.Empty(entity.Items);
    }

    [Fact]
    public void CustomModifier()
    {
        // arrange
        var category = _sut.Fake<Category>(x =>
        {
            x.Name = "child";
            x.ParentCategory = _sut.Fake<Category>(c =>
            {
                c.Name = "parent";
            });
        });

        // act
        _dbContext.Save();

        // assert
        var entity = _dbContext.Categories
            .Include(x => x.ParentCategory)
            .First(x => x.Id == category.Id);

        Assert.Equal(category.Name, entity.Name);
        Assert.Equal(category.ParentCategory!.Name, entity.ParentCategory!.Name);
    }

    [Fact]
    public void RequiredRelations_CreatedAutomatically()
    {
        // arrange
        var excerpt = _sut.Fake<Excerpt>();

        // act
        _dbContext.Save();

        // assert
        var entity = _dbContext.Excerpts
            .Include(x => x.Element)
            .Include(x => x.Composite)
            .First(x => x.Id == excerpt.Id);

        Assert.NotNull(entity.Element);
        Assert.NotNull(entity.Composite);
    }

    [Fact]
    public void JsonFields_NotPopulated()
    {
        // arrange
        var id = _sut.Fake<Item>().Id;

        // act
        _dbContext.Save();

        // assert
        var entity = _dbContext.Items
            .First(x => x.Id == id);

        Assert.Null(entity.Orders);
        Assert.Null(entity.Details);
    }
}
