using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShutterFace
{
    partial class SettingsForm
    {
        private Label lblBlurCellSize;
        private NumericUpDown nudBlurCellSize;
        private Label lblBigPixels;
        private NumericUpDown nudBigPixels;
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
            this.lblBlurCellSize = new Label();
            this.nudBlurCellSize = new NumericUpDown();
            this.lblBigPixels = new Label();
            this.nudBigPixels = new NumericUpDown();
            this.lblConfidenceThreshold = new Label();
            this.nudConfidenceThreshold = new NumericUpDown();
            this.pnlSettings = new Panel();
            this.gbBlur = new GroupBox();
            this.gbConfidence = new GroupBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();
            // 
            // lblBlurCellSize
            // 
            this.lblBlurCellSize.AutoSize = true;
            this.lblBlurCellSize.Location = new Point(20, 25);
            this.lblBlurCellSize.Name = "lblBlurCellSize";
            this.lblBlurCellSize.Size = new Size(108, 20);
            this.lblBlurCellSize.TabIndex = 0;
            this.lblBlurCellSize.Text = "Vergröberung:";
            // 
            // nudBlurCellSize
            // 
            this.nudBlurCellSize.Location = new Point(20, 50);
            this.nudBlurCellSize.Name = "nudBlurCellSize";
            this.nudBlurCellSize.Size = new Size(100, 26);
            this.nudBlurCellSize.TabIndex = 1;
            // 
            // lblBigPixels
            // 
            this.lblBigPixels.AutoSize = true;
            this.lblBigPixels.Location = new Point(20, 85);
            this.lblBigPixels.Name = "lblBigPixels";
            this.lblBigPixels.Size = new Size(156, 20);
            this.lblBigPixels.TabIndex = 2;
            this.lblBigPixels.Text = "Big Pixels (längste Seite):";
            // 
            // nudBigPixels
            // 
            this.nudBigPixels.Location = new Point(20, 110);
            this.nudBigPixels.Name = "nudBigPixels";
            this.nudBigPixels.Size = new Size(100, 26);
            this.nudBigPixels.TabIndex = 3;
            // 
            // lblConfidenceThreshold
            // 
            this.lblConfidenceThreshold.AutoSize = true;
            this.lblConfidenceThreshold.Location = new Point(20, 25);
            this.lblConfidenceThreshold.Name = "lblConfidenceThreshold";
            this.lblConfidenceThreshold.Size = new Size(156, 20);
            this.lblConfidenceThreshold.TabIndex = 4;
            this.lblConfidenceThreshold.Text = "Konfidenz-Schwelle:";
            // 
            // nudConfidenceThreshold
            // 
            this.nudConfidenceThreshold.DecimalPlaces = 2;
            this.nudConfidenceThreshold.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
            this.nudConfidenceThreshold.Location = new Point(20, 50);
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
            this.gbBlur.Size = new Size(380, 145);
            this.gbBlur.TabIndex = 7;
            this.gbBlur.TabStop = false;
            this.gbBlur.Text = "Unschärfe-Einstellungen";
            // 
            // gbConfidence
            // 
            this.gbConfidence.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.gbConfidence.Location = new Point(10, 165);
            this.gbConfidence.Name = "gbConfidence";
            this.gbConfidence.Size = new Size(380, 85);
            this.gbConfidence.TabIndex = 8;
            this.gbConfidence.TabStop = false;
            this.gbConfidence.Text = "Tracking-Einstellungen";
            // 
            // btnSave
            // 
            this.btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnSave.Location = new Point(346, 345);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(89, 30);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Speichern";
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
            this.btnCancel.Text = "Abbrechen";
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
            this.Text = "Einstellungen";
            this.Icon = AboutForm.CreateAppIconAsIcon();
            this.Load += SettingsForm_Load;
            this.pnlSettings.Controls.Add(this.gbConfidence);
            this.pnlSettings.Controls.Add(this.gbBlur);
            this.gbBlur.Controls.Add(this.lblBigPixels);
            this.gbBlur.Controls.Add(this.nudBigPixels);
            this.gbBlur.Controls.Add(this.lblBlurCellSize);
            this.gbBlur.Controls.Add(this.nudBlurCellSize);
            this.gbConfidence.Controls.Add(this.lblConfidenceThreshold);
            this.gbConfidence.Controls.Add(this.nudConfidenceThreshold);
            this.ResumeLayout(false);
        }

        #endregion
    }
}
