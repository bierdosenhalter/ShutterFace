using System.Text.Json;

namespace ShutterFace
{
    /// <summary>
    /// Serializes and deserializes tracking data for *.track files.
    /// Owns the JSON options so the form stays free of persistence details.
    /// </summary>
    internal static class TrackingStore
    {
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new RectJsonConverter() }
        };

        public static string Serialize(IReadOnlyList<TrackingRect> rects, string videoPath)
        {
            var data = new TrackingData
            {
                TrackingRects = [.. rects],
                VideoPath = videoPath
            };

            return JsonSerializer.Serialize(data, SerializerOptions);
        }

        public static TrackingData? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<TrackingData>(json, SerializerOptions);
        }
    }
}
