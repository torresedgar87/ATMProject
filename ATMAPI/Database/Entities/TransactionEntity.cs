namespace ATMAPI.Database.Entities;

// Defining the right nullable fields can help keep bad data out
public class TransactionEntity
{
    public int Id { get; set; }
    public Guid IdAlias {  get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }

    public override string ToString()
    {
        return $"{Id}, {IdAlias}, {FromAccountId}, {ToAccountId}, {Amount}, {Type} , {CreatedAt}";
    }
}