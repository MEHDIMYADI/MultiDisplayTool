using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

/*
 * Copyright (c) 2024 Mehdi Dimyadi
 * 
 * This file is part of the MultiDisplayTool project.
 * 
 * The Program class serves as the entry point for the application, handling command-line
 * arguments to determine if the application should run in silent mode. It provides methods
 * for setting the primary display and moving windows to different monitors by executing
 * external tools with specific arguments.
 * 
 * Repository: https://github.com/mehdimyadi
 * Social: @mehdimyadi
 * 
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

namespace MultiDisplayTool
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool silentMode = args.Contains("--silent");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(silentMode));
        }

        public static void SetPrimaryDisplay()
        {
            ExecuteTool("/SetNextPrimary");
        }

        public static void MoveWindowsToNextMonitor(string monitor)
        {
            string arguments = monitor == "Primary" ? "/MoveWindow Primary All" : "/MoveWindow Next All Primary";
            ExecuteTool(arguments);
        }

        private static void ExecuteTool(string arguments)
        {
            string toolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Tools", "MultiMonitorTool.exe");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = toolPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false
                });

                // Log tool execution
                Console.WriteLine($"Executed tool with arguments: {arguments}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to execute tool: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}