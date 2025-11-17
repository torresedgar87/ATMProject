using System.ComponentModel.DataAnnotations;
using ATMAPI.Database.Entities;

namespace ATMAPI.DTO.Request;

public class CreateTransaction
{
    [Required]
    public Guid Id { get; set; }
    [Required]
    [EnumDataType(typeof(TransactionType))]
    public TransactionType Type { get; set; }
    [Required]
    public decimal Amount { get; set; }
    public int FromAccountId { get; set; }
    public int ToAccountId { get; set; }
}