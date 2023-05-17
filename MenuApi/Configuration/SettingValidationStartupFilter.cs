namespace MenuApi.Configuration;

public class SettingValidationStartupFilter : IStartupFilter
{
    private readonly IEnumerable<IValidatable> validatableObjects;

    public SettingValidationStartupFilter(IEnumerable<IValidatable> validatableObjects)
    {
        this.validatableObjects = validatableObjects;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        foreach (var validatableObject in validatableObjects)
        {
            validatableObject.Validate();
        }

        // don't alter the configuration
        return next;
    }
}
