#!/bin/bash
SHDIR="$(cd "$(dirname "$0")" && pwd)"
RUNARGS=${@:2}

SHOW_USAGE() {
    echo "Usage: run [program] (args to program)"
    echo "console       Build cduhub-cli"
    echo
    echo "convert-font  Run the font converter"
    echo "extract-font  Run the extract-font utility"
    echo
    echo "ambient       Run the ambient mcdu-dotnet sample"
    echo "cdulamps      Run the cdulamps mcdu-dotnet sample"
    echo "characters    Run the characters mcdu-dotnet sample"
    echo "clock         Run the clock mcdu-dotnet sample"
    echo "colours       Run the colours mcdu-dotnet sample"
    echo "cooked-input  Run the cooked-input mcdu-dotnet sample"
    echo "fast-update   Run the fast-update mcdu-dotnet sample"
    echo "fenix-mcdu    Run the fenix-mcdu mcdu-dotnet sample"
    echo "fgcp-test     Run the fgcp-test mcdu-dotnet sample"
}

RUN_BUILD() {
    "$SHDIR/build.sh" "$1" --nobuild --run ${RUNARGS[@]}
    exit $?
}

if [ "$#" -eq 0 ]; then
    SHOW_USAGE
    exit 1
fi

case $1 in
    ambient)
        RUN_BUILD ambient
        ;;
    cdulamps)
        RUN_BUILD cdulamps
        ;;
    characters)
        RUN_BUILD characters
        ;;
    clock)
        RUN_BUILD clock
        ;;
    colours)
        RUN_BUILD colours
        ;;
    console)
        RUN_BUILD console
        ;;
    convert-font)
        RUN_BUILD convert-font
        ;;
    cooked-input)
        RUN_BUILD cooked-input
        ;;
    extract-font)
        RUN_BUILD extract-font
        ;;
    fast-update)
        RUN_BUILD fast-update
        ;;
    fenix-mcdu)
        RUN_BUILD fenix-mcdu
        ;;
    fgcp-test)
        RUN_BUILD fgcp-test
        ;;
    *)
        SHOW_USAGE
        exit 1
        ;;
esac
