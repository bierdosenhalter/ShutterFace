using OpenCvSharp;

namespace ShutterFace.Tests;

public class VideoLoaderTests
{
    [Fact]
    public void IsVideoFile_ValidMp4_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.mp4"));
    }

    [Fact]
    public void IsVideoFile_ValidAvi_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.avi"));
    }

    [Fact]
    public void IsVideoFile_ValidMov_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.mov"));
    }

    [Fact]
    public void IsVideoFile_ValidWmv_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.wmv"));
    }

    [Fact]
    public void IsVideoFile_ValidMkv_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.mkv"));
    }

    [Fact]
    public void IsVideoFile_ValidFlv_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.flv"));
    }

    [Fact]
    public void IsVideoFile_ValidWebm_ReturnsTrue()
    {
        Assert.True(VideoLoader.IsVideoFile("video.webm"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("image.png")]
    [InlineData("document.pdf")]
    [InlineData("audio.mp3")]
    [InlineData("video.unknown")]
    public void IsVideoFile_InvalidOrEmptyPath_ReturnsFalse(string? path)
    {
        Assert.False(VideoLoader.IsVideoFile(path!));
    }

    [Fact]
    public void OpenVideo_ValidVideo_ReturnsTrueAndSetsTotalFrames()
    {
        var model = new TrackerState();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };
        string tempPath = CopyEmbeddedVideo();

        bool opened = loader.OpenVideo(tempPath, out int totalFrames);

        Assert.True(opened);
        Assert.True(totalFrames > 0);
        Assert.Equal(tempPath, model.VideoPath);
    }

    [Fact]
    public void OpenVideo_NonExistentFile_ReturnsFalseAndDisposesNull()
    {
        var model = new TrackerState();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };

        bool opened = loader.OpenVideo("nonexistent_video.mp4", out int totalFrames);

        Assert.False(opened);
        Assert.Equal(0, totalFrames);
    }

    [Fact]
    public void IsOpen_ReturnsFalseBeforeOpening()
    {
        var model = new TrackerState();
        var loader = new VideoLoader(model);

        Assert.False(loader.IsOpen);
    }

    [Fact]
    public void IsOpen_ReturnsTrueAfterSuccessfulOpen()
    {
        var model = new TrackerState();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };
        string tempPath = CopyEmbeddedVideo();

        loader.OpenVideo(tempPath, out _);

        Assert.True(loader.IsOpen);
    }

    [Fact]
    public void LoadFrame_SeeksCorrectFrame_ReturnsNonNullFrame()
    {
        var model = new TrackerState();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };
        loader.RunOnUi = a => a.Invoke();
        string tempPath = CopyEmbeddedVideo();

        bool result = loader.OpenVideo(tempPath, out int totalFrames);
        bool frameRendered = false;
        loader.RenderFrame = f =>
        {
            frameRendered = true;
            Assert.NotNull(f);
        };

        Assert.True(result);
        loader.LoadFrame(0);
        Assert.True(frameRendered);
    }

    private static string CopyEmbeddedVideo()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "test_video.mp4");
        using var stream = typeof(VideoLoaderTests).Assembly.GetManifestResourceStream("ShutterFace.Tests.Resources.file_example_MP4_480_1_5MG.mp4");
        Assert.NotNull(stream);
        using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);
        return tempPath;
    }
}

public class TrackerStaticMethodTests
{
    [Theory]
    [InlineData(0, 0, 100, 100, 50, 50, 50, 150)] // newRect overlaps previousRect corners (bottom right)
    [InlineData(50, 50, 150, 150, 0, 0, 100, 100)] // newRect contains previousRect entirely
    [InlineData(0, 0, 100, 100, 90, 90, 200, 200)] // newRect contains a corner of previousRect
    public void IsEdgeInBounds_OverlappingRects_ReturnsTrue(int newX, int newY, int newW, int newH,
        int prevX, int prevY, int prevW, int prevH)
    {
        var newRect = new Rect(newX, newY, newW, newH);
        var previousRect = new Rect(prevX, prevY, prevW, prevH);

        Assert.True(Tracker.IsEdgeInBounds(newRect, previousRect));
    }

    [Theory]
    [InlineData(95, 100, 10, 10, 0, 0, 105, 110)] // newRect corner (105,100) overlaps previousRect edge at X=105
    [InlineData(48, 98, 6, 4, 0, 0, 100, 100)]      // small overlapping rect near corner of previousRect
    public void IsEdgeInBounds_SlightlyOverlapping_ReturnsTrue(int newX, int newY, int newW, int newH,
        int prevX, int prevY, int prevW, int prevH)
    {
        var newRect = new Rect(newX, newY, newW, newH);
        var previousRect = new Rect(prevX, prevY, prevW, prevH);

        Assert.True(Tracker.IsEdgeInBounds(newRect, previousRect));
    }

    [Theory]
    [InlineData(50, 100, 50, 50, 0, 0, 100, 100)] // newRect starts at previousRect bottom edge (touching at Y=100)
    [InlineData(0, 56, 100, 43, 0, 0, 100, 100)]   // overlapping vertically enough for corner test
    public void IsEdgeInBounds_AdjacentEdges_ReturnsTrue(int newX, int newY, int newW, int newH,
        int prevX, int prevY, int prevW, int prevH)
    {
        var newRect = new Rect(newX, newY, newW, newH);
        var previousRect = new Rect(prevX, prevY, prevW, prevH);

        Assert.True(Tracker.IsEdgeInBounds(newRect, previousRect));
    }

    [Theory]
    [InlineData(200, 200, 50, 50, 0, 0, 100, 100)] // completely far away
    [InlineData(300, 400, 10, 10, 0, 0, 100, 100)] // very small rect far away from previousRect
    public void IsEdgeInBounds_DistanceTooFar_ReturnsFalse(int newX, int newY, int newW, int newH,
        int prevX, int prevY, int prevW, int prevH)
    {
        var newRect = new Rect(newX, newY, newW, newH);
        var previousRect = new Rect(prevX, prevY, prevW, prevH);

        Assert.False(Tracker.IsEdgeInBounds(newRect, previousRect));
    }
}
