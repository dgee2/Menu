using AutoMapper;

namespace MenuApi.MappingProfiles
{
    public class ViewModelProfile : Profile
    {
        public ViewModelProfile()
        {
            CreateMap<DBModel.Ingredient, ViewModel.Ingredient>();
            CreateMap<DBModel.Recipe, ViewModel.Recipe>();
            CreateMap<DBModel.Recipe, ViewModel.FullRecipe>();
            CreateMap<DBModel.RecipeIngredient, ViewModel.RecipeIngredient>().ReverseMap();
        }
    }
}
