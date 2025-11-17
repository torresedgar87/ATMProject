using ATMAPI.Database;
using ATMAPI.Database.Entities;
using ATMAPI.DTO;
using ATMAPI.DTO.Request;
using ATMAPI.Errors.Services;

namespace ATMAPI.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;
    
    public UserService(IUserRepository userRepository, IAccountRepository accountRepository)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
    }

    private User MapUserEntityToUser(UserEntity userEntity, List<AccountEntity> accountEntities)
    {
        return new User
        {
            Id = userEntity.IdAlias,
            Email = userEntity.Email,
            FirstName = userEntity.FirstName,
            LastName = userEntity.LastName,
            Accounts = accountEntities
                .Select(a => new Account
                {
                    Id = a.IdAlias,
                    Type = a.Type,
                    Balance = a.Balance,
                }).ToList(),
        };
    }
        
    public async Task<User> CreateUserAsync(CreateUser user)
    {
        var userEntity = await _userRepository.GetUserByEmailAsync(user.Email);

        if (userEntity == null)
        {
            await _userRepository.CreateUserAsync(new UserEntity
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IdAlias = Guid.NewGuid()
            });

            userEntity = await _userRepository.GetUserByEmailAsync(user.Email);
            
            if (userEntity == null)
            {
                throw new UserCreationException($"User creation failed for {user.Email}");
            }
        }
        
        var accountEntities = await _accountRepository.GetAccountsByUserIdAsync(userEntity.Id);
        
        if (!accountEntities.Any())
        {
            await _accountRepository.InitAccountAsync(userEntity.Id);
            
            accountEntities = await _accountRepository.GetAccountsByUserIdAsync(userEntity.Id);

            if (!accountEntities.Any())
            {
                throw new UserCreationException($"User creation failed to create default accounts for {user.Email}");
            }
        }
        
        return MapUserEntityToUser(userEntity,  accountEntities);
    }

    public async Task<User> GetUserByEmailAsync(string email)
    {
        var userEntity = await _userRepository.GetUserByEmailAsync(email);

        if (userEntity == null)
        {
            return null;
        }
        
        var accountEntities = await _accountRepository.GetAccountsByUserIdAsync(userEntity.Id);
        
        return MapUserEntityToUser(userEntity, accountEntities);
    }
    
    public async Task<User> GetUserByIdAliasAsync(Guid idAlias)
    {
        var userEntity = await _userRepository.GetUserByIdAliasAsync(idAlias);

        if (userEntity == null)
        {
            return null;
        }
        
        var accountEntities = await _accountRepository.GetAccountsByUserIdAsync(userEntity.Id);
        
        return MapUserEntityToUser(userEntity, accountEntities);
    }
}