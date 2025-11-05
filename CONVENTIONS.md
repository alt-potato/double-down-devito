# Coding Conventions

This is a living document open to discussion! Make a pull request!

## Backend

### General

- File-scoped namespaces are preferred to block-scoped namespaces.

### Repository layer

- Methods should be consistently named in the format `{verb}(All)(By{query parameters})Async`.
  - eg. `GetAsync`, `GetByIdAsync`, `GetByGameIdAsync`
- Methods that return a collection of entities should use a `List<T>` or `IReadOnlyList<T>`.
- The way a repository signals an entity was not found should be consistent.
  - For queries that retrieve entities (Get, Find, etc.):
    - If retrieving a single entity, should return a nullable type.
    - If retrieving a collection of entities, should return an empty list.
  - For queries that modify entities (Update, Patch, Delete, etc.):
    - Throw an appropriate `Exception`.
- Methods that create/update/delete entities should return the created/modified/deleted entity.
- Models are encouraged to use rowversion as a simple way to implement optimistic concurrency control.

  ```cs
  public class Game
  {
      // ...
      public byte[] RowVersion { get; set; } = []
  }

  public class GameConfiguration : IEntityTypeConfiguration<Game>
  {
      public void Configure(EntityTypeBuilder<Game> builder)
      {
          // ...

          builder.Property(r => r.RowVersion).IsRowVersion();
      }
  }
  ```

### Tests

- Test files should end in "Tests" instead of "Test" when it contains more than one test.
