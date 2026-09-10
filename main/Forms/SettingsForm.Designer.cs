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
            this.SuspendLayout();
            // 
            // lblGridCellSize
            // 
            this.lblGridCellSize.AutoSize = true;
            this.lblGridCellSize.Location = new Point(14, 25);
            this.lblGridCellSize.Name = "lblGridCellSize";
            this.lblGridCellSize.Size = new Size(80, 20);
            this.lblGridCellSize.TabIndex = 0;
            // 
            // nudGridCellSize
            // 
            this.nudGridCellSize.Location = new Point(14, 48);
            this.nudGridCellSize.Name = "nudGridCellSize";
            this.nudGridCellSize.Size = new Size(100, 26);
            this.nudGridCellSize.TabIndex = 1;
            // 
            // lblConfidenceThreshold
            // 
            this.lblConfidenceThreshold.AutoSize = true;
            this.lblConfidenceThreshold.Location = new Point(14, 28);
            this.lblConfidenceThreshold.Name = "lblConfidenceThreshold";
            this.lblConfidenceThreshold.Size = new Size(80, 20);
            this.lblConfidenceThreshold.TabIndex = 4;
            // 
            // nudConfidenceThreshold
            // 
            this.nudConfidenceThreshold.DecimalPlaces = 2;
            this.nudConfidenceThreshold.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudConfidenceThreshold.Location = new Point(14, 51);
            this.nudConfidenceThreshold.Name = "nudConfidenceThreshold";
            this.nudConfidenceThreshold.Size = new Size(100, 26);
            this.nudConfidenceThreshold.TabIndex = 5;
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
            this.gbBlur.Location = new Point(10, 10);
            this.gbBlur.Name = "gbBlur";
            this.gbBlur.Size = new Size(380, 90);
            this.gbBlur.TabIndex = 7;
            this.gbBlur.TabStop = false;
            this.gbBlur.Text = "Blur Settings";
            // 
            // gbConfidence
            // 
            this.gbConfidence.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.gbConfidence.Location = new Point(10, 106);
            this.gbConfidence.Name = "gbConfidence";
            this.gbConfidence.Size = new Size(380, 95);
            this.gbConfidence.TabIndex = 8;
            this.gbConfidence.TabStop = false;
            this.gbConfidence.Text = "Tracking Settings";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnSave.Location = new Point(346, 345);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(89, 30);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += BtnSave_Click;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCancel.Location = new Point(251, 345);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(89, 30);
            this.btnCancel.TabIndex = 10;
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
            this.ClientSize = new Size(444, 390);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
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
            this.gbBlur.Controls.Add(this.lblGridCellSize);
            this.gbBlur.Controls.Add(this.nudGridCellSize);
            this.gbConfidence.Controls.Add(this.lblConfidenceThreshold);
            this.gbConfidence.Controls.Add(this.nudConfidenceThreshold);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
