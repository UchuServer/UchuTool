using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using Uchu.Tool.Action;

namespace Uchu.Tool
{
    class Program
    {
        /// <summary>
        /// Runs the application.
        /// </summary>
        /// <param name="args">Arguments from the command line.</param>
        public static int Main(string[] args)
        {
            // Create the root command for parsing arguments.
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    "--directory",
                    () => "uchu",
                    "Directory to download and run Uchu from."),
                new Option(
                    "--update",
                    "If an update is available, it will be downloaded with no confirmation."),
                new Option(
                    "--no-update",
                    "Ignores checking for updates."),
                new Option(
                    "--no-run",
                    "Ignores running Uchu. Intended to be used when only updating the release."),
            };
            rootCommand.Name = "Uchu Tool";
            rootCommand.Description = "Tool for assisting with downloading Uchu releases and running them.";

            // Set up the handler.
            rootCommand.Handler = CommandHandler.Create<string, bool, bool, bool>((directory, update, noUpdate, noRun) =>
            {
                // Check for Nexus LU Launcher.
                // If any update option flag is specified, this is not checked.
                if (!update && !noUpdate)
                {
                    // Return if the check failed and the user aborted.
                    if (new CheckNexusLULauncher().CheckForClient())
                    {
                        return -1;
                    }
                }
                
                // Check the release.
                var updateAction = new Update(directory);
                if (!noUpdate || !File.Exists(updateAction.CurrentReleaseFile))
                {
                    Console.WriteLine("Checking the latest release.");
                    updateAction.CheckLatestRelease(update);
                }
                
                // Run the Uchu server.
                var runAction = new Run(directory);
                if (!noRun)
                {
                    Console.WriteLine("Starting Uchu.");
                    runAction.StartUchu();
                }
                
                // Return 0 (success).
                return 0;
            });

            // Invoke the command and return the response code.
            return rootCommand.InvokeAsync(args).Result;
        }
    }
}