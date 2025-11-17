namespace ATMAPI.Database.Entities;

public class AccountEntity
{
    public int Id { get; set; }
    public int IdAlias { get; set; }
    public int UserId { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LastProcessedTransactionId { get; set; }
}