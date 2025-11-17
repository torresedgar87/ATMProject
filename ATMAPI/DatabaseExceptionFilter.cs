using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace ATMAPI;

public class DatabaseExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DbUpdateException ||
            context.Exception is DbUpdateConcurrencyException)
        {
            var errorList = new List<string>();
            errorList.Add(context.Exception.Message);
            
            context.Result = new ObjectResult(new
            {
                Title = "Database error occurred.",
                Errors = errorList,
                Status =  StatusCodes.Status500InternalServerError,
            });

            context.ExceptionHandled = true;
        }
    }
}