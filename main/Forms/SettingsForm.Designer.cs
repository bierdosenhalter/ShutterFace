using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShutterFace
{
    partial class SettingsForm
    {
        private Label lblGridCellSize;
        private NumericUpDown nudGridCellSize;
        private Label lblConfidenceThreshold;
        private NumericUpDown nudConfidenceThreshold;
        private Panel pnlSettings;
        private GroupBox gbBlur;
        private GroupBox gbConfidence;
        private Button btnSave;
        private Button btnCancel;
        private TableLayoutPanel btnLayout;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblGridCellSize = new Label();
            this.nudGridCellSize = new NumericUpDown();
            this.lblConfidenceThreshold = new Label();
            this.nudConfidenceThreshold = new NumericUpDown();
            this.pnlSettings = new Panel();
            this.gbBlur = new GroupBox();
            this.gbConfidence = new GroupBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.btnLayout = new TableLayoutPanel();
            this.SuspendLayout();
            // 
            // pnlSettings
            // 
            this.pnlSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.pnlSettings.AutoScroll = true;
            this.pnlSettings.Location = new Point(12, 12);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new Size(400, 320);
            this.pnlSettings.TabIndex = 6;
            // 
            // gbBlur
            // 
            this.gbBlur.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.gbBlur.Controls.Add(this.lblGridCellSize);
            this.gbBlur.Controls.Add(this.nudGridCellSize);
            this.gbBlur.Location = new Point(10, 12);
            this.gbBlur.Name = "gbBlur";
            this.gbBlur.Size = new Size(376, 70);
            this.gbBlur.TabIndex = 7;
            this.gbBlur.TabStop = false;
            this.gbBlur.Text = "Blur Settings";
            // 
            // gbConfidence
            // 
            this.gbConfidence.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.gbConfidence.Controls.Add(this.lblConfidenceThreshold);
            this.gbConfidence.Controls.Add(this.nudConfidenceThreshold);
            this.gbConfidence.Location = new Point(10, 88);
            this.gbConfidence.Name = "gbConfidence";
            this.gbConfidence.Size = new Size(376, 75);
            this.gbConfidence.TabIndex = 8;
            this.gbConfidence.TabStop = false;
            this.gbConfidence.Text = "Tracking Settings";
            // 
            // lblGridCellSize
            // 
            this.lblGridCellSize.AutoSize = true;
            this.lblGridCellSize.Location = new Point(10, 22);
            this.lblGridCellSize.Name = "lblGridCellSize";
            this.lblGridCellSize.Size = new Size(82, 20);
            this.lblGridCellSize.TabIndex = 0;
            this.lblGridCellSize.Text = "Grid Cell Size:";
            // 
            // nudGridCellSize
            // 
            this.nudGridCellSize.Location = new Point(10, 45);
            this.nudGridCellSize.Name = "nudGridCellSize";
            this.nudGridCellSize.Size = new Size(90, 23);
            this.nudGridCellSize.TabIndex = 1;
            this.nudGridCellSize.Margin = new Padding(3, 3, 15, 3);
            // 
            // lblConfidenceThreshold
            // 
            this.lblConfidenceThreshold.AutoSize = true;
            this.lblConfidenceThreshold.Location = new Point(10, 22);
            this.lblConfidenceThreshold.Name = "lblConfidenceThreshold";
            this.lblConfidenceThreshold.Size = new Size(85, 20);
            this.lblConfidenceThreshold.TabIndex = 4;
            this.lblConfidenceThreshold.Text = "Confidence Threshold:";
            // 
            // nudConfidenceThreshold
            // 
            this.nudConfidenceThreshold.DecimalPlaces = 2;
            this.nudConfidenceThreshold.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudConfidenceThreshold.Location = new Point(10, 45);
            this.nudConfidenceThreshold.Name = "nudConfidenceThreshold";
            this.nudConfidenceThreshold.Size = new Size(90, 23);
            this.nudConfidenceThreshold.TabIndex = 5;
            this.nudConfidenceThreshold.Margin = new Padding(3, 3, 15, 3);
            // 
            // btnLayout
            // 
            this.btnLayout.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.btnLayout.ColumnCount = 2;
            this.btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));
            this.btnLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 94F));
            this.btnLayout.Controls.Add(this.btnCancel, 1, 0);
            this.btnLayout.Controls.Add(this.btnSave, 0, 0);
            this.btnLayout.Location = new Point(12, 326);
            this.btnLayout.Name = "btnLayout";
            this.btnLayout.RowCount = 1;
            this.btnLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.btnLayout.Size = new Size(376, 34);
            this.btnLayout.TabIndex = 9;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.btnSave.DialogResult = DialogResult.OK;
            this.btnSave.Location = new Point(3, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(88, 26);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += BtnSave_Click;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new Point(97, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(88, 26);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += BtnCancel_Click;
            // 
            // SettingsForm
            // 
            this.AcceptButton = btnSave;
            this.AutoScaleDimensions = new SizeF(9F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.CancelButton = btnCancel;
            this.ClientSize = new Size(436, 380);
            this.Controls.Add(this.btnLayout);
            this.Controls.Add(this.pnlSettings);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.Icon = AboutForm.CreateAppIconAsIcon();
            this.Load += SettingsForm_Load;
            this.pnlSettings.Controls.Add(this.gbConfidence);
            this.pnlSettings.Controls.Add(this.gbBlur);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
