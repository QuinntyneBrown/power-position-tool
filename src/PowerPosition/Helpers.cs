using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerPosition
{
    public static class Helpers
    {
        private static readonly TimeZoneInfo GmtTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

        /// <summary>
        /// Calculates the settlement day UTC start and end times and the number of periods in that day.
        /// </summary>
        /// <param name="date"></param>
        /// <returns>Returns a tuple comprising (startTime,endTime,numberOfPeriods).</returns>
        public static SettlementDay SettlementDay(DateTimeOffset date)
        {
            var startTime =
                new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Unspecified).Date.AddHours(-1.0);
            var endTime = startTime.AddDays(1.0);
            var startTimeUtc = TimeZoneInfo.ConvertTimeToUtc(startTime, GmtTimeZoneInfo);
            var endTimeUtc = TimeZoneInfo.ConvertTimeToUtc(endTime, GmtTimeZoneInfo);
            var numberOfPeriods = (int)endTimeUtc.Subtract(startTimeUtc).TotalHours;
            return new SettlementDay {StartUtc = startTimeUtc, EndUtc = endTimeUtc, NumberOfPeriods = numberOfPeriods};
        }

        /// <summary>
        /// Calculates all of the settlement period local start times for a settlement day.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static IEnumerable<DateTime> LocalTimes(DateTimeOffset date)
        {
            var settlementDay = SettlementDay(date);
            return
                Enumerable.Range(1, settlementDay.NumberOfPeriods)
                    .Select(
                        i =>
                            TimeZoneInfo.ConvertTimeFromUtc(settlementDay.StartUtc.AddHours(i - 1.0), GmtTimeZoneInfo));
        }
    }
}