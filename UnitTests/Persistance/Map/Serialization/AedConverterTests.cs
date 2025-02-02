using Domain.Entities.Map;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.Map.Serialization
{
    public class AedConverterTests
    {
        private readonly AedConverter converter;


        public AedConverterTests()
        {
            converter = new AedConverter();
        }

        [Fact]
        public void Converter_Should_WriteInGeoJsonFormat()
        {
            var aed = GetEntity();

            var serialized = JsonConvert.SerializeObject(aed, new JsonConverter[] { converter });

            ConverterUtils.AssertIsSerializedInGeoJson(serialized);
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var aed = GetEntity();

            var serialized = JsonConvert.SerializeObject(aed, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, aed, converter);
        }

        private Aed GetEntity()
        {
            return new Aed()
            {
                Id = 123,
                InDoor = true,
                Location = "test location",
                Coordinates = new Coordinates(10.5, 5.25),
                Description = "test description",
                OpeningHours = "Jun-Jul Mo-Fr 08:00-18:00",
                Phone = "123 456 789",
                Access = AedAccessType.Public,
                Address = new Address("test street"),
                Operator = "test operator",
                Level = "2",
                Availability = new Availability(
                    new List<MonthlyRule>()
                    {
                        new MonthlyRule(
                            new List<string>() {"Jun", "Jul"},
                            new List<DailyRule>()
                            {
                                new DailyRule(
                                    new List<string>() {"Mo", "Tu", "We", "Th", "Fr" },
                                    new List<HourRule>()
                                    {
                                        new HourRule("08:00", "18:00")
                                    }
                                )
                            }
                        )
                    },
                    new List<SpecialRule>()
                    {
                        new SpecialRule(
                            new List<string>() { "2024-08-15", "2024-11-01", "2024-11-11" },
                            true,
                            new List<HourRule>()
                            {
                                new HourRule("10:00", "14:00")
                            }
                        ),
                        new SpecialRule(
                            new List<string>() { "2024-04-01", "2025-04-21", "2026-04-06" },
                            false,
                            new List<HourRule>()
                            {
                                new HourRule("10:00", "14:00")
                            }
                        )
                    }
                ),
            };
        }
    }
}
