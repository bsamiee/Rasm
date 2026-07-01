# [RASM_WORKSPACE]

Rasm is a RhinoWIP and Grasshopper2 monorepo for product-neutral AEC libraries, host-boundary packages, agent-operated tooling, and downstream Rhino/GH2 products. Apps and plugins are consumers; shared capability lands first in libraries that absorb geometry, host, runtime, UI, compute, persistence, packaging, and evidence concerns behind canonical owners.

Rasm is a polyglot AEC platform organized into strict strata: the C# branch is the Rhino9(WIP)/GH2-aware producer (the geometry kernel, the host-neutral AEC-domain, the app-platform, and the host boundaries); Python is the host-free science/compute/data/geometry/IFC companion; TypeScript is the host-free web/edge platform. The three branches couple only at the wire. The canonical hierarchy — the strata, the dependency direction, the universal-vs-Rhino-capture rule, and the geometry/mesh/IFC flow — is owned by `libs/.planning/architecture.md`.

All content within `libs/` is meant to power hundreds, if not thousands of future apps of various kinds, in-host on Rhino 9/WIP, gh2, or totally standalone, remote, web, etc. The `libs/` folder is meant to provide all of the core logic, functionality, and features in such a way that it works in Rhino/gh2 as well as agnostic standalone instances. The functionality provided should also allow for the creation of complex, advanced, and extremely feature/capability rich apps with minimal code, no boilerplate, ceremony, or agent overhead. All functionality must be made with agent first considerations, such that an agent should not need to know hundreds of custom API's/function calls, but instead, the `libs/` code internalized/integrated it, fully capturing the rhino/gh2 + external packaging API/capabilities, and created higher order abstractions, allowing for flexible/powerful app creation through parameterization, and polymorphism.

NOTE: It is crucial that EACH folder within `libs/` is made with immense focus on entry points, and usage pattern, envisioning downstream/future apps and how they will use the functionality, we do NOT want knob/ceremony spam, instead, internalizing, automating/integrating features and capabilities with intelligence to be multi-modal, faceted and automatic so agents have far less opportunity to handroll or incorrectly use capabilities we made within `libs/`.

## [01]-[TARGET]

- [CRITICAL]: Rhino 9/WIP on macOS (WIP will eventually be Rhino 9)
- `net10.0` for hosted plugins and shared C# projects.
- Grasshopper product surfaces through `Grasshopper2`.
- Yak package output for Mac package roots.
- Polyglot workspace roots: C# libraries and plugins, Python tooling, TypeScript/web surfaces and generated wire consumers.
- Out of scope: NO GH1 `.gha` OR Rhino 8 target, Windows package target, RhinoCode publishing path, speculative Rhino command shells, and app-side reinvention of shared library capability.

## [02]-[TOPOLOGY]

| [INDEX] | [SURFACE]                 | [OWNER]                   | [ROLE]                                                                                |
| :-----: | :------------------------ | :------------------------ | :------------------------------------------------------------------------------------ |
|  [01]   | `apps/grasshopper/Radyab` | Test/throw-away app       | Thin test app, not binding to any patterns                                            |
|  [02]   | `libs/csharp`             | C# library suite          | Geometry, host, runtime, UI, compute, and persistence foundations.                    |
|  [03]   | `tests/csharp`            | Managed C# proof surface  | Contract, law, and scenario-adjacent tests for shared libraries.                      |
|  [04]   | `tests/python`            | Python tool proof surface | Assay and repo-tool behavior tests.                                                   |
|  [05]   | `tools/assay`             | Typed operator            | Static, test, bridge, package, code, docs, provision, and API evidence rails.         |
|  [06]   | `tools/rhino-bridge`      | Live Rhino owner          | Host lifecycle, scenario execution, cargo, spool, protocol, and evidence.             |
|  [07]   | `tools/cs-analyzer`       | C# architecture pressure  | Local Roslyn diagnostics for repeated source-shape laws.                              |
|  [08]   | `tools/yak`               | Package metadata          | Tracked Yak manifests and icons for package roots.                                    |
|  [09]   | `docs`                    | Durable doctrine          | Agent-facing standards, host notes, stack doctrine, source-backed reference material. |

## [03]-[LIBRARY_OWNERS]

The C# library suite is organized into strict strata — the `Rasm` geometry/numeric kernel, the host-neutral AEC-domain, the app-platform, and the host boundaries. The package roster, each package's charter, the dependency direction, and the universal-vs-host-capture rule are owned by `libs/.planning/architecture.md`. The polyglot branch roles — C# as the Rhino9(WIP)/GH2-aware producer, Python as the host-free science/compute/data/geometry/IFC companion, and TypeScript as the host-free web/edge platform — and the planning corpus are introduced in `libs/.planning/README.md`.

## [04]-[HOST_RUNTIME]

RhinoWIP and GH2 assemblies resolve through shared build properties, not per-project references. Host assemblies stay outside package output: `RhinoCommon`, `Rhino.UI`, `Rhino.Runtime.Code`, `Grasshopper2`, `GrasshopperIO`, `Eto`, `Microsoft.macOS`, and RhinoWIP-hosted drawing assemblies.

Live host evidence flows through the bridge plugin. Scenarios are source-only diagnostics under the relevant test or library mirror path; they do not carry `#r`, `#load`, or absolute build-output references. The bridge rail owns host-filtered reference projection, fresh artifact refs, scenario name injection, capture path injection, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity evidence.

Plugin projects classify themselves in their project files; build behavior does not depend on product names. Package membership is evaluated from MSBuild properties and package metadata.

## [05]-[TOOL_OWNERS]

Every root, tool, and library routes generated output through an owned store: `.artifacts`, `.cache`, package staging roots, scoped report directories, or owner-declared state files. Root scratch output is a defect.

- `tools/assay` is the repo operator. Its registry owns public command shape; its envelopes own result interpretation. It returns typed reports, artifacts, faults, routing notes, and evidence rather than relying on stderr or human-scanned logs. Structural search, API catalogs, static analysis, tests, bridge orchestration, package work, and docs checks route through the relevant Assay rail.
- `tools/assay provision` is the Rasm evidence envelope for Forge-provisioned server and native campaign facts. `Parametric_Forge` owns service composition, installed provisioning and scientific executables, Docker/Compose assets, credential and port policy, and native exports; Rasm owns the sanitized `ProvisionRun` facts, manifests, locks, `.api` catalogues, and evidence that consume those machine surfaces.
- Rasm agents invoke Assay as `uv run python -m tools.assay provision up|down|status|doctor|ports|inventory|extensions|plan|env|check|apply`. Assay accepts Forge schema-v3 JSON only and projects sanitized `ProvisionRun` evidence. Direct `forge-provision`, `psql`, `paths`, `prune`, `self-test`, Docker/Compose, cleanup, and diagnostic JSON remain Forge-level debugging surfaces.
- `tools/rhino-bridge` owns live RhinoWIP execution. Contract owns protocol and fault shapes; Supervisor owns host lifecycle and folds; Stub stays dependency-zero; Shell owns in-host RPC/admission; Cargo owns scenarios and capture evidence. Libraries and prompts do not recreate launch, endpoint, quit, cargo, or spool choreography.
- `tools/cs-analyzer` captures repeated C# shape laws after source diffs prove the rule reduces surface while preserving behavior. Analyzer diagnostics are architecture pressure, not suppression targets.
- `tools/yak` stores package metadata only. Package staging, deployment, publish, artifact roots, and host refresh are Assay/package responsibilities.

## [06]-[PLANNING_AND_EVIDENCE]

New foundational libraries use planning campaigns before production source when scope is broad or future-consumer-facing. A campaign makes infra truth honest, captures manifests and lockfiles, extracts API catalogs through repo evidence rails, runs research and adversarial passes before authoring, enumerates isolated and in-concert capability across modalities, then collapses surviving capability into owner ledgers, row/case/policy axes, and decision-complete pages.

## [07]-[DEVELOPMENT_MODEL]

Code starts from the deepest reusable library owner that can absorb the capability. App and plugin layers declare product intent, ports, and output bindings; they do not reimplement geometry kernels, host lifecycles, GH2 wiring, runtime composition, UI primitives, compute orchestration, persistence, packaging, or evidence capture.

External libraries, host APIs, package catalogs, and generated evidence are implementation material. A provider capability becomes a local row, case, delegate column, receipt field, or boundary adapter on the owning surface. Thin wrappers, provider-branded public shapes, command spam, flag spam, and app-local copies are defects.

Quality proof follows the changed owner and the active instruction cadence. Documentation and instruction changes use text, owner, path, and preservation checks unless an executable rail is explicitly requested.
