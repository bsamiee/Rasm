# [INTERNALIZATION_TAXONOMY]

## [TRANSCRIPT]

Role: refinement / Internalization and Anti-Pattern Taxonomy Refiner for Rasm `AGENTS.md` work.

Scope: read-only synthesis over context reports and current instruction overlays. No active repository files were edited before this report. The only intended write is this same report under `docs/standards/_reports/agents-md-050626/track-owner-routing/03-internalization-taxonomy.md`.

Required reports read:
- `docs/standards/_reports/agents-md-050626/track-source-scans/01-assay-context.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/06-poly-csharp.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/07-poly-typescript.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/08-poly-python.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/09-poly-bash-sql.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/02-csharp-large-context.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/03-csharp-platform-context.md`
- `docs/standards/_reports/agents-md-050626/track-source-scans/12-tests-bridge-context.md`

Current overlays and standards read:
- `CLAUDE.md`
- `AGENTS.md`
- `docs/standards/README.md`
- `docs/standards/AGENTS.md`
- `docs/standards/agents-md.md`
- `tools/assay/AGENTS.md`
- `libs/csharp/AGENTS.md`

Memory was used only as a targeting aid for current `docs/standards` and `tools/assay` context. Live repo files and context reports are the controlling evidence.

## [SYNTHESIS]

context converges on one cross-language posture: the repo is not asking for "more abstractions." It is asking for fewer, deeper, owner-centered surfaces where variation is represented as values, cases, rows, folds, typed rails, receipts, operation algebras, service/layer requirements, catalog facts, or source-owned tables.

The strongest wording pattern is not "avoid helpers" or "use polymorphism." The strongest pattern is:

When adding <change class>, extend <named owner rail> through <allowed extension action> before adding <rejected substitute>.

That pattern lets future overlays stay compact. Root policy owns the broad principles. Leaf overlays should only name the local owner rail and the local failure mode.

## [TAXONOMY]

### [1][FUTURE_ONLY_POSTURE]

Intent: standards describe the strongest viable target, not the current code ceiling.

Accepted pattern:
- State the newest viable target directly.
- Route present-state proof to source, manifests, architecture, roadmap, or tool output only when claiming something already exists.
- Treat older code, partial adoption, and compatibility surfaces as replacement evidence.

Rejected pattern:
- "Current code still uses X, so preserve X."
- "Add a compatibility alias until callers migrate."
- "Document both old and new surfaces."

AGENTS wording block:
```markdown
Standards in this folder target the newest viable implementation shape. Current drift, missing callers, older manifests, or partial adoption do not weaken the target; they only decide where present-state proof routes.
```

Leaf overlay wording:
```markdown
When adding <capability>, build toward <future owner rail> even if current callers are absent. Do not add compatibility aliases, transitional wrappers, or old-baseline caveats.
```

### [2][OWNER_RAIL_FIRST]

Intent: every change starts by identifying the existing owner that should grow.

Accepted owners:
- C#: operation algebra, generated union, smart enum, value object, typed receipt, `Fin`/`Validation`/`Eff` rail, host boundary capsule.
- TypeScript: discriminated union, schema authority, `satisfies` table, Effect service/layer, exhaustive match.
- Python: behavior-bearing enum, msgspec message model, catalog row, registry bind, tagged detail union, `Result`/`Option` rail, aspect slot.
- Bash: dispatch table, option metadata table, nameref output contract, typed exit receipt.
- SQL: domain/composite/enum/range type, SQLSTATE rail, generated column/view, catalog extraction query, set-based query algebra.
- Tests/bridge: law matrix, generator/oracle owner, source-owned scenario, fact bag, bridge evidence marker.

Rejected substitutes:
- sibling functions, option bags, wrapper classes, helper files, one-off registries, duplicate DTO/schema/model families, facade APIs, branch ladders.

AGENTS wording block:
```markdown
When adding a local behavior family, identify the owner rail first: operation algebra, typed case, row table, fold, receipt, schema authority, service/layer, catalog query, or boundary adapter. Extend that owner before adding public entrypoints, helper files, wrapper objects, option bags, or parallel models.
```

Short leaf pattern:
```markdown
When adding <local variant>, extend <OwnerRail> with <case,row,fold,receipt,projection> before adding <bad sibling surface>.
```

### [3][INTERNALIZATION_BEFORE_EXPOSURE]

Intent: external libraries, backends, host APIs, and runtime mechanisms become internal behavior. They do not become agent-facing knobs or package-forwarding APIs.

Accepted pattern:
- Integrate package/backend behavior into the local operation algebra, runtime record, store, envelope, receipt, typed intent, scheduler, service layer, or boundary capsule.
- Expose Rasm concepts: intent, operation, receipt, projection, capability record, fact stream, or typed result.
- Route exact package versions and package proof to manifests, architecture, or tool output.

Rejected pattern:
- backend-branded commands;
- public provider selectors;
- package API pass-throughs;
- `DbContextOptions`/`SqliteConnection`/toolkit settings bags crossing boundaries;
- cloud/remote/storage flags when the owner rail can internalize the behavior;
- service locators and package facades.

Universal wording block:
```markdown
External libraries and runtime backends are implementation surfaces. Internalize them into the owning rail before exposing behavior: operation algebra, runtime record, store method, scheduler, service/layer, receipt, projection, envelope, or boundary adapter. Public API names Rasm intent and typed results, not provider knobs or package facades.
```

Assay-shaped wording:
```markdown
Runtime backend or storage behavior extends settings, store, engine execution, history persistence, and envelope/artifact rows before adding a CLI flag, command family, helper module, wrapper service, or parallel store type.
```

C# platform wording:
```markdown
Approved package behavior binds to the local operation algebra or runtime record first. Expose typed operations, receipts, projections, capability records, and Rasm intents; route exact package proof to manifests or architecture.
```

### [4][MINIMUM_SHAPE_MAXIMUM_CAPABILITY]

Intent: fewer public shapes should carry more capability. Small shape count is not the same as deleted behavior.

Accepted pattern:
- Collapse repeated cases into one dense polymorphic surface.
- Preserve behavior through richer cases, projections, tables, folds, and receipts.
- Prefer one deep owner type over many loose shallow types.

Rejected pattern:
- deleting behavior to reduce LOC;
- splitting into helper files to make files look smaller;
- one interface per implementation;
- one params/model/result type per method;
- generic receipt ledgers that erase algorithm evidence.

AGENTS wording block:
```markdown
Minimize shape count, not capability. Preserve behavior by deepening the owning rail with cases, payloads, folds, projections, and receipts; do not delete capability, split helper files, or add shallow sibling shapes to make the surface look simpler.
```

Rejection block:
```markdown
No helper extraction, facade split, generic ledger, or one-method interface when the existing owner can carry the variant as a case, row, fold, payload, or typed receipt.
```

### [5][DATA_DRIVEN_AND_ALGORITHMIC_LOGIC]

Intent: logic varies by data, bounded vocabularies, generated cases, tables, folds, and source-owned facts, not by imperative branch sprawl.

Accepted pattern:
- table-driven dispatch;
- generated union or smart enum dispatch;
- schema-derived projection;
- set-based SQL;
- Bash associative dispatch;
- catalog rows and registry binds;
- generated/law case sweeps.

Rejected pattern:
- `if`/`else` or `switch` ladders that restate vocabulary;
- mutable accumulation for domain transforms;
- row-at-a-time SQL loops where set queries can own the operation;
- option parsing or command routing through repeated branch blocks.

AGENTS wording block:
```markdown
Behavior varies through owned data: bounded vocabularies, case payloads, row tables, query algebra, generated dispatch, folds, and source-owned catalogs. Branch chains are allowed only at boundary adapters or where pattern matching is the owner dispatch.
```

Folder-specific examples:
```markdown
Command: add one dispatch row and one implementation.
Tool: add one catalog row and one registry bind.
SQL error: add one SQLSTATE condition and structured fields.
Test: add one law matrix or generated sweep before repeated facts.
```

### [6][BOUNDARY_CANONICALIZATION]

Intent: external names and unsafe runtime channels are converted at the boundary; internal code uses canonical Rasm names and typed rails.

Accepted pattern:
- Map external contract names once at ingress/egress.
- Convert host nulls, exceptions, disposables, callbacks, handles, return codes, and text errors into typed rails.
- Preserve obsolete native values only as owner-boundary projections when live runtime/documents can emit them.

Rejected pattern:
- native names leaking through domain surfaces;
- public obsolete aliases;
- parsing PostgreSQL/Rhino/bridge text output as proof;
- raw nullable/bool/exception rails in domain logic;
- direct host-internal access outside boundary capsules.

AGENTS wording block:
```markdown
External contract names, obsolete native values, nulls, exceptions, handles, callbacks, return codes, and text diagnostics convert at the owning boundary. Internal code uses canonical Rasm vocabulary and typed rails. Boundary projections are allowed only when the live external system can emit the value; they are not compatibility APIs.
```

### [7][PROOF_CLASSIFICATION_BEFORE_ASSERTION]

Intent: tests, bridge scenarios, and runtime proof must choose the right rail before writing assertions or claims.

Accepted classifications:
- static-managed law;
- bridge-owned runtime scenario;
- architecture/tooling/fuzz/benchmark rail;
- mutation survivor;
- proof gap.

Rejected pattern:
- skipped xUnit as a bridge gap;
- static fake of host runtime;
- mocks/headless substitutes for Rhino/GH2 behavior;
- scenario probes with no real production source or host fact;
- raw monadic state peeking as primary proof.

AGENTS wording block:
```markdown
Before adding or changing a test assertion, classify the behavior as static-managed law, bridge-owned runtime scenario, architecture/tooling/fuzz/benchmark rail, mutation survivor, or proof gap. Bridge-owned behavior becomes source-owned `*.verify.csx` runtime law, not static fake proof, skipped xUnit, mock host, or documentation caveat.
```

Rail-peeking wording:
```markdown
Raw `IsSome`/`IsNone`/`IsSucc`/`IsFail` checks are secondary invariants unless rail state is the contract under test. Primary proof uses independent oracles, `Spec` helpers, failure categories, bridge facts, or receipt invariants.
```

### [8][LOCAL_EXTENSION_GRAMMAR_OVER_SLOGANS]

Intent: overlays must make the correct edit obvious to a fresh agent.

Accepted pattern:
- trigger;
- owner;
- action;
- rejected substitute;
- route when local owner is not enough.

Rejected pattern:
- "Use advanced polymorphism."
- "Avoid helpers."
- "Keep it clean."
- capability tutorials copied into overlays;
- package lists without action;
- command catalogs.

AGENTS wording template:
```markdown
When <change class>, read <owner source> and extend <owner rail> by <allowed action>. Do not add <specific rejected substitute>; route <non-owned fact> to <README/architecture/roadmap/source/tool>.
```

Example:
```markdown
When adding wire behavior, extend `WireOp`, `WireEdit`, `WireQuery`, `WireResult`, or `WireRepositoryRail`. Do not add a second reflective GH2 wire reader or direct host-internal access outside that capsule.
```

### [9][ROUTE_AWAY_WITHOUT_DUPLICATION]

Intent: `AGENTS.md` names behavior-changing local rules. It does not become a README, architecture, roadmap, package matrix, command reference, transcript, or research summary.

Accepted route owners:
- README: entrypoint orientation, public command/operator workflow.
- Architecture: code shape, package roles, host/native proof, state that can drift.
- Roadmap: future sequence and not-yet-landed project shape.
- Tool README: command syntax and public wire behavior.
- Manifests/project files: exact packages, versions, graph membership.
- Source owners: actual rails, cases, receipts, generated contracts.
- `proof.md`: evidence labels, freshness, proof gaps, preservation rules.

Rejected pattern:
- copying command tables into root or leaf overlays;
- package versions in instruction prose;
- research transcript in active docs;
- hand-maintained generated dictionaries;
- run-local artifact paths as durable proof.

AGENTS wording block:
```markdown
Keep this overlay to local action constraints. Route command syntax to the tool README, package and graph truth to manifests/project files, implementation sequence to roadmap, package/native proof to architecture or tool output, and generated dictionaries to their source extraction.
```

### [10][STOP_RULES_FOR_UNPROVABLE_RUNTIME]

Intent: a stop rule is needed when continuing would fabricate proof or hard-code the wrong owner.

Accepted stop triggers:
- missing host/runtime API proof;
- native package load uncertainty;
- unavailable bridge/runtime state;
- missing shared-contract owner;
- unproved shutdown/drain/embedding behavior;
- provider/native/encryption/corrupt-store proof gap;
- bridge scenario without real source/host fact.

Rejected pattern:
- continuing with a static assumption;
- adding a compatibility shim;
- documenting a caveat as if it were proof;
- moving proof into root prose.

AGENTS wording block:
```markdown
Stop when the change requires host/runtime/native/provider behavior that static source cannot prove. Route to the owning architecture, README, API rail, bridge scenario, or shared-contract owner before adding code or public surface.
```

## [ANTI_PATTERN_CATALOG]

Use these as reusable rejection families. Pair each with a replacement owner in the local overlay.

| [INDEX] | [ANTI_PATTERN]                                   | [REPLACEMENT_PATTERN]                                                         |
| :-----: | :----------------------------------------------- | :---------------------------------------------------------------------------- |
|   [1]   | Helper or utility extraction for one caller.     | Inline into owner or extend owner rail.                                       |
|   [2]   | Wrapper-only API around dependency or host call. | Boundary capsule with policy, typed failure, batching, proof, or safety.      |
|   [3]   | Provider/backend knob as public API.             | Internal operation algebra, runtime record, store, service/layer, or receipt. |
|   [4]   | Parallel DTO/schema/model/type for one concept.  | One schema/message/value owner with derived projections.                      |
|   [5]   | Options bag or grab-bag params.                  | Closed case, smart enum payload, value object, typed command, or schema case. |
|   [6]   | Repeated branch ladder over bounded vocabulary.  | Dispatch table, generated union dispatch, smart enum behavior, match fold.    |
|   [7]   | Generic receipt ledger.                          | Typed owner-local receipt preserving route/status/native/model/proof fields.  |
|   [8]   | Public package facade.                           | Rasm intent, operation, projection, capability record, or typed result.       |
|   [9]   | Static fake of native runtime behavior.          | Source-owned bridge scenario with structured facts.                           |
|  [10]   | Hand-maintained generated dictionary.            | Catalog/source extraction plus generated reference.                           |
|  [11]   | Command catalog in `AGENTS.md`.                  | Tool README or generated CLI reference.                                       |
|  [12]   | Compatibility alias or transitional wrapper.     | Direct replacement through canonical owner.                                   |
|  [13]   | Current-baseline caveat.                         | Future-only target plus present-state proof route.                            |
|  [14]   | Run-local artifact path as durable proof.        | Command, scenario, status, and fact keys.                                     |
|  [15]   | Exact package/version prose in overlay.          | Manifest/project file route.                                                  |

## [CONCRETE_WORDING_BLOCKS]

### [ROOT_OR_PARENT_AGENTS_MD]

```markdown
When a subtree adds a capability family, local overlays must name the owner rail that grows: operation algebra, typed case, row table, fold, receipt, schema authority, service/layer, catalog query, boundary adapter, or source-owned scenario. Reject slogans that only say "avoid helpers" or "use polymorphism" without naming the replacement owner.
```

```markdown
External dependencies, host APIs, runtimes, and backends are integrated as behavior inside the owning rail. Public surfaces expose Rasm intent, operations, receipts, projections, capability records, or typed results; they do not expose package facades, provider selectors, option bags, backend-branded commands, or compatibility shims.
```

```markdown
For test-owning overlays, require behavior classification before assertions: static-managed law, bridge-owned runtime scenario, architecture/tooling/fuzz/benchmark rail, mutation survivor, or proof gap. Command syntax routes to tool docs; the overlay owns local oracle rules, helper-promotion rules, runtime boundaries, and stop behavior.
```

### [CODE_OWNING_LEAF]

```markdown
## [2][OWNER_CONTRACT]

This folder owns <concern> through <OwnerRailA>, <OwnerRailB>, and <OwnerRailC>. Add behavior by extending those rails with cases, rows, folds, projections, or typed receipts. Do not add sibling entrypoints, helper files, wrapper services, option bags, or parallel model families for the same concept.
```

```markdown
## [3][EXTENSION_GRAMMAR]

- <Variant>: add one <case/row/bind/type/member> to <OwnerRail>.
- <Boundary behavior>: convert external nulls, exceptions, handles, callbacks, and native statuses into <TypedRail>.
- <Dependency behavior>: integrate <dependency class> inside <OwnerRail>; expose <RasmConcept>, not provider knobs.
- <Proof behavior>: route static proof to <source/tool>; route runtime proof to <scenario/bridge>.
```

```markdown
## [6][REJECTIONS]

- No helper, utility, facade, manager, service, common, misc, option-bag, or wrapper-only object when <OwnerRail> can carry the behavior.
- No public package API, provider selector, backend mode, or compatibility alias where a typed Rasm intent, operation, receipt, projection, or boundary adapter can own the behavior.
- No branch ladder over <bounded vocabulary>; use <dispatch owner>.
```

### [TOOL_OWNING_LEAF]

```markdown
This tool is one engine over data rows. Programs, commands, checks, output envelopes, artifacts, and backend behavior extend through catalog rows, registry binds, tagged details, store methods, aspect slots, and envelope folds. Operator prose and command syntax route to `README.md`.
```

```markdown
Runtime libraries are internal mechanisms unless they change public operator workflow. Integrate them through axis payloads, catalog rows, aspect slots, store methods, status folds, and detail variants before adding command flags or wrapper modules.
```

### [PLATFORM_LEAF]

```markdown
Platform package behavior is internal. Bind packages to the local runtime record, scheduler, lifecycle op, store query, substrate intent, observable projection, or typed receipt before adding service registries, provider options, package facades, or public toolkit settings.
```

```markdown
Stop when runtime/native/provider behavior cannot be proven from source, manifests, architecture, API lookup, or bridge/runtime evidence. Do not continue by adding a shim, compatibility flag, static fake, or documentation caveat.
```

### [TEST_AND_BRIDGE_LEAF]

```markdown
Before adding assertions, classify behavior as static-managed law, bridge-owned runtime scenario, architecture/tooling/fuzz/benchmark rail, mutation survivor, or proof gap. Static specs prove managed algorithms and failure categories; bridge scenarios prove real Rhino/GH2 runtime behavior through source-owned `*.verify.csx` laws.
```

```markdown
Promote helper capability only when multiple specs consume the same law, oracle, generator, serializer, fixture, or scenario harness. Keep one-spec data local and prefer one dense law matrix over repeated narrow facts.
```

## [APPLICATION_GUIDE_FOR_FUTURE_OVERLAYS]

Use this checklist when refining a future `AGENTS.md`:

- Does deleting the overlay remove a real local behavior delta?
- Does the lead name scope, parent relation, highest-risk invariant, and route-away?
- For each change class, does the overlay name the owner rail to extend?
- Are external libraries described as internal behavior, not knobs?
- Are exact versions, package lists, command syntax, and graph facts routed away?
- Are tests classified before proof is chosen?
- Are runtime/native/provider proof gaps stop rules, not caveats?
- Are rejections paired with replacement owners?
- Are large folders using exact rails rather than concern nouns?
- Is any sentence just root policy restated without local action? Delete or localize it.

## [CONFIDENCE]

High confidence on the taxonomy. Every context report independently points to the same structure: future-only posture, one owner rail per concern, internalized external behavior, typed rails, dense data-driven dispatch, evidence-gated runtime proof, and compact trigger-driven overlay wording.

High confidence on the AGENTS wording shape. `docs/standards/agents-md.md` already requires local extension grammar, route-away behavior, anti-fragility, and rejection replacements; this report turns those requirements into reusable code-posture patterns.

Medium confidence on exact future placement. The taxonomy can live in `docs/standards/agents-md.md`, root `AGENTS.md`, parent overlays, or a short companion standard. The safest next edit would add only compact wording to `agents-md.md` and keep folder-specific examples in the relevant overlays.

No validation gates were run because this was a read-only _reports/report task. After the temporary report file is written, the appropriate check is `git diff --check -- docs/standards/_reports/agents-md-050626/track-owner-routing/03-internalization-taxonomy.md`.
