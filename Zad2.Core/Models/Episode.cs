namespace Zad2.Core.Models;

public record Episode(
    int Id,
    string Name,
    string EpisodeCode,
    string Url,
    IReadOnlyList<string> Characters
);