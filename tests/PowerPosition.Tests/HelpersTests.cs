using System;
using System.Linq;
using NUnit.Framework;

namespace PowerPosition.Tests
{
    [TestFixture]
    public class HelpersTests
    {
        private static readonly DateTimeOffset LongDay = new DateTimeOffset(2014, 10, 26, 12, 0, 0, TimeSpan.FromHours(1.0));
        private static readonly DateTimeOffset WinterDay = new DateTimeOffset(2014, 12, 12, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset ShortDay = new DateTimeOffset(2015, 3, 29, 12, 0, 0, TimeSpan.Zero);
        private static readonly DateTimeOffset SummerDay = new DateTimeOffset(2015, 5, 10, 16, 7, 0, TimeSpan.FromHours(1.0));
        
        [Test]
        public void LongSettlementDay()
        {
            var actual = Helpers.SettlementDay(LongDay);
            Assert.That(actual.StartUtc, Is.EqualTo(new DateTime(2014, 10, 25, 22, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.EndUtc, Is.EqualTo(new DateTime(2014, 10, 26, 23, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.NumberOfPeriods, Is.EqualTo(25));
        }

        [Test]
        public void WinterSettlementDay()
        {
            var actual = Helpers.SettlementDay(WinterDay);
            Assert.That(actual.StartUtc, Is.EqualTo(new DateTime(2014, 12, 11, 23, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.EndUtc, Is.EqualTo(new DateTime(2014, 12, 12, 23, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.NumberOfPeriods, Is.EqualTo(24));
        }

        [Test]
        public void ShortSettlementDay()
        {
            var actual = Helpers.SettlementDay(ShortDay);
            Assert.That(actual.StartUtc, Is.EqualTo(new DateTime(2015, 3, 28, 23, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.EndUtc, Is.EqualTo(new DateTime(2015, 3, 29, 22, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.NumberOfPeriods, Is.EqualTo(23));
        }

        [Test]
        public void SummerSettlementDay()
        {
            var actual = Helpers.SettlementDay(SummerDay);
            Assert.That(actual.StartUtc, Is.EqualTo(new DateTime(2015, 5, 9, 22, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.EndUtc, Is.EqualTo(new DateTime(2015, 5, 10, 22, 0, 0, DateTimeKind.Utc)));
            Assert.That(actual.NumberOfPeriods, Is.EqualTo(24));
        }

        [Test]
        public void LongDayLocalTimes()
        {
            var actual = Helpers.LocalTimes(LongDay).Select(d => d.ToString("HH:mm"));
            var expected = new[]
            {
                "23:00", "00:00", "01:00", "01:00", "02:00", "03:00", "04:00", "05:00",
                "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00",
                "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00",
                "22:00"
            };
            CollectionAssert.AreEqual(expected,actual);
        }

        [Test]
        public void WinterDayLocalTimes()
        {
            var actual = Helpers.LocalTimes(WinterDay).Select(d => d.ToString("HH:mm"));
            var expected = new[]
            {
                "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00",
                "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00",
                "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00"
            };
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void ShortDayLocalTimes()
        {
            var actual = Helpers.LocalTimes(ShortDay).Select(d => d.ToString("HH:mm"));
            var expected = new[]
            {
                "23:00", "00:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00",
                "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00",
                "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00"
            };
            CollectionAssert.AreEqual(expected, actual);
        }

        [Test]
        public void SummerDayLocalTimes()
        {
            var actual = Helpers.LocalTimes(SummerDay).Select(d => d.ToString("HH:mm"));
            var expected = new[]
            {
                "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00",
                "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00",
                "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00"            
            };
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
