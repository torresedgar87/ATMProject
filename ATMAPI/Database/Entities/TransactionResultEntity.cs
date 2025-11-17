namespace ATMAPI.Database.Entities;

public class TransactionResultEntity
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int TransactionId {  get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Message { get; set; }
}