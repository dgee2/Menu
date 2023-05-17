using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace MenuApi.Integration.Tests.Factory;

public class IntegrationBaseClass
{
    protected WebApplicationFactory<Startup> Factory { get; private set; }

    [OneTimeSetUp]
    public void Setup()
    {
        Factory = new WebApplicationFactory<Startup>();
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        Factory?.Dispose();
    }
}
