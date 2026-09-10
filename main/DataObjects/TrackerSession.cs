namespace ShutterFace.DataObjects
{
    [Serializable]
    internal sealed class TrackerSession
    {
        public List<TrackerBox> TrackingRects { get; init; } = new();
        public string VideoPath { get; set; } = string.Empty;
    }
}