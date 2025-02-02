using Domain.Entities.Event;
using Domain.Enums;
using Domain.Common;
using Domain.ValueObjects;
using Infrastructure.Persistance.Repositories.Event.Serialization;
using Newtonsoft.Json;
using UnitTests.Utils;

namespace UnitTests.Persistance.Event.Serialization
{
    public class ReportedEventConverterTests
    {
        private readonly ReportedEventConverter converter;


        public ReportedEventConverterTests()
        {
            converter = new ReportedEventConverter();
        }

        [Fact]
        public void Converter_Should_ReadWriteSameObject()
        {
            var entity = GetEntity();

            var serialized = JsonConvert.SerializeObject(entity, new JsonConverter[] { converter });

            ConverterUtils.AssertReadWriteAreSame(serialized, entity, converter);
        }

        [Fact]
        public void Converter_Should_Deserialize_Event_With_Responders()
        {
            var deserialized = JsonConvert.DeserializeObject<ReportedEvent>(
                GetTestReportedEventJson(),
                new JsonConverter[] { converter });
            
            Assert.Equivalent(GetExpectedReportedEvent(), deserialized);
        }
        
        private ReportedEvent GetEntity()
        {
            return new ReportedEvent()
            {
                Id = "123",
                Status = EventStatusType.Accepted,
                Reporter = new Reporter() { UserId = "456" },
                Coordinates = new Coordinates(4.5, 6.5),
                CreatedAt = DateTime.UtcNow,
                ClosedAt = DateTime.UtcNow,
            };
        }

        private ReportedEvent GetExpectedReportedEvent()
        {
            Timeline<ResponderTimelineEntry> timeline = new Timeline<ResponderTimelineEntry>();
            var entry = timeline.AddEntry(new ResponderTimelineEntry(
                               status: ResponderStatusType.Pending,
                                transportType: TransportType.Walking,
                                eta: null,
                                coordinates: new Coordinates(25, 15)));
            entry.CreatedAt = new DateTime(2024, 5, 23, 9, 25, 3, 294, DateTimeKind.Utc);

            return new ReportedEvent
            {
                Id = "2366",
                CreatedAt = new DateTime(2024, 5, 23, 9, 25, 3, 294, DateTimeKind.Utc),
                Description = "test desc",
                EventType = "event",
                InjuredCount = 1,
                Coordinates = new Coordinates(25, 15),
                Reporter = new Reporter { UserId = "2362" },
                Status = EventStatusType.Pending,
                Responders = new List<Responder>
                {
                    new Responder
                    (
                        eventId : "2366",
                        identityId : "2134-8478-3678",
                        status : ResponderStatusType.Pending,
                        transport: TransportType.Walking,
                        coordinates : new Coordinates(25, 15),
                        userId: "2374"
                    ) { Timeline = timeline}
                }
            };
        }

        private string GetTestReportedEventJson()
        {
            return @"
            {
                ""_id"": ""events/2366"",
                ""_key"": ""2366"",
                ""_rev"": ""_h4RCorO---"",
                ""created_date"": ""2024-05-23T09:25:03.2940000Z"",
                ""description"": ""test desc"",
                ""event_type"": ""event"",
                ""injured_count"": 1,
                ""location"": {
                    ""longitude"": 25,
                    ""latitude"": 15
                },
                ""reporter_ref"": ""2362"",
                ""status"": ""Pending"",
                ""responders"": [
                    {
                        ""event_ref"": ""2366"",
                        ""identity_id"": ""2134-8478-3678"",
                        ""location"": {
                            ""longitude"": 25,
                            ""latitude"": 15
                        },
                        ""responder_ref"": ""2374"",
                        ""status"": ""Pending"",
                        ""transport_type"": ""Walking"",
                        ""timeline"": [
                            {
                                ""location"": {
                                    ""longitude"": 25,
                                    ""latitude"": 15
                                },
                                ""status"": ""Pending"",
                                ""created_date"": ""2024-05-23T09:25:03.2940000Z"",
                                ""transport_type"": ""Walking""
                            }
                        ]
                    },
                ]
            }";
        }
    }
}
