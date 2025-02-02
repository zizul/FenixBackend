using IntegrationTests.Common.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.Notifications;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using FirebaseAdmin.Messaging;

namespace IntegrationTests.Infrastructure
{
    public class AppNotifierTests : IClassFixture<DatabaseFixture>
    {
        private readonly IOptions<FirebaseAdminOptions> options;
        private readonly ILogger<FirebaseNotifier> logger;


        public AppNotifierTests(DatabaseFixture fixture)
        {
            options = fixture.Application.Services.GetRequiredService<IOptions<FirebaseAdminOptions>>();
            logger = fixture.Application.Services.GetRequiredService<ILogger<FirebaseNotifier>>();
        }

        [Fact]
        public void AppNotifier_Should_ThrowInvalidCredentials()
        {
            var customOptions = Options.Create(new FirebaseAdminOptions());

            var act = () => new FirebaseNotifier(customOptions, logger);

            var exception = Assert.Throws<System.InvalidOperationException>(act);
            Assert.Equal("Error creating credential from JSON. Unrecognized credential type .", exception.Message);
        }

        [Fact]
        public async Task AppNotifier_Should_ThrowInvalidFCMToken()
        {
            var appNotifier = new FirebaseNotifier(options, logger);
            var message = new Message()
            {
                Token = "Invalid token",
                Notification = new Notification()
                {
                    Title = "Test title",
                    Body = "Test body",
                },
                Data = new Dictionary<string, string>
                {
                    { "test", "test" }
                }
            };

            var act = async () => await appNotifier.Send(message);

            var exception = await Assert.ThrowsAsync<FirebaseMessagingException>(act);
            Assert.Equal("The registration token is not a valid FCM registration token", exception.Message);
        }
    }
}