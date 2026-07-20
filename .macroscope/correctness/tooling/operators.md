---
include:
  - "tools/**"
  - ".claude/**"
---

# [OPERATORS_AND_WORKFLOWS]

Operator and workflow code is deterministic orchestration judged as production source: schema-typed agent outputs, bounded loops, every stage writing its product to disk and returning a path receipt. Bare promises without await ownership, unbounded polling, wall-clock branching, subagents instructed to wait, and hand-authored exit semantics beside a typed envelope contract are findings.

## [01]-[ASSAY_CONTRACTS]

- Normal invocation emits exactly one JSON `Envelope` on stdout, diagnostics on stderr, exit code a projection of envelope status; code writing the protocol stream past the registry emit (a raw `print` or `sys.stdout.write`), or a hand-parsed status, is a finding — the wire contract already downgrades an attempted second emit to a `FAULTED` invariant envelope.
- `Completed(FAILED)` means a tool ran and found defects, and a non-zero tool exit stays on the `Completed` channel; a `Fault` means routing, spawn, lease, timeout, or precondition failure — conflating the two in a caller is a finding.
- `core/model.py` is the sole owner of the status-to-exit-code map and the severity fold; a rail or caller re-deriving an exit code, hardcoding a numeric rc, or re-implementing the fold instead of projecting `Envelope.status.exit_code` is a finding.
- Host-exclusive and dotnet-slot work acquires the shared lease through the one `leased(...)` combinator, returning `busy` when unheld; a rail running an exclusive tool without it, or hand-rolling a lockfile, is a finding, and host-bound claims (`bridge`, `package`, `provision`, copy-staged mutation) reject a non-local `exec_target` as `UNSUPPORTED` before argv composition.
- `assay static` diagnoses by default and mutates only under `--fix`, never rewriting a C# target that does not compile; no dry-run flag exists, and proposing one re-litigates a settled contract. Build gating reads `dotnet build` rc plus scoped zero-new findings, never the raw `--project` advisory aggregate.
- Artifacts route through `.artifacts/assay` via the artifact store — the single `makedirs` lives behind `ArtifactScope.ensure()`; a direct `os.makedirs`/`open` bypassing the store boundary, or a tool write landing at repo root (`mutants/` by name), is a finding.
- Provision evidence is redacted by contract at the adapter seam: a raw password, password-bearing DSN, token value, or absolute store path crossing the seam is an adapter fault, never a redaction to add downstream.

## [02]-[BRIDGE_AND_ANALYZERS]

- `tools/rhino-bridge/Contract` is additive-only: renaming or reusing an existing field, union discriminator, status rank, or exit code is a finding.
- A `[RhinoScenario]` entrypoint takes one `ScenarioContext`, returns `Fin<Unit>`, and emits through the typed evidence surface (`Fact`/`Note`/`Require`/`Expect`/`Certify`/`Capture.Snapshot`); scenario code carrying `#r`, `#load`, absolute build-output paths, local report paths, or direct capture files is a finding, as is capability access unbacked by a `Requires` probe.
- `--evidence author` output is candidate material, never proof: a committed `.reference.json` whose `admission` was not human-promoted, or an author-mode run treated as passing, is a finding.
- Interactive MCP host access stays idle during any bridge-held lifecycle — an interleaved probe contaminates the same command-history and spool evidence the cargo runner records; bridge writes land only under `.artifacts/assay/bridge/<runId>/` or `~/.rasm/`.
- `tools/cs-analyzer`'s rule register IS the code — `Kernel/Catalog.cs` plus both `AnalyzerReleases.{Shipped,Unshipped}.md` ledgers, whose divergence is a build failure; a `RuleRow` without its ledger entry or witness spans, and a prose catalog of the rules anywhere, are findings. Suppression rides `[CspExempt(justification)]` from the one contracts assembly, never `#pragma` or `.editorconfig`.
- A promoted rule — cs-analyzer `RuleRow` or `tools/biome` GritQL row — describes the semantic shape of one anti-pattern (trigger, predicate, exemption) and ships with witnesses that must fire and compact code that must not; reviewer prose never restates what those gates enforce.

## [03]-[WORKFLOW_AND_SKILL_SHAPE]

- Workflow scripts under `.claude/workflows/` fan out fresh-context agents through plain control flow: retry lanes are attempt-counted with bounded backoff, a dead pass isolates without rejecting its chain, and stage handoffs carry navigation facts, never verdicts.
- A law a lane must enforce rides a carrier its instruction chain reaches: a lane barred from the owning file cannot be bound by it, so the dispatch prompt parameterizes the law — a sole carrier unreachable by a consumer lane is a silent no-op, and the finding names the unreachable owner and the prompt slot owed.
- Every `agent()` call carries a label, model tier, effort, and a closed JSON schema (`additionalProperties: false` with explicit `required`); an unschematized call, or a retry loop that is unbounded or aborts the whole chain on one death, is a finding.
- Per-run scratch directories mint deterministically from the normalized target set, never clock or randomness, so a resume rehydrates the same directory; a stage returns a thin path receipt with its full product on disk, never bulk product inline.
- Read-only recon lanes ride codex wrappers behind the `CODEX` flag; a pass whose acceptance gate runs a network-bound toolchain (`dotnet restore`, `uv sync`, `pnpm install`) stays native, and routing it onto a codex wrapper is a finding.
- `SessionStart` mutations are the hook-owned secret snapshot and session caches only; a hook sourcing local shell files, installing tools, or wiring host bootstrap is a finding — machine tooling routes through the Forge owner, never a repo script.
- A durable finding lands at exactly one owner — workflow script, owning skill, or owning `CLAUDE.md` — never duplicated across them.
