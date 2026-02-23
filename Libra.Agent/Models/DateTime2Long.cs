using System.Runtime.CompilerServices;

namespace Libra.Agent.Extensions
{
    public static class DateTimeExtensions
    {
        // Unix 纪元起点: 1970-01-01 00:00:00 UTC
        private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        #region DateTime → long

        /// <summary>
        /// DateTime 转 Unix 时间戳（秒，UTC）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestamp(this DateTime dateTime)
            => dateTime.ToUniversalTime().Subtract(UnixEpoch).Ticks / TimeSpan.TicksPerSecond;

        /// <summary>
        /// DateTime 转 Unix 时间戳（毫秒，UTC）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestampMs(this DateTime dateTime)
            => dateTime.ToUniversalTime().Subtract(UnixEpoch).Ticks / TimeSpan.TicksPerMillisecond;

        /// <summary>
        /// DateTime? 安全转 Unix 时间戳（秒）
        /// </summary>
        public static long? ToUnixTimestampSafe(this DateTime? dateTime)
            => dateTime?.ToUnixTimestamp();

        /// <summary>
        /// DateTime? 安全转 Unix 时间戳（毫秒）
        /// </summary>
        public static long? ToUnixTimestampMsSafe(this DateTime? dateTime)
            => dateTime?.ToUnixTimestampMs();

        #endregion

        #region long → DateTime

        /// <summary>
        /// Unix 时间戳（秒）转 DateTime（UTC）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTimeFromUnix(this long unixTimestamp)
            => UnixEpoch.AddSeconds(unixTimestamp);

        /// <summary>
        /// Unix 时间戳（毫秒）转 DateTime（UTC）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToDateTimeFromUnixMs(this long unixTimestampMs)
            => UnixEpoch.AddMilliseconds(unixTimestampMs);

        /// <summary>
        /// Unix 时间戳（秒）转 DateTime（本地时区）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToLocalDateTimeFromUnix(this long unixTimestamp)
            => unixTimestamp.ToDateTimeFromUnix().ToLocalTime();

        /// <summary>
        /// Unix 时间戳（毫秒）转 DateTime（本地时区）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime ToLocalDateTimeFromUnixMs(this long unixTimestampMs)
            => unixTimestampMs.ToDateTimeFromUnixMs().ToLocalTime();

        /// <summary>
        /// long? 安全转 DateTime（UTC）
        /// </summary>
        public static DateTime? ToDateTimeFromUnixSafe(this long? unixTimestamp)
            => unixTimestamp?.ToDateTimeFromUnix();

        /// <summary>
        /// long? 安全转 DateTime（UTC，毫秒精度）
        /// </summary>
        public static DateTime? ToDateTimeFromUnixMsSafe(this long? unixTimestampMs)
            => unixTimestampMs?.ToDateTimeFromUnixMs();

        #endregion

        #region DateTimeOffset 扩展

        /// <summary>
        /// DateTimeOffset 转 Unix 时间戳（秒）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestamp(this DateTimeOffset dateTimeOffset)
            => dateTimeOffset.ToUnixTimeSeconds();

        /// <summary>
        /// DateTimeOffset 转 Unix 时间戳（毫秒）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long ToUnixTimestampMs(this DateTimeOffset dateTimeOffset)
            => dateTimeOffset.ToUnixTimeMilliseconds();

        /// <summary>
        /// Unix 时间戳（秒）转 DateTimeOffset（UTC）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset ToDateTimeOffsetFromUnix(this long unixTimestamp)
            => DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);

        /// <summary>
        /// Unix 时间戳（毫秒）转 DateTimeOffset（UTC）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTimeOffset ToDateTimeOffsetFromUnixMs(this long unixTimestampMs)
            => DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMs);

        #endregion
    }
}
