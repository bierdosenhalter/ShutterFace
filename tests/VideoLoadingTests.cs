using OpenCvSharp;
using System.Reflection;

namespace MotionTrackerFaceBlur.Tests;

public class VideoLoadingTests
{
    [Fact]
    public void LoadVideoFromPath_VideoCaptureIsOpen_OpenedSuccessfully()
    {
        string embeddedResource = GetEmbeddedVideoPath();

        using var capture = new VideoCapture(embeddedResource);

        Assert.True(capture.IsOpened());
    }

    [Fact]
    public void LoadVideoFromPath_ReadFirstFrame_ReturnsNonEmptyMat()
    {
        string embeddedResource = GetEmbeddedVideoPath();

        using var capture = new VideoCapture(embeddedResource);
        Assert.True(capture.IsOpened());

        using var frame = new Mat();
        bool readSuccess = capture.Read(frame);

        Assert.True(readSuccess);
        Assert.False(frame.Empty());
    }

    [Fact]
    public void LoadVideoFromPath_FrameCount_IsGreaterThanZero()
    {
        string embeddedResource = GetEmbeddedVideoPath();

        using var capture = new VideoCapture(embeddedResource);
        Assert.True(capture.IsOpened());

        long frameCount = capture.FrameCount;

        Assert.True(frameCount > 0, "Video must have at least one frame");
    }

    [Fact]
    public void LoadVideoFromPath_Fps_IsGreaterThenZero()
    {
        string embeddedResource = GetEmbeddedVideoPath();

        using var capture = new VideoCapture(embeddedResource);
        Assert.True(capture.IsOpened());

        double fps = capture.Fps;

        Assert.True(fps > 0, "Video FPS must be greater than zero");
    }

    private static string GetEmbeddedVideoPath()
    {
        // Write embedded resource to a temp file so VideoCapture can open it
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("MotionTrackerFaceBlur.Tests.Resources.file_example_MP4_480_1_5MG.mp4");

        Assert.NotNull(stream);

        string tempPath = Path.Combine(Path.GetTempPath(), "test_video.mp4");

        using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);

        return tempPath;
    }
}
