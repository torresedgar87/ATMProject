using ATMAPI.Database.Entities;
using ATMAPI.DTO;
using ATMAPI.DTO.Request;

namespace ATMAPI.Database;

public interface IUserRepository
{
    Task CreateUserAsync(UserEntity user);
        
    Task<UserEntity?> GetUserByEmailAsync(string email);
    
    Task<UserEntity?> GetUserByIdAliasAsync(Guid idAlias);
}