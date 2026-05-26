# [H1][CSHARP_LIBS_AGENTS]
>**Dictum:** *Library folders capture full capability behind small, powerful rails.*

<br>

`libs/csharp` contains future-facing C# libraries. Build them as reusable capability layers for downstream plugins, apps, tools, and agents that do not exist yet. No current consumer is required to justify complete domain functionality when the folder is a library boundary.

This file documents lib-scope deltas over the root `AGENTS.md` and `CLAUDE.md`. Library contract (capture native capability, expose small OOP boundary, keep intelligence internal), surface preference (deep polymorphic over many loose), greenfield posture, and quality-gate validation are owned by those upstream documents — do not restate.

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
