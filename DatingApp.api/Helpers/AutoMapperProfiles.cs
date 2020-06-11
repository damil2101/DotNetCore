using AutoMapper;
using DatingApp.api.Dtos;
using DatingApp.api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace DatingApp.api.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User,UserForListDto>()
                .ForMember(dest=>dest.PhotoUrl, 
                            opt => opt.MapFrom(source => source.Photos.FirstOrDefault(p=>p.IsMain).Url))
                .ForMember(dest => dest.Age,
                            opt => opt.MapFrom(source => source.DOB.CalculateAge()));            
            CreateMap<User,UserForDetailedDto>()
                .ForMember(dest=>dest.PhotoUrl, 
                            opt => opt.MapFrom(source => source.Photos.FirstOrDefault(p=>p.IsMain).Url))
                .ForMember(dest => dest.Age,
                            opt => opt.MapFrom(source => source.DOB.CalculateAge()));            
            CreateMap<Photo,PhotosForDetailedDto>();

            CreateMap<UserForUpdateDto,User>();
            CreateMap<Photo,PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto,Photo>();
            CreateMap<UserForRegisterDto,User>();
            CreateMap<MessageForCreationDto,Message>().ReverseMap();

            CreateMap<Message,MessageToReturnDto>()
                    .ForMember(dest=>dest.SenderPhotoUrl, 
                                opt => opt.MapFrom(source=>source.Sender.Photos.FirstOrDefault(p=>p.IsMain).Url))
                    .ForMember(dest=>dest.RecipientPhotoUrl, 
                                opt => opt.MapFrom(source=>source.Recipient.Photos.FirstOrDefault(p=>p.IsMain).Url));            

        }
    }
}