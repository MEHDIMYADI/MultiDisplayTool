using Microsoft.Win32;
using System;

/*
 * Copyright (c) 2024 Mehdi Dimyadi
 * 
 * This file is part of the MultiDisplayTool project.
 * 
 * The StartupManager class provides functionality to manage the application's startup
 * behavior by interacting with the Windows Registry. It allows adding or removing the
 * application from the startup list and checking whether it is set to start automatically.
 * 
 * Repository: https://github.com/mehdimyadi
 * Social: @mehdimyadi
 * 
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

namespace MultiDisplayTool
{
    internal class StartupManager
    {
        private const string AppName = "MultiDisplayTool";
        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public void AddToStartup()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string arguments = "--silent";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null)
                    {
                        Console.WriteLine("Failed to open registry key.");
                        return;
                    }

                    key.SetValue(AppName, $"\"{exePath}\" {arguments}");
                    Console.WriteLine($"{AppName} added to startup with path: {exePath} and arguments: {arguments}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to startup: {ex.Message}");
            }
        }

        public void RemoveFromStartup()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null)
                    {
                        Console.WriteLine("Failed to open registry key.");
                        return;
                    }

                    if (key.GetValue(AppName) != null)
                    {
                        key.DeleteValue(AppName);
                        Console.WriteLine($"{AppName} removed from startup.");
                    }
                    else
                    {
                        Console.WriteLine($"{AppName} is not in the startup list.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from startup: {ex.Message}");
            }
        }

        public bool IsInStartup()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath))
            {
                if (key == null)
                {
                    Console.WriteLine("Failed to open registry key.");
                    return false;
                }

                return key.GetValue(AppName) != null;
            }
        }
    }
}
