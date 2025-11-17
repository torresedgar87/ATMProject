using System.ComponentModel.DataAnnotations;

namespace ATMAPI.DTO.Request;

public class GetUserByEmail
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
}