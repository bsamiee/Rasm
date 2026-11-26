---
type: regex
target: { source: file, path: .memory/tessel-facet-runtime.md }
pattern: 'previously|formerly|used to|no longer|moved from|old (?:folder|path|location)|was `'
flags: i
match: not_contains
---
