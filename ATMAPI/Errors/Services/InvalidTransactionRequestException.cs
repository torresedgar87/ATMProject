namespace ATMAPI.Errors.Services;

public class InvalidTransactionRequestException : Exception
{
    public InvalidTransactionRequestException()
    {
        
    }

    public InvalidTransactionRequestException(string message) : base(message)
    {
        
    }

    public InvalidTransactionRequestException(string message, Exception inner) : base(message, inner)
    {
        
    }
}