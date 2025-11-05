namespace Project.Api.Models.Interfaces;

public interface ITimestamped
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
