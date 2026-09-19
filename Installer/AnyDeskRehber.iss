#define MyAppName "AnyDesk Rehber"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "AnyDesk Rehber"
#define MyAppExeName "AnyDeskRehber.WinUI3.exe"

[Setup]
AppId={{E27BB16A-09A1-4DF0-AF16-31D573DCFA64}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}

DefaultDirName={autopf}\AnyDesk Rehber
DefaultGroupName=AnyDesk Rehber

OutputDir=Output
OutputBaseFilename=AnyDeskRehber_Setup

Compression=lzma2
SolidCompression=yes

ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

PrivilegesRequired=admin
WizardStyle=modern

UninstallDisplayName=AnyDesk Rehber
UninstallDisplayIcon={app}\{#MyAppExeName}

SetupLogging=yes
CloseApplications=yes

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: "desktopicon"; Description: "Masaüstü kısayolu oluştur"; GroupDescription: "Ek seçenekler:"; Flags: unchecked

[Files]
Source: "..\bin\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AnyDesk Rehber"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\AnyDesk Rehber"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "AnyDesk Rehber'i çalıştır"; Flags: nowait postinstall skipifsilent