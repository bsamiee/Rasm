---
type: regex
target: { source: file, path: .memory/tessel-runtime.md }
pattern: '^name: "?tessel-runtime"?\n(?:[^\n]*\n)*?description: [^\n]{1,152}\n(?:[^\n]*\n)*?[ \t]+type: "?(?:feedback|reference|project|user)"?[ \t]*\n'
flags: m
---
