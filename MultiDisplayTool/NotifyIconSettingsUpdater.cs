using Microsoft.Win32;
using System;

/*
 * Copyright (c) 2024 Mehdi Dimyadi
 * 
 * This file is part of the MultiDisplayTool project.
 * 
 * The NotifyIconSettingsUpdater class provides functionality to update the settings for
 * the application’s notification icon in the Windows registry. It adjusts the visibility
 * of the notification icon by modifying the relevant registry keys and values.
 * 
 * Repository: https://github.com/mehdimyadi
 * Social: @mehdimyadi
 * 
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

namespace MultiDisplayTool
{
    internal class NotifyIconSettingsUpdater
    {
        public static void UpdateNotifyIconSettings(string programName, short setting)
        {
            // Validate the setting value
            if (setting < 0 || setting > 1)
            {
                throw new ArgumentException("Invalid setting value. It must be 0 or 1.");
            }

            // Define the registry path
            string basePath = @"Control Panel\NotifyIconSettings";

            // Open the registry key for current user
            using (RegistryKey baseKey = Registry.CurrentUser.OpenSubKey(basePath))
            {
                if (baseKey == null)
                {
                    throw new InvalidOperationException("Registry path does not exist.");
                }

                // Get subkeys (GUIDs)
                string[] subKeys = baseKey.GetSubKeyNames();
                foreach (string subKey in subKeys)
                {
                    string childPath = $"{basePath}\\{subKey}";

                    using (RegistryKey childKey = baseKey.OpenSubKey(subKey, writable: true))
                    {
                        if (childKey == null)
                        {
                            continue; // Skip if we can't open the subkey
                        }

                        // Get ExecutablePath and IsPromoted values
                        object execPathObj = childKey.GetValue("ExecutablePath");
                        object isPromotedObj = childKey.GetValue("IsPromoted");

                        if (execPathObj is string execPath && execPath.Contains(programName))
                        {
                            // Update the IsPromoted value
                            childKey.SetValue("IsPromoted", setting, RegistryValueKind.DWord);
                        }
                    }
                }
            }
        }
    }
}
