using Domain.Entities.User;
using Domain.ValueObjects;
using Infrastructure.Persistance.Core;

namespace IntegrationTests.Common.Fixtures
{
    public class LocationFixture : DatabaseFixture
    {
        public static Coordinates StartLocation { get; } = new Coordinates(1.5, 1.5);
        public static Coordinates NewCorrectLocation { get; } = new Coordinates(9, -9);
        public static Coordinates LocationOutOfRange { get; } = new Coordinates(-220.5, 120);

        public static string DEVICE_ID_1 { get; } = "deviceId1";
        public static string DEVICE_ID_2 { get; } = "deviceId2";
        public static string DEVICE_ID_3 { get; } = "deviceId3";
        public static string UNKNOWN_DEVICE_ID { get; } = "123";
        public static List<Device> AddedDevices = new List<Device>();

        // setup
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await CreateCollection(GlobalCollections.USER_DEVICES);

            Device device1 = GetDevice("tester1", StartLocation, DEVICE_ID_1, false);
            Device device2 = GetDevice("tester1", StartLocation, DEVICE_ID_2, false);
            Device device3 = GetDevice("tester1", StartLocation, DEVICE_ID_3, false);
            AddedDevices.Add(await CreateDocument(GlobalCollections.USER_DEVICES, device1));
            AddedDevices.Add(await CreateDocument(GlobalCollections.USER_DEVICES, device2));
            AddedDevices.Add(await CreateDocument(GlobalCollections.USER_DEVICES, device3));
        }

        // teardown
        public override Task DisposeAsync()
        {
            return base.DisposeAsync();
        }

        private Device GetDevice(string userId, Coordinates coordinates, string deviceId, bool isActive)
        {
            return new Device()
            {
                UserId = userId,
                DeviceId = deviceId,
                FirebaseToken = "test"
            };
        }
    }
}
