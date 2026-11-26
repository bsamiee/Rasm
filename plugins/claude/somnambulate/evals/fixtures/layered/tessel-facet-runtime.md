---
name: tessel-facet-runtime
description: "Writing or running Facet scripts: its scripts folder, the @facet-api header, E_API_VERSION refusals, one undo step per script"
metadata:
  type: reference
---

Facet scripts sit in `~/Library/Tessel/Facet/Scripts`, under the scripts folder rule in [[tessel-runtime]] (tried 2026-11-18)
- Each script declares `@facet-api 3` on its first line, else Facet refuses it with `E_API_VERSION` (tried 2026-11-21)
- Facet records one undo step per change a script makes, so wrap edits in `facet.batch()` to keep one (sourced: Facet Scripting Guide 3.2)
