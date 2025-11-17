using ATMAPI.DTO;
using ATMAPI.DTO.Request;
using ATMAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATMAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController
{
    private IAccountService _accountService;
    
    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<ActionResult<Account>> GetAccountByIdAliasAsync([FromQuery] GetAccountById getAccountById)
    {
        var account = await _accountService.GetAccountByIdAliasAsync(getAccountById.Id);
        return new OkObjectResult(account);
    }
}