# [PY_GEOMETRY_API_PYE57]

`pye57` supplies E57 point-cloud file IO over the libe57 (`xerces-c`-backed) C++ core: an `E57` file handle for multi-scan read/write and field-buffer allocation, a `ScanHeader` typed view over the per-scan libe57 structure node (pose, index/intensity/cartesian/spherical bounds, acquisition times, point count), the `COORDINATE_SYSTEMS` enum carrying the Cartesian/spherical field-name maps, and a `convert_spherical_to_cartesian` projection. The scan-ingestion owner composes `E57.read_scan` into a field-keyed `numpy` dict, hands the XYZ block to `laspy`/`open3d`/`pdal` consumers across the wire, and never hand-rolls E57 binary parsing or raw libe57 node manipulation.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pye57`
- package: `pye57`
- import: `from pye57 import E57, ScanHeader` (or `import pye57`); `E57`/`ScanHeader`/`libe57` are the only top-level re-exports — `COORDINATE_SYSTEMS`, `SUPPORTED_*_POINT_FIELDS`, and `convert_spherical_to_cartesian` live in `pye57.e57`/`pye57.utils`
- owner: `geometry`
- rail: scan-processing
- installed: `0.4.19`
- entry points: none (library only)
- capability: multi-scan E57 read/write, single- and multi-field `numpy` buffer allocation against the libe57 prototype, Cartesian/spherical intake with inline pose-to-global transform and invalid-state masking, quaternion pose math, typed `ScanHeader` access to every libe57 metadata node, and raw scan read/write bypassing the conditioning path

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file, scan, and coordinate-system family
- rail: scan-processing

`pye57.libe57` exposes the raw libe57 node classes — `ImageFile`, `StructureNode`, `FloatNode`, `CompressedVectorNode`, `SourceDestBuffer`, `E57Exception` — as the boundary escape hatch.

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]   | [CAPABILITY]                                                                   |
| :-----: | :------------------- | :-------------- | :----------------------------------------------------------------------------- |
|  [01]   | `E57`                | file handle     | multi-scan read/write, buffer alloc, global-frame transform, ctx-manager close |
|  [02]   | `ScanHeader`         | scan metadata   | typed view of a libe57 scan node: pose, bounds, acquisition, count, fields     |
|  [03]   | `COORDINATE_SYSTEMS` | `enum.Enum`     | `CARTESIAN`/`SPHERICAL`; `.value` is the field-name->dtype-char map            |
|  [04]   | `pye57.libe57`       | C++ node module | raw libe57 node classes (see lead); boundary-only escape hatch                 |
|  [05]   | `PyE57Exception`     | fault           | `PyE57Exception(BaseException)`; libe57 faults raise `libe57.E57Exception`     |

[PUBLIC_TYPE_SCOPE]: supported-field maps (`pye57.e57`)
- rail: scan-processing
- type-family: `dict[str, str]` mapping each supported field name to its `numpy` dtype char — NOT a frozenset; `make_buffer` reads the dtype char to allocate the array, and `COORDINATE_SYSTEMS.*.value` reuses the Cartesian/spherical sub-maps for mode detection

| [INDEX] | [SYMBOL]                           | [ENTRIES]                                                         |
| :-----: | :--------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `SUPPORTED_CARTESIAN_POINT_FIELDS` | `cartesianX`/`cartesianY`/`cartesianZ` -> `"d"`                   |
|  [02]   | `SUPPORTED_SPHERICAL_POINT_FIELDS` | `sphericalRange`/`sphericalAzimuth`/`sphericalElevation` -> `"d"` |
|  [03]   | `SUPPORTED_POINT_FIELDS`           | rows [01] + [02] plus the extra fields fenced below               |

```python signature
# SUPPORTED_POINT_FIELDS extends [01] + [02] with (field name -> numpy dtype char):
"intensity": "f",
"colorRed": "B", "colorGreen": "B", "colorBlue": "B",
"rowIndex": "H", "columnIndex": "H",
"cartesianInvalidState": "b", "sphericalInvalidState": "b",
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: E57 file lifecycle, read, and buffer allocation
- rail: scan-processing

`read_scan(index, *, intensity=False, colors=False, row_column=False, transform=True, ignore_missing_fields=False)` returns the conditioned field-keyed dict (invalid-state masked, pose applied when `transform=True`); `read_scan_raw(index, ignore_unsupported_fields=False)` returns every supported field as-stored. `make_buffer(field_name, capacity, do_conversion=True, do_scaling=True) -> (ndarray, SourceDestBuffer)` and `make_buffers(field_names, ...) -> (dict[str, ndarray], VectorSourceDestBuffer)` return a `(array-or-dict, libe57-buffer)` pair, not a bare dict — `read_scan` calls `make_buffers` internally, so direct allocation is only needed for the raw write path.

| [INDEX] | [SURFACE]                                                 | [ENTRY_FAMILY] | [RAIL]                                                     |
| :-----: | :-------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `E57(path, mode='r') -> E57`                              | construction   | open `.e57`; `mode='w'` auto-writes default header         |
|  [02]   | `e.scan_count -> int`                                     | query          | `len(data3D)` scans in the file                            |
|  [03]   | `e.get_header(index) -> ScanHeader`                       | query          | wrap the `data3D[index]` node in a `ScanHeader`            |
|  [04]   | `e.read_scan(index, *, ...) -> dict`                      | read           | conditioned field-keyed `numpy` dict (see lead)            |
|  [05]   | `e.read_scan_raw(index, ...) -> dict`                     | read           | every supported field as-stored, no mask, no transform     |
|  [06]   | `e.make_buffer(field_name, ...)`                          | allocation     | array sized by `SUPPORTED_POINT_FIELDS` dtype + buffer     |
|  [07]   | `e.make_buffers(field_names, ...)`                        | allocation     | per-field array dict + buffer vector for reader/writer     |
|  [08]   | `e.scan_position(index) -> ndarray`                       | query          | sensor XYZ via `to_global([0,0,0], rotation, translation)` |
|  [09]   | `E57.to_global(points, rotation, translation) -> ndarray` | transform      | static; rotate + translate `(N,3)` to global frame         |
|  [10]   | `e.close()` / `e.__del__` / `e.__exit__`                  | lifecycle      | close the libe57 `ImageFile`                               |
|  [11]   | `e.root -> StructureNode` / `e.data3d -> VectorNode`      | property       | libe57 root node / `data3D` scan vector (boundary escape)  |

[ENTRYPOINT_SCOPE]: E57 write path
- rail: scan-processing

`write_scan_raw(data, *, name=None, rotation=None, translation=None, scan_header=None)` is the single write entry: it builds the scan structure node (pose, bounds, intensity/color limits, acquisition times) and the points prototype from the present fields, then streams the data dict in 5M-point chunks. `convert_spherical_to_cartesian` lives in `pye57.utils`.

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                                                               |
| :-----: | :----------------------------------------------- | :------------- | :------------------------------------------------------------------- |
|  [01]   | `e.write_default_header()`                       | write          | root nodes format/guid/version/data3D/images2D; auto at `mode='w'`   |
|  [02]   | `e.write_scan_raw(data, *, ...)`                 | write          | append one scan; pose from `rotation`/`translation` or `scan_header` |
|  [03]   | `convert_spherical_to_cartesian(rae) -> ndarray` | transform      | `(N,3)` RAE -> XYZ; spherical projection before pose transform       |

[ENTRYPOINT_SCOPE]: ScanHeader typed metadata
- rail: scan-processing

`ScanHeader` wraps the libe57 scan node and `__getitem__`-delegates to it; every property below reads a child node's `.value()` and raises through `libe57.E57Exception` when absent (pose properties degrade to identity/zero). Pose accessors return `ndarray`; bounds accessors return scalars.

| [INDEX] | [SURFACE]                                                        | [ENTRY_FAMILY] | [RAIL]                                             |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------------------------------------- |
|  [01]   | `ScanHeader(scan_node)`                                          | construction   | wrap one scan node                                 |
|  [02]   | `ScanHeader.from_data3d(data3d) -> list[ScanHeader]`             | construction   | map a whole `data3D` vector                        |
|  [03]   | `h.point_fields -> list[str]` / `h.scan_fields -> list[str]`     | query          | prototype field names / scan-node child names      |
|  [04]   | `h.get_coordinate_system(COORDINATE_SYSTEMS)`                    | query          | detects `CARTESIAN`/`SPHERICAL`; raises if neither |
|  [05]   | `h.has_pose() -> bool`                                           | query          | `node.isDefined("pose")`                           |
|  [06]   | `h.point_count -> int`                                           | property       | `points.childCount()`                              |
|  [07]   | `h.rotation` / `h.translation`                                   | property       | quaternion `(w,x,y,z)` and `(3,)` translation      |
|  [08]   | `h.rotation_matrix`                                              | property       | `(3,3)` matrix (identity when no pose)             |
|  [09]   | `h.pose` / `h.points` / `h.indexBounds`                          | property       | raw libe57 sub-nodes (boundary escape)             |
|  [10]   | `h.intensityLimits` / `h.cartesianBounds` / `h.sphericalBounds`  | property       | raw libe57 sub-nodes (boundary escape)             |
|  [11]   | `h.xMinimum`..`h.zMaximum`                                       | property       | Cartesian bounding-box scalars                     |
|  [12]   | `h.rangeMinimum` / `h.rangeMaximum`                              | property       | spherical range bounds                             |
|  [13]   | `h.elevationMinimum` / `h.elevationMaximum`                      | property       | spherical elevation bounds                         |
|  [14]   | `h.azimuthStart` / `h.azimuthEnd`                                | property       | spherical azimuth bounds                           |
|  [15]   | `h.rowMinimum` / `h.rowMaximum`                                  | property       | row index bounds                                   |
|  [16]   | `h.columnMinimum` / `h.columnMaximum`                            | property       | column index bounds                                |
|  [17]   | `h.returnMinimum` / `h.returnMaximum`                            | property       | return index bounds                                |
|  [18]   | `h.intensityMinimum` / `h.intensityMaximum`                      | property       | intensity-limit scalars                            |
|  [19]   | `h.acquisitionStart_dateTimeValue` (and `..End..`)               | property       | acquisition GPS time                               |
|  [20]   | `h.acquisitionStart_isAtomicClockReferenced` (and `..End..`)     | property       | atomic-clock flag                                  |
|  [21]   | `h.guid`                                                         | property       | scan identity                                      |
|  [22]   | `h.temperature` / `h.relativeHumidity` / `h.atmosphericPressure` | property       | environmental metadata                             |
|  [23]   | `h.pretty_print(node=None, indent='') -> list[str]`              | debug          | recursive node dump as a line list (NOT a printer) |

## [04]-[IMPLEMENTATION_LAW]

[E57_TOPOLOGY]:
- import: `from pye57 import E57, ScanHeader` at boundary scope only; module-level import is banned by the manifest import policy. The `pye57.libe57` node API is a boundary escape hatch, never threaded into domain code.
- read axis: `read_scan(index, *, ...)` is the polymorphic intake — coordinate system is auto-detected via `ScanHeader.get_coordinate_system`, requested fields (`intensity`/`colors`/`row_column`) are appended, the invalid-state column masks every field, spherical input is `convert_spherical_to_cartesian`-projected, and `transform=True` applies the scan pose so the dict always exits as global-frame `cartesianX`/`cartesianY`/`cartesianZ` plus the requested extras. `read_scan_raw` is the unconditioned escape for custom field handling — never a parallel reader family.
- buffer axis: `make_buffer`/`make_buffers` size `numpy` arrays from the `SUPPORTED_POINT_FIELDS` dtype-char map and return the `(array, libe57-buffer)` pair the reader/writer consumes; the array dict, not the buffer vector, is the data surface.
- write axis: `E57(path, 'w')` auto-writes the default header; `write_scan_raw(data, ...)` is the single append entry — pose, index/intensity/color limits, cartesian bounds, acquisition times, and the points prototype are all derived from the present fields and the optional `scan_header`, then streamed in 5M-point chunks. There is no per-field write function family.
- pose axis: `E57.to_global` is a static quaternion transform (`pyquaternion.Quaternion(rotation).rotation_matrix @ pts + t`); `ScanHeader.rotation` is the 4-element quaternion `(w,x,y,z)`, `rotation_matrix` the derived `(3,3)`. Apply `to_global` to a `read_scan_raw` array to lift sensor-frame points to global frame.
- evidence: each read captures scan index, point count, coordinate system, present field names, and whether a pose was applied; each write captures scan name, written point count, and bounds as a scan-io receipt.
- boundary: pye57 owns E57 (`.e57`) intake/egress only. The conditioned XYZ block hands to `open3d`/`small-gicp` registration, LAS/LAZ to `laspy`, generic point-cloud pipelines to `pdal`, mesh exchange to `meshio`/`trimesh`. Raw libe57 node trees never cross into domain code.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pye57`
- Owns: E57 multi-scan read/write, typed scan-metadata access, `numpy` field-buffer allocation, quaternion pose-to-global transform
- Accept: `.e57` files in read or write mode, field-keyed `numpy` dicts for raw scan write, quaternion-plus-translation pose tuples
- Reject: wrapper-renames of `read_scan`/`write_scan_raw`; hand-rolled E57 binary or libe57 node parsing in domain code; treating `SUPPORTED_*_POINT_FIELDS` as a frozenset instead of the dtype-char map; treating `make_buffer`/`make_buffers` returns as bare dicts; identity minting the runtime owns

[CAPTURE_GAP]:
