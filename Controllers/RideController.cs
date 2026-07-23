using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ecoa.Core.Services;

namespace Ecoa.Controllers;

[ApiController]
[Route("api/rides")]
[Authorize]
public class RideController : ControllerBase
{
    private readonly RideService _rideService;

    public RideController(RideService rideService)
    {
        _rideService = rideService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartRide()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _rideService.StartRideAsync(userId.Value);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    public record AddPointsRequest(List<RideService.AddPointRequest> Points);

    [HttpPost("{id}/points")]
    public async Task<IActionResult> AddPoints(Guid id, [FromBody] AddPointsRequest request)
    {
        var result = await _rideService.AddPointsAsync(id, request.Points);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true });
    }

    public record AddImuSamplesRequest(List<RideService.AddImuSampleRequest> Samples);

    [HttpPost("{id}/imu")]
    public async Task<IActionResult> AddImuSamples(Guid id, [FromBody] AddImuSamplesRequest request)
    {
        var result = await _rideService.AddImuSamplesAsync(id, request.Samples);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { success = true });
    }

    [HttpGet("{id}/imu")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRideImuSamples(Guid id)
    {
        var result = await _rideService.GetImuSamplesAsync(id);
        return Ok(result.Value);
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopRide(Guid id)
    {
        var result = await _rideService.StopRideAsync(id);
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRide(Guid id)
    {
        var result = await _rideService.GetRideAsync(id);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id}/points")]
    public async Task<IActionResult> GetRidePoints(Guid id)
    {
        var result = await _rideService.GetRidePointsAsync(id);
        return Ok(result.Value);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyRides()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _rideService.GetUserRidesAsync(userId.Value);
        return Ok(result.Value);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActiveRide()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _rideService.GetActiveRideAsync(userId.Value);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPendingRides()
    {
        var result = await _rideService.GetPendingRidesAsync();
        return Ok(result.Value);
    }

    public record ValidateRideRequest(bool Approved, string? Notes);

    [HttpPost("{id}/validate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ValidateRide(Guid id, [FromBody] ValidateRideRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _rideService.ValidateRideAsync(id, request.Approved, userId.Value, request.Notes);
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
