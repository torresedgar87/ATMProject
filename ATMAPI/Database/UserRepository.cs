using ATMAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMAPI.Database;

public class UserRepository : IUserRepository
{
    private readonly APIDbContext _dbContext;
    
    public UserRepository(APIDbContext context)
    {
        _dbContext = context;
    }

    public Task CreateUserAsync(UserEntity user)
    {
        _dbContext.Users.Add(user);
        return _dbContext.SaveChangesAsync();
    }

    public Task<UserEntity?> GetUserByEmailAsync(string email)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<UserEntity?> GetUserByIdAliasAsync(Guid idAlias)
    {
        return _dbContext.Users.FirstOrDefaultAsync(u => u.IdAlias == idAlias);
    }
}