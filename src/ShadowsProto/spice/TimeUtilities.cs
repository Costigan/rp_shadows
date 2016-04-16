using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Shadow.spice
{
    /// <summary>
    /// Static methods for manipulating timestamps
    /// </summary>
    public static class TimeUtilities
    {
        public static DateTime Epoch = new DateTime(2000, 1, 1, 11, 58, 55, 816);
        public static DateTime TTEpoch = new DateTime(2000, 1, 1, 12, 0, 0, 0);

        public enum TimeFormat
        {
            ITOS,
            FDS,
            DateTimeString,
            ET,
            STK
        };

        public static TimeFormat StringFormat = TimeFormat.ITOS;

        public static Regex DOYDateRecognizer = null;

        public static UInt32 Time42ToSeconds(long time42)
        {
            return (UInt32) (time42 >> 16);
        }

        public static UInt16 Time42ToSubseconds(long time42)
        {
            return (UInt16) (time42 & 0xFFFFL);
        }

        public static float Time42ToSubsecondsFLoat(long time42)
        {
            return (time42 & 0xFFFFL)/65536f;
        }

        public static DateTime Time42ToDateTime(long time42)
        {
            if (time42 > 31020971732762L || time42 < 0L)
                time42 = 31020971732762;   // clipping due to bugs
            else if (time42 < 0L)
                time42 = 0L;
            long seconds = time42 >> 16;
            long subseconds = time42 & 0xFFFFL;
            const long tickResolution = 10000000L; // Ticks per second
            long coarseTicks = seconds*tickResolution;
            double ffine = (subseconds)/65536.0D;
            var fineTicks = (long) (ffine*tickResolution);
            long ticks = coarseTicks + fineTicks;
            DateTime date = Epoch.AddTicks(ticks);
            return date;
        }

        public static DateTime Time40ToDateTime(UInt32 time40)
        {
            return Epoch.AddTicks(10000000L * time40);
        }

        public static DateTime Time42ToTTDateTime(long time42)
        {
            long seconds = time42 >> 16;
            long subseconds = time42 & 0xFFFFL;
            const long tickResolution = 10000000L; // Ticks per second
            long coarseTicks = seconds * tickResolution;
            double ffine = (subseconds) / 65536.0D;
            var fineTicks = (long)(ffine * tickResolution);
            long ticks = coarseTicks + fineTicks;
            DateTime date = TTEpoch.AddTicks(ticks);
            return date;
        }

        public static string Time42ToString(long time42)
        {
            switch (StringFormat)
            {
                case TimeFormat.ITOS:
                    return Time42ToITOS(time42);
                case TimeFormat.FDS:
                    return Time42ToLogan(time42);
                case TimeFormat.DateTimeString:
                    return Time42ToDateTimeString(time42);
                case TimeFormat.ET:
                    return Time42ToEtString(time42);
                case TimeFormat.STK:
                    return Time42ToSTK(time42);
                default:
                    return Time42ToITOS(time42);
            }
        }

        internal static dynamic Time40ToString(UInt32 time40)
        {
            switch (StringFormat)
            {
                case TimeFormat.ITOS:
                    return Time40ToITOS(time40);
                case TimeFormat.FDS:
                    return Time40ToLogan(time40);
                case TimeFormat.DateTimeString:
                    return Time40ToDateTimeString(time40);
                default:
                    return Time40ToITOS(time40);
            }
        }

        public static string Time42ToITOS(long time42)
        {
            var dt = Time42ToDateTime(time42);
            var result = dt.ToString("yy-") + dt.DayOfYear.ToString("000");
            result = result + dt.ToString("-HH:mm:ss.") + dt.Millisecond.ToString("000");
            return result;
        }

        public static string Time40ToITOS(UInt32 time40)
        {
            var dt = Time40ToDateTime(time40);
            var result = dt.ToString("yy-") + dt.DayOfYear.ToString("000");
            result = result + dt.ToString("-HH:mm:ss.") + dt.Millisecond.ToString("000");
            return result;
        }

        public static string Time42ToSTK(long time42)
        {
            var dt = Time42ToDateTime(time42);
            return dt.ToString("dd MMM yyyy HH:mm:ss.fff");
        }

        public static string Time40ToSTK(UInt32 time40)
        {
            var dt = Time42ToDateTime(time40);
            dt = dt.AddSeconds(-3);
            return dt.ToString("dd MMM yyyy HH:mm:ss.fff");
        }

        public static string Time42ToEtString(long time42)
        {
            return Time42ToET(time42).ToString(CultureInfo.InvariantCulture);
        }

        public static long STKToTime42(string s)
        {
            DateTime result;
            return DateTime.TryParse(s, out result) ? DateTimeToTime42(result) : 0;
        }

        public static string DateTimeToString(DateTime dt)
        {
            var result = dt.ToString("yy-") + dt.DayOfYear.ToString("000");
            result = result + dt.ToString("-HH:mm:ss.") + dt.Millisecond.ToString("000");
            return result;
        }

        public static string FileTimestamp(DateTime dt)
        {
            return dt.ToString("yy") + dt.DayOfYear.ToString("000") + dt.ToString("HHmmss");
        }

        public static string Time42ToLogan(long time42)
        {
            return Time42ToDateTime(time42).ToString("yy-MM-dd HH:mm:ss.ffffff");
        }

        public static string Time40ToLogan(UInt32 time40)
        {
            return Time40ToDateTime(time40).ToString("yy-MM-dd HH:mm:ss.ffffff");
        }

        public static string Time42ToDateTimeString(long time42)
        {
            return Time42ToDateTime(time42).ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
        }

        public static string Time40ToDateTimeString(UInt32 time40)
        {
            return Time40ToDateTime(time40).ToString("yyyy-MM-ddTHH:mm:ss.ffffff");
        }

        public static string ScTimeToGseString(uint coarse, uint fine)
        {
            // To convert from epoch UTC (which should be 1/1/1970
            // 00:00:00 UTC) to a human readable time, you'll need to
            // find the number of ticks between the DateTime class'
            // base time (1/1/0001 00:00:00) to epoch time. You
            // multiply your epoch time by the tick resolution (100
            // nanoseconds / tick) and add your base ticks (epoch time
            // - base time). Then you pass the ticks into the DateTime
            // constructor and get a nice human-readable result. Here
            // is an example:

            //long baseTicks = 621355968000000000;
            const long tickResolution = 10000000;
            long coarseTicks = (coarse*tickResolution);
            double ffine = fine/65536.0;
            var fineTicks = (long) (ffine*tickResolution);
            long ticks = coarseTicks + fineTicks;
            DateTime date = Epoch.AddTicks(ticks);
            //DateTime date = new DateTime(ticks, DateTimeKind.Utc);
            var msec = (int) (ffine*1000d);
            return date.ToString("yy-MM-dd HH:mm:ss.") + msec.ToString("000");
            //return date.ToLongDateString() + " " + date.ToLongTimeString();
        }

        public static Int64 StringToTime42(string datestring)
        {
            var d = DateTime.Parse(datestring);
            return DateTimeToTime42(d);
        }

        //        public static string TimeTypeToString(TimeType time)
        //        {
        //            // To convert from epoch UTC (which should be 1/1/1970
        //            // 00:00:00 UTC) to a human readable time, you'll need to
        //            // find the number of ticks between the DateTime class'
        //            // base time (1/1/0001 00:00:00) to epoch time. You
        //            // multiply your epoch time by the tick resolution (100
        //            // nanoseconds / tick) and add your base ticks (epoch time
        //            // - base time). Then you pass the ticks into the DateTime
        //            // constructor and get a nice human-readable result. Here
        //            // is an example:
        //
        //            long fine = (long)(0xFFFF & time);
        //            long coarse = (long)(time >> 16);
        //            long tickResolution = 10000000;
        //            long coarseTicks = (coarse * tickResolution);
        //            double ffine = fine / 65536.0;
        //            long fineTicks = (long)(ffine * tickResolution);
        //            long ticks = coarseTicks + fineTicks;
        //            DateTime date = Epoch.AddTicks(ticks);
        //            return date.ToString("yy-MM-dd HH:mm:ss.") + date.Millisecond.ToString("000");
        //        }

        //        public static DateTime Time42ToDateTime(TimeType time)
        //        {
        //            long fine = (long)(0xFFFF & time);
        //            long coarse = (long)(time >> 16);
        //            long tickResolution = 10000000;
        //            long coarseTicks = (coarse * tickResolution);
        //            double ffine = fine / 65536.0;
        //            long fineTicks = (long)(ffine * tickResolution);
        //            long ticks = coarseTicks + fineTicks;
        //            DateTime date = Epoch.AddTicks(ticks);
        //            return date;
        //        }

        public static Int64 DateTimeToTime42(DateTime time)
        {
            TimeSpan span = time - Epoch;
            long seconds = span.Days*86400L + span.Hours*3600L + span.Minutes*60L + span.Seconds;
            long fine = (span.Milliseconds*65536)/1000;
            long v = (seconds << 16) | fine;
            return v;
        }

        public static DateTime ETToDateTime(double et)
        {
            var ticks = (long) (et*10000000L);
            DateTime result = Epoch.AddTicks(ticks);
            return result;
        }

        public static double DateTimeToET(DateTime time)
        {
            return (time - Epoch).TotalSeconds + 3d;   // The 3D accounts for leap seconds since 2000.  This is valid only for dates after Jul 1 2012.
        }

        public static long ETToTime42(double et)
        {
            return (long) ((et-3d)*65536D);
        }

        public static double Time42ToET(long time42)
        {
            return time42/65536D + 3d;
        }

        public static Int64 Time42FromSecondsSubseconds(uint seconds, ushort subseconds)
        {
            return ((seconds << 16) | subseconds);
        }

        public static long Time42ToTicks(long time42)
        {
            return (long) (time42*(10000000D/65536D));
        }

        public static DateTime InjectorEpoch = new DateTime(2000, 1, 1, 11, 58, 56, 0);

        public static long Time42ToInjectorSeconds(long time42)
        {
            DateTime dt = Time42ToDateTime(time42);
            TimeSpan span = dt - InjectorEpoch;
            long totalSeconds = span.Days*3600L*24L + span.Hours*3600L + span.Minutes*60L + span.Seconds;
            return totalSeconds;
        }

        public static long Time42ToInjectorSubseconds(long time42)
        {
            DateTime dt = Time42ToDateTime(time42);
            int msec = dt.Millisecond;
            return msec*1000L;
        }

        public static DateTime? ParseDOYTime(string text)
        {
            try
            {
                if (DOYDateRecognizer == null)
                    DOYDateRecognizer = new Regex(@"^(\d\d)-(\d\d\d)-(\d\d):(\d\d):(\d\d).(\d\d\d)[ ]*$",
                                                  RegexOptions.IgnoreCase | RegexOptions.Compiled |
                                                  RegexOptions.Singleline);
                var match = DOYDateRecognizer.Match(text);
                if (!match.Success) return null;
                int year = int.Parse(match.Groups[1].Value) + 2000;
                if (year < 2000 || year > 2099) return null;
                int doy = int.Parse(match.Groups[2].Value);
                if (doy > 366 || doy == 366 && !DateTime.IsLeapYear(year)) return null;
                int hour = int.Parse(match.Groups[3].Value);
                if (hour > 23) return null;
                int minute = int.Parse(match.Groups[4].Value);
                if (minute > 59) return null;
                int second = int.Parse(match.Groups[5].Value);
                if (second > 59) return null;
                int msec = int.Parse(match.Groups[6].Value);
                var result = new DateTime(year, 1, 1, hour, minute, second, msec, DateTimeKind.Utc);
                result = result.AddDays(doy - 1);
                if (result < Epoch) return null;
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static TimeSpan Time42ToTimeSpan(long time42)
        {
            return Time42ToDateTime(time42) - Epoch;
        }

        public static string TimeSpanToString(TimeSpan span)
        {
            return span.ToString();
        }

        public static string Time42ToSpanString(long time42)
        {
            return TimeSpanToString(Time42ToTimeSpan(time42));
        }

    }
}