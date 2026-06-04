# [PROMPT] Tool Rebuild — Unify-and-Finalize Methodology

Reusable prompt for **rebuilding an existing tool into a unified, finalized system**. Paste this, then name the target tool (e.g. `tools/<name>`) and any audit/critique that diagnosed it.

---

## [1] WHAT "REBUILD" MEANS HERE

A rebuild is **not** a from-scratch rewrite. It finalizes a tool whose initial implementation accreted **piecemeal across many sessions** — so it carries the predictable rot: entangled/duplicated types and constants (in any language), parallel shapes for one concept, dual paradigms / split-brain seams, status-string projectors, per-binary code fragmentation, fragile hardcoded logic, and hand-rolled code where a library already owns the concern. The job is **polymorphic collapse into one coherent, dense, agent-first system** — preserving every capability while destroying the entanglement.

Treat all code as greenfield: break APIs freely, no shims/adapters/aliases/back-compat wrappers. Density means **merging parallel types/enums/methods into one polymorphic dispatch surface**, not deleting functionality and not chasing a byte count.

## [2] NORTH-STAR / VALUE FILTER (apply to every proposal)

- **Agent-first / agent-only.** One machine contract: exactly one structured result object to stdout, all diagnostics to stderr, typed `Result`/error rails, exit codes that are the sole machine signal. Minimal returns: failures carry the *why + the fix*; successes are terse.
- **Auto-integrated, zero ceremony.** Capabilities are inherent (config/context-selected, transparent), never new commands or knobs an agent must remember — agents will improvise fragile ad-hoc scripts instead. Design the *event flow* so the right thing happens automatically.
- **Maximize the ecosystem.** Approved manifest dependencies are the implementation surface. Use the library's deepest idiomatic capability; never hand-roll what a combinator/primitive owns. A lint wall that forbids stdlib equivalents (no raw `json`/`asyncio`/`logging`/env-reads/etc.) makes lib-maximization *the quality gate*.
- **No grab-bag.** A capability earns inclusion only if it lets an agent do something it otherwise can't, or can only do via a fragile script — AND it reuses the unified shapes/rails. Reject everything else with a reason.
- **Density = concept count, not byte count.** Triggers are concept-density pressure points (≥3 parallel types / sibling factories / repeated dispatch arms / single-call helpers), never LOC.

## [3] FOLDER STRUCTURE

- **`<tool>/_design/`** — the design corpus: one design doc per intended source module (`_design/<layer>/<file>.md`: purpose, canonical shapes, one validated snippet, integration seams, extensibility, considerations) **plus three root docs**: `ARCHITECTURE.md` (the keystone — shapes/axes/invariants, aspect/cross-cutting doctrine, concurrency model, the **decision ledger**, the **parity matrix**, build-order DAG, honest density record), `README.md` (the agent operator contract: invocation, the result-envelope schema, exit codes, concurrency, migration), `AGENTS.md` (delta-only edit guidance: load order, per-file ownership, spam tripwires).
- **`<tool>/_TMP/`** — the **full, gate-validated reference implementation**, staged as a self-contained absolute-import package (e.g. `<pkg>._TMP.*`) so type-checkers/linters resolve it in place without touching the real stub paths. Promotion to the real module paths is then a mechanical scrub-and-move.

Keep `_design/` and `_TMP/` **synchronized** at all times — the docs are the spec, `_TMP/` is the proof; a doc↔code mismatch is a defect.

## [4] THE MULTI-STAGE WAVE METHODOLOGY

Run as a sequence of background workflows (one well-scoped fan-out per phase); read each phase's results and apply architectural judgment before the next. The orchestrator stays in the loop throughout.

1. **Deep-read + verdict harvest.** Read the existing tool, its README, and any audit/critique. Hold the contradiction set yourself (these become the decision ledger).
2. **Research wave (fan-out, ≥1 agent per library + per orchestrated tool).** Read the **actual installed source** and **current official docs** — never training-data API shape. Return per library: verified signatures (with file:line), advanced/underused levers, the precise integration seam, version gotchas. Separately harvest the old tool's **complete capability surface** + any external script surface it must absorb (the parity inputs).
3. **Author the keystone `ARCHITECTURE.md` yourself.** Resolve **every** contradiction to a single locked verdict in a numbered **decision ledger** (`D1..Dn`, each row: fork → verdict → source). Build the **parity matrix** (every old verb/step → new owner; nothing dropped). Fix the axes, invariants, cross-cutting doctrine, build-order DAG. This is the single shared spec every downstream agent reads.
4. **Per-module design-doc authoring (fan-out).** Each agent reads `ARCHITECTURE.md` (ledger = law) + its library dossiers + the validated snippets, writes one `_design/` doc with exact shapes and one re-provable snippet.
5. **Adversarial critique waves (loop until dry).** Independent critics over the corpus: shape/constant/alias spam; integration completeness (every lib genuinely used, no capability gap); **snippet-truth vs installed source** (every embedded line is valid, current-language, and exists in the real surface); dual-paradigm/split-brain; one-doc-per-module dedup; parity. Plus a red-team that independently designs a *better* tool and attacks the corpus. Each finding: severity + exact fix. Refine; repeat until a pass returns nothing.
6. **Full-code implementation into `_TMP` (dependency-ordered TIERS with barriers).** Tier the modules by their dependency DAG; run each tier as a parallel fan-out, **barrier between tiers**, and have each tier's agents read the prior tiers' *actual written code* (not just the docs) so signatures match exactly — this eliminates drift. Complete code: zero placeholders/`TODO`/`...`, modern language idioms, the lint wall satisfied.
7. **Real quality gate — run by YOU, not agents.** Lint(all rules)/type-check(strict)/compile/import are ground truth via the actual toolchain/compiler. **Never trust agent self-reported gate results** (agents mis-run tools from the wrong cwd and report phantom errors *or* false greens). Pin gate invocations to the repo root.
8. **Holistic scope-aware critique on the full `_TMP` → targeted fix waves → re-gate.** Critics see the *whole* codebase: cross-module duplication, cute/hacky patterns (global-injection, `cast`/ignore clusters, control-flow hacks), lib-depth, AOT/concurrency correctness, unified-contract. Fix disjoint files in parallel tiers; re-gate; loop until dry.
9. **Fold finalized code back into `_design/`.** Update each doc's shapes/snippet/seams to match the final `_TMP`; scrub staging-path artifacts; reconcile `ARCHITECTURE.md` (ledger amendments, honest measured density record — retire any wrong pre-estimate).
10. **Final gate + completeness sweep.** Zero contradiction markers in the corpus; parity confirmed; snippets type-plausible against installed source; gates green; one-doc-per-module.

## [5] NON-NEGOTIABLE DISCIPLINES (hard-won lessons)

- **Architectural authority stays with the orchestrator.** Agents research and implement; *you* synthesize the ledger, apply the value filter, and **reject bloat/grab-bag**. Lead with the decision, not the menu.
- **Verify load-bearing claims with a runtime test**, not reasoning — e.g. prove a cross-event-loop assumption actually holds, prove a new-language construct compiles, prove a failure path produces the expected output. The compiler/runtime is the authority.
- **Reject FALSE collapses.** "Duplication" that is actually *parameterized divergence* must not be merged — verify the premise before collapsing. Pushing back ("the math/premise fails") is part of the job.
- **No new shape for an existing concept.** Extend via the sanctioned mechanism (a tagged-union variant / a new enum member / one data row), never a parallel type/alias.
- **Inherent over imperative.** A new capability is a config-/context-selected branch in the *one* existing seam (e.g. a polymorphic execution backend), or an automatic enrichment of the existing result/event flow — not a new command, arm, or knob, unless it is genuinely a new concern.
- **Point-and-go robustness.** Construction/import must **never hard-fail on environment** (missing files, cwd, CI bootstrap, remote). Missing prerequisites become clean per-operation failures, not crashes. The tool must "just work" anywhere an agent points it.
- **Config in the manifest.** All dependency config lives in the project manifest (no loose config files); modern settings; version floors not pins; no needless dependency grouping.
- **Comments/docstrings are "why," not "what."** No ledger-citation ceremony scattered in code (that lives in `ARCHITECTURE.md`); keep genuine gotchas/invariants.

## [6] ORCHESTRATION NOTES

- This is an **ultracode** effort: author and run a workflow per phase; token cost is not the constraint, correctness and exhaustiveness are. Several workflows in sequence, one per phase, so you stay in the loop between them.
- Use **schemas** on agent returns to keep results terse and consistent; use **`Explore`/read-only** agents for research/critique and default (write-capable) agents for implementation.
- Within a phase, fan out disjoint work in parallel; use a **barrier** only when a stage genuinely needs all prior results (dedup, dependency tiers).
- Persist a short project memory: what was rebuilt, the locked decisions, what was rejected and why (so a future session doesn't re-chase them).

## [7] DELIVERABLE END-STATE

`_TMP/` = the gate-validated full implementation (every capability of the old tool + the new, passing every gate, run for real). `_design/` = the finalized spec that matches it exactly, contradiction-free, with a complete decision ledger and parity matrix. Promotion to the real source paths is a trivial mechanical step the operator triggers when ready.
