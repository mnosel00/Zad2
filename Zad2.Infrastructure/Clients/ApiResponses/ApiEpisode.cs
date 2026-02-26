namespace Zad2.Infrastructure.Clients.ApiResponses;

internal record ApiEpisode(
    int Id,
    string Name,
    string Episode,
    string Url,
    List<string> Characters
);