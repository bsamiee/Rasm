# [RASM_WORKSPACE]

Rasm is a RhinoWIP and Grasshopper2 monorepo for product-neutral AEC libraries, host-boundary packages, agent-operated tooling, and downstream Rhino/GH2 products. Apps and plugins are consumers; shared capability lands first in libraries that absorb geometry, host, runtime, UI, compute, persistence, packaging, and evidence concerns behind canonical owners.

Rasm is a polyglot AEC platform organized into strict strata: the C# branch is the Rhino9(WIP)/GH2-aware producer (the geometry kernel, the host-neutral AEC-domain, the app-platform, and the host boundaries); Python is the host-free science/compute/data/geometry/IFC companion; TypeScript is the host-free web/edge platform. The three branches couple only at the wire. The canonical hierarchy — the strata, the dependency direction, the universal-vs-Rhino-capture rule, and the geometry/mesh/IFC flow — is owned by `libs/.planning/architecture.md`.

All `libs/` content powers future apps of every kind — in-host on Rhino 9/WIP and GH2, standalone, remote, and web. Libraries own the core logic and expose it agent-first: host, GH2, and external-package APIs are fully captured and internalized behind higher-order abstractions, so an agent composes parameterized, polymorphic capability instead of learning hundreds of provider calls, and builds feature-rich apps with minimal code, boilerplate, or ceremony. Every folder is designed around its entry points and envisioned downstream usage — no knob or ceremony spam; intelligence is internalized, multi-modal, and automatic so consumers have minimal room to hand-roll or misuse `libs/` capability.

## [01]-[TARGET]

- [CRITICAL]: Rhino 9/WIP on macOS (WIP will eventually be Rhino 9)
- `net10.0` for hosted plugins and shared C# projects.
- Grasshopper product surfaces through `Grasshopper2`.
- Yak package output for Mac package roots.
- Polyglot workspace roots: C# libraries and plugins, Python companion libraries and tooling, TypeScript web/edge platform libraries.
- Out of scope: NO GH1 `.gha` OR Rhino 8 target, Windows package target, RhinoCode publishing path, speculative Rhino command shells, and app-side reinvention of shared library capability.

## [02]-[TOPOLOGY]

| [INDEX] | [SURFACE]            | [OWNER]                  | [ROLE]                                                                        |
| :-----: | :------------------- | :----------------------- | :---------------------------------------------------------------------------- |
|  [01]   | `libs/csharp`        | C# library suite         | Foundational packages for AEC app development (Rhino 9/wip, gh2)              |
|  [02]   | `libs/python`        | Python library suite     | Host-free companion packages.                                                 |
|  [03]   | `libs/typescript`    | TypeScript library suite | Host-free web/edge platform.                                                  |
|  [04]   | `tests`              | Polyglot proof surface   | C#, Python, and TypeScript suites plus generated contract fixtures.           |
|  [05]   | `tools/assay`        | Typed operator           | Static, test, bridge, package, code, docs, provision, and API evidence rails. |
|  [06]   | `tools/rhino-bridge` | Live Rhino owner         | Host lifecycle, scenario execution, cargo, spool, protocol, and evidence.     |
|  [07]   | `tools/cs-analyzer`  | C# architecture pressure | Local Roslyn diagnostics for repeated source-shape laws.                      |
|  [08]   | `tools/biome`        | TS architecture pressure | Promoted GritQL lint rules the root `biome.json` registers at error.          |
|  [09]   | `tools/yak`          | Package metadata         | Tracked Yak manifests and icons for package roots.                            |
|  [10]   | `docs`               | Durable doctrine         | Agent-facing standards, host notes, stack doctrine, and reference material.   |

## [03]-[LIBRARY_OWNERS]

The package roster, each package's charter, the dependency direction, and the universal-vs-host-capture rule are owned by `libs/.planning/architecture.md`; the branch roles and the planning corpus are introduced in `libs/.planning/README.md`.

## [04]-[HOST_RUNTIME]

RhinoWIP and GH2 assemblies resolve through shared build properties, not per-project references. Host assemblies stay outside package output: `RhinoCommon`, `Rhino.UI`, `Rhino.Runtime.Code`, `Grasshopper2`, `GrasshopperIO`, `Eto`, `Microsoft.macOS`, and RhinoWIP-hosted drawing assemblies.

Live host evidence flows through the bridge plugin. Scenarios are source-only diagnostics under the relevant test or library mirror path; they do not carry `#r`, `#load`, or absolute build-output references. The bridge rail owns host-filtered reference projection, fresh artifact refs, scenario name injection, capture path injection, stdout, stderr, exception, Rhino, document, tolerance, and bridge identity evidence.

Plugin projects classify themselves in their project files; build behavior does not depend on product names. Package membership is evaluated from MSBuild properties and package metadata.

## [05]-[TOOL_OWNERS]

Every root, tool, and library routes generated output through an owned store: `.artifacts`, `.cache`, package staging roots, scoped report directories, or owner-declared state files. Root scratch output is a defect, and the boundary is enforced rather than assumed — `tests/python/_testkit/test_policy.py` holds the closed allowlist of legitimate repo-root entries, so any unrouted tool write fails the suite by name and a deliberate new root file lands with its allowlist row in the same change.

- `tools/assay` is the repo operator. Its registry owns public command shape; its envelopes own result interpretation. It returns typed reports, artifacts, faults, routing notes, and evidence rather than relying on stderr or human-scanned logs. Structural search, API catalogs, static analysis, tests, bridge orchestration, package work, and docs checks route through the relevant Assay rail.
- `tools/assay provision` is the Rasm evidence envelope for Forge-provisioned server and native campaign facts. `Parametric_Forge` owns service composition, installed provisioning and scientific executables, Docker/Compose assets, credential and port policy, and native exports; Rasm owns the sanitized `ProvisionRun` facts, manifests, locks, `.api` catalogues, and evidence that consume those machine surfaces.
- Rasm agents invoke Assay as `uv run python -m tools.assay provision up|down|status|doctor|ports|inventory|extensions|plan|env|check|apply`. Assay accepts Forge schema-v3 JSON only and projects sanitized `ProvisionRun` evidence. Direct `forge-provision`, `psql`, `paths`, `prune`, `self-test`, Docker/Compose, cleanup, and diagnostic JSON remain Forge-level debugging surfaces.
- `tools/rhino-bridge` owns live RhinoWIP execution. Contract owns protocol and fault shapes; Supervisor owns host lifecycle and folds; Stub stays dependency-zero; Shell owns in-host RPC/admission; Cargo owns scenarios and capture evidence. Libraries and prompts do not recreate launch, endpoint, quit, cargo, or spool choreography.
- `tools/cs-analyzer` captures repeated C# shape laws after source diffs prove the rule reduces surface while preserving behavior. Analyzer diagnostics are architecture pressure, not suppression targets.
- `tools/biome` carries the promoted GritQL rule roster — the TypeScript doctrine's mechanical shape laws. The root `biome.json` registers every rule at error, and the `tests/typescript/_architecture` gauge proves the roster, its firing spans, and its severity against disk.
- `tools/yak` stores package metadata only. Package staging, deployment, publish, artifact roots, and host refresh are Assay/package responsibilities.
- GitHub repository settings — merge hygiene, rulesets, review automation — are settings-as-code `@pulumi/github` rows in Parametric_Forge `services/topology.ts`; the services driver preview is the verification surface, never the GitHub UI. Agent secrets arrive through the canonical `.claude/hooks/setup-env.sh` Doppler rail; custody law is the `secrets` skill.

## [06]-[PLANNING_AND_EVIDENCE]

New foundational libraries use planning campaigns before production source when scope is broad or future-consumer-facing. The planning law lives in `libs/.planning/` — `architecture.md` (the topology), `campaign-method.md` (the loop, the bar, the agent-role law), `README.md` (the authoring standard), `planning-targets.md` (every planning surface). A campaign makes infra truth honest, captures manifests and lockfiles, extracts API catalogs through repo evidence rails, runs research and adversarial passes before authoring, enumerates isolated and in-concert capability across modalities, then collapses surviving capability into owner ledgers, row/case/policy axes, and decision-complete pages.

## [07]-[DEVELOPMENT_MODEL]

Code starts from the deepest reusable library owner that can absorb the capability. App and plugin layers declare product intent, ports, and output bindings; they do not reimplement geometry kernels, host lifecycles, GH2 wiring, runtime composition, UI primitives, compute orchestration, persistence, packaging, or evidence capture.

External libraries, host APIs, package catalogs, and generated evidence are implementation material. A provider capability becomes a local row, case, delegate column, receipt field, or boundary adapter on the owning surface. Thin wrappers, provider-branded public shapes, command spam, flag spam, and app-local copies are defects.

Quality proof follows the changed owner and the active instruction cadence. Documentation and instruction changes use text, owner, path, and preservation checks unless an executable rail is explicitly requested.
