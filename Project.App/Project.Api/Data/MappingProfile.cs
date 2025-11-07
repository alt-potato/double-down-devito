using AutoMapper;
using Project.Api.DTOs;
using Project.Api.Models;

namespace Project.Api.Data;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDTO>();
        CreateMap<CreateUserDTO, User>();
        CreateMap<UpdateUserDTO, User>();

        CreateMap<Hand, HandDTO>().ReverseMap();
        CreateMap<Hand, HandUpdateDTO>().ReverseMap();
        CreateMap<Hand, HandPatchDTO>().ReverseMap();

        // CreateMap<Room, RoomDTO>();
        // CreateMap<CreateRoomDTO, Room>()
        //     // enforce lowercase gamemode
        //     .ForMember(
        //         dest => dest.GameMode,
        //         opt => opt.MapFrom(src => src.GameMode.ToLowerInvariant())
        //     );
        // CreateMap<UpdateRoomDTO, Room>()
        //     // enforce lowercase gamemode
        //     .ForMember(
        //         dest => dest.GameMode,
        //         opt => opt.MapFrom(src => src.GameMode.ToLowerInvariant())
        //     )
        //     // only assign if not default
        //     .ForAllMembers(opts =>
        //         opts.Condition(
        //             (_, _, srcMember) =>
        //             {
        //                 if (srcMember is string s)
        //                     return !string.IsNullOrEmpty(s);
        //                 return srcMember != default;
        //             }
        //         )
        //     );
    }
}
