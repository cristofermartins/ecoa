using Ecoa.Core.Entities;

namespace Ecoa.Core.Ports;

public interface IRideRepository
{
    Task<Ride?> GetByIdAsync(Guid id);
    Task<Ride?> GetByIdWithPointsAsync(Guid id);
    Task<List<Ride>> GetByUserIdAsync(uint userId);
    Task<List<Ride>> GetPendingValidationAsync();
    Task<Ride> CreateAsync(Ride ride);
    Task UpdateAsync(Ride ride);
    Task AddPointsAsync(List<RidePoint> points);
    Task<List<RidePoint>> GetPointsByRideIdAsync(Guid rideId);
    Task AddImuSamplesAsync(List<RideImuSample> samples);
    Task<List<RideImuSample>> GetImuSamplesByRideIdAsync(Guid rideId);
    Task<Ride?> GetByIdWithPointsAndImuAsync(Guid id);
    Task<Ride?> GetActiveByUserIdAsync(uint userId);
}
