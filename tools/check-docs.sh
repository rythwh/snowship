#!/usr/bin/env bash
# Verify that every repo path referenced in the docs still exists, so the knowledge base does not rot.
# Checks markdown link targets and `backticked` paths that start with a known top-level folder.
#
# Usage: tools/check-docs.sh            # exit 1 if any referenced path is missing

set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT" || exit 1

missing=0
checked=0

while IFS= read -r file; do
	dir="$(dirname "$file")"
	# Markdown links: [text](target) -- skip URLs and pure anchors.
	while IFS= read -r target; do
		target="${target%%#*}"
		[[ -z "$target" || "$target" =~ ^(https?:|mailto:) ]] && continue
		# Links are relative to the doc's directory.
		resolved="$dir/$target"
		checked=$((checked + 1))
		if [[ ! -e "$resolved" ]]; then
			echo "$file: missing link target -> $target"
			missing=$((missing + 1))
		fi
	done < <(grep -oE '\]\([^)]+\)' "$file" | sed -E 's/^\]\((.*)\)$/\1/' | sed -E 's/ .*//')

	# Backticked repo-root paths such as `Assets/Code/Map/Map.cs` or `ProjectSettings/...`.
	while IFS= read -r target; do
		target="${target%%:*}"   # allow `path:Symbol`
		checked=$((checked + 1))
		if [[ ! -e "$target" ]]; then
			echo "$file: missing path -> $target"
			missing=$((missing + 1))
		fi
	done < <(grep -oE '`(Assets|Packages|ProjectSettings|docs|tools|NonIncludedResources)/[^` *]+`' "$file" | tr -d '`' | grep -v '[<>{}]')
done < <(find docs CLAUDE.md -name '*.md' 2>/dev/null)

echo "Checked $checked references, $missing missing."
[[ $missing -eq 0 ]]
