#!/usr/bin/env bash
# Wrapper for the Unity CLI that always targets this project (safe with several Editors open).
# Uses `unity` from PATH, falling back to %LOCALAPPDATA%\Unity\bin\unity.exe.
#
# Usage examples:
#   tools/unity.sh status --json          # is the Editor connected? (needs com.unity.pipeline in the project)
#   tools/unity.sh pipeline list --json   # shows running Editors, Pipeline package state, Safe Mode
#   tools/unity.sh command                # list commands the connected Editor exposes
#   tools/unity.sh recompile              # ask a running Editor to recompile and report errors
#
# Override the executable with UNITY_CLI=/path/to/unity.exe.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if [[ -n "${UNITY_CLI:-}" ]]; then
	CLI="$UNITY_CLI"
elif command -v unity >/dev/null 2>&1; then
	CLI="unity"
else
	CLI="${LOCALAPPDATA:-/c/Users/$USER/AppData/Local}/Unity/bin/unity.exe"
fi
if ! command -v "$CLI" >/dev/null 2>&1 && [[ ! -x "$CLI" ]]; then
	echo "Unity CLI not found at $CLI (set UNITY_CLI)" >&2
	exit 1
fi

# The CLI matches Editors by a Windows path with backslashes; a Git Bash path (/c/...) or
# forward-slash path (C:/...) matches no instance, so convert it.
PROJECT_PATH="$ROOT"
if command -v cygpath >/dev/null 2>&1; then
	PROJECT_PATH="$(cygpath -w "$ROOT")"
fi

# Commands that accept --project-path; everything else is passed through untouched
# (`pipeline ...` and `job ...` do not accept it and resolve the project from the current directory).
cd "$ROOT"
case "${1:-}" in
	status|command|commands|list|recompile|mcp)
		exec "$CLI" --no-banner "$@" --project-path "$PROJECT_PATH"
		;;
	*)
		exec "$CLI" --no-banner "$@"
		;;
esac
