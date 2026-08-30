@echo off

set "BATDIR=%~dp0"

set PROG=%1
shift
if "%PROG%"=="" goto :BADARGS

set RUNARGS=
:NEXTARG
    if "%~1"=="" goto :ENDARGS
    set "RUNARGS=%RUNARGS% %1"
    shift
    goto :NEXTARG
:ENDARGS

if "%PROG%"=="console"      call "%BATDIR%build.bat" console      -nobuild -run %RUNARGS%
if "%PROG%"=="windows"      call "%BATDIR%build.bat" windows      -nobuild -run %RUNARGS%
if "%PROG%"=="convert-font" call "%BATDIR%build.bat" convert-font -nobuild -run %RUNARGS%
if "%PROG%"=="extract-font" call "%BATDIR%build.bat" extract-font -nobuild -run %RUNARGS%
if "%PROG%"=="ambient"      call "%BATDIR%build.bat" ambient      -nobuild -run %RUNARGS%
if "%PROG%"=="cdulamps"     call "%BATDIR%build.bat" cdulamps     -nobuild -run %RUNARGS%
if "%PROG%"=="characters"   call "%BATDIR%build.bat" characters   -nobuild -run %RUNARGS%
if "%PROG%"=="clock"        call "%BATDIR%build.bat" clock        -nobuild -run %RUNARGS%
if "%PROG%"=="colours"      call "%BATDIR%build.bat" colours      -nobuild -run %RUNARGS%
if "%PROG%"=="cooked-input" call "%BATDIR%build.bat" cooked-input -nobuild -run %RUNARGS%
if "%PROG%"=="fast-update"  call "%BATDIR%build.bat" fast-update  -nobuild -run %RUNARGS%
if "%PROG%"=="fenix-mcdu"   call "%BATDIR%build.bat" fenix-mcdu   -nobuild -run %RUNARGS%
if "%PROG%"=="fgcp-test"    call "%BATDIR%build.bat" fgcp-test    -nobuild -run %RUNARGS%

goto :EOF

:BADARGS
echo Usage: run [program] (args to program)
echo console       Run cduhub-cli
echo windows       Run cduhub-windows
echo.
echo convert-font  Run the font converter
echo extract-font  Run the extract-font utility
echo.
echo ambient       Run the ambient mcdu-dotnet sample
echo cdulamps      Run the cdulamps mcdu-dotnet sample
echo characters    Run the characters mcdu-dotnet sample
echo clock         Run the clock mcdu-dotnet sample
echo colours       Run the colours mcdu-dotnet sample
echo cooked-input  Run the cooked-input mcdu-dotnet sample
echo fast-update   Run the fast-update mcdu-dotnet sample
echo fenix-mcdu    Run the fenix-mcdu mcdu-dotnet sample
echo fgcp-test     Run the fgcp-test mcdu-dotnet sample
