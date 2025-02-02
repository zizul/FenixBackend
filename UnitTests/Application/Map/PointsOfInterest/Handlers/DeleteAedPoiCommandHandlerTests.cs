using Application.Services.Map.PointsOfInterest.Commands;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using NSubstitute;

namespace UnitTests.Application.Map.PointsOfInterest.Handlers
{
    public class DeleteAedPoiCommandHandlerTests
    {
        private readonly IAedRepository repositoryMock;


        public DeleteAedPoiCommandHandlerTests()
        {
            repositoryMock = Substitute.For<IAedRepository>();
        }

        [Fact]
        public async Task Handle_Should_CallDelete()
        {
            var handler = new DeleteAedPoiCommandHandler(repositoryMock);
            var command = new DeleteAedPoiCommandDto("123");

            await handler.Handle(command, default);

            await repositoryMock
                .Received()
                .Delete(Arg.Is<string>(id => id == "123"));
        }
    }
}
