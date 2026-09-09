using OpenCvSharp;

namespace ShutterFace.Tests;

public class FrameRendererTests
{
    [Fact]
    public void GetTrackingListText_Analyzed_ReturnsOnePrefix()
    {
        var tracking = new TrackerBox
        {
            Name = "Face 1",
            IsAnalyzed = true
        };

        var result = FrameRenderer.GetTrackingListText(tracking);

        Assert.StartsWith("1", result.TrimStart());
        Assert.Contains("Face 1", result);
    }

    [Fact]
    public void GetTrackingListText_NotAnalyzed_ReturnsZeroPrefix()
    {
        var tracking = new TrackerBox
        {
            Name = "Face 2",
            IsAnalyzed = false
        };

        var result = FrameRenderer.GetTrackingListText(tracking);

        Assert.StartsWith("0", result.TrimStart());
        Assert.Contains("Face 2", result);
    }
}

public class TrackerStoreTests
{
    [Fact]
    public void Serialize_TrackingRects_AndVideoPath_ReturnsJson()
    {
        var rects = new List<TrackerBox>
        {
            new()
            {
                Name = "Face 1",
                StartFrame = 0,
                EndFrame = 100,
                InitialRect = new Rect(10, 20, 30, 40),
                IsAnalyzed = false
            }
        };

        var json = TrackerStore.Serialize(rects, "/path/to/video.mp4");

        Assert.Contains("\"VideoPath\"", json);
        Assert.Contains("/path/to/video.mp4", json);
        Assert.Contains("\"Face 1\"", json);
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsCorrectData()
    {
        var rect = new TrackerBox
        {
            Name = "Tracker A",
            StartFrame = 5,
            EndFrame = 50,
            InitialRect = new Rect(100, 200, 300, 400),
            IsAnalyzed = true
        };

        var json = TrackerStore.Serialize([rect], "/video.mp4");
        var result = TrackerStore.Deserialize(json);

        Assert.NotNull(result);
        Assert.Equal("/video.mp4", result.VideoPath);
        Assert.Single(result.TrackingRects);

        var r = result.TrackingRects[0];
        Assert.Equal("Tracker A", r.Name);
        Assert.Equal(5, r.StartFrame);
        Assert.Equal(50, r.EndFrame);
    }

    [Fact]
    public void SerializeDeserialize_EmptyList_RoundsTrip()
    {
        var json = TrackerStore.Serialize([], "/empty.mp4");
        var result = TrackerStore.Deserialize(json);

        Assert.NotNull(result);
        Assert.Empty(result.TrackingRects);
        Assert.Equal("/empty.mp4", result.VideoPath);
    }

    [Fact]
    public void Deserialize_NullJson_ReturnsNull()
    {
        var result = TrackerStore.Deserialize(null!);

        Assert.Null(result);
    }

    [Fact]
    public void Serialize_MultipleRects_PreservesAllData()
    {
        var rects = new List<TrackerBox>
        {
            new() { Name = "A", StartFrame = 0, EndFrame = 100, InitialRect = new Rect(0, 0, 10, 20) },
            new() { Name = "B", StartFrame = 50, EndFrame = 200, InitialRect = new Rect(30, 40, 50, 60) }
        };

        var json = TrackerStore.Serialize(rects, "/test.mp4");
        var result = TrackerStore.Deserialize(json);

        Assert.NotNull(result);
        Assert.Collection(result.TrackingRects,
            a => Assert.Equal("A", a.Name),
            b => Assert.Equal("B", b.Name)
        );
    }
}
