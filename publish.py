"""
TheNexusAvenger

Creates the binaries for distribution.
Taken from Nexus LU Launcher's publishing with some modifications.
"""

PROJECTS = [
    "Uchu.Tool",
]
PLATFORMS = [
    ["Windows-x64", "win-x64"],
    ["macOS-x64", "osx-x64"],
    ["Linux-x64", "linux-x64"],
]
EXTRA_PARAMETERS = {
    "win-x64": ["/p:IncludeNativeLibrariesForSelfExtract=true"],
}

import os
import shutil
import subprocess
import sys

"""
Clears a publish directory of unwanted files.
"""
def cleanDirectory(directory):
    for file in os.listdir(directory):
        if file.endswith(".pdb"):
            os.remove(directory + "/" + file)


# Display a warning for Windows runs.
if os.name == "nt":
    sys.stderr.write("Windows was detected. Linux and macOS binaries will be missing the permissions to run.\n")
else:
    sys.stderr.write("Windows was not detected. Windows binaries will not have an icon with the executable.\n")

# Create the directory.
if os.path.exists("bin"):
    shutil.rmtree("bin")
os.mkdir("bin")

# Compile the releases.
for project in PROJECTS:
    for platform in PLATFORMS:
        # Compile the project for the platform.
        print("Exporting " + project + " for " + platform[0])
        buildParameters = ["dotnet", "publish", "-r", platform[1], "-c", "Release", project + "/" + project + ".csproj"]
        if platform[1] in EXTRA_PARAMETERS.keys():
            for entry in EXTRA_PARAMETERS[platform[1]]:
                buildParameters.append(entry)
        subprocess.call(buildParameters, stdout=open(os.devnull, "w"))

        # Clear the unwanted files of the compile.
        dotNetVersion = os.listdir(project + "/bin/Release/")[0]
        outputDirectory = project + "/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish"
        for file in os.listdir(outputDirectory):
            if file.endswith(".pdb"):
                os.remove(outputDirectory + "/" + file)

        # Create the archive.
        shutil.make_archive("bin/" + project + "-" + platform[0], "zip", project + "/bin/Release/" + dotNetVersion + "/" + platform[1] + "/publish")

# Clear the existing macOS GUI release.
if os.path.exists("bin/Uchu.Tool-macOS-x64.zip"):
    print("Clearing the macOS x64 Uchu.Tool release.")
    os.remove("bin/Uchu.Tool-macOS-x64.zip")

# Package the macOS release.
print("Packaging macOS release.")
dotNetVersion = os.listdir("Uchu.Tool/bin/Release/")[0]
shutil.copytree("Uchu.Tool/bin/Release/" + dotNetVersion + "/osx-x64/publish", "bin/Uchu.Tool-macOS-x64/Uchu Tool.app/Contents/MacOS")
os.mkdir("bin/Uchu.Tool-macOS-x64/Uchu Tool.app/Contents/Resources")
shutil.copy("packaging/macOS/UchuLogo.icns", "bin/Uchu.Tool-macOS-x64/Uchu Tool.app/Contents/Resources/UchuLogo.icns")
shutil.copy("packaging/macOS/StartUchuTool", "bin/Uchu.Tool-macOS-x64/Uchu Tool.app/Contents/MacOS/StartUchuTool")
if os.name != "nt":
    subprocess.call(["chmod", "+x", "bin/Uchu.Tool-macOS-x64/Uchu Tool.app/Contents/MacOS/StartUchuTool"])
shutil.copy("packaging/macOS/Info.plist", "bin/Uchu.Tool-macOS-x64/Uchu Tool.app/Contents/Info.plist")
shutil.make_archive("bin/Uchu.Tool-macOS-x64", "zip", "bin/Uchu.Tool-macOS-x64")
shutil.rmtree("bin/Uchu.Tool-macOS-x64")
