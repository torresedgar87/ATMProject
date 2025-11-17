using ATMAPI.DTO;

namespace ATMAPI.Services;

public interface IAccountService
{
    Task<Account> GetAccountByIdAliasAsync(int idAlias);
}