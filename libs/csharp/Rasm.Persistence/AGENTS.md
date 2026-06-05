# [RASM_PERSISTENCE_AGENTS]

Scope: `libs/csharp/Rasm.Persistence/` only. Root policy and `libs/csharp/AGENTS.md` own universal C# and library-family rules; local README, `_ARCHITECTURE.md`, and `ROADMAP.md` own store state, package facts, and implementation sequence.

## [1][READ_ORDER]

- Before production work, read `README.md`, `_ARCHITECTURE.md`, and `ROADMAP.md` to decide store state, provider proof, and sequence.
- Before changing durable-work scheduling or runtime receipts, read `Rasm.AppHost/AGENTS.md`.
- Before changing live-state observables, read `Rasm.AppUi/AGENTS.md`.
- Before changing benchmark, cache, model-result, or execution-artifact storage, read `Rasm.Compute/AGENTS.md`.
- Before package, BCL, or host-reference changes, read `docs/system-api-map` and `docs/host-libraries.md`.

## [2][OWNER_CONTRACT]

`Rasm.Persistence` is the local durable-state platform. It owns store rails, query algebra, migrations, snapshots, read-only live-state projection, support-artifact storage, and typed store receipts; it does not own domain semantics, UI observation, GH solve behavior, AppHost scheduling, or compute execution.

Keep storage operations as typed lifecycle operations and typed query algebras. Use bracketed resource lifetimes, one operation context per call, explicit store receipts, and source-owned serialization or snapshot rails.

## [3][EXTENSION_GRAMMAR]

- New store operation: extend the lifecycle operation rail and typed query algebra before adding a repository wrapper.
- New durable receipt: extend store receipt fields when the fields carry route, status, count, checksum, compression, snapshot, or migration proof.
- New provider-specific behavior: put the proof in `_ARCHITECTURE.md`; keep this overlay to the owner action.
- New live-state emission: serialize emission inside the owner rail before AppUi observes it.
- New support artifact: route request ownership through AppHost; Persistence stores, redacts, exports, and cleans.

## [4][BOUNDARY_RULES]

| [INDEX] | [BOUNDARY]         | [RULE]                                                             |
| :-----: | :----------------- | :----------------------------------------------------------------- |
|   [1]   | `Rasm`             | No store, serializer, or persistence API                           |
|   [2]   | `Rasm.Rhino`       | No default persistence reference                                   |
|   [3]   | `Rasm.Grasshopper` | No store call during solve                                         |
|   [4]   | `Rasm.AppHost`     | Schedules durable work and correlates receipts                     |
|   [5]   | `Rasm.AppUi`       | Observes live state through its scheduler boundary                 |
|   [6]   | Support bundles    | AppHost requests; Persistence stores, redacts, exports, and cleans |

## [5][REJECTIONS]

- No GH solve hot-path calls.
- No EF, SQLite, serializer, or store references in domain projects.
- No generic repository wrapper, generic receipt ledger, or long-lived context.
- No default companion database lane in the in-process plugin path.
- No provider API, native asset, or encryption claim without architecture proof.
- No package versions in documentation text; version truth lives in central manifests.

## [6][STOP_RULES]

If provider behavior, native asset loading, encryption availability, corrupt-store recovery, downgrade guards, live-state serialization, backup, or snapshot behavior cannot be proved statically, route the claim to architecture proof or runtime verification instead of weakening the storage contract.
