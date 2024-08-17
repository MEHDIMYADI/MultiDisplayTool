using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

/*
 * Copyright (c) 2024 Mehdi Dimyadi
 * 
 * This file is part of the MultiDisplayTool project.
 * 
 * MainForm handles the application's main form functionality, including tray icon management,
 * context menu interactions, settings loading/saving, and window movement across displays.
 * 
 * Repository: https://github.com/mehdimyadi
 * Social: @mehdimyadi
 * 
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

namespace MultiDisplayTool
{
    public partial class MainForm : Form
    {

        private static NotifyIcon notifyIcon;
        private bool autoMoveEnabled;
        private StartupManager startupManager;

        public MainForm(bool silentMode)
        {
            startupManager = new StartupManager();

            this.Load += (s, e) =>
            {
                this.Visible = false;
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                InitializeTrayIcon(silentMode);
                LoadSettings();
                UpdateStartupMenuItem();
                SetNotifyIconAlwaysVisible(); // icon is always visible
            };
        }

        private void InitializeTrayIcon(bool silentMode)
        {
            var contextMenu = CreateContextMenu();
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Asset", "MultiDisplayToolNotify.ico")),
                Visible = true,
                Text = Properties.Resources.mainFormTitle,
                ContextMenuStrip = CreateContextMenu(),
                BalloonTipText = Properties.Resources.mainFormBalloonTipText
            };

            // Show the balloon tip only if silentMode is false
            if (!silentMode)
            {
                notifyIcon.ShowBalloonTip(5000);
            }

            // Subscribe to the MouseClick event
            notifyIcon.MouseClick += NotifyIcon_MouseClick;

        }

        private ContextMenuStrip CreateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();

            // Add options to set primary display
            contextMenu.Items.Add(Properties.Resources.mainFormSetPrimaryDisplay, null, (sender, e) => Program.SetPrimaryDisplay());

            // Move all windows to the primary monitor
            contextMenu.Items.Add(Properties.Resources.mainFormMoveWindowsToPrimary, null, (sender, e) => Program.MoveWindowsToNextMonitor("Primary"));

            // Move all windows to the next monitor
            contextMenu.Items.Add(Properties.Resources.mainFormMoveWindowsToNext, null, (sender, e) => Program.MoveWindowsToNextMonitor("Next"));

            // Auto-move windows item
            var autoMoveItem = new ToolStripMenuItem
            {
                Text = autoMoveEnabled ? Properties.Resources.mainFormDisableAutoMove : Properties.Resources.mainFormEnableAutoMove
            };
            autoMoveItem.Click += (sender, e) => ToggleAutoMoveEntry();
            contextMenu.Items.Add(autoMoveItem);

            // Startup item
            var startupItem = new ToolStripMenuItem
            {
                Text = startupManager.IsInStartup() ? Properties.Resources.mainFormRemoveFromStartup : Properties.Resources.mainFormAddToStartup
            };
            startupItem.Click += (sender, e) => ToggleStartupEntry();
            contextMenu.Items.Add(startupItem);

            // About option
            contextMenu.Items.Add(Properties.Resources.About, null, ShowAboutInfo);

            // Exit option
            contextMenu.Items.Add(Properties.Resources.Exit, null, ExitApplication);

            return contextMenu;
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            // Check if the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                Program.MoveWindowsToNextMonitor("Next");
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Stop monitoring when right-clicking
                WindowMover.StopMonitoring();

                // Restart monitoring after the context menu has been interacted with or a short delay
                // This can be done in a separate method or after some user interaction
                RestartWindowMoverMonitoring();
            }
        }

        private void RestartWindowMoverMonitoring()
        {
            // Delay or logic to restart monitoring
            // For demonstration, we'll use a delay
            Timer restartTimer = new Timer();
            restartTimer.Interval = 1000; // 1 second delay
            restartTimer.Tick += (s, e) =>
            {
                restartTimer.Stop();
                WindowMover.StartMonitoring(); // Restart monitoring
            };
            restartTimer.Start();
        }

        // Method to set the NotifyIcon to always be visible using Windows Registry
        private void SetNotifyIconAlwaysVisible()
        {
            try
            {
                NotifyIconSettingsUpdater.UpdateNotifyIconSettings("MultiDisplayTool.exe", 1); // 1 = show icon, 0 = hide icon
                Console.WriteLine("Tray icon visibility updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.mainFormErrorSavingSettings, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                Properties.Settings.Default.AutoMoveEnabled = autoMoveEnabled;
                Properties.Settings.Default.Save();
                Console.WriteLine("Settings saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.mainFormErrorSavingSettings, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                autoMoveEnabled = Properties.Settings.Default.AutoMoveEnabled;
                Console.WriteLine($"Settings loaded: AutoMoveEnabled = {autoMoveEnabled}");
                if (autoMoveEnabled)
                {
                    // Start monitoring for new processes
                    WindowMover.StartMonitoring();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Properties.Resources.mainFormErrorLoadingSettings, ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to show the About info when the "About" menu item is clicked
        private void ShowAboutInfo(object sender, EventArgs e)
        {
            // Stop monitoring when show about
            WindowMover.StopMonitoring();

            using (var aboutForm = new AboutForm())
            {
                aboutForm.ShowDialog();
            }

            // Restart monitoring after the context menu has been interacted with or a short delay
            // This can be done in a separate method or after some user interaction
            RestartWindowMoverMonitoring();
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            notifyIcon.Visible = false; // Hide tray icon before exiting
            WindowMover.StopMonitoring(); // Stop monitoring when the user presses a key
            Application.Exit(); // Close the application
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            notifyIcon.Visible = false; // Hide tray icon
            WindowMover.StopMonitoring(); // Stop monitoring when the user presses a key
            base.OnFormClosing(e);
        }

        private void ToggleAutoMoveEntry()
        {
            autoMoveEnabled = !autoMoveEnabled;
            SaveSettings();

            if (autoMoveEnabled)
            {
                // Start monitoring for new processes
                WindowMover.StartMonitoring();
                UpdateStartupMenuItem();
            }
            else
            {
                // Stop monitoring when the user presses a key
                WindowMover.StopMonitoring();
                UpdateStartupMenuItem();
            }
        }

        private void ToggleStartupEntry()
        {
            if (startupManager.IsInStartup())
            {
                startupManager.RemoveFromStartup();
                UpdateStartupMenuItem();
            }
            else
            {
                startupManager.AddToStartup();
                UpdateStartupMenuItem();
            }
        }

        private void UpdateStartupMenuItem()
        {
            if (notifyIcon.ContextMenuStrip.Items[3] is ToolStripMenuItem autoMoveItem)
            {
                autoMoveItem.Text = autoMoveEnabled ? Properties.Resources.mainFormDisableAutoMove : Properties.Resources.mainFormEnableAutoMove;
            }

            if (notifyIcon.ContextMenuStrip.Items[4] is ToolStripMenuItem startupItem)
            {
                startupItem.Text = startupManager.IsInStartup() ? Properties.Resources.mainFormRemoveFromStartup : Properties.Resources.mainFormAddToStartup;
            }
        }
    }
}