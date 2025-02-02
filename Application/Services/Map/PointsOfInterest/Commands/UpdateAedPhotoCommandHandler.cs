using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using AutoMapper;
using MediatR;

namespace Application.Map.PointsOfInterest.Commands
{
    public class UpdateAedPhotoCommandHandler : IRequestHandler<UpdateAedPhotoCommandDto, string>
    {
        private readonly IAedRepository aedRepository;


        public UpdateAedPhotoCommandHandler(IAedRepository aedRepository)
        {
            this.aedRepository = aedRepository;
        }

        public async Task<string> Handle(UpdateAedPhotoCommandDto request, CancellationToken cancellationToken)
        {
            var url = await aedRepository.UpdatePhoto(request.AedId, request.File);
            return url;
        }
    }
}