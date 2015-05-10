using System;
using System.Collections.Generic;
using System.Threading;
using Services;

namespace PowerPosition
{
    /// <summary>
    /// An error that has occured within the report computation.
    /// </summary>
    public class Error
    {
        public string Context { get; set; }
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// A settlement day.
    /// </summary>
    public class SettlementDay
    {
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public int NumberOfPeriods { get; set; }
    }

    /// <summary>
    /// The power trades for one day.
    /// </summary>
    public class PowerTrades
    {
        public DateTimeOffset Day { get; set; }
        public PowerTrade[] Trades { get; set; }
    }

    /// <summary>
    /// The position for one day.
    /// </summary>
    public class Position
    {
        public DateTimeOffset Day { get; set; }
        public PowerPeriod[] Periods { get; set; }
    }

    /// <summary>
    /// The intra-day day-ahead power position report.
    /// </summary>
    public class Report
    {
        public DateTimeOffset Day { get; set; }
        public string[] Contents { get; set; }
    }

    /// <summary>
    /// The report configuration.
    /// </summary>
    public class ReportSpecification
    {
        public string OutputPath { get; set; }
        public string FilenameFormat { get; set; }
        public string FilenameDateFormat { get; set; }
        public string Headers { get; set; }
        public string LocalTimeFormat { get; set; }
    }

    /// <summary>
    /// The environment within which the report computation is executed.
    /// </summary>
    public class Environment
    {
        public CancellationTokenSource TokenSource { get; set; }
        public TimeSpan Interval { get; set; }
        public IPowerService PowerService { get; set; }
        public Func<IPowerService, DateTimeOffset, PowerTrades> GetTrades { get; set; }
        public Func<PowerTrades, PowerTrades> ValidateTrades { get; set; }
        public Func<PowerTrades, Position> BuildPosition { get; set; }
        public Func<ReportSpecification, Position, Report> BuildReport { get; set; }
        public Action<ReportSpecification, Report> WriteReport { get; set; }
        public int PipelineBufferSize { get; set; }
        public ReportSpecification ReportSpecification { get; set; }
    }

}