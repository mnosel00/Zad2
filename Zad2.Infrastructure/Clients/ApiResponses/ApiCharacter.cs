using System.Text.Json.Serialization;

namespace Zad2.Infrastructure.Clients.ApiResponses;

internal record ApiCharacter(
    int Id,
    string Name,
    string Type,
    string Url,
    List<string> Episode
);