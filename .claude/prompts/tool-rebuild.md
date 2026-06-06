# [PROMPT] Tool Rebuild — Unify and Finalize

Reusable prompt for rebuilding an existing tool into a unified finalized system. Name the target tool and any audit or critique that diagnosed it.

## [1] WHAT REBUILD MEANS

A rebuild is not a from-scratch rewrite. It finalizes a tool whose implementation accreted across sessions and now carries duplicated types/constants, parallel shapes for one concept, dual paradigms, status-string projectors, per-binary fragmentation, fragile hardcoded logic, or local code where a library already owns the concern.

The job is polymorphic collapse into one coherent agent-first system while preserving every capability. Treat the code as greenfield: break APIs freely, remove shims and compatibility aliases, and collapse concepts instead of preserving stale public shapes.

## [2] VALUE FILTER

- **Agent-first contract.** One structured result object to stdout, diagnostics to stderr, typed result/error rails, and exit codes as the machine signal.
- **Auto-integrated behavior.** Capabilities are selected by config/context or enriched internally, not exposed as ceremony an agent has to remember.
- **Ecosystem leverage.** Use approved dependencies where they own the concern; do not hand-roll primitives already provided by the stack.
- **No grab bag.** A capability earns inclusion only when it enables real agent work and reuses unified shapes.
- **Density is concept count.** Merge parallel concepts; do not delete capability to reduce bytes.

## [3] FOLDER STRUCTURE

- **`<tool>/_design/`** — design corpus: one design doc per intended source module, plus `ARCHITECTURE.md` for shapes/axes/invariants, cross-cutting doctrine, concurrency model, decision ledger, parity matrix, build-order DAG, and density record; `README.md` for the operator contract; `AGENTS.md` for delta-only local ownership.
- **`<tool>/_TMP/`** — self-contained reference implementation staged under temporary absolute imports so promotion to real module paths is mechanical.

Keep `_design/` and `_TMP/` synchronized; doc-code mismatch is a defect.

## [4] MULTI-STAGE METHODOLOGY

1. **Deep-read and verdict harvest.** Read the existing tool, its README, and any audit or critique. Preserve contradictions as decision-ledger inputs.
2. **Research wave.** Inspect installed source and owner docs for each relevant library and orchestrated tool. Return signatures, advanced levers, integration seams, and real constraints.
3. **Author `ARCHITECTURE.md`.** Resolve contradictions into a numbered decision ledger. Build the parity matrix, axes, invariants, cross-cutting doctrine, and build-order DAG.
4. **Author per-module design docs.** Each doc uses `ARCHITECTURE.md` and the relevant library dossiers to define exact shapes, snippets, seams, and extension points.
5. **Critique waves.** Attack shape spam, alias spam, integration gaps, snippet drift, split-brain design, duplicated docs, parity loss, and bloat. Refine until critique stops finding actionable defects.
6. **Implement into `_TMP`.** Build dependency-ordered tiers with barriers where signatures must settle before downstream code lands. Complete code has no placeholders or unresolved TODOs.
7. **Holistic critique and fixes.** Review the full temporary implementation for cross-module duplication, control-flow hacks, shallow library usage, concurrency mistakes, and contract drift.
8. **Fold final code back into `_design/`.** Update shapes, snippets, seams, ledger amendments, and the density record to match the implementation.
9. **Completeness sweep.** Confirm parity, contradiction cleanup, one-doc-per-module ownership, and mechanical promotability.

## [5] DISCIPLINES

- **Architectural authority stays centralized.** Research can fan out; final decisions are singular and recorded.
- **Prove load-bearing claims.** Runtime, compiler, or owner-source evidence settles assumptions.
- **Reject false collapses.** Parameterized divergence is not duplication.
- **No new shape for an existing concept.** Extend the sanctioned union, enum, or data row.
- **Inherent over imperative.** A new capability should be context-selected or automatically enriched through an existing seam.
- **Point-and-go robustness.** Missing prerequisites become clean per-operation failures, not startup crashes.
- **Config in the manifest.** Dependency configuration belongs in the project manifest.
- **Comments and docstrings explain why.** Ledger citations belong in `ARCHITECTURE.md`, not scattered through code.

## [6] DELIVERABLE END STATE

`_TMP/` contains the complete implementation. `_design/` contains the finalized spec that matches it exactly, with decision ledger, parity matrix, density record, and promotion path.
