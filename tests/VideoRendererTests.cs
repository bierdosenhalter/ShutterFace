using OpenCvSharp;
using System.Windows.Forms;

namespace ShutterFace.Tests;

public class VideoRendererTests
{
    [Fact]
    public void UpdateListViewImageKey_Analyzed_ReturnsAnalyzedImageKey()
    {
        var tracking = new TrackerBox
        {
            Name = "Face 1",
            IsAnalyzed = true
        };

        using var listView = new ListView();
        listView.LargeImageList = new ImageList();
        var item = new ListViewItem(tracking.Name);
        listView.Items.Add(item);

        VideoRenderer.UpdateListViewStatus(listView, new[] { tracking });

        Assert.Equal("analyzed", listView.Items[0].ImageKey);
    }

    [Fact]
    public void UpdateListViewImageKey_NotAnalyzed_ReturnsNotAnalyzedImageKey()
    {
        var tracking = new TrackerBox
        {
            Name = "Face 2",
            IsAnalyzed = false
        };

        using var listView = new ListView();
        listView.LargeImageList = new ImageList();
        var item = new ListViewItem(tracking.Name);
        listView.Items.Add(item);

        VideoRenderer.UpdateListViewStatus(listView, new[] { tracking });

        Assert.Equal("not_analyzed", listView.Items[0].ImageKey);
    }

    [Fact]
    public void UpdateListViewImageKey_EmptyTrackingBoxes_DoesNotModifyItems()
    {
        using var listView = new ListView();
        listView.LargeImageList = new ImageList();
        listView.Items.Add("Existing");
        listView.Items.Add("Another");

        VideoRenderer.UpdateListViewStatus(listView, Array.Empty<TrackerBox>());

        Assert.Equal("Existing", listView.Items[0].Text);
        Assert.Equal("Another", listView.Items[1].Text);
    }
}
