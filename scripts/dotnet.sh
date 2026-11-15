#!/usr/bin/env bash
set -euo pipefail
calistrack_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
calistrack_tools="$(dirname "$calistrack_root")/.calistrack-tools"
if [[ -x "$calistrack_tools/dotnet/dotnet" ]]; then
    export DOTNET_ROOT="$calistrack_tools/dotnet"
    export DOTNET_CLI_HOME="$calistrack_tools"
    export NUGET_PACKAGES="$calistrack_tools/nuget"
    export PATH="$DOTNET_ROOT:$PATH"
    export DOTNET_CLI_TELEMETRY_OPTOUT=1
fi
cd "$calistrack_root"
exec dotnet "$@"
