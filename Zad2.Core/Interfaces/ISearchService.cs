using Zad2.Core.DTOs;

namespace Zad2.Core.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<SearchResultDto>> SearchAsync(string term, int? limit = null, CancellationToken cancellationToken = default);
}