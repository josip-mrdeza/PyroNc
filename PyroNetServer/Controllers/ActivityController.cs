using Microsoft.AspNetCore.Mvc;

namespace PyroNetServer.Controllers;

[ApiController]
[Route("")]
public class ActivityController : ControllerBase
{
    [HttpGet]
    public void Index()
    {
        
    }
}