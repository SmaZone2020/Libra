using System;
using System.Collections.Generic;
using System.Linq;
namespace Libra.Server.Service
{
    public static class DataStreamLog
    {
        private static readonly object _lock = new();

        public static Dictionary<string, long> Today { get; } = new();

        public static Dictionary<string, long> LastHour { get; } = new();

        public static void Add(long bytes)
        {
            var now = DateTime.Now;

            lock (_lock)
            {
                UpdateToday(now, bytes);
                UpdateLastHour(now, bytes);
            }
        }

        private static void UpdateToday(DateTime now, long bytes)
        {
            string hourKey = now.ToString("HH:00");

            if (!Today.ContainsKey(hourKey))
                Today[hourKey] = 0;

            Today[hourKey] += bytes;

            var todayKeys = Today.Keys
                .Where(k =>
                {
                    if (DateTime.TryParse(k, out var dt))
                        return dt.Date != now.Date;
                    return false;
                })
                .ToList();

            foreach (var key in todayKeys)
                Today.Remove(key);

            if (Today.Count > 24)
            {
                var oldest = Today.Keys
                    .OrderBy(k => DateTime.Parse(k))
                    .First();

                Today.Remove(oldest);
            }
        }

        private static void UpdateLastHour(DateTime now, long bytes)
        {
            string minuteKey = now.ToString("HH:mm");

            if (!LastHour.ContainsKey(minuteKey))
                LastHour[minuteKey] = 0;

            LastHour[minuteKey] += bytes;

            var cutoff = now.AddMinutes(-59);

            var expired = LastHour.Keys
                .Where(k =>
                {
                    if (DateTime.TryParse(k, out var dt))
                    {
                        var fullTime = now.Date.Add(dt.TimeOfDay);
                        return fullTime < cutoff;
                    }
                    return false;
                })
                .ToList();

            foreach (var key in expired)
                LastHour.Remove(key);
        }
    }

    public static class DataStreamLogOutput
    {
        private static readonly object _lock = new();

        public static Dictionary<string, long> Today { get; } = new();

        public static Dictionary<string, long> LastHour { get; } = new();

        public static void Add(long bytes)
        {
            var now = DateTime.Now;

            lock (_lock)
            {
                UpdateToday(now, bytes);
                UpdateLastHour(now, bytes);
            }
        }

        private static void UpdateToday(DateTime now, long bytes)
        {
            string hourKey = now.ToString("HH:00");

            if (!Today.ContainsKey(hourKey))
                Today[hourKey] = 0;

            Today[hourKey] += bytes;

            var todayKeys = Today.Keys
                .Where(k =>
                {
                    if (DateTime.TryParse(k, out var dt))
                        return dt.Date != now.Date;
                    return false;
                })
                .ToList();

            foreach (var key in todayKeys)
                Today.Remove(key);

            if (Today.Count > 24)
            {
                var oldest = Today.Keys
                    .OrderBy(k => DateTime.Parse(k))
                    .First();

                Today.Remove(oldest);
            }
        }

        private static void UpdateLastHour(DateTime now, long bytes)
        {
            string minuteKey = now.ToString("HH:mm");

            if (!LastHour.ContainsKey(minuteKey))
                LastHour[minuteKey] = 0;

            LastHour[minuteKey] += bytes;

            var cutoff = now.AddMinutes(-59);

            var expired = LastHour.Keys
                .Where(k =>
                {
                    if (DateTime.TryParse(k, out var dt))
                    {
                        var fullTime = now.Date.Add(dt.TimeOfDay);
                        return fullTime < cutoff;
                    }
                    return false;
                })
                .ToList();

            foreach (var key in expired)
                LastHour.Remove(key);
        }
    }
}