# [CLAUDE_MANIFEST]

[CRITICAL]: Always read `libs/.planning/campaign-method.md`, ALL work in ALL sessions will ALWAYS be understood to be "planning" we are working on `.md` files and creating all "functionality/features/capabilities" strictly within `.planning/` folders, never making code files `.cs/.py/.ts` UNLESS the scope exists outside of `libs/`, all changes to tooling/external-packages/etc will be real, including anything in `tools/` and `tests/`, but all meaningfull work is in the `libs/` and we are ALWAYS doing design-doc work, code fences are treated as real, not speculative. UNDERSTAND, THE PURPOSE IS TO GIVE US MAXIMUM FREEDOM TO REBUILD ALL DESIGN DOCS/FEATURES/CAPABILITIES ROOT/GROUND-UP WITH NO HESITATION, TO ENSURE WE HAVE THE WORLD-CLASS/BLEEDING-EDGE CAPABILITY WE WANT, ALL FEATURES ADDED VIA REBUILDING, NEVER TACKING-ON FLAT CODE. That is why we are doing design-docs that are meant to be implementation ready, never assume a design-doc is finished, ALWAYS treat them as terrible/low-value, and be aggressive in improvement, and always follow the `docs/stacks/<language>/` folders for code doctrine. We are aggressively adding new external sources to `Directory.Packages.props`, `pnpm-workspace.yaml`, `pyproject.toml`, and optimizing package selection, and creating high value `.api/` folder files with full extraction. Ensure we always update the relevant language + plannning folder `README.md` for new package admissions/changes/removals, we understand that `libs/<language>/.api/` is the substrate/universally used external sources, to always use those whenever making any changes to a specific planning folder, and we treat those `.api/` files as equally as the specific `libs/<language>/<folder>/.api/` content.

Operate as a hostile, adversarial principle in a bleeding-edge polyglot monorepo; all code is always open to structural rebuilding/restructuring to optimize logic, file quality, and functionality for maximum density/complexity/richness and surface/LOC reduciton (without functionaltiy removal/weakning). Build the strongest source-backed implementation the workspace admits: newest viable language and platform features, full external-library capability, dense polymorphic owners, and root-up refactors instead of additive code. Never tolerate any fragile logic in code, always parameterized code, especially ingress/egress, NEVER couple code, or documentation in files, within this project or outside of it, everything is done to be aligned, without coupling.

Use contracts/interfaces for lib code functionality, anticipate the most advanced/complex future apps and assume that as baseline, this project is heavily focused on creating world-class `libs` content, so that hundreds of apps will be made within this project later, based on code within `libs` never re-creating logic that already exists, and always make code in `libs` to be headache free, no ceremony, knobs, param spam, assume agent coders/devs, and frame all docs, code comments, and code functionality towards that; integrating functionality/logic/capability internally and internalizing it and making it hidden to downstream consumers without reducing capability, but freeing future agent coding to not need to now all the nuance of functionality - this is CRITICAL, functionality/capability is meant to be captured/integrated fully from all sources (external libs included) within libs, without constraining future capability, but reducing future agent coding overhead to not need to know the entire API of rhino/gh2/external-packages by having everything possible already integrated in our `libs` in higher order abstractions/capabilities already capturing those needs.

## [01]-[WORKSPACE_LAW]

[IMPORTANT]:
- [ALWAYS] Use `.claude/skills/workflow-creator` when creating a workflow.
- [ALWAYS] Treat monorepo code as polymorphic, agnostic, and universal by default.
- [ALWAYS] Place every C# package on the canonical strata (KERNEL -> AEC-DOMAIN -> APP-PLATFORM -> HOST-BOUNDARY -> APP), depending strictly upward; build a host-neutral owner only where a non-Rhino runtime consumes the contract ("universal" is never host-free C#), and keep geometry/mesh/IFC to one owner per runtime meeting at the wire. The package roster and full hierarchy law are `libs/.planning/architecture.md`.
- [ALWAYS] Identify canonical object shapes, field names, semantics, and receipts that scale across packages, tools, apps, plugins, sidecars, services, and web consumers.
- [ALWAYS] Use one canonical semantic name per bounded concept; arity, filters, provider, and modality live in request shape, case, policy row, or boundary adapter, not parallel names.
- [ALWAYS] Extend the canonical owner before adding rails, public surfaces, wrappers, commands, flags, provider selectors, schemas, models, helpers, or files.
- [ALWAYS] Treat planned future consumers as real design pressure. Zero current consumers never reduces the capability bar.
- [ALWAYS] Capture host APIs, external packages, generated API evidence, and platform quirks into focused local owners so downstream code composes capability instead of re-learning provider surfaces.
- [ALWAYS] Keep boundary mapping at the edge; internal code uses canonical names and shapes.
- [NEVER] Split one concern across parallel objects, services, error rails, command families, or compatibility shims.
- [NEVER] Create operation families such as `Get`, `GetMany`, `GetBy<Key>`, `List`, or `Search` for one concept when one polymorphic operation can discriminate by input value.
- [NEVER] Preserve stale APIs, wrappers, aliases, or old-baseline caveats when a root-up collapse improves the system.

## [02]-[REQUIRED_STANDARDS]

Use the route-owned standard for the file being edited:

| [INDEX] | [FILE_TYPE]                          | [ROUTE]              |
| :-----: | ------------------------------------ | -------------------- |
|  [01]   | TypeScript (`.ts`, `.tsx`)           | `coding-ts`          |
|  [02]   | C# production (`.cs`)                | `docs/stacks/csharp` |
|  [03]   | C# tests (`.spec.cs`)                | `testing-cs`         |
|  [04]   | Runtime scenarios (`Scenarios/*.cs`) | `testing-cs`         |
|  [05]   | Python (`.py`)                       | `docs/stacks/python` |
|  [06]   | Bash/sh (`.sh`, `.bash`)             | `coding-bash`        |
|  [07]   | SQL (`.sql`)                         | `coding-pg`          |

`docs/stacks/csharp` is the route-owned C# production standard. C# source composes `docs/stacks/csharp/README.md`, `language.md`, `shapes.md`, `surfaces-and-dispatch.md`, `rails-and-effects.md`, `boundaries.md`, `algorithms.md`, and `system-apis.md`. Specialized C# domains route through `docs/stacks/csharp/domain/README.md`.

`docs/stacks/python` is the route-owned Python production standard. Python source composes `docs/stacks/python/README.md`, `language.md`, `shapes.md`, `surfaces-and-dispatch.md`, `rails-and-effects.md`, `concurrency.md`, `boundaries.md`, `algorithms.md`, `system-apis.md`, and `runtime.md`. Numerical and scientific computing routes through `docs/stacks/python/algorithms.md` plus the root Python doctrine index.

## [03]-[DEPENDENCY_POLICY]

[IMPORTANT]: External libraries, manifests, and host APIs are implementation surfaces.
- [ALWAYS] Treat dependencies declared in `pyproject.toml`, `pnpm-workspace.yaml`, `Directory.Packages.props`, project files, lockfiles, and equivalent manifests as first-class material.
- [ALWAYS] Mine admitted packages to their full useful capability before writing local kernels.
- [ALWAYS] Prefer ecosystem libraries that already own the domain concern over lower-level reinvention.
- [ALWAYS] Internalize external capability into canonical local owners organized by domain, axis, row, case, receipt, or rail.
- [ALWAYS] Keep central package/version/tool ownership centralized in the one owning manifest or tool configuration — no per-package `pyproject.toml`, `package.json`, or `*.props`; assume the newest stable release and pin a package only when it is not yet compatible, removing the pin when compatibility lands.
- [ALWAYS] Keep Python dependencies in root `pyproject.toml` as lean unpinned package names by default; add bounds or `python_version` markers only when resolver evidence requires them, prefer the newest viable release, remove constraints as compatibility lands, and keep wheel/floor/gate rationale out of Python docs, design docs, `.api` files, and comments.
- [ALWAYS] Keep C# MSBuild, NuGet, and `.csproj` manifests label-grouped by owner, sorted within coherent clusters, and limited to one-line maintenance comments.
- [ALWAYS] Put shared C# substrate API catalogues under `libs/csharp/.api/`; package `.api/` folders carry domain catalogues and folder-specific overlays.
- [NEVER] Hand-roll functionality provided by admitted dependencies.
- [NEVER] Create thin wrappers that rename, forward, or partially expose external APIs without adding domain value.
- [NEVER] Encode package versions, provider caveats, or command catalogs outside the owning manifest, package charter, README, or tool owner.

## [04]-[IMPLEMENTATION_CONSTRAINTS]

[CRITICAL]:
- [NEVER] Use weak, unbounded, or erased types where the language can express the domain precisely.
- [NEVER] Use exception-style control flow in domain logic; use typed error rails and the required route's recovery patterns.
- [NEVER] Use imperative branching when a bounded vocabulary, dispatch table, generated switch, match, fold, or monadic rail can own the variation.
- [NEVER] Use mutable accumulation for domain transforms; use immutable folds, projections, collection combinators, or effect/resource pipelines.
- [NEVER] Proliferate schemas, structs, models, branded types, records, classes, aliases, or DTOs for the same concept.
- [NEVER] Create helper/utility files or functions for single-caller or thin indirection.
- [NEVER] Extract code to new files to reduce LOC. Densify in place through polymorphism, folds, generated owners, and table-driven dispatch.
- [NEVER] Delete functionality to satisfy a density or LOC signal. Preserve capability through denser owners.
- [NEVER] Add comments that carry task, session, subagent, review-label, proof, history, or process narration.

[IMPORTANT]:
- [ALWAYS] Frame all comments and prose within code files, code-fences and docs in general to be AGENT FIRST/ONLY/FOCUSED, the only useful comments/prose are those that IMPLICITLY guide agentic coding/management/maintenance.
- [ALWAYS] Collapse related variants into one polymorphic surface before adding entrypoints.
- [ALWAYS] Drive logic with data, bounded vocabularies, discriminants, table rows, and reusable projections.
- [ALWAYS] Co-locate domain logic with its owner instead of scattering it into generic support files.
- [ALWAYS] Collapse repeated mutation/status/count construction into one fact stream with slot/kind metadata when three or more buckets share construction.
- [ALWAYS] Keep typed algorithm receipts when fields carry route, status, sampling, solver, spectral, mesh, extraction, benchmark, or host evidence.
- [ALWAYS] Treat analyzer diagnostics as architecture pressure: fix true positives, refine false positives, and avoid suppressions that add ceremony without improving correctness.

## [05]-[BEHAVIOR]

[IMPORTANT]:
- [ALWAYS] Tools over internal knowledge: read files, search code, verify assumptions through source, manifests, docs, and tool output.
- [ALWAYS] Parallelize independent searches, reads, and checks.
- [ALWAYS] Use bounded subagents for independent exploration, research, verification, and disjoint implementation.

## [06]-[OWNER_ROUTING]

[IMPORTANT]:
- [ALWAYS] Resolve external library, framework, SDK, or host-API usage through `Context7` before internalizing into a canonical owner: `Context7` also indexes this repo's own packages, so resolve internal API shape through it before opening source, while `uv run python -m tools.assay api` answers which members verifiably exist locally; verified-local wins on conflict. The web/docs research selection law is the user-global doctrine, not restated here.
- [ALWAYS] Dependency graph facts live in manifests, package-manager configuration, lockfiles, project files, and the tool owner that consumes them.
- [ALWAYS] Quality routes are selected by the owning language/tool surface for the changed files. Root policy owns intent, not command catalogs.
- [ALWAYS] Keep static analysis, tests, runtime scenarios, metadata lookup, formatting, restore, and generated-contract checks orthogonal.
- [ALWAYS] For docs-only, catalog-only, read-only, declaration-order, move-only, source-comment-only, docstring-only, XML-doc-only, and TSDoc-only work, use text, path, table, link, owner, and preservation checks unless the user requests an executable quality rail.
- [NEVER] Add package versions, tool commands, hardcoded project targets, or suite paths to root policy when a manifest, README, repo tool, or language owner carries the exact command.
- [ALWAYS] LSP owns live navigation and post-edit diagnostics over local source.
- [ALWAYS] Invoke the repo operator as `uv run python -m tools.assay ...`; bare `assay ...` is only valid when `command -v assay` proves a local wrapper exists.
- [ALWAYS] `uv run python -m tools.assay api` owns external-artifact decompile/reflection over host DLLs, NuGet packages, installed Python distributions, and `node_modules` declarations.
- [ALWAYS] Route live NuGet feed intelligence through the `nuget` MCP; `assay api` answers which members verifiably exist in the restored assembly, and verified-local wins on conflict. Apply a version change by hand-editing the grouped `Directory.Packages.props` (never `dotnet add`), confirm with `dotnet restore`/`dotnet nuget why`, and drive folder-wide modernization through the `survey-packages`/`survey-gaps` workflows; the standalone `nuget.commandline` CLI is unused.
- [ALWAYS] Treat `Rasm.Bim` as the sole IFC semantic authority: C# owns the `BimModel`/`BimWire`/`ElementSet`/`IfcSemanticModel` graph (GeometryGym, in-process). The `ifc` skill and `ifc` MCP own only live read-only inspection, GLB tessellation (`IfcConvert` keyed by the `XxHash128` content key, cache-checked against the `Rasm.Persistence` artifact index), and the IDS oracle (`ifctester` → `IdsVerdict` rows feeding `IdsAudit.Reconcile`); they never re-author the semantic model, re-implement geometry the kernel owns, or write Psets/Qtos as the system of record.
- [ALWAYS] The inherited local CodeRabbit -> Greptile review runs only after the owner-scoped `uv run python -m tools.assay` gate passes for the changed files; PR-level review and automation route through the repo `gh`/`mcp__github__*` owners.
- [ALWAYS] `uv run python -m tools.assay code` owns structural/pattern search over ast-grep metavariables, tree-sitter queries, and CI artifacts; prefer LSP for plain single-symbol navigation.
- [ALWAYS] `uv run python -m tools.assay static/test/bridge/package` own gating quality rails and mutation routes. LSP is read-only.
- [ALWAYS] `uv run python -m tools.assay provision` owns Rasm campaign provisioning through sanitized `ProvisionRun` evidence; direct `forge-provision`, `forge-scientific-env`, Docker/Compose, direct database shells, cleanup, and diagnostic JSON calls are Forge-level debugging, not Rasm campaign surfaces.
- [ALWAYS] Treat bridge proof as `EvidenceCertificate` plus reviewed `ReferenceEvidence`; MCP exploration can promote invariants into scenarios, but never substitutes for certificate-backed verify.

## [07]-[DOCUMENTATION_AND_OUTPUT]

[IMPORTANT]:
- [ALWAYS] Use `backticks` for file paths, symbols, and CLI commands.
- [ALWAYS] Keep responses actionable and lead with what changed.
- [ALWAYS] Treat durable docs, prompts, standards, skills, examples, and templates as agent-facing declarative law.
- [NEVER] Add provenance blocks, research-origin sections, source tails, freshness disclaimers, defensive version caveats, checklist tails, or report framing to durable docs.
- [NEVER] Tell a prompt recipient to read root instructions, load skills, follow instruction files, use known tools, or run standard checks when those obligations already come from active instructions.
- [NEVER] Restate quality ladders, command catalogs, skill loading, load-order ladders, or system/developer rules in generated artifacts.

Plans are decision-complete blueprints. Include context, critical files, implementation approach, acceptance signals, and assumptions only when they change execution. Do not include workflow narration, alternatives considered, command catalogs, or boilerplate closure.

## [08]-[FILE_ORGANIZATION]

[IMPORTANT] Section separators: language comment marker + space + `---` + bracketed UPPERCASE snake label with no internal spaces + dash fill to the established language width.

```typescript
// --- [TYPES] ---------------------------------------------------------------------------

// --- [SUBSECTION]
```

```python
# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [SUBSECTION]
```

```csharp
// --- [SERVICES] ------------------------------------------------------------------------

// --- [SUBSECTION]
```

Canonical order, omitting unused sections: `TYPES` -> `CONSTANTS` -> `MODELS` -> `ERRORS` -> `SERVICES` -> `OPERATIONS` -> `COMPOSITION` -> `EXPORTS`.

`[RUNTIME_PRELUDE]` may precede the canonical order only for imports, shebangs, strict modes, session setup, and load gates.

- `[TYPES]`: type aliases, inferred types, protocols/interfaces, enums, discriminated unions, generated algebraic owners, value-object declarations.
- `[CONSTANTS]`: dependency-free immutable anchors, caps, suffixes, primitive policies, schedules, and static literals.
- `[MODELS]`: runtime schemas, records/classes, value objects, DTOs, table/domain models, receipts, result carriers.
- `[ERRORS]`: typed error rails, tagged failures, domain failure policies.
- `[SERVICES]`: service contracts, dependency surfaces, application/service classes.
- `[OPERATIONS]`: pure transforms, effect/result pipelines, algorithms, repository operations.
- `[COMPOSITION]`: layers, decorators, dependency wiring, middleware, runtime composition roots.
- `[EXPORTS]`: named exports, `__all__`, or language-equivalent public surface declarations.

[IMPORTANT]:
- [ALWAYS] Apply ordering as `section` -> `owner block` -> `runtime/declaration dependency` -> `semantic rank` -> `kind` -> `smaller-to-larger` -> `alphabetical`.
- [ALWAYS] Prefer concept discovery order from stable declarations to composition: vocabulary, constants, models, failures, services, operations, wiring, exports.
- [ALWAYS] Treat one generated type, smart enum, value object, schema/model family, wire model family, kernel, registry, catalog, table, dispatcher, query family, or composition root as an owner block; sort inside the owner instead of flattening its members into unrelated top-level sections.
- [ALWAYS] Keep dependency clusters intact when a declaration must follow the symbol it imports, inspects, derives from, registers, decodes, wraps, initializes, traps, migrates, or composes.
- [ALWAYS] Use smaller-to-larger only after ownership and dependency order are satisfied: one-line anchors before multi-line policies, simple axes before rich models, leaf operations before orchestration.
- [ALWAYS] Use alphabetical order only for equivalent declarations with the same owner, kind, dependency level, and semantic rank.
- [ALWAYS] Treat kind as an owner-local tiebreaker, not a new section: type/member family precedes accessibility, size, and alphabetical order only when ownership, dependency, and semantic rank are equivalent.
- [ALWAYS] For equivalent same-owner members, prefer public contract before internal extension before private implementation unless static construction, generated semantics, or read-before-use dependency requires another order.
- [ALWAYS] Keep semantically ordered sequences in domain order: severity, lifecycle, routing, key, protocol, generated-case, table-row, migration-step, and public API order are load-bearing when the owner defines them.
- [ALWAYS] Co-locate tightly coupled symbols when strict section order obscures ownership or violates language/runtime constraints.
- [ALWAYS] Insert domain extensions immediately after the closest core section, using precise labels only when they name real ownership: `[TABLES]`, `[BOUNDARIES]`, `[REPOSITORIES]`, `[GROUPS]`, `[MIDDLEWARE]`, `[INDEXES]`, `[POLICIES]`, or `[ENTRY]`.
- [ALWAYS] Use nested algorithm subsection labels inside large kernels only when they identify a real operation family, such as `[VECTOR_HEAT]` or `[NORMAL_ESTIMATION]`.
- [ALWAYS] Keep internal cache keys, memo tables, mutable registries, and algorithm state records with the operation, kernel, or runtime owner that reads and mutates them.
- [ALWAYS] Treat logger handles, provider handles, and dependency-backed runtime capabilities as `[SERVICES]`, not immutable anchors.
- [NEVER] Put derived codecs, decoders, registries, lookup tables, generated maps, dispatch rows, callable row catalogs, mutable memo tables, or DDL-dependent objects in top-level `[CONSTANTS]` when they depend on later models, functions, owners, runtime state, or migration state; place them in the owning later section or a precise extension such as `[TABLES]` or `[COMPOSITION]`.
- [NEVER] Split source-generated owners, delegate-backed enum behavior, validation partials, private operation-local state, resource/disposal boundaries, dispatch tables, SQL invariants, or migration units to satisfy mechanical section order.
- [NEVER] Rename recurring categories per file; use canonical labels unless a domain extension is materially clearer.
- [NEVER] Use alias or drift labels that merely rename core categories or hide complexity: `SCHEMA`, `FUNCTIONS`, `LAYERS`, `IMPORTS`, `INTERFACES`, `ENUMS`, `DTO`, `QUERIES`, `HELPERS`, `UTILS`, `COMMON`, `MISC`.

Language overlays refine the canonical order by runtime semantics:
- C#: `[Union]`, `[SmartEnum]`, `[ValueObject]`, generated case families, static entries, delegate partials, validation partials, factories, and projections stay inside the declaring owner block. Preserve generated-case and smart-enum semantic order, with one generated case or static entry per physical declaration line unless a generator or runtime contract requires grouping. Static construction order inside a type is semantic when later fields derive from earlier fields. Static kernels, projectors, acceptors, and extension folds are `[OPERATIONS]` unless they own an actual dependency or service boundary. Inside a section, prefer attributes/delegates/marker types, enums/smart enums, readonly structs/records/value objects, records/classes/services, then owner-local private types when all earlier ordering constraints are equal. Inside a C# owner block, prefer generated/static dependency entries, fields/state, constructors/factories, properties, public operations, explicit boundary adapters, internal operations, then private kernels/implementation details.
- Python: imports, `TYPE_CHECKING`, and import-time gates precede ordinary sections. Runtime decoders, encoders, registries, and tables follow the models/functions they inspect because module-level assignments execute immediately and runtime annotation consumers such as `msgspec` and `beartype` resolve real objects. `Annotated` validator functions may use `[BOUNDARIES]` between immutable constants and dependent aliases when the aliases must reference the real validator object.
- TypeScript: side-effect/value imports preserve runtime order, and `import type`/`export type` stay explicit. Runtime schemas/classes are `[MODELS]`, `Effect.Service` owners are `[SERVICES]`, `Layer`/runtime wiring is `[COMPOSITION]`, and catalog or registry rows that reference functions/classes stay after their referenced owners.
- Bash: shebang, ShellCheck directives, `set`/`shopt`, and environment/path gates are `[RUNTIME_PRELUDE]`; `readonly` values are `[CONSTANTS]`; `declare -Ar` maps are `[TABLES]`; traps, dispatch, source guards, and `_main` are late `[COMPOSITION]` or `[ENTRY]`.
- PostgreSQL: extensions, schemas, and search-path guards are `[RUNTIME_PRELUDE]`; domains and types are `[TYPES]`; tables, constraints, generated columns, and partitions are `[MODELS]`; functions split by service boundary or query operation; indexes, triggers, row-level security, and policies are `[COMPOSITION]`; grants and comments are late `[EXPORTS]`.
- YAML/YML: manifests and configuration files are data surfaces, not sectioned source; do not add code-section dividers. Preserve sequence order, anchors, comments, duplicate-key constraints, schema-defined key order, and executable order. Mapping-key reorder is presentation-only unless the owning tool documents order-dependent behavior; otherwise prefer required identity/version fields before optional metadata, resources, executable units, outputs, and publication/export fields.
