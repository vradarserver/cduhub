@echo off
set "BATDIR=%~dp0"

rem ##################################################
rem ## Parse arguments

:ARGS
set TARGET=""
set CONFIG=Debug
set BUILD=YES
set RUN=NO
set BADARG=""
set RUNARGS=
:NEXTARG
    if "%1"=="" goto :ENDARGS
    if "%RUN%"=="YES" goto :ADDRUN

    set BADARG=BAD
    if "%1"=="solution"      set BADARG=OK & set TARGET=SLN
    if "%1"=="console"       set BADARG=OK & set TARGET=CONSOLE
    if "%1"=="restore"       set BADARG=OK & set TARGET=RESTORE

    if "%1"=="ambient"       set BADARG=OK & set TARGET=SAMAMBI
    if "%1"=="characters"    set BADARG=OK & set TARGET=SAMCHAR
    if "%1"=="clock"         set BADARG=OK & set TARGET=SAMCLOCK
    if "%1"=="colours"       set BADARG=OK & set TARGET=SAMCOLS
    if "%1"=="cooked-input"  set BADARG=OK & set TARGET=SAMCOOKI
    if "%1"=="fast-update"   set BADARG=OK & set TARGET=SAMFSTUP
    if "%1"=="fenix-mcdu"    set BADARG=OK & set TARGET=SAMFENIX
    if "%1"=="leds"          set BADARG=OK & set TARGET=SAMLEDS

    if "%1"=="convert-font"  set BADARG=OK & set TARGET=COFONT
    if "%1"=="extract-font"  set BADARG=OK & set TARGET=EXFONT

    if "%1"=="-debug"        set BADARG=OK & set CONFIG=Debug
    if "%1"=="-nobuild"      set BADARG=OK & set BUILD=NO
    if "%1"=="-release"      set BADARG=OK & set CONFIG=Release
    if "%1"=="-run"          set BADARG=OK & set RUN=YES

    if %BADARG%==BAD goto :USAGE
    shift
    goto :NEXTARG

:ADDRUN
    set "RUNARGS=%RUNARGS% %1"
    shift
    goto :NEXTARG
    
:ENDARGS
    if "%TARGET%"=="COFONT"     goto :COFONT
    if "%TARGET%"=="CONSOLE"    goto :CONSOLE
    if "%TARGET%"=="EXFONT"     goto :EXFONT
    if "%TARGET%"=="RESTORE"    goto :RESTORE
    if "%TARGET%"=="SAMAMBI"    goto :SAMAMBI
    if "%TARGET%"=="SAMCHAR"    goto :SAMCHAR
    if "%TARGET%"=="SAMCLOCK"   goto :SAMCLOCK
    if "%TARGET%"=="SAMCOLS"    goto :SAMCOLS
    if "%TARGET%"=="SAMCOOKI"   goto :SAMCOOKI
    if "%TARGET%"=="SAMFENIX"   goto :SAMFENIX
    if "%TARGET%"=="SAMFSTUP"   goto :SAMFSTUP
    if "%TARGET%"=="SAMLEDS"    goto :SAMLEDS
    if "%TARGET%"=="SLN"        goto :SLN

:USAGE

echo Usage: build command options
echo restore      Restore all NuGet packages
echo solution     Build the solution
echo console      Build cduhub-cli
echo.
echo convert-font Convert font resources to MCDU-DOTNET font files
echo extract-font Build the extract-font utility
echo.
echo ambient      Build the ambient mcdu-dotnet sample
echo characters   Build the characters mcdu-dotnet sample
echo clock        Build the clock mcdu-dotnet sample
echo colours      Build the colours mcdu-dotnet sample
echo cooked-input Build the cooked-input mcdu-dotnet sample
echo fast-update  Build the fast-update mcdu-dotnet sample
echo fenix-mcdu   Build the fenix-mcdu mcdu-dotnet sample
echo leds         Build the leds mcdu-dotnet sample
echo.
echo -debug       Use Debug configuration (default)
echo -nobuild     Skip the build phase
echo -release     Use Release configuration
echo -run         Run the target after compilation

if %BADARG%==OK goto :EOF
echo.
echo Unknown parameter "%1"
goto :EOF

rem ##################################################
rem ## Common actions

:DOTNET
    echo.
    if %RUN%==NO goto :DNBUILD
    set "NOBUILD= "
    if %BUILD%==NO set "NOBUILD=--no-build "
    echo dotnet run %NOBUILD%-c %CONFIG% --project "%PROJ%" -- %RUNARGS%
         dotnet run %NOBUILD%-c %CONFIG% --project "%PROJ%" -- %RUNARGS%
    if ERRORLEVEL 1 goto :EOF
    exit /b 0
:DNBUILD
    echo dotnet build -c %CONFIG% "%PROJ%"
         dotnet build -c %CONFIG% "%PROJ%"
    if ERRORLEVEL 1 goto :EOF
    exit /b 0

rem ##################################################
rem ## Pseudo targets

:RESTORE
    dotnet restore "%BATDIR%cduhub.sln"
    goto :EOF

rem ##################################################
rem ## Build targets

:COFONT
    set  "PROJ=%BATDIR%utilities\convert-font\convert-font.csproj"
    call :DOTNET
    goto :EOF

:CONSOLE
    set  "PROJ=%BATDIR%apps\cduhub-cli\cduhub-cli.csproj"
    call :DOTNET
    goto :EOF

:EXFONT
    set  "PROJ=%BATDIR%utilities\extract-font\extract-font.csproj"
    call :DOTNET
    goto :EOF

:SLN
    if "%RUN%"=="YES" echo "The run option doesn't make sense for the solution, ignoring it"
    set  RUN=NO
    set  "PROJ=%BATDIR%cduhub.sln"
    call :DOTNET
    goto :EOF

:SAMAMBI
    set  "PROJ=%BATDIR%library\samples\ambient\ambient.csproj"
    call :DOTNET
    goto :EOF

:SAMCHAR
    set  "PROJ=%BATDIR%library\samples\characters\characters.csproj"
    call :DOTNET
    goto :EOF

:SAMCLOCK
    set  "PROJ=%BATDIR%library\samples\clock\clock.csproj"
    call :DOTNET
    goto :EOF

:SAMCOLS
    set  "PROJ=%BATDIR%library\samples\colours\colours.csproj"
    call :DOTNET
    goto :EOF

:SAMCOOKI
    set "PROJ=%BATDIR%library\samples\cooked-input\cooked-input.csproj"
    call :DOTNET
    goto :EOF

:SAMFENIX
    set  "PROJ=%BATDIR%library\samples\fenix-mcdu\fenix-mcdu.csproj"
    call :DOTNET
    goto :EOF

:SAMFSTUP
    set "PROJ=%BATDIR%library\samples\fast-update\fast-update.csproj"
    call :DOTNET
    goto :EOF

:SAMLEDS
    set  "PROJ=%BATDIR%library\samples\leds\leds.csproj"
    call :DOTNET
    goto :EOF
