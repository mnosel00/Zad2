using Zad2.Core.Models;
using Zad2.Infrastructure.Clients.ApiResponses;

namespace Zad2.Infrastructure.Mappers;

internal static class ApiResponseMapper
{
    public static Character ToCharacter(ApiCharacter api) =>
        new(api.Id, api.Name, api.Type, api.Url, api.Episode);

    public static Location ToLocation(ApiLocation api) =>
        new(api.Id, api.Name, api.Type, api.Url);

    public static Episode ToEpisode(ApiEpisode api) =>
        new(api.Id, api.Name, api.Episode, api.Url, api.Characters);

    public static IReadOnlyList<Character> ToCharacters(IEnumerable<ApiCharacter> apiList) =>
        apiList.Select(ToCharacter).ToList();

    public static IReadOnlyList<Location> ToLocations(IEnumerable<ApiLocation> apiList) =>
        apiList.Select(ToLocation).ToList();

    public static IReadOnlyList<Episode> ToEpisodes(IEnumerable<ApiEpisode> apiList) =>
        apiList.Select(ToEpisode).ToList();

    public static PaginatedResponse<Character> ToPaginatedCharacters(PaginatedResponse<ApiCharacter> response) =>
        new(ToCharacters(response.Results), response.TotalCount, response.NextPage, response.PrevPage);

    public static PaginatedResponse<Location> ToPaginatedLocations(PaginatedResponse<ApiLocation> response) =>
        new(ToLocations(response.Results), response.TotalCount, response.NextPage, response.PrevPage);

    public static PaginatedResponse<Episode> ToPaginatedEpisodes(PaginatedResponse<ApiEpisode> response) =>
        new(ToEpisodes(response.Results), response.TotalCount, response.NextPage, response.PrevPage);
}