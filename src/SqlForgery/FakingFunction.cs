namespace SqlForgery;

public class FakingFunction
{
    public FakingFunction(Delegate function, Type[]? ignoredTypes = null)
    {
        Function = function;
        IgnoredTypes = ignoredTypes ?? [];
    }

    public Delegate Function { get; init; }
    public Type[] IgnoredTypes { get; init; }
}
