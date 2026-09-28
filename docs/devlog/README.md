# Devlog

One file per work session or change set: `YYYY-MM-DD-<short-slug>.md`. These are the "memories" of
what was done and why, so later sessions can see the history behind the current code without
digging through git.

Template:

```markdown
# YYYY-MM-DD: <title>

## Goal
What was asked / why.

## Changes
- `path/to/File.cs`: what changed (symbol-level).

## Verification
How it was checked (compile via `tools/unity-log.sh errors`, play-mode steps, debug commands). Say plainly what was NOT verified.

## Issues
- Fixed: ID, ID (moved to Resolved in ../issues.md)
- New: ID (added to ../issues.md)

## Follow-ups / notes for next time
Decisions made, dead ends, things to watch.
```

## Entries
- [2026-09-28-initial-review.md](2026-09-28-initial-review.md): full codebase review, docs and tooling set up
- [2026-09-28-unity-6.6-compile-fix.md](2026-09-28-unity-6.6-compile-fix.md): project compiles on 6000.6.3f1, Unity CLI connected
- [2026-09-28-commit-split.md](2026-09-28-commit-split.md): working tree split into 6 commits, Quick Start play-tested
