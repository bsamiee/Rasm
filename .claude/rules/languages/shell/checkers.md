---
paths:
  - "**/*.sh"
---

Shell scripts pass shfmt and shellcheck:
- shfmt reads `.editorconfig` (4 spaces, indented case arms) and writes `${ cmd;}`
- shellcheck refuses `arr[word_$id]` inside `(( ))`, and `${arr[word_$id]}` parses
