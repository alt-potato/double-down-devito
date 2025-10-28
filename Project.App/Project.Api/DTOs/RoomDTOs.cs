using FluentValidation;

namespace Project.Api.DTOs;

public class RoomDTO
{
    public Guid Id { get; set; }
    public Guid HostId { get; set; }
    public bool IsPublic { get; set; }
    public string GameMode { get; set; } = string.Empty;
    public string GameState { get; set; } = string.Empty;
    public string GameConfig { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPlayers { get; set; }
    public int MinPlayers { get; set; }
    public string DeckId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateRoomDTO
{
    public Guid HostId { get; set; }
    public bool IsPublic { get; set; } = true;
    public string GameMode { get; set; } = string.Empty;
    public string GameConfig { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxPlayers { get; set; } = 10;
    public int MinPlayers { get; set; } = 1;
}

public class UpdateRoomDTO : CreateRoomDTO
{
    public Guid Id { get; set; }
}

public class CreateRoomDtoValidator : AbstractValidator<CreateRoomDTO>
{
    public CreateRoomDtoValidator()
    {
        RuleFor(x => x.MinPlayers)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Minimum players must be at least 1.");

        RuleFor(x => x.MaxPlayers)
            .GreaterThanOrEqualTo(x => x.MinPlayers)
            .WithMessage("Maximum players must be greater than or equal to the minimum players.");

        RuleFor(x => x.GameMode).NotEmpty().WithMessage("Game mode is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description can't be longer than 500 characters.");
    }
}

public class UpdateRoomDtoValidator : AbstractValidator<UpdateRoomDTO>
{
    public UpdateRoomDtoValidator()
    {
        // inherit all rules from CreateRoomDtoValidator
        Include(new CreateRoomDtoValidator());

        RuleFor(x => x.Id).NotEmpty().WithMessage("A Room ID is required to perform an update.");
    }
}
