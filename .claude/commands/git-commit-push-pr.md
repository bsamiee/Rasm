---
description: Commit, push, and open a PR
allowed-tools: Bash(git checkout:*), Bash(git add:*), Bash(git status:*), Bash(git push:*), Bash(git commit:*), Bash(gh pr create:*), Bash(git stash:*), Bash(git branch:*), Edit, Read
---

## Context

- Current branch: !`git branch --show-current`
- Current git status: !`git status`
- Current git diff: !`git diff HEAD --stat`

## Workflow

1. **Branch**: If on `main`, create feature branch (`feat/short-description`) preserving all changes
2. **Stage**: `git add -A` (all changes)
3. **Commit**: Single commit, message format `[TYPE]: description` (FEAT/FIX/REFACTOR/CHORE)
4. **Push**: Push feature branch to origin (`git push -u origin <branch>`)
5. **PR**: Create PR via `gh pr create --base main` using template below

## PR Template

```markdown
# [Summary]

<!-- 1-2 sentences: what and why -->

## [Changes]

- <!-- bullet points of key changes -->

---

<!-- PR-MONITOR: START -->
<!-- PR-MONITOR: END -->
```

## Execution

Run all steps in a single message.
