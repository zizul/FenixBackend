using Application.Services.Readiness.DTOs;
using Application.Services.Readiness.DTOs.Common;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;

namespace Presentation.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route(ApiRoutes.READINESS_ROUTE)]
    [ApiVersion(ApiVersions.V1String)]
    public class ReadinessController : ControllerBase
    {
        private readonly ISender mediator;


        public ReadinessController(ISender mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet("{identityId}")]
        [ProducesResponseType(typeof(UserReadinessDataDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(string identityId)
        {
            var command = new GetReadinessQueryDto(identityId);

            var resource = await mediator.Send(command);

            return Ok(resource);
        }

        [HttpPut("{identityId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Upsert(string identityId, [FromBody] UserReadinessDataDto data)
        {
            var command = new UpdateReadinessCommandDto(identityId, data);

            await mediator.Send(command); 

            return NoContent();
        }
    }
}