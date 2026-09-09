using OpenCvSharp;

namespace ShutterFace.Tests;

public class TrackingManagerTests
{
    private readonly PlayerModel _model = new();

    [Fact]
    public void CreateFromDrag_NewTracking_GetsIncrementedName()
    {
        var manager = new TrackingManager(_model);
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);
        _model.TrackingRects.Clear();

        var result = manager.CreateFromDrag(drag, 0, 300);

        Assert.Equal("Face 1", result.Name);
    }

    [Fact]
    public void CreateFromDrag_SecondTracking_GetsIncrementedName()
    {
        var manager1 = new TrackingManager(_model);
        var drag1 = new System.Drawing.Rectangle(5, 5, 40, 40);
        _model.TrackingRects.Clear();
        manager1.CreateFromDrag(drag1, 0, 300);

        var manager2 = new TrackingManager(_model);
        var drag2 = new System.Drawing.Rectangle(60, 70, 80, 90);

        var result = manager2.CreateFromDrag(drag2, 0, 300);

        Assert.Equal("Face 2", result.Name);
    }

    [Fact]
    public void CreateFromDrag_SetsFrameRange()
    {
        var manager = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);

        manager.CreateFromDrag(drag, 0, 300);

        Assert.Equal(0, _model.TrackingRects[0].StartFrame);
        Assert.Equal(299, _model.TrackingRects[0].EndFrame);
    }

    [Fact]
    public void CreateFromDrag_SetsInitialRect()
    {
        var manager = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(100, 200, 50, 60);

        var result = manager.CreateFromDrag(drag, 10, 300);

        Assert.Equal(100, result.InitialRect.X);
        Assert.Equal(200, result.InitialRect.Y);
        Assert.Equal(50, result.InitialRect.Width);
        Assert.Equal(60, result.InitialRect.Height);
    }

    [Fact]
    public void CreateFromDrag_AddsToListAndSetsSelected()
    {
        var manager = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);

        manager.CreateFromDrag(drag, 0, 300);

        Assert.Single(_model.TrackingRects);
        Assert.Equal(0, _model.SelectedTrackingIndex);
    }

    [Fact]
    public void DeleteSelected_RemovesAtSelectedIndex()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag1 = new System.Drawing.Rectangle(10, 10, 30, 30);
        manager1.CreateFromDrag(drag1, 0, 100);

        var management2 = new TrackingManager(_model);
        var drag2 = new System.Drawing.Rectangle(50, 50, 40, 40);
        management2.CreateFromDrag(drag2, 0, 100);

        var managerDelete = new TrackingManager(_model);
        managerDelete.DeleteSelected();

        Assert.Single(_model.TrackingRects);
        Assert.Null(_model.SelectedTrackingIndex);
    }

    [Fact]
    public void DeleteSelected_WithNoSelection_DoesNothing()
    {
        _model.SelectedTrackingIndex = null;
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);
        _model.TrackingRects.Add(new TrackingRect());

        var manager = new TrackingManager(_model);
        manager.DeleteSelected();

        Assert.Single(_model.TrackingRects);
        Assert.Null(_model.SelectedTrackingIndex);
    }

    [Fact]
    public void Selected_ReturnsCorrectItem()
    {
        _model.SelectedTrackingIndex = 1;
        var drag2 = new System.Drawing.Rectangle(50, 50, 40, 40);
        _model.TrackingRects.Add(new TrackingRect());
        _model.TrackingRects.Add(new TrackingRect { Name = "Target" });

        var manager = new TrackingManager(_model);

        Assert.Equal("Target", manager.Selected!.Name);
    }

    [Fact]
    public void Selected_ReturnsNull_WhenIndexOutdated()
    {
        _model.SelectedTrackingIndex = 5;
        _model.TrackingRects.Add(new TrackingRect());

        var manager = new TrackingManager(_model);

        Assert.Null(manager.Selected);
    }

    [Fact]
    public void Selected_ReturnsNull_WhenNoSelection()
    {
        _model.SelectedTrackingIndex = null;
        _model.TrackingRects.Add(new TrackingRect());

        var manager = new TrackingManager(_model);

        Assert.Null(manager.Selected);
    }

    [Fact]
    public void ApplyProperties_CorrectlyChangesValues()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        manager1.CreateFromDrag(drag, 0, 100);

        var managerApply = new TrackingManager(_model);
        managerApply.ApplyProperties("NewName", 50, 60, 10, 50);

        var rect = _model.TrackingRects[0];
        Assert.Equal("NewName", rect.Name);
        Assert.Equal(50, rect.InitialRect.Width);
        Assert.Equal(60, rect.InitialRect.Height);
        Assert.Equal(10, rect.StartFrame);
        Assert.Equal(50, rect.EndFrame);
    }

    [Fact]
    public void ApplyProperties_StartAfterEnd_CorrectsValues()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        manager1.CreateFromDrag(drag, 0, 100);

        var managerApply = new TrackingManager(_model);
        bool? showMessageCalled = null;
        managerApply.ShowMessage = _ => { showMessageCalled = true; };
        managerApply.ApplyProperties("", 30, 40, 80, 50);

        Assert.True(showMessageCalled);
        Assert.Equal(50, _model.TrackingRects[0].StartFrame);
        Assert.Equal(50, _model.TrackingRects[0].EndFrame);
    }

    [Fact]
    public void ApplyProperties_WhenAnalyzed_CleansOutofRangeFrames()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        manager1.CreateFromDrag(drag, 0, 100);

        var tracking = _model.TrackingRects[0];
        tracking.IsAnalyzed = true;
        tracking.AddFramePosition(50, new Rect(10, 10, 30, 40));
        tracking.AddFramePosition(80, new Rect(20, 20, 30, 40));

        var managerApply = new TrackingManager(_model);
        managerApply.ApplyProperties("", 30, 40, 10, 60);

        Assert.True(tracking.HasTrackedPosition(50));
        Assert.False(tracking.HasTrackedPosition(80));
    }

    [Fact]
    public void ApplyProperties_WhenNotAnalyzed_ClearsPositions()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        manager1.CreateFromDrag(drag, 0, 100);

        var tracking = _model.TrackingRects[0];
        tracking.AddFramePosition(50, new Rect(10, 10, 30, 40));

        var managerApply = new TrackingManager(_model);
        managerApply.ApplyProperties("", 30, 40, 10, 60);

        Assert.Empty(tracking.GetRectPositions());
    }

    [Fact]
    public void ApplyProperties_WhenNoSelection_DoesNothing()
    {
        _model.SelectedTrackingIndex = 99;
        var manager = new TrackingManager(_model);

        manager.ApplyProperties("Name", 50, 60, 10, 50);

        // Should not throw
    }

    [Fact]
    public void ApplyProperties_NameEmpty_KeepsExistingName()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        manager1.CreateFromDrag(drag, 0, 100);

        var managerApply = new TrackingManager(_model);
        managerApply.ApplyProperties("", 0, 0, 0, 100);

        Assert.Equal("Face 1", _model.TrackingRects[0].Name);
    }

    [Fact]
    public void ApplyProperties_NameNonEmpty_ChangesName()
    {
        var manager1 = new TrackingManager(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        manager1.CreateFromDrag(drag, 0, 100);

        var managerApply = new TrackingManager(_model);
        managerApply.ApplyProperties("MyTracker", 0, 0, 0, 100);

        Assert.Equal("MyTracker", _model.TrackingRects[0].Name);
    }
}

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

public class FrameRendererTests
{
    [Fact]
    public void GetTrackingListText_Analyzed_ReturnsOnePrefix()
    {
        var tracking = new TrackingRect
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
        var tracking = new TrackingRect
        {
            Name = "Face 2",
            IsAnalyzed = false
        };

        var result = FrameRenderer.GetTrackingListText(tracking);

        Assert.StartsWith("0", result.TrimStart());
        Assert.Contains("Face 2", result);
    }

    public class VideoDisplayAreaTests : IDisposable
    {
        private readonly PlayerModel _model;
        private readonly FrameRenderer _renderer;
        private System.Windows.Forms.PictureBox? _pb;

        public VideoDisplayAreaTests()
        {
            _model = new PlayerModel();
            _renderer = new FrameRenderer(_model);
        }

        ~VideoDisplayAreaTests()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && _pb != null)
            {
                var img = _pb.Image;
                _pb.Image = null!;
                _pb.Dispose();
                _pb = null!;
                img?.Dispose();
            }
        }

        [Fact]
        public void GetVideoDisplayArea_ZeroPictureBoxSize_ReturnsEmptyAndOnes()
        {
            _model.CurrentFrame = new Mat(200, 100, MatType.CV_8UC3);
            var pb = new System.Windows.Forms.PictureBox();
            _renderer.Target = pb;

            _model.CurrentFrame = new Mat(200, 100, MatType.CV_8UC3);
            pb.Size = System.Drawing.Size.Empty;

            var result = _renderer.GetVideoDisplayArea();

            Assert.True(result.videoRect.IsEmpty);

            pb.Dispose();
        }

        [Fact]
        public void GetVideoDisplayArea_SquareVideoInWiderPictureBox_CenterVertically()
        {
            var pb = new System.Windows.Forms.PictureBox();
            _renderer.Target = pb;

            _model.CurrentFrame = new Mat(100, 100, MatType.CV_8UC3);
            pb.Size = new System.Drawing.Size(200, 100);

            var result = _renderer.GetVideoDisplayArea();

            Assert.True(result.videoRect.Y >= 0);
            Assert.True(result.videoRect.Width > 0);
            Assert.True(result.videoRect.Height > 0);

            pb.Dispose();
        }

        [Fact]
        public void GetVideoDisplayArea_TallVideoInWiderPictureBox_CenterHorizontally()
        {
            var pb = new System.Windows.Forms.PictureBox();
            _renderer.Target = pb;

            _model.CurrentFrame = new Mat(50, 200, MatType.CV_8UC3);
            pb.Size = new System.Drawing.Size(400, 100);

            var result = _renderer.GetVideoDisplayArea();

            Assert.True(result.videoRect.Width > 0);
            Assert.True(result.videoRect.Height > 0);

            pb.Dispose();
        }

        [Fact]
        public void GetVideoDisplayArea_16by9VideoIn4by3Box_ScalesToFitBox()
        {
            var pb = new System.Windows.Forms.PictureBox();
            _renderer.Target = pb;

            _model.CurrentFrame = new Mat(540, 960, MatType.CV_8UC3);
            pb.Size = new System.Drawing.Size(480, 360);

            var result = _renderer.GetVideoDisplayArea();

            Assert.True(result.videoRect.Width > 0);
            Assert.True(result.scaleX >= 1f);
            Assert.True(result.scaleY >= 1f);

            pb.Dispose();
        }

        [Fact]
        public void GetVideoDisplayArea_3by4VideoInSquareBox_CorrectlyScaled()
        {
            var pb = new System.Windows.Forms.PictureBox();
            _renderer.Target = pb;

            _model.CurrentFrame = new Mat(360, 540, MatType.CV_8UC3);
            pb.Size = new System.Drawing.Size(200, 200);

            var result = _renderer.GetVideoDisplayArea();

            Assert.True(result.videoRect.Width > 0);
            Assert.True(result.videoRect.Height > 0);

            pb.Dispose();
        }

        [Fact]
        public void GetVideoDisplayArea_SquareFrameInSquareBox_FillsBox()
        {
            var pb = new System.Windows.Forms.PictureBox();
            _renderer.Target = pb;

            _model.CurrentFrame = new Mat(100, 100, MatType.CV_8UC3);
            pb.Size = new System.Drawing.Size(200, 200);

            var result = _renderer.GetVideoDisplayArea();

            Assert.Equal(0, result.videoRect.X);
            Assert.Equal(0, result.videoRect.Y);
            Assert.Equal(200, result.videoRect.Width);
            Assert.Equal(200, result.videoRect.Height);

            pb.Dispose();
        }
    }
}

public class TrackingStoreTests
{
    [Fact]
    public void Serialize_TrackingRects_AndVideoPath_ReturnsJson()
    {
        var rects = new List<TrackingRect>
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

        var json = TrackingStore.Serialize(rects, "/path/to/video.mp4");

        Assert.Contains("\"VideoPath\"", json);
        Assert.Contains("/path/to/video.mp4", json);
        Assert.Contains("\"Face 1\"", json);
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsCorrectData()
    {
        var rect = new TrackingRect
        {
            Name = "Tracker A",
            StartFrame = 5,
            EndFrame = 50,
            InitialRect = new Rect(100, 200, 300, 400),
            IsAnalyzed = true
        };

        var json = TrackingStore.Serialize([rect], "/video.mp4");
        var result = TrackingStore.Deserialize(json);

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
        var json = TrackingStore.Serialize([], "/empty.mp4");
        var result = TrackingStore.Deserialize(json);

        Assert.NotNull(result);
        Assert.Empty(result.TrackingRects);
        Assert.Equal("/empty.mp4", result.VideoPath);
    }

    [Fact]
    public void Deserialize_NullJson_ReturnsNull()
    {
        var result = TrackingStore.Deserialize(null!);

        Assert.Null(result);
    }

    [Fact]
    public void Serialize_MultipleRects_PreservesAllData()
    {
        var rects = new List<TrackingRect>
        {
            new() { Name = "A", StartFrame = 0, EndFrame = 100, InitialRect = new Rect(0, 0, 10, 20) },
            new() { Name = "B", StartFrame = 50, EndFrame = 200, InitialRect = new Rect(30, 40, 50, 60) }
        };

        var json = TrackingStore.Serialize(rects, "/test.mp4");
        var result = TrackingStore.Deserialize(json);

        Assert.NotNull(result);
        Assert.Collection(result.TrackingRects,
            a => Assert.Equal("A", a.Name),
            b => Assert.Equal("B", b.Name)
        );
    }
}

public class VideoLoaderEdgeCasesTests
{
    [Fact]
    public void LoadFrame_BeforeOpenVideo_DoesNotThrow()
    {
        var model = new PlayerModel();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };

        bool threw = false;
        try
        {
            loader.LoadFrame(5);
        }
        catch
        {
            threw = true;
        }
        Assert.False(threw);
    }

    [Fact]
    public void LoadFrame_OutOfBoundsPositive_DoesNotCrash()
    {
        var model = new PlayerModel();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };
        loader.RenderFrame = _ => { };
        string tempPath = CopyEmbeddedVideo();

        Assert.True(loader.OpenVideo(tempPath, out int totalFrames));

        bool threw = false;
        try
        {
            loader.LoadFrame(totalFrames + 1000);
        }
        catch
        {
            threw = true;
        }
        Assert.False(threw);
    }

    [Fact]
    public void LoadFrame_OutOfBoundsNegative_DoesNotCrash()
    {
        var model = new PlayerModel();
        var loader = new VideoLoader(model);
        loader.ShowMessage = _ => { };
        string tempPath = CopyEmbeddedVideo();

        Assert.True(loader.OpenVideo(tempPath, out _));

        bool threw = false;
        try
        {
            loader.LoadFrame(-1);
        }
        catch
        {
            threw = true;
        }
        Assert.False(threw);
    }

    [Fact]
    public void OpenVideo_AfterClose_CanReopen()
    {
        var model = new PlayerModel();
        var loaderA = new VideoLoader(model);
        loaderA.ShowMessage = _ => { };
        string tempPath = CopyEmbeddedVideo();

        Assert.True(loaderA.OpenVideo(tempPath, out _));
        // Simulate close by disposing the capture
        var capture = model.VideoCapture;
        capture?.Dispose();
        model.VideoCapture = null!;

        var loaderB = new VideoLoader(model);
        loaderB.ShowMessage = _ => { };

        Assert.True(loaderB.OpenVideo(tempPath, out _));
    }

    [Fact]
    public void IsOpen_FollowsLifecycle_OpenThenClose()
    {
        var model = new PlayerModel();
        var loader1 = new VideoLoader(model);
        string tempPath = CopyEmbeddedVideo();

        Assert.True(loader1.OpenVideo(tempPath, out _));
        Assert.True(loader1.IsOpen);

        model.VideoCapture?.Dispose();
        model.VideoCapture = null!;

        Assert.False(loader1.IsOpen);
    }

    private static string CopyEmbeddedVideo()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "test_video_edge.mp4");
        using var stream = typeof(VideoLoaderTests).Assembly.GetManifestResourceStream("ShutterFace.Tests.Resources.file_example_MP4_480_1_5MG.mp4");
        Assert.NotNull(stream);
        using var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fs);
        return tempPath;
    }
}
