using Microsoft.AspNetCore.Mvc;
using OrchardCore.Environment.Shell;

namespace Module1.Controllers;

[ApiController]
[Route("[controller]")]
public class TenantScopedDependencyController : ControllerBase
{
    private readonly ShellSettings _settings;

    public TenantScopedDependencyController(ShellSettings settings)
    {
        _settings = settings;
    }

    public string Get()
    {
        return _settings.Name;
    }
}

[ApiController]
[Route("[controller]")]
public class TenantScopedDependency2Controller : ControllerBase
{
    public string Get([FromServices] ShellSettings settings)
    {
        return settings.Name;
    }
}