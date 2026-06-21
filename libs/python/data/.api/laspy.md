# [PY_DATA_API_LASPY]

`laspy` supplies LAS/LAZ point-cloud file IO for the data scan-exchange rail and owns the COPC octree-subset read. The package reads, writes, appends, and converts LAS 1.0–1.4 / LAS 2.0 records, manages extra dimensions and CRS, and runs spatial/level-of-detail queries against Cloud-Optimized Point Cloud files through `laspy.copc.CopcReader`; it never re-implements binary LAS parsing or the LASzip range codec. cp315-core `laspy.copc` reads UNCOMPRESSED COPC only — compressed `.copc.laz`/LAZ rides the companion `lazrs`/`laszip` backend (`.api/lazrs.md`, `.api/laszip.md`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `laspy`
- package: `laspy`
- import: `import laspy`
- owner: `data`
- rail: scan-exchange
- installed: `2.7.0` reflection-verified (pure-Python wheel, cp315-clean)
- entry points: `import laspy` plus the `laspy` console script (`laspy.cli`)
- capability: LAS/LAZ read/write/append/convert, mmap access, extra-dimension and CRS management, VLR/EVLR access, and the COPC octree-subset spatial/LOD read via `laspy.copc.CopcReader`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: LAS/LAZ surface
- rail: scan-exchange

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]     | [CAPABILITY]                                        |
| :-----: | :---------------------- | :---------------- | :-------------------------------------------------- |
|  [01]   | `LasData`               | in-memory file    | `x`, `y`, `z`, `header`, `points`, `xyz`, `write`   |
|  [02]   | `LasHeader`             | header record     | version, point format, scale/offset, bounds, CRS    |
|  [03]   | `PointFormat`           | format descriptor | `id`, `dimension_names`, dtype, standard/extra dims |
|  [04]   | `ScaleAwarePointRecord` | scaled buffer     | per-point storage with scale/offset applied         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: LAS/LAZ IO operations
- rail: scan-exchange

| [INDEX] | [SURFACE]                                                            | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `laspy.read(source, *, laz_backend=(), decompression_selection=...)` | eager read     | load full LAS/LAZ into `LasData`  |
|  [02]   | `LasData.x` / `LasData.y` / `LasData.z`                              | dimension      | scaled coordinate dimension array |
|  [03]   | `LasData.header`                                                     | metadata       | the `LasHeader` of the record     |
|  [04]   | `LasData.write(destination, *, do_compress=None, laz_backend=None)`  | write          | write LAS/LAZ to path or stream   |
|  [05]   | `PointFormat.id`                                                     | descriptor     | point-format integer identifier   |
|  [06]   | `PointFormat.dimension_names`                                        | descriptor     | iterator of dimension names       |
|  [07]   | `LasHeader.parse_crs(prefer_wkt=True) -> Optional[pyproj.CRS]`       | metadata       | parse CRS from header VLRs        |

## [04]-[COPC_SUBSET]

[COPC_SCOPE]: octree-subset read
- rail: scan-exchange
- engine: `laspy.copc.CopcReader` reads LAZ-1.4 COPC files organized as an octree, exposing spatial-bounds and level-of-detail queries that return a `ScaleAwarePointRecord` without decoding the whole file.

| [INDEX] | [SYMBOL]                                                            | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------------------------ | :------------ | :------------------------------------------------------------------ |
|  [01]   | `laspy.copc.CopcReader`                                             | COPC reader   | octree-indexed spatial/LOD reader over local paths and HTTP URLs    |
|  [02]   | `laspy.copc.Bounds(mins: np.ndarray, maxs: np.ndarray)`             | bounds record | `overlaps(other)`, `ensure_3d(mins, maxs)` axis-aligned query box   |
|  [03]   | `laspy.ScaleAwarePointRecord(array, point_format, scales, offsets)` | scaled buffer | scale/offset-aware point record; `zeros(...)`/`empty(...)` builders |

| [INDEX] | [SURFACE]                                                                                    | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `CopcReader.open(source, *, http_num_threads=80, decompression_selection=...) -> CopcReader` | creation       | open COPC file path or HTTP URL        |
|  [02]   | `CopcReader.query(bounds=None, resolution=None, level=None) -> ScaleAwarePointRecord`        | spatial        | combined bounds + resolution/LOD query |
|  [03]   | `Bounds.overlaps(other) -> bool`                                                             | predicate      | axis-aligned overlap test              |
|  [04]   | `Bounds.ensure_3d(mins, maxs) -> Bounds`                                                     | projection     | promote 2D bounds to 3D box            |

## [05]-[IMPLEMENTATION_LAW]

[COPC_TOPOLOGY]:
- `CopcReader.open(source)` accepts a local path, an HTTP/HTTPS URL (via `requests`), or a file-like object; it reads the COPC header and root octree page eagerly, then serves `query` calls lazily over the octree.
- `query(bounds, resolution, level)` discriminates the read: `bounds` is a `laspy.copc.Bounds` (2D bounds skip Z filtering, promoted via `ensure_3d`); `resolution` and `level` are mutually exclusive — `resolution` derives an octree level range from `copc_info.spacing`, `level` selects an explicit `int` or `range` LOD; the result is a `ScaleAwarePointRecord`.
- `ScaleAwarePointRecord` extends `PackedPointRecord` with `scales`/`offsets` arrays of shape `(3,)`, exposing scaled `x`/`y`/`z` coordinates directly.

[BACKEND_LAW]:
- cp315-core `laspy.copc` reads UNCOMPRESSED COPC only; `CopcReader.__init__` raises `LazError` when the `lazrs` backend is absent.
- Compressed COPC/LAZ decoding rides the companion backend on the `<3.15` band: `lazrs` (`.api/lazrs.md`, Rust `laz-rs`) is the default and required COPC backend, with `laszip` (`.api/laszip.md`, native LASzip) the alternative; `laspy` selects between them through `LazBackend.{Lazrs, LazrsParallel, Laszip}`.

[RAIL_LAW]:
- Package: `laspy`
- Owns: LAS/LAZ point-cloud file read/write/append/convert and the COPC octree-subset spatial query
- Accept: LAS 1.0–2.0 files, LAZ via `lazrs`/`laszip` backend, and COPC files or HTTP URLs through `CopcReader`
- Reject: hand-rolled binary LAS parsing; direct byte manipulation of point records; re-implementing the LASzip range codec the companion backend owns
