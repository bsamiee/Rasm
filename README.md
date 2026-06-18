# [RASM_WORKSPACE]

Rasm is a RhinoWIP and Grasshopper2 monorepo for product-neutral AEC libraries, host-boundary packages, agent-operated tooling, and downstream Rhino/GH2 products. Apps and plugins are consumers; shared capability lands first in libraries that absorb geometry, host, runtime, UI, compute, persistence, packaging, and evidence concerns behind canonical owners.

Rasm is one tri-language AEC platform organized into strict strata: the C# branch is the Rhino9(WIP)/GH2-aware producer (the geometry kernel, the host-neutral AEC-domain, the app-platform, and the host boundaries); Python is the host-free science/compute/data/geometry/IFC companion; TypeScript is the host-free web/edge platform. The three branches couple only at the wire. The canonical hierarchy — the strata, the dependency direction, the universal-vs-Rhino-capture rule, and the geometry/mesh/IFC flow — is owned by `libs/.planning/architecture.md`.

## [1]-[TARGET]

- RhinoWIP on macOS.
- `net10.0` for hosted plugins and shared C# projects.
- Grasshopper product surfaces through `Grasshopper2`.
- Yak package output for Mac package roots.
- Polyglot workspace roots: C# libraries and plugins, Python tooling, TypeScript/web surfaces and generated wire consumers.
- Out of scope: GH1 `.gha`, Rhino 8 target, Windows package target, RhinoCode publishing path, speculative Rhino command shells, and app-side reinvention of shared library capability.

## [2]-[TOPOLOGY]

| [SURFACE]                 | [OWNER]                   | [ROLE]                                                                                    |
| :------------------------ | :------------------------ | :---------------------------------------------------------------------------------------- |
| `apps/grasshopper/Radyab` | GH2 product boundary      | Thin plugin shell over shared GH2 and geometry libraries.                                 |
| `libs/csharp`             | C# library suite          | Geometry, host, runtime, UI, compute, and persistence foundations.                        |
| `tests/csharp`            | Managed C# proof surface  | Contract, law, and scenario-adjacent tests for shared libraries.                          |
| `tests/python`            | Python tool proof surface | Assay and repo-tool behavior tests.                                                       |
| `tools/assay`             | Typed operator            | Static, test, bridge, package, code, docs, and API evidence rails.                        |
| `tools/rhino-bridge`      | Live Rhino owner          | Host lifecycle, scenario execution, cargo, spool, protocol, and evidence.                 |
| `tools/cs-analyzer`       | C# architecture pressure  | Local Roslyn diagnostics for repeated source-shape laws.                                  |
| `tools/yak`               | Package metadata          | Tracked Yak manifests and icons for package roots.                                        |
| `docs`                    | Durable doctrine          | Agent-facing standards, host notes, stack doctrine, and source-backed reference material. |

## [3]-[LIBRARY_OWNERS]

The C# library suite is organized into strict strata — the `Rasm` geometry/numeric kernel, the host-neutral AEC-domain, the app-platform, and the host boundaries. The package roster, each package's charter, the dependency direction, and the universal-vs-host-capture rule are owned by `libs/.planning/architecture.md`. The polyglot branch roles — C# as the Rhino9(WIP)/GH2-aware producer, Python as the host-free science/compute/data/geometry/IFC companion, and TypeScript as the host-free web/edge platform — and the planning corpus are introduced in `libs/.planning/README.md`.

## [4]-[TOOL_OWNERS]

`tools/assay` is the repo operator. Its registry owns public command shape; its envelopes own result interpretation. It returns typed reports, artifacts, faults, routing notes, and evidence rather than relying on stderr or human-scanned logs. Structural search, API catalogs, static analysis, tests, bridge orchestration, package work, and docs checks route through the relevant Assay rail.

`tools/assay provision` is the Rasm campaign surface for local PG18/Timescale/ParadeDB and native runtime closure checks. `Parametric_Forge` owns the installed `rasm-provision` and `forge-scientific-env` executables; Rasm owns the manifests, locks, `.api` catalogues, and evidence that consume them.

`tools/rhino-bridge` owns live RhinoWIP execution. Contract owns protocol and fault shapes; Supervisor owns host lifecycle and folds; Stub stays dependency-zero; Shell owns in-host RPC/admission; Cargo owns scenarios and capture evidence. Libraries and prompts do not recreate launch, endpoint, quit, cargo, or spool choreography.

`tools/cs-analyzer` captures repeated C# shape laws after source diffs prove the rule reduces surface while preserving behavior. Analyzer diagnostics are architecture pressure, not suppression targets.

`tools/yak` stores package metadata only. Package staging, deployment, publish, artifact roots, and host refresh are Assay/package responsibilities.

## [5]-[PLANNING_AND_EVIDENCE]

New foundational libraries use planning campaigns before production source when scope is broad or future-consumer-facing. A campaign makes infra truth honest, captures manifests and lockfiles, extracts API catalogs through repo evidence rails, runs research and adversarial passes before authoring, enumerates isolated and in-concert capability across modalities, then collapses surviving capability into owner ledgers, row/case/policy axes, and decision-complete pages.

Hidden `.planning/` folders are implementation source when a package charter makes them the owner. Hidden `.reports/` folders are mining material for their visible doctrine pages. `.api/` folders are generated catalog evidence.

Every root, tool, and library routes generated output through an owned store: `.artifacts`, `.cache`, package staging roots, scoped report directories, or owner-declared state files. Root scratch output is a defect.

## [6]-[HOST_RUNTIME]

RhinoWIP and GH2 assemblies resolve through shared build properties, not per-project references. Host assemblies stay outside package output: `RhinoCommon`, `Rhino.UI`, `Rhino.Runtime.Code`, `Grasshopper2`, `GrasshopperIO`, `Eto`, `Microsoft.macOS`, and RhinoWIP-hosted drawing assemblies.

Live host evidence flows through the bridge plugin. Scenarios are source-only diagnostics under the relevant test or library mirror path; they do not carry `#r`, `#load`, or absolute build-output references. The bridge rail owns host-filtered reference projection, fresh artifact refs, scenario name injection, capture path injection, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity evidence.

Plugin projects classify themselves in their project files; build behavior does not depend on product names. Package membership is evaluated from MSBuild properties and package metadata.

## [7]-[DEVELOPMENT_MODEL]

Code starts from the deepest reusable library owner that can absorb the capability. App and plugin layers declare product intent, ports, and output bindings; they do not reimplement geometry kernels, host lifecycles, GH2 wiring, runtime composition, UI primitives, compute orchestration, persistence, packaging, or evidence capture.

External libraries, host APIs, package catalogs, and generated evidence are implementation material. A provider capability becomes a local row, case, delegate column, receipt field, or boundary adapter on the owning surface. Thin wrappers, provider-branded public shapes, command spam, flag spam, and app-local copies are defects.

Quality proof follows the changed owner and the active instruction cadence. Documentation and instruction changes use text, owner, path, and preservation checks unless an executable rail is explicitly requested.
