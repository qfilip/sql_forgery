using SqlForgery.Tests.Database;
using SqlForgery.Tests.Database.Entities;
using SqlForgery.Tests.Utils;
using Xunit;

namespace SqlForgery.Tests;

public class Class1
{
    public Class1()
    {
        
    }

    [Fact]
    public void InitialTest()
    {
        var cc = new ContextCreator();
        var ctx = cc.GetTestDbContext();
        var forger = new Forger(ctx, FakingFunctions.Get());

        var item = forger.Fake<Category>();
        ctx.SaveChanges();
    }
}
