using System.Runtime.CompilerServices;

namespace Libra.Agent.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestamp(this DateTime dateTime)
            => dateTime.ToUniversalTime().Subtract(UnixEpoch).Ticks / TimeSpan.TicksPerSecond;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestampMs(this DateTime dateTime)
            => dateTime.ToUniversalTime().Subtract(UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond;

        public static long? ToUnixTimestampSafe(this DateTime? dateTime)
            => dateTime?.ToUnixTimestamp();

        public static long? ToUnixTimestampMsSafe(this DateTime? dateTime)
            => dateTime?.ToUnixTimestampMs();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTimeFromUnix(this long unixTimestamp)
            => UnixEpoch.AddSeconds(unixTimestamp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTimeFromUnixMs(this long unixTimestampMs)
            => UnixEpoch.AddMilliseconds(unixTimestampMs);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToLocalDateTimeFromUnix(this long unixTimestamp)
            => unixTimestamp.ToDateTimeFromUnix().ToLocalTime();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToLocalDateTimeFromUnixMs(this long unixTimestampMs)
            => unixTimestampMs.ToDateTimeFromUnixMs().ToLocalTime();

        public static DateTime? ToDateTimeFromUnixSafe(this long? unixTimestamp)
            => unixTimestamp?.ToDateTimeFromUnix();

        public static DateTime? ToDateTimeFromUnixMsSafe(this long? unixTimestampMs)
            => unixTimestampMs?.ToDateTimeFromUnixMs();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestamp(this DateTimeOffset dateTimeOffset)
            => dateTimeOffset.ToUnixTimeSeconds();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestampMs(this DateTimeOffset dateTimeOffset)
            => dateTimeOffset.ToUnixTimeMilliseconds();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset ToDateTimeOffsetFromUnix(this long unixTimestamp)
            => DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset ToDateTimeOffsetFromUnixMs(this long unixTimestampMs)
            => DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMs);
    }
}
