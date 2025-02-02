using Microsoft.AspNetCore.Authentication;

namespace IntegrationTests.Common
{
    internal class TestAuthHandlerOptions : AuthenticationSchemeOptions
    {
        public string DefaultUserId { get; set; } = null!;
    }
}