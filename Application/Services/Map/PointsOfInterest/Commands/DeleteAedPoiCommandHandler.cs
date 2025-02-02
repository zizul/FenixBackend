using Application.Services.Event.Contracts;
using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using MediatR;

namespace Application.Services.Map.PointsOfInterest.Commands
{
    public class DeleteAedPoiCommandHandler : IRequestHandler<DeleteAedPoiCommandDto>
    {
        private readonly IAedRepository aedRepository;


        public DeleteAedPoiCommandHandler(IAedRepository aedRepository)
        {
            this.aedRepository = aedRepository;
        }

        public async Task Handle(DeleteAedPoiCommandDto request, CancellationToken cancellationToken)
        {
            await aedRepository.Delete(request.Id);
        }
    }
}