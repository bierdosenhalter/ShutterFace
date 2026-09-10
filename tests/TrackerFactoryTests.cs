using OpenCvSharp;
using ShutterFace.DataObjects;
using ShutterFace.Tracking;

namespace ShutterFace.Tests;

public class TrackerFactoryTests
{
    private readonly TrackerState _model = new();

    [Fact]
    public void CreateFromDrag_NewTracking_GetsIncrementedName()
    {
        var factory = new TrackerFactory(_model);
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);
        _model.TrackingRects.Clear();

        var result = factory.CreateFromDrag(drag, 0, 300);

        Assert.Equal("Face 1", result.Name);
    }

    [Fact]
    public void CreateFromDrag_SecondTracking_GetsIncrementedName()
    {
        var factory1 = new TrackerFactory(_model);
        var drag1 = new System.Drawing.Rectangle(5, 5, 40, 40);
        _model.TrackingRects.Clear();
        factory1.CreateFromDrag(drag1, 0, 300);

        var factory2 = new TrackerFactory(_model);
        var drag2 = new System.Drawing.Rectangle(60, 70, 80, 90);

        var result = factory2.CreateFromDrag(drag2, 0, 300);

        Assert.Equal("Face 2", result.Name);
    }

    [Fact]
    public void CreateFromDrag_SetsFrameRange()
    {
        var factory = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);

        factory.CreateFromDrag(drag, 0, 300);

        Assert.Equal(0, _model.TrackingRects[0].StartFrame);
        Assert.Equal(299, _model.TrackingRects[0].EndFrame);
    }

    [Fact]
    public void CreateFromDrag_SetsInitialRect()
    {
        var factory = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(100, 200, 50, 60);

        var result = factory.CreateFromDrag(drag, 10, 300);

        Assert.Equal(100, result.InitialRect.X);
        Assert.Equal(200, result.InitialRect.Y);
        Assert.Equal(50, result.InitialRect.Width);
        Assert.Equal(60, result.InitialRect.Height);
    }

    [Fact]
    public void CreateFromDrag_AddsToListAndSetsSelected()
    {
        var factory = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);

        factory.CreateFromDrag(drag, 0, 300);

        Assert.Single(_model.TrackingRects);
        Assert.Equal(0, _model.SelectedTrackingIndex);
    }

    [Fact]
    public void DeleteSelected_RemovesAtSelectedIndex()
    {
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag1 = new System.Drawing.Rectangle(10, 10, 30, 30);
        factory1.CreateFromDrag(drag1, 0, 100);

        var factory2 = new TrackerFactory(_model);
        var drag2 = new System.Drawing.Rectangle(50, 50, 40, 40);
        factory2.CreateFromDrag(drag2, 0, 100);

        var deleteTracker = new TrackerFactory(_model);
        deleteTracker.DeleteSelected();

        Assert.Single(_model.TrackingRects);
        Assert.Null(_model.SelectedTrackingIndex);
    }

    [Fact]
    public void DeleteSelected_WithNoSelection_DoesNothing()
    {
        _model.SelectedTrackingIndex = null;
        var drag = new System.Drawing.Rectangle(10, 20, 50, 60);
        _model.TrackingRects.Add(new TrackerBox());

        var factory = new TrackerFactory(_model);
        factory.DeleteSelected();

        Assert.Single(_model.TrackingRects);
        Assert.Null(_model.SelectedTrackingIndex);
    }

    [Fact]
    public void Selected_ReturnsCorrectItem()
    {
        _model.SelectedTrackingIndex = 1;
        var drag2 = new System.Drawing.Rectangle(50, 50, 40, 40);
        _model.TrackingRects.Add(new TrackerBox());
        _model.TrackingRects.Add(new TrackerBox { Name = "Target" });

        var factory = new TrackerFactory(_model);

        Assert.Equal("Target", factory.Selected!.Name);
    }

    [Fact]
    public void Selected_ReturnsNull_WhenIndexOutdated()
    {
        _model.SelectedTrackingIndex = 5;
        _model.TrackingRects.Add(new TrackerBox());

        var factory = new TrackerFactory(_model);

        Assert.Null(factory.Selected);
    }

    [Fact]
    public void Selected_ReturnsNull_WhenNoSelection()
    {
        _model.SelectedTrackingIndex = null;
        _model.TrackingRects.Add(new TrackerBox());

        var factory = new TrackerFactory(_model);

        Assert.Null(factory.Selected);
    }

    [Fact]
    public void ApplyProperties_CorrectlyChangesValues()
    {
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        factory1.CreateFromDrag(drag, 0, 100);

        var applyFactory = new TrackerFactory(_model);
        applyFactory.ApplyProperties("NewName", 50, 60, 10, 50);

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
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        factory1.CreateFromDrag(drag, 0, 100);

        var applyFactory = new TrackerFactory(_model);
        bool? showMessageCalled = null;
        applyFactory.ShowMessage = _ => { showMessageCalled = true; };
        applyFactory.ApplyProperties("", 30, 40, 80, 50);

        Assert.True(showMessageCalled);
        Assert.Equal(50, _model.TrackingRects[0].StartFrame);
        Assert.Equal(50, _model.TrackingRects[0].EndFrame);
    }

    [Fact]
    public void ApplyProperties_WhenAnalyzed_CleansOutofRangeFrames()
    {
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        factory1.CreateFromDrag(drag, 0, 100);

        var trackerBox = _model.TrackingRects[0];
        trackerBox.IsAnalyzed = true;
        trackerBox.AddFramePosition(50, new Rect(10, 10, 30, 40));
        trackerBox.AddFramePosition(80, new Rect(20, 20, 30, 40));

        var applyFactory = new TrackerFactory(_model);
        applyFactory.ApplyProperties("", 30, 40, 10, 60);

        Assert.True(trackerBox.HasTrackedPosition(50));
        Assert.False(trackerBox.HasTrackedPosition(80));
    }

    [Fact]
    public void ApplyProperties_WhenNotAnalyzed_ClearsPositions()
    {
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        factory1.CreateFromDrag(drag, 0, 100);

        var trackerBox = _model.TrackingRects[0];
        trackerBox.AddFramePosition(50, new Rect(10, 10, 30, 40));

        var applyFactory = new TrackerFactory(_model);
        applyFactory.ApplyProperties("", 30, 40, 10, 60);

        Assert.Empty(trackerBox.GetRectPositions());
    }

    [Fact]
    public void ApplyProperties_WhenNoSelection_DoesNothing()
    {
        _model.SelectedTrackingIndex = 99;
        var factory = new TrackerFactory(_model);

        factory.ApplyProperties("Name", 50, 60, 10, 50);

        // Should not throw
    }

    [Fact]
    public void ApplyProperties_NameEmpty_KeepsExistingName()
    {
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        factory1.CreateFromDrag(drag, 0, 100);

        var applyFactory = new TrackerFactory(_model);
        applyFactory.ApplyProperties("", 0, 0, 0, 100);

        Assert.Equal("Face 1", _model.TrackingRects[0].Name);
    }

    [Fact]
    public void ApplyProperties_NameNonEmpty_ChangesName()
    {
        var factory1 = new TrackerFactory(_model);
        _model.TrackingRects.Clear();
        var drag = new System.Drawing.Rectangle(0, 0, 30, 40);
        factory1.CreateFromDrag(drag, 0, 100);

        var applyFactory = new TrackerFactory(_model);
        applyFactory.ApplyProperties("MyTracker", 0, 0, 0, 100);

        Assert.Equal("MyTracker", _model.TrackingRects[0].Name);
    }
}
