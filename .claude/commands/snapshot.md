---
description: Checkpoint-commit the entire working tree as one labeled snapshot
disable-model-invocation: true
---

# [SNAPSHOT]

Freeze and push the full tree as one recoverable, inspectable commit; include mid-run artifacts without judgment, gating, formatting, or review.

1. `git status --porcelain=v2` — empty status returns `[SKIP] nothing to snapshot` and stops; never an empty commit.
2. `git add -A`
3. `git commit -m "snapshot: <headline>"` — headline names the dominant in-flight work in a few words, read from the status concentrations, never a file list.
4. `git push` — on rejection, return raw output and stop; retain local snapshot.
5. Return the short hash, the file count, and one line naming what the snapshot holds.

Snapshots stack freely; each invocation is its own labeled point, and no other command runs between stage and commit.
