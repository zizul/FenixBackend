using Domain.Entities.Map;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;

namespace IntegrationTests.Common.Fixtures
{
    public class PoiFixture : DatabaseFixture
    {
        public static Coordinates ReadOnlyLocation { get; } = new Coordinates(1, 1);
        public static Coordinates WriteReadLocation { get; } = new Coordinates(10.5, 10.5);
        public static Coordinates DeleteLocation { get; } = new Coordinates(-50, -50);
        public static Coordinates LocationOutOfRange { get; } = new Coordinates(-220.5, 120);

        public Dictionary<Coordinates, List<Aed>> DbAeds { get; } = new();
        public Dictionary<Coordinates, List<EmergencyDepartment>> DbSors { get; } = new();
        public Dictionary<Coordinates, List<EmergencyDepartment>> DbNiswols { get; } = new();

        public Dictionary<int, double> DbPoiDistanceFactors { get; } = new();


        public PoiFixture()
        {
        }

        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await InitPoiCollections();

            await AddAeds(4, ReadOnlyLocation);
            await AddSors(6, ReadOnlyLocation);
            await AddNiswols(8, ReadOnlyLocation);

            await AddAeds(6, WriteReadLocation);
            await AddSors(3, WriteReadLocation);
            await AddNiswols(4, WriteReadLocation);
        }

        // teardown
        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        public async Task InitPoiCollections()
        {
            await CreateCollection(GlobalCollections.AEDS);
            await CreateCollection(GlobalCollections.SOR);
            await CreateCollection(GlobalCollections.NISWOL);
        }

        private async Task AddAeds(int count, Coordinates location)
        {
            var aeds = new List<Aed>();
            for (int i = 0; i < count; i++)
            {
                int distanceFactor = count - i;
                var result = await AddAed(GetNearLocation(location, distanceFactor), i % 2 == 0);
                DbPoiDistanceFactors[result.Id] = distanceFactor;
                aeds.Add(result);
            }
            DbAeds.Add(location, aeds);
        }

        private async Task<Aed> AddAed(Coordinates location, bool isIndoor)
        {
            var aed = new Aed()
            {
                Coordinates = location,
                Description = "test",
                InDoor = isIndoor,
                Location = "test location",
                OpeningHours = "24/7",
                Phone = "123 456 789",
                Access = AedAccessType.Public,
                Address = new Address("test street"),
                Operator = "test operator",
                Level = isIndoor ? "2" : "",
            };

            var result = await CreateDocument(GlobalCollections.AEDS, aed);
            return result;
        }

        private async Task AddSors(int count, Coordinates location)
        {
            var poi = new List<EmergencyDepartment>();
            for (int i = 0; i < count; i++)
            {
                int distanceFactor = count - i;
                var result = await AddEmergencyDepartment(GetNearLocation(location, distanceFactor), GlobalCollections.SOR);
                DbPoiDistanceFactors[result.Id] = distanceFactor;
                poi.Add(result);
            }
            DbSors.Add(location, poi);
        }

        private async Task AddNiswols(int count, Coordinates location)
        {
            var poi = new List<EmergencyDepartment>();
            for (int i = 0; i < count; i++)
            {
                int distanceFactor = count - i;
                var result = await AddEmergencyDepartment(GetNearLocation(location, distanceFactor), GlobalCollections.NISWOL);
                DbPoiDistanceFactors[result.Id] = distanceFactor;
                poi.Add(result);
            }
            DbNiswols.Add(location, poi);
        }

        private async Task<EmergencyDepartment> AddEmergencyDepartment(Coordinates location, string departmentCollection)
        {
            var department = new EmergencyDepartment()
            {
                Coordinates = location,
                Address = new Address(street: "test street", postalCode: "12 345"),
                DepartmentName = "test",
            };

            var result = await CreateDocument(departmentCollection, department);
            return result;
        }

        /// <summary>
        /// Used to set different locations,
        /// in order to test sorting by distance
        /// </summary>
        private Coordinates GetNearLocation(Coordinates location, int distanceFactor)
        {
            var nearLongitude = location.Longitude + (Math.Pow(10, -4) * distanceFactor);
            return new Coordinates(nearLongitude, location.Latitude);
        }
    }
}