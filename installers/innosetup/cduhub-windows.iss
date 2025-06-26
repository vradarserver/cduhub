#define public Root       "..\.."
#define public Publish    "..\..\publish\cduhub-windows"
#ifndef VERSION
  #define public VERSION    "v0.0.0-alpha-0"
#endif

[Setup]
AppName=CDU Hub
AppVerName=CDU Hub {#VERSION}
DefaultDirName={autopf}\cduhub
DefaultGroupName=CDU Hub
DisableDirPage=no
LicenseFile={#Root}\LICENSE
ArchitecturesInstallIn64BitMode=x64
MinVersion=6.3
OutputBaseFileName=cduhub-windows-{#VERSION}
SetupIconFile={#Root}\images\win32.ico
UninstallDisplayIcon={app}\cduhub-windows.exe

[Files]
Source: "{#Root}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Publish}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs;

[Icons]
Name: "{group}\CDU Hub"; Filename: "{app}\cduhub-windows.exe"; WorkingDir: "{app}"
