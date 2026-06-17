# [PYTHON_IDEAS]

The Python branch forward pool: the big ideas imagined first and distilled into `TASKLOG.md` items, the cross-package concert, and the folder horizons aggregated from the five packages' refinement passes. A concept lands here only if it couples Python packages to each other or deepens a branch owner; a concept that crosses a language boundary stays in the Tier-0 `IDEAS.md` and is referenced as a Tier-0 seam, never restated. Lean rows carry `[PAGE#CLUSTER]` as the sole location key; owner state is read at the owning package `ARCHITECTURE.md` `[OWNER_REGISTRY]`, never marked here.

## [1]-[BRANCH_CONCERT]

The flagship is one scientific/AEC workflow authored end-to-end across the five packages, meeting only at the runtime ports and the named seams, never at one another's interiors. The threads:

- One runtime foundation, four consumers — `runtime` mints `ContentIdentity`, `BoundaryFault`/`RuntimeRail`, `Retry`, `ResourceRoot`, `LanePolicy`, `ReceiptContributor`, and `ServerHost`/`Credential` once; `compute`, `data`, `geometry`, and `artifacts` compose them as settled vocabulary and never re-mint a second content-identity, receipt, retry, or wire owner.
- One content address, every artifact — a blob written by any package is fetched by content key; a `compute`-graduated sub-result, a `geometry` GLB, a `data` egress bundle, and an `artifacts` document all key by the one `ContentIdentity`, so determinism and cache-hit-by-reference are derivable, not flags.
- One evidence rail outward — `compute/graduation#GRADUATION` carries a `HandoffAxis` geometry case so `geometry` registration transforms, reconstructed meshes, topology graphs, and form-finding reach the managed owner system through the single graduation receipt, never a parallel handoff per package.
- One tessellation companion, served once — the IfcOpenShell `IFC → mesh/GLB` daemon is `geometry`-native, hosted by the runtime `ServerHost` over the existing inbound contract; it mints no transport, and `data` mesh-file shapes feed it as inputs while the IFC-to-GLB rail stays in `geometry`.

## [2]-[CROSS_PACKAGE_LAWS]

- One owner per axis, one entrypoint family per rail, one fault family per branch — three-or-more parallel types/enums/dispatch arms collapse into one polymorphic owner (a tagged union + `match`, a `StrEnum` with a behavior table, a fold, or a `frozendict` data table); a new capability is a row, a case, or a policy value on an existing owner, never a sibling surface.
- Runtime owns the shared value shapes the determinism closure must reference without a sibling dependency: the node-edit delta algebra and the canonical mesh contract reside in `runtime` as dependency-free structural shapes (keyed by `ContentIdentity`), lowered into the consuming packages at their own boundary so neither package imports the other's interior.
- The receipt family is an open `Protocol`-bounded family discriminated by the receipt `tag`, not a single closed union rooted in one package — each owner declares its own arm Struct and the runtime port discriminates by tag, so no cross-package import root is forced.
- A fence names an external member only after the owning package `.api` catalogue verifies its spelling; a phantom member is the named defect, an admitted capability no owner exploits is a named gap.

## [3]-[FORWARD_POOL]

The big ideas distilled into `TASKLOG.md` rows; each rides ONE owner axis (a new owned page or a deepened existing owner), never a sibling surface. Generative/deep-learning AI is out of scope; classical inferential statistics, surrogate/classification model assets, and validated numerics are in scope.

| [INDEX] | [CONCEPT]                                                        | [PAGE#CLUSTER]                          |
| :-----: | :-------------------------------------------------------------- | :-------------------------------------- |
|   [1]   | per-algorithm typed solver-receipt family + dense/sparse/ODE/optimize/root lanes | `compute/array-solver#SOLVER`           |
|   [2]   | study DOE generation, global sensitivity, calibration, Pareto, surrogates, queryable receipt lake | `compute/units-study#STUDY`             |
|   [3]   | dimensioned + dual-path-uncertainty quantity threaded through the solve/study graph | `compute/units-study#QUANTITY`          |
|   [4]   | signal & spectral workbench over unit-and-coordinate-aware labelled arrays | `compute/array-solver#SIGNAL`           |
|   [5]   | verified symbolic-to-native codegen with an equivalence-proof gate | `compute/array-solver#SOLVER`           |
|   [6]   | adjoint-through-solve inverse design (discrete adjoint over the FEM forward) | `compute/differentiable#INVERSE`        |
|   [7]   | validated-numerics certifier — guaranteed interval/ball enclosures | `compute/rigor#CERTIFY`                  |
|   [8]   | simulation mesh+field interchange + weak-form FEM assembly + in-situ | `compute/simframe#ASSEMBLY`             |
|   [9]   | Bayesian inference/posterior owner — MCMC/HMC/VI + convergence diagnostics | `compute/graduation#INFERENCE`          |
|  [10]   | array-native computational geometry (KD-tree/Voronoi/Delaunay/alpha-shape) | `compute/array-solver#SPATIAL`          |
|  [11]   | transactional lakehouse table engine — MERGE/time-travel/OPTIMIZE/CDC | `data/columnar-query#LAKEHOUSE`         |
|  [12]   | versioned tensor datacube + out-of-core array + ragged/CF cube | `data/graph-mesh#TENSOR`                |
|  [13]   | data-contract statistical firewall — profile/drift/expectations/quarantine | `data/columnar-query#QUALITY`           |
|  [14]   | federated search + continuous standing-query engine with per-hit lineage | `data/columnar-query#QUERY`             |
|  [15]   | dataframe-agnostic Arrow zero-copy interchange carrier         | `data/schema-geo#INTEROP`               |
|  [16]   | accelerated network-science algorithm family (networkx→rustworkx) | `data/graph-mesh#GRAPH`                 |
|  [17]   | parametric constraint solver + git-for-geometry object-graph diff | `geometry/parametric#CONSTRAIN`         |
|  [18]   | native CAD/BIM/scan in-process ingest (STEP/AP242/E57/LAS/IFC5) | `geometry/interchange#INGEST`           |
|  [19]   | semantic DocumentModel + bidirectional extraction + corpus query | `artifacts/docmodel#MODEL`              |
|  [20]   | conformance grading + physical redaction + PAdES sealing plane | `artifacts/conformance#GRADE`           |
|  [21]   | print-production color management — ICC/CMYK/wide-gamut + imposition | `artifacts/color#GAMUT`                 |
|  [22]   | scientific image processing — segmentation/morphology/registration | `artifacts/imaging#IMAGE`               |
|  [23]   | provider-fingerprint + replay-verify determinism closure       | `runtime/content-identity#IDENTITY`     |
|  [24]   | orchestration triggers + content-addressed node memo + durable exactly-once queue | `runtime/resources-lanes#LANE`          |
|  [25]   | typed capability catalog + MCP/SDK/CLI projection             | `runtime/capabilities#CATALOG`          |
|  [26]   | parametrized headless notebook execution + content-addressed cell outputs | `runtime/notebook#RUN`                  |
|  [27]   | bounded-memory chunked-streaming iterator over the lane bound  | `runtime/resources-lanes#LANE`          |

## [4]-[FOLDER_HORIZONS]

The next-deepening pass per folder, aggregated from the five packages' refinement horizons. Each row is the residual deepening an existing owner absorbs once its catalogue floor fills.

| [INDEX] | [FOLDER]    | [HORIZON]                                                                                                       |
| :-----: | :---------- | :------------------------------------------------------------------------------------------------------------ |
|   [1]   | `artifacts` | pillow-blocked image chain finalizes on toolchain install; pyvista/vtk 3D scene path on the native floor; report-templating composition; strided GLB content-addressing |
|   [2]   | `compute`   | sympy lambdify-to-managed-owner codegen handoff; geometry `HandoffAxis`; pymc/arviz inference member spellings on cp315 wheel resolve; `StubCodegen` decode of graduation evidence at the Tier-0 seam |
|   [3]   | `data`      | Arrow C Data Interface zero-copy; ADBC/ConnectorX remote sourcing; GeoParquet/CRS-normalized egress; deepen against real engines |
|   [4]   | `geometry`  | IFC companion warm-pool cold-start amortization; 64 KiB ArtifactSync frame framing (Crc32 + XxHash128), proven at the Tier-0 companion seam |
|   [5]   | `runtime`   | the two SPIKE owners (`ServerHost`, `Credential`) finalize on the sub-3.15 companion floor + `grpcio.aio` boot; `ContentIdentity` seed byte-identity at the Tier-0 seam |
