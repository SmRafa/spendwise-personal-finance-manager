namespace PersonalFinanceManager.Models;

public class TransactionListViewModel
{
    public IReadOnlyList<FinanceTransaction> Items { get; init; } = [];
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public string? Search { get; init; }
    public int? AccountId { get; init; }
    public int? CategoryId { get; init; }
    public TransactionType? Type { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}
