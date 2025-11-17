using System.ComponentModel.DataAnnotations;

namespace ATMAPI.DTO.Request;

public class GetUserById
{
    [Required]
    public Guid Id { get; set; }
}