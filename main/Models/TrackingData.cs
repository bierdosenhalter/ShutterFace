namespace ShutterFace
{
    [Serializable]
    public class TrackingData
    {
        public List<TrackingRect> TrackingRects { get; set; } = new();
        public string VideoPath { get; set; } = string.Empty;
    }
}