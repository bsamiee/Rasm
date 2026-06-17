# [PYTHON_BRANCH]

The Python branch charter: a first-class, agnostic, universal scientific/compute/data/geometry/IFC library branch of five peer packages, re-derived from first principles every campaign and held to the suite bar in idiomatic modern Python. The single root `pyproject.toml` is the only Python manifest; package folders carry no package-local manifest and future source lands directly under `libs/python/<package>`. This charter absorbs the branch topology and the cross-package laws; the source tree, dependency graph, and cross-folder seams live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, open work in `TASKLOG.md`, the `.api` governance in `api-catalogues.md`, and the per-package owner registries on each package `ARCHITECTURE.md`.

## [1]-[PACKAGE_TOPOLOGY]

Five first-class packages, each owning one axis of the science/compute/data/geometry/IFC stack. Package folders carry `README.md`, `.planning/`, `.api/`, and future source directly under the package root; no nested source namespace, no empty package markers, no package-local manifest.

| [INDEX] | [PACKAGE]   | [ROLE]                                                                                                                  | [FLOOR]                                  |
| :-----: | :---------- | :-------------------------------------------------------------------------------------------------------------------- | :--------------------------------------- |
|   [1]   | `runtime`   | the execution foundation: context/settings admission, the one boundary-fault + Result/Option rail, the one resilience policy, the one content-identity owner, resource roots, bounded anyio lanes, local receipts + the contributor port, the inbound companion gRPC server-runtime + credential axis, API/structural-parsing evidence, the private daemon entrypoint grammar | `>=3.15` core; `ServerHost`/`Credential` resolve on the companion floor |
|   [2]   | `compute`   | offline scientific evidence that graduates: array admission, one numeric-intent solver with accelerator rows, units + uncertainty claims, study + experiment-run orchestration, model-asset validation, the graduation receipt with a geometry handoff case, the typed-stub codegen, the Bayesian inference owner | `>=3.15` core; accelerator/model/deploy-asset rows on the marker floor |
|   [3]   | `data`      | portable data interchange: typed dataset refs, columnar lazy/streaming scan + egress, cross-engine query plans, the Delta lakehouse lifecycle, schema claims + a data-contract gate, dataframe-agnostic interop, vector + raster geospatial, graph payloads, chunked tensor stores, mesh-file exchange | `>=3.15` core; cp315-wheel-gated scientific rows on the marker floor |
|   [4]   | `geometry`  | geometry and IFC/BIM interchange and the load-bearing cross-boundary package: the IfcOpenShell tessellation companion daemon (IFC to mesh/GLB + semantic XML/JSON), IFC property/quantity/relationship analysis, point-cloud/3D-scan registration and reconstruction, non-manifold topology, AEC computational geometry | companion floor `python_version<'3.13'` (native/OCCT/VTK leg) |
|   [5]   | `artifacts` | artifact production: one polymorphic document/PDF/Office/structured-text plan, one VisualSpec-to-ExportPlan axis (2D charts + 3D scientific visualization), one report-templating composition, one preview owner, one compression owner | `>=3.15` core; image-toolchain + native-VTK rows on the marker floor |

The branch runs a `requires-python='>=3.15'` core on the normal-GIL CPython build (`cpython-315`), with a `python_version<'3.13'` companion floor homing the native/OCCT/VTK and gRPC-server stack for the geometry and runtime offline/companion legs. This single sanctioned interpreter divergence simultaneously isolates the copyleft `ifcopenshell` wheel from the permissive core lock and homes the server stack the core floor cannot resolve. The floor-realization is two distributions — a core `>=3.15` project and an isolated companion project carrying its own `[build-system]`, a lowered `requires-python`, and `tool.uv` environment forks — open at `TASKLOG.md` as the companion lock-scope decision gating the runtime and geometry companion-floor owners.

## [2]-[TEST_POLICY]

One branch test policy synthesized from the per-package rails; package-specific gates appear only where a package owns a distinct probe. Proof runs at the planned phase gate, not after each edit. Every rail names its owner; the executable command lives with that rail owner, never restated here.

| [INDEX] | [RAIL]      | [SCOPE]                                                                                                                       |
| :-----: | :---------- | :-------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `pytest`    | the law-matrix spec suite per package, with `hypothesis` property-based testing driving the algebraic laws over each owner   |
|   [2]   | coverage    | per-file coverage gate over each package spec suite, no owner left unexercised                                              |
|   [3]   | `mutmut`    | mutation kill-ratio over the spec suite; a surviving mutant is a missing law                                                |
|   [4]   | `ty`        | typed-signature transcription resolves clean — every fence's PEP 695 generics, `msgspec`/`pydantic` owners, and `Protocol` ports type-check |
|   [5]   | `ruff`      | routed lint + format closure with zero diagnostics; `tools/py_analyzer` AST rules promote any class/type/string/constant spam pattern no shipped rule rejects |
|   [6]   | `uv`        | locked restore against the root manifest, and the wheel/companion/toolchain floor gates that install marker-gated wheels before `assay api` re-reflects |
|   [7]   | `assay api` | every fence member resolves to an `.api` evidence row; a phantom member is the named defect |

Package-specific gates: `geometry` adds a daemon-bridge probe — the `IfcCompanion` daemon serves the inbound gRPC contract under the companion floor — and a companion-floor gate confirming all five native pins reflect on the cp312 interpreter. `runtime` adds the same companion-floor gate for `grpcio`/`grpcio-tools`/`protobuf`. `data`, `compute`, and `artifacts` each gate a wheel/toolchain floor that installs cp315/marker-floor wheels before re-reflecting. The residual `SPIKE` set at planning exit is exactly the live-host/companion-floor probes; every SPIKE owner is driven to `FINALIZED` against the installed distributions during planning, never deferred to implementation.

## [3]-[ENTRY]

The branch method, bar, fence law, named defect class, and red-team passes are the cross-language [campaign method](../../.planning/campaign-method.md); the doc-set tiers, column schemas, page#cluster notation, page grammar, signature law, language law, and review law are the [planning standard](../../.planning/README.md). This branch is Tier 1: it carries cross-folder facts as `pkg/page#CLUSTER` and never names another language — cross-language seams live only in the Tier-0 ledger and are referenced as Tier-0 seams. Owner state lives in exactly one surface per package — the package `ARCHITECTURE.md` `[OWNER_REGISTRY]` `[STATE]` column; `IDEAS.md`, `TASKLOG.md`, and this charter route to that registry and never carry owner state.
