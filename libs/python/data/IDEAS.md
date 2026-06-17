# [PY_DATA_IDEAS]

The forward pool of higher-order concepts for `data`, grounded in the host-free interchange role. Each idea is a card — slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

## [1]-[OPEN]

[INCREMENTAL_CDC_MATERIALIZE]: fold the Delta changefeed, the relational query engine, and content-identity into an incrementally-materialized derived-snapshot owner that recomputes only changed partitions.
- A derived-view spec reads the `lakehouse` `ChangeFeed` between two snapshot versions, routes the changed rows through the `query` engine, and re-keys only the touched partition bundles by `ContentIdentity`, leaving unchanged content-keys untouched.
- Unlocks cheap incremental refresh of derived analytical Arrow snapshots over large Delta tables, turning a full re-scan into a partition-delta recompute keyed by content identity, the foundation of a reproducible derived-data layer.
- Draws on the gap that the changefeed and the query engine exist as separate owners with no incremental-materialization concept between them; Delta Change Data Feed plus content-addressed partition keys make delta-only recompute a first-class data capability that no current owner expresses.

[GEOARROW_SPATIAL_ENGINE]: add GeoArrow/GeoParquet-1.1 native-encoding egress and a DuckDB-spatial join/index engine to `geospatial` as one spatial-query axis.
- Native `geometry_encoding="geoarrow"` egress through geopandas `io.arrow`, plus DuckDB-spatial ST joins, point-in-polygon, R-tree prefilter, and H3 binning as rows on the spatial surface.
- Unlocks columnar spatial joins and H3 indexing in plain Arrow and SQL, scalable site and coverage analysis without heavy GIS, and smaller GeoArrow egress for the wire.
- Draws on the gap that the geospatial owner emits only WKB GeoJSON/GeoParquet/FlatGeobuf with no spatial join or index; GeoParquet 1.1 GeoArrow encoding and DuckDB-spatial joins outperform STRtree/sjoin on large vector sets.

[IBIS_PORTABLE_IR]: add an Ibis backend-agnostic expression-IR `QuerySpec` case to `query`, beside DuckDB and narwhals as the analytical-intent portability axis.
- One Ibis expression compiles to DuckDB/DataFusion/Polars/remote SQL and materializes to uniform Arrow, decoupling analytical logic from any single engine.
- Unlocks authoring analytical logic once and running it against any engine or remote warehouse, with cross-backend predicate-pushdown and no rewrite when the backend changes.
- Draws on the gap that `ibis-framework` is admitted with no consumer and the query owner carries only DuckDB and narwhals; Ibis (20+ backends, lazy SQL) and narwhals (input-type agnosticism) solve different portability axes and both belong on the owner.

[TABLE_FORMAT_AXIS]: generalize the `lakehouse` table-format binding into one `TableFormat` axis on the same `Lakehouse` owner.
- Delta now, Iceberg via `pyiceberg` (MERGE plus REST-catalog), and Lance for multimodal/AI-asset versioning, where a new format is one axis row dispatching `_apply` to its provider, never a parallel owner.
- Unlocks a vendor-neutral lakehouse across Delta/Iceberg/Lance from one owner, AI-asset versioning via Lance, and cross-engine catalog interop via Iceberg REST.
- Draws on the gap that the lakehouse owner hardcodes deltalake while the axis is designed-for but unbuilt; Iceberg and Lance matured into full-featured Python writers, leaving the owner single-format.

[POINTCLOUD_SCAN_EXCHANGE]: add a point-cloud interchange row to `mesh` — LAS/LAZ/COPC read/write feeding the geometry scan companion.
- LAS/LAZ/COPC decode/encode over laspy and pdal, octree-chunked spatial-subset requests, and a numpy/Arrow columnar point-record bridge, feeding the geometry scan-processing/registration at the mesh seam.
- Unlocks host-free lidar/scan ingest and cloud-optimized subset retrieval as the data-side input to the geometry scan-to-BIM companion, closing the reality-capture leg without RhinoCommon coupling.
- Draws on the gap that the mesh owner carries only FE (meshio) and surface (trimesh) meshes while AEC reality-capture centers on LAS/LAZ/COPC lidar; COPC is the cloud-native scan standard and is absent.

[VIRTUAL_REFERENCE_CUBE]: add a VirtualiZarr virtual-reference row to `tensor` — metadata-only chunk manifests over archival byte ranges committed to icechunk.
- Chunk-manifest references over archival HDF5/NetCDF/GeoTIFF byte ranges, combined via xarray into one virtual datacube committed to the transactional icechunk store with zero copy.
- Unlocks zero-copy cloud-optimized access to large archival HDF5/NetCDF/GeoTIFF as virtual zarr datacubes, a major egress-cost and storage win for the scientific-input leg.
- Draws on the gap that the tensor owner materializes into zarr while VirtualiZarr makes archival files queryable as virtual zarr stores via byte-range manifests, complementing the admitted icechunk engine.

## [2]-[CLOSED]

[ARROW_PYCAPSULE_INTEROP]: dropped — the `interop/frame.md` design page already carries the decision-complete `ArrowCStream` blueprint over `arro3-core`/`nanoarrow`, so the pyarrow-free Arrow carrier is build work in `TASKLOG.md`, not a forward concept; an idea fully fenced on its page is no longer in the forward pool.
