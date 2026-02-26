namespace Zad2.Infrastructure.Clients.ApiResponses;

internal record ApiPaginatedResponse<T>(
    ApiPageInfo Info,
    List<T> Results
);