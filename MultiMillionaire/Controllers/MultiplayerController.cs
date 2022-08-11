using Microsoft.AspNetCore.Mvc;

namespace MultiMillionaire.Controllers;

[Route("multiplayer")]
public class MultiplayerController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("host")]
    public IActionResult Host()
    {
        return View();
    }

    [HttpPost("spectate")]
    public IActionResult Spectate()
    {
        return View();
    }
}