using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecoa.Core.Services;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/pool")]
[Authorize]
public class PoolController : ControllerBase
{
    private readonly PoolService _poolService;

    public PoolController(PoolService poolService)
    {
        _poolService = poolService;
    }

    public record AddIncentiveRequest(string Name, string Description, long Price, string Code, string Provider);

    [HttpGet]
    public async Task<IActionResult> GetPool()
    {
        var result = await _poolService.GetDefaultPoolAsync();
        return Ok(result.Value);
    }

    [HttpGet("incentives")]
    public async Task<IActionResult> GetIncentives()
    {
        var result = await _poolService.GetAllIncentivesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpGet("incentives/available")]
    public async Task<IActionResult> GetAvailableIncentives()
    {
        var result = await _poolService.GetAvailableIncentivesAsync();
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(result.Value);
    }

    [HttpPost("incentives")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddIncentive([FromBody] AddIncentiveRequest request)
    {
        var result = await _poolService.AddIncentiveAsync(
            request.Name, request.Description, request.Price, request.Code, request.Provider);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
        return Ok(new { txHash = result.Value });
    }
}
