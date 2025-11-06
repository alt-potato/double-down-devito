namespace Project.Test.Helpers.Builders;

public interface IBuilder<T>
    where T : class
{
    /// <summary>
    /// Build and return the constructed object.
    /// </summary>
    T Build();
}
