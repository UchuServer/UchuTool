# Uchu Tool [![discord](https://img.shields.io/discord/762298384979329114?label=Discord&logo=discord&logoColor=white)](https://discord.gg/mrhBXVVNBD)
Helper tool for downloading and running releases of [Uchu](https://github.com/uchuserver/uchu).
> The LEGO Group has not endorsed or authorized the operation of this game and is not liable
  for any safety issues in relation to its operation.

## Running
### Download and Run [Nexus LU Launcher](https://github.com/TheNexusAvenger/Nexus-LU-Launcher)
Uchu requires an *unpacked* LEGO Universe client distribution to work. Nexus LU Launcher
([download](https://github.com/TheNexusAvenger/Nexus-LU-Launcher/releases/latest)) is supported
by Uchu. Running Nexus LU Launcher and running the initial download will allow Uchu to automatically
configure the client.

### Download and Run Uchu Tool
Uchu Tool downloads can be found in the releases tab on GitHub. When run, it will automatically
fetch the latest release of Uchu and start it. Unless Nexus LU Launcher was not used, not
configuration changes will be required.

### Add Your User
When you've got your server up and running, it's time to create a user account. If you're on Windows,
find the window titled Authentication. On Linux/macOS, you just need the one window in which the
server is running. Type `/adduser <username>` and press enter to create a user (don't include the
`<>`). Uchu will prompt you for a password. You can set your permissions using `/gamemaster <username> <level>`.
The highest level available is **9**.

After this, you are ready to open LEGO Universe and play!

## Command Line Arguments
For power users, Uchu Tool offers a few extra command line arguments.

### --directory
```bash
uchutool --directory location
```

Changes the directory to download and run Uchu from.

### --update
```bash
uchutool --update
```

When a new release of Uchu is made, the tool will prompt you when ran to update. If `--update` is
applied, it will automatically confirm updates and apply the update. There also disables the check
for Nexus LU Launcher on start.

### --no-update
```bash
uchutool --update
```

`--no-update` disables update checks, except for the first time the tool is run.

### --no-run
```bash
uchutool --no-run
```

`--no-run` does not start Uchu when run. This is intended for updating the release but not running.