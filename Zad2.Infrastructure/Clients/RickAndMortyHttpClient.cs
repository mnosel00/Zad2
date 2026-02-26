using System.Net;
using System.Text.Json;
using Zad2.Core.Interfaces;
using Zad2.Core.Models;
using Zad2.Infrastructure.Clients.ApiResponses;

namespace Zad2.Infrastructure.Clients;

public class RickAndMortyHttpClient : IRickAndMortyClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public RickAndMortyHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region Characters

    public async Task<PaginatedResponse<Character>> GetCharactersAsync(
        string? name = null, 
        int? page = null, 
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("character", name, page);
        var response = await GetPaginatedAsync<ApiCharacter>(url, cancellationToken);
        
        return new PaginatedResponse<Character>(
            response.Results.Select(MapToCharacter).ToList(),
            response.TotalCount,
            response.NextPage,
            response.PrevPage
        );
    }

    public async Task<IReadOnlyList<Character>> GetAllCharactersAsync(
        string? name = null, 
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("character", name);
        var results = await GetAllPagesAsync<ApiCharacter>(url, cancellationToken);
        return results.Select(MapToCharacter).ToList();
    }

    public async Task<Character?> GetCharacterByIdAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync<ApiCharacter>($"character/{id}", cancellationToken);
        return result is null ? null : MapToCharacter(result);
    }

    public async Task<IReadOnlyList<Character>> GetCharactersByIdsAsync(
        IEnumerable<int> ids, 
        CancellationToken cancellationToken = default)
    {
        var results = await GetByIdsAsync<ApiCharacter>("character", ids, cancellationToken);
        return results.Select(MapToCharacter).ToList();
    }

    #endregion

    #region Locations

    public async Task<PaginatedResponse<Location>> GetLocationsAsync(
        string? name = null, 
        int? page = null, 
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("location", name, page);
        var response = await GetPaginatedAsync<ApiLocation>(url, cancellationToken);
        
        return new PaginatedResponse<Location>(
            response.Results.Select(MapToLocation).ToList(),
            response.TotalCount,
            response.NextPage,
            response.PrevPage
        );
    }

    public async Task<IReadOnlyList<Location>> GetAllLocationsAsync(
        string? name = null, 
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("location", name);
        var results = await GetAllPagesAsync<ApiLocation>(url, cancellationToken);
        return results.Select(MapToLocation).ToList();
    }

    public async Task<Location?> GetLocationByIdAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync<ApiLocation>($"location/{id}", cancellationToken);
        return result is null ? null : MapToLocation(result);
    }

    public async Task<IReadOnlyList<Location>> GetLocationsByIdsAsync(
        IEnumerable<int> ids, 
        CancellationToken cancellationToken = default)
    {
        var results = await GetByIdsAsync<ApiLocation>("location", ids, cancellationToken);
        return results.Select(MapToLocation).ToList();
    }

    #endregion

    #region Episodes

    public async Task<PaginatedResponse<Episode>> GetEpisodesAsync(
        string? name = null, 
        int? page = null, 
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("episode", name, page);
        var response = await GetPaginatedAsync<ApiEpisode>(url, cancellationToken);
        
        return new PaginatedResponse<Episode>(
            response.Results.Select(MapToEpisode).ToList(),
            response.TotalCount,
            response.NextPage,
            response.PrevPage
        );
    }

    public async Task<IReadOnlyList<Episode>> GetAllEpisodesAsync(
        string? name = null, 
        CancellationToken cancellationToken = default)
    {
        var url = BuildUrl("episode", name);
        var results = await GetAllPagesAsync<ApiEpisode>(url, cancellationToken);
        return results.Select(MapToEpisode).ToList();
    }

    public async Task<Episode?> GetEpisodeByIdAsync(
        int id, 
        CancellationToken cancellationToken = default)
    {
        var result = await GetByIdAsync<ApiEpisode>($"episode/{id}", cancellationToken);
        return result is null ? null : MapToEpisode(result);
    }

    public async Task<IReadOnlyList<Episode>> GetEpisodesByIdsAsync(
        IEnumerable<int> ids, 
        CancellationToken cancellationToken = default)
    {
        var results = await GetByIdsAsync<ApiEpisode>("episode", ids, cancellationToken);
        return results.Select(MapToEpisode).ToList();
    }

    #endregion

    #region Private Helper Methods

    private async Task<PaginatedResponse<T>> GetPaginatedAsync<T>(
        string url, 
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new PaginatedResponse<T>([], 0, null, null);
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var apiResponse = JsonSerializer.Deserialize<ApiPaginatedResponse<T>>(content, _jsonOptions)
            ?? throw new JsonException($"Failed to deserialize paginated response from {url}");

        return new PaginatedResponse<T>(
            apiResponse.Results,
            apiResponse.Info.Count,
            ExtractPageNumber(apiResponse.Info.Next),
            ExtractPageNumber(apiResponse.Info.Prev)
        );
    }

    private async Task<List<T>> GetAllPagesAsync<T>(
        string initialUrl, 
        CancellationToken cancellationToken)
    {
        var allResults = new List<T>();
        string? currentUrl = initialUrl;

        while (!string.IsNullOrEmpty(currentUrl))
        {
            var response = await _httpClient.GetAsync(currentUrl, cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                break;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<ApiPaginatedResponse<T>>(content, _jsonOptions)
                ?? throw new JsonException($"Failed to deserialize paginated response from {currentUrl}");

            allResults.AddRange(apiResponse.Results);
            currentUrl = apiResponse.Info.Next;
        }

        return allResults;
    }

    private async Task<T?> GetByIdAsync<T>(
        string url, 
        CancellationToken cancellationToken) where T : class
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<T>(content, _jsonOptions);
    }

    private async Task<List<T>> GetByIdsAsync<T>(
        string endpoint, 
        IEnumerable<int> ids, 
        CancellationToken cancellationToken)
    {
        var idList = ids.ToList();
        
        if (idList.Count == 0)
        {
            return [];
        }

        var url = $"{endpoint}/{string.Join(",", idList)}";
        var response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        // API returns array for multiple IDs, single object for one ID
        if (idList.Count == 1)
        {
            var singleResult = JsonSerializer.Deserialize<T>(content, _jsonOptions);
            return singleResult is null ? [] : [singleResult];
        }

        return JsonSerializer.Deserialize<List<T>>(content, _jsonOptions) ?? [];
    }

    private static string BuildUrl(string endpoint, string? name = null, int? page = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
        {
            queryParams.Add($"name={Uri.EscapeDataString(name)}");
        }

        if (page.HasValue)
        {
            queryParams.Add($"page={page.Value}");
        }

        return queryParams.Count > 0 
            ? $"{endpoint}?{string.Join("&", queryParams)}" 
            : endpoint;
    }

    private static int? ExtractPageNumber(string? url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return null;
        }

        var uri = new Uri(url);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        var pageValue = query["page"];

        return int.TryParse(pageValue, out var page) ? page : null;
    }

    #endregion

    #region Mappers

    private static Character MapToCharacter(ApiCharacter api) =>
        new(api.Id, api.Name, api.Type, api.Url, api.Episode);

    private static Location MapToLocation(ApiLocation api) =>
        new(api.Id, api.Name, api.Type, api.Url);

    private static Episode MapToEpisode(ApiEpisode api) =>
        new(api.Id, api.Name, api.Episode, api.Url, api.Characters);

    #endregion
}