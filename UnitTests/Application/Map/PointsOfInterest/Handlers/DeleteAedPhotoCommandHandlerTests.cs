using Application.Map.PointsOfInterest.Commands;
using NSubstitute;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Microsoft.AspNetCore.Http;

namespace UnitTests.Application.Map.PointsOfInterest.Handlers
{
    public class DeleteAedPhotoCommandHandlerTests
    {
        private readonly IAedRepository repositoryMock;


        public DeleteAedPhotoCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IAedRepository>();
        }

        [Fact]
        public async Task Handle_Should_ReturnResult()
        {
            SetupRepository();
            var handler = new DeleteAedPhotoCommandHandler(repositoryMock);
            var command = new DeleteAedPhotoCommandDto("123");

            await handler.Handle(command, default);

            await repositoryMock
                .Received()
                .DeletePhoto(Arg.Is<string>(m => m == "123"));
        }

        private void SetupRepository()
        {
            repositoryMock.DeletePhoto(Arg.Any<string>());
        }
    }
}
