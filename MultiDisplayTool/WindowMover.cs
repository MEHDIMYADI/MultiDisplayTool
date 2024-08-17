using System;
using System.Runtime.InteropServices;

/*
 * Copyright (c) 2024 Mehdi Dimyadi
 * 
 * This file is part of the MultiDisplayTool project.
 * 
 * The WindowMover class monitors window creation events and moves newly created windows
 * to the next monitor. It uses Windows API functions and hooks to achieve this functionality.
 * 
 * Repository: https://github.com/mehdimyadi
 * Social: @mehdimyadi
 * 
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

namespace MultiDisplayTool
{
    internal class WindowMover
    {
        // WinEvent constants
        private const uint EVENT_OBJECT_CREATE = 0x8000;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        // Window styles
        private const int GWL_STYLE = -16;
        private const int WS_VISIBLE = 0x10000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        // Delegate for WinEventProc
        private delegate void WinEventDelegate(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime
        );

        // Fields
        private static WinEventDelegate _winEventDelegate;
        private static IntPtr _winEventHook;
        private static bool _isMonitoring;

        // Imported functions
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(
            uint eventMin,
            uint eventMax,
            IntPtr hmodWinEventProc,
            WinEventDelegate lpfnWinEventProc,
            uint idProcess,
            uint idThread,
            uint dwFlags
        );

        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        // Public methods
        public static void StartMonitoring()
        {
            if (_isMonitoring) return;

            _winEventDelegate = WinEventProc;
            _winEventHook = SetWinEventHook(
                EVENT_OBJECT_CREATE,
                EVENT_OBJECT_CREATE,
                IntPtr.Zero,
                _winEventDelegate,
                0,
                0,
                WINEVENT_OUTOFCONTEXT
            );

            if (_winEventHook == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                // Consider using a logging framework or a more robust error handling approach
                Console.WriteLine($"Failed to set hook. Error Code: {errorCode}");
            }
            else
            {
                _isMonitoring = true;
                // Optionally log or notify the start of monitoring
            }
        }

        public static void StopMonitoring()
        {
            if (!_isMonitoring) return;

            if (UnhookWinEvent(_winEventHook))
            {
                _winEventHook = IntPtr.Zero;
                _isMonitoring = false;
                // Optionally log or notify the stop of monitoring
            }
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                // Consider using a logging framework or a more robust error handling approach
                Console.WriteLine($"Failed to stop monitoring. Error Code: {errorCode}");
            }
        }

        // Private method to handle window events
        private static void WinEventProc(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime
        )
        {
            // Perform checks to minimize unnecessary work
            if (hwnd == IntPtr.Zero || !IsWindowVisible(hwnd))
                return;

            long style = GetWindowLong(hwnd, GWL_STYLE).ToInt64();

            // Check if the window is visible and not a tool window
            if ((style & WS_VISIBLE) != 0 && (style & WS_EX_TOOLWINDOW) == 0)
            {
                // Move the window to the next monitor
                Program.MoveWindowsToNextMonitor("Next");
                // Minimize or remove console output in production
            }
        }
    }
}