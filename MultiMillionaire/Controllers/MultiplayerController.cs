using Microsoft.AspNetCore.Mvc;
using MultiMillionaire.Models;

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
    public IActionResult Spectate([FromForm] MultiplayerGame game)
    {
        return View(game);
    }
}