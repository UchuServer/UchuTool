using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Uchu.Tool.Action
{
    public class CheckNexusLULauncher
    {
        /// <summary>
        /// Message for Nexus LU Launcher not being run.
        /// </summary>
        private const string NotRanMessage = "Nexus LU Launcher was not detected. It is recommended to run it first to have avoid configuring it.\n\tDownload: https://github.com/TheNexusAvenger/Nexus-LU-Launcher/releases/latest";

        /// <summary>
        /// Message for Nexus LU Launcher being run but with no packed client.
        /// </summary>
        private const string NoUnpackedClientMessage = "Nexus LU Launcher was detected, but an unpacked client was not. Make sure an unpacked client is being used by going to Settings -> Source in Nexus LU Launcher.";
        
        /// <summary>
        /// Check whether a directory is the res folder of an unpacked client. Accounts for the possibility of the
        /// directory not existing at all.
        /// </summary>
        /// <param name="directory">Path to the directory.</param>
        /// <returns>Whether it's an unpacked res folder.</returns>
        private static bool EnsureUnpackedClient(string directory)
        {
            return !string.IsNullOrWhiteSpace(directory) &&
                   directory.EndsWith("res") &&
                   Directory.Exists(directory) &&
                   Directory.GetFiles(directory, "*.luz", SearchOption.AllDirectories).Any();
        }
        
        /// <summary>
        /// Requests confirmation if Nexus LU Launcher is not set up correctly.
        /// </summary>
        /// <param name="message">Message for the user to confirm.</param>
        /// <returns>Whether the user wants to abort for the client not existing.</returns>
        private static bool RequestConfirmation(string message)
        {
            // Display the message.
            Console.WriteLine(message);
            Console.Write("\nDo you want to continue? A manual configuration will be required to run Uchu. (y/N): ");
                    
            // Wait for a message and return based on the response.
            var response = Console.ReadLine();
            return response == null || !response.ToLower().StartsWith("y");
        }
        
        /// <summary>
        /// Checks for Nexus LU Launcher to have run. As of Uchu Pull Request #296,
        /// Nexus LU Launcher is natively supported.
        /// The code is mostly pulled from Uchu's Nexus LU Launcher detection code, which
        /// follows the file specification as of V.0.3.0.
        /// </summary>
        /// <returns>Whether the user wants to abort for the client not existing.</returns>
        public bool CheckForClient()
        {
            // Get .nlul location (~/.nlul) and request confirmation if it doesn't exist.
            var nlulHomeLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nlul");
            if (!Directory.Exists(nlulHomeLocation))
                return RequestConfirmation(NotRanMessage);

            // Get path to NLUL config file and request confirmation if it doesn't exist.
            // Even with older releases, (V.0.2.0 and older), the file shoudl exist.
            var launcherFileLocation = Path.Combine(nlulHomeLocation, "launcher.json");
            if (!File.Exists(launcherFileLocation))
                return RequestConfirmation(NotRanMessage);

            // Parse NLUL config file
            var nlulConfig = JsonDocument.Parse(File.ReadAllText(launcherFileLocation));

            // Client parent directory is ClientParentLocation if set, otherwise nlulHomeLocation
            // V.0.2.0 did not use this field, so it is not guaranteed to be populated.
            var clientParentLocation = nlulHomeLocation;
            if (nlulConfig.RootElement.TryGetProperty("ClientParentLocation", out var customClientParentLocation))
                clientParentLocation = customClientParentLocation.GetString();
            if (!Directory.Exists(clientParentLocation))
                return RequestConfirmation(NotRanMessage);

            // Iterate over subdirectories to search for a valid client. Return false if one exists.
            Debug.Assert(clientParentLocation != null, nameof(clientParentLocation) + " != null");
            foreach (var clientDirectory in Directory.GetDirectories(clientParentLocation))
            {
                // Check the res directory and ignore it if it isn't unpacked.
                var resLocation = Path.Combine(clientDirectory, "res");
                if (!EnsureUnpackedClient(resLocation))
                    continue;
                
                // Output that an unpacked client was found and return false (not aborting).
                Console.WriteLine("Nexus LU Launcher with an unpacked client detected.");
                return false;
            }
            
            // Confirm that no unpacked client exists.
            return RequestConfirmation(NoUnpackedClientMessage);
        }
    }
}