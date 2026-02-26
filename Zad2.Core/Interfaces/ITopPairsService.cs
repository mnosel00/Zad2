using Zad2.Core.DTOs;

namespace Zad2.Core.Interfaces;

public interface ITopPairsService
{
    Task<IEnumerable<TopPairDto>> GetTopPairsAsync(int? min = null, int? max = null, int? limit = null, CancellationToken cancellationToken = default);
}