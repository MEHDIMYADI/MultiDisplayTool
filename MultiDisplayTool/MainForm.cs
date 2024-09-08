using Microsoft.Win32;
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
        private bool autoSetPrimaryDisplay;
        private readonly StartupManager startupManager;
        readonly bool silentModeDetect = false;

        public MainForm(bool silentMode)
        {
            startupManager = new StartupManager();
            silentModeDetect = silentMode;

            this.Load += (s, e) =>
            {
                LoadSettings();
                this.Visible = false;
                this.ShowInTaskbar = false;
                this.WindowState = FormWindowState.Minimized;
                InitializeTrayIcon(silentMode);
                UpdateStartupMenuItem();
                SetNotifyIconAlwaysVisible(); // icon is always visible

                // Use CheckAndStartMonitoring to manage monitoring based on the updated autoMoveEnabled state
                WindowMover.CheckAndStartMonitoring(autoMoveEnabled);
            };

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        private void InitializeTrayIcon(bool silentMode)
        {
            var contextMenu = CreateContextMenu();
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Asset", "MultiDisplayToolNotify.ico")),
                Visible = true,
                Text = Properties.Resources.mainFormTitle,
                ContextMenuStrip = contextMenu,
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

            // Subscribe to the Opening and Closed events
            contextMenu.Opening += ContextMenu_Opening;
            contextMenu.Closed += ContextMenu_Closed;

            // Add options to set primary display
            contextMenu.Items.Add(Properties.Resources.mainFormSetPrimaryDisplay, null, (sender, e) => Program.SetPrimaryDisplay());

            // Move all windows to the primary monitor
            contextMenu.Items.Add(Properties.Resources.mainFormMoveWindowsToPrimary, null, (sender, e) => Program.MoveWindowsToNextMonitor("Primary"));

            // Move all windows to the next monitor
            contextMenu.Items.Add(Properties.Resources.mainFormMoveWindowsToNext, null, (sender, e) => Program.MoveWindowsToNextMonitor("Next"));

            // Auto-move startup and shutdown windows item
            var autoSetPrimaryItem = new ToolStripMenuItem
            {
                Text = autoSetPrimaryDisplay ? Properties.Resources.mainFormDisableAutoSetPrimary : Properties.Resources.mainFormEnableAutoSetPrimary
            };
            autoSetPrimaryItem.Click += (sender, e) => ToggleAutoSetPrimaryDisplayEntry();
            contextMenu.Items.Add(autoSetPrimaryItem);

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

        // Called when the context menu is about to open
        private void ContextMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Stop monitoring when the context menu is shown
            WindowMover.CheckAndStartMonitoring(false);
        }

        // Called when the context menu is closed
        private void ContextMenu_Closed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            // Restart monitoring based on the current autoMoveEnabled state
            WindowMover.CheckAndStartMonitoring(autoMoveEnabled);
        }

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Program.MoveWindowsToNextMonitor("Next");
            }
            else if (e.Button == MouseButtons.Right)
            {
                WindowMover.StopMonitoring();
            }
        }

        private void SetNotifyIconAlwaysVisible()
        {
            try
            {
                NotifyIconSettingsUpdater.UpdateNotifyIconSettings("MultiDisplayTool.exe", 1); // 1 = show icon, 0 = hide icon
                Console.WriteLine("Tray icon visibility updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating tray icon visibility: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                autoMoveEnabled = Properties.Settings.Default.AutoMoveEnabled;
                autoSetPrimaryDisplay = Properties.Settings.Default.AutoSetPrimaryDisplay;

                Console.WriteLine($"Settings loaded: AutoMoveEnabled = {autoMoveEnabled}, AutoSetPrimaryDisplay = {autoSetPrimaryDisplay}");

                if (autoSetPrimaryDisplay && silentModeDetect)
                {
                    Program.SetPrimaryDisplay();
                }

                // Use CheckAndStartMonitoring to manage monitoring based on the autoMoveEnabled setting
                WindowMover.CheckAndStartMonitoring(autoMoveEnabled);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            // Check if the session switch reason is a successful logon or unlock
            if (e.Reason == SessionSwitchReason.SessionLogon || e.Reason == SessionSwitchReason.SessionUnlock)
            {
                if (autoSetPrimaryDisplay)
                {
                    Program.SetPrimaryDisplay();
                }
            }
        }

        private void ShowAboutInfo(object sender, EventArgs e)
        {
            // Stop monitoring when showing the About form
            WindowMover.CheckAndStartMonitoring(false);

            using (var aboutForm = new AboutForm())
            {
                // Show the About form modally
                aboutForm.ShowDialog();
            }

            // Restart monitoring based on the autoMoveEnabled state after closing the About form
            WindowMover.CheckAndStartMonitoring(autoMoveEnabled);
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            WindowMover.StopMonitoring();
            Application.Exit();
        }

        private void ToggleAutoMoveEntry()
        {
            autoMoveEnabled = !autoMoveEnabled;
            Properties.Settings.Default.AutoMoveEnabled = autoMoveEnabled;
            Properties.Settings.Default.Save();

            // Use CheckAndStartMonitoring to manage monitoring based on the updated autoMoveEnabled state
            WindowMover.CheckAndStartMonitoring(autoMoveEnabled);
            UpdateStartupMenuItem();
        }

        private void ToggleAutoSetPrimaryDisplayEntry()
        {
            autoSetPrimaryDisplay = !autoSetPrimaryDisplay;
            Properties.Settings.Default.AutoSetPrimaryDisplay = autoSetPrimaryDisplay;
            Properties.Settings.Default.Save();
            UpdateStartupMenuItem();
        }

        private void ToggleStartupEntry()
        {
            if (startupManager.IsInStartup())
            {
                startupManager.RemoveFromStartup();
            }
            else
            {
                startupManager.AddToStartup();
            }
            UpdateStartupMenuItem();
        }

        private void UpdateStartupMenuItem()
        {
            if (notifyIcon.ContextMenuStrip.Items[3] is ToolStripMenuItem autoSetPrimaryItem)
            {
                autoSetPrimaryItem.Text = autoSetPrimaryDisplay ? Properties.Resources.mainFormDisableAutoSetPrimary : Properties.Resources.mainFormEnableAutoSetPrimary;
            }

            if (notifyIcon.ContextMenuStrip.Items[4] is ToolStripMenuItem autoMoveItem)
            {
                autoMoveItem.Text = autoMoveEnabled ? Properties.Resources.mainFormDisableAutoMove : Properties.Resources.mainFormEnableAutoMove;
            }

            if (notifyIcon.ContextMenuStrip.Items[5] is ToolStripMenuItem startupItem)
            {
                startupItem.Text = startupManager.IsInStartup() ? Properties.Resources.mainFormRemoveFromStartup : Properties.Resources.mainFormAddToStartup;
            }
        }
    }
}