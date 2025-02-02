using Application.Services.User.Contracts;
using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using AutoMapper;
using Domain.Entities.User;
using MediatR;

namespace Application.Services.User.Commands
{
    public class AddOrUpdateDeviceCommandHandler : IRequestHandler<AddOrUpdateDeviceCommandDto, DeviceDto>
    {
        private readonly IDeviceRepository repository;
        private readonly IMapper mapper;


        public AddOrUpdateDeviceCommandHandler(IDeviceRepository repository, IMapper mapper)
        {
            this.repository = repository;
            this.mapper = mapper;
        }

        public async Task<DeviceDto> Handle(AddOrUpdateDeviceCommandDto request, CancellationToken cancellationToken)
        {
            var device = mapper.Map<Device>(request);

            var added = await repository.AddOrUpdate(device);

            var addedDto = mapper.Map<DeviceDto>(added);
            return addedDto;
        }
    }
}