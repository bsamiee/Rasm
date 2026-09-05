---
paths:
  - "**/*.sh"
---

Shell scripts call the tools mise provides:
- `fd`, `rg`, `jq`, and `yq` are on `PATH`, and no script holds a fallback branch
- yq: an `as $x` binding breaks a `select` inside a `..` pipeline, and path algebra runs in jq over `yq -o=json` output
