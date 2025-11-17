namespace ATMAPI.Database.Entities;

public enum TransactionStatus
{
    Unknown,
    Pending,
    InProgress,
    Processed,
    Invalid,
    Failed,
}