# [PYTHON_TASKLOG]

Open work only. Closed rows move into the owning page or disappear. Owner state lives on each package charter DENSITY_BAR, never here.

## [1]-[INTERPRETER_FLOOR]

[PY_FLOOR_001]:
- Owner: branch / manifest
- Work: `requires-python='>=3.15'` makes every `python_version<'3.13'` and `<'3.15'` marker DEAD under uv universal resolution — all marker-gated pins are pruned from `uv.lock`, so the geometry-companion own-lock-scope the topology mandates is unreachable under a single floor. The named resolution (`architecture.md#INTERPRETER_FLOORS`) is two distributions: a core `>=3.15` project plus an isolated companion project with its own `[build-system]`, a lowered `requires-python`, and `tool.uv.required-environments` forks, so the marker-gated and sub-3.13 pins resolve in the companion lock while the core `>=3.15` floor and the ty/ruff 3.15 configs hold exactly. This row and `PY_FLOOR_002` are one decision (core/companion split), executed together.
- Exit: the two-distribution split lands, the sub-3.13 and sub-3.15 environments resolve their marker-gated pins into the companion lock, and `assay api query` reflects them in place.

[PY_FLOOR_002]:
- Owner: branch / packaging
- Work: `[tool.uv] package = false` produces no installable distribution; a branch positioned as adoptable-if-public ships no wheel, no `[build-system]`, no entry-point registration, and the companion daemon cannot ship as a console entry. The two-distribution split named at `architecture.md#INTERPRETER_FLOORS` adds a `[build-system]` (hatchling) to each project so the core ships an installable wheel and the companion project registers the daemon console entry under its own lower floor. Executed jointly with `PY_FLOOR_001`.
- Exit: both distributions produce installable artifacts and the companion `Entrypoint` registers a console entry.

[PY_FLOOR_003]:
- Owner: branch / manifest
- Work: the next-loop ideation pages require new distributions absent from the root `pyproject.toml`, each gating its MISSING/PARTIAL row and each carrying an interpreter-floor + license posture (`region-map/api-owners.md#[6]`): `python-flint` (`PY_RIGOR_001`, LGPL-2.1+ posture review — admit to the companion lock alongside the LGPL `ifcopenshell` if no permissive path, marker-floor likely), `scikit-fem`+`netcdf4` (`PY_SIM_001`, Catalyst/VTK-m on the `python_version<'3.13'` native floor), `optimistix` (`PY_DIFF_001`, the discrete-adjoint outer-loop solver; jax marker floor `PY_API_003`), `icechunk`+`zarr`+`cubed`+`awkward` (`PY_TENSOR_001`), `pystac`+`pystac-client`+`odc-stac`+`h3`+`s2sphere` (`PY_TENSOR_001` STAC/DGG, gated — out of the `cube` surface until admitted), `cupy` (the `gpu` Substrate row, compute `arrays`, marker-floor; the `gpu` enum value stays out of the closed `Substrate` union until this lands), `python-solvespace` (`PY_PARA_001`, native build), `pythonocc-core`+`laspy`+`pye57`+`pdal` (`PY_INGEST_001`, native CAD/scan ingest, native builds + marker-floor likely), `numpyro`+`pymc`+`arviz` (`PY_INFER_001`, Bayesian inference; jax marker floor for numpyro), `pyhanko`+`fonttools` (`PY_CONF_001`; veraPDF is a JVM external-tool seam, not a pin), `croniter` (`PY_ORCH_001`), `papermill`+`nbclient` (`PY_NB_001`), `narwhals`+`nanoarrow` (`PY_INTEROP_001`), `rustworkx` (`PY_GRAPH_001`), `scikit-image` (`PY_IMG_001`, marker-floor confirm). The admission is the precondition the FENCE_LAW would otherwise block: a row that adds only a card without a registered pin is UNDONE — it is in the third bucket "card admitted, owner UNVERIFIED" until its pin lands, distinct from both `[PROPOSED]` and `[FINAL]`. This admits the pins so symbol/cluster/seam registration (already landed in the region-map `[PROPOSED]` blocks) can carry transcription fences. The LGPL members (`python-flint`) resolve in the companion lock, never the core MIT/Apache lock, mirroring the `ifcopenshell` boundary.
- Exit: each distribution is admitted with its interpreter floor + license routing, the LGPL members land in the companion lock, and `assay api query` reflects each in place so the `[PROPOSED]` region-map rows finalize. Foundational — gates every MISSING row; peer to `PY_FLOOR_001`. Until each pin lands its dependent row is UNVERIFIED, not buildable.

## [2]-[CONTENT_IDENTITY]

[PY_HASH_001]:
- Owner: runtime
- Work: an XxHash128 distribution reproducing the C# `System.IO.Hashing.XxHash128.HashToUInt128(bytes, seed)` digest byte-identically is not yet pinned in the root manifest; `content-identity#IDENTITY` `_digest128`/`_digest64` bind to it at admission. The seed-derivation (a 64-bit XxHash3 digest over the format/deflection/tolerance string) is proven against the C# `InterchangeIdentity.Seed` so a cross-setting hit never collides.
- Exit: the XxHash128 pin is admitted, catalogued, and the seed parity is proven against the C# output.

## [3]-[API_CATALOGUE]

[PY_API_001]:
- Owner: runtime
- Work: the companion server wire (`grpcio`, `grpcio-tools`, `protobuf`) is catalogued but rides the `python_version<'3.13'` floor; under `>=3.15` `grpcio`/`protobuf` are present only as marker-free transitive deps of `specklepy` (major 6, not the pinned major 7) and `grpcio-tools` is absent. The `server-host#SERVE` fence members confirm against the C# `ComputeService`/`ArtifactSync` descriptors once the floor decision admits the sub-3.13 environment.
- Exit: `PY_FLOOR_001` admits the floor, `grpcio-tools` installs, and `assay api query` fills the full member surface.

[PY_API_002a]:
- Owner: data
- Work: 19 data distributions are absent from the `>=3.15` lock (polars, pyarrow, pandas, deltalake, duckdb, adbc-driver-manager, connectorx, dask, xarray, geopandas, shapely, pyogrio, pyproj, rasterio, rhino3dm, meshio, trimesh, h5py, pandera); `networkx` is captured. Each page carries the import name and the wheel-floor capture gap; no members are invented while the cp315 wheel is missing. `rasterio` additionally rides the `python_version<'3.15'` marker.
- Exit: a cp315 wheel or the marker-floor environment admits each distribution, then `assay api query` fills public types, entrypoints, and implementation law.

[PY_API_003]:
- Owner: compute
- Work: `numba`/`jax`/`onnx`/`onnxruntime`/`scikit-learn` ride the `python_version<'3.15'` marker (no cp315 wheel); `numpy`/`scipy`/`pint`/`uncertainties` carry no cp315 wheel; `sympy` is cp315-reflected. Fill the marker-floor catalogues once the environment installs them.
- Exit: the marker-floor environment admits each distribution and `assay api query` fills the catalogue.

[PY_API_004]:
- Owner: artifacts
- Work: `pillow` has no cp315 wheel and a blocked source build (missing llvm-ar then absent libjpeg/zlib headers); it transitively blocks `pikepdf`/`reportlab`/`weasyprint`/`python-pptx`/`matplotlib`. `pyvista`/`vtk` ride the `python_version<'3.13'` native floor. The other 19 are cp315-reflected.
- Exit: `pillow` installs (a cp315 wheel publishes or the host provisions libjpeg/zlib + an archiver) and the native VTK floor is admitted, then the six and the two re-reflect.

## [4]-[PAGE_GRADE]

[PY_DOC_001]:
- Owner: package planning
- Work: cold-grade every package-local planning page against the suite standard and the upgraded `docs/stacks/python/` after the marker-floor catalogues fill; drive every DENSITY_BAR `SPIKE` to `FINALIZED` by folding verified member spellings back into the fences.
- Exit: a cold read of each package corpus surfaces nothing and the residual `SPIKE` set is exactly the live-host/companion-floor probes.

## [5]-[FORWARD_FRONTIER]

[PY_NEXT_001]:
- Owner: branch (next loop)
- Work: SUPERSEDED by `PY_PARA_001` — the object-graph DAG diff concern (Speckle-class) now has a named consumer (the parametric constraint solver) and folds into the `parametric` page; the C# Persistence diff algebra remains the cross-boundary peer, but the geometry-diff half is Python-owned.
- Exit: `PY_PARA_001` admits the `parametric` page.

## [6]-[NEXT_LOOP_IDEATION] — PARTIAL/MISSING implement targets (2026-06-16)

The next-loop implement-target pool from the cross-lens + frontier-atlas ideation pass (`.artifacts/planning-briefs/python-ideation.md`, PY1-PY26). PARTIAL rows deepen an existing owner in place; MISSING rows open a new owned page or fold a determinism/orchestration extension into an existing owner. EXISTS rows are skipped. Each row is decision-complete to the page-naming bar; transcription-complete fences follow once the marker-floor catalogues (`PY_API_*`) fill the cp315/companion distributions and `PY_FLOOR_003` admits the new pins. Owner symbols, page+cluster tokens, seams, and distribution routing for every row below are registered `[PROPOSED]` in `region-map/{owner-symbols.md,page-regions.md,seam-splits.md,api-owners.md}` — registration has no install dependency and is done; only the transcription fence and pin admission remain.

TOPOLOGICAL BUILD ORDER (scheduling; value-rank stays for prioritization): `PY_HASH_001` → `PY_DET_001` (foundational, NOT a mid-tier extension — the whole receipt-lake/memoization/replay spine is downstream) → { `PY_SOLVE_001`, `PY_FLOOR_003` } → { `PY_RIGOR_001`, `PY_STUDY_001`, `PY_QUANT_001` } → { `PY_CODEGEN_001`, `PY_SIM_001`, `PY_QUERY_001`, `PY_ORCH_001` } → { `PY_DIFF_001`, `PY_CAP_001` }. `PY_COLOR_001` schedules BEFORE `PY_CONF_001` (conformance consumes the color hash). `PY_STUDY_001` (value-rank 2) is dependency-blocked on `PY_DET_001` (value-rank 8) for the provider fingerprint — the build order resolves the contradiction by promoting `PY_DET_001` ahead.

### [6.1]-[PARTIAL] — deepen existing owner

[PY_SOLVE_001]:
- Owner: compute
- Work: `NumericIntent` returns a generic `SolveResult` (route + residual). Introduce a closed `SolverReceipt` union of per-algorithm receipts — `Factorization` (LU/QR/Cholesky/SVD/eig/Schur + `LinearProvider` row numpy·mkl·openblas·jax), `Integration` (Radau/BDF/LSODA/DOP853 + event detection + dense-output interpolant + stiffness/steps/rejections/Jacobian-evals), `Convergence` (constrained NLP SLSQP/trust-constr + global differential-evolution/dual-annealing + NLLS LM/TRF + multi-objective + KKT stationarity), and a sparse `Factorization` carrying CSR/CSC/COO + SuperLU/CHOLMOD + Krylov CG/GMRES/BiCGSTAB + ILU/AMG preconditioner row + AMD/METIS reorder + sparsity-pattern. Add a vector root-finding lane (Powell/Anderson/Broyden) with analytic-Jacobian injection from the symbolic rail. Never collapse to a generic `IReceipt`.
- Exit: every `solve` route emits its algorithm-specific `SolverReceipt` case, the dense/sparse/integration/optimize/root lanes resolve their full method+provider rows, and `assay api query` fills the scipy/numpy/jax member surface (gated on `PY_API_003`).

[PY_STUDY_001]:
- Owner: compute
- Work: `StudyPlan` owns param-axis/sample-grid/route/termination only. Deepen `study(...)` to discriminate `{sweep, doe, optimize, calibrate, sensitivity, propagate}`: named DOE designs (Latin-hypercube/Sobol/Halton/full+fractional-factorial/central-composite/Box-Behnken), fan-out over the existing `LanePolicy`/`StagePlan` substrate (Dask is one `Substrate` row, not a forked scheduler), Sobol/Morris/FAST/Delta global-sensitivity indices with a `SensitivityReceipt`, Bayesian calibration, multi-objective Pareto fronts, and GP/PCE/RBF surrogates feeding active-learning refinement. Every run lands as a content-addressed `StudyReceipt` (design matrix + per-point `SolverReceipt`s + convergence + provider fingerprint) materialized as parquet/arrow in a duckdb-queryable lake with AS-OF + provenance-DAG query shape, replayable bit-for-bit.
- Exit: `study` resolves all six request shapes, the `StudyReceipt` lake is queryable AS-OF, and per-point receipts reference `PY_SOLVE_001` cases. The GP/PCE/RBF surrogates are `scikit-learn`/`scipy`-native and live on this `study` page — they do NOT reuse `ModelAsset` (onnx/onnxruntime/skl2onnx), which owns only ONNX-shaped assets. PEER-PLANE to the C# V6 Solve Farm: PY2 is the duckdb-embedded study lake federated by `ContentIdentity`, V6 is the Postgres-resident farm; the design-space/Pareto artifact shape is shared, the residency plane is Python-owned. Depends on `PY_DET_001` (provider fingerprint — foundational, scheduled ahead of this value-rank-2 row).

[PY_QUANT_001]:
- Owner: compute
- Work: `QuantityClaim` carries unit+uncertainty claims but is not threaded through the solver/study graph. Thread a dimensioned (pint) + correlated-uncertainty (uncertainties linear-propagation + Monte-Carlo dual path) quantity over xarray-labelled axes through the whole solve/study/signal graph; dimensional coercion at boundaries, dimensional violations structurally unrepresentable, a final answer ships a calibrated confidence band AND a dimensional proof. Pairs with the certified-enclosure peer (`PY_RIGOR_001`): "a ± sigma" and "provably in [lo,hi]" are orthogonal first-class properties.
- Exit: a numeric value flows the full graph carrying unit + dual-path uncertainty, the `UncertaintyReceipt` records method/samples/band, and dimensional violation is a boundary reject. BOUNDARY STRATEGY (pint does not survive most scipy calls; uncertainties does not vectorize through jax): STRIP-AT-KERNEL-ENTRY / RE-ATTACH-AT-RECEIPT — `QuantityClaim` strips units+uncertainty to bare arrays at the boundary adapter entering a scipy/jax kernel, records the dimensional signature + correlation structure on the call, and re-attaches the dimensioned-uncertain envelope when the result lands in the receipt; never a pint-aware wrapper threaded through kernel internals. The linear-propagation vs. Monte-Carlo dual path is selected at the strip boundary by problem shape.

[PY_SIG_001]:
- Owner: compute
- Work: no signal/spectral surface. Add a polymorphic `analyze(signal, ...)` route on the solver owner family over FFT/RFFT/STFT/CWT-wavelet/Welch-PSD/periodogram, FIR/IIR filter design + filtfilt zero-phase filtering, resampling/decimation, peak/envelope detection — all coordinate-and-unit-aware over xarray-labelled n-dimensional arrays, emitting a spectral receipt (window, NFFT, leakage, SNR) feeding the study and uncertainty rails.
- Exit: `analyze` resolves the FFT/STFT/wavelet/PSD/filter routes over labelled arrays and emits the spectral receipt. Depends on `PY_QUANT_001` (unit-aware axes).

[PY_CODEGEN_001]:
- Owner: compute
- Work: `SymbolicDerivation` is lambdify-codegen for the C# handoff only. Deepen to a verified lowering rail: author in sympy, derive exact Jacobians/Hessians/sensitivities, lower through numba `@njit`/jax `jit`/lambdify/C with an `EquivalenceLaw.Prove` gate certifying the lowered kernel agrees with the symbolic reference to certified tolerance BEFORE entering a solver lane; the lowered kernel feeds `PY_SOLVE_001` directly.
- Exit: the lowering rail proves equivalence to tolerance and the verified kernel enters a solve lane. CAPABILITY-BOUNDARY NOTE: the jax `jit` lowering target rides the marker floor (no cp315 wheel, `PY_API_003`), so the verified-jax lane is reachable only in the companion distribution. Depends on `PY_SOLVE_001`, `PY_RIGOR_001` (tolerance proof), `PY_API_003` (numba/jax marker floor).

[PY_LAKE_001]:
- Owner: data
- Work: `DatasetRef` treats Delta as one `DatasetKind` row and disclaims transactional ownership. Open a `lakehouse` page promoting Delta to a full transactional lifecycle — MERGE/upsert, time-travel by version or timestamp, OPTIMIZE compaction + Z-order clustering, VACUUM, schema evolution, partition evolution, CDC change-feed — all cases on one `TableOp` union over deltalake, each snapshot keyed by `ContentIdentity`. Bronze→silver→gold medallion promotion gated structurally by the quality firewall (`PY_QUAL_001`) and the table lifecycle.
- Exit: `TableOp` resolves the full lifecycle, snapshots key by `ContentIdentity`, and tier promotion is gated by `PY_QUAL_001`. Depends on `PY_API_002a` (deltalake cp315 wheel).

[PY_QUAL_001]:
- Owner: data
- Work: `ContractGate` is a single schema claim. Deepen into a `quality` page — a statistical firewall over pandera/scipy: per-column profiles (null-rate, cardinality, quantiles, histogram), PSI/KS distribution drift versus a pinned baseline content-hash, expectations-suite gating, and a quarantine-routing fold for failing rows. A dataset cannot promote a medallion tier unless its profile matches the contract and drift is under policy.
- Exit: the firewall profiles, detects drift versus baseline, gates promotion, and routes quarantine. Depends on `PY_API_002a` (pandera cp315 wheel).

[PY_QUERY_001]:
- Owner: data
- Work: `ScanPlan`/the query surface has no standing-query engine or cross-modal fusion. Deepen the query owner with a continuous standing-query engine (tumbling/sliding/session windows + watermarks + incremental view maintenance over polars/duckdb) where one definition powers a dashboard tile, an alert, a retention sweep, and a report; and a fusion-ranking surface fusing vector kNN (surrogate-embedding only — AI-out-of-scope respected), spatial (shapely/GiST), and text (BM25/tsvector) into one ranked result with per-hit lineage.
- Exit: one standing-query definition drives all four consumers and the fusion surface returns a single ranked result with lineage. PEER-PLANE to the C# V15 Federated Search + V16 Continuous-Query — PY18 is duckdb/polars-embedded, V15/V16 are pgvector/Timescale-server; federated by the shared lineage edge (the V8 join dimension), NOT a second federation owner. Depends on `PY_DET_001` (lineage), `PY_API_002a`.

[PY_DOC_002]:
- Owner: artifacts
- Work: `DocumentPlan` dispatches formats over an opaque payload and `ArtifactReceipt` carries no conformance/bundle/delta cases. Open a `docmodel` page with a semantic msgspec tagged-union document tree (page/section/block/run/table/figure/field/annotation/structure-element) that is the SINGLE interior representation every backend lowers FROM (reportlab/pymupdf canvas, OOXML via python-docx/pptx/openpyxl, WeasyPrint Paged-Media, SVG) and every extractor recovers TO (pymupdf layout segmentation + table detection + reading-order + AcroForm graph), backed by a duckdb/arrow document-query surface so a corpus is a queryable table; production and extraction are inverses over one node algebra. Extend `ArtifactReceipt` with `DocumentDelta`/`ExtractionProvenance` cases. The `GeometryDelta` algebra (`PY_PARA_001`) reuses this DocumentDelta node-edit shape.
- Exit: the DocumentModel round-trips produce→extract→re-produce identity-stable, the document-query surface answers corpus predicates, and `ArtifactReceipt` carries the new cases. Depends on `PY_API_004` (pillow/pymupdf chain).

### [6.2]-[MISSING] — new owned page or fold-in extension

[PY_DIFF_001]:
- Owner: compute (new `differentiable` page)
- Work: no owner for differentiation THROUGH a solve. Open a `differentiable` page: one `inverse(forward, objective, design)` owning PDE-constrained optimization — shape optimization, topology optimization (SIMP density / level-set with filtering+projection), full-waveform/EIT inverse reconstruction, parameter identification — with exact gradients computed THROUGH the forward solve without differentiating Newton or the linear solver internals. FORWARD COHERENCE (buildability gate): `PY_SIM_001`'s scikit-fem assembly is numpy/scipy-resident and NOT jax-traceable — a jax custom-VJP cannot trace the scipy sparse solve. The named architecture is a HAND-IMPLEMENTED DISCRETE ADJOINT over the scipy forward: assemble the transposed KKT/adjoint system from the same scikit-fem bilinear form, solve it on the `PY_SOLVE_001` sparse lane, contract with the design-parameter derivative; the outer fixed-point/optimization loop is driven by `optimistix` (Gauss-Newton/LM root + minimisation). The jax custom-VJP / IFT-of-fixed-points path is admitted ONLY against a jax-native forward (a jax-resident assembly behind a future jax-FEM pin), a SEPARATE forward object — never traced through scikit-fem. Emits a `SensitivityField` receipt (design gradient, adjoint residual, KKT stationarity). Distinct from `NumericIntent`'s jax grad/vmap row (which differentiates explicit expressions, not solves).
- Exit: `inverse` resolves the shape/topology/inverse-reconstruction/parameter-ID cases through the discrete adjoint over the scipy forward and emits `SensitivityField`. DISJOINT from C# V18 Differentiable Geometry — V18 differentiates FIXED DDG operators (Laplacian/heat/spectral/remeshing) in-host; PY3 differentiates an ARBITRARY assembled FEM/PDE solve via a discrete adjoint. Different forward object, no shared kernel. CRITICAL-PATH NOTE: `optimistix` (and any future jax-native forward) has no cp315 wheel and resolves only in the marker-floor companion distribution (`PY_API_003`), AND this row sits behind `PY_SIM_001` (FEM forward + transposed-form assembly) + `PY_FLOOR_003` pins — its true earliest-start is late despite the headline-frontier framing. Depends on `PY_SOLVE_001` (sparse lane for the adjoint solve), `PY_SIM_001` (FEM forward + transposed-form assembly), `PY_FLOOR_003` (`optimistix` pin), `PY_API_003` (jax/optimistix marker floor).

[PY_RIGOR_001]:
- Owner: compute (new `rigor` page)
- Work: no validated-numerics owner. Open a `rigor` page: `certify(expr, domain)` over interval arithmetic and arbitrary-precision midpoint-radius ball arithmetic (python-flint/arb) returning GUARANTEED enclosures with proven error bounds (rounding + truncation) — NOT the probabilistic confidence bands `PY_QUANT_001` produces. Certified root isolation (interval Newton / Krawczyk over arb balls), validated quadrature (arb rigorous integration), and univariate arb Taylor-series enclosures. Emits an `Enclosure` receipt (midpoint, radius, precision bits, proof-witness). The two-epistemics duality peer to `PY_QUANT_001`. OWNER-BACKED SCOPE: python-flint provides arb ball arithmetic + univariate Taylor series ONLY — multivariate Taylor models, verified ODE enclosures (CAPD-class), and rigorous global-optimization branch-and-bound have NO pinned PyPI owner and are DEFERRED out of `certify`'s case set until a maintained distribution is admitted; the FENCE_LAW forbids a card whose named members have no pin.
- Exit: `certify` returns a guaranteed `Enclosure` for the root-isolation/quadrature/Taylor-series cases. Depends on `PY_FLOOR_003` (`python-flint` admission, LGPL posture) + its cp315/marker-floor catalogue. DISJOINT from C# V2 interval-certified geometry — arb ball arithmetic certifies arbitrary numeric expressions, V2 certifies geometric predicates; no shared kernel.

[PY_TENSOR_001]:
- Owner: data (new `tensorstore` page)
- Work: no n-dimensional versioned-array owner — tensors are not tables. Open a `tensorstore` page: a transactional, time-travelled, branch/tag-able chunked-tensor store (Icechunk over Zarr v3 with sharding) for n-dimensional scientific arrays (time×band×depth×lat×lon, simulation field histories, gigavoxel TSDF grids), keyed by `ContentIdentity`. One `cube(...)` unifies xarray-labelled CF-convention datacubes (datacube folds in as the labelled-cube owner block), dask-array/cubed blockwise out-of-core execution (rechunk/map_overlap/blockwise — promoting chunked-array algebra above a Dask Substrate row), and Awkward ragged arrays for variable-connectivity meshes/per-cell variable point clouds. STAC Earth-observation collections and H3/S2 DGG planetary-aggregation binning are GATED behind their pins (`pystac`/`pystac-client`/`odc-stac`, `h3`/`s2sphere`, `PY_FLOOR_003`) — out of the `cube` member surface until admitted, per the FENCE_LAW. Emits a `CubeSnapshot` receipt with array-shard provenance.
- Exit: `cube` resolves the labelled/out-of-core/ragged cases (STAC/DGG cases land only once their pins are admitted), snapshots branch/tag/time-travel by `ContentIdentity`, and emits `CubeSnapshot`. ICECHUNK MATURITY: Icechunk's transaction/conflict-resolution semantics on concurrent commit are still hardening (`api-owners.md#[6]`); the `ContentIdentity`-keyed conflict-detection path is the schedule risk this row carries. Depends on `PY_FLOOR_003` (icechunk/zarr-v3/cubed/awkward; STAC/DGG pins gate their cases) + `PY_API_002a` (xarray/dask).

[PY_SIM_001]:
- Owner: compute/geometry-adjacent (new `simframe` page)
- Work: no SIMULATION mesh+field record owner. Open a `simframe` page: one `import`/`export` over CGNS, EXODUS-II, VTK/VTU/VTP/VTM, XDMF, MED/Salome, Gmsh-MSH, Tecplot, UGRID over the HDF5/NetCDF/Zarr container substrate (meshio + h5py + netcdf4 — meshio is already catalogued), where a mesh carries point-data/cell-data/field-data time series, disambiguated from the CAD/BIM/scan (`MeshPayload`/glTF/STEP/E57) and columnar (parquet/arrow) families by a simulation-format-family detector and round-trip-verified for field-attachment + topology conformance (`InteropField` receipt). Plus `assemble(form, basis)` lowering bilinear/linear weak forms to sparse matrices/vectors over arbitrary element bases (scikit-fem: Raviart-Thomas/Nedelec/MINI/Argyris/Crouzeix-Raviart) — the Galerkin layer above the existing PDE-stencil notion, feeding `PY_DIFF_001` — coupled to in-situ visualization + simulation steering (ParaView Catalyst v2 / VTK-m) emitting an `AssemblyReceipt` + `InSituFrame` provenance edge.
- Exit: `import`/`export` round-trips field-attachment for the sim-format family, `assemble` lowers weak forms over the element bases (and the transposed bilinear form `PY_DIFF_001`'s discrete adjoint consumes), and in-situ frames emit provenance edges. The canonical mesh the assembly consumes is a RUNTIME-owned dependency-free structural Protocol — vertices/faces/attributes as plain numpy buffers + a `ContentIdentity` key, NO trimesh/meshio import in runtime (`region-map/seam-splits.md#MESH_VALUE`) — to which the heavy backend meshes adapt at their boundary; compute/data/geometry consume the Protocol without cross-importing and runtime imports no first-wave package. The MESH-AS-DIFFERENTIABLE-DOMAIN law is non-violating only under this structural-Protocol residence, never a backed mesh value-object in runtime. Depends on `PY_SOLVE_001` (sparse lane), `PY_FLOOR_003` (scikit-fem/netcdf4/Catalyst; h5py/meshio already catalogued) + `PY_API_002a`/`PY_API_004`.

[PY_PARA_001]:
- Owner: geometry (new `parametric` page)
- Work: supersedes `PY_NEXT_001`. No constraint-graph solver and no object-graph geometry version control. Open a `parametric` page: `constrain(sketch, constraints)` (python-solvespace/variational) doing DOF analysis, well/over/under-constrained diagnosis, redundant-constraint detection, and sketch→exact-geometry resolution with a dragged-update Jacobian — well/over/under-constrained is a STRUCTURAL verdict, not a runtime error — fused with hash-deduplicated object-graph geometry diff/merge/three-way-conflict version control (specklepy-class; specklepy is already a runtime transport row), emitting a `ConstraintReceipt` (DOF count, redundancy) and a `GeometryDelta` (typed inserted/deleted/moved/reparametrized node edits keyed by `ContentIdentity`, reusing the `PY_DOC_002` DocumentDelta node-edit algebra).
- Exit: `constrain` returns the DOF verdict + dragged Jacobian and the object graph diffs/merges with typed conflict cases. `GeometryDelta` and `PY_DOC_002`'s `DocumentDelta` both CONSUME the one RUNTIME-owned node-edit delta algebra (`region-map/seam-splits.md#DELTA_ALGEBRA`) — neither package imports the other, resolving the artifacts→geometry direction that `architecture.md#DEPENDENCY_DIRECTION` does not permit. PYTHON-OWNS the DOF + object-graph-diff half; READS the C# V2 topological-naming wire (naming is C#-Persistence-owned), never minting a second naming authority. Depends on `PY_FLOOR_003` (python-solvespace) + `PY_DOC_002` (runtime delta algebra).

[PY_CONF_001]:
- Owner: artifacts (new `conformance` page)
- Work: no trust-boundary owner for emitted artifacts. Open a `conformance` page: every emission carries a typed `ConformanceGrade` (veraPDF-class PDF/A clause validation, PDF/UA tag-tree coverage, ICC/output-intent embedding, font-subset completeness) — compliance is a queryable graded property, not a pass/fail gate. Physical content redaction (remove from content stream, not overlay), incremental PDF signing (PAdES via pyhanko) + RFC-3161 timestamp + encryption, and a Merkle/hash-chained bundle manifest over `ContentKey` members (consumed FROM `ContentIdentity`, never re-minted) sealing a multi-member archive any peer verifies offline. Determinism contract: reproducible font subsetting + fixed XMP/timestamps + sorted xref so identical inputs hash identically. Extends `ArtifactReceipt` with `ConformanceGrade`/`BundleManifest` cases.
- Exit: every emission carries a per-clause `ConformanceGrade`, redaction is physical, signing/timestamp/sealing complete, and a bundle re-hashes offline. Depends on `PY_FLOOR_003` (pyhanko/fonttools; veraPDF is a JVM external-tool seam, not a pin), `PY_COLOR_001` (the deterministic color hash — scheduled BEFORE this row), + `PY_API_004` (pikepdf/pymupdf chain).

[PY_COLOR_001]:
- Owner: artifacts (new `color` page)
- Work: no print-physics color owner — `VisualSpec` composes figures but does not own gamut/separation. Open a `color` page: ICC profile transforms (littlecms via pillow), CMYK/spot separation, soft-proofing, wide-gamut (P3/Rec2020/scRGB-float) offscreen encode with ColorSpace folded into `ContentIdentity` so an sRGB and a P3 render of the same figure key distinctly, plus imposition/bleed/crop-mark/n-up sheet algebra. The deterministic-hash dependency `PY_CONF_001` relies on.
- Exit: the ICC/CMYK/wide-gamut transforms and imposition algebra resolve, and ColorSpace folds into the content key. Scheduled BEFORE `PY_CONF_001`, which consumes the deterministic color hash. Depends on `PY_API_004` (pillow/littlecms).

[PY_CAP_001]:
- Owner: runtime (new `capabilities` page)
- Work: no operation-metadata/permission/cost owner. Open a `capabilities` page: every operation (solve/study/geometry-op/IFC-query/artifact-render/dataset-query) registered as a typed, effect-classed, cost-modeled, permission-gated capability GENERATED from the canonical op surfaces — not hand-maintained. An intent planner does dry-run/cost-preview; the catalog projects to an MCP server so an external agent drives the runtime, and the SAME catalog generates a polyglot SDK and the (private) cyclopts CLI grammar. A new operation is one row, never a parallel surface.
- Exit: the capability registry is generated from the op surfaces, the intent planner dry-runs/cost-previews, and the MCP/SDK/CLI projections derive from the one registry. CONSUMES the C# V7 capability atom — PY17 projects the C# V7 registry to a Python-process MCP/SDK/CLI; it does NOT re-mint canonical op identity (V7 names Python as a polyglot SDK codegen target). GENERATION MECHANISM: Python has no compile-time source-generator form, so "generated, not hand-maintained" is RUNTIME INTROSPECTION — a decorator-registry over each canonical op surface + `beartype`/`msgspec` schema extraction at import builds the catalog; this is a category difference from the C# Roslyn generator, not a translation. Depends on `PY_ORCH_001` (effect/cost on DAG nodes).

[PY_DET_001]:
- Owner: runtime (fold into `ContentIdentity`/`Receipt`)
- Work: `ContentIdentity`/`Receipt` carry the content key but no determinism plane. Fold in a provider-fingerprint capture (BLAS vendor, thread count, SIMD level, library versions, RNG state) and extend the content-address closure over (inputs, code identity, provider fingerprint, RNG seed) keying every cached solver/study result — memoized reuse, replay-verify, AS-OF time-travel all derive from one stamp. Determinism is a DERIVABLE property, not a flag; cross-machine replay flags bit-divergence. A standalone `provenance` package is NOT opened — runtime already owns content-identity.
- Exit: the fingerprint is captured, the content closure keys cached results, and cross-machine replay produces a bit-divergence verdict. Depends on `PY_HASH_001` (XxHash128 pin). FOUNDATIONAL: consumed by `PY_STUDY_001` (lake key), `PY_QUERY_001` (lineage), `PY_ORCH_001` (node-memo key); promoted ahead of the value-rank-2 study row, not a mid-tier extension. EXTENDS the C# A6 provider-determinism fingerprint closure (same hash domain, adds Python BLAS/RNG/SIMD members), never a parallel key. FINGERPRINT-CANONICALIZATION PARITY EXIT: content-seed parity (`PY_HASH_001`) is necessary but not sufficient for "same hash domain" — the fingerprint-member serialization (how BLAS vendor / thread-count / SIMD-level / library-version / RNG-state canonicalize to bytes before the fold) must match the C# A6 member byte layout exactly, proven against the C# `receipts-and-benchmarks#BENCHMARK_CLAIMS` fingerprint output. Absent that proof the verb downgrades to PEER-PLANE (separate fingerprint federated by content key); EXTENDS holds only with the canonicalization-parity proof.

[PY_INGEST_001]:
- Owner: geometry (new `interchange` page)
- Work: no native CAD/BIM/scan ingest owner — the C# brief (`.artifacts/planning-briefs/aggressive-ideation.md#[4]` Python-companion note) EXPLICITLY names Python the home for native-format bridges (Revit/Navisworks importers), IFC5 parsing, CAD-STEP/AP242 (OpenCascade/pythonocc, A3-disjoint from the IFC `.step` extension), and the mesh/scan-format breadth — the consumer the prior `interchange` defer-verdict said was absent is the C# suite itself, already stated. Open an `interchange` page: one `ingest(source, ...)` over a codec table — STEP/IGES/AP242 via pythonocc-core (native OpenCascade B-rep), E57/LAS/LAZ point clouds via pye57/laspy/pdal, Revit/Navisworks native-format bridges, IFC5 parsing — each codec resolving to the canonical mesh Protocol (`MESH_VALUE`) or a B-rep/point-set value keyed by `ContentIdentity`, with a format-family detector disambiguating from the simulation (`PY_SIM_001`) and columnar families. The in-process plane the .NET stack structurally cannot reach (no native OCCT/E57 in-process).
- Exit: `ingest` resolves the STEP/IGES/AP242/E57/LAS/Revit/Navisworks/IFC5 cases to the canonical mesh Protocol or B-rep/point-set value, each keyed by `ContentIdentity`, and the format-family detector disambiguates. Depends on `PY_FLOOR_003` (pythonocc-core/laspy/pye57/pdal, native builds + marker-floor likely), `PY_SIM_001`/`MESH_VALUE` (canonical mesh Protocol). PYTHON-OWNS the in-process native-ingest plane; the C# suite is the named consumer over the existing companion wire.

[PY_INFER_001]:
- Owner: compute (new `inference` page)
- Work: no Bayesian-inference/posterior owner — `PY_QUANT_001` propagates FORWARD uncertainty (linear + Monte-Carlo) and `PY_STUDY_001`'s `calibrate` route names Bayesian calibration but no inference engine. Open an `inference` page: one `infer(model, data, ...)` over NumPyro/PyMC — MCMC/HMC/NUTS sampling, variational inference, posterior-predictive checks, ArviZ convergence diagnostics (R-hat, ESS, divergences), model comparison (WAIC/LOO/Bayes factors) — emitting a `PosteriorReceipt` (sampler, draws, R-hat, ESS, divergence count, log-evidence). This is classical inferential statistics, NOT the excluded generative/deep-learning AI frontier (`python-ideation.md#7` narrows the exclusion to generative/deep-learning only). The inverse/posterior complement to `PY_QUANT_001`'s forward path and the engine `PY_STUDY_001`'s `calibrate` route names.
- Exit: `infer` resolves the MCMC/HMC/VI/posterior-predictive/model-comparison cases, emits `PosteriorReceipt`, and the `PY_STUDY_001` calibrate route consumes it as its engine. Depends on `PY_FLOOR_003` (numpyro/pymc/arviz; numpyro rides the jax marker floor `PY_API_003`), `PY_QUANT_001` (uncertainty axes).

### [6.3]-[MISSING] — genuinely-Python-native frontiers the five-package silhouette could not express

Concepts the corpus structurally cannot model and the C# stack does not claim (`.artifacts/planning-briefs/python-ideation.md#[3.2.1]`). The PyData/scientific-Python ecosystem owns these better than any .NET stack; their absence was the shape-bias of anchoring the pool to the existing five owners. AI/ML/generative-design stays out of scope.

[PY_NB_001]:
- Owner: runtime (new `notebook` page)
- Work: no reproducible-scientific-authorship owner. Open a `notebook` page: parametrized headless computational-notebook execution (papermill/nbclient/marimo), content-addressed cell outputs reusing the `PY_DET_001` closure, export-to-replay-script, study/solve receipts rendered inline. NOT AI/ML — the reproducible-authorship plane. Peer to the C# V10 "reproducible computational notebooks" but Python-process-resident; CONSUMES the `PY_CAP_001` capability catalog for pinned-capability cells, never re-minting op identity.
- Exit: a parametrized notebook runs headless, cell outputs key by `ContentIdentity`, and export-to-replay reproduces bit-for-bit. Depends on `PY_FLOOR_003` (papermill/nbclient), `PY_DET_001`, `PY_CAP_001`.

[PY_INTEROP_001]:
- Owner: data (deepen `ColumnarEgress`/`ScanPlan`)
- Work: no dataframe-agnostic zero-copy carrier. Deepen the columnar owner with the interchange protocol (`__dataframe__`, narwhals backend-agnostic ops, Arrow PyCapsule / nanoarrow C Data Interface) owning the cross-library polars↔pandas↔duckdb↔cudf zero-copy plane. The C# register flags "Arrow zero-copy plane is not the modeled carrier" as a Persistence gap; Python OWNS it.
- Exit: a frame crosses polars/pandas/duckdb/cudf zero-copy through the one carrier and the egress receipt records the interchange path. Depends on `PY_FLOOR_003` (narwhals/nanoarrow), `PY_API_002a`.

[PY_SPATIAL_001]:
- Owner: compute (new `spatial` route on the solver owner family)
- Work: no array-native computational-geometry owner — neither IFC nor scan nor FEM nor constraints. Add a `spatial` route: `scipy.spatial` cKDTree, Voronoi/Delaunay/ConvexHull/Qhull, alpha shapes, Hausdorff, nearest-neighbour spatial joins over a numpy point set — the workhorse of scientific geometry. Emits a `SpatialIndex` surface over labelled arrays.
- Exit: the KD-tree/Delaunay/Voronoi/ConvexHull/alpha-shape/Hausdorff routes resolve over numpy point sets. Depends on `PY_API_003` (scipy).

[PY_IMG_001]:
- Owner: artifacts (new `imaging` page)
- Work: no scientific image-processing owner — the raster/voxel peer to point-cloud `ScanProcessing`. Open an `imaging` page: scikit-image / scipy.ndimage segmentation, morphology, registration, feature extraction over raster/voxel arrays. The natural consumer of `PY_TENSOR_001`'s voxel/TSDF cube. Emits an `ImageField` owner over labelled arrays.
- Exit: segmentation/morphology/registration/feature-extraction resolve over raster and voxel arrays and consume the tensor cube. Depends on `PY_FLOOR_003` (scikit-image, marker-floor confirm), `PY_API_004`.

[PY_GRAPH_001]:
- Owner: data (deepen `GraphPayload`)
- Work: `GraphPayload` is a single payload+algorithm shape with no rustworkx acceleration or full networkx algorithm surface (re-graded PARTIAL from EXISTS). Deepen to the network-science family: networkx→rustworkx accelerated backend, community detection, centralities, max-flow, spectral graph analysis (GNN-free). A real PyData frontier the C# stack does not own.
- Exit: the accelerated backend resolves the centrality/community/flow/spectral routes and the payload carries the algorithm receipt. Depends on `PY_FLOOR_003` (rustworkx), `PY_API_002a` (networkx captured).

[PY_STREAM_001]:
- Owner: runtime (fold into `LanePolicy`/`StagePlan`)
- Work: no bounded-memory streaming-iterator primitive — implied across `PY_TENSOR_001`/`PY_SIM_001`/`PY_ORCH_001`, owned by none. Fold a chunked-streaming-iterator extension into the lane owner that lets a 500GB point cloud or multi-TB simulation history flow through a transform under a memory cap, draining through the existing `LanePolicy` bound. A standalone streaming package is NOT opened.
- Exit: a transform streams a larger-than-memory source under a bounded-memory lane and the drain receipt records the chunk schedule. Depends on `PY_DET_001` (chunk content-address).

[PY_ORCH_001]:
- Owner: runtime (fold into `StagePlan`/`LanePolicy`)
- Work: `StagePlan` owns multi-stage DAG but not pipeline-as-data orchestration, automation triggers, or a durable queue. Fold in: a typed job-graph with topological scheduling + content-addressed node memoization (unchanged sub-results reused, only affected nodes recompute on input delta — the data analogue of the C# affected-project law), one `Trigger` union folding watchfiles filesystem-watch / croniter cron / inbound-gRPC verb / object-store-event, and a durable SQLite-WAL / object-store journal with exactly-once replay surviving daemon restarts. One pipeline definition powers a watch-triggered local run, a cron service run, and a companion-offloaded farm run. A standalone `orchestration` package is NOT opened.
- Exit: the DAG memoizes by content address, the `Trigger` union fires pipelines, and the durable queue replays exactly-once across restarts. SPLIT VERB: the node-memoization half CONSUMES the C# V6 job-graph node identity — the Python DAG is a runtime peer federated by the node content-address (the affected-project law analogue), not a forked scheduler vocabulary; the `Trigger` union (watchfiles/croniter/inbound-gRPC/object-store-event) and the durable SQLite-WAL exactly-once queue are PYTHON-OWNS — independently Python-resident, consuming no C# node identity (the same split-verb treatment `PY_PARA_001` applies). Depends on `PY_FLOOR_003` (croniter; watchfiles already catalogued) + `PY_DET_001` (node memo key).
