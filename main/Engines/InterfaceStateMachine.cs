namespace ShutterFace;

/// <summary>Manages InterfaceMode transitions with validation.</summary>
internal sealed class InterfaceStateMachine
{
    private InterfaceMode _mode;

    public InterfaceMode Mode => _mode;

    /// <summary>Sets mode directly (test-only, bypasses validation).</summary>
    internal void ForceMode(InterfaceMode mode) => _mode = mode;

    public InterfaceStateMachine() => _mode = InterfaceMode.Idle;

    /// <summary>Attempt transition; throws on illegal transition.</summary>
    public void SetMode(InterfaceMode mode)
    {
        if (!IsValidTransition(_mode, mode))
        {
            throw new InvalidOperationException($"Cannot transition from {_mode} to {mode}.");
        }

        _mode = mode;
    }

    /// <summary>Always return to idle (stop/cancel path).</summary>
    public void CancelAndReturnToIdle() => _mode = InterfaceMode.Idle;

    private static bool IsValidTransition(InterfaceMode from, InterfaceMode to)
    {
        if (from == InterfaceMode.Idle)
        {
            return to is InterfaceMode.DragCreate or InterfaceMode.Analyzing or InterfaceMode.Exporting or InterfaceMode.ResizeDraggingAnchor;
        }

        // All active states can only cancel back to Idle
        return to == InterfaceMode.Idle;
    }
}
