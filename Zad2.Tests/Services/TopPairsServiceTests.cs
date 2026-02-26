using Moq;
using Xunit;
using Zad2.Core.Interfaces;
using Zad2.Core.Models;
using Zad2.Core.Services;

namespace Zad2.Tests.Services;

public class TopPairsServiceTests
{
    private readonly Mock<IRickAndMortyClient> _mockClient;
    private readonly TopPairsService _sut;

    // Character URL constants for readability
    private const string CharacterAUrl = "https://rickandmortyapi.com/api/character/1";
    private const string CharacterBUrl = "https://rickandmortyapi.com/api/character/2";
    private const string CharacterCUrl = "https://rickandmortyapi.com/api/character/3";

    public TopPairsServiceTests()
    {
        _mockClient = new Mock<IRickAndMortyClient>();
        _sut = new TopPairsService(_mockClient.Object);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldCorrectlyCountAndOrderPairs()
    {
        // Arrange
        // Episode setup:
        // - Episode 1: A, B -> pair (A,B)
        // - Episode 2: A, B -> pair (A,B)
        // - Episode 3: A, B, C -> pairs (A,B), (A,C), (B,C)
        // - Episode 4: A, C -> pair (A,C)
        // Result: A-B = 3, A-C = 2, B-C = 1
        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "url1", new[] { CharacterAUrl, CharacterBUrl }),
            new(2, "Episode 2", "S01E02", "url2", new[] { CharacterAUrl, CharacterBUrl }),
            new(3, "Episode 3", "S01E03", "url3", new[] { CharacterAUrl, CharacterBUrl, CharacterCUrl }),
            new(4, "Episode 4", "S01E04", "url4", new[] { CharacterAUrl, CharacterCUrl })
        };

        var characters = new List<Character>
        {
            new(1, "Character A", "Type", CharacterAUrl, []),
            new(2, "Character B", "Type", CharacterBUrl, []),
            new(3, "Character C", "Type", CharacterCUrl, [])
        };

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        // Act
        var results = (await _sut.GetTopPairsAsync()).ToList();

        // Assert
        Assert.Equal(3, results.Count);

        // First pair should be A-B with 3 shared episodes
        Assert.Equal(3, results[0].Episodes);
        Assert.Contains("Character A", new[] { results[0].Character1.Name, results[0].Character2.Name });
        Assert.Contains("Character B", new[] { results[0].Character1.Name, results[0].Character2.Name });

        // Second pair should be A-C with 2 shared episodes
        Assert.Equal(2, results[1].Episodes);
        Assert.Contains("Character A", new[] { results[1].Character1.Name, results[1].Character2.Name });
        Assert.Contains("Character C", new[] { results[1].Character1.Name, results[1].Character2.Name });

        // Third pair should be B-C with 1 shared episode
        Assert.Equal(1, results[2].Episodes);
        Assert.Contains("Character B", new[] { results[2].Character1.Name, results[2].Character2.Name });
        Assert.Contains("Character C", new[] { results[2].Character1.Name, results[2].Character2.Name });
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldFilterByMinAndMax()
    {
        // Arrange
        // Same setup: A-B = 3, A-C = 2, B-C = 1
        // Filter: min = 2, max = 2 -> Only A-C should be returned
        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "url1", new[] { CharacterAUrl, CharacterBUrl }),
            new(2, "Episode 2", "S01E02", "url2", new[] { CharacterAUrl, CharacterBUrl }),
            new(3, "Episode 3", "S01E03", "url3", new[] { CharacterAUrl, CharacterBUrl, CharacterCUrl }),
            new(4, "Episode 4", "S01E04", "url4", new[] { CharacterAUrl, CharacterCUrl })
        };

        var characters = new List<Character>
        {
            new(1, "Character A", "Type", CharacterAUrl, []),
            new(3, "Character C", "Type", CharacterCUrl, [])
        };

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        // Act
        var results = (await _sut.GetTopPairsAsync(min: 2, max: 2)).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(2, results[0].Episodes);
        Assert.Contains("Character A", new[] { results[0].Character1.Name, results[0].Character2.Name });
        Assert.Contains("Character C", new[] { results[0].Character1.Name, results[0].Character2.Name });
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldApplyLimit()
    {
        // Arrange
        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "url1", new[] { CharacterAUrl, CharacterBUrl }),
            new(2, "Episode 2", "S01E02", "url2", new[] { CharacterAUrl, CharacterBUrl }),
            new(3, "Episode 3", "S01E03", "url3", new[] { CharacterAUrl, CharacterBUrl, CharacterCUrl }),
            new(4, "Episode 4", "S01E04", "url4", new[] { CharacterAUrl, CharacterCUrl })
        };

        var characters = new List<Character>
        {
            new(1, "Character A", "Type", CharacterAUrl, []),
            new(2, "Character B", "Type", CharacterBUrl, [])
        };

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        // Act
        var results = (await _sut.GetTopPairsAsync(limit: 1)).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(3, results[0].Episodes); // Top pair (A-B) with 3 episodes
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldReturnEmptyList_WhenNoEpisodes()
    {
        // Arrange
        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Episode>());

        // Act
        var results = await _sut.GetTopPairsAsync();

        // Assert
        Assert.Empty(results);
        _mockClient.Verify(
            c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), 
            Times.Never);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldMakeSingleCallForCharacterDetails()
    {
        // Arrange
        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "url1", new[] { CharacterAUrl, CharacterBUrl, CharacterCUrl })
        };

        var characters = new List<Character>
        {
            new(1, "Character A", "Type", CharacterAUrl, []),
            new(2, "Character B", "Type", CharacterBUrl, []),
            new(3, "Character C", "Type", CharacterCUrl, [])
        };

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        // Act
        await _sut.GetTopPairsAsync();

        // Assert - Verify batch call is made exactly once (no N+1 problem)
        _mockClient.Verify(
            c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnAllResults_WhenLimitIsNull()
    {
        // Arrange
        const string searchTerm = "test";

        var characters = Enumerable.Range(1, 5)
            .Select(i => new Character(i, $"Character {i}", "Type", $"https://rickandmortyapi.com/api/character/{i}", []))
            .ToList();

        var locations = Enumerable.Range(1, 5)
            .Select(i => new Location(i, $"Location {i}", "Type", $"https://rickandmortyapi.com/api/location/{i}"))
            .ToList();

        var episodes = Enumerable.Range(1, 5)
            .Select(i => new Episode(i, $"Episode {i}", $"S01E0{i}", $"https://rickandmortyapi.com/api/episode/{i}", []))
            .ToList();

        _mockClient
            .Setup(c => c.GetAllCharactersAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        _mockClient
            .Setup(c => c.GetAllLocationsAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(locations);

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        // Act
        var results = (await _sut.SearchAsync(searchTerm, limit: null)).ToList();

        // Assert
        Assert.Equal(15, results.Count);
        Assert.Equal(5, results.Count(r => r.Type == "character"));
        Assert.Equal(5, results.Count(r => r.Type == "location"));
        Assert.Equal(5, results.Count(r => r.Type == "episode"));
    }

    [Fact]
    public async Task SearchAsync_ShouldPassCancellationTokenToClient()
    {
        // Arrange
        const string searchTerm = "test";
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        _mockClient
            .Setup(c => c.GetAllCharactersAsync(searchTerm, expectedToken))
            .ReturnsAsync(new List<Character>())
            .Verifiable();

        _mockClient
            .Setup(c => c.GetAllLocationsAsync(searchTerm, expectedToken))
            .ReturnsAsync(new List<Location>())
            .Verifiable();

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(searchTerm, expectedToken))
            .ReturnsAsync(new List<Episode>())
            .Verifiable();

        // Act
        await _sut.SearchAsync(searchTerm, cancellationToken: expectedToken);

        // Assert
        _mockClient.Verify(c => c.GetAllCharactersAsync(searchTerm, expectedToken), Times.Once);
        _mockClient.Verify(c => c.GetAllLocationsAsync(searchTerm, expectedToken), Times.Once);
        _mockClient.Verify(c => c.GetAllEpisodesAsync(searchTerm, expectedToken), Times.Once);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldReturnDefaultLimitOf20_WhenLimitIsNull()
    {
        // Arrange
        // Create 8 characters to generate 28 unique pairs (8 * 7 / 2 = 28)
        var characterUrls = Enumerable.Range(1, 8)
            .Select(i => $"https://rickandmortyapi.com/api/character/{i}")
            .ToList();

        // Single episode with all 8 characters creates all 28 pairs
        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "url1", characterUrls)
        };

        var characters = Enumerable.Range(1, 8)
            .Select(i => new Character(i, $"Character {i}", "Type", characterUrls[i - 1], []))
            .ToList();

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        // Act
        var results = (await _sut.GetTopPairsAsync(limit: null)).ToList();

        // Assert
        Assert.Equal(20, results.Count);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldIncludeInclusiveBoundaries_WhenMinAndMaxProvided()
    {
        // Arrange
        // Setup: A-B = 5 episodes, B-C = 10 episodes, A-C = 15 episodes
        // Filter: min = 5, max = 10 -> Should return A-B (5) and B-C (10)
        var episodes = new List<Episode>();

        // Episodes 1-5: A and B together (A-B = 5)
        for (var i = 1; i <= 5; i++)
        {
            episodes.Add(new Episode(i, $"Episode {i}", $"S01E{i:D2}", $"url{i}",
                new[] { CharacterAUrl, CharacterBUrl }));
        }

        // Episodes 6-15: B and C together (B-C = 10)
        for (var i = 6; i <= 15; i++)
        {
            episodes.Add(new Episode(i, $"Episode {i}", $"S01E{i:D2}", $"url{i}",
                new[] { CharacterBUrl, CharacterCUrl }));
        }

        // Episodes 16-30: A and C together (A-C = 15)
        for (var i = 16; i <= 30; i++)
        {
            episodes.Add(new Episode(i, $"Episode {i}", $"S02E{i - 15:D2}", $"url{i}",
                new[] { CharacterAUrl, CharacterCUrl }));
        }

        var characters = new List<Character>
        {
            new(1, "Character A", "Type", CharacterAUrl, []),
            new(2, "Character B", "Type", CharacterBUrl, []),
            new(3, "Character C", "Type", CharacterCUrl, [])
        };

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(episodes);

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(characters);

        // Act
        var results = (await _sut.GetTopPairsAsync(min: 5, max: 10)).ToList();

        // Assert
        Assert.Equal(2, results.Count);

        // B-C should be first (10 episodes)
        Assert.Equal(10, results[0].Episodes);
        Assert.Contains("Character B", new[] { results[0].Character1.Name, results[0].Character2.Name });
        Assert.Contains("Character C", new[] { results[0].Character1.Name, results[0].Character2.Name });

        // A-B should be second (5 episodes)
        Assert.Equal(5, results[1].Episodes);
        Assert.Contains("Character A", new[] { results[1].Character1.Name, results[1].Character2.Name });
        Assert.Contains("Character B", new[] { results[1].Character1.Name, results[1].Character2.Name });

        // Verify A-C (15 episodes) is NOT included
        Assert.DoesNotContain(results, r => r.Episodes == 15);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldPassCancellationTokenToClient()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "url1", new[] { CharacterAUrl, CharacterBUrl })
        };

        var characters = new List<Character>
        {
            new(1, "Character A", "Type", CharacterAUrl, []),
            new(2, "Character B", "Type", CharacterBUrl, [])
        };

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), expectedToken))
            .ReturnsAsync(episodes)
            .Verifiable();

        _mockClient
            .Setup(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), expectedToken))
            .ReturnsAsync(characters)
            .Verifiable();

        // Act
        await _sut.GetTopPairsAsync(cancellationToken: expectedToken);

        // Assert
        _mockClient.Verify(c => c.GetAllEpisodesAsync(It.IsAny<string?>(), expectedToken), Times.Once);
        _mockClient.Verify(c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), expectedToken), Times.Once);
    }
}