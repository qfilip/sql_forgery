﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SqlForgery;

public sealed class Forger
{
    private readonly IDictionary<Type, Delegate> _fakingFunctions;
    private readonly IDictionary<Type, NavigationType[]> _navigations;
    private readonly DbContext _dbContext;
    
    public Forger(DbContext dbContext, IDictionary<Type, Delegate> fakingFunctions)
    {
        _dbContext = dbContext;
        _fakingFunctions = fakingFunctions;

        var fakingTypes = _fakingFunctions.Keys.ToArray();
        var entityTypes = _dbContext.Model.GetEntityTypes().ToArray();
        var ownedEntities = entityTypes.Where(x => x.IsOwned()).ToArray();

        CheckForNonDbTypes(entityTypes, fakingTypes);
        
        _navigations = new Dictionary<Type, NavigationType[]>();

        foreach(var entityType in entityTypes)
        {
            var navigations = entityType.GetDeclaredNavigations()
                .Where(x => !x.ClrType.IsGenericType)
                .ToArray();

            var navigationTypes = navigations
                .Select(x => MapNavigationType(entityType, x, entityTypes, ownedEntities))
                .ToArray();
           
            _navigations.TryAdd(entityType.ClrType, navigationTypes);
        }
    }

    private static void CheckForNonDbTypes(IEntityType[] entityTypes, Type[] fakingTypes)
    {
        var dbTypes = entityTypes.Select(x => x.ClrType).ToArray();
        var nonDbTypes = fakingTypes
            .Where(x => !dbTypes.Contains(x))
            .ToArray();

        if(nonDbTypes.Any())
        {
            var forbidden = string.Join(',', nonDbTypes.Select(x => x.Name));
            throw new ArgumentException($"Faking functions contain types not present in the database: {forbidden}");
        }

    }

    private static NavigationType MapNavigationType(
        IEntityType entityType,
        INavigation navigation,
        IEntityType[] allEntityTypes,
        IEntityType[] ownedEntities)
    {
        var navigationType = GetUnderlyingType(navigation.ClrType);
        
        var isOwned = ownedEntities.Any(x => x.ClrType == navigationType);
        
        var isSelfReferenceRelation = entityType.ClrType == navigation.ClrType;
        
        var required = !isOwned && !isSelfReferenceRelation;

        if(isOwned && isSelfReferenceRelation)
        {
            var message = $"Self referencing type {navigation.ClrType} cannot be owned by {entityType.ClrType}";
            throw new ArgumentException(message);
        }

        var oneToOneRelation = allEntityTypes
            .Where(x =>
                x.ClrType == navigation.ClrType &&
                x.GetDeclaredNavigations().Any(n => n.ClrType == entityType.ClrType))
            .SingleOrDefault();

        return new(
            type: navigation.ClrType,
            required: required,
            substituteProperty: oneToOneRelation != null ? entityType.ClrType : null);
    }

    private static Type GetUnderlyingType(Type type)
    {
        var enumerableType = type.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            .FirstOrDefault();
        
        if (enumerableType == null)
        {
            return type;
        }
        
        var genericArguments = enumerableType.GetGenericArguments();
        if(genericArguments == null || genericArguments.Length == 0)
        {
            throw new ArgumentException($"Couldn't find generic arguments for type {type.Name}");
        }

        if(genericArguments.Length > 1)
        {
            var message = $"Cannot use {type} with SqlForgery. Enumerables with more than one generic parameter not supported.";
            throw new ArgumentException(message);
        }

        return genericArguments[0];
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

    private object Fake(Type entityType, Type? substituteProperty = null, object? substituteObject = null)
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
            navigationTypes = Array.Empty<NavigationType>();
        }

        foreach (var navigationType in navigationTypes)
        {
            if(!entityType.GetProperties().Any(x => x.PropertyType == navigationType.Type))
            {
                throw new InvalidOperationException($"Invalid mapping of navigation property array. Navigation type: {navigationType} doesn't exist on entity type: {entityType}");
            }

            var navigationProperty = entityType.GetProperties()
                .Where(x =>
                    x.PropertyType == navigationType.Type &&
                    x.GetValue(fakedEntity) == null)
                .FirstOrDefault();

            if (navigationProperty == null || !navigationType.Required)
            {
                continue;
            }

            if(
                substituteProperty != null &&
                substituteObject != null &&
                navigationProperty.PropertyType == substituteProperty)
            {
                if(navigationProperty.PropertyType != substituteObject.GetType())
                {
                    throw new ArgumentException($"Expected substitute object of type {navigationProperty.PropertyType}, but received {substituteObject.GetType()}");
                }

                navigationProperty.SetValue(fakedEntity, substituteObject);
                continue;
            }

            var fakedNavigation = navigationType.HasSubstituteProperty()
                ? Fake(navigationType.Type, fakedEntityType, fakedEntity)
                : Fake(navigationType.Type);

            navigationProperty.SetValue(fakedEntity, fakedNavigation);
        }

        return fakedEntity;
    }
}
