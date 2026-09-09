namespace ShutterFace.Tests;

/// <summary>Tests for InterfaceStateMachine state transitions.</summary>
public class InterfaceStateMachineTests
{
    [Fact]
    public void Constructor_StartsInIdle()
    {
        var sm = new InterfaceStateMachine();
        Assert.Equal(InterfaceMode.Idle, sm.Mode);
    }

    [Fact]
    public void FromIdle_CanGoToDragCreate()
    {
        var sm = new InterfaceStateMachine();
        sm.SetMode(InterfaceMode.DragCreate);
        Assert.Equal(InterfaceMode.DragCreate, sm.Mode);
    }

    [Fact]
    public void FromIdle_CanGoToAnalyzing()
    {
        var sm = new InterfaceStateMachine();
        sm.SetMode(InterfaceMode.Analyzing);
        Assert.Equal(InterfaceMode.Analyzing, sm.Mode);
    }

    [Fact]
    public void FromIdle_CanGoToExporting()
    {
        var sm = new InterfaceStateMachine();
        sm.SetMode(InterfaceMode.Exporting);
        Assert.Equal(InterfaceMode.Exporting, sm.Mode);
    }

    [Fact]
    public void FromIdle_CannotStayInIdle()
    {
        var sm = new InterfaceStateMachine();
        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.Idle));
    }

    [Fact]
    public void FromDragCreate_CannotGoToAnalyzing()
    {
        var sm = CreateIn(InterfaceMode.DragCreate);
        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.Analyzing));
    }

    [Fact]
    public void FromDragCreate_CannotGoToExporting()
    {
        var sm = CreateIn(InterfaceMode.DragCreate);
        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.Exporting));
    }

    [Fact]
    public void FromDragCreate_CanReturnToIdle()
    {
        var sm = CreateIn(InterfaceMode.DragCreate);
        sm.SetMode(InterfaceMode.Idle);
        Assert.Equal(InterfaceMode.Idle, sm.Mode);
    }

    [Fact]
    public void FromIdle_CanGoToResizeDraggingAnchor()
    {
        var sm = new InterfaceStateMachine();
        sm.SetMode(InterfaceMode.ResizeDraggingAnchor);
        Assert.Equal(InterfaceMode.ResizeDraggingAnchor, sm.Mode);
    }

    [Fact]
    public void FromResizeDraggingAnchor_CanOnlyGoIdle()
    {
        var sm = CreateIn(InterfaceMode.ResizeDraggingAnchor);

        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.Analyzing));
    }

    [Fact]
    public void FromAnalyzing_CanOnlyGoIdle()
    {
        var sm = CreateIn(InterfaceMode.Analyzing);

        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.DragCreate));
        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.Exporting));
    }

    [Fact]
    public void FromExporting_CanOnlyGoIdle()
    {
        var sm = CreateIn(InterfaceMode.Exporting);

        Assert.Throws<InvalidOperationException>(() => sm.SetMode(InterfaceMode.DragCreate));
    }

    [Fact]
    public void CancelFromIdle_IsIdempotent()
    {
        var sm = new InterfaceStateMachine();
        sm.CancelAndReturnToIdle();
        sm.CancelAndReturnToIdle();
        Assert.Equal(InterfaceMode.Idle, sm.Mode);
    }

    private static InterfaceStateMachine CreateIn(InterfaceMode mode)
    {
        var sm = new InterfaceStateMachine();
        sm.ForceMode(mode);
        return sm;
    }
}
