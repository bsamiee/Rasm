# [SPIKE_INFRA_NOTES]

Living log of gaps, shortcomings, side-effects, and integration ideation for the Rasm tier-2 spike infrastructure provisioned in `~/Documents/99.Github/Parametric_Forge`. Hand to the Forge agent to harden the spike toolchain. The orchestrator is the single writer; campaign workflow agents report infra findings in their structured returns and the orchestrator consolidates them here.

## [1]-[INFRA_SURFACE]

Provisioned in Parametric_Forge (deployed `darwin-rebuild switch ...#macbook`):
- `modules/home/programs/languages/scientific-tools.nix` — the `forge-scientific-env` `writeShellApplication` wrapper: native build/runtime exports (gfortran CC/CXX/FC, GDAL/GEOS/PROJ/HDF5/netCDF/Arrow/OpenBLAS, ONNX Runtime) + `UV_NO_MANAGED_PYTHON=1` / `UV_PYTHON_DOWNLOADS=never` / `MACOSX_DEPLOYMENT_TARGET=14.0`. Source-builds the 9 un-gated scientific packages.
- `modules/home/scripts/provisioning/rasm-spike-stack/default.nix` — `rasm-spike-stack` (`up`/`down`/`status`/`psql-timescale`/`psql-search`/`env`); Timescale PG18 (`timescaledb-ha:pg18.4-ts2.27.2-all`) + ParadeDB (`paradedb:0.24.0`) public images; labelled, operator-container-safe teardown; assets under `.artifacts/spikes/provisioning/`.
- `modules/home/programs/languages/db-tools.nix` — `rasmPostgres18` local package-set (timescaledb/postgis/pg_search/pgvector/pgvectorscale/pg_duckdb/pgaudit), Darwin-broken extensions guarded out.
- `modules/home/programs/languages/dev-tools.nix` — `ilspycmd` + `nuget-to-json` (drive the `assay api` decompile rail).
- `overlays/duckdb/default.nix` — DuckDB CLI 1.5.3 (matches the C# DuckDB.NET 1.5.3 line).

## [2]-[TOOLCHAIN_GAPS] — scientific-tools.nix

- **MISSING `eigen`** — blocks un-gating `small-gicp` (pybind11 + Eigen header-only source build). Medium: un-gates parallel GICP/VGICP point-cloud registration on cp315.
- **MISSING `pdal` + `boost`** — blocks un-gating `pdal` (sdist build needs system PDAL + GDAL/GEOS/PROJ + Boost). Medium: un-gates the point-cloud pipeline on cp315.
- `ONNXRUNTIME_LIB` exports `libonnxruntime.${version}.dylib` — confirm that version-suffixed file exists on the macOS onnxruntime derivation (the unversioned `libonnxruntime.dylib` may be the only real name). Low (cosmetic env var; the Python onnxruntime row is gated regardless).
- `suitesparse` only needed if a sparse-LA package beyond scipy's bundled CHOLMOD path is admitted (none today). Low.

## [3]-[SPIKE_STACK_GAPS] — rasm-spike-stack

- `_verify_timescale` / `_verify_search` probe only `timescaledb` + `pg_search`. The `-all` Timescale image bundles pgvector/pgvectorscale/postgis and ParadeDB bundles pgvector, but the script does not `CREATE EXTENSION`/verify them. Add a `verify-extensions` subcommand probing the full Persistence set (pgvector, pgvectorscale, postgis, pg_search, hypothetically pg_duckdb). Medium for Persistence tier-2 SPIKE proofs.
- No DuckDB / ONNX / headless-Avalonia / BLAS container — correct by design (toolchain/.NET-test-surface checks, not long-lived services); campaign SPIKE-closure agents must run those probes against `forge-scientific-env` + the .NET test host, not this stack.
- Fixed ports 55432/55433; document the collision-avoidance env overrides (`RASM_TIMESCALE_PORT`/`RASM_SEARCH_PORT`) prominently.

## [4]-[CP315_PACKAGE_GAPS] — package-level, not infra (tracked for completeness)

- `lonboard` -> `geoarrow-rust-core` caps cp314 (no cp315 wheel/sdist) -> gated `python_version<'3.15'`.
- Keep-gated cp315 blockers: connectorx / icechunk (Rust no-cp315), jax/jaxlib / numba / numpyro / onnxruntime / optimistix / pye57 / python-flint / scikit-image / scikit-learn (CPython/ABI ceiling), pdal (sdist + toolchain). Companion `<3.13`: ifcopenshell / open3d / vtk / pyvista / topologicpy / grpcio / small-gicp.

## [5]-[ASSAY_INTEGRATION_IDEATION] — TBD (dedicated ideation pass)

Open question to resolve truthfully with a small ideation pass: should the spike provisioning + scientific toolchain surface through `tools/assay` (e.g. `assay spike up|down|status|env`, `assay env`) so the campaign drives spike proofs through one repo-owned tool instead of a Home-Manager bash script — given `assay api` already owns decompile/reflection and `assay static/test/bridge/package` own gating rails? Proposal (pros/cons + concrete shape) to be appended here.

## [6]-[OPEN_QUESTIONS_FOR_FORGE]

- (appended as the campaign surfaces them)
