using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SqlForgery;

public sealed class Forger
{
    private readonly IDictionary<Type, Delegate> _fakingFunctions;
    private readonly IDictionary<Type, Type[]> _navigations;
    private readonly DbContext _dbContext;
    
    public Forger(DbContext dbContext, IDictionary<Type, Delegate> fakingFunctions)
    {
        _dbContext = dbContext;
        _fakingFunctions = fakingFunctions;

        var entityTypes = _dbContext.Model.GetEntityTypes();
        _navigations = new Dictionary<Type, Type[]>();

        foreach(var entityType in entityTypes)
        {
            var navigations = entityType.GetDeclaredNavigations()
                .Select(x => x.ClrType)
                .Where(x => !x.IsGenericType)
                .ToArray();

            var hasSelfReference = navigations.Any(x => x == entityType.ClrType);

            var requiredNavigations = hasSelfReference
                ? navigations.Where(x => x != entityType.ClrType).ToArray()
                : navigations;
           
            _navigations.Add(entityType.ClrType, requiredNavigations);
        }
    }

    public TEntity Fake<TEntity>(Action<TEntity>? modifier = null)
        where TEntity : class
    {
        var entity = Fake(typeof(TEntity)) as TEntity;
        modifier?.Invoke(entity!);

        var dbSet = _dbContext.Set<TEntity>();
        dbSet.Add(entity!);

        return entity!;
    }

    private object Fake(Type entityType)
    {
        _ = _fakingFunctions.TryGetValue(entityType, out var fakingFunction);
        if (fakingFunction == null)
        {
            throw new ArgumentException($"Faking function for type {entityType}, not found");
        }

        var fakedEntity = fakingFunction.DynamicInvoke();

        if (fakedEntity == null)
        {
            throw new InvalidOperationException($"Faking function for type {entityType} returned null");
        }

        var fakedEntityType = fakedEntity.GetType();
        if (entityType != fakedEntityType)
        {
            throw new InvalidOperationException($"Faking function for type {entityType} returns the wrong type of: {fakedEntityType}");
        }

        _ = _navigations.TryGetValue(entityType, out var navigationTypes);
        if(navigationTypes == null)
        {
            navigationTypes = Array.Empty<Type>();
        }

        foreach (var navigationType in navigationTypes)
        {
            var navigationProperties = entityType.GetProperties()
                .Where(x => x.PropertyType == navigationType)
                .ToArray();

            if (navigationProperties.Length == 0)
            {
                throw new InvalidOperationException($"Invalid mapping of navigation property array. Navigation type: {navigationType} doesn't exist on entity type: {entityType}");
            }

            foreach (var navigationProperty in navigationProperties)
            {
                var navigationPropertyValue = navigationProperty.GetValue(fakedEntity);
                if (navigationPropertyValue == null)
                {
                    navigationProperty.SetValue(fakedEntity, Fake(navigationProperty.PropertyType));
                }
            }
        }

        return fakedEntity;
    }

    
}
