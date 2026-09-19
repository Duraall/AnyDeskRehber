# AnyDesk Rehber

A lightweight Windows desktop address book for organizing AnyDesk computers and starting connections quickly. Built with C#, .NET 8, WinUI 3, and the Windows App SDK.

> [!NOTE]
> This is an independent community project. It is not developed, sponsored, or endorsed by AnyDesk Software GmbH. AnyDesk and related trademarks belong to their respective owners.

## Features

- Save computers with a name, AnyDesk ID/alias, group, and optional note
- Start an AnyDesk connection directly from a computer card
- Edit and delete saved computers with confirmation
- Search saved computers
- Mark computers as favorites
- Create persistent groups that remain available even when empty
- Rename or delete groups from the group context menu
- Deleting a group does not delete its computers; affected computers become ungrouped
- Select an existing group while adding or editing a computer
- Copy an AnyDesk ID with a minimal notification
- Light and dark themes
- Manual status values: Unknown, Online, and Offline
- Prevent duplicate AnyDesk IDs/aliases
- Safe JSON writes with automatic backup and recovery

## Requirements

### To run the installed application

- Windows 10 version 1809 (build 17763) or later
- AnyDesk installed if you want to launch remote connections from the application

The x64 installer is built as a self-contained application, so end users do not need to install .NET, Visual Studio, Python, or development tools.

### To build from source

- Windows
- Visual Studio with WinUI 3 / Windows App SDK development tools
- .NET 8 SDK
- Windows SDK
- Inno Setup 6, only if you want to build the installer

## Build and Run from Source

1. Clone the repository:

```powershell
git clone https://github.com/Duraall/AnyDeskRehber.git
cd AnyDeskRehber
```

2. Open `AnyDeskRehber.WinUI3.sln` in Visual Studio.
3. Select the `x64` platform.
4. Select `Debug` for development or `Release` for a release build.
5. Press **F5** to build and run the application.

## Build from the Command Line

To create a self-contained x64 publish:

```powershell
dotnet publish ".\AnyDeskRehber.WinUI3.csproj" `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:Platform=x64
```

The published files are generated under:

```text
bin\publish\win-x64\
```

## Build the Installer

The repository contains the Inno Setup definition at:

```text
Installer\AnyDeskRehber.iss
```

After publishing the application, compile the installer with Inno Setup 6:

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" ".\Installer\AnyDeskRehber.iss"
```

The generated installer is:

```text
Installer\Output\AnyDeskRehber_Setup.exe
```

A helper script is also available at `Installer\Build-Installer.ps1`.

## Data Storage

Application data is stored per Windows user under:

```text
%APPDATA%\AnyDeskRehber\
```

Main files:

```text
address_book.json
address_book.backup.json
groups.json
```

The application writes contact data through a temporary file before replacing the live JSON file. The previous address book is kept as `address_book.backup.json`, and the backup is used as a recovery source if the main JSON file cannot be loaded.

User data and generated build output are not included in the Git repository.

## Groups

Groups are stored independently from contacts. This means an empty group remains available after its last computer is deleted.

A group can be renamed or deleted from its context menu. Deleting a group does **not** delete the computers assigned to it; those computers become ungrouped.

Older groups stored only through contact records are migrated into the independent group list.

## Computer Status

The application uses the following manual status values:

- **Unknown**
- **Online**
- **Offline**

The application does not claim to automatically detect whether an arbitrary remote AnyDesk address is online. Reliable automatic remote-status detection would require an appropriate external management/API integration.

## Duplicate ID Protection

The same AnyDesk ID or alias cannot be added twice. For numeric IDs, spaces and hyphens are normalized when checking for duplicates.

## AnyDesk Integration

When a connection is requested, the application searches for AnyDesk in the standard installation locations:

```text
C:\Program Files (x86)\AnyDesk\AnyDesk.exe
C:\Program Files\AnyDesk\AnyDesk.exe
```

If AnyDesk is available, the saved ID/alias is passed to AnyDesk to start the connection.

## Project Structure

```text
AnyDeskRehber.WinUI3.sln
AnyDeskRehber.WinUI3.csproj
App.xaml
App.xaml.cs
MainWindow.xaml
MainWindow.xaml.cs
Models/
Services/
Properties/
Installer/
app.manifest
```

Build directories such as `bin/`, `obj/`, `.vs/`, installer output, and generated executable files are excluded through `.gitignore`.

## License

This project is licensed under the GNU General Public License v3.0. See the `LICENSE` file for license information.
