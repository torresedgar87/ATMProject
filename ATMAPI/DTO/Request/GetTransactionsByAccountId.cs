using System.ComponentModel.DataAnnotations;

namespace ATMAPI.DTO.Request;

public class GetTransactionsByAccountId
{
    [Required]
    public int AccountId { get; set; }
}