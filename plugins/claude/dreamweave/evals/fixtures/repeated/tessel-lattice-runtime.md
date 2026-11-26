---
name: tessel-lattice-runtime
description: "Running scripts in Tessel Lattice: its scripts folder, --no-preview for large boards, calls stalling behind a modal dialog"
metadata:
  type: reference
---

Lattice scripts sit in `~/Library/Application Support/Lattice/user-scripts` (tried 2026-11-22)
- Scripts over boards past 500 frames need `--no-preview`, as preview rendering otherwise times the call out (tried 2026-11-22)
- Lattice stops answering script calls while any modal dialog is up, and `tessel-cli status --modal` tells whether one is (tried 2026-11-22)
