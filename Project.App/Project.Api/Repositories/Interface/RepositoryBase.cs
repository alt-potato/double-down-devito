using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Project.Api.Data;
using Project.Api.Utilities;

namespace Project.Api.Repositories.Interface;

/// <summary>
/// Base class containing common repository functionality that doesn't depend on key structure.
/// </summary>
/// <typeparam name="TEntity">The entity type to work with.</typeparam>
public abstract class RepositoryBase<TEntity>(
    AppDbContext context,
    ILogger<RepositoryBase<TEntity>>? logger = null
)
    where TEntity : class
{
    protected readonly AppDbContext _context = context;
    protected readonly DbSet<TEntity> _dbSet = context.Set<TEntity>();
    protected readonly ILogger<RepositoryBase<TEntity>>? _logger = logger;

    /// <summary>
    /// Gets the first entity that matches a given LINQ predicate. If no entity is found, returns null.
    ///
    /// If tracking is true, the entity will be tracked by the context.
    /// </summary>
    protected async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        bool tracking = true
    )
    {
        return tracking
            ? await _dbSet.FirstOrDefaultAsync(predicate)
            : await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate);
    }

    /// <summary>
    /// Gets all entities that match a given LINQ predicate, with optional ordering, including, and tracking.
    ///
    /// If no arguments are provided, returns all entities.
    /// </summary>
    protected async Task<IReadOnlyList<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        int? skip = null,
        int? take = null,
        bool tracking = false
    )
    {
        IQueryable<TEntity> query = _dbSet;

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        if (include != null)
        {
            query = include(query);
        }

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        if (orderBy != null)
        {
            query = orderBy(query);
        }

        if (skip.HasValue && skip.Value > 0)
        {
            query = query.Skip(skip.Value);
        }

        if (take.HasValue && take.Value > 0)
        {
            query = query.Take(take.Value);
        }

        return await query.ToListAsync();
    }

    /// <summary>
    /// Check if an entity exists using a LINQ predicate, which can be translated to SQL by EF Core.
    /// </summary>
    protected async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    /// <summary>
    /// Count the number of entities that match a LINQ predicate, or all entities if the predicate is null.
    /// </summary>
    protected async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null) =>
        predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);

    /// <summary>
    /// Create a new entity and save it to the database.
    /// </summary>
    protected async Task<TEntity> CreateAsync(TEntity entity, string? debugId = null)
    {
        if (entity == null)
        {
            throw new BadRequestException(
                $"{typeof(TEntity).Name} {(debugId == null ? "" : $"with ID {debugId}")}cannot be null."
            );
        }

        _logger?.LogDebug(
            "Creating new {type}{id}...",
            typeof(TEntity).Name,
            debugId == null ? "" : $" with ID {debugId}"
        );

        await _dbSet.AddAsync(entity);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully created {type}{id}!",
            typeof(TEntity).Name,
            debugId == null ? "" : $" with ID {debugId}"
        );
        return entity;
    }

    /// <summary>
    /// Create multiple new entities and save them to the database.
    /// </summary>
    protected async Task<IReadOnlyList<TEntity>> CreateRangeAsync(params TEntity[] entities)
    {
        if (entities == null || entities.Length == 0)
        {
            return [];
        }

        _logger?.LogDebug(
            "Creating {count} new {type} entities",
            entities.Length,
            typeof(TEntity).Name
        );

        List<TEntity> entityList = [.. entities]; // materialize the list
        await _dbSet.AddRangeAsync(entityList);
        await SaveChangesWithConcurrencyCheckAsync();

        _logger?.LogInformation(
            "Successfully created {count} {type} entities",
            entities.Length,
            typeof(TEntity).Name
        );
        return entityList;
    }

    protected abstract Task<TEntity> GetTrackedEntityOrThrowAsync(TEntity entity);

    /// <summary>
    /// Convenience method to save changes to the database with concurrency checking.
    /// </summary>
    /// <returns></returns>
    protected async Task SaveChangesWithConcurrencyCheckAsync() =>
        await _context.SaveChangesWithConcurrencyCheckAsync(_logger);
}

public static class BaseRepositoryExtensions
{
    /// <summary>
    /// Save changes to the database with concurrency checking.
    /// </summary>
    /// <throws cref="ConflictException">Thrown if the save fails due to concurrency issues.</throws>
    public static async Task SaveChangesWithConcurrencyCheckAsync(
        this AppDbContext context,
        ILogger? logger = null
    )
    {
        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException e)
        {
            logger?.LogError(e, "Error saving changes: {e.Message}", e.Message);
            throw new ConflictException(e.Message);
        }
    }
}
