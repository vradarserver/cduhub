#!/bin/bash
SHDIR="$(cd "$(dirname "$0")" && pwd)"

SHOW_USAGE() {
    echo "Usage: build.sh command options"
    echo "restore       Restore all NuGet packages"
    echo "solution      Build the solution (excluding Windows-only projects)"
    echo "console       Build cduhub-cli"
    echo
    echo "convert-font  Convert font resources to MCDU-DOTNET font files"
    echo "extract-font  Build the extract-font utility"
    echo
    echo "ambient       Build the ambient mcdu-dotnet sample"
    echo "cdulamps      Build the cdulamps mcdu-dotnet sample"
    echo "characters    Build the characters mcdu-dotnet sample"
    echo "clock         Build the clock mcdu-dotnet sample"
    echo "colours       Build the colours mcdu-dotnet sample"
    echo "cooked-input  Build the cooked-input mcdu-dotnet sample"
    echo "fast-update   Build the fast-update mcdu-dotnet sample"
    echo "fenix-mcdu    Build the fenix-mcdu mcdu-dotnet sample"
    echo "fgcp-test     Build the fgcp-test mcdu-dotnet sample"
    echo "inproc-plugin Build the in-process plugin sample"
    echo
    echo "--debug       Use Debug configuration (default)"
    echo "--nobuild     Skip the build phase"
    echo "--release     Use Release configuration"
    echo "--run         Run the target after compilation"
}

BUILD_DOTNET() {
    if [ "$BUILD" = "YES" ]; then
        echo
        echo dotnet build "$1" --configuration $CONFIG /p:"SolutionDir=$SHDIR"
             dotnet build "$1" --configuration $CONFIG /p:"SolutionDir=$SHDIR"
        if [ $? -ne 0 ]; then
            exit 1
        fi
    fi
}

RUN_DOTNET() {
    if [ "$RUN" = "YES" ]; then
        #echo
        #echo dotnet run --project "$1" --no-build --configuration $CONFIG -- ${RUNARGS[@]}
             dotnet run --project "$1" --no-build --configuration $CONFIG -- ${RUNARGS[@]}
        exit $?
    fi
}

RUN_PROGRAM() {
    if [ "$RUN" = "YES" ]; then
        $1 ${RUNARGS[@]}
        exit $?
    fi
}

TARGET=""
CONFIG=NoWinDebug
BUILD=YES
RUN=NO
declare -a RUNARGS=()

for arg in "$@"
do
    if [ "$RUN" = "YES" ]; then
        RUNARGS+=("$arg")
    else
        case $arg in
            solution | restore | ambient | cdulamps | characters | clock | colours | console | convert-font | cooked-input | extract-font | fast-update | fenix-mcdu | fgcp-test | inproc-plugin)
                TARGET="$arg"
                ;;
            -run | --run)
                RUN=YES
                ;;
            -debug | --debug)
                CONFIG=NoWinDebug
                ;;
            -release | --release)
                CONFIG=NoWinRelease
                ;;
            -nobuild | --nobuild | --no-build)
                BUILD=NO
                ;;
            *)
                SHOW_USAGE
                exit 1
                ;;
        esac
    fi
done

case $TARGET in
    ambient)
        BUILD_DOTNET "$SHDIR/library/samples/ambient/ambient.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/ambient/ambient.csproj"
        ;;
    cdulamps)
        BUILD_DOTNET "$SHDIR/library/samples/cdulamps/cdulamps.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/cdulamps/cdulamps.csproj"
        ;;
    characters)
        BUILD_DOTNET "$SHDIR/library/samples/characters/characters.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/characters/characters.csproj"
        ;;
    clock)
        BUILD_DOTNET "$SHDIR/library/samples/clock/clock.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/clock/clock.csproj"
        ;;
    colours)
        BUILD_DOTNET "$SHDIR/library/samples/colours/colours.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/colours/colours.csproj"
        ;;
    console)
        BUILD_DOTNET "$SHDIR/apps/cduhub-cli/cduhub-cli.csproj"
        RUN_DOTNET   "$SHDIR/apps/cduhub-cli/cduhub-cli.csproj"
        ;;
    convert-font)
        BUILD_DOTNET "$SHDIR/utilities/convert-font/convert-font.csproj"
        RUN_DOTNET   "$SHDIR/utilities/convert-font/convert-font.csproj"
        ;;
    cooked-input)
        BUILD_DOTNET "$SHDIR/library/samples/cooked-input/cooked-input.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/cooked-input/cooked-input.csproj"
        ;;
    extract-font)
        BUILD_DOTNET "$SHDIR/utilities/extract-font/extract-font.csproj"
        RUN_DOTNET   "$SHDIR/utilities/extract-font/extract-font.csproj"
        ;;
    fast-update)
        BUILD_DOTNET "$SHDIR/library/samples/fast-update/fast-update.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/fast-update/fast-update.csproj"
        ;;
    fenix-mcdu)
        BUILD_DOTNET "$SHDIR/library/samples/fenix-mcdu/fenix-mcdu.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/fenix-mcdu/fenix-mcdu.csproj"
        ;;
    fgcp-test)
        BUILD_DOTNET "$SHDIR/library/samples/fgcp-test/fgcp-test.csproj"
        RUN_DOTNET   "$SHDIR/library/samples/fgcp-test/fgcp-test.csproj"
        ;;
    inproc-plugin)
        BUILD_DOTNET "$SHDIR/library/samples/inprocess-plugin/inprocess-plugin.csproj"
        RUN_DOTNET   "$SHDIR/apps/cduhub-cli/cduhub-cli.csproj"
        ;;
    restore)
        dotnet restore "$SHDIR/cduhub.sln"
        ;;
    solution)
        BUILD_DOTNET "$SHDIR/cduhub.sln"
        RUNARGS="The run option makes no sense for the solution, ignoring it"
        RUN_PROGRAM "echo"
        ;;
    *)
        SHOW_USAGE
        exit 1
        ;;
esac
