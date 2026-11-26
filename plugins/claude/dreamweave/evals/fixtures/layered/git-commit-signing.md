---
name: git-commit-signing
description: "Committing in any repository: sign every commit with the SSH signing key; covers the CI bot exception and how to confirm a signature"
metadata:
  type: feedback
---

Committing: sign every commit with the SSH key `~/.ssh/signing_ed25519`

**Why:** the main branch refuses unsigned commits at push
- CI bot commits stay unsigned, as the bot holds no key
- `git log --show-signature -1` confirms a signature (tried 2026-11-12)
