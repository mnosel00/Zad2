namespace Zad2.Core.Models;

public record Character(
    int Id,
    string Name,
    string Type,
    string Url,
    IReadOnlyList<string> Episode
);