# [H1][CSHARP_LIBS_AGENTS]
>**Dictum:** *Library folders capture full capability behind small, powerful rails.*

<br>

`libs/csharp` is the parent of all `Rasm.*` projects: future-facing C# libraries built as reusable capability layers for downstream plugins, apps, tools, and agents that do not exist yet. No current consumer is required to justify complete domain functionality when the folder is a library boundary.

This file documents lib-scope deltas over root `AGENTS.md` and `CLAUDE.md`. Library contract (capture native capability, expose small OOP boundary, keep intelligence internal), surface preference, greenfield posture, dependency/package policy, and quality-gate validation are owned by those upstream documents — do not restate. Per-project deltas live in each `Rasm.*/AGENTS.md`; host-composition phasing (which packages are `[NOT_IN_GRAPH]` until a consumer exists) is owned by `docs/host-libraries.md` and `CLAUDE.md` §3 — point, do not copy.

---
## [1][SCAFFOLDING_PROTOCOL]
>**Dictum:** *New folders start from sibling truth and native truth.*

<br>

[IMPORTANT] Before creating or implementing a new folder:
1. Read `CLAUDE.md`, root `AGENTS.md`, this file, and the nearest folder-local `AGENTS.md`.
2. Read every existing folder in the target project directory and map owners, files, public rails, operation algebras, state records, receipts, and validation patterns.
3. Read relevant docs under `docs/external-libs`, `docs/system-api-map`, and project skills before choosing packages, system APIs, or host references.
4. Deep-read the external/native API with local truth sources: package XML/nuspec/DLL, RhinoWIP XML/decompile, GH2 XML/decompile, source docs, and repo scripts.
5. Produce a compact roadmap before production code when the folder is new or the concern boundary changes.
6. Audit the roadmap or implementation against native truth and existing folder patterns before completion.

Roadmaps must include: purpose + boundary (1-2 paragraphs); source-verified API catalog with false/missing APIs called out; source-discipline note requiring local XML/decompile proof for every named native member; proposed file architecture with durable concern ownership; centralization/removal plan for duplicated logic in sibling folders; value-add capabilities beyond raw API access; validation commands and runtime scope.

---
## [2][VALUE_ADD]
>**Dictum:** *Library code earns its abstraction by adding capability.*

<br>

Expected value-add examples (downstream consumers should get more power and less boilerplate than direct API usage):
- Idempotent create/update flows with explicit conflict policy.
- Batch operations with partial-failure diagnostics and grouped receipts.
- Native capability graphs, dependency audits, stale/missing resource checks, and summaries.
- Typed metadata, user-string, source, unit, tolerance, and layer policies.
- Block/document/preview/UI-ready projections from one canonical construction output.
- Management rails for resources with author/update, refresh/reload, cleanup, graph/audit, and event/watch capability.
- Automatic overload selection, validity checks, and native sentinel normalization.
- Event/watch rails with scoped disposal and typed snapshots.
- MathNet-backed fitting or solving only after explicit domain coordinate projection.

Do not hold back high-value library functionality because no caller exists yet. A library folder sets the capability ceiling; apps should stay thin.

---
## [3][SEMANTICS]
>**Dictum:** *Names should be short enough to use and precise enough to scale.*

<br>

- Prefer 1-2 word domain names for files, types, operations, and policies.
- Name files by durable concern: `Blocks.cs`, `State.cs`, `Kernels.cs`, `Frames.cs`, `Outputs.cs`, `Archive.cs`.
- Avoid bloated names that encode implementation steps, transient plans, or every native object involved.
- Avoid weak names: `Helpers`, `Utils`, `Manager`, `Service`, `Common`, `Misc`, `Options`, `Params`.
- Keep semantics universal inside the bounded context. Boundary adapters may translate external names; internals use canonical vocabulary.
- Rename decisively when a better semantic owner exists. Do not preserve transitional aliases or compatibility shims.

---
## [4][FILE_ARCHITECTURE]
>**Dictum:** *Files represent durable ownership, not a way to hide complexity.*

<br>

- Start with the smallest file set that preserves concern ownership.
- Add a file only when it owns a durable sub-concern with multiple callers or a distinct native boundary.
- Defer speculative splits with explicit thresholds; `Kernels.cs`, `Frames.cs`, and `Outputs.cs` start only after real multi-caller pressure.
- Keep operation intent separate from native execution when both are large enough to blur each other.
- Keep state records compact and typed; avoid record sprawl for one-use parameter bags.
- Keep generated/native kernels internal unless exposing native identity adds real semantic value.
- Reuse existing folder patterns before inventing new layout.
- **Data-only catalogue libs follow the same rules as geometry libs but never reference upstream geometry; they are sibling to `Rasm` core, not subordinate** (e.g. `Rasm.Materials` — zero ProjectReference. Composition with geometry happens in downstream consumer libs that reference both.).

---
## [5][LIB_TOPOLOGY]
>**Dictum:** *The reference DAG is the boundary; never let it cycle or leak a host.*

<br>

Durable per-lib ownership and the only legal `ProjectReference` direction (parent `[2][NAVIGATION_CONTEXT]` table lists the host-boundary subset only; this is the full lib-internal map):

| [PROJECT]          | [OWNS]                                                                   | [STATE]  | [REFERENCES]                             |
| ------------------ | ------------------------------------------------------------------------ | -------- | ---------------------------------------- |
| `Rasm`             | Geometry kernel + analysis algebra (`Domain`, `Analysis`, `Vectors`)     | active   | none (root); carries `Rhino.*` BCL       |
| `Rasm.Rhino`       | RhinoWIP boundary (Camera, Commands, Construction, Exchange, Blocks, UI) | active   | `Rasm`                                   |
| `Rasm.Grasshopper` | GH2 component/data/UI boundary                                           | active   | `Rasm`                                   |
| `Rasm.Materials`   | Architectural material catalogues + pure layout data                     | active   | none (zero geometry, zero host)          |
| `Rasm.AppUi`       | Unified product UI rail (Avalonia/ReactiveUI/LiveCharts/Skia)            | active   | `Rasm`, `Rasm.Grasshopper`, `Rasm.Rhino` |
| `Rasm.AppHost`     | Unified runtime platform (DI/host/telemetry coordination)                | scaffold | (planned; no `.csproj` yet)              |
| `Rasm.Compute`     | Measured-execution platform over `Rasm.Vectors`                          | scaffold | (planned; no `.csproj` yet)              |
| `Rasm.Persistence` | Local durable state (store/query/migrations/snapshots)                   | scaffold | (planned; no `.csproj` yet)              |

Invariants (verifiable, not inferable from a single file):
- **Acyclic, `Rasm`-rooted DAG.** Every reference flows toward `Rasm`. A new `Rasm.*` -> `Rasm.*` edge that is not toward the kernel, or any cycle, is a design error — collapse the concern instead.
- **Host-isolation by host.** `Rhino.*` global usings are legal ONLY in `Rasm` and `Rasm.Rhino`; `Grasshopper2.*` ONLY in `Rasm.Grasshopper`. `Rasm.Materials` (and any data-only catalogue) carries neither host namespace nor any geometry reference. A boundary project must not pull in a foreign host's globals — route cross-host composition through `Rasm.AppUi`, the only multi-host consumer.
- **Scaffold vs active.** `Rasm.AppHost`, `Rasm.Compute`, `Rasm.Persistence` have no `.csproj` and are absent from `Workspace.slnx`: they are `_ARCHITECTURE.md`/`ROADMAP.md`-stage. Do not add them to other projects' references or assume their public surface exists. Their package adoption is gated by `docs/host-libraries.md` (`[NOT_IN_GRAPH]` until a bootstrap consumer); follow each project's own `AGENTS.md` Phase 0 before writing production code.
- **Materials is sibling, not subordinate** (restated boundary from `[4]`): composition with geometry happens only in a downstream consumer that references both — never by adding a geometry reference to `Rasm.Materials`.

Adding a project to the solution is a `Workspace.slnx`/`Directory.*.props` trigger change: validate with full static (`uv run python -m tools.quality static full`), per `CLAUDE.md` §5.2.
