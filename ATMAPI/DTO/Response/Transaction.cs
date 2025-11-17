using ATMAPI.Database.Entities;

namespace ATMAPI.DTO;

public class Transaction
{
    public Guid Id {  get; set; }
    public decimal Amount { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus? Status { get; set; }
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}