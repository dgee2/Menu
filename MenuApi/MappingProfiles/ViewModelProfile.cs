using AutoMapper;

namespace MenuApi.MappingProfiles;

public class ViewModelProfile : Profile
{
    public ViewModelProfile()
    {
        CreateMap<DBModel.Ingredient, ViewModel.Ingredient>();
        CreateMap<DBModel.Recipe, ViewModel.Recipe>();
        CreateMap<DBModel.Recipe, ViewModel.FullRecipe>();
        CreateMap<DBModel.RecipeIngredient, ViewModel.RecipeIngredient>();
        CreateMap<ViewModel.RecipeIngredient, DBModel.RecipeIngredient>()
            .ForCtorParam("IngredientName", o => o.MapFrom(s => s.Name))
            .ForCtorParam("UnitName", o => o.MapFrom(s => s.Unit));

        CreateMap<DBModel.GetRecipeIngredient, ViewModel.RecipeIngredient>()
            .ForMember(x => x.Name, x => x.MapFrom(o => o.IngredientName))
            .ForMember(x => x.Unit, x => x.MapFrom(o => o.UnitName));
    }
}
