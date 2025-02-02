using Application.Services.Readiness.Contracts;
using Application.Services.Readiness.DTOs;
using AutoMapper;
using Domain.Entities.Readiness;
using MediatR;

namespace Application.Services.Readiness.Commands
{
    public class UpdateReadinessCommandHandler : IRequestHandler<UpdateReadinessCommandDto>
    {
        private readonly IReadinessRepository repository;
        private readonly IMapper mapper;


        public UpdateReadinessCommandHandler(IReadinessRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task Handle(UpdateReadinessCommandDto request, CancellationToken cancellationToken)
        {
            var entity = mapper.Map<UserReadiness>(request);

            await repository.Update(request.IdentityId, entity);
        }
    }
}