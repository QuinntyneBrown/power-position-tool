using NUnit.Framework;
using System;
using System.Linq;
using Services;

namespace PowerPosition.Tests
{
    [TestFixture]
    public class PositionReportTests
    {
        private static readonly DateTimeOffset SummerDay = new DateTimeOffset(2015, 5, 10, 16, 7, 0, TimeSpan.FromHours(1.0));

        [Test]
        public void ValidateValidPowerTrades()
        {
            var powerTrades = new PowerTrades { Day = SummerDay, Trades = Enumerable.Range(1, 2).Select(i => PowerTrade.Create(SummerDay.DateTime, 24)).ToArray() };
            var actual = PositionReport.ValidateTrades(powerTrades);
            Assert.That(actual.Day, Is.EqualTo(powerTrades.Day));
            for (var i = 0; i < powerTrades.Trades.Count(); i++)
            {
                CollectionAssert.AreEqual(actual.Trades[i].Periods, powerTrades.Trades[i].Periods, new PowerPeriodComparer(0.0001));                
            }
        }

        [Test]
        public void ValidateInvalidPowerTrades()
        {
            var powerTrades = new PowerTrades { Day = SummerDay, Trades = new[] { PowerTrade.Create(SummerDay.DateTime, 24), PowerTrade.Create(SummerDay.DateTime, 23)} };
            Assert.Throws<ArgumentException>(() => PositionReport.ValidateTrades(powerTrades));            
        }

        [Test]
        public void BuildPositionNoPowerTrades()
        {
            var powerTrade = PowerTrade.Create(SummerDay.DateTime, 24);
            var trades = new PowerTrades { Day = SummerDay, Trades = new PowerTrade[0] };
            var actual = PositionReport.BuildPosition(trades);
            var expectedPeriods = powerTrade.Periods;
            Assert.That(actual.Day, Is.EqualTo(trades.Day));
            CollectionAssert.AreEqual(expectedPeriods, actual.Periods, new PowerPeriodComparer(0.0001));
        }

        [Test]
        public void BuildPositionOnePowerTrade()
        {
            var powerTrade = PowerTrade.Create(SummerDay.DateTime, 24);
            var trades = new PowerTrades { Day = SummerDay, Trades = new[] { powerTrade } };
            var actual = PositionReport.BuildPosition(trades);
            var expectedPeriods = powerTrade.Periods;
            Assert.That(actual.Day, Is.EqualTo(trades.Day));
            CollectionAssert.AreEqual(expectedPeriods, actual.Periods, new PowerPeriodComparer(0.0001));            
        }

        [Test]
        public void BuildPositionMultiplePowerTrades()
        {
            var powerTrade = CreatePowerTrade(SummerDay, 24);
            var trades = new PowerTrades { Day = SummerDay, Trades = new[] { powerTrade, powerTrade, powerTrade } };
            var actual = PositionReport.BuildPosition(trades);
            var expectedPeriods = powerTrade.Periods.Select(p => new PowerPeriod {Period = p.Period, Volume = p.Volume * 3.0}).ToArray();
            Assert.That(actual.Day, Is.EqualTo(trades.Day));
            CollectionAssert.AreEqual(expectedPeriods, actual.Periods, new PowerPeriodComparer(0.0001));            
        }

        [Test]
        public void BuildReport()
        {
            var reportSpec = new ReportSpecification {Headers = "Local Time,Volume", LocalTimeFormat = "HH:mm"};
            var periods = Enumerable.Range(1, 24).Select(i => new PowerPeriod {Period = i, Volume = i}).ToArray();
            var position = new Position {Day = SummerDay, Periods = periods};
            var actual = PositionReport.BuildReport(reportSpec,position);
            var expectedLocalTimes = new[]
            {
                "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00",
                "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00",
                "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00"
            };
            var expectedVolumes = Enumerable.Range(1, 24).ToArray();
            var expectedContents = Enumerable.Repeat(reportSpec.Headers, 1).Concat(expectedLocalTimes.Zip(expectedVolumes, (t, v) => string.Format("{0},{1}", t, v)));
            Assert.That(actual.Day, Is.EqualTo(position.Day));
            CollectionAssert.AreEqual(expectedContents, actual.Contents);
        }

        private PowerTrade CreatePowerTrade(DateTimeOffset day, int numberOfPeriods)
        {
            var powerTrade = PowerTrade.Create(day.DateTime, numberOfPeriods);

            for (var i = 0; i < numberOfPeriods; i++)
                powerTrade.Periods[i] = new PowerPeriod {Period = i + 1, Volume = i};

            return powerTrade;
        }
    }

    public class PowerPeriodComparer : System.Collections.IComparer
    {
        private readonly double _epsilon;

        public PowerPeriodComparer(double epsilon)
        {
            _epsilon = epsilon;
        }

        public int Compare(object x, object y)
        {
            var a = (PowerPeriod)x;
            var b = (PowerPeriod)y;

            var dayComparison = a.Period.CompareTo(b.Period);
            if (dayComparison == 0)
            {
                var delta = Math.Abs(a.Volume - b.Volume);
                if (delta < _epsilon)
                    return 0;
                return a.Volume.CompareTo(b.Volume);
            }

            return dayComparison;
        }
    }
}
