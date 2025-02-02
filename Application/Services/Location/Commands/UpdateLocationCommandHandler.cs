using Application.Services.Location.Contracts;
using Application.Services.Location.DTOs;
using AutoMapper;
using Domain.Entities.Location;
using MediatR;

namespace Application.Services.Location.Commands
{
    public class UpdateLocationCommandHandler : IRequestHandler<UpdateLocationCommandDto>
    {
        private readonly IDeviceLocationRepository repository;
        private readonly IMapper mapper;


        public UpdateLocationCommandHandler(IDeviceLocationRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task Handle(UpdateLocationCommandDto request, CancellationToken cancellationToken)
        {
            var device = mapper.Map<DeviceLocation>(request);
            await repository.Update(device, request.IsTransaction);
        }
    }
}
