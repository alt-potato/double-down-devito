namespace Project.Api.Models.Interface;

public interface IEntity<TKey>
    where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; }
    public byte[] RowVersion { get; set; }
}
