namespace ATMAPI.Errors.Services;

public class TransactionCreationException : Exception
{
    public TransactionCreationException()
    {
        
    }

    public TransactionCreationException(string message) : base(message)
    {
        
    }

    public TransactionCreationException(string message, Exception inner) : base(message, inner)
    {
        
    }
}