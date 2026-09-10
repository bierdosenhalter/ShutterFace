using ShutterFace.Converters;
using ShutterFace.DataObjects;
using System.Text.Json;

namespace ShutterFace.FileHandling
{
    /// <summary>
    /// Serializes and deserializes tracking data for *.sft project files.
    /// Owns the JSON options so the form stays free of persistence details.
    /// </summary>
    internal static class TrackerStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            Converters = { new RectJsonConverter() }
        };

        public static string Serialize(IReadOnlyList<TrackerBox> rects, string videoPath, int videoWidth = 0, int videoHeight = 0)
        {
            var data = new TrackerSession
            {
                TrackingRects = [.. rects],
                VideoPath = videoPath,
                VideoWidth = videoWidth,
                VideoHeight = videoHeight
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
                catch (JsonException)
                {
                    // Ignore and return null default
                }
            }

            return result;
        }
    }
}
