using Ecoa.Core.Entities;
using Ecoa.Core.Ports;
using Ecoa.Core.Utils;

namespace Ecoa.Core.Services;

public class PoolService
{
    private readonly IPoolRepository _poolRepository;
    private readonly IStellarService _stellarService;

    public PoolService(IPoolRepository poolRepository, IStellarService stellarService)
    {
        _poolRepository = poolRepository;
        _stellarService = stellarService;
    }

    public async Task<Result<IncentivePool>> GetDefaultPoolAsync()
    {
        var pool = await _poolRepository.GetDefaultAsync();
        if (pool == null)
        {
            pool = await _poolRepository.CreateAsync(new IncentivePool
            {
                Name = "Pool Principal Ecoa"
            });
        }
        return Result<IncentivePool>.Success(pool);
    }

    public async Task<Result<string>> AddIncentiveAsync(string name, string description, long price, string code, string provider)
    {
        try
        {
            var txHash = await _stellarService.AddIncentiveAsync(name, description, price, code, provider);
            return Result<string>.Success(txHash);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Erro ao adicionar incentivo: {ex.Message}");
        }
    }

    public async Task<Result<List<IncentiveInfo>>> GetAllIncentivesAsync()
    {
        try
        {
            var incentives = await _stellarService.GetAllIncentivesAsync();
            return Result<List<IncentiveInfo>>.Success(incentives);
        }
        catch (Exception ex)
        {
            return Result<List<IncentiveInfo>>.Failure($"Erro ao listar incentivos: {ex.Message}");
        }
    }

    public async Task<Result<List<IncentiveInfo>>> GetAvailableIncentivesAsync()
    {
        try
        {
            var incentives = await _stellarService.GetAvailableIncentivesAsync();
            return Result<List<IncentiveInfo>>.Success(incentives);
        }
        catch (Exception ex)
        {
            return Result<List<IncentiveInfo>>.Failure($"Erro ao listar incentivos: {ex.Message}");
        }
    }
}
