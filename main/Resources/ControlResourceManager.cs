namespace ShutterFace.Resources
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Resources;

    internal static class ControlResourceManager
    {
        private static readonly ResourceManager s_rm = new("ShutterFace.Resources.Strings", typeof(ControlResourceManager).Assembly);
        private static readonly List<CultureInfo> _availableCultures = new()
        {
            new CultureInfo("en-US"),
            new CultureInfo("de-DE"),
        };

        private static CultureInfo _currentCulture;

        private static readonly ConcurrentDictionary<string, string?> s_stringCache = new();

        static ControlResourceManager()
        {
            _currentCulture = CultureInfo.GetCultureInfo("en-US");
        }

        public static CultureInfo Culture
        {
            set
            {
                _currentCulture = value ?? CultureInfo.GetCultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = _currentCulture;
                CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;
                s_stringCache.Clear();
            }
            get => _currentCulture;
        }

        public static string GetString(string name)
        {
            if (s_stringCache.TryGetValue(name, out var cached))
            {
                return cached ?? string.Empty;
            }

            var value = s_rm.GetString(name, CultureInfo.CurrentUICulture);
            if (value != null)
            {
                s_stringCache[name] = value;
                return value;
            }

            LogMissingResource(name);
            string fallback = $"[MISSING:{name}]";
            s_stringCache[name] = fallback;
            return fallback;
        }

        public static string FormatString(string name, params object?[] args)
        {
            string format = GetString(name);
            return string.IsNullOrEmpty(format) ? string.Empty : string.Format(CultureInfo.InvariantCulture, format, args);
        }

        private static void LogMissingResource(string name)
        {
            System.Diagnostics.Debug.Write($"[MISSING RESOURCE: {name}]{Environment.NewLine}");
        }

        public static ResourceManager ResourceManager => s_rm;

        public static IEnumerable<CultureInfo> AvailableCultures => _availableCultures;
    }
}
