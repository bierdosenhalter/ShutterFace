using System.Text.Json;

namespace ShutterFace
{
    /// <summary>
    /// Serializes and deserializes tracking data for *.track files.
    /// Owns the JSON options so the form stays free of persistence details.
    /// </summary>
    internal static class TrackerStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new RectJsonConverter() }
        };

        public static string Serialize(IReadOnlyList<TrackerBox> rects, string videoPath)
        {
            var data = new TrackerSession
            {
                TrackingRects = [.. rects],
                VideoPath = videoPath
            };

            return JsonSerializer.Serialize(data, SerializerOptions);
        }

        public static TrackerSession? Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;

            var result = JsonSerializer.Deserialize<TrackerSession>(json, SerializerOptions);

            if (result == null && !string.IsNullOrEmpty(json))
            {
                try
                {
                    result = new TrackerSession();
                }
                catch
                {
                    // Ignore and return null default
                }
            }

            return result;
        }
    }
}
