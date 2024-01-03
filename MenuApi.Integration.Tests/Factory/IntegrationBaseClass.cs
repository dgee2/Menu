using Microsoft.AspNetCore.Mvc.Testing;

namespace MenuApi.Integration.Tests.Factory;

public class IntegrationBaseClass
{
    protected WebApplicationFactory<Program> Factory { get; private set; }

    public IntegrationBaseClass()
    {
        Factory = new WebApplicationFactory<Program>();
    }
}