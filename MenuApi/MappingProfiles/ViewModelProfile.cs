using System;
using AutoMapper;

namespace MenuApi.MappingProfiles
{
    public class ViewModelProfile : Profile
    {
        public ViewModelProfile()
        {
            CreateMap<ViewModel.Ingredient, DBModel.Ingredient>().ReverseMap();
        }
    }
}
