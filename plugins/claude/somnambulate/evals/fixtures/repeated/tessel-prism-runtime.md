---
name: tessel-prism-runtime
description: "Running scripts or batch exports in Tessel Prism: its scripts folder, the --headless flag batch exports need, calls hanging on a modal dialog"
metadata:
  type: reference
---

Prism scripts sit in `~/Library/Tessel/Prism/Scripts`, and a script anywhere else never runs (tried 2026-11-18)
- Batch exports need `--headless`, as Prism otherwise opens a window per document (tried 2026-11-20)
- Open modal dialogs block every script call until they close, and `tessel-cli status --modal` reports one (tried 2026-11-19)
