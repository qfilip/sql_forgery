namespace SqlForgery;

internal sealed class NavigationType
{

    private readonly Type? _substituteProperty;
    public NavigationType(
        Type type,
        bool required = true,
        Type? substituteProperty = null
        )
    {
        Type = type;
        Required = required;
        
        _substituteProperty = substituteProperty;
    }

    public Type Type { get; init; }
    public bool Required { get; init; }

    public bool HasSubstituteProperty() => _substituteProperty != null;
    public Type GetSubstituteProperty() => _substituteProperty!;
}
