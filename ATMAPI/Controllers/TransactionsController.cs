using ATMAPI.DTO;
using ATMAPI.DTO.Request;
using ATMAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATMAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] CreateTransaction createTransaction)
    {
        var transaction = await _transactionService.CreateTransactionAsync(createTransaction);
        return new OkObjectResult(transaction);
    }

    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> GetTransactionsByAccountId([FromQuery] GetTransactionsByAccountId getTransactionsByAccountId)
    {
        var transactions =
            await _transactionService.GetTransactionsResultCombinedByAccountIdAsync(getTransactionsByAccountId.AccountId);
        var orderedTransactions = transactions.OrderByDescending(t => t.CreatedAt);
        return new OkObjectResult(orderedTransactions);
    }
}