using Application.Services.Map.PointsOfInterest.Contracts;
using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using AutoMapper;
using Domain.Entities.Map;
using MediatR;

namespace Application.Map.PointsOfInterest.Commands
{
    public class AddAedPoiCommandHandler : IRequestHandler<AddAedPoiCommandDto, AedResultDto>
    {
        private readonly IAedRepository aedRepository;
        private readonly IMapper mapper;


        public AddAedPoiCommandHandler(IAedRepository aedRepository, IMapper mapper)
        {
            this.aedRepository = aedRepository;
            this.mapper = mapper;
        }

        public async Task<AedResultDto> Handle(AddAedPoiCommandDto request, CancellationToken cancellationToken)
        {
            var aed = mapper.Map<Aed>(request);

            var addedAed = await aedRepository.Add(aed);

            var aedResult = mapper.Map<AedResultDto>(addedAed);
            return aedResult;
        }
    }
}