using System;
using AutoMapper;

namespace MenuApi.MappingProfiles
{
    public class ViewModelProfile : Profile
    {
        public ViewModelProfile()
        {
            CreateMap<ViewModel.Ingredient, DBModel.Ingredient>().ReverseMap();
            CreateMap<ViewModel.NewIngredient, DBModel.Ingredient>()
                .ForMember(d => d.Id, opt => opt.MapFrom(o => Guid.NewGuid()));
        }
    }
}
