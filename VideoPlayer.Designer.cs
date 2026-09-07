using System.Diagnostics;

namespace MotionTrackerFaceBlur
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    partial class VideoPlayer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            VideoBox = new PictureBox();
            FrameSlider = new TrackBar();
            OpenVideoBtn = new Button();
            ExportVideoBtn = new Button();
            AddTrackingBtn = new Button();
            AnalyzeBtn = new Button();
            StopAnalyzeBtn = new Button();
            DeleteTrackingBtn = new Button();
            TrackingListBox = new ListBox();
            mainTable = new TableLayoutPanel();
            buttonLayoutPanel = new FlowLayoutPanel();
            LoadTrackingBtn = new Button();
            gprTracking = new GroupBox();
            SaveTrackingBtn = new Button();
            lblStartFrame = new Label();
            txtBoxHeight = new NumericUpDown();
            txtBoxWidth = new NumericUpDown();
            labelHeight = new Label();
            labelWidth = new Label();
            txtEndFrame = new NumericUpDown();
            txtStartFrame = new NumericUpDown();
            txtTrackingName = new TextBox();
            lblEndFrame = new Label();
            lblTrackingName = new Label();
            btnApplyChanges = new Button();
            timePanel = new Panel();
            lblStartTime = new Label();
            lblCurrentTime = new Label();
            lblEndTime = new Label();
            pnlFrameSlider = new Panel();
            trackRangeIndicator = new Panel();
            statusStripBottom = new StatusStrip();
            tssStatusLabel = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)VideoBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FrameSlider).BeginInit();
            mainTable.SuspendLayout();
            buttonLayoutPanel.SuspendLayout();
            gprTracking.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtBoxHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtBoxWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtEndFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtStartFrame).BeginInit();
            timePanel.SuspendLayout();
            pnlFrameSlider.SuspendLayout();
            statusStripBottom.SuspendLayout();
            SuspendLayout();
            // 
            // VideoBox
            // 
            VideoBox.BackColor = SystemColors.ControlDark;
            VideoBox.BorderStyle = BorderStyle.FixedSingle;
            VideoBox.Dock = DockStyle.Fill;
            VideoBox.Location = new Point(3, 3);
            VideoBox.Name = "VideoBox";
            VideoBox.Size = new Size(1194, 1102);
            VideoBox.SizeMode = PictureBoxSizeMode.Zoom;
            VideoBox.TabIndex = 0;
            VideoBox.TabStop = false;
            VideoBox.MouseDown += PictureBox_MouseDown;
            VideoBox.MouseMove += PictureBox_MouseMove;
            VideoBox.MouseUp += PictureBox_MouseUp;
            // 
            // FrameSlider
            // 
            FrameSlider.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FrameSlider.Location = new Point(3, 3);
            FrameSlider.Maximum = 100;
            FrameSlider.Name = "FrameSlider";
            FrameSlider.Size = new Size(1188, 44);
            FrameSlider.TabIndex = 1;
            FrameSlider.TickFrequency = 10;
            FrameSlider.Scroll += FrameSlider_Scroll;
            FrameSlider.SizeChanged += FrameSlider_SizeChanged;
            // 
            // OpenVideoBtn
            // 
            OpenVideoBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            OpenVideoBtn.Location = new Point(3, 53);
            OpenVideoBtn.Name = "OpenVideoBtn";
            OpenVideoBtn.Size = new Size(273, 44);
            OpenVideoBtn.TabIndex = 2;
            OpenVideoBtn.Text = "Open Video";
            OpenVideoBtn.Click += OpenVideoBtn_Click;
            // 
            // ExportVideoBtn
            // 
            ExportVideoBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ExportVideoBtn.Enabled = false;
            ExportVideoBtn.Location = new Point(3, 153);
            ExportVideoBtn.Name = "ExportVideoBtn";
            ExportVideoBtn.Size = new Size(273, 44);
            ExportVideoBtn.TabIndex = 3;
            ExportVideoBtn.Text = "Export Video";
            ExportVideoBtn.Click += ExportVideoBtn_Click;
            // 
            // AddTrackingBtn
            // 
            AddTrackingBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            AddTrackingBtn.Enabled = false;
            AddTrackingBtn.Location = new Point(3, 103);
            AddTrackingBtn.Name = "AddTrackingBtn";
            AddTrackingBtn.Size = new Size(273, 44);
            AddTrackingBtn.TabIndex = 4;
            AddTrackingBtn.Text = "Add Tracking Point";
            AddTrackingBtn.Click += AddTrackingBtn_Click;
            // 
            // AnalyzeBtn
            // 
            AnalyzeBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            AnalyzeBtn.Location = new Point(3, 203);
            AnalyzeBtn.Name = "AnalyzeBtn";
            AnalyzeBtn.Size = new Size(273, 44);
            AnalyzeBtn.TabIndex = 5;
            AnalyzeBtn.Text = "Analyze";
            AnalyzeBtn.Click += AnalyzeBtn_Click;
            // 
            // StopAnalyzeBtn
            // 
            StopAnalyzeBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            StopAnalyzeBtn.Location = new Point(3, 253);
            StopAnalyzeBtn.Name = "StopAnalyzeBtn";
            StopAnalyzeBtn.Size = new Size(273, 44);
            StopAnalyzeBtn.TabIndex = 6;
            StopAnalyzeBtn.Text = "Stop Analyzing";
            StopAnalyzeBtn.Click += StopAnalyzeBtn_Click;
            // 
            // DeleteTrackingBtn
            // 
            DeleteTrackingBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            DeleteTrackingBtn.Enabled = false;
            DeleteTrackingBtn.Location = new Point(3, 303);
            DeleteTrackingBtn.Name = "DeleteTrackingBtn";
            DeleteTrackingBtn.Size = new Size(273, 44);
            DeleteTrackingBtn.TabIndex = 7;
            DeleteTrackingBtn.Text = "Delete Tracking";
            DeleteTrackingBtn.Click += DeleteTrackingBtn_Click;
            // 
            // TrackingListBox
            // 
            TrackingListBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            TrackingListBox.Location = new Point(3, 353);
            TrackingListBox.Name = "TrackingListBox";
            TrackingListBox.Size = new Size(273, 260);
            TrackingListBox.TabIndex = 8;
            TrackingListBox.SelectedIndexChanged += TrackingListBox_SelectedIndexChanged;
            // 
            // mainTable
            // 
            mainTable.ColumnCount = 2;
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            mainTable.Controls.Add(VideoBox, 0, 0);
            mainTable.Controls.Add(buttonLayoutPanel, 1, 0);
            mainTable.Controls.Add(timePanel, 0, 2);
            mainTable.Controls.Add(pnlFrameSlider, 0, 1);
            mainTable.Dock = DockStyle.Fill;
            mainTable.Location = new Point(0, 0);
            mainTable.Name = "mainTable";
            mainTable.RowCount = 3;
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainTable.Size = new Size(1480, 1208);
            mainTable.TabIndex = 9;
            // 
            // buttonLayoutPanel
            // 
            buttonLayoutPanel.Controls.Add(LoadTrackingBtn);
            buttonLayoutPanel.Controls.Add(OpenVideoBtn);
            buttonLayoutPanel.Controls.Add(AddTrackingBtn);
            buttonLayoutPanel.Controls.Add(ExportVideoBtn);
            buttonLayoutPanel.Controls.Add(AnalyzeBtn);
            buttonLayoutPanel.Controls.Add(StopAnalyzeBtn);
            buttonLayoutPanel.Controls.Add(DeleteTrackingBtn);
            buttonLayoutPanel.Controls.Add(TrackingListBox);
            buttonLayoutPanel.Controls.Add(gprTracking);
            buttonLayoutPanel.Dock = DockStyle.Fill;
            buttonLayoutPanel.Location = new Point(1203, 3);
            buttonLayoutPanel.Name = "buttonLayoutPanel";
            buttonLayoutPanel.Size = new Size(274, 1102);
            buttonLayoutPanel.TabIndex = 10;
            // 
            // LoadTrackingBtn
            // 
            LoadTrackingBtn.Location = new Point(3, 3);
            LoadTrackingBtn.Name = "LoadTrackingBtn";
            LoadTrackingBtn.Size = new Size(270, 44);
            LoadTrackingBtn.TabIndex = 10;
            LoadTrackingBtn.Text = "Load Tracking";
            LoadTrackingBtn.Click += LoadTrackingBtn_Click;
            // 
            // gprTracking
            // 
            gprTracking.Controls.Add(SaveTrackingBtn);
            gprTracking.Controls.Add(lblStartFrame);
            gprTracking.Controls.Add(txtBoxHeight);
            gprTracking.Controls.Add(txtBoxWidth);
            gprTracking.Controls.Add(labelHeight);
            gprTracking.Controls.Add(labelWidth);
            gprTracking.Controls.Add(txtEndFrame);
            gprTracking.Controls.Add(txtStartFrame);
            gprTracking.Controls.Add(txtTrackingName);
            gprTracking.Controls.Add(lblEndFrame);
            gprTracking.Controls.Add(lblTrackingName);
            gprTracking.Controls.Add(btnApplyChanges);
            gprTracking.Location = new Point(3, 619);
            gprTracking.Name = "gprTracking";
            gprTracking.Size = new Size(270, 374);
            gprTracking.TabIndex = 9;
            gprTracking.TabStop = false;
            gprTracking.Text = "Tracking Properties";
            // 
            // SaveTrackingBtn
            // 
            SaveTrackingBtn.Location = new Point(8, 324);
            SaveTrackingBtn.Name = "SaveTrackingBtn";
            SaveTrackingBtn.Size = new Size(254, 44);
            SaveTrackingBtn.TabIndex = 0;
            SaveTrackingBtn.Text = "Save";
            SaveTrackingBtn.Click += SaveTrackingBtn_Click;
            // 
            // lblStartFrame
            // 
            lblStartFrame.AutoSize = true;
            lblStartFrame.Location = new Point(12, 175);
            lblStartFrame.Name = "lblStartFrame";
            lblStartFrame.Size = new Size(99, 32);
            lblStartFrame.TabIndex = 9;
            lblStartFrame.Text = "Strat Fr.:";
            // 
            // txtBoxHeight
            // 
            txtBoxHeight.Location = new Point(112, 128);
            txtBoxHeight.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            txtBoxHeight.Name = "txtBoxHeight";
            txtBoxHeight.Size = new Size(150, 39);
            txtBoxHeight.TabIndex = 7;
            txtBoxHeight.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // txtBoxWidth
            // 
            txtBoxWidth.Location = new Point(114, 83);
            txtBoxWidth.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            txtBoxWidth.Name = "txtBoxWidth";
            txtBoxWidth.Size = new Size(150, 39);
            txtBoxWidth.TabIndex = 6;
            txtBoxWidth.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new Point(12, 133);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new Size(91, 32);
            labelHeight.TabIndex = 3;
            labelHeight.Text = "Height:";
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Location = new Point(12, 88);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new Size(83, 32);
            labelWidth.TabIndex = 2;
            labelWidth.Text = "Width:";
            // 
            // txtEndFrame
            // 
            txtEndFrame.Location = new Point(112, 218);
            txtEndFrame.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            txtEndFrame.Name = "txtEndFrame";
            txtEndFrame.Size = new Size(150, 39);
            txtEndFrame.TabIndex = 8;
            // 
            // txtStartFrame
            // 
            txtStartFrame.Location = new Point(114, 173);
            txtStartFrame.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            txtStartFrame.Name = "txtStartFrame";
            txtStartFrame.Size = new Size(150, 39);
            txtStartFrame.TabIndex = 7;
            // 
            // txtTrackingName
            // 
            txtTrackingName.Location = new Point(112, 38);
            txtTrackingName.Name = "txtTrackingName";
            txtTrackingName.Size = new Size(150, 39);
            txtTrackingName.TabIndex = 5;
            // 
            // lblEndFrame
            // 
            lblEndFrame.AutoSize = true;
            lblEndFrame.Location = new Point(12, 218);
            lblEndFrame.Name = "lblEndFrame";
            lblEndFrame.Size = new Size(91, 32);
            lblEndFrame.TabIndex = 4;
            lblEndFrame.Text = "End Fr.:";
            // 
            // lblTrackingName
            // 
            lblTrackingName.AutoSize = true;
            lblTrackingName.Location = new Point(12, 43);
            lblTrackingName.Name = "lblTrackingName";
            lblTrackingName.Size = new Size(83, 32);
            lblTrackingName.TabIndex = 1;
            lblTrackingName.Text = "Name:";
            // 
            // btnApplyChanges
            // 
            btnApplyChanges.Location = new Point(8, 274);
            btnApplyChanges.Name = "btnApplyChanges";
            btnApplyChanges.Size = new Size(256, 44);
            btnApplyChanges.TabIndex = 0;
            btnApplyChanges.Text = "Apply";
            btnApplyChanges.Click += BtnApplyChanges_Click;
            // 
            // timePanel
            // 
            timePanel.Controls.Add(lblStartTime);
            timePanel.Controls.Add(lblCurrentTime);
            timePanel.Controls.Add(lblEndTime);
            timePanel.Dock = DockStyle.Fill;
            timePanel.Location = new Point(3, 1171);
            timePanel.Name = "timePanel";
            timePanel.Size = new Size(1194, 34);
            timePanel.TabIndex = 11;
            // 
            // lblStartTime
            // 
            lblStartTime.AutoSize = true;
            lblStartTime.Location = new Point(10, 10);
            lblStartTime.Name = "lblStartTime";
            lblStartTime.Size = new Size(102, 32);
            lblStartTime.TabIndex = 0;
            lblStartTime.Text = "00:00:00";
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.AutoSize = true;
            lblCurrentTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCurrentTime.Location = new Point(450, 10);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Size = new Size(112, 32);
            lblCurrentTime.TabIndex = 1;
            lblCurrentTime.Text = "00:00:00";
            // 
            // lblEndTime
            // 
            lblEndTime.AutoSize = true;
            lblEndTime.Location = new Point(900, 10);
            lblEndTime.Name = "lblEndTime";
            lblEndTime.Size = new Size(102, 32);
            lblEndTime.TabIndex = 2;
            lblEndTime.Text = "00:00:00";
            // 
            // pnlFrameSlider
            // 
            pnlFrameSlider.Controls.Add(trackRangeIndicator);
            pnlFrameSlider.Controls.Add(FrameSlider);
            pnlFrameSlider.Dock = DockStyle.Fill;
            pnlFrameSlider.Location = new Point(3, 1111);
            pnlFrameSlider.Name = "pnlFrameSlider";
            pnlFrameSlider.Size = new Size(1194, 54);
            pnlFrameSlider.TabIndex = 12;
            // 
            // trackRangeIndicator
            // 
            trackRangeIndicator.Anchor = AnchorStyles.None;
            trackRangeIndicator.BackColor = Color.FromArgb(100, 50, 205, 50);
            trackRangeIndicator.Location = new Point(3, 41);
            trackRangeIndicator.Name = "trackRangeIndicator";
            trackRangeIndicator.Size = new Size(274, 10);
            trackRangeIndicator.TabIndex = 13;
            // 
            // statusStripBottom
            // 
            statusStripBottom.ImageScalingSize = new Size(24, 24);
            statusStripBottom.Items.AddRange(new ToolStripItem[] { tssStatusLabel });
            statusStripBottom.Location = new Point(0, 1166);
            statusStripBottom.Name = "statusStripBottom";
            statusStripBottom.Size = new Size(1480, 42);
            statusStripBottom.TabIndex = 10;
            statusStripBottom.Text = "statusStrip1";
            // 
            // tssStatusLabel
            // 
            tssStatusLabel.Name = "tssStatusLabel";
            tssStatusLabel.Size = new Size(78, 32);
            tssStatusLabel.Text = "Ready";
            // 
            // VideoPlayer
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1480, 1208);
            Controls.Add(statusStripBottom);
            Controls.Add(mainTable);
            Name = "VideoPlayer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Motion Tracker";
            ((System.ComponentModel.ISupportInitialize)VideoBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)FrameSlider).EndInit();
            mainTable.ResumeLayout(false);
            buttonLayoutPanel.ResumeLayout(false);
            gprTracking.ResumeLayout(false);
            gprTracking.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtBoxHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtBoxWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtEndFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtStartFrame).EndInit();
            timePanel.ResumeLayout(false);
            timePanel.PerformLayout();
            pnlFrameSlider.ResumeLayout(false);
            pnlFrameSlider.PerformLayout();
            statusStripBottom.ResumeLayout(false);
            statusStripBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox VideoBox;
        private System.Windows.Forms.TrackBar FrameSlider;
        private System.Windows.Forms.Button OpenVideoBtn;
        private System.Windows.Forms.Button ExportVideoBtn;
        private System.Windows.Forms.Button AddTrackingBtn;
        private System.Windows.Forms.Button AnalyzeBtn;
        private System.Windows.Forms.Button StopAnalyzeBtn;
        private System.Windows.Forms.Button DeleteTrackingBtn;
        private System.Windows.Forms.ListBox TrackingListBox;
        private System.Windows.Forms.TableLayoutPanel mainTable;
        private System.Windows.Forms.FlowLayoutPanel buttonLayoutPanel;
        private System.Windows.Forms.GroupBox gprTracking;
        private System.Windows.Forms.NumericUpDown txtEndFrame;
        private System.Windows.Forms.NumericUpDown txtStartFrame;
        private System.Windows.Forms.TextBox txtTrackingName;
        private System.Windows.Forms.Label lblEndFrame;
        private System.Windows.Forms.Label lblTrackingName;
        private System.Windows.Forms.Button btnApplyChanges;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblCurrentTime;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.Panel timePanel;
        private System.Windows.Forms.StatusStrip statusStripBottom;
        private System.Windows.Forms.ToolStripStatusLabel tssStatusLabel;
        private Button SaveTrackingBtn;
        private System.Windows.Forms.NumericUpDown txtBoxWidth;
        private System.Windows.Forms.NumericUpDown txtBoxHeight;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelHeight;

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
        private Label lblStartFrame;
        private Panel pnlFrameSlider;
        private Panel trackRangeIndicator;
        private Button LoadTrackingBtn;
    }
}