using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Project.Api.Data;
using Project.Api.Models.Interfaces;
using Project.Api.Utilities;

namespace Project.Api.Repositories.Interface;

/// <summary>
/// Base class for repositories, providing base implementations for common CRUD operations.
/// </summary>
/// <typeparam name="TEntity">The entity type to work with.</typeparam>
/// <typeparam name="TKey">The primary key type for the entity.</typeparam>
public abstract class SingleRepository<TEntity, TKey>(
    AppDbContext context,
    ILogger<SingleRepository<TEntity, TKey>>? logger = null
) : RepositoryBase<TEntity>(context, logger)
    where TEntity : class, IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    /// <summary>
    /// Gets an entity by its primary key. If no entity is found, returns null.
    ///
    /// If tracking is true, the entity will be tracked by the context.
    /// </summary>
    protected virtual async Task<TEntity?> GetAsync(TKey id, bool tracking = true) =>
        await GetAsync(e => e.Id.Equals(id), tracking);

    /// <summary>
    /// Check if an entity exists by its primary key. Lighter than GetAsync.
    /// </summary>
    protected virtual async Task<bool> ExistsAsync(TKey id) =>
        await ExistsAsync(e => e.Id.Equals(id));

    /// <summary>
    /// Create a new entity and save it to the database.
    /// </summary>
    protected virtual async Task<TEntity> CreateAsync(TEntity entity) =>
        await CreateAsync(
            entity ?? throw new BadRequestException($"{typeof(TEntity).Name} cannot be null."),
            entity.Id.ToString()
        );

    /// <summary>
    /// Update an existing entity and save it to the database.
    /// </summary>
    protected virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new BadRequestException($"{typeof(TEntity).Name} cannot be null.");
        }

        _logger?.LogDebug("Updating {type} with ID {id}", typeof(TEntity).Name, entity.Id);

        TEntity trackedEntity = await GetTrackedEntityOrThrowAsync(entity);

        // update the tracked entity with values from the provided entity
        _context.Entry(trackedEntity).CurrentValues.SetValues(entity);

        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {type} with ID {id}",
            typeof(TEntity).Name,
            entity.Id
        );
        return trackedEntity;
    }

    /// <summary>
    /// Update an existing entity using the provided action and save it to the database.
    /// </summary>
    protected virtual async Task<TEntity> UpdateAsync(TKey id, Action<TEntity> updateAction)
    {
        _logger?.LogDebug("Updating {type} with ID {id}", typeof(TEntity).Name, id);

        TEntity entity = await GetTrackedEntityOrThrowAsync(id);

        updateAction(entity);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {type} with ID {id}",
            typeof(TEntity).Name,
            entity.Id
        );
        return entity;
    }

    /// <summary>
    /// Update multiple existing entities and save them to the database.
    /// </summary>
    protected virtual async Task<IReadOnlyList<TEntity>> UpdateRangeAsync(params TEntity[] entities)
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
        TKey[] entityIds = [.. entities.Select(e => e.Id)];
        IReadOnlyList<TEntity> trackedEntities = await GetTrackedEntitiesOrThrowAsync(entityIds);

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

    protected virtual async Task<IReadOnlyList<TEntity>> UpdateRangeAsync(
        Action<TEntity> updateAction,
        params TKey[] ids
    )
    {
        if (ids == null || ids.Length == 0)
        {
            return [];
        }

        _logger?.LogDebug("Updating {count} {type} entities", ids.Length, typeof(TEntity).Name);

        // batch entity verification to reduce database round trips
        IReadOnlyList<TEntity> trackedEntities = await GetTrackedEntitiesOrThrowAsync(ids);

        List<TEntity> entityList = [];
        for (int i = 0; i < ids.Length; i++)
        {
            TEntity trackedEntity = trackedEntities[i];

            updateAction(trackedEntity);

            entityList.Add(trackedEntity);
        }

        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully updated {count} {type} entities",
            ids.Length,
            typeof(TEntity).Name
        );
        return entityList;
    }

    /// <summary>
    /// Delete an entity from the database.
    /// </summary>
    protected virtual async Task<TEntity> DeleteAsync(TEntity entity)
    {
        if (entity == null)
        {
            throw new BadRequestException($"{typeof(TEntity).Name} cannot be null.");
        }

        _logger?.LogDebug("Deleting {type} with ID {id}", typeof(TEntity).Name, entity.Id);

        entity = await GetTrackedEntityOrThrowAsync(entity);

        _dbSet.Remove(entity);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully deleted {type} with ID {id}",
            typeof(TEntity).Name,
            entity.Id
        );
        return entity;
    }

    /// <summary>
    /// Delete an entity from the database by its primary key.
    /// </summary>
    protected virtual async Task<TEntity> DeleteAsync(TKey id)
    {
        TEntity entity = await GetTrackedEntityOrThrowAsync(id);

        _dbSet.Remove(entity);
        await SaveChangesWithConcurrencyCheckAsync();
        return entity;
    }

    /// <summary>
    /// Delete multiple entities from the database.
    /// </summary>
    protected virtual async Task<IReadOnlyList<TEntity>> DeleteRangeAsync(params TEntity[] entities)
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
        TKey[] entityIds = [.. entities.Select(e => e.Id)];
        IReadOnlyList<TEntity> trackedEntities = await GetTrackedEntitiesOrThrowAsync(entityIds);

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
    /// Get an entity from the database by its primary key, or throw if not found.
    ///
    /// Used as an early check to avoid unnecessary database queries.
    /// </summary>
    protected virtual async Task<TEntity> GetTrackedEntityOrThrowAsync(TKey id)
    {
        // first check if entity is already tracked by the context
        EntityEntry<TEntity>? trackedEntry = _context
            .ChangeTracker.Entries<TEntity>()
            .FirstOrDefault(e => e.Entity.Id.Equals(id));

        if (trackedEntry != null)
        {
            return trackedEntry.Entity;
        }

        // if not tracked, query the database directly with tracking enabled
        TEntity? entity = await GetAsync(e => e.Id.Equals(id), tracking: true);

        if (entity == null)
        {
            _logger?.LogWarning("{type} with ID {id} not found.", typeof(TEntity).Name, id);
            throw new NotFoundException($"{typeof(TEntity).Name} with ID {id} not found.");
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

        // if not found, try to find an entity with a matching primary key
        TEntity trackedEntity = await GetTrackedEntityOrThrowAsync(entity.Id);

        // ensure rowversion matches given entity
        if (trackedEntity.RowVersion != entity.RowVersion)
        {
            throw new ConflictException(
                $"{typeof(TEntity).Name} with ID {entity.Id} has been modified since you fetched it."
            );
        }

        return trackedEntity;
    }

    /// <summary>
    /// Get multiple entities from the database by their primary keys, or throw if any are not found.
    ///
    /// Used as an early check to avoid unnecessary database queries in batch operations.
    /// </summary>
    protected virtual async Task<IReadOnlyList<TEntity>> GetTrackedEntitiesOrThrowAsync(
        params TKey[] ids
    )
    {
        if (ids == null || ids.Length == 0)
        {
            return [];
        }

        List<TEntity> result = [];
        List<TKey> missingIds = [];

        // First check change tracker for all entities
        foreach (TKey id in ids)
        {
            EntityEntry<TEntity>? trackedEntry = _context
                .ChangeTracker.Entries<TEntity>()
                .FirstOrDefault(e => e.Entity.Id.Equals(id));

            if (trackedEntry != null)
            {
                result.Add(trackedEntry.Entity);
            }
            else
            {
                missingIds.Add(id);
            }
        }

        // if all entities were found in change tracker, return them
        if (missingIds.Count == 0)
        {
            return result;
        }

        // query database for missing entities in a single batch
        List<TEntity> missingEntities = await _dbSet
            .Where(e => missingIds.Contains(e.Id))
            .ToListAsync();

        // Verify we found all missing entities
        HashSet<TKey> foundIds = [.. missingEntities.Select(e => e.Id)];
        List<TKey> notFoundIds = [.. missingIds.Where(id => !foundIds.Contains(id))];

        if (notFoundIds.Count > 0)
        {
            _logger?.LogWarning(
                "{type} with IDs {ids} not found.",
                typeof(TEntity).Name,
                string.Join(", ", notFoundIds)
            );
            throw new NotFoundException(
                $"{typeof(TEntity).Name} with IDs {string.Join(", ", notFoundIds)} not found."
            );
        }

        result.AddRange(missingEntities);
        return result;
    }
}

/// <summary>
/// Convenience base class for repositories with a GUID primary key.
/// </summary>
/// <typeparam name="TEntity">The entity type to work with.</typeparam>
public abstract class Repository<TEntity>(
    AppDbContext context,
    ILogger<SingleRepository<TEntity, Guid>>? logger = null
) : SingleRepository<TEntity, Guid>(context, logger)
    where TEntity : class, IEntity<Guid> { }
