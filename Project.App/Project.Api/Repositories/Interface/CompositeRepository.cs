using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project.Api.Data;
using Project.Api.Models.Interface;
using Project.Api.Utilities;

namespace Project.Api.Repositories.Interface;

/// <summary>
/// Base class for repositories, providing base implementations for common CRUD operations for entities with composite keys.
/// </summary>
/// <typeparam name="TEntity">The entity type to work with.</typeparam>
/// <typeparam name="TKey1">The first component of the composite key.</typeparam>
/// <typeparam name="TKey2">The second component of the composite key.</typeparam>
public abstract class CompositeRepository<TEntity, TKey1, TKey2>(
    AppDbContext context,
    ILogger<CompositeRepository<TEntity, TKey1, TKey2>>? logger = null
) : RepositoryBase<TEntity>(context, logger)
    where TEntity : class, ICompositeEntity<TKey1, TKey2>
    where TKey1 : IEquatable<TKey1>
    where TKey2 : IEquatable<TKey2>
{
    /// <summary>
    /// Gets an entity by its composite key. If no entity is found, returns null.
    ///
    /// If tracking is true, the entity will be tracked by the context.
    /// </summary>
    protected async Task<TEntity?> GetAsync(TKey1 key1, TKey2 key2, bool tracking = true) =>
        await GetAsync((key1, key2).CreateEqualityPredicate<TEntity, TKey1, TKey2>(), tracking);

    /// <summary>
    /// Check if an entity exists by its composite key. Lighter than GetAsync.
    /// </summary>
    protected async Task<bool> ExistsAsync(TKey1 key1, TKey2 key2) =>
        await ExistsAsync((key1, key2).CreateEqualityPredicate<TEntity, TKey1, TKey2>());

    /// <summary>
    /// Create a new entity and save it to the database.
    /// </summary>
    protected async Task<TEntity> CreateAsync(TEntity entity) =>
        await CreateAsync(entity, $"({entity.Key1}, {entity.Key2})");

    /// <summary>
    /// Update an existing entity and save it to the database.
    /// </summary>
    protected async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new BadRequestException($"{typeof(TEntity).Name} cannot be null.");
        }

        _logger?.LogDebug(
            "Updating {type} with composite key ({key1}, {key2})",
            typeof(TEntity).Name,
            entity.Key1,
            entity.Key2
        );

        TEntity trackedEntity = await GetTrackedEntityOrThrowAsync(entity);

        // update the tracked entity with values from the provided entity
        _context.Entry(trackedEntity).CurrentValues.SetValues(entity);

        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {type} with composite key ({key1}, {key2})",
            typeof(TEntity).Name,
            entity.Key1,
            entity.Key2
        );
        return trackedEntity;
    }

    /// <summary>
    /// Update an existing entity using the provided action and save it to the database.
    /// </summary>
    protected async Task<TEntity> UpdateAsync(TKey1 key1, TKey2 key2, Action<TEntity> updateAction)
    {
        _logger?.LogDebug(
            "Updating {type} with composite key ({key1}, {key2})",
            typeof(TEntity).Name,
            key1,
            key2
        );

        TEntity entity = await GetTrackedEntityOrThrowAsync(key1, key2);

        updateAction(entity);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {type} with composite key ({key1}, {key2})",
            typeof(TEntity).Name,
            entity.Key1,
            entity.Key2
        );
        return entity;
    }

    /// <summary>
    /// Update multiple existing entities and save them to the database.
    /// </summary>
    protected async Task<IReadOnlyList<TEntity>> UpdateRangeAsync(params TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
        {
            return [];
        }

        _logger?.LogDebug(
            "Updating {count} {type} entities",
            entities.Length,
            typeof(TEntity).Name
        );

        // batch entity verification to reduce database round trips
        var entityKeys = entities.Select(e => (e.Key1, e.Key2)).ToArray();
        IReadOnlyList<TEntity> trackedEntities = await GetTrackedEntitiesOrThrowAsync(entityKeys);

        List<TEntity> entityList = [];
        for (int i = 0; i < entities.Length; i++)
        {
            TEntity entity = entities[i];
            TEntity trackedEntity = trackedEntities[i];

            // update the tracked entity with values from the provided entity
            _context.Entry(trackedEntity).CurrentValues.SetValues(entity);
            entityList.Add(trackedEntity);
        }

        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {count} {type} entities",
            entities.Length,
            typeof(TEntity).Name
        );
        return entityList;
    }

    protected async Task<IReadOnlyList<TEntity>> UpdateRangeAsync(
        Action<TEntity> updateAction,
        params (TKey1, TKey2)[] keys
    )
    {
        if (keys == null || keys.Length == 0)
        {
            return [];
        }

        _logger?.LogDebug("Updating {count} {type} entities", keys.Length, typeof(TEntity).Name);

        // batch entity verification to reduce database round trips
        IReadOnlyList<TEntity> trackedEntities = await GetTrackedEntitiesOrThrowAsync(keys);

        List<TEntity> entityList = [];
        for (int i = 0; i < keys.Length; i++)
        {
            TEntity trackedEntity = trackedEntities[i];

            updateAction(trackedEntity);

            entityList.Add(trackedEntity);
        }

        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {count} {type} entities",
            keys.Length,
            typeof(TEntity).Name
        );
        return entityList;
    }

    /// <summary>
    /// Delete an entity from the database.
    /// </summary>
    protected async Task<TEntity> DeleteAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new BadRequestException($"{typeof(TEntity).Name} cannot be null.");
        }

        _logger?.LogDebug(
            "Deleting {type} with composite key ({key1}, {key2})",
            typeof(TEntity).Name,
            entity.Key1,
            entity.Key2
        );

        entity = await GetTrackedEntityOrThrowAsync(entity);

        _dbSet.Remove(entity);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully deleted {type} with composite key ({key1}, {key2})",
            typeof(TEntity).Name,
            entity.Key1,
            entity.Key2
        );
        return entity;
    }

    /// <summary>
    /// Delete an entity from the database by its composite key.
    /// </summary>
    protected async Task<TEntity> DeleteAsync(TKey1 key1, TKey2 key2)
    {
        TEntity entity = await GetTrackedEntityOrThrowAsync(key1, key2);

        _dbSet.Remove(entity);
        await SaveChangesWithConcurrencyCheckAsync();
        return entity;
    }

    /// <summary>
    /// Delete multiple entities from the database.
    /// </summary>
    protected async Task<IReadOnlyList<TEntity>> DeleteRangeAsync(params TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
        {
            return [];
        }

        _logger?.LogDebug(
            "Deleting {count} {type} entities",
            entities.Length,
            typeof(TEntity).Name
        );

        // Batch entity verification to reduce database round trips
        var entityKeys = entities.Select(e => (e.Key1, e.Key2)).ToArray();
        IReadOnlyList<TEntity> trackedEntities = await GetTrackedEntitiesOrThrowAsync(entityKeys);

        _dbSet.RemoveRange(trackedEntities);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully deleted {count} {type} entities",
            entities.Length,
            typeof(TEntity).Name
        );
        return trackedEntities;
    }

    /// <summary>
    /// Get an entity from the database by its composite key, or throw if not found.
    ///
    /// Used as an early check to avoid unnecessary database queries.
    /// </summary>
    protected async Task<TEntity> GetTrackedEntityOrThrowAsync(TKey1 key1, TKey2 key2)
    {
        // first check if entity is already tracked by the context
        EntityEntry<TEntity>? trackedEntry = _context
            .ChangeTracker.Entries<TEntity>()
            .FirstOrDefault(e => e.Entity.Key1.Equals(key1) && e.Entity.Key2.Equals(key2));

        if (trackedEntry != null)
        {
            return trackedEntry.Entity;
        }

        // if not tracked, query the database directly with tracking enabled
        TEntity? entity = await GetAsync(
            (key1, key2).CreateEqualityPredicate<TEntity, TKey1, TKey2>(),
            tracking: true
        );

        if (entity == null)
        {
            _logger?.LogWarning(
                "{type} with composite key ({key1}, {key2}) not found.",
                typeof(TEntity).Name,
                key1,
                key2
            );
            throw new NotFoundException(
                $"{typeof(TEntity).Name} with composite key ({key1}, {key2}) not found."
            );
        }

        return entity;
    }

    /// <summary>
    /// Check if an entity is tracked by the context, and if not, get it from the database and ensure
    /// its rowversion matches.
    ///
    /// Used as an early check to avoid unnecessary database queries.
    /// </summary>
    protected override async Task<TEntity> GetTrackedEntityOrThrowAsync(TEntity entity)
    {
        // try to find entity in change tracker
        EntityEntry<TEntity> entry = _context.Entry(entity);

        if (entry.State != EntityState.Detached)
        {
            // entity is already tracked, just return
            return entity;
        }

        // if not found, try to find an entity with a matching composite key
        TEntity trackedEntity = await GetTrackedEntityOrThrowAsync(entity.Key1, entity.Key2);

        // ensure rowversion matches given entity
        if (trackedEntity.RowVersion != entity.RowVersion)
        {
            throw new ConflictException(
                $"{typeof(TEntity).Name} with composite key ({entity.Key1}, {entity.Key2}) has been modified since you fetched it."
            );
        }

        return trackedEntity;
    }

    /// <summary>
    /// Get multiple entities from the database by their composite keys, or throw if any are not found.
    ///
    /// Used as an early check to avoid unnecessary database queries in batch operations.
    /// </summary>
    protected async Task<IReadOnlyList<TEntity>> GetTrackedEntitiesOrThrowAsync(
        params (TKey1, TKey2)[] keys
    )
    {
        if (keys == null || keys.Length == 0)
        {
            return [];
        }

        List<TEntity> result = [];
        List<(TKey1, TKey2)> missingKeys = [];

        // First check change tracker for all entities
        foreach (var (key1, key2) in keys)
        {
            EntityEntry<TEntity>? trackedEntry = _context
                .ChangeTracker.Entries<TEntity>()
                .FirstOrDefault(e => e.Entity.Key1.Equals(key1) && e.Entity.Key2.Equals(key2));

            if (trackedEntry != null)
            {
                result.Add(trackedEntry.Entity);
            }
            else
            {
                missingKeys.Add((key1, key2));
            }
        }

        // if all entities were found in change tracker, return them
        if (missingKeys.Count == 0)
        {
            return result;
        }

        // query database for missing entities in a single batch
        List<TEntity> missingEntities = await _dbSet
            .Where(e => missingKeys.Any(k => e.Key1.Equals(k.Item1) && e.Key2.Equals(k.Item2)))
            .ToListAsync();

        // Verify we found all missing entities
        HashSet<(TKey1, TKey2)> foundKeys = [.. missingEntities.Select(e => (e.Key1, e.Key2))];
        List<(TKey1, TKey2)> notFoundKeys = [.. missingKeys.Where(k => !foundKeys.Contains(k))];

        if (notFoundKeys.Count > 0)
        {
            _logger?.LogWarning(
                "{type} with composite keys {keys} not found.",
                typeof(TEntity).Name,
                string.Join(", ", notFoundKeys.Select(k => $"({k.Item1}, {k.Item2})"))
            );
            throw new NotFoundException(
                $"{typeof(TEntity).Name} with composite keys {string.Join(", ", notFoundKeys.Select(k => $"({k.Item1}, {k.Item2})"))} not found."
            );
        }

        result.AddRange(missingEntities);
        return result;
    }
}
