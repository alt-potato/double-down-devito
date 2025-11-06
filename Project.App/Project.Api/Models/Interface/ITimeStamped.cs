namespace Project.Api.Models.Interface;

public interface ITimestamped
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
