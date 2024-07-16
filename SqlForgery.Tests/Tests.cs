using Microsoft.EntityFrameworkCore;
using SqlForgery.Tests.Database;
using SqlForgery.Tests.Database.Entities;
using SqlForgery.Tests.Utils;
using Xunit;

namespace SqlForgery.Tests;

public class Tests
{
    private readonly TestDbContext _dbContext;
    private readonly Forger _sut;
    public Tests()
    {
        _dbContext = new ContextCreator().GetTestDbContext();
        _sut = new(_dbContext, FakingFunctions.Get());
    }

    [Fact]
    public void Test()
    {
        _sut.Fake<Excerpt>();
        _dbContext.Save();
    }

    [Fact]
    public void Relation_SelfReference_DoesntCauseStakOverflow()
    {
        var item = _sut.Fake<Category>();
        _dbContext.Save();
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
        _dbContext.Excerpts.First(x => x.Id == excerpt.Id);
    }
}
