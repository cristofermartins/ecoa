using Microsoft.EntityFrameworkCore;
using Ecoa.Core.Entities;
using Ecoa.Core.Ports;

namespace Ecoa.Infrastructure.Data;

public class RideRepository : IRideRepository
{
    private readonly AppDbContext _context;

    public RideRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ride?> GetByIdAsync(Guid id)
        => await _context.Rides
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Ride?> GetByIdWithPointsAsync(Guid id)
        => await _context.Rides
            .Include(r => r.User)
            .Include(r => r.Points)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<Ride?> GetByIdWithPointsAndImuAsync(Guid id)
        => await _context.Rides
            .Include(r => r.User)
            .Include(r => r.Points)
            .Include(r => r.ImuSamples)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<List<Ride>> GetByUserIdAsync(uint userId)
        => await _context.Rides
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<List<Ride>> GetPendingValidationAsync()
        => await _context.Rides
            .Include(r => r.User)
            .Where(r => r.Status == RideStatus.PendingValidation)
            .OrderByDescending(r => r.EndedAt)
            .ToListAsync();

    public async Task<Ride> CreateAsync(Ride ride)
    {
        _context.Rides.Add(ride);
        await _context.SaveChangesAsync();
        return ride;
    }

    public async Task UpdateAsync(Ride ride)
    {
        _context.Rides.Update(ride);
        await _context.SaveChangesAsync();
    }

    public async Task AddPointsAsync(List<RidePoint> points)
    {
        _context.RidePoints.AddRange(points);
        await _context.SaveChangesAsync();
    }

    public async Task<List<RidePoint>> GetPointsByRideIdAsync(Guid rideId)
        => await _context.RidePoints
            .Where(p => p.RideId == rideId)
            .OrderBy(p => p.RecordedAt)
            .ToListAsync();

    public async Task AddImuSamplesAsync(List<RideImuSample> samples)
    {
        _context.RideImuSamples.AddRange(samples);
        await _context.SaveChangesAsync();
    }

    public async Task<List<RideImuSample>> GetImuSamplesByRideIdAsync(Guid rideId)
        => await _context.RideImuSamples
            .Where(s => s.RideId == rideId)
            .OrderBy(s => s.RecordedAt)
            .ToListAsync();

    public async Task<Ride?> GetActiveByUserIdAsync(uint userId)
        => await _context.Rides
            .Include(r => r.User)
            .Include(r => r.Points)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.Status == RideStatus.Active);
}
