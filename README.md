# MSI App Player 5 installer source

This WPF project rebuilds the installer executable for MSI App Player 5 (source version `5.12.120.6303`). It does not build the emulator or the original self-extracting bootstrapper.

## Build on Windows

Install Visual Studio 2022 or its Build Tools with MSBuild, WPF desktop build support, and the .NET Framework 4.8 targeting pack. In a Visual Studio Developer PowerShell, run:

```powershell
.\scripts\Build-Installer.ps1 -SetupDirectory C:\Payload\Setup
```

The Setup directory must contain matching `HD-Common.dll`, `DiscordRpc.dll`, `Newtonsoft.Json.dll`, `Microsoft.WindowsAPICodePack.dll`, and `BstkTypeLib.dll` (either at its root or under `CommonInstallUtils`). Use trusted dependencies from the matching release. Build output is `bin\Release\BlueStacksInstaller.exe` and its `.exe.config`.

## Package the rebuilt executable with a matching payload

```powershell
.\scripts\Build-Installer.ps1 -SetupDirectory C:\Payload\Setup -PackagePath .\artifacts\MSI-App-Player-Setup.zip
```

Use a dedicated, fully extracted Setup directory. The script copies its entire contents, replaces the installer executable and configuration, and creates `CommonInstallUtils.zip` from the unpacked directory if an archive is not already provided. It checks for `PF.zip`, `PD.zip`, `7zr.exe`, and the matching Qt redistributable archive. Preserve the release's `64bit` marker for an x64 package, and include its native libraries, assets, configuration, and any separate Android guest image packages. The checks do not verify payload versions, signatures, or archive contents.

The output is a ZIP of the extracted Setup layout, not an MSI or a signed self-extracting installer. Extract the ZIP and test `BlueStacksInstaller.exe` in a disposable Windows VM before distributing it. Test fresh installation, upgrade, cancellation, and rollback with the matching payload.
