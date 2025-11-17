using ATMAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMAPI.Database;

public class TransactionRepository : ITransactionRepository
{
    private readonly APIDbContext _dbContext;
    private readonly ILogger<TransactionRepository> _logger;
    
    public TransactionRepository(APIDbContext dbContext, ILogger<TransactionRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public Task CreateTransactionAsync(TransactionEntity transactionEntity)
    {
        try
        {
            _dbContext.Transactions.Add(transactionEntity);
            return _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }

    public Task<TransactionEntity?> GetTransactionByIdAliasAsync(Guid idAlias)
    {
        return _dbContext.Transactions.FirstOrDefaultAsync(t => t.IdAlias == idAlias);
    }

    public Task<List<TransactionEntity>> GetTransactionByAccountIdAsync(int accountId, int lastProcessedTransactionId = 0)
    {
        return _dbContext
            .Transactions
            .Where(t => (t.FromAccountId == accountId || t.ToAccountId == accountId)
                && t.Id >  lastProcessedTransactionId)
            .ToListAsync();
    }

    public Task<List<TransactionResultCombinedEntity>> GetTransactionResultCombinedByAccountIdAsync(int accountId, int lastProcessedTransactionId = 0)
    {
        var sql = @"
            SELECT t.*, r.id AS transactionResultId, r.createdAt AS processedAt, r.message, r.status
            FROM Transactions t
            LEFT JOIN TransactionResults r
            ON t.Id = r.TransactionId
            WHERE (t.fromAccountId = {0} OR t.toAccountId = {0}) AND t.id > {1}";
        
        return _dbContext.TransactionResultCombined
            .FromSqlRaw(sql, accountId, lastProcessedTransactionId)
            .ToListAsync();
    }

    public Task CreateTransactionResultAsync(TransactionResultEntity transactionResultEntity)
    {
        try
        {
            _dbContext.TransactionResults.Add(transactionResultEntity);
            return _dbContext.SaveChangesAsync();
        }
        catch(DbUpdateException  ex)
        {
            _logger.LogError(ex, ex.Message);
            throw;
        }
    }
}