# [H1][CSHARP_LIBS_AGENTS]
>**Dictum:** *Library folders capture full capability behind small, powerful rails.*

<br>

`libs/csharp` contains future-facing C# libraries. Build them as reusable capability layers for downstream plugins, apps, tools, and agents that do not exist yet. No current consumer is required to justify complete domain functionality when the folder is a library boundary.

Each folder should let downstream code express intent with minimal ceremony, then receive typed results, receipts, diagnostics, projections, or native-ready output without re-learning host APIs, sequencing rules, or low-level overload surfaces.

---
## [1][LIBRARY_CONTRACT]
>**Dictum:** *Full API capture means subsuming native capability, not mirroring it.*

<br>

- Capture the entire relevant external/API domain: types, methods, lifecycle, events, sentinel values, ownership, disposal, mutation, batch behavior, failure modes, and missing capabilities.
- Expose a small public OOP boundary: one concern owner, one operation rail, compact typed atoms/policies, and few semantic entrypoints.
- Keep intelligence internal: validation, overload selection, batching, idempotency, dependency graphs, receipts, diagnostics, transforms, projections, and policy defaults.
- Add value beyond wrappers: downstream code should get more power and less boilerplate than direct API usage.
- Preserve capability while reducing surface: do not delete functionality for density; internalize it behind denser polymorphic rails.
- Use approved external libraries directly when they strengthen the rail: LanguageExt for ROP/effects, Thinktecture for closed vocabularies, MathNet for real numerical value.

---
## [2][SCAFFOLDING_PROTOCOL]
>**Dictum:** *New folders start from sibling truth and native truth.*

<br>

[IMPORTANT] Before creating or implementing a new folder:
1. Read `CLAUDE.md`, root `AGENTS.md`, this file, and the nearest folder-local `AGENTS.md`.
2. Read every existing folder in the target project directory and map owners, files, public rails, operation algebras, state records, receipts, and validation patterns.
3. Read relevant docs under `docs/external-libs`, `docs/system-api-map`, and project skills before choosing packages, system APIs, or host references.
4. Deep-read the external/native API with local truth sources: package XML/nuspec/DLL, RhinoWIP XML/decompile, GH2 XML/decompile, source docs, and repo scripts.
5. Produce a compact roadmap before production code when the folder is new or the concern boundary changes.
6. Audit the roadmap or implementation against native truth and existing folder patterns before completion.

Roadmaps must include:
- Purpose and boundary in 1-2 paragraphs.
- Source-verified API catalog with false/missing APIs called out.
- Source discipline note requiring local XML/decompile proof for every named native member.
- Proposed file architecture with durable concern ownership.
- Centralization/removal plan for duplicated logic in sibling folders.
- Value-add capabilities beyond raw API access.
- Validation commands and runtime scope.

---
## [3][SURFACE_RULES]
>**Dictum:** *Consumer power rises as public ceremony falls.*

<br>

- Use one public owner per concern. Avoid managers, helpers, services, adapters, and parallel access paths.
- Use one operation algebra when a concern has multiple actions. Keep native knobs behind semantic policies.
- Do not expose every native parameter. Collapse knob spam into typed intent, policies, and native defaults.
- Keep internals functional: `Fin`, `Validation`, `Option`, `Seq`, folds, discriminants, generated unions, and smart enums.
- Convert native return shapes at the boundary: null, false, empty arrays, exceptions, invalid geometry, and disposable ownership become typed rails.
- Design for future downstream dev experience, not only current app calls. Library boundaries should be complete enough for advanced unknown consumers.

---
## [4][VALUE_ADD]
>**Dictum:** *Library code earns its abstraction by adding capability.*

<br>

Expected value-add examples:
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
## [5][SEMANTICS]
>**Dictum:** *Names should be short enough to use and precise enough to scale.*

<br>

- Prefer 1-2 word domain names for files, types, operations, and policies.
- Name files by durable concern: `Blocks.cs`, `State.cs`, `Kernels.cs`, `Frames.cs`, `Outputs.cs`, `Archive.cs`.
- Avoid bloated names that encode implementation steps, transient plans, or every native object involved.
- Avoid weak names: `Helpers`, `Utils`, `Manager`, `Service`, `Common`, `Misc`, `Options`, `Params`.
- Keep semantics universal inside the bounded context. Boundary adapters may translate external names; internals use canonical vocabulary.
- Rename decisively when a better semantic owner exists. Do not preserve transitional aliases or compatibility shims.

---
## [6][FILE_ARCHITECTURE]
>**Dictum:** *Files represent durable ownership, not a way to hide complexity.*

<br>

- Start with the smallest file set that preserves concern ownership.
- Add a file only when it owns a durable sub-concern with multiple callers or a distinct native boundary.
- Defer speculative splits with explicit thresholds; `Kernels.cs`, `Frames.cs`, and `Outputs.cs` start only after real multi-caller pressure.
- Keep operation intent separate from native execution when both are large enough to blur each other.
- Keep state records compact and typed; avoid record sprawl for one-use parameter bags.
- Keep generated/native kernels internal unless exposing native identity adds real semantic value.
- Reuse existing folder patterns before inventing new layout.

---
## [7][VALIDATION]
>**Dictum:** *Truth beats memory, public docs, and plausible API names.*

<br>

Required evidence scales with risk:
- Repo manifests and build props for target framework, references, packages, and project classification.
- Pinned local package XML/nuspec/DLL evidence for external-library claims.
- Local RhinoWIP/GH2 XML and decompile evidence for host API claims.
- Current source reads for sibling ownership, duplicated logic, and centralization paths.
- `bash scripts/check-cs.sh check` for C# static validation unless the change is docs-only and the script reports no C#-relevant changes.
- `git diff --check` for documentation, roadmap, and instruction-surface changes.
- `git diff --check --no-index /dev/null <file>` for new untracked docs before they enter the index.
- Exact overload spelling in roadmaps; avoid compressed pseudo-signatures that imply nonexistent native overload families.

Runtime Rhino/GH verification is required only when behavior claims depend on live host mutation, load, UI, or bridge execution.
