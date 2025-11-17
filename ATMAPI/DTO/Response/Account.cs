using ATMAPI.Database.Entities;

namespace ATMAPI.DTO;

public class Account
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public AccountType Type { get; set; }
    public List<Transaction> Transactions { get; set; }
}