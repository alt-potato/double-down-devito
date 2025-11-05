using System.Linq.Expressions;

namespace Project.Api.Models.Interface;

/// <summary>
/// Interface for entities with composite primary keys.
/// </summary>
/// <typeparam name="TKey1">The type of the first key component.</typeparam>
/// <typeparam name="TKey2">The type of the second key component.</typeparam>
public interface ICompositeEntity<TKey1, TKey2>
    where TKey1 : IEquatable<TKey1>
    where TKey2 : IEquatable<TKey2>
{
    /// <summary>
    /// Gets the first component of the composite key.
    /// </summary>
    TKey1 Key1 { get; }

    /// <summary>
    /// Gets the second component of the composite key.
    /// </summary>
    TKey2 Key2 { get; }

    /// <summary>
    /// Gets or sets the row version for optimistic concurrency.
    /// </summary>
    byte[] RowVersion { get; set; }
}

/// <summary>
/// Static helper methods for working with composite entities.
/// </summary>
public static class CompositeEntityExtensions
{
    /// <summary>
    /// Creates a predicate for a composite entity using the provided key.
    /// The predicate will filter entities based on the equality of the composite key components.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey1">The type of the first key component.</typeparam>
    /// <typeparam name="TKey2">The type of the second key component.</typeparam>
    /// <param name="keys">The composite key as a tuple.</param>
    /// <returns>A predicate that filters entities based on the provided composite key.</returns>
    public static Expression<Func<TEntity, bool>> CreateEqualityPredicate<TEntity, TKey1, TKey2>(
        this (TKey1, TKey2) keys
    )
        where TEntity : ICompositeEntity<TKey1, TKey2>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2> =>
        e => e.Key1.Equals(keys.Item1) && e.Key2.Equals(keys.Item2);

    /// <summary>
    /// Gets the composite key as a tuple from an entity.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <typeparam name="TKey1">The type of the first key component.</typeparam>
    /// <typeparam name="TKey2">The type of the second key component.</typeparam>
    /// <param name="entity">The entity.</param>
    /// <returns>The composite key as a tuple.</returns>
    public static (TKey1, TKey2) GetCompositeKey<TEntity, TKey1, TKey2>(this TEntity entity)
        where TEntity : ICompositeEntity<TKey1, TKey2>
        where TKey1 : IEquatable<TKey1>
        where TKey2 : IEquatable<TKey2>
    {
        return (entity.Key1, entity.Key2);
    }
}
