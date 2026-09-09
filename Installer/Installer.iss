; Inno Setup script for WorkoutGenSD
; Supports two build modes:
; - Minimal (downloads .NET 4.8 at install time if needed)
; - Bundled (include the .NET 4.8 offline installer in the EXE)
;
; Build commands (requires Inno Setup ISCC.exe on PATH or specify full path):
;  iscc Installer.iss         -> produces setup_minimal.exe
;  iscc /DBUNDLE Installer.iss -> produces setup_bundled.exe (bundles Prereqs\NDP48-offline.exe)
;
; Notes:
; - Place the .NET 4.8 offline redistributable in Installer\Prereqs as NDP48-offline.exe if building bundled.
;   Official Microsoft fwlink for .NET 4.8 offline redistributable is used as the default download URL.
; - The script checks the registry Release value (>= 528040) to determine presence of .NET 4.8.
; - The installer will run the .NET installer silently when needed (/q /norestart).

#define AppName "WorkoutGenSD"
#define AppVersion "0.2026.0908.0"
#ifdef BUNDLE
#define OutputBase "setup_bundled"
#else
#define OutputBase "setup_minimal"
#endif

#define DotNetFileName "NDP48-offline.exe"
#define DotNetFwLink "https://go.microsoft.com/fwlink/?linkid=2088631"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={commonpf}\{#AppName}
DefaultGroupName={#AppName}
UninstallDisplayIcon={app}\WorkoutGenSD.exe
SetupIconFile=..\WorkoutGenSD\icon.ico
Compression=lzma2
SolidCompression=yes
OutputBaseFilename={#OutputBase}
OutputDir=.

[Files]
; Application files - expects build output in ..\WorkoutGenSD\bin\Release\
Source: "..\WorkoutGenSD\bin\Release\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

#ifdef BUNDLE
Source: "Prereqs\{#DotNetFileName}"; DestDir: "{tmp}"; Flags: deleteafterinstall
#endif

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; Flags: unchecked

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\WorkoutGenSD.exe"
Name: "{commondesktop}\{#AppName}"; Filename: "{app}\WorkoutGenSD.exe"; Tasks: desktopicon

[Run]
; Run .NET offline installer if needed (file will either be bundled to {tmp} or downloaded at install time)
Filename: "{tmp}\{#DotNetFileName}"; Parameters: "/q /norestart"; Flags: waituntilterminated; Check: NeedsDotNetInstall and FileExists(ExpandConstant('{tmp}\{#DotNetFileName}'))
; Launch application after install
Filename: "{app}\WorkoutGenSD.exe"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[Code]
const
  DotNetKey = 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full';
  DotNetRelease48 = 528040; // .NET Framework 4.8 release key

function URLDownloadToFileA(Caller: LongWord; URL: string; FileName: string; Reserved: LongWord; StatusCallback: LongWord): LongWord;
  external 'URLDownloadToFileA@urlmon.dll stdcall';

function DownloadFile(const Url, DestFile: string): Boolean;
var
  res: LongWord;
begin
  res := URLDownloadToFileA(0, Url, DestFile, 0, 0);
  Result := (res = 0);
end;

function IsDotNet48Installed(): Boolean;
var
  release: Cardinal;
begin
  // Try 64-bit view first, then 32-bit view
  if RegQueryDWordValue(HKLM64, DotNetKey, 'Release', release) then
	Result := release >= DotNetRelease48
  else if RegQueryDWordValue(HKLM, DotNetKey, 'Release', release) then
	Result := release >= DotNetRelease48
  else
	Result := False;
end;

function NeedsDotNetInstall(): Boolean;
begin
  Result := not IsDotNet48Installed();
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  DotNetPath: string;
#ifndef BUNDLE
  Url: string;
#endif
begin
  if CurStep = ssInstall then
  begin
	if NeedsDotNetInstall() then
	begin
	  DotNetPath := ExpandConstant('{tmp}\{#DotNetFileName}');
#ifndef BUNDLE
	  Url := '{#DotNetFwLink}';
	  // Attempt download if not bundled
	  if not FileExists(DotNetPath) then
	  begin
		MsgBox('Downloading .NET Framework 4.8. This may take a few minutes...', mbInformation, MB_OK);
		if not DownloadFile(Url, DotNetPath) then
		begin
		  MsgBox('Failed to download .NET Framework 4.8. Please download and install it manually, then re-run this installer.', mbError, MB_OK);
		end;
	  end;
#endif
	end;
  end;
end;
