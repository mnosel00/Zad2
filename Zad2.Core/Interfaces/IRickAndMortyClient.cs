using Zad2.Core.Models;

namespace Zad2.Core.Interfaces;

public interface IRickAndMortyClient
{
    Task<PaginatedResponse<Character>> GetCharactersAsync(string? name = null, int? page = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Character>> GetAllCharactersAsync(string? name = null, CancellationToken cancellationToken = default);
    
    Task<Character?> GetCharacterByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<Location>> GetLocationsAsync(string? name = null, int? page = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Location>> GetAllLocationsAsync(string? name = null, CancellationToken cancellationToken = default);
    
    Task<Location?> GetLocationByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PaginatedResponse<Episode>> GetEpisodesAsync(string? name = null, int? page = null, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Episode>> GetAllEpisodesAsync(string? name = null, CancellationToken cancellationToken = default);
    
    Task<Episode?> GetEpisodeByIdAsync(int id, CancellationToken cancellationToken = default);
}