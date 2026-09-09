using OpenCvSharp;

namespace ShutterFace.Tests;

public class VideoRendererTests
{
    [Fact]
    public void UpdateListBoxText_Analyzed_ReturnsOnePrefix()
    {
        var tracking = new TrackerBox
        {
            Name = "Face 1",
            IsAnalyzed = true
        };

        using var listBox = new System.Windows.Forms.ListBox();
        listBox.Items.Add(tracking.Name);

        VideoRenderer.UpdateListBoxColors(listBox, new[] { tracking });

        Assert.StartsWith("1 ", listBox.Items[0] as string);
    }

    [Fact]
    public void UpdateListBoxText_NotAnalyzed_ReturnsZeroPrefix()
    {
        var tracking = new TrackerBox
        {
            Name = "Face 2",
            IsAnalyzed = false
        };

        using var listBox = new System.Windows.Forms.ListBox();
        listBox.Items.Add(tracking.Name);

        VideoRenderer.UpdateListBoxColors(listBox, new[] { tracking });

        Assert.StartsWith("0 ", listBox.Items[0] as string);
    }

    [Fact]
    public void UpdateListBoxText_EmptyTrackingBoxes_DoesNotModifyListBox()
    {
        using var listBox = new System.Windows.Forms.ListBox();
        listBox.Items.Add("Existing");
        listBox.Items.Add("Another");

        VideoRenderer.UpdateListBoxColors(listBox, Array.Empty<TrackerBox>());

        Assert.Equal("Existing", listBox.Items[0]);
        Assert.Equal("Another", listBox.Items[1]);
    }
}
