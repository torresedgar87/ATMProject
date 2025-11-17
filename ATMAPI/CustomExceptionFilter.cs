using ATMAPI.Errors.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ATMAPI;

public class CustomExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidTransactionRequestException ||
            context.Exception is TransactionCreationException ||
            context.Exception is UserCreationException)
        {
            var errorList = new List<string>();
            errorList.Add(context.Exception.Message);

            context.Result = new ObjectResult(new
            {
                Title = "API error occured.",
                Errors = errorList,
                Status = StatusCodes.Status500InternalServerError
            });

            context.ExceptionHandled = true;
        }
    }
}