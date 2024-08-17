namespace MultiDisplayTool
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
            this.lblDescription = new System.Windows.Forms.Label();
            this.lnkGitHub = new System.Windows.Forms.LinkLabel();
            this.btnOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblDescription
            // 
            this.lblDescription.AutoSize = true;
            this.lblDescription.Location = new System.Drawing.Point(12, 9);
            this.lblDescription.MaximumSize = new System.Drawing.Size(260, 0);
            this.lblDescription.Name = "lblDescription";
            this.lblDescription.Size = new System.Drawing.Size(252, 65);
            this.lblDescription.TabIndex = 0;
            this.lblDescription.Text = global::MultiDisplayTool.Properties.Resources.aboutDescription;
            // 
            // lnkGitHub
            // 
            this.lnkGitHub.AutoSize = true;
            this.lnkGitHub.LinkArea = new System.Windows.Forms.LinkArea(8, 11);
            this.lnkGitHub.Location = new System.Drawing.Point(12, 86);
            this.lnkGitHub.Name = "lnkGitHub";
            this.lnkGitHub.Size = new System.Drawing.Size(118, 17);
            this.lnkGitHub.TabIndex = 1;
            this.lnkGitHub.TabStop = true;
            this.lnkGitHub.Text = global::MultiDisplayTool.Properties.Resources.aboutGitHubLink;
            this.lnkGitHub.UseCompatibleTextRendering = true;
            this.lnkGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGitHub_LinkClicked);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(105, 106);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = global::MultiDisplayTool.Properties.Resources.aboutOKButton;
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 141);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lnkGitHub);
            this.Controls.Add(this.lblDescription);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = global::MultiDisplayTool.Properties.Resources.aboutFormTitle;
            this.Load += new System.EventHandler(this.AboutForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.LinkLabel lnkGitHub;
        private System.Windows.Forms.Button btnOK;
    }
}