using System;
using System.Windows.Forms;

/*
 * Copyright (c) 2024 Mehdi Dimyadi
 * 
 * This file is part of the MultiDisplayTool project.
 * 
 * The AboutForm class provides a form to display information about the application.
 * It includes an icon, layout adjustments, and functionality for opening a GitHub link
 * and closing the form when the OK button is clicked.
 * 
 * Repository: https://github.com/mehdimyadi
 * Social: @mehdimyadi
 * 
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

namespace MultiDisplayTool
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath); // Set app icon
        }

        private void AboutForm_Load(object sender, EventArgs e)
        {
            AdjustFormLayout();
        }

        private void AdjustFormLayout()
        {
            // Adjust the OK button's position based on the final layout
            btnOK.Top = lnkGitHub.Bottom + 20;
            this.ClientSize = new System.Drawing.Size(this.ClientSize.Width, btnOK.Bottom + 20);
            btnOK.Left = (this.ClientSize.Width - btnOK.Width) / 2;
        }

        private void lnkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Link.LinkData as string,
                UseShellExecute = true
            });
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close(); // Close the form when OK button is clicked
        }
    }
}