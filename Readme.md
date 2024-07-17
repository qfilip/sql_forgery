# SqlForgery

This is a simple library for faking SQL (relational) data using EntityFramework Core. It is intended for testing purposes only.

I strongly believe that faking relational data is far superior to mocking, even in unit tests. However, SQL relationship constraints can make that process very tedious. This library aims to automate it, by populating navigation properties in the class.

## How it works

Consider the following simple example from EF Core documentation page:

```csharp
public class BloggingContext : DbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True;ConnectRetryCount=0");
    }
}

public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }
    public int Rating { get; set; }

    // navigation property
    public List<Post> Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int BlogId { get; set; }

    // navigation property
    public Blog Blog { get; set; }
}
```

In this example `Blog.Posts` and `Post.Blog` are navigational properties. Since this is a one to many relationship, inserting `Blog` without `Post` can be done, while vice-versa is not true. SqlForgery will only populate required navigational fields.

## How to use

SqlForgery is unopinionated how you will fake the data. You must supply `IDictionary<Type, Delegate>` for each `DbSet<T>` class, where `Type` is class you wish to fake, and `Delegate` a function that produces faked object. Example:

```csharp
var fakingFunctions = new Dictionary<Type, Delegate>()
{
    {
        typeof(Blog),
        () => new Blog
        {
            BlogId = 1,
            Url = "http://blog.com",
            Rating = 5
        }
    },
    {
       typeof(Post),
        () => new Post
        {
            PostId = 1
            Title = "test"
            Content = "content"
        }
    }
};
```
Notice the absence of navigational properties. Ideally, this dictionary should be defined just once, or be made static. You can use Faker library or something similar if you want real looking data.

The faking function in here is just a draft. You can customize properties as needed when faking an entity on the spot (see below). Check source repository tests for inspiration.

Using EF Core SQLite for example, faking `Post` can be done like so:
```csharp
// ideally automate this part in tests
var connection = new SqliteConnection("Filename=:memory:");
connection.Open();

var options = new DbContextOptionsBuilder<BloggingContext>()
    .UseSqlite(connection)
    .Options;

var context = new BloggingContext(options);

var forger = new Forger(context, fakingFunctions);
```

Fake `Post` and `Blog` will be created automatically:

```csharp
var post = forger.Fake<Post>(); // returns faked object
context.SaveChanges();
/*
you won't get foreign key exception because Blog was inserted as well.
*/

var entity = context.Posts
    .Include(x => x.Blog)
    .First(x => x.Id == post.Id);
```

Customize faked object

```csharp
var post = forger.Fake<Post>(p => {
    x.Title = "I'm different"
});
```

Customize `Post` and its related `Blog`

```csharp
var post = forger.Fake<Post>(p => {
    x.Title = "I'm different";
    // use navigation property here
    x.Blog = forger.Fake<Blog>(b => b.Rating = 10);
});
```

Fake `Blog` (no `Post` will be created)

```csharp
var blog = forger.Fake<Blog>();
```

Fake `Blog` with 10 related `Post`s

```csharp
var blog = forger.Fake<Blog>(b => {
    b.Posts = Enumerable.Range(0, 10)
        .Select(_ => forger.Fake<Post>(p => p.Blog = b))
        .ToArray();
});
```