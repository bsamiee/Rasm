# [RASM_WORKSPACE]

Rasm carries a platform tier and a product tier: `libs/` holds independently adoptable library estates, and every app, plugin, and service composes them exactly as it takes an external package. Capability lands in the platform first — polymorphic, parameterized, and free of app coupling in shape, naming, or structure — so a product shell declares intent, binds host edges, and emits output.

Estates carry the domains they hold: C# the host-bound AEC domain, Python the host-free science, compute, data, geometry, and exchange domain, TypeScript the host-free web, edge, backend, and deployment domain. Each originates, operates, and deploys through its own toolchain, so a single-language application ships with no peer branch present, and a domain widens at its owning branch under one admission ladder.

Cross-language contracts split in two classes. Every branch mints an infrastructure contract from its own inputs, and `tests/contracts/` defines the shape and proves parity across the mints. One producer named by the capability it holds emits a domain contract, and every peer decodes it. `tests/contracts/MANIFEST.md` binds each contract to its class; `libs/.planning/ARCHITECTURE.md` owns the stratification law, the consumption model, the design language, the admission ladder, and that class boundary.

Every `libs/` package is an independently versioned dependency an unrelated application takes exactly as it takes any external package. One branch serves N count of unrelated consumers at once — single-tenant and multi-tenant, in-host and headless, sidecar, companion, standalone, CLI, service and edge — a package assumes no consumer, or sibling set. Deployment shape arrives as data on the axis roster the composition root supplies, and a package unable to serve an axis value refuses at admission with typed evidence.

All `libs/` content powers future apps of every kind — in-host on Rhino 9/WIP and GH2, standalone, remote, and web. Libraries own the core logic and expose it, designed to be agent-first: host and external-package APIs are fully captured and internalized behind higher-order abstractions, so an agent composes parameterized, polymorphic capability instead of learning hundreds of provider calls, and builds feature-rich apps with minimal code, boilerplate, or ceremony. Every folder is designed around one polymorphic entry per bounded concept — no knob or ceremony spam; intelligence is internalized, multi-modal, and automatic for agents to write minimal hand-roll code or misuse `libs/` capability.

Review depth: `.coderabbit.yaml`, `.greptile/`, and `.macroscope/` carry the repo's reviewer tone, scope maps, and doctrine-derived guidance; review behavior is tuned there and never duplicated into docs.

## [01]-[HOSTS]

Each host row is `host` axis capability the owning branch supplies; a new host lands as one host-boundary package under `libs/.planning/ARCHITECTURE.md` `[12]-[ADMISSION]`, and the domain packages gain descriptor rows rather than the host's name.

[CRITICAL]: Rhino 9/WIP on macOS, the WIP lane IS the Rhino 9 target; NO GH1 `.gha` OR Rhino 8, Windows target.

| [INDEX] | [HOST]         | [BRANCH] | [SURFACE]                                                           |
| :-----: | :------------- | :------- | :------------------------------------------------------------------ |
|  [01]   | Rhino 9/WIP    | C#       | `net10.0` hosted plugins, Yak package output for Mac package roots. |
|  [02]   | `Grasshopper2` | C#       | GH2 product surfaces; shared C# projects target `net10.0`.          |

## [02]-[TOPOLOGY]

| [INDEX] | [SURFACE]            | [OWNER]                  | [ROLE]                                                                        |
| :-----: | :------------------- | :----------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `libs/csharp`        | C# library suite         | Rhino 9/WIP and GH2-aware AEC and host-boundary packages.                     |
|  [02]   | `libs/python`        | Python library suite     | Host-free science, compute, data, geometry, IFC, and artifact packages.       |
|  [03]   | `libs/typescript`    | TypeScript library suite | Host-free web, edge, runtime, persistence, security, UI, and deployment.      |
|  [04]   | `tests`              | Polyglot proof surface   | C#, Python, and TypeScript suites plus the cross-language contract corpus.    |
|  [05]   | `tools/assay`        | Typed operator           | Static, test, bridge, package, code, docs, provision, and API evidence rails. |
|  [06]   | `tools/rhino-bridge` | Live Rhino owner         | Host lifecycle, scenario execution, cargo, spool, protocol, and evidence.     |
|  [07]   | `tools/cs-analyzer`  | C# architecture pressure | Local Roslyn diagnostics for repeated source-shape laws.                      |
|  [08]   | `tools/biome`        | TS architecture pressure | Promoted GritQL lint rules the root `biome.json` registers at error.          |
|  [09]   | `tools/yak`          | Package metadata         | Tracked Yak manifests and icons for package roots.                            |
|  [10]   | `docs`               | Durable doctrine         | Agent-facing standards, host notes, stack doctrine, and reference material.   |

## [03]-[LIBRARY_OWNERS]

`libs/.planning/ARCHITECTURE.md` owns the stratification law, the consumption axis roster, the universal-vs-branch-local rule, the design language every estate shares, and the admission ladder a new row, adapter, page, sub-domain, package, host boundary, or branch climbs. Each branch `ARCHITECTURE.md` owns its package roster, those packages' charters, and their reference direction; `libs/.planning/README.md` owns the branch roles and the planning corpus standard.

## [04]-[HOST_RUNTIME]

RhinoWIP and GH2 assemblies resolve through shared build properties, not per-project references. Host assemblies stay outside package output: `RhinoCommon`, `Rhino.UI`, `Rhino.Runtime.Code`, `Grasshopper2`, `GrasshopperIO`, `Eto`, `Microsoft.macOS`, and RhinoWIP-hosted drawing assemblies.

Live host evidence flows through the bridge plugin. Scenarios are source-only diagnostics under the relevant test or library mirror path; they do not carry `#r`, `#load`, or absolute build-output references. Its rail owns host-filtered reference projection, fresh artifact refs, scenario name injection, capture path injection, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity evidence.

Plugin projects classify themselves in their project files; build behavior does not depend on product names. Package membership is evaluated from MSBuild properties and package metadata.

## [05]-[TOOL_OWNERS]

Every root, tool, and library routes generated output through an owned store: `.artifacts`, `.cache`, package staging roots, scoped report directories, or owner-declared state files. Root scratch output is a defect that repairs at the writing tool's own configuration, never through a wrapper that relocates the write after the fact.

- `tools/assay` is the repo operator: its registry owns public command shape, its envelopes own result interpretation, and it returns typed reports, artifacts, faults, routing notes, and evidence rather than stderr or human-scanned logs. Structural search, API catalogs, static analysis, tests, bridge orchestration, package work, and docs checks route through the relevant Assay rail.
- `tools/assay provision` is the Rasm evidence envelope for Forge-provisioned server and native campaign facts. `Parametric_Forge` owns service composition, installed provisioning and scientific executables, Docker/Compose assets, credential and port policy, and native exports; Rasm owns the sanitized `ProvisionRun` facts, manifests, locks, `.api` catalogues, and evidence that consume those machine surfaces.
- Rasm agents invoke Assay as `uv run python -m tools.assay provision <verb>`; the registry and per-claim `--help` own the verb census. Assay accepts Forge schema-v3 JSON only and projects sanitized `ProvisionRun` evidence. Direct `forge-provision`, `psql`, `paths`, `prune`, `self-test`, Docker/Compose, cleanup, and diagnostic JSON remain Forge-level debugging surfaces.
- `tools/rhino-bridge` owns live RhinoWIP execution. Contract owns protocol and fault shapes; Supervisor owns host lifecycle and folds; Stub stays dependency-zero; Shell owns in-host RPC/admission; Cargo owns scenarios and capture evidence. Libraries and prompts do not recreate launch, endpoint, quit, cargo, or spool choreography.
- `tools/cs-analyzer` captures repeated C# shape laws after source diffs prove the rule reduces surface while preserving behavior. Analyzer diagnostics are architecture pressure, not suppression targets.
- `tools/biome` carries the promoted GritQL rule roster — the TypeScript doctrine's mechanical shape laws. One root `biome.json` registers every rule at error, and the `tests/typescript/_architecture` gauge proves the roster, its firing spans, and its severity against disk.
- `tools/yak` stores package metadata only. Package staging, deployment, publish, artifact roots, and host refresh are Assay/package responsibilities.
- GitHub repository settings — merge hygiene, rulesets, review automation — are settings-as-code `@pulumi/github` rows in Parametric_Forge `services/topology.ts`; the services driver preview is the verification surface, never the GitHub UI. Agent secrets arrive through the canonical `.claude/hooks/setup-env.sh` Doppler rail; custody law is the `secrets` skill.

## [06]-[PLANNING_AND_EVIDENCE]

New foundational libraries use planning campaigns before production source when scope is broad or future-consumer-facing. Planning law lives in `libs/.planning/` — `ARCHITECTURE.md` (the topology), `campaign-method.md` (the loop, the bar, the agent-role law), `README.md` (the authoring standard), `planning-targets.md` (every planning surface). Each campaign makes infra truth honest, captures manifests and lockfiles, extracts API catalogs through repo evidence rails, runs research and adversarial passes before authoring, enumerates isolated and in-concert capability across modalities, then collapses surviving capability into owner ledgers, row/case/policy axes, and decision-complete pages.

## [07]-[DEVELOPMENT_MODEL]

Code starts from the deepest reusable library owner that can absorb the capability. App and plugin layers declare product intent, ports, and output bindings; they do not reimplement geometry kernels, host lifecycles, GH2 wiring, runtime composition, UI primitives, compute orchestration, persistence, packaging, or evidence capture.

External libraries, host APIs, package catalogs, and generated evidence are implementation material. Provider capability enters as a local row, case, delegate column, receipt field, or boundary adapter on the owning surface. Thin wrappers, provider-branded public shapes, command spam, flag spam, and app-local copies are defects.
