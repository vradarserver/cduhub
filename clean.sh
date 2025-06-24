#!/bin/bash
SHDIR="$(cd "$(dirname "$0")" && pwd)"

find "$SHDIR" -type d \( -name "bin" -o -name "obj" -o -name "publish" \) -exec rm -r {} +

