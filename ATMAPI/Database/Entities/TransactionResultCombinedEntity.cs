namespace ATMAPI.Database.Entities;

public class TransactionResultCombinedEntity
{
    public int Id { get; set; }
    public Guid IdAlias {  get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? TransactionResultId { get; set; }
    public TransactionStatus? Status { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? Message { get; set; }
}