using Application.Map.PointsOfInterest.Commands;
using NSubstitute;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Application.Map.PointsOfInterest.Handlers
{
    public class UpdateAedPhotoCommandHandlerTests
    {
        private readonly IAedRepository repositoryMock;


        public UpdateAedPhotoCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IAedRepository>();
        }

        [Fact]
        public async Task Handle_Should_ReturnResult()
        {
            SetupRepository();
            var handler = new UpdateAedPhotoCommandHandler(repositoryMock);
            var command = new UpdateAedPhotoCommandDto("123", default);

            var result = await handler.Handle(command, default);

            await repositoryMock
                .Received()
                .UpdatePhoto(Arg.Is<string>(m => m == "123"), Arg.Is<IFormFile>(m => m == default));
        }

        private void SetupRepository()
        {
            repositoryMock.UpdatePhoto(Arg.Any<string>(), Arg.Any<IFormFile>())
                .Returns("test-url");
        }
    }
}
