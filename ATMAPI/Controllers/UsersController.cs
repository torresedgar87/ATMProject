using ATMAPI.DTO;
using ATMAPI.DTO.Request;
using ATMAPI.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ATMAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController
{
    private IUserService _userService;
    
    public UsersController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser([FromBody] CreateUser createUser)
    {
        var user = await _userService.CreateUserAsync(createUser);
        return new OkObjectResult(user);
    }

    [HttpGet] public async Task<ActionResult<User>> GetUserByEmail([FromQuery] GetUserByEmail getUserByEmail)
    {
        var user = await _userService.GetUserByEmailAsync(getUserByEmail.Email);

        if (user == null)
        {
            return new NotFoundObjectResult(getUserByEmail);
        }
        
        return new OkObjectResult(user);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUserById([FromRoute] GetUserById getUserById)
    {
        var user = await _userService.GetUserByIdAliasAsync(getUserById.Id);

        if (user == null)
        {
            return new NotFoundObjectResult(getUserById);
        }
        
        return new OkObjectResult(user);
    }
}