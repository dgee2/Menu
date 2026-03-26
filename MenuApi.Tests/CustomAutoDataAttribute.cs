using AutoFixture;
using AutoFixture.Xunit3;

namespace MenuApi.Tests;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CustomAutoDataAttribute : AutoDataAttribute
{
    public CustomAutoDataAttribute() : base(() =>
    {
        var fixture = new Fixture();
        fixture.Customizations.Add(new ValueObjectSpecimenBuilder());
        return fixture;
    })
    {
    }
}
