using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Project.Api.Data;
using Project.Api.Repositories.Interface;

namespace Project.Test.Helpers;

public class RepositoryTestBase<TRepo, TEntity> : IDisposable
    where TRepo : RepositoryBase<TEntity>
    where TEntity : class
{
    protected readonly AppDbContext _context;
    protected readonly TRepo _rut; // repository under test :)

    /// <summary>
    /// Base class for integration tests of repositories, providing an in-memory database and a mocked logger.
    /// </summary>
    public RepositoryTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"testdb_{Guid.CreateVersion7()}")
            .AddInterceptors(new RowVersionInterceptor())
            .Options;

        _context = new AppDbContext(options);

        var mockLogger = new Mock<ILogger<TRepo>>().Object;
        _rut = (TRepo)Activator.CreateInstance(typeof(TRepo), _context, mockLogger)!;
    }

    /// <summary>
    /// Seeds the in-memory database using the provided entity.
    /// </summary>
    protected async Task<T> SeedData<T>(T entity)
        where T : class
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync();

        return entity;
    }

    /// <summary>
    /// Seeds the in-memory database with the provided entities.
    /// </summary>
    protected async Task<T[]> SeedData<T>(params T[] entities)
        where T : class
    {
        _context.Set<T>().AddRange(entities);
        await _context.SaveChangesAsync();

        return entities;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            // clear database after each test
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
