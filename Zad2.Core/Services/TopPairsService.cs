using Zad2.Core.DTOs;
using Zad2.Core.Interfaces;
using Zad2.Core.Models;

namespace Zad2.Core.Services;

public class TopPairsService : ITopPairsService
{
    private const int DefaultLimit = 20;
    private readonly IRickAndMortyClient _client;

    public TopPairsService(IRickAndMortyClient client)
    {
        _client = client;
    }

    public async Task<IEnumerable<TopPairDto>> GetTopPairsAsync(
        int? min = null, 
        int? max = null, 
        int? limit = null, 
        CancellationToken cancellationToken = default)
    {
        // Step 1: Fetch all episodes
        var episodes = await _client.GetAllEpisodesAsync(cancellationToken: cancellationToken);

        // Step 2 & 3: Generate pairs and count frequency
        var pairCounts = CountCharacterPairs(episodes);

        // Step 4: Filter by min/max
        var filteredPairs = pairCounts.AsEnumerable();

        if (min.HasValue)
        {
            filteredPairs = filteredPairs.Where(p => p.Value >= min.Value);
        }

        if (max.HasValue)
        {
            filteredPairs = filteredPairs.Where(p => p.Value <= max.Value);
        }

        // Step 5 & 6: Order by shared episodes and apply limit
        var effectiveLimit = limit ?? DefaultLimit;
        var topPairs = filteredPairs
            .OrderByDescending(p => p.Value)
            .Take(effectiveLimit)
            .ToList();

        if (topPairs.Count == 0)
        {
            return [];
        }

        // Step 7: Extract unique character IDs and fetch in a single call
        var uniqueCharacterIds = topPairs
            .SelectMany(p => new[] { p.Key.Id1, p.Key.Id2 })
            .Distinct()
            .ToList();

        var characters = await _client.GetCharactersByIdsAsync(uniqueCharacterIds, cancellationToken);
        var characterLookup = characters.ToDictionary(c => c.Id);

        // Step 8: Map to DTOs
        var result = new List<TopPairDto>();

        foreach (var pair in topPairs)
        {
            if (!characterLookup.TryGetValue(pair.Key.Id1, out var char1) ||
                !characterLookup.TryGetValue(pair.Key.Id2, out var char2))
            {
                continue; // Skip if character data is missing
            }

            result.Add(new TopPairDto(
                new CharacterBaseDto(char1.Name, char1.Url),
                new CharacterBaseDto(char2.Name, char2.Url),
                pair.Value
            ));
        }

        return result;
    }

    private static Dictionary<CharacterPair, int> CountCharacterPairs(IReadOnlyList<Episode> episodes)
    {
        var pairCounts = new Dictionary<CharacterPair, int>();

        foreach (var episode in episodes)
        {
            var characterIds = episode.Characters
                .Select(ExtractIdFromUrl)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .Distinct()
                .OrderBy(id => id)
                .ToList();

            // Generate all unique unordered pairs
            for (var i = 0; i < characterIds.Count; i++)
            {
                for (var j = i + 1; j < characterIds.Count; j++)
                {
                    var pair = new CharacterPair(characterIds[i], characterIds[j]);
                    
                    if (pairCounts.TryGetValue(pair, out var count))
                    {
                        pairCounts[pair] = count + 1;
                    }
                    else
                    {
                        pairCounts[pair] = 1;
                    }
                }
            }
        }

        return pairCounts;
    }

    private static int? ExtractIdFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        var lastSlashIndex = url.LastIndexOf('/');
        if (lastSlashIndex < 0 || lastSlashIndex == url.Length - 1)
        {
            return null;
        }

        var idPart = url[(lastSlashIndex + 1)..];
        return int.TryParse(idPart, out var id) ? id : null;
    }

    /// <summary>
    /// Represents an unordered pair of character IDs.
    /// IDs are stored in ascending order to ensure pair equality regardless of order.
    /// </summary>
    private readonly record struct CharacterPair
    {
        public int Id1 { get; }
        public int Id2 { get; }

        public CharacterPair(int id1, int id2)
        {
            // Always store in ascending order for consistent equality
            if (id1 <= id2)
            {
                Id1 = id1;
                Id2 = id2;
            }
            else
            {
                Id1 = id2;
                Id2 = id1;
            }
        }
    }
}