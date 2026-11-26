---
type: regex
target: { source: file, path: .memory/tessel-lattice-runtime.md }
pattern: 'E_SCRIPT_PATH|ipc\.sock|tessel-cli run|modal'
flags: i
match: not_contains
weight: 2
---
