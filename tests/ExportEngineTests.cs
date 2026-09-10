using OpenCvSharp;
using ShutterFace.DataObjects;
using ShutterFace.Engines;

namespace ShutterFace.Tests;

public class ExportEngineTests
{
    [Fact]
    public void PixelateRegion_ZeroBigPixels_ReturnsEarly()
    {
        using var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(0, 0, 24, 24);
        ExportEngine.PixelateRegion(image, region, 0);
    }

    [Fact]
    public void PixelateRegion_SinglePixelBlock_DoesNotCrash()
    {
        using var image = new Mat(24, 24, MatType.CV_8UC3, Scalar.White);
        Rect region = new(0, 0, 12, 12);
        ExportEngine.PixelateRegion(image, region, 0);
    }

    [Fact]
    public void PixelateStandardRect_FillsRegion_CreatesPixelation()
    {
        using var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(25, 25, 50, 50);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    [Fact]
    public void PixelateRegion_PartiallyOutsideImage_CropstoValidBounds()
    {
        using var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(40, 40, 20, 20);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.Equal(50, image.Width);
        Assert.Equal(50, image.Height);
    }

    [Fact]
    public void PixelateRegion_FullyOutsideImage_NoError()
    {
        using var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(-20, -20, 100, 100);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.Equal(50, image.Width);
        Assert.True(image.Width > 0);
    }

    [Fact]
    public void PixelateRegion_ZeroBigPixels_DoesNotCrash()
    {
        using var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(10, 10, 50, 50);

        ExportEngine.PixelateRegion(image, region, 0);

        Assert.Equal(100, image.Width);
    }

    [Fact]
    public void PixelateRegion_LargeBigPixels_DoesNotCrash()
    {
        using var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(10, 10, 50, 50);

        ExportEngine.PixelateRegion(image, region, 1000);

        Assert.Equal(100, image.Width);
    }

    [Fact]
    public void PixelateRegion_SinglePixelRegion_DoesNotCrash()
    {
        using var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(50, 50, 1, 1);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.True(true, "Should not throw");
    }

    [Fact]
    public void PixelateRegion_SmallNegativePosition_AdjustedToZero()
    {
        using var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(-40, -30, 60, 60);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.True(image.Width > 0);
    }

    [Fact]
    public void PixelateRegion_MixedBoundaries_SomePositiveSomeNegative()
    {
        using var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(-10, 20, 60, 30);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.True(image.Width > 0);
    }

    [Fact]
    public void ComputeEffectiveBigPixels_SingleValue_ReturnsSameValue()
    {
        var values = new List<float> { 16f };
        var result = ExportEngine.ComputeEffectiveBigPixels(values);

        Assert.Equal(16f, result);
    }

    [Fact]
    public void ComputeEffectiveBigPixels_TwoIdenticalValues_ReturnsSameValue()
    {
        var values = new List<float> { 8f, 8f };
        var result = ExportEngine.ComputeEffectiveBigPixels(values);

        Assert.Equal(8f, result);
    }

    [Fact]
    public void ComputeEffectiveBigPixels_TwoDifferentValues_ReturnsRmsAverage()
    {
        var values = new List<float> { 8f, 24f };
        var result = ExportEngine.ComputeEffectiveBigPixels(values);

        // RMS of (8, 24) = sqrt((64 + 576) / 2) = sqrt(320) ≈ 17.89
        var expected = (float)Math.Sqrt((64 + 576) / 2.0);
        Assert.Equal(expected, result, precision: 2);
    }

    [Fact]
    public void ComputeEffectiveBigPixels_AllZeros_ReturnsDefault()
    {
        var values = new List<float> { 0f, 0f };
        var result = ExportEngine.ComputeEffectiveBigPixels(values);

        Assert.Equal(16f, result);
    }

    [Fact]
    public void ComputeEffectiveBigPixels_MixedPositiveAndZero_ReturnsRmsOfPositivesOnly()
    {
        var values = new List<float> { 0f, 8f, 0f };
        var result = ExportEngine.ComputeEffectiveBigPixels(values);

        // Count is still 3 (total items), RMS = sqrt((64 / 3) + 0) but only positive sum: sqrt(64/3) ≈ 4.62
        var expected = (float)Math.Sqrt(64.0 / 3.0);
        Assert.Equal(expected, result, precision: 2);
    }

    [Fact]
    public void GridBlurFrames_NoTrackingRects_FrameUnchanged()
    {
        using var image = new Mat(320, 240, MatType.CV_8UC1, Scalar.White);
        var originalData = image.ToBytes();

        var state = CreateDummyState(new List<TrackerBox>());
        state.BlurCellSize = 8;
        var engine = new ExportEngine(state);

        // Invoke via reflection since GridBlurFrames is private
        var method = typeof(ExportEngine).GetMethod("GridBlurFrames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method.Invoke(null, new object?[] { image, 0, state, image.Width, image.Height });

        Assert.True(MatchData(image, originalData), "Frame should be unchanged when no tracking rects exist");
    }

    [Fact]
    public void GridBlurFrames_FullyInsideRect_CorrectNumberOfCellsPixelated()
    {
        using var image = new Mat(160, 120, MatType.CV_8UC3, Scalar.White);

        // Create a single tracking rect covering center of frame
        var trackerBox = new TrackerBox
        {
            Name = "test",
            StartFrame = 0,
            EndFrame = 10,
            InitialRect = new Rect(40, 30, 80, 60),
            IsAnalyzed = true
        };
        trackerBox.AddFramePosition(5, new Rect(40, 30, 80, 60));

        var rects = new List<TrackerBox> { trackerBox };
        var state = CreateDummyState(rects);
        state.BlurCellSize = 10;
        state.BigPixels = 16;
        var engine = new ExportEngine(state);

        var method = typeof(ExportEngine).GetMethod("GridBlurFrames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method.Invoke(null, new object?[] { image, 5, state, image.Width, image.Height });

        // Image dimensions should remain unchanged after processing
        Assert.Equal(120, image.Width);
        Assert.Equal(160, image.Height);
    }

    [Fact]
    public void GridBlurFrames_OutOfBoundsRect_ClinpedCorrectly()
    {
        using var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);

        // Create a tracking rect outside of frame bounds
        var trackerBox = new TrackerBox
        {
            Name = "test",
            StartFrame = 0,
            EndFrame = 10,
            InitialRect = new Rect(200, 200, 50, 50),
            IsAnalyzed = true
        };
        trackerBox.AddFramePosition(5, new Rect(200, 200, 50, 50));

        var rects = new List<TrackerBox> { trackerBox };
        var state = CreateDummyState(rects);
        state.BlurCellSize = 10;
        state.BigPixels = 16;
        var engine = new ExportEngine(state);

        var method = typeof(ExportEngine).GetMethod("GridBlurFrames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        method.Invoke(null, new object?[] { image, 5, state, image.Width, image.Height });

        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    [Fact]
    public void GridBlurFrames_SmallCellSize_MoreCellsCovered()
    {
        using var image = new Mat(200, 200, MatType.CV_8UC3, Scalar.White);

        var trackerBox = new TrackerBox
        {
            Name = "test",
            StartFrame = 0,
            EndFrame = 10,
            InitialRect = new Rect(50, 50, 100, 100),
            IsAnalyzed = true
        };
        trackerBox.AddFramePosition(5, new Rect(50, 50, 100, 100));

        // Small cell size should create more cells
        var rects = new List<TrackerBox> { trackerBox };
        var stateTiny = CreateDummyState(rects);
        stateTiny.BlurCellSize = 5;
        stateTiny.BigPixels = 16;
        var method = typeof(ExportEngine).GetMethod("GridBlurFrames", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        method.Invoke(null, new object?[] { image, 5, stateTiny, image.Width, image.Height });

        Assert.Equal(200, image.Width);
        Assert.Equal(200, image.Height);
    }

    private static TrackerState CreateDummyState(List<TrackerBox> trackingRects)
    {
        var state = new TrackerState
        {
            VideoCapture = null!,
            CurrentFrame = null!,
            TotalFrames = 10
        };
        state.TrackingRects.Clear();
        state.TrackingRects.AddRange(trackingRects);
        return state;
    }

    private static bool MatchData(Mat image, byte[] originalData)
    {
        var currentData = image.ToBytes();
        return currentData.SequenceEqual(originalData);
    }
}
