using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync (HttpContext context)
        {
            // Default variable result
            string message = "sorry, internal server error occurred. Kindly try again";
            int statusCode = (int) HttpStatusCode.InternalServerError;
            string title = "Error";

            try
            {
                await next(context);
                // check statusCode 429 : to many request
                if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Warning";
                    message = "To many request made .";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // check statusCode 401 : unAuthrize
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert";
                    message = "you are not Authorized to access.";
                    statusCode = (int)StatusCodes.Status401Unauthorized;
                    await ModifyHeader(context, title, message, statusCode);
                }

                // check statusCode 403 : Forbidden
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Out Of Access";
                    message = "you are not Authorized allowed to access.";
                    statusCode = (int)StatusCodes.Status403Forbidden;
                    await ModifyHeader(context, title, message, statusCode);
                }
            }
            catch (Exception ex)
            {
                // Log Original Exception  / File,Debugger,Console
                LogException.LogExceptions(ex);

                // check TimeOut exception : 408 
                if(ex is TimeoutException || ex is TaskCanceledException)
                {
                    title = "Out Of Time";
                    message = "Request timeout ... try again";
                    statusCode = (int) StatusCodes.Status408RequestTimeout;
                }
                // if none of the exception will execute the default
                await ModifyHeader(context,title, message, statusCode);
            }
        }

        private static async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            // display message to client
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title
            }),CancellationToken.None
            );
            return;
        }
    }
}
