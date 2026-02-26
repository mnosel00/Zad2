namespace Zad2.Infrastructure.Clients.ApiResponses;

internal record ApiPageInfo(
    int Count,
    int Pages,
    string? Next,
    string? Prev
);