namespace Zad2.Core.Models;

public record PaginatedResponse<T>(
    IReadOnlyList<T> Results,
    int TotalCount,
    int? NextPage,
    int? PrevPage
);