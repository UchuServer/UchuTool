using System;
using System.Diagnostics;
using System.IO;

namespace Uchu.Tool.Action
{
    public class Run : BaseAction
    {
        /// <summary>
        /// Location of the executable.
        /// </summary>
        public string UchuExecutable => Path.Combine(this.UchuDirectory, File.Exists(Path.Combine(this.UchuDirectory, "Uchu.Master.exe")) ? "Uchu.Master.exe" : "Uchu.Master");
        
        /// <summary>
        /// Creates the Run action.
        /// </summary>
        /// <param name="uchuDirectory">Directory to download and run Uchu.</param>
        public Run(string uchuDirectory) : base(uchuDirectory)
        {
            
        }

        /// <summary>
        /// Starts Uchu.
        /// </summary>
        public void StartUchu()
        {
            // Return if Uchu isn't downloaded.
            if (!File.Exists(this.UchuExecutable))
            {
                Console.WriteLine("Uchu executable not found. Unable to start.");
                return;
            }
            
            // Start the process and wait for it to exist.
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = this.UchuExecutable,
                    WorkingDirectory = this.UchuDirectory,
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}