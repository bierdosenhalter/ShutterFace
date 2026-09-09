namespace ShutterFace.Resources
{
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class CultureConfig
    {
        private static readonly string s_configDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ShutterFace");

        private static readonly string s_configPath = Path.Combine(s_configDir, "culture.json");

        static CultureConfig()
        {
            if (!Directory.Exists(s_configDir))
            {
                Directory.CreateDirectory(s_configDir);
            }
        }

        public static CultureInfo? LoadDefaultCulture()
        {
            if (!File.Exists(s_configPath))
            {
                return null;
            }

            try
            {
                var json = File.ReadAllText(s_configPath);
                var cultureName = ExtractCultureName(json);
                if (!string.IsNullOrEmpty(cultureName))
                {
                    return CultureInfo.GetCultureInfo(cultureName);
                }
            }
            catch
            {
                // If config file is corrupted, ignore it
            }

            return null;
        }

        public static void SaveCulture(CultureInfo culture)
        {
            var json = $"{{\"culture\":\"{culture.Name}\"}}";
            try
            {
                File.WriteAllText(s_configPath, json);
            }
            catch
            {
                // If we can't save, just ignore it
            }
        }

        private static string? ExtractCultureName(string json)
        {
            const string key = "\"culture\":\"";
            var start = json.IndexOf(key, StringComparison.Ordinal);
            if (start < 0) return null;

            var valueStart = start + key.Length;
            var end = json.IndexOf('"', valueStart);
            if (end < 0) return null;

            return json.Substring(valueStart, end - valueStart);
        }
    }
}
