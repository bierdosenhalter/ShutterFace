using OpenCvSharp;
using System.Text.Json;

namespace ShutterFace.Tests;

public class SerializationTests
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

    private static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new RectJsonConverter() }
    };

    [Fact]
    public void Read_WritesAndReadsBack_ReturnsSameDimensions()
    {
        var rect = new Rect(100, 200, 50, 60);
        var json = JsonSerializer.Serialize(rect, Options);

        var result = JsonSerializer.Deserialize<Rect>(json, Options);
        Assert.Equal(100, result.X);
        Assert.Equal(200, result.Y);
        Assert.Equal(50, result.Width);
        Assert.Equal(60, result.Height);
    }

    [Fact]
    public void Read_HandlesZeroValues()
    {
        var json = "{\"X\":0,\"Y\":0,\"Width\":0,\"Height\":0}";
        var result = JsonSerializer.Deserialize<Rect>(json, Options);
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(0, result.Width);
        Assert.Equal(0, result.Height);
    }

    [Fact]
    public void Write_CanBeReadWithLegacyPropertyName_Size()
    {
        // Existing .track files may have "Size" instead of "Width"
        var json = "{\"X\":10,\"Y\":20,\"Size\":{\"width\":30,\"height\":40}}";
        var result = JsonSerializer.Deserialize<Rect>(json, Options);
        Assert.Equal(10, result.X);
        Assert.Equal(20, result.Y);
    }
}
