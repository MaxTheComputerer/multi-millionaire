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

    [HttpGet("host")]
    public IActionResult Host()
    {
        return View();
    }

    [HttpGet("audience")]
    public IActionResult Audience()
    {
        return View();
    }

    [HttpGet("spectate")]
    public IActionResult Spectate()
    {
        return View();
    }
}