using AutoMapper.Configuration.Annotations;
using Domain.ValueObjects;
using Newtonsoft.Json.Linq;

namespace Infrastructure.Persistance.Repositories.Common
{
    internal static class SerializationUtils
    {
        public const string TYPE = "type";
        public const string FEATURE = "Feature";
        public const string POINT = "Point";
        public const string GEOMETRY = "geometry";
        public const string COORDINATES = "coordinates";
        public const string PROPERTIES = "properties";

        public const string ADDRESS = "address";
        public const string STREET = "street";
        public const string HOUSE = "house";
        public const string FLAT = "flat";
        public const string FLOOR = "floor";
        public const string POSTAL_CODE = "postal_code";
        public const string TOWN = "town";

        public const string AVAILABILITY = "availability";
        public const string MONTHLY_RULES = "monthlyRules";
        public const string DAILY_RULES = "dailyRules";
        public const string MONTHS = "months";
        public const string DAYS = "days";
        public const string HOURS = "hours";
        public const string OPEN_HOUR = "openHour";
        public const string CLOSE_HOUR = "closeHour";
        public const string SPECIAL = "special";
        public const string DATES = "dates";
        public const string REPEATABLE = "repeatable";

        public static Coordinates ReadCoordinatesFromGeoJson(dynamic json)
        {
            var geometryJson = json[GEOMETRY];
            return new Coordinates(
                (double)geometryJson[COORDINATES][0],
                (double)geometryJson[COORDINATES][1]);
        }

        public static void WriteCoordinatesInGeoJson(dynamic geometryJson, Coordinates coordinates)
        {
            geometryJson[TYPE] = POINT;
            geometryJson[COORDINATES] = new JArray
            {
                (double)coordinates.Longitude,
                (double)coordinates.Latitude
            };
        }

        public static JObject CreateGeoJson(dynamic geometryJson, dynamic propertiesJson)
        {
            dynamic json = new JObject();
            json[TYPE] = "Feature";
            json[GEOMETRY] = geometryJson;
            json[PROPERTIES] = propertiesJson;
            return json;
        }

        public static Address ReadAddress(dynamic json)
        {
            return new Address(
                    (string)json[ADDRESS][STREET],
                    (string?)json[ADDRESS][HOUSE],
                    (string?)json[ADDRESS][FLAT],
                    (string?)json[ADDRESS][FLOOR],
                    (string?)json[ADDRESS][POSTAL_CODE],
                    (string?)json[ADDRESS][TOWN]);
        }

        public static void WriteAddress(dynamic json, Address address)
        {
            json[ADDRESS] = new JObject
            {
                { STREET, address.Street },
                { HOUSE, address.House },
                { FLAT, address.Flat },
                { FLOOR, address.Floor },
                { POSTAL_CODE, address.PostalCode },
                { TOWN, address.Town }
            };
        }

        public static Availability ReadAvailability(dynamic json)
        {
            dynamic availabilityJson = json[AVAILABILITY];

            List<MonthlyRule> monthlyRules = new List<MonthlyRule>();

            foreach (var montlyRuleJson in availabilityJson[MONTHLY_RULES])
            {
                List<string> months = new List<string>();

                foreach (var month in montlyRuleJson[MONTHS])
                {
                    months.Add(month.ToString());
                }

                List<DailyRule> dailyRules = new List<DailyRule>();

                foreach (var dailyRuleJson in montlyRuleJson[DAILY_RULES])
                {
                    List<string> days = new List<string>();

                    foreach (var day in dailyRuleJson[DAYS])
                    {
                        days.Add(day.ToString());
                    }

                    List<HourRule> hours = new List<HourRule>();

                    foreach (var hoursJson in dailyRuleJson[HOURS])
                    {
                        hours.Add(new HourRule((string)hoursJson[OPEN_HOUR], (string)hoursJson[CLOSE_HOUR]));
                    }

                    dailyRules.Add(new DailyRule(days, hours));
                }

                monthlyRules.Add(new MonthlyRule(months, dailyRules));
            }

            List<SpecialRule> specialRules = new List<SpecialRule>();
            foreach (var specialRuleJson in availabilityJson[SPECIAL])
            {
                if (specialRuleJson[DATES] == null)
                {
                    continue;
                }

                List<string> dates = new List<string>();

                foreach (var date in specialRuleJson[DATES])
                {
                    dates.Add(date.ToString());
                }

                bool repeatable = specialRuleJson[REPEATABLE];

                List<HourRule> hours = new List<HourRule>();

                if (specialRuleJson[HOURS] != null)
                {
                    foreach (var hoursJson in specialRuleJson[HOURS])
                    {
                        hours.Add(new HourRule((string)hoursJson[OPEN_HOUR], (string)hoursJson[CLOSE_HOUR]));
                    }
                }

                specialRules.Add(new SpecialRule(dates, repeatable, hours));
            }

            return new Availability(monthlyRules, specialRules);
        }

        public static void WriteAvailability(dynamic json, Availability availability)
        {
            var monthlyRulesJson = new JArray();

            foreach (var monthlyRule in availability.MonthlyRules)
            {
                var monthsJson = new JArray();

                foreach (var month in monthlyRule.Months)
                {
                    monthsJson.Add(month.ToString());
                }

                var dailyRulesJson = new JArray();

                foreach (var dailyRule in monthlyRule.DailyRules)
                {
                    var daysJson = new JArray();

                    foreach (var day in dailyRule.Days)
                    {
                        daysJson.Add(day.ToString());
                    }

                    var hourRulesJson = new JArray();

                    foreach (var hourRule in dailyRule.HourRules)
                    {
                        hourRulesJson.Add(new JObject
                        {
                            { OPEN_HOUR, hourRule.OpenHour },
                            { CLOSE_HOUR, hourRule.CloseHour }
                        });
                    }

                    dailyRulesJson.Add(new JObject
                    {
                        { DAYS, daysJson },
                        { HOURS, hourRulesJson }
                    });
                }

                monthlyRulesJson.Add(new JObject
                {
                    { MONTHS, monthsJson },
                    { DAILY_RULES, dailyRulesJson }
                });
            }

            var specialRulesJson = new JArray();

            foreach (var specialRule in availability.SpecialRules)
            {
                var datesJson = new JArray();

                foreach (var date in specialRule.Dates)
                {
                    datesJson.Add(date.ToString());
                }

                var hourRulesJson = new JArray();

                foreach (var hourRule in specialRule.HourRules)
                {
                    hourRulesJson.Add(new JObject
                    {
                        { OPEN_HOUR, hourRule.OpenHour },
                        { CLOSE_HOUR, hourRule.CloseHour }
                    });
                }

                specialRulesJson.Add(new JObject
                {
                    { DATES, datesJson },
                    { REPEATABLE, specialRule.Repeatable },
                    { HOURS, hourRulesJson },
                });
            }

            json[AVAILABILITY] = new JObject();
            json[AVAILABILITY][MONTHLY_RULES] = monthlyRulesJson;
            json[AVAILABILITY][SPECIAL] = specialRulesJson;
        }
    }
}
