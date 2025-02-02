using Application.Services.Map.PointsOfInterest.Contracts;
using Domain.Common;
using Domain.Entities.Map;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;
using Infrastructure.Persistance.Core.Arango;
using System.ComponentModel;
using System.Globalization;

namespace Infrastructure.Persistance.Repositories.Map
{
    internal class PoiRepository : IPoiRepository
    {
        private readonly IDocumentRepository<Aed> aedsRepository;
        private readonly IDocumentRepository<EmergencyDepartment> departmentsRepository;


        public PoiRepository(
            IDocumentRepository<Aed> aedsRepository,
            IDocumentRepository<EmergencyDepartment> departmentsRepository)
        {
            this.aedsRepository = aedsRepository;
            this.departmentsRepository = departmentsRepository;
        }

        public async Task<List<Aed>> GetAeds(Coordinates coordinates, double? rangeInKm, bool isSortByDistance)
        {
            return (await aedsRepository.Execute(
                GetQuery(GlobalCollections.AEDS, coordinates, rangeInKm, isSortByDistance)))
                .ToList();
        }

        public Task<List<EmergencyDepartment>> GetNiswols(Coordinates coordinates, double? rangeInKm, bool isSortByDistance)
        {
            return GetDepartments(coordinates, rangeInKm, GlobalCollections.NISWOL, isSortByDistance);
        }

        public Task<List<EmergencyDepartment>> GetSors(Coordinates coordinates, double? rangeInKm, bool isSortByDistance)
        {
            return GetDepartments(coordinates, rangeInKm, GlobalCollections.SOR, isSortByDistance);
        }

        private async Task<List<EmergencyDepartment>> GetDepartments(
            Coordinates coordinates, double? rangeInKm, string departmentCollection, bool isSortByDistance)
        {
            return (await departmentsRepository.Execute(
                GetQuery(departmentCollection, coordinates, rangeInKm, isSortByDistance)))
                .ToList();

        }

        private string GetQuery(string collection, Coordinates coordinates, double? rangeInKm, bool isSortByDistance)
        {
            var aql = AqlQuery.CreateForQuery(collection);

            if (rangeInKm.HasValue)
            {
                double rangeInM = rangeInKm.Value * GlobalValues.KM_IN_M;
                aql.AddGeoInRangeFilter(coordinates, rangeInM);
            }

            if (isSortByDistance)
            {
                var queryLatitude = coordinates.Latitude.ToString(CultureInfo.InvariantCulture);
                var queryLongitude = coordinates.Longitude.ToString(CultureInfo.InvariantCulture);
                var distance = $"DISTANCE(doc.geometry.coordinates[1], doc.geometry.coordinates[0], {queryLatitude}, {queryLongitude})";
                aql.AddSort((distance, ListSortDirection.Ascending));
            }

            return aql.Return()
                .GetQuery();
        }
    }
}
