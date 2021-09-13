using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Uchu.Tool.Action
{
    public class GithubReleaseAsset
    {
        /// <summary>
        /// Name of the release.
        /// </summary>
        public string name { get; set; }
        
        /// <summary>
        /// Download URL of the asset.
        /// </summary>
        public string browser_download_url { get; set; }
    }
    
    public class GithubRelease
    {
        /// <summary>
        /// Name of the release.
        /// </summary>
        public string name { get; set; }
        
        /// <summary>
        /// Unique tag name of the release.
        /// </summary>
        public string tag_name { get; set; }
        
        /// <summary>
        /// Assets of the releases.
        /// </summary>
        public List<GithubReleaseAsset> assets { get; set; }
    }

    public class CurrentReleaseData
    {
        /// <summary>
        /// Name of the current release.
        /// </summary>
        public string ReleaseDisplayName { get; set; }
        
        /// <summary>
        /// Unique tag name of the current release.
        /// </summary>
        public string ReleaseTagName { get; set; }
    }
    
    public class Update : BaseAction
    {
        /// <summary>
        /// Dictionary of the OS platform to the name to look for in the releases.
        /// </summary>
        public static readonly Dictionary<OSPlatform, string> PlatformsToName = new Dictionary<OSPlatform, string>()
        {
            { OSPlatform.Windows, "Windows" },
            { OSPlatform.OSX, "macOS" },
            { OSPlatform.Linux, "Linux" },
        };
        
        /// <summary>
        /// File where the current release is stored.
        /// </summary>
        public string CurrentReleaseFile => Path.Combine(this.UchuDirectory, "CurrentRelease.json");
        
        /// <summary>
        /// Path to download the ZIP archive of the server.
        /// </summary>
        public string DownloadLocation => Path.Combine(this.UchuDirectory, "download.zip");
        
        /// <summary>
        /// Path to extract the Uchu release to.
        /// </summary>
        public string ExtractLocation => Path.Combine(this.UchuDirectory, "extract");
        
        /// <summary>
        /// Creates the Update action.
        /// </summary>
        /// <param name="uchuDirectory">Directory to download and run Uchu.</param>
        public Update(string uchuDirectory) : base(uchuDirectory)
        {
            
        }

        /// <summary>
        /// Returns the latest release of Uchu.
        /// </summary>
        private GithubRelease GetLatestRelease()
        {
            // Send the HTTP request for the releases.
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Uchu Releases Fetch");
            var response = client.GetAsync("https://api.github.com/repos/uchuserver/uchu/releases").Result;
            var stringResponse = response.Content.ReadAsStringAsync().Result;
            
            // Throw a rate limit error if found.
            if (response.ReasonPhrase == "rate limit exceeded")
            {
                throw new InvalidDataException("Rate limit from GitHub detected.");
            }
            
            // Parse the JSON response and return the first entry.
            var releases = JsonConvert.DeserializeObject<List<GithubRelease>>(stringResponse);
            return releases != null && releases.Count > 0 ? releases[0] : null;
        }

        /// <summary>
        /// Returns the current release in use.
        /// </summary>
        private CurrentReleaseData GetCurrentRelease()
        {
            return File.Exists(this.CurrentReleaseFile) ? JsonConvert.DeserializeObject<CurrentReleaseData>(File.ReadAllText(this.CurrentReleaseFile)) : null;
        }

        /// <summary>
        /// Copes the files from a source directory to a destination directory.
        /// </summary>
        /// <param name="source">Source directory to copy from.</param>
        /// <param name="destination">Target directory to copy to.</param>
        private void CopyDirectory(string source, string destination)
        {
            // Create the directory.
            if (!Directory.Exists(destination))
            {
                Directory.CreateDirectory(destination);
            }
            
            // Copy the files.
            foreach (var file in Directory.GetFiles(source))
            {
                var fileName = Path.GetFileName(file);
                var sourcePath = Path.Combine(source, fileName);
                var destinationPath = Path.Combine(destination, fileName);
                if (File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                File.Copy(sourcePath, destinationPath);
            }
            
            // Copy the directories.
            foreach (var directory in Directory.GetDirectories(source))
            {
                var directoryName = Path.GetFileName(directory);
                var sourcePath = Path.Combine(source, directoryName);
                var destinationPath = Path.Combine(destination, directoryName);
                this.CopyDirectory(sourcePath, destinationPath);
            }
        }

        /// <summary>
        /// Checks the latest release and downloads it if confirmed.
        /// </summary>
        /// <param name="autoConfirm">Whether to auto-confirm the release.</param>
        public async Task CheckLatestRelease(bool autoConfirm = false)
        {
            // Get the latest release.
            var release = GetLatestRelease();
            if (release == null)
            {
                Console.WriteLine("Unable to find a release. Unable to download.");
                return;
            }
            
            // Determine the download URL.
            var platformName = PlatformsToName.First(pair => RuntimeInformation.IsOSPlatform(pair.Key)).Value;
            var downloadAsset = release.assets.FirstOrDefault(asset => asset.name.ToLower().Contains(platformName.ToLower()));
            if (downloadAsset == null)
            {
                Console.WriteLine("Unable to find release for the current operating system: " + platformName);
                return;
            }
            
            // Determine whether to download the release.
            var currentRelease = this.GetCurrentRelease();
            if (currentRelease != null)
            {
                // Return if the release is up to date.
                if (currentRelease.ReleaseTagName == release.tag_name)
                {
                    Console.WriteLine("No new release found.");
                    return;
                }
                
                // Request confirming the release.
                if (!autoConfirm)
                {
                    // Display the message.
                    Console.WriteLine("A new release is detected.");
                    Console.WriteLine("\tCurrent release: " + currentRelease.ReleaseDisplayName + " (" + currentRelease.ReleaseTagName + ")");
                    Console.WriteLine("\tNew release: " + release.name + " (" + release.tag_name + ")\n");
                    Console.Write("Do you want to download the new release? (Y/n): ");
                    
                    // Wait for a message and return if the message starts with a no (assume yes).
                    var response = Console.ReadLine();
                    if (response != null && response.ToLower().StartsWith("n"))
                    {
                        return;
                    }
                }
            }
            
            // Download the release.
            Console.WriteLine("Downloading release " + release.name + " (" + release.tag_name + ") from " + downloadAsset.browser_download_url);
            if (!Directory.Exists(this.UchuDirectory))
            {
                Directory.CreateDirectory(this.UchuDirectory);
            }
            if (File.Exists(this.DownloadLocation))
            {
                File.Delete(this.DownloadLocation);
            }

            var tcs = new TaskCompletionSource();

            WebClient client = new WebClient();

            client.DownloadProgressChanged += (_, e) =>
            {
                // Set cursor to start of line.
                Console.CursorLeft = 0;

                // Progress bar looks like this:
                //  72% (36 / 50 MB) [#######################         ]
                var percentage = e.ProgressPercentage.ToString().PadLeft(3);
                var megabytesToReceive = ((int) (e.TotalBytesToReceive / 1e6)).ToString();
                var megabytesReceived = ((int) (e.BytesReceived / 1e6)).ToString();

                // Calculate amount of #s to write.
                var maxWidth = Console.BufferWidth - 16 - megabytesToReceive.Length * 2;
                var progressCharacters = (int) (e.ProgressPercentage / 100f * maxWidth);

                var megabytes = $"({megabytesReceived} / {megabytesToReceive} MB) {new string(' ', megabytesToReceive.Length - megabytesReceived.Length)}";
                Console.Write($"\r{percentage}% {megabytes}[{new string('#', progressCharacters)}{new string(' ', maxWidth - progressCharacters)}]");

                // Start the next Console.WriteLine on a new line
                if (percentage == "100")
                    Console.Write("\n");
            };
            client.DownloadFileCompleted += (_, _) =>
            {
                // Extract the ZIP.
                Console.WriteLine("Extracting release.");
                if (Directory.Exists(this.ExtractLocation))
                {
                    Directory.Delete(this.ExtractLocation, true);
                }
                ZipFile.ExtractToDirectory(this.DownloadLocation, this.ExtractLocation);

                // Determine the directory to copy from.
                var extractedDirectory = this.ExtractLocation;
                while (Directory.GetFiles(extractedDirectory).Length == 0 && Directory.GetDirectories(extractedDirectory).Length == 1)
                {
                    extractedDirectory = Directory.GetDirectories(extractedDirectory)[0];
                }

                // Copy the files.
                Console.WriteLine("Copying release files.");
                this.CopyDirectory(extractedDirectory, this.UchuDirectory);

                // Store the downloaded release.
                File.WriteAllText(this.CurrentReleaseFile, JsonConvert.SerializeObject(new CurrentReleaseData()
                {
                    ReleaseDisplayName = release.name,
                    ReleaseTagName = release.tag_name,
                }));

                // Clear the files.
                if (File.Exists(this.DownloadLocation))
                {
                    File.Delete(this.DownloadLocation);
                }
                if (Directory.Exists(this.ExtractLocation))
                {
                    Directory.Delete(this.ExtractLocation, true);
                }

                tcs.TrySetResult();
            };

            // Start the download.
            client.DownloadFileAsync(new Uri(downloadAsset.browser_download_url), this.DownloadLocation);

            // Return when the download process is complete.
            await tcs.Task;
        }
    }
}
