using Microsoft.AspNetCore.Mvc;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace MultiMillionaire.Controllers;

[Route("multiplayer")]
public class MultiplayerController : Controller
{
    private readonly IDetectionService _detectionService;

    public MultiplayerController(IDetectionService detectionService)
    {
        _detectionService = detectionService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("host")]
    public IActionResult Host()
    {
        ViewData["IsMobile"] = _detectionService.Device.Type == Device.Mobile;
        return View();
    }

    [HttpGet("audience")]
    public IActionResult Audience()
    {
        ViewData["IsMobile"] = _detectionService.Device.Type == Device.Mobile;
        return View();
    }

    [HttpGet("spectate")]
    public IActionResult Spectate()
    {
        ViewData["IsMobile"] = _detectionService.Device.Type == Device.Mobile;
        return View();
    }
}