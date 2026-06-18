# [PY_DATA_TASKLOG]

Open and closed work for `data`, distilled from `IDEAS.md`. Each task card leads with a status marker — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` open; `[COMPLETE]` or `[DROPPED]` closed — and carries the capability or file to build, the external packages to integrate, the integration points and boundaries, and the key considerations. `[1]-[OPEN]` holds live work; `[2]-[CLOSED]` records finished or dropped tasks.

## [1]-[OPEN]

[QUEUED] Build the incremental CDC-materialization owner in `columnar/dataset.py`.
- Capability: a derived-snapshot spec that reads a `lakehouse` `ChangeFeed` between two versions, routes changed rows through the `query` engine, and re-keys only the touched partition bundles by `ContentIdentity`, leaving unchanged content-keys intact — a partition-delta recompute, never a full re-scan.
- Packages: `deltalake` (the `load_cdf` change feed with `_change_type`/`_commit_version`), `duckdb` (the partition-delta recompute), `pyarrow` (the snapshot bundles).
- Integration: composes the `lakehouse` `ChangeFeed` op and the `query` `QueryEngine` rather than re-minting either; the derived snapshot folds the shared `QueryReceipt` and keys partitions by the one `ContentIdentity`; internal to the `columnar` owner, never a parallel materialization module.
- Considerations: the changed-partition set derives from the CDF `_commit_version` range, so an unchanged partition's content-key is reused untouched; the recompute stays lazy through the `query` engine so the delta pushes into the scan.

[QUEUED] Add the GeoArrow egress and DuckDB-spatial engine rows to `geospatial/claim.py`.
- Capability: a native `geometry_encoding="geoarrow"` `SpatialEgress` row and a DuckDB-spatial join/index engine (ST joins, point-in-polygon, R-tree prefilter, H3 binning) as a spatial-query axis on the geospatial surface.
- Packages: `geopandas` (`io.arrow` GeoArrow egress), `duckdb` (the spatial extension ST joins and H3 functions), `pyproj` (CRS normalization on the join inputs).
- Integration: internal to the `geospatial` owner; the spatial-query axis emits plain Arrow/SQL results keyed by one `ContentIdentity`, composing the `query` engine for the SQL path; lonboard/GeoArrow visualization stays at `artifacts`, never coupled here.
- Considerations: GeoParquet 1.1 GeoArrow encoding and DuckDB-spatial joins outperform STRtree/sjoin on large sets; the H3 binning belongs to the spatial axis, not a parallel index owner.

[QUEUED] Add the Ibis backend-agnostic IR `QuerySpec` case to `query/relational.py`.
- Capability: a `QuerySpec.Ir` case compiling one Ibis expression to DuckDB/DataFusion/Polars/remote SQL and materializing to the same uniform `pyarrow.Table` the other cases produce.
- Packages: `ibis-framework` (the lazy expression IR and the 20+ backend compilers), `pyarrow` (the uniform result binding), `adbc-driver-manager` (cp315 source-build remote transport), `connectorx` (the `<3.15` gated-band fast SQL-to-Arrow transport).
- Integration: internal to the `query` owner as one more `QuerySpec` row; remote-connection acquisition routes through the runtime `TransportResource`, never a second transport owner; the result folds into the shared `QueryReceipt`.
- Considerations: Ibis (analytical-intent portability) and narwhals (input-type agnosticism) are distinct axes and both stay on the owner; `connectorx` rides the `<3.15` gated band, so its transport arm imports function-local under `# noqa: PLC0415`, never a module-top import on the cp315-core query page; confirm the Ibis-to-Arrow materialization spelling against the live distribution.

[QUEUED] Generalize the `lakehouse/table.py` binding into one `TableFormat` axis.
- Capability: a `TableFormat` axis on `Lakehouse` dispatching `_apply` to the format provider — Delta now, Iceberg (MERGE plus REST-catalog), Lance (multimodal/AI-asset versioning) — so a new format is one axis row, never a parallel owner.
- Packages: `deltalake` (the current Delta provider), `pyiceberg` (Iceberg MERGE and REST catalog), `lance` (multimodal versioning), `pyarrow` (the shared Arrow read/write surface).
- Integration: internal to the `lakehouse` owner; each format keys its commit by the same `ContentIdentity` and emits one `LakeReceipt`; the `query` engine reads any format's Arrow snapshot through `from_arrow`.
- Considerations: the `LakeOp` axis is format-agnostic, so the format binding is a separate discriminant from the operation; confirm the `pyiceberg` MERGE and the `lance` write entrypoints against the live distributions before admitting the rows.

[QUEUED] Add the LAS/LAZ/COPC point-cloud row to `mesh/exchange.py`.
- Capability: a point-cloud `MeshBackend` row reading and writing LAS/LAZ/COPC with octree-chunked spatial-subset requests and a numpy/Arrow columnar point-record bridge, feeding the geometry scan companion at the mesh seam.
- Packages: `laspy` (LAS/LAZ point-record decode/encode), `pdal` (the COPC octree-chunked spatial-subset pipeline), `pyarrow` (the columnar point-record bridge).
- Integration: internal to the `mesh` owner as host-free file exchange; the point records cross to the geometry scan-processing/registration companion at the wire (content-identity plus the columnar bridge), never coupled to a managed interior.
- Considerations: COPC is the cloud-native scan standard, so the subset request rides the COPC octree rather than a full read; the bridge stays columnar so the geometry companion consumes Arrow, not a pdal-specific object.

[QUEUED] Add the VirtualiZarr virtual-reference row to `tensor/store.py`.
- Capability: a `TensorBackend="virtual"` row building metadata-only chunk-manifest references over archival HDF5/NetCDF/GeoTIFF byte ranges, combined via xarray into one virtual datacube committed to the icechunk transactional store.
- Packages: `virtualizarr` (the cp315-clean byte-range chunk-manifest virtual reference, module-top), `xarray` (combining references into one datacube), `icechunk` (the transactional commit, the one `<3.15` gated arm), `zarr` (the virtual-store façade), `h5py` (the archival HDF5 reader).
- Integration: internal to the `tensor` owner as one more backend row; the virtual store presents the same `TensorStore` surface and keys by `ContentIdentity`, committing through the existing icechunk session; never a parallel store owner.
- Considerations: `virtualizarr`/`zarr`/`xarray`/`h5py` are cp315-clean and import module-top, while only the `icechunk` commit arm rides the `python_version<'3.15'` gated band and imports the dist function-local under `# noqa: PLC0415`, never a module-top import on the cp315-core tensor page; the reference is zero-copy (manifest only), so write paths stay read-through; confirm the `virtualizarr`-to-icechunk commit spelling against the live distribution before treating the row as the archival-access default.

[QUEUED] Author the missing folder `.api` catalogues for the cp315-clean fenced interchange libraries.
- Capability: a folder `.api/<package>.md` catalogue, decompile/reflection-verified via `assay api`, for every library transcribed as settled fence code that has no catalogue today — `narwhals` (the interop/query/admission agnostic-frame surface), `zarr`/`cubed`/`awkward` (the tensor store/plan/ragged surface), `nanoarrow`/`arro3-core` (the pyarrow-free Arrow C-data carrier) — so each settled member verifies against its catalogue per the one-`.api`-per-library law.
- Packages: `narwhals`, `zarr`, `cubed`, `awkward`, `nanoarrow`, `arro3-core` (all cp315-clean, installable on the core for reflection); `virtualizarr`/`icechunk`/`xarray` catalogues ride the tensor virtual-reference task.
- Integration: internal documentation work feeding `interop/frame.md`, `query/relational.md`, `contracts/admission.md`, and `tensor/store.md`, retiring each page's catalogue-pending RESEARCH item as its catalogue lands; the catalogue is the resource the build pass reads to draw the real API without guessing.
- Considerations: the gated/uninstalled dists (`connectorx`, `pyiceberg`, `lance`, `obstore`, `laspy`, `pdal`, `ibis-framework`) stay marked RESEARCH until their provenance installs — an honest deferral, not a false-done; only the cp315-clean rows above catalogue now.

[QUEUED] Build the planned `cloud-egress/store.py` `ObjectEgress` owner.
- Capability: native object-store egress (put/get/list/delete) over `obstore` for the Arrow/Parquet/GeoParquet/zarr bundles the other owners emit, keyed by `ContentIdentity`.
- Packages: `obstore` (the highest-throughput native object-store path), `pyarrow` (the bundle payloads).
- Integration: composes the runtime `TransportResource`/`ResourceRef` for credentials and endpoint, never a second transport owner; consumes the egress bundles from `columnar`, `geospatial`, and `tensor` rather than re-minting them.
- Considerations: `obstore` is admitted but unused, the visible cloud-egress gap; the owner stays an egress façade over the runtime transport, not a duplicate fsspec/transport layer.

## [2]-[CLOSED]

None.
