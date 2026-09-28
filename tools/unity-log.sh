#!/usr/bin/env bash
# Summarise the Unity Editor log for this project (the Editor writes to Logs/Editor.log).
#
# Usage:
#   tools/unity-log.sh errors       # de-duplicated C# compile errors, split into project vs packages (default)
#   tools/unity-log.sh warnings     # de-duplicated C# compile warnings from project code only
#   tools/unity-log.sh exceptions   # runtime exceptions (message line + first stack frame), de-duplicated
#   tools/unity-log.sh tail [N]     # last N (default 60) log lines with the timestamp prefix stripped
#   tools/unity-log.sh path         # print the log path being read
#
# Override the log location with UNITY_LOG=/path/to/Editor.log.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG="${UNITY_LOG:-$ROOT/Logs/Editor.log}"
MODE="${1:-errors}"

if [[ ! -f "$LOG" ]]; then
	echo "Log not found: $LOG" >&2
	exit 1
fi

# Editor.log lines look like "2026-09-28T19:15:28.806Z|0x8398|<message>"; strip that prefix.
strip() { sed -E 's/^[0-9]{4}-[0-9]{2}-[0-9]{2}T[^|]*\|[^|]*\|//' "$LOG" | tr -d '\r'; }

case "$MODE" in
	errors)
		all="$(strip | grep -E 'error CS[0-9]+' | sort -u || true)"
		project="$(grep -vE '(Library[\\/]PackageCache|^Packages[\\/])' <<<"$all" || true)"
		packages="$(grep -E '(Library[\\/]PackageCache|^Packages[\\/])' <<<"$all" || true)"
		echo "== Project compile errors (Assets/) =="
		if [[ -n "$project" ]]; then echo "$project"; else echo "(none)"; fi
		echo
		echo "== Package compile errors (Library/PackageCache, Packages/) =="
		if [[ -n "$packages" ]]; then
			# Collapse to file(line,col): code so the same error from each recompile shows once.
			sed -E 's/: error (CS[0-9]+):.*/: \1/' <<<"$packages" | sort -u
		else
			echo "(none)"
		fi
		echo
		echo "NOTE: the log accumulates across recompiles in one Editor session; errors fixed since may still appear."
		echo "      If project errors are (none) but packages fail, project code was probably never compiled."
		;;
	warnings)
		strip | grep -E 'warning CS[0-9]+' | grep -vE '(Library[\\/]PackageCache|^Packages[\\/])' | sort -u || true
		;;
	exceptions)
		# Print each "...Exception: msg" line followed by its first stack frame, then de-duplicate.
		strip | awk '/Exception: |Exception\r?$/ { msg=$0; getline nxt; print msg " || " nxt }' | sort | uniq -c | sort -rn || true
		;;
	tail)
		strip | tail -n "${2:-60}"
		;;
	path)
		echo "$LOG"
		;;
	*)
		sed -n '2,12p' "$0"
		exit 2
		;;
esac
