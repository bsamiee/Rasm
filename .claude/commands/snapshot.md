---
description: Checkpoint-commit the entire working tree as one labeled snapshot
disable-model-invocation: true
---

# [SNAPSHOT]

A snapshot freezes the whole tree as one recoverable, inspectable commit and pushes it — mid-run artifacts included, nothing judged, gated, formatted, or reviewed.

1. `git status --porcelain=v2` — empty status returns `[SKIP] nothing to snapshot` and stops; never an empty commit.
2. `git add -A`
3. `git commit -m "snapshot: <headline>"` — headline names the dominant in-flight work in a few words, read from the status concentrations, never a file list.
4. `git push` — a rejection surfaces raw and stops; the local snapshot stands regardless.
5. Return the short hash, the file count, and one line naming what the snapshot holds.

Snapshots stack freely; each invocation is its own labeled point, and no other command runs between stage and commit.
