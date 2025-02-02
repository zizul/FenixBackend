using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using AutoMapper;
using Domain.Entities.Map;
using MediatR;

namespace Application.Map.PointsOfInterest.Commands
{
    public class UpdateAedPoiCommandHandler : IRequestHandler<UpdateAedPoiCommandDto, AedResultDto>
    {
        private readonly IAedRepository aedRepository;
        private readonly IMapper mapper;


        public UpdateAedPoiCommandHandler(IAedRepository aedRepository, IMapper mapper)
        {
            this.aedRepository = aedRepository;
            this.mapper = mapper;
        }

        public async Task<AedResultDto> Handle(UpdateAedPoiCommandDto request, CancellationToken cancellationToken)
        {
            var aed = mapper.Map<Aed>(request);

            var modifiedAed = await aedRepository.Update(request.Id, aed);

            var aedResult = mapper.Map<AedResultDto>(modifiedAed);
            return aedResult;
        }
    }
}