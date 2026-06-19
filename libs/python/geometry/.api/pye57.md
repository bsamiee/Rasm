# [PY_GEOMETRY_API_PYE57]

`pye57` supplies E57 point-cloud file IO through `E57` (file handle), `ScanHeader` (per-scan metadata), and `COORDINATE_SYSTEMS` for reading and writing multi-scan `.e57` files, extracting Cartesian or spherical point arrays, accessing pose/bounds metadata, and writing raw scan data for the geometry scan-processing rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pye57`
- package: `pye57`
- module: `pye57`
- asset: compiled extension (libe57 wrapper), cp313 wheel available
- rail: scan-processing

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: file and scan family
- rail: scan-processing

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `E57`                | file handle   | multi-scan read/write, buffer allocation, global-frame transform |
|  [02]   | `ScanHeader`         | scan metadata | pose, bounds, acquisition times, point count, coordinate system  |
|  [03]   | `COORDINATE_SYSTEMS` | enum          | `CARTESIAN`, `SPHERICAL`                                         |

[PUBLIC_TYPE_SCOPE]: constants
- rail: scan-processing
- type-family: field set (frozenset of supported field name strings)

| [INDEX] | [SYMBOL]                           | [CAPABILITY]                      |
| :-----: | :--------------------------------- | :-------------------------------- |
|  [01]   | `SUPPORTED_POINT_FIELDS`           | all supported field name strings  |
|  [02]   | `SUPPORTED_CARTESIAN_POINT_FIELDS` | Cartesian-mode field name strings |
|  [03]   | `SUPPORTED_SPHERICAL_POINT_FIELDS` | spherical-mode field name strings |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: E57 file operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                                                                                                     | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :---------------------------------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `E57(path, mode='r') -> E57`                                                                                                  | construction   | open E57 file for read or write     |
|  [02]   | `e.scan_count -> int`                                                                                                         | query          | number of scans in file             |
|  [03]   | `e.get_header(index) -> ScanHeader`                                                                                           | query          | metadata for scan at index          |
|  [04]   | `e.make_buffer(field_name, capacity, do_conversion=True, do_scaling=True) -> dict`                                            | allocation     | allocate single-field numpy buffer  |
|  [05]   | `e.make_buffers(field_names, capacity, do_conversion=True, do_scaling=True) -> dict`                                          | allocation     | allocate multi-field numpy buffers  |
|  [06]   | `e.read_scan(index, *, intensity=False, colors=False, row_column=False, transform=True, ignore_missing_fields=False) -> Dict` | read           | read scan as dict of arrays         |
|  [07]   | `e.read_scan_raw(index, ignore_unsupported_fields=False) -> Dict`                                                             | read           | read raw fields without transform   |
|  [08]   | `e.scan_position(index) -> tuple`                                                                                             | query          | XYZ sensor position for scan        |
|  [09]   | `e.to_global(points, rotation, translation) -> ndarray`                                                                       | transform      | apply pose to Cartesian point array |
|  [10]   | `e.write_default_header()`                                                                                                    | write          | write E57 file header               |
|  [11]   | `e.write_scan_raw(data, *, name=None, rotation=None, translation=None, scan_header=None)`                                     | write          | write raw scan data dict            |
|  [12]   | `e.close()`                                                                                                                   | lifecycle      | close file handle                   |
|  [13]   | `e.data3d`                                                                                                                    | property       | underlying libe57 data3D node list  |
|  [14]   | `e.root`                                                                                                                      | property       | underlying libe57 root node         |

[ENTRYPOINT_SCOPE]: ScanHeader fields and operations
- rail: scan-processing

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `ScanHeader.from_data3d(data3d)`              | construction   | build header from libe57 data3d node     |
|  [02]   | `h.has_pose() -> bool`                        | query          | whether pose is available                |
|  [03]   | `h.get_coordinate_system(COORDINATE_SYSTEMS)` | query          | detect coordinate system type            |
|  [04]   | `h.pretty_print(node=None, indent='')`        | debug          | human-readable dump of scan metadata     |
|  [05]   | `h.point_count`                               | property       | total point count in scan                |
|  [06]   | `h.pose`                                      | property       | pose dict with `rotation`/`translation`  |
|  [07]   | `h.rotation`                                  | property       | rotation component of pose               |
|  [08]   | `h.translation`                               | property       | translation component of pose            |
|  [09]   | `h.rotation_matrix`                           | property       | 3x3 rotation matrix                      |
|  [10]   | `h.cartesianBounds`                           | property       | XYZ min/max bounding box                 |
|  [11]   | `h.sphericalBounds`                           | property       | spherical range/elevation/azimuth bounds |
|  [12]   | `h.acquisitionStart` / `h.acquisitionEnd`     | property       | acquisition date-time records            |

[ENTRYPOINT_SCOPE]: module-level utility
- rail: scan-processing

| [INDEX] | [SURFACE]                                        | [ENTRY_FAMILY] | [RAIL]                               |
| :-----: | :----------------------------------------------- | :------------- | :----------------------------------- |
|  [01]   | `convert_spherical_to_cartesian(rae) -> ndarray` | transform      | range-azimuth-elevation to XYZ array |

## [04]-[IMPLEMENTATION_LAW]

[E57_TOPOLOGY]:
- `E57(path, mode='r')` opens for read; `E57(path, mode='w')` opens for write; write mode requires `write_default_header()` before any scan write.
- `read_scan` returns a `dict` with numpy array values keyed by field name (`cartesianX`, `cartesianY`, `cartesianZ`, `intensity`, `colorRed`, `colorGreen`, `colorBlue`, `rowIndex`, `columnIndex`).
- `transform=True` in `read_scan` applies the scan pose to produce global-frame Cartesian coordinates; `transform=False` returns sensor-frame coordinates.
- `make_buffer`/`make_buffers` pre-allocate numpy arrays for the given capacity; they are passed as the `data` dict to `write_scan_raw`.
- `ScanHeader.rotation_matrix` is a `(3, 3)` numpy array; `ScanHeader.translation` is a length-3 array.
- `SUPPORTED_CARTESIAN_POINT_FIELDS` covers: `cartesianX`, `cartesianY`, `cartesianZ`, `intensity`, `colorRed`, `colorGreen`, `colorBlue`, `rowIndex`, `columnIndex`, `cartesianInvalidState`.

[LOCAL_ADMISSION]:
- Scan data enters via `E57(path)` and `read_scan(index)` which yields a field-keyed numpy dict; pose transform is applied inline when `transform=True`.
- Write pipeline: `E57(path, 'w')` -> `write_default_header()` -> allocate buffers -> fill numpy arrays -> `write_scan_raw(data, rotation=..., translation=...)`.
- `to_global(points, rotation, translation)` applies pose to a raw points array returned from `read_scan_raw`.

[RAIL_LAW]:
- Package: `pye57`
- Owns: E57 point-cloud file read and write, scan metadata access, pose-transform application
- Accept: `.e57` files in read or write mode, numpy arrays for raw scan data
- Reject: hand-rolled E57 binary parsing, direct libe57 node manipulation in production code
