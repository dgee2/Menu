using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace MenuApi.Integration.Tests.Factory;

public class IntegrationBaseClass
{
    protected WebApplicationFactory<Program> Factory { get; private set; }

    public IntegrationBaseClass()
    {
        Factory = new WebApplicationFactory<Program>();
    }
}