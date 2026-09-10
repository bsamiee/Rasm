---
description: Commit and push the entire working tree as one labeled snapshot
disable-model-invocation: true
---

# [SNAPSHOT]

Push the whole tree, mid-run artifacts included, as one recoverable commit without judgment, gating, formatting, or review.

1. `git status --porcelain=v2`, empty output ends the run with nothing to snapshot
2. `git add -A`
3. `git commit -m "snapshot: <headline>"`, `<headline>` names the dominant uncommitted work in a few words, read from the status
4. `git push`, a rejected push ends the run with the raw output and keeps the local commit
5. Report the short hash, the file count, and one line naming what the snapshot holds

No other command runs between stage and commit, each invocation is its own labeled point.
