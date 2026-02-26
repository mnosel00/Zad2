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
        Assert.Equal(3, results[0].Episodes);
        Assert.Equal(2, results[1].Episodes);
        Assert.Equal(1, results[2].Episodes);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldFilterByMinAndMax()
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
        Assert.Equal(3, results[0].Episodes);
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

        // Assert
        _mockClient.Verify(
            c => c.GetCharactersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetTopPairsAsync_ShouldReturnDefaultLimitOf20_WhenLimitIsNull()
    {
        // Arrange
        var characterUrls = Enumerable.Range(1, 8)
            .Select(i => $"https://rickandmortyapi.com/api/character/{i}")
            .ToList();

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
        var episodes = new List<Episode>();

        for (var i = 1; i <= 5; i++)
        {
            episodes.Add(new Episode(i, $"Episode {i}", $"S01E{i:D2}", $"url{i}",
                new[] { CharacterAUrl, CharacterBUrl }));
        }

        for (var i = 6; i <= 15; i++)
        {
            episodes.Add(new Episode(i, $"Episode {i}", $"S01E{i:D2}", $"url{i}",
                new[] { CharacterBUrl, CharacterCUrl }));
        }

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
        Assert.Equal(10, results[0].Episodes);
        Assert.Equal(5, results[1].Episodes);
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