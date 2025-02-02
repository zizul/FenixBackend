using Application.Services.User.DTOs;
using Application.Services.User.DTOs.Common;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;

namespace Presentation.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route(ApiRoutes.USERS_ROUTE)]
    [ApiVersion(ApiVersions.V1String)]
    public class UsersController : ControllerBase
    {
        private readonly ISender mediator;


        public UsersController(ISender mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] UserDto data)
        {
            var command = new AddUserCommandDto(data);

            var resource = await mediator.Send(command);

            return Created(
                $"{ApiRoutes.USERS_ROUTE}/{resource.IdentityId}",
                resource);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update(string id, [FromBody] UserDto user)
        {
            var command = new UpdateUserCommandDto(id, user);

            var resource = await mediator.Send(command);

            return Ok(resource);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(string id)
        {
            var command = new DeleteUserCommandDto(id);

            await mediator.Send(command);

            return NoContent();
        }

        [HttpPost($"{{id}}/{ApiRoutes.DEVICES_RESOURCE}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddOrUpdateDevice(string id, [FromBody] DeviceDto device)
        {
            var command = new AddOrUpdateDeviceCommandDto(id, device);

            await mediator.Send(command);

            return NoContent();
        }
    }
}