using Microsoft.AspNetCore.Mvc;
using NLog;
using FluentValidation;
using Application.Exceptions;

namespace Presentation.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly NLog.ILogger logger;


        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
            this.logger = LogManager.GetCurrentClassLogger();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                await HandleException(context, exception);
            }
        }

        private async Task HandleException(HttpContext context, Exception exception)
        {
            var details = GetExceptionDetails(exception);

            logger.Error($"Exception occurred while processing request: {context.Request.Method} {context.Request.Path} - {details.Status} - {details.Detail} - Full details: {exception}");

            context.Response.StatusCode = (int)details.Status!;
            await context.Response.WriteAsJsonAsync(details);
        }

        private ProblemDetails GetExceptionDetails(Exception exception)
        {
            ProblemDetails problem;
            if (exception is ValidationException)
            {
                problem = GetValidationExceptionDetails();
            }
            else if (exception is ArgumentException)
            {
                problem = GetArgumentExceptionExceptionDetails();
            }
            else if (exception is ResourceNotFoundException)
            {
                problem = GetNotFoundResourceExceptionDetails();
            }
            else if (exception is ResourceConflictException)
            {
                problem = GetResourceConflictExceptionDetails();
            }
            else
            {
                problem = GetInternalServerExceptionDetails();
            }

            problem.Detail = exception.Message;
            return problem;
        }

        private ProblemDetails GetValidationExceptionDetails()
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "ValidationFailure",
                Title = "Validation error"
            };

            return problemDetails;
        }

        private ProblemDetails GetArgumentExceptionExceptionDetails()
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = "WrongParameter",
                Title = "Url parameter is wrong"
            };

            return problemDetails;
        }

        private ProblemDetails GetNotFoundResourceExceptionDetails()
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Type = "NotFound",
                Title = "Resource not found"
            };

            return problemDetails;
        }

        private ProblemDetails GetResourceConflictExceptionDetails()
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Type = "Conflict",
                Title = "Resource conflicted"
            };

            return problemDetails;
        }

        private ProblemDetails GetInternalServerExceptionDetails()
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "InternalServerError",
                Title = "Internal server error"
            };

            return problemDetails;
        }
    }
}
