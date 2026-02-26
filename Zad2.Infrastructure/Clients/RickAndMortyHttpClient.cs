using System.Net;
using System.Text.Json;
using Zad2.Core.Interfaces;
using Zad2.Core.Models;
using Zad2.Infrastructure.Clients.ApiResponses;
using Zad2.Infrastructure.Mappers;

namespace Zad2.Infrastructure.Clients;

public class RickAndMortyHttpClient : BaseApiClient, IRickAndMortyClient
{
    private const string CharacterEndpoint = "character";
    private const string LocationEndpoint = "location";
    private const string EpisodeEndpoint = "episode";

    public RickAndMortyHttpClient(HttpClient httpClient) : base(httpClient)
    {
    }

    #region Characters

    public async Task<PaginatedResponse<Character>> GetCharactersAsync(
        string? name = null,
        int? page = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(CharacterEndpoint, name, page);
        var response = await GetPaginatedAsync<ApiCharacter>(url, cancellationToken);
        return ApiResponseMapper.ToPaginatedCharacters(response);
    }

    public async Task<IReadOnlyList<Character>> GetAllCharactersAsync(
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(CharacterEndpoint, name);
        var results = await GetAllPagesAsync<ApiCharacter>(url, cancellationToken);
        return ApiResponseMapper.ToCharacters(results);
    }

    public async Task<Character?> GetCharacterByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync<ApiCharacter>($"{CharacterEndpoint}/{id}", cancellationToken);
        return result is null ? null : ApiResponseMapper.ToCharacter(result);
    }

    public async Task<IReadOnlyList<Character>> GetCharactersByIdsAsync(
        IEnumerable<int> ids,
        CancellationToken cancellationToken = default)
    {
        var results = await GetByIdsAsync<ApiCharacter>(CharacterEndpoint, ids, cancellationToken);
        return ApiResponseMapper.ToCharacters(results);
    }

    #endregion

    #region Locations

    public async Task<PaginatedResponse<Location>> GetLocationsAsync(
        string? name = null,
        int? page = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(LocationEndpoint, name, page);
        var response = await GetPaginatedAsync<ApiLocation>(url, cancellationToken);
        return ApiResponseMapper.ToPaginatedLocations(response);
    }

    public async Task<IReadOnlyList<Location>> GetAllLocationsAsync(
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(LocationEndpoint, name);
        var results = await GetAllPagesAsync<ApiLocation>(url, cancellationToken);
        return ApiResponseMapper.ToLocations(results);
    }

    public async Task<Location?> GetLocationByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync<ApiLocation>($"{LocationEndpoint}/{id}", cancellationToken);
        return result is null ? null : ApiResponseMapper.ToLocation(result);
    }

    #endregion

    #region Episodes

    public async Task<PaginatedResponse<Episode>> GetEpisodesAsync(
        string? name = null,
        int? page = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(EpisodeEndpoint, name, page);
        var response = await GetPaginatedAsync<ApiEpisode>(url, cancellationToken);
        return ApiResponseMapper.ToPaginatedEpisodes(response);
    }

    public async Task<IReadOnlyList<Episode>> GetAllEpisodesAsync(
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(EpisodeEndpoint, name);
        var results = await GetAllPagesAsync<ApiEpisode>(url, cancellationToken);
        return ApiResponseMapper.ToEpisodes(results);
    }

    public async Task<Episode?> GetEpisodeByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync<ApiEpisode>($"{EpisodeEndpoint}/{id}", cancellationToken);
        return result is null ? null : ApiResponseMapper.ToEpisode(result);
    }

    #endregion
}