using Moq;
using Xunit;
using Zad2.Core.Interfaces;
using Zad2.Core.Models;
using Zad2.Core.Services;

namespace Zad2.Tests.Services;

public class SearchServiceTests
{
    private readonly Mock<IRickAndMortyClient> _mockClient;
    private readonly SearchService _sut;

    public SearchServiceTests()
    {
        _mockClient = new Mock<IRickAndMortyClient>();
        _sut = new SearchService(_mockClient.Object);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnCombinedResults_WhenGivenTerm()
    {
        // Arrange
        const string searchTerm = "rick";

        var characters = new List<Character>
        {
            new(1, "Rick Sanchez", "Human", "https://rickandmortyapi.com/api/character/1", []),
            new(2, "Rick D. Sanchez III", "Human", "https://rickandmortyapi.com/api/character/2", [])
        };

        var locations = new List<Location>
        {
            new(1, "Rick's Hideout", "Base", "https://rickandmortyapi.com/api/location/1")
        };

        var episodes = new List<Episode>
        {
            new(1, "The Rickshank Rickdemption", "S03E01", "https://rickandmortyapi.com/api/episode/1", [])
        };

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
        var results = (await _sut.SearchAsync(searchTerm)).ToList();

        // Assert
        Assert.Equal(4, results.Count);
        Assert.Equal(2, results.Count(r => r.Type == "character"));
        Assert.Single(results.Where(r => r.Type == "location"));
        Assert.Single(results.Where(r => r.Type == "episode"));

        // Verify correct names are mapped
        Assert.Contains(results, r => r.Name == "Rick Sanchez" && r.Type == "character");
        Assert.Contains(results, r => r.Name == "Rick's Hideout" && r.Type == "location");
        Assert.Contains(results, r => r.Name == "The Rickshank Rickdemption" && r.Type == "episode");
    }

    [Fact]
    public async Task SearchAsync_ShouldApplyLimit_WhenLimitIsProvided()
    {
        // Arrange
        const string searchTerm = "test";
        const int limit = 2;

        var characters = new List<Character>
        {
            new(1, "Character 1", "Type", "https://rickandmortyapi.com/api/character/1", []),
            new(2, "Character 2", "Type", "https://rickandmortyapi.com/api/character/2", []),
            new(3, "Character 3", "Type", "https://rickandmortyapi.com/api/character/3", [])
        };

        var locations = new List<Location>
        {
            new(1, "Location 1", "Type", "https://rickandmortyapi.com/api/location/1")
        };

        var episodes = new List<Episode>
        {
            new(1, "Episode 1", "S01E01", "https://rickandmortyapi.com/api/episode/1", [])
        };

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
        var results = (await _sut.SearchAsync(searchTerm, limit)).ToList();

        // Assert
        Assert.Equal(limit, results.Count);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyList_WhenTermIsEmpty()
    {
        // Arrange
        var emptyTerm = "";

        // Act
        var results = await _sut.SearchAsync(emptyTerm);

        // Assert
        Assert.Empty(results);
        _mockClient.Verify(c => c.GetAllCharactersAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_ShouldReturnEmptyList_WhenNoResultsFound()
    {
        // Arrange
        const string searchTerm = "nonexistent";

        _mockClient
            .Setup(c => c.GetAllCharactersAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Character>());

        _mockClient
            .Setup(c => c.GetAllLocationsAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Location>());

        _mockClient
            .Setup(c => c.GetAllEpisodesAsync(searchTerm, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Episode>());

        // Act
        var results = await _sut.SearchAsync(searchTerm);

        // Assert
        Assert.Empty(results);
    }
}