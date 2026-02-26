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
}