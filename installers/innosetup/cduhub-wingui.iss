#define public Root       "..\.."
#define public Publish    "..\..\publish\cduhub-wingui"
#ifndef VERSION
  #define public VERSION    "v0.0.0-alpha-0"
#endif

[Setup]
AppName=CDU Hub
AppVerName=CDU Hub {#VERSION}
DefaultDirName={autopf}\cduhub
DefaultGroupName=CduHub
DisableDirPage=no
LicenseFile={#Root}\LICENSE
MinVersion=6.3
OutputBaseFileName=cduhub-wingui-{#VERSION}
SetupIconFile={#Root}\images\win32.ico
UninstallDisplayIcon={app}\cduhub-wingui.exe

[Files]
Source: "{#Root}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion;
Source: "{#Publish}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs;

[Icons]
Name: "{group}\CDU Hub"; Filename: "{app}\cduhub-wingui.exe"; WorkingDir: "{app}"
