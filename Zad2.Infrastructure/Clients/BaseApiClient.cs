using System.Net;
using System.Text.Json;
using Zad2.Core.Models;
using Zad2.Infrastructure.Clients.ApiResponses;

namespace Zad2.Infrastructure.Clients;

public abstract class BaseApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    protected BaseApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    protected async Task<PaginatedResponse<T>> GetPaginatedAsync<T>(
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

    protected async Task<List<T>> GetAllPagesAsync<T>(
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

    protected async Task<T?> GetByIdAsync<T>(
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

    protected async Task<List<T>> GetByIdsAsync<T>(
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

    protected static string BuildUrl(string endpoint, string? name = null, int? page = null)
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
        var query = uri.Query.TrimStart('?');

        if (string.IsNullOrEmpty(query))
        {
            return null;
        }

        foreach (var parameter in query.Split('&'))
        {
            var keyValue = parameter.Split('=', 2);
            if (keyValue.Length == 2 &&
                keyValue[0].Equals("page", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(keyValue[1], out var page))
            {
                return page;
            }
        }

        return null;
    }
}