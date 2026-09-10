using OpenCvSharp;
using ShutterFace.DataObjects;
using ShutterFace.Resources;

namespace ShutterFace;

internal partial class SettingsForm : Form
{
    private readonly AppConfigData _originalSettings;
    private readonly TrackerState? _trackerState;
    private int _displayValue;

    public SettingsForm(AppConfigData settings, TrackerState? trackerState = null)
    {
        _originalSettings = settings;
        _trackerState = trackerState;
        InitializeComponent();
        LoadDefaults();
    }

    public int GridCellSizePixels => _displayValue;
    public float ConfidenceThreshold => (float)nudConfidenceThreshold.Value;

    private void LoadDefaults()
    {
        nudGridCellSize.Value = _originalSettings.GridCellSizePixels;
        nudConfidenceThreshold.Value = (decimal)_originalSettings.ConfidenceThreshold;
    }

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        nudGridCellSize.Minimum = 1;
        nudGridCellSize.Maximum = 2000;
        nudGridCellSize.Increment = 1m;

        nudConfidenceThreshold.Minimum = 0.1m;
        nudConfidenceThreshold.Maximum = 1m;
        nudConfidenceThreshold.Increment = 0.05m;

        nudGridCellSize.ValueChanged += NudGridCellSize_ValueChanged;

        Text = ControlResourceManager.GetString("Form_Settings");
        gbBlur.Text = ControlResourceManager.GetString("GroupBlur");
        gbConfidence.Text = ControlResourceManager.GetString("GroupTracking");
        lblGridCellSize.Text = ControlResourceManager.GetString("LabelGridCellSize");
        lblConfidenceThreshold.Text = ControlResourceManager.GetString("LabelConfidenceThreshold");

        nudGridCellSize.ValueChanged += NudGridCellSize_ValueChanged;

        UpdateGridCellDisplay();
    }

    private void NudGridCellSize_ValueChanged(object? sender, EventArgs e)
    {
        _displayValue = (int)nudGridCellSize.Value;
    }

    private void UpdateGridCellDisplay()
    {
        if (_trackerState == null)
            return;

        int width = _trackerState.VideoCapture?.FrameWidth ?? 1920;
        int height = _trackerState.VideoCapture?.FrameHeight ?? 1080;
        if (width < 1 || height < 1)
            return;

        int cellSize = _trackerState.GetGridCellSize(Math.Max(1, width), Math.Max(1, height));
        lblGridCellSize.Text = ControlResourceManager.GetString("LabelGridCellSize") + ": " + cellSize + "px";
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        _displayValue = (int)nudGridCellSize.Value;
        float confidence = (float)nudConfidenceThreshold.Value;

        var newSettings = new AppConfigData(
            BigPixels: _originalSettings.BigPixels,
            GridCellSizePixels: _displayValue,
            ConfidenceThreshold: confidence);

        AppConfig.Save(newSettings);

        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }
}
