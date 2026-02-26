using Zad2.Core.DTOs;
using Zad2.Core.Interfaces;

namespace Zad2.Core.Services;

public class SearchService : ISearchService
{
    private readonly IRickAndMortyClient _client;

    public SearchService(IRickAndMortyClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<SearchResultDto>> SearchAsync(
        string term, 
        int? limit = null, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return [];
        }

        // Run all three API calls concurrently
        var charactersTask = _client.GetAllCharactersAsync(term, cancellationToken);
        var locationsTask = _client.GetAllLocationsAsync(term, cancellationToken);
        var episodesTask = _client.GetAllEpisodesAsync(term, cancellationToken);

        await Task.WhenAll(charactersTask, locationsTask, episodesTask);

        var characters = await charactersTask;
        var locations = await locationsTask;
        var episodes = await episodesTask;

        // Map to DTOs and combine
        var results = characters
            .Select(c => new SearchResultDto(c.Name, "character", c.Url))
            .Concat(locations.Select(l => new SearchResultDto(l.Name, "location", l.Url)))
            .Concat(episodes.Select(e => new SearchResultDto(e.Name, "episode", e.Url)));

        // Apply limit if provided
        if (limit.HasValue && limit.Value > 0)
        {
            results = results.Take(limit.Value);
        }

        return results.ToList();
    }
}