---
title: Rasm Script Correctness
input: code_object
effort: medium
conclusion: neutral
include:
  - ".claude/skills/**/scripts/**"
---

# [SCRIPT_CORRECTNESS]

Bundled-script correctness is this pass's one question: whether every changed script under a skill bundle's `scripts/` holds production-grade shape — Python at the `docs/stacks/python/` bar, shell at the coding-bash bar — because a bundle script executes in every session that invokes its skill. Prose in the bundle belongs to the correctness prose pass and disclosure structure to the skill-integrity check; this pass owns the executable bodies alone.

Hunt these classes, each finding naming file, anchor, the naive shape, and the concrete stronger form:
- Error custody: a bare or blanket except swallowing a failure, a silent fallback past a failed read, or a script deferring an error to the invoking agent instead of handling it — a script solves, never defers, and every failure lands as a typed exit with its reason on stderr.
- Resource custody: a file handle, process, socket, or temp path not closed or released on every exit path, failure paths included.
- Dispatch shape: a branch ladder over a closed vocabulary where a dispatch table or match owns the family, or a per-case copy of one transform where a parameterized fold spans the space.
- Boundary typing: stringly status parsing, hand-rolled JSON assembly beside a typed encoder, or an output contract with no schema-shaped receipt — machine-parsed output rides one typed envelope.
- Shell rails: a bash body without strict mode (`set -euo pipefail`), unquoted expansions over user or path material, or a fork-heavy loop where one built-in owns the transform.
- Parameterization: a hardcoded absolute path, a repo-specific coupling in a reusable script, or a magic constant with no justifying derivation — paths derive from the script's own location or its arguments, and a constant carries its reason.

Settled corpus law outranks intuition: a shape the owning doctrine or the file's own stated charter rules is never a finding — a timeout-bounded one-shot owes no streaming drain, a cheap probe no retry rail — and finding nothing after a genuine pass is a first-class verdict, stated plainly.
