using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecoa.Core.Services;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/validations")]
[Authorize(Roles = "Admin")]
public class ValidationController : ControllerBase
{
    private readonly ValidationService _validationService;

    public ValidationController(ValidationService validationService)
    {
        _validationService = validationService;
    }

    public record ApproveRequest(uint ActionId);
    public record RejectRequest(uint ActionId, string Notes);

    [HttpPost("approve")]
    public async Task<IActionResult> Approve([FromBody] ApproveRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _validationService.ApproveActionAsync(request.ActionId, userId.Value);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpPost("reject")]
    public async Task<IActionResult> Reject([FromBody] RejectRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _validationService.RejectActionAsync(request.ActionId, userId.Value, request.Notes);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

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
