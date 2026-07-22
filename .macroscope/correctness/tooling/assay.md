---
include:
  - "tools/assay/**"
  - "tests/python/tools/**"
---

# [ASSAY]

Assay is the polyglot gate operator; its rails are deterministic orchestration judged as production source, and its Envelope wire contract binds every rail and caller.

- Normal invocation emits exactly one JSON `Envelope` on stdout, diagnostics on stderr, exit code a projection of envelope status; code writing the protocol stream past the registry emit (a raw `print` or `sys.stdout.write`), or a hand-parsed status, is a finding — the wire contract already downgrades an attempted second emit to a `FAULTED` invariant envelope.
- `Completed(FAILED)` means a tool ran and found defects, and a non-zero tool exit stays on the `Completed` channel; a `Fault` means routing, spawn, lease, timeout, or precondition failure — conflating the two in a caller is a finding.
- `core/model.py` is the sole owner of the status-to-exit-code map and the severity fold; a rail or caller re-deriving an exit code, hardcoding a numeric rc, or re-implementing the fold instead of projecting `Envelope.status.exit_code` is a finding.
- Host-exclusive and dotnet-slot work acquires the shared lease through the one `leased(...)` combinator, returning `busy` when unheld; a rail running an exclusive tool without it, or hand-rolling a lockfile, is a finding, and host-bound claims (`bridge`, `package`, `provision`, copy-staged mutation) reject a non-local `exec_target` as `UNSUPPORTED` before argv composition.
- `assay static` diagnoses by default and mutates only under `--fix`, never rewriting a C# target that does not compile; no dry-run flag exists, and proposing one re-litigates a settled contract. Build gating reads `dotnet build` rc with scoped zero-new findings, never the raw `--project` advisory aggregate.
- Artifacts route through `.artifacts/assay` via the artifact store — the single `makedirs` lives behind `ArtifactScope.ensure()`; a direct `os.makedirs`/`open` bypassing the store boundary, or a tool write landing at repo root (`mutants/` by name), is a finding.
- Provision evidence is redacted by contract at the adapter seam: a raw password, password-bearing DSN, token value, or absolute store path crossing the seam is an adapter fault, never a redaction to add downstream.
- Assay's own suite lives at `tests/python/tools/assay` and moves in the same change as the rail it proves; a rail change without its spec change is incomplete.
