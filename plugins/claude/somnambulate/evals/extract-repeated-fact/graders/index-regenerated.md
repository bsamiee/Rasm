---
type: regex
target: { source: file, path: .memory/MEMORY.md }
pattern: '^## tessel\n(?:- [^\n]+\n)*- \[(tessel-(?!(?:prism|facet|lattice)-runtime\])[a-z-]+)\]\(\1\.md\) — [^\n]+$'
flags: m
---
