using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecoa.Core.Services;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/actions")]
[Authorize]
public class ActionController : ControllerBase
{
    private readonly ActionService _actionService;

    public ActionController(ActionService actionService)
    {
        _actionService = actionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyActions()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _actionService.GetUserActionsAsync(userId.Value);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAction(uint id)
    {
        var result = await _actionService.GetActionAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        var result = await _actionService.GetPendingActionsAsync();
        return Ok(result.Value);
    }

    private uint? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (uint.TryParse(claim, out var id))
            return id;
        return null;
    }
}
