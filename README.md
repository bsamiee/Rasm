# [RASM_WORKSPACE]

Rasm carries a platform tier and a product tier: `libs/` holds independently adoptable library estates, and every app, plugin, and service composes them exactly as it takes an external package. Capability lands in the platform first — polymorphic, parameterized, and free of app coupling in shape, naming, or structure — so a product shell declares intent, binds host edges, and emits output.

Estates carry the domains they hold: C# the host-bound AEC domain, Python the host-free science, compute, data, geometry, and exchange domain, TypeScript the host-free web, edge, backend, and deployment domain. Each originates, operates, and deploys through its own toolchain, so a single-language application ships with no peer branch present, and a domain widens at its owning branch under one admission ladder.

Cross-language contracts split by atomic case: `tests/contracts/manifest.json` binds each case to its authority class, exact definition, actors, and proof, and that manifest mints the class vocabulary a crossing elects. `libs/.planning/ARCHITECTURE.md` owns the stratification law, consumption model, design language, admission ladder, and the class boundary each authority holds.

Each `libs/<language>/contracts/` folder is a permanent generated SDK and import boundary, not planning scaffolding or a second schema owner. Estate Protobuf and publisher sources stay in `tests/contracts/`; generated structural repetition is accepted output, while hand-authored wire mirrors beside it are defects.

Every `libs/` package is an independently versioned dependency an unrelated application takes exactly as it takes any external package. One branch serves unrelated consumers at once — single-tenant and multi-tenant, in-host and headless, sidecar, companion, standalone, CLI, service and edge — a package assumes no consumer, or sibling set. Deployment shape arrives as data on the axis roster the composition root supplies, and a package unable to serve an axis value refuses at admission with typed evidence.

All `libs/` content powers future apps of every kind — in-host on Rhino 9/WIP and GH2, standalone, remote, and web. Libraries capture host and provider APIs whole and internalize them behind higher-order abstractions, so an agent composes parameterized, polymorphic capability instead of learning hundreds of provider calls. Every folder folds one polymorphic entry per bounded concept, so intelligence rides inside the owner and an app built on it carries neither hand-rolled provider code nor knob and ceremony spam.

Review depth: `.coderabbit.yaml`, `.greptile/`, and `.macroscope/` carry the repo's reviewer tone, scope maps, and doctrine-derived guidance; review behavior is tuned there and never duplicated into docs.

## [01]-[HOSTS]

Each host row is `host` axis capability the owning branch supplies; a new host lands as one host-boundary package under `libs/.planning/ARCHITECTURE.md` `[12]-[ADMISSION]`, and the domain packages gain descriptor rows rather than the host's name.

[CRITICAL]: Rhino 9/WIP on macOS, the WIP lane IS the Rhino 9 target; NO GH1 `.gha` OR Rhino 8, Windows target.

| [INDEX] | [HOST]         | [BRANCH] | [SURFACE]                                                           |
| :-----: | :------------- | :------- | :------------------------------------------------------------------ |
|  [01]   | `Rhino 9/WIP`  | C#       | `net10.0` hosted plugins, Yak package output for Mac package roots. |
|  [02]   | `Grasshopper2` | C#       | GH2 product surfaces; shared C# projects target `net10.0`.          |

## [02]-[TOPOLOGY]

| [INDEX] | [SURFACE]            | [OWNER]                  | [ROLE]                                                                       |
| :-----: | :------------------- | :----------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `libs/csharp`        | C# library suite         | Rhino 9/WIP and GH2-aware AEC and host-boundary packages.                    |
|  [02]   | `libs/python`        | Python library suite     | Host-free science, data, geometry, artifacts, and isolated native providers. |
|  [03]   | `libs/typescript`    | TypeScript library suite | Host-free web, edge, runtime, persistence, security, UI, and deployment.     |
|  [04]   | `tests`              | Polyglot proof surface   | C#, Python, and TypeScript suites plus the cross-language contract corpus.   |
|  [05]   | `tools/assay`        | Typed operator           | Typed evidence rails across every claim; the CLI `--help` owns the roster.   |
|  [06]   | `tools/rhino-bridge` | Live Rhino owner         | Host lifecycle, scenario execution, cargo, spool, protocol, and evidence.    |
|  [07]   | `tools/cs-analyzer`  | C# architecture pressure | Local Roslyn diagnostics for repeated source-shape laws.                     |
|  [08]   | `tools/biome`        | TS architecture pressure | Promoted GritQL lint rules the root `biome.json` registers at error.         |
|  [09]   | `tools/yak`          | Package metadata         | Tracked Yak manifests and icons for package roots.                           |
|  [10]   | `docs`               | Durable doctrine         | Agent-facing standards, host notes, stack doctrine, and reference material.  |

## [03]-[LIBRARY_OWNERS]

`libs/.planning/ARCHITECTURE.md` owns the stratification law, the consumption axis roster, the universal-vs-branch-local rule, the design language every estate shares, and the admission ladder a new row, adapter, page, sub-domain, package, host boundary, or branch climbs. Each branch `ARCHITECTURE.md` owns its package roster, those packages' charters, and their reference direction; `libs/.planning/README.md` owns the branch roles and the planning corpus standard.

## [04]-[HOST_RUNTIME]

RhinoWIP and GH2 assemblies resolve through shared build properties, not per-project references. Host assemblies stay outside package output; those shared build properties own the roster.

Live host evidence flows through the bridge plugin. Scenarios are source-only diagnostics under the relevant test or library mirror path; they do not carry `#r`, `#load`, or absolute build-output references. Its rail owns host-filtered reference projection, fresh artifact refs, scenario name injection, capture path injection, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity evidence.

Plugin projects classify themselves in their project files; build behavior does not depend on product names. Package membership is evaluated from MSBuild properties and package metadata.

## [05]-[TOOL_OWNERS]

Every root, tool, and library routes generated output through an owned store: `.artifacts`, `.cache`, package staging roots, scoped report directories, or owner-declared state files. Root scratch output is a defect that repairs at the writing tool's own configuration, never through a wrapper that relocates the write after the fact.

- `tools/assay` runs every quality claim on its own rail: its registry mints command shape and one JSON `Envelope` carries the verdict a caller reads.
- `tools/assay provision` envelopes the machine estate as sanitized evidence; `Parametric_Forge` owns composition, executables, and credential policy.
- `provision` admits Forge schema-v3 JSON alone and projects one `ProvisionRun`; raw `forge-provision` calls and Compose diagnostics stay Forge-side.
- `tools/rhino-bridge` owns every live RhinoWIP step launch to quit, so no library, suite, or prompt re-spells endpoint, cargo, or spool choreography.
- `tools/cs-analyzer` admits a C# shape law once a source diff proves the rule cuts surface while preserving behavior.
- `tools/biome` spells the branch doctrine's mechanical shape laws, and `tests/typescript/_architecture` proves roster, spans, and severity on disk.
- `tools/yak` stores package metadata alone; Assay owns staging, deployment, publish, artifact roots, and host refresh.
- `Parametric_Forge` `services/topology.ts` owns GitHub settings as `@pulumi/github` rows proved at driver preview; `secrets` owns credential custody.

## [06]-[PLANNING_AND_EVIDENCE]

New foundational libraries earn a planning campaign before production source wherever scope runs broad or future-consumer-facing. `libs/.planning/` owns every planning law — topology, the campaign loop and its quality bar, the authoring standard, and the target index — and each campaign closes by collapsing surviving capability into owner ledgers, row/case/policy axes, and decision-complete pages.

## [07]-[DEVELOPMENT_MODEL]

Code starts from the deepest reusable library owner that can absorb the capability. App and plugin layers declare product intent, ports, and output bindings; they do not reimplement geometry kernels, host lifecycles, GH2 wiring, runtime composition, UI primitives, compute orchestration, persistence, packaging, or evidence capture.

External libraries, host APIs, package catalogs, and generated evidence are implementation material. Provider capability enters as a local row, case, delegate column, receipt field, or boundary adapter on the owning surface. Thin wrappers, provider-branded public shapes, command spam, flag spam, and app-local copies are defects.
