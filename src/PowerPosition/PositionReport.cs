using System;
using System.IO;
using System.Linq;
using Services;

namespace PowerPosition
{
    /// <summary>
    /// Comprises all the position reporting logic expressed as steps in a pipelined computation:
    /// </summary>
    public static class PositionReport
    {
        /// <summary>
        /// Retrieves all of the power trades for a given day from the PowerService.
        /// </summary>
        /// <param name="powerService">The source of power trades.</param>
        /// <param name="day">The day to get trades for.</param>
        /// <returns>Returns all of the power trades for the given day.</returns>
        public static PowerTrades GetTrades(IPowerService powerService, DateTimeOffset day)
        {
            return new PowerTrades {Day = day, Trades = powerService.GetTrades(day.DateTime).ToArray()};
        }

        /// <summary>
        /// Validates the power trades for a day. Invalid trades cause an exception to be thrown.
        /// </summary>
        /// <param name="trades">The power trades for a day.</param>
        /// <returns>The power trades for a day.</returns>
        public static PowerTrades ValidateTrades(PowerTrades trades)
        {
            var settlementDay = Helpers.SettlementDay(trades.Day);
            if (trades.Trades.Any(t => t.Periods.Length != settlementDay.NumberOfPeriods))
                throw new ArgumentException(string.Format("expected all power trades for {0} to have {1} periods", trades.Day, settlementDay.NumberOfPeriods), "trades");
            return trades;
        }

        /// <summary>
        /// Aggregates the power trades for a day into the position.
        /// </summary>
        /// <param name="trades">The power trades for a day.</param>
        /// <returns>The position.</returns>
        public static Position BuildPosition(PowerTrades trades)
        {
            var settlementDay = Helpers.SettlementDay(trades.Day);
            var seed = PowerTrade.Create(trades.Day.DateTime, settlementDay.NumberOfPeriods);
            var periods =
                trades.Trades.Select(t => t.Periods)
                .Aggregate(seed.Periods,
                        (periods1, periods2) =>
                            periods1.Zip(periods2,
                                (period1, period2) => new PowerPeriod {Period = period1.Period, Volume = period1.Volume + period2.Volume})
                                .ToArray());
            return new Position {Day = trades.Day, Periods = periods};
        }

        /// <summary>
        /// Builds the report on the position.
        /// </summary>
        /// <param name="reportSpec">Configuration parameters used to format the report.</param>
        /// <param name="position">The position.</param>
        /// <returns>The report.</returns>
        public static Report BuildReport(ReportSpecification reportSpec, Position position)
        {
            var contents =
                Enumerable.Repeat(reportSpec.Headers, 1)
                    .Concat(position.Periods.Zip(Helpers.LocalTimes(position.Day),
                        (period, localTime) => string.Format("{0},{1}", localTime.ToString(reportSpec.LocalTimeFormat), period.Volume))).ToArray();
            return new Report {Day = position.Day, Contents = contents};
        }

        /// <summary>
        /// Writes a report to a file.
        /// </summary>
        /// <param name="reportSpec">Configuration parameters used to write the report.</param>
        /// <param name="report">The report.</param>
        public static void WriteReport(ReportSpecification reportSpec, Report report)
        {
            Directory.CreateDirectory(reportSpec.OutputPath);
            var reportFilename = string.Format(reportSpec.FilenameFormat, report.Day.LocalDateTime.ToString(reportSpec.FilenameDateFormat));
            var reportFilepath = Path.Combine(reportSpec.OutputPath, reportFilename);
            var temporaryFilename = string.Format("{0}.tmp", reportFilename);
            var temporaryFilepath = Path.Combine(reportSpec.OutputPath, temporaryFilename);
            File.WriteAllLines(temporaryFilepath, report.Contents);
            if (File.Exists(reportFilepath))
                File.Delete(reportFilepath);
            File.Move(temporaryFilepath, reportFilepath);
        }
    }
}