using ATMAPI.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace ATMAPI.Database;

public class APIDbContext : DbContext
{
    public APIDbContext(DbContextOptions<APIDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<AccountEntity> Accounts { get; set; }
    public DbSet<TransactionEntity> Transactions { get; set; }
    public DbSet<TransactionResultEntity> TransactionResults { get; set; }
    public DbSet<TransactionResultCombinedEntity> TransactionResultCombined { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        
    }
}