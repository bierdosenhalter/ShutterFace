namespace ShutterFace
{
    partial class AboutForm
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
            picIcon = new PictureBox();
            lblTitle = new Label();
            lblSubtitle = new Label();
            btnOk = new Button();
            ((System.ComponentModel.ISupportInitialize)picIcon).BeginInit();
            SuspendLayout();
            // 
            // picIcon
            // 
            picIcon.Image = CreateAppIcon();
            picIcon.Location = new Point(12, 12);
            picIcon.Name = "picIcon";
            picIcon.Size = new Size(30, 30);
            picIcon.SizeMode = PictureBoxSizeMode.Zoom;
            picIcon.TabIndex = 0;
            picIcon.TabStop = false;
            // 
            // lblTitle
            // 
            lblTitle.AutoSize = true;
            lblTitle.Font = new Font(@"Segoe UI", 10f, FontStyle.Regular);
            lblTitle.Location = new Point(48, 12);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(130, 19);
            lblTitle.TabIndex = 1;
            lblTitle.Text = @"ShutterFace";
            // 
            // lblSubtitle
            // 
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font(@"Segoe UI", 8f, FontStyle.Regular);
            lblSubtitle.ForeColor = Color.FromArgb(80, 80, 80);
            lblSubtitle.Location = new Point(48, 32);
            lblSubtitle.Name = "lblSubtitle";
            lblSubtitle.Size = new Size(162, 15);
            lblSubtitle.TabIndex = 2;
            lblSubtitle.Text = @"Built with OpenCode (qwen3.6-35b-128k)";
            // 
            // btnOk
            // 
            btnOk.DialogResult = DialogResult.OK;
            btnOk.Font = new Font(@"Segoe UI", 9f, FontStyle.Regular);
            btnOk.Location = new Point(194, 100);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(67, 26);
            btnOk.TabIndex = 3;
            btnOk.Text = @"OK";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += BtnOk_Click;
            // 
            // AboutForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(274, 130);
            Controls.Add(picIcon);
            Controls.Add(lblTitle);
            Controls.Add(lblSubtitle);
            Controls.Add(btnOk);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutForm";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterParent;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picIcon;
        private Label lblTitle;
        private Label lblSubtitle;
        private Button btnOk;
    }
}
