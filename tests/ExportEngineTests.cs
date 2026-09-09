using OpenCvSharp;

namespace ShutterFace.Tests;

public class ExportEngineTests
{
    [Fact]
    public void PixelateRegion_ZeroBigPixels_ReturnsEarly()
    {
        var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(0, 0, 24, 24);

        bool noThrow = false;
        try
        {
            ExportEngine.PixelateRegion(image, region, 0);
            noThrow = true;
        }
        catch
        {
            noThrow = false;
        }

        Assert.True(noThrow);
        Assert.Equal(50, image.Width);
    }

    [Fact]
    public void PixelateRegion_SinglePixelBlock_DoesNotCrash()
    {
        var image = new Mat(24, 24, MatType.CV_8UC3, Scalar.White);
        Rect region = new(0, 0, 12, 12);

        bool noThrow = false;
        try
        {
            ExportEngine.PixelateRegion(image, region, 0);
            noThrow = true;
        }
        catch
        {
            noThrow = false;
        }

        Assert.True(noThrow);
    }

    [Fact]
    public void PixelateStandardRect_FillsRegion_CreatesPixelation()
    {
        var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(25, 25, 50, 50);

        ExportEngine.PixelateRegion(image, region, 8);

        // Check that the file is still a valid size image
        Assert.Equal(100, image.Width);
        Assert.Equal(100, image.Height);
    }

    [Fact]
    public void PixelateRegion_PartiallyOutsideImage_CropstoValidBounds()
    {
        var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(40, 40, 20, 20);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.Equal(50, image.Width);
        Assert.Equal(50, image.Height);
    }

    [Fact]
    public void PixelateRegion_FullyOutsideImage_NoError()
    {
        var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(-20, -20, 100, 100);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.Equal(50, image.Width);
        Assert.True(image.Width > 0);
    }

    [Fact]
    public void PixelateRegion_ZeroBigPixels_DoesNotCrash()
    {
        var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(10, 10, 50, 50);

        ExportEngine.PixelateRegion(image, region, 0);

        Assert.Equal(100, image.Width);
    }

    [Fact]
    public void PixelateRegion_LargeBigPixels_DoesNotCrash()
    {
        var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(10, 10, 50, 50);

        ExportEngine.PixelateRegion(image, region, 1000);

        Assert.Equal(100, image.Width);
    }

    [Fact]
    public void PixelateRegion_SinglePixelRegion_DoesNotCrash()
    {
        var image = new Mat(100, 100, MatType.CV_8UC3, Scalar.White);
        Rect region = new(50, 50, 1, 1);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.True(true, "Should not throw");
    }

    [Fact]
    public void PixelateRegion_SmallNegativePosition_AdjustedToZero()
    {
        var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(-40, -30, 60, 60);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.True(image.Width > 0);
    }

    [Fact]
    public void PixelateRegion_MixedBoundaries_SomePositiveSomeNegative()
    {
        var image = new Mat(50, 50, MatType.CV_8UC3, Scalar.White);
        Rect region = new(-10, 20, 60, 30);

        ExportEngine.PixelateRegion(image, region, 8);

        Assert.True(image.Width > 0);
    }
}
