using AutoFixture;
using AutoFixture.Xunit3;

namespace MenuApi.Integration.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ShortStringAutoDataAttribute : AutoDataAttribute
{
    public ShortStringAutoDataAttribute() : base(() =>
    {
        var fixture = new Fixture
        {
            RepeatCount = 0,
        };

        fixture.Register(() => Guid.NewGuid().ToString("N"));
        return fixture;
    })
    {
    }
}
