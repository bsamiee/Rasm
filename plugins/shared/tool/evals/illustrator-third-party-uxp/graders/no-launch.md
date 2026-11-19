---
type: regex
weight: 1
arm: both
target: trace
match: not_contains
pattern: '"command":"(?:[^"\\]|\\.)*?(open\s+-[a-zA-Z]*a\b|osascript|\bsdef\s|killall|kickstart)'
---
