# Coding Conventions

This is a living document open to discussion! Make a pull request!

## Backend

### General

- File-scoped namespaces are preferred to block-scoped namespaces.
- Methods that return a collection of entities should use an `IReadOnlyList<T>`, or a `List<T>` if mutable.
- Methods that create/update/delete entities should return the created/modified/deleted entity.

### Service Layer

- Methods should be named after the business operation they perform.
  - eg. `CreateGameAsync`, `UpdateGamestateAsync`
- Methods with inputs should take in a DTO, map it to the corresponding object if necessary, and call the repository layer.

### Repository layer

- Method naming should be data-centric, ie. `{verb}(All)(By{query parameters})Async`.
  - eg. `GetAsync`, `GetByIdAsync`, `GetAllByGameIdAsync`
- The way a repository signals an entity was not found should be consistent.
  - For queries that retrieve entities (Get, Find, etc.):
    - If retrieving a single entity, should return a nullable type.
    - If retrieving a collection of entities, should return an empty list.
  - For queries that modify entities (Update, Patch, Delete, etc.):
    - Throw an appropriate `ApiException`.
- Models are encouraged to use rowversion as a simple way to implement optimistic concurrency control.

  ```cs
  public class Cat
  {
      /* ... */

      public byte[] RowVersion { get; set; } = []
  }

  public class CatConfiguration : IEntityTypeConfiguration<Cat>
  {
      public void Configure(EntityTypeBuilder<Cat> builder)
      {
          /* ... */

          builder.Property(r => r.RowVersion).IsRowVersion();
      }
  }
  ```

### Tests

- Test files should end in "Tests" instead of "Test" when it contains more than one test.
