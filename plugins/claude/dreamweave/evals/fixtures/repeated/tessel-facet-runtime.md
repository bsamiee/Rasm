---
name: tessel-facet-runtime
description: "Writing or running Facet scripts: its scripts folder, the @facet-api header, E_API_VERSION refusals, script calls hanging on a dialog"
metadata:
  type: reference
---

Facet scripts sit in `~/Documents/Facet Automations` (tried 2026-11-18)
- Each script declares `@facet-api 3` on its first line, else Facet refuses it with `E_API_VERSION` (tried 2026-11-21)
- Script calls hang while a modal dialog is open in Facet, and `tessel-cli status --modal` shows whether one is (tried 2026-11-21)
