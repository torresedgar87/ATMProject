using System.ComponentModel.DataAnnotations;

namespace ATMAPI.DTO.Request;

public class CreateUser
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 3,  ErrorMessage = "First name must be between 3 and 50 characters")]
    public string FirstName { get; set; }
    
    [StringLength(50, MinimumLength = 3,  ErrorMessage = "Last name must be between 3 and 50 characters")]
    public string LastName { get; set; }
}