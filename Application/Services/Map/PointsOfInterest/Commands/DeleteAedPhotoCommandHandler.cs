using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Map.PointsOfInterest.Commands
{
    public class DeleteAedPhotoCommandHandler : IRequestHandler<DeleteAedPhotoCommandDto>
    {
        private readonly IAedRepository aedRepository;


        public DeleteAedPhotoCommandHandler(IAedRepository aedRepository)
        {
            this.aedRepository = aedRepository;
        }

        public async Task Handle(DeleteAedPhotoCommandDto request, CancellationToken cancellationToken)
        {
            await aedRepository.DeletePhoto(request.AedId);
        }
    }
}