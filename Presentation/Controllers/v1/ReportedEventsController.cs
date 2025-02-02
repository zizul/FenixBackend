using Application.Services.Event.DTOs;
using Application.Services.Event.DTOs.Common;
using Asp.Versioning;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using Presentation.Utils;

namespace Presentation.Controllers.v1
{
    [Authorize]
    [ApiController]
    [Route(ApiRoutes.EVENTS_ROUTE)]
    [ApiVersion(ApiVersions.V1String)]
    public class ReportedEventsController : ControllerBase
    {
        private readonly ISender mediator;


        public ReportedEventsController(ISender mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReportedEventResultDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> ReportEvent([FromBody] ReportedEventDto data)
        {
            string identityId = OidcUtils.GetUserIdentifier(User);
            var command = new ReportEventCommandDto(identityId, data);

            var resource = await mediator.Send(command);

            return Created(
                $"{ApiRoutes.EVENTS_ROUTE}/{resource.Id}",
                resource);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateEvent(string id, [FromBody] EventStatusType statusType)
        {
            var command = new UpdateEventCommandDto(id, statusType);

            await mediator.Send(command);

            return NoContent();
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetUserEventsResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserEvents(string? status)
        {
            string identityId = OidcUtils.GetUserIdentifier(User);
            bool isResponder = OidcUtils.IsUserHasRole(User, Roles.RESPONDER);

            var query = new GetUserEventsQueryDto(identityId, isResponder, QueryFiltersUtils.ParseEventStatus(status));

            var result = await mediator.Send(query);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ReportedEventResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEvent(string id)
        {
            string identityId = OidcUtils.GetUserIdentifier(User);
            bool isResponder = OidcUtils.IsUserHasRole(User, Roles.RESPONDER);

            var query = new GetReportedEventByIdQueryDto(id, identityId, isResponder);

            var result = await mediator.Send(query);

            return Ok(result);
        }

        // TODO: remove (need only for tests)
        [HttpPost($"{{eventId}}/{ApiRoutes.RESPONDERS_RESOURCE}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AssignResponder(string eventId)
        {
            string identityId = OidcUtils.GetUserIdentifier(User);
            var command = new AssignResponderCommandDto(eventId, identityId);

            await mediator.Send(command);

            return NoContent();
        }

        [HttpPut($"{{eventId}}/{ApiRoutes.RESPONDERS_RESOURCE}/{{identityId}}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateResponder(
            string eventId, string identityId, [FromBody] ResponderInputDto responderData)
        {
            var command = new UpdateResponderStatusCommandDto(
                    eventId, identityId, responderData);

            await mediator.Send(command);

            return NoContent();
        }
    }
}