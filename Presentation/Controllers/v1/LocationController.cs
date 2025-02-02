using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using Application.Services.Location.DTOs.Common;
using Application.Services.Location.DTOs;

namespace Presentation.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route(ApiRoutes.LOCATION_ROUTE)]
    [ApiVersion(ApiVersions.V1String)]
    public class LocationController : ControllerBase
    {
        private readonly ISender mediator;

        public LocationController(ISender mediator)
        {
            this.mediator = mediator;
        }

        [HttpPut("{deviceId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateLocation(string deviceId, bool? t, [FromBody] DeviceLocationDto deviceLocation)
        {
            var command = new UpdateLocationCommandDto(deviceId, deviceLocation, t ?? false);

            await mediator.Send(command);

            return NoContent();
        }
    }

}
