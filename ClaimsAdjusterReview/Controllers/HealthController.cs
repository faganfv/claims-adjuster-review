using Microsoft.AspNetCore.Mvc;

namespace ClaimsAdjusterReview.Controllers;

/// <summary>
/// Health check controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "running", timestamp = DateTime.UtcNow });
    }
}
