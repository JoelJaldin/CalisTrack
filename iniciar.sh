#!/usr/bin/env bash
set -euo pipefail
calistrack_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
exec "$calistrack_root/scripts/dotnet.sh" run --project "$calistrack_root/CalisTrack.csproj" "$@"
