using Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Presentation.Middleware;

namespace UnitTests.Presentation.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        public static IEnumerable<object[]> TestCases => new List<object[]>
        {
            new object[]
            {
                new RequestDelegate((innerContext) =>
                {
                    return Task.CompletedTask;
                }),
                StatusCodes.Status200OK
            },
            new object[]
            {
                new RequestDelegate((innerContext) =>
                {
                    throw new ValidationException("test error");
                }),
                StatusCodes.Status400BadRequest
            },
            new object[] 
            {
                new RequestDelegate((innerContext) =>
                {
                    throw new ArgumentException("test error");
                }),
                StatusCodes.Status400BadRequest
            },
            new object[] 
            {
                new RequestDelegate((innerContext) =>
                {
                    throw new ResourceNotFoundException("test error");
                }),
                StatusCodes.Status404NotFound
            },
            new object[]
            {
                new RequestDelegate((innerContext) =>
                {
                    throw new ResourceConflictException("test error");
                }),
                StatusCodes.Status409Conflict
            },
            new object[]
            {
                new RequestDelegate((innerContext) =>
                {
                    throw new Exception("test error");
                }),
                StatusCodes.Status500InternalServerError
            },
        };

        [Theory]
        [MemberData(nameof(TestCases))]
        public async Task ExceptionMiddleware_Should_ReturnCorrectCode(
            RequestDelegate requestDelegate, int expectedReturnCode)
        {
            var httpContext = new DefaultHttpContext();
            var middleware = new ExceptionHandlingMiddleware(requestDelegate);

            await middleware.InvokeAsync(httpContext);

            Assert.Equal(expectedReturnCode, httpContext.Response.StatusCode);
        }
    }
}
