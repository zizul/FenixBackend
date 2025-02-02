using Application.Services.Map.PointsOfInterest.DTOs;
using Application.Services.Map.PointsOfInterest.DTOs.Common;
using Asp.Versioning;
using Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Common;
using Presentation.Utils;

namespace Presentation.Controllers.v1
{
    /// <summary>
    /// POI - point of interest
    /// </summary>
    [ApiController]
    [Route(ApiRoutes.POI_ROUTE)]
    [ApiVersion(ApiVersions.V1String)]
    public class MedicalPoiController : ControllerBase
    {
        private readonly ISender mediator;


        public MedicalPoiController(ISender mediator)
        {
            this.mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetMedicalPoiResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPoi(
            double longitude, double latitude, double rangeInKm, string? filters = null, bool? isSortByDistance = null)
        {
            var query = new GetMedicalPoiQueryDto(
                    new Coordinates(longitude, latitude),
                    rangeInKm,
                    QueryFiltersUtils.ParsePoiFilters(filters),
                    isSortByDistance ?? false);

            var result = await mediator.Send(query);

            return Ok(result);
        }

        [Authorize]
        [HttpPost(ApiRoutes.AED_RESOURCE)]
        [ProducesResponseType(typeof(AedResultDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> AddAed([FromBody] AedDto aed)
        {
            var command = new AddAedPoiCommandDto(aed);

            var resource = await mediator.Send(command);

            return Created(
                $"{ApiRoutes.POI_ROUTE}/{ApiRoutes.AED_RESOURCE}/{resource.Id}",
                resource);
        }

        [Authorize]
        [HttpPut($"{ApiRoutes.AED_RESOURCE}/{{id}}")]
        [ProducesResponseType(typeof(AedResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAed(string id, [FromBody] AedDto aed)
        {
            var command = new UpdateAedPoiCommandDto(id, aed);

            var resource = await mediator.Send(command);

            return Ok(resource);
        }

        [Authorize]
        [HttpDelete($"{ApiRoutes.AED_RESOURCE}/{{id}}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAed(string id)
        {
            var command = new DeleteAedPoiCommandDto(id);

            await mediator.Send(command);

            return NoContent();
        }

        [HttpPut($"{ApiRoutes.AED_RESOURCE}/{{id}}/{ApiRoutes.PHOTO_RESOURCE}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateAedPhoto(string id, IFormFile photo)
        {
            var command = new UpdateAedPhotoCommandDto(id, photo);

            var url = await mediator.Send(command);

            return Ok(url);
        }

        [HttpDelete($"{ApiRoutes.AED_RESOURCE}/{{id}}/{ApiRoutes.PHOTO_RESOURCE}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteAedPhoto(string id)
        {
            var command = new DeleteAedPhotoCommandDto(id);

            await mediator.Send(command);

            return NoContent();
        }
    }
}