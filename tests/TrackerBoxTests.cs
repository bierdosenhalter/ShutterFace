using OpenCvSharp;
using ShutterFace.DataObjects;

namespace ShutterFace.Tests;

public class TrackerBoxTests
{
    [Fact]
    public void GetRectAtFrame_ReturnsExactPosition_WhenFrameHasTrackedPosition()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(10, new Rect(100, 200, 50, 60));

        var result = rect.GetRectAtFrame(10);

        Assert.NotNull(result);
        Assert.Equal(100, result.Value.X);
        Assert.Equal(200, result.Value.Y);
    }

    [Fact]
    public void GetRectAtFrame_ReturnsInterpolatedPosition_WhenFrameIsBetweenTrackedFrames()
    {
        var rect = new TrackerBox();
        // Frame 0: (0, 0)
        rect.AddFramePosition(0, new Rect(0, 0, 100, 100));
        // Frame 10: (100, 100)
        rect.AddFramePosition(10, new Rect(100, 100, 200, 200));

        // Frame 5 should be exactly halfway
        var result = rect.GetRectAtFrame(5);

        Assert.NotNull(result);
        Assert.Equal(50, result.Value.X);
        Assert.Equal(50, result.Value.Y);
        Assert.Equal(150, result.Value.Width);
        Assert.Equal(150, result.Value.Height);
    }

    [Fact]
    public void GetRectAtFrame_ReturnsFirstFramePosition_WhenBeforeFirstTrackedFrame()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(10, new Rect(500, 500, 80, 80));

        var result = rect.GetRectAtFrame(0);

        Assert.NotNull(result);
        Assert.Equal(500, result.Value.X);
    }

    [Fact]
    public void GetRectAtFrame_ReturnsLastFramePosition_WhenAfterLastTrackedFrame()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(10, new Rect(300, 400, 90, 90));

        var result = rect.GetRectAtFrame(50);

        Assert.NotNull(result);
        Assert.Equal(300, result.Value.X);
    }

    [Fact]
    public void GetRectAtFrame_ReturnsNull_WhenNoTrackedPositions()
    {
        var rect = new TrackerBox();

        var result = rect.GetRectAtFrame(5);

        Assert.Null(result);
    }

    [Fact]
    public void ClearPositions_ClearsAllState()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(0, new Rect(0, 0, 100, 100));
        rect.AddFramePosition(10, new Rect(100, 100, 200, 200));
        rect.IsAnalyzed = true;

        rect.ClearPositions();

        Assert.Empty(rect.GetRectPositions());
        Assert.Null(rect.PreviousRect);
        Assert.False(rect.IsAnalyzed);
    }

    [Fact]
    public void RemoveFramePosition_RemovesSpecificFrame()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(0, new Rect(0, 0, 100, 100));
        rect.AddFramePosition(5, new Rect(50, 50, 150, 150));
        rect.AddFramePosition(10, new Rect(100, 100, 200, 200));

        rect.RemoveFramePosition(5);

        Assert.False(rect.HasTrackedPosition(5));
        // Interpolation should still work between frames 0 and 10
        var result = rect.GetRectAtFrame(7);
        Assert.NotNull(result);
        Assert.Equal(70, result.Value.X);
    }

    [Fact]
    public void GetRectPosition_ReturnsCopyOfPositions()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(0, new Rect(0, 0, 100, 100));

        var positions = rect.GetRectPositions();

        // Should be a separate copy that can be modified independently
        Assert.Single(positions);
        positions[5] = new Rect(999, 999, 0, 0);
        Assert.DoesNotContain(5, rect.GetRectPositions().Keys);
    }

    [Fact]
    public void AddFramePosition_OverwritesExistingFrame()
    {
        var rect = new TrackerBox();
        rect.AddFramePosition(5, new Rect(10, 20, 30, 40));
        rect.AddFramePosition(5, new Rect(100, 200, 300, 400));

        var result = rect.GetRectAtFrame(5);

        Assert.NotNull(result);
        Assert.Equal(100, result.Value.X);
        Assert.Equal(200, result.Value.Y);
    }
}
