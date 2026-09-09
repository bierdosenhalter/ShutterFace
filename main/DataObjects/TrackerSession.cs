namespace ShutterFace.DataObjects
{
    [Serializable]
    public class TrackerSession
    {
        public List<TrackerBox> TrackingRects { get; set; } = new();
        public string VideoPath { get; set; } = string.Empty;
    }
}