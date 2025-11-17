using ATMAPI.DTO;
using ATMAPI.DTO.Request;

namespace ATMAPI.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(CreateUser user);
    
    Task<User> GetUserByEmailAsync(string email);
    
    Task<User> GetUserByIdAliasAsync(Guid id);
}