---
name: tessel-prism-runtime
description: "Running scripts or batch exports in Tessel Prism: its scripts folder, the --headless flag batch exports need, E_NO_HOST right after launch"
metadata:
  type: reference
---

Prism scripts sit in `~/Library/Tessel/Prism/Scripts`, under the scripts folder rule in [[tessel-runtime]] (tried 2026-11-18)
- Batch exports need `--headless`, as Prism otherwise opens a window per document (tried 2026-11-20)
- Calls in the first seconds after launch fail with `E_NO_HOST` until Prism finishes loading its catalog, so retry after `tessel-cli status` reports `ready` (tried 2026-11-20)
