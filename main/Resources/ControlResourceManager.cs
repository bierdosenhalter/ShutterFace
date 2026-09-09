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
            _currentCulture = CultureInfo.CurrentCulture;
        }

        public static CultureInfo Culture
        {
            set
            {
                _currentCulture = value;
                Thread.CurrentThread.CurrentUICulture = value;
                CultureInfo.DefaultThreadCurrentUICulture = value;
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

            var currentCulture = Thread.CurrentThread.CurrentUICulture;

            var value = s_rm.GetString(name, _currentCulture);
            if (value != null)
            {
                s_stringCache[name] = value;
                return value;
            }

            return string.Empty;
        }

        public static string FormatString(string name, params object?[] args)
        {
            string format = GetString(name);
            return string.IsNullOrEmpty(format) ? string.Empty : string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static ResourceManager ResourceManager => s_rm;

        public static IEnumerable<CultureInfo> AvailableCultures => _availableCultures;
    }
}
