using Application.Services.Readiness.Contracts;
using Application.Services.Readiness.DTOs;
using Application.Services.Readiness.DTOs.Common;
using AutoMapper;
using Domain.Entities.Readiness;
using MediatR;

namespace Application.Services.Readiness.Queries
{
    public class GetReadinessQueryHandler : IRequestHandler<GetReadinessQueryDto, UserReadinessDataDto>
    {
        private readonly IReadinessRepository repository;
        private readonly IMapper mapper;


        public GetReadinessQueryHandler(IReadinessRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<UserReadinessDataDto> Handle(GetReadinessQueryDto request, CancellationToken cancellationToken)
        {
            var entity = await repository.Get(request.IdentityId);

            var dto = mapper.Map<UserReadinessDataDto>(entity);

            return dto;
        }
    }
}