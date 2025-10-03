using Microsoft.AspNetCore.Mvc;
using SystemHealthDashboard.Services;

namespace SystemHealthDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : ControllerBase
{
    private readonly DummyService _dummyService;

    public HelloController(DummyService dummyService)
    {
        _dummyService = dummyService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var message = _dummyService.GetMessage();
        return Ok(new { message });
    }
}
