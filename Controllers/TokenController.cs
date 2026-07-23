using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecoa.Core.Services;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class TokenController : ControllerBase
{
    private readonly TokenService _tokenService;

    public TokenController(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public record RedeemIncentiveRequest(ulong IncentiveId);

    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _tokenService.GetBalanceAsync(userId.Value);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _tokenService.GetTransactionsAsync(userId.Value);
        return Ok(result.Value);
    }

    [HttpGet("incentives")]
    public async Task<IActionResult> GetAvailableIncentives()
    {
        var result = await _tokenService.GetAvailableIncentivesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("redeem-incentive")]
    public async Task<IActionResult> RedeemIncentive([FromBody] RedeemIncentiveRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _tokenService.RedeemIncentiveAsync(userId.Value, request.IncentiveId);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { incentive = result.Value });
    }

    private uint? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (uint.TryParse(claim, out var id))
            return id;
        return null;
    }
}
