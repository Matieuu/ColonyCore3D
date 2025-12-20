#!/bin/bash

set -e

BUILD_PROFILE="debug"
CARGO_FLAGS=""
DOTNET_CONF="Debug"

if [[ -n "$1" && ("$1" == "--release" || "$1" == "-r") ]]; then
    BUILD_PROFILE="release"
    CARGO_FLAGS="--release"
    DOTNET_CONF="Release"
fi

BRAIN_DIR="./brain"
MAIN_DIR="./main"
RUST_OUT="$BRAIN_DIR/target/$BUILD_PROFILE"
DOTNET_OUT="$MAIN_DIR/bin/$DOTNET_CONF/net10.0"

(cd "$BRAIN_DIR" && cargo build $CARGO_FLAGS)

mkdir -p "$DOTNET_OUT"
mv "$RUST_OUT"/lib*.so "$DOTNET_OUT"/ 2>/dev/null || true
# mv "$RUST_OUT"/*.dll   "$DOTNET_OUT"/ 2>/dev/null || true

(
    cd "$MAIN_DIR"
    dotnet build --configuration "$DOTNET_CONF"
)

(
    cd "$DOTNET_OUT"
    ./main
)
