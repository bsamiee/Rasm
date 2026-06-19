# [PY_COMPUTE_API_MESHIO]

`meshio` supplies a single in-memory `Mesh` container plus polymorphic `read`/`write` dispatch across roughly 40 unstructured-mesh formats for the compute mesh-exchange rail; points, typed `CellBlock` connectivity, and named point/cell data round-trip through one shape regardless of on-disk format.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- module: `meshio`
- asset: pure-Python runtime library (requires `numpy`, `rich`)
- rail: mesh-exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh container family
- rail: mesh-exchange

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [ROLE]                                                     |
| :-----: | :----------- | :------------ | :--------------------------------------------------------- |
|  [01]   | `Mesh`       | class         | points plus cell blocks, point/cell data, sets, field data |
|  [02]   | `CellBlock`  | class         | one homogeneous cell type with connectivity array and tags |
|  [03]   | `ReadError`  | exception     | malformed input or unknown format on read                  |
|  [04]   | `WriteError` | exception     | unsupported cell type or format mismatch on write          |

[PUBLIC_TYPE_SCOPE]: `Mesh` members
- rail: mesh-exchange — constructor `Mesh(points, cells, point_data, cell_data, field_data, point_sets, cell_sets)`

| [INDEX] | [SYMBOL]             | [KIND]    | [ROLE]                                       |
| :-----: | :------------------- | :-------- | :------------------------------------------- |
|  [01]   | `points`             | attribute | `(n, dim)` float array of vertex coordinates |
|  [02]   | `cells`              | attribute | list of `CellBlock` connectivity blocks      |
|  [03]   | `point_data`         | attribute | name to per-point array mapping              |
|  [04]   | `cell_data`          | attribute | name to per-block array list mapping         |
|  [05]   | `field_data`         | attribute | name to metadata array mapping               |
|  [06]   | `point_sets`         | attribute | name to point-index array mapping            |
|  [07]   | `cell_sets`          | attribute | name to per-block index array mapping        |
|  [08]   | `cells_dict`         | property  | cell type to merged connectivity array       |
|  [09]   | `cell_data_dict`     | property  | name to cell type to array nested mapping    |
|  [10]   | `cell_sets_dict`     | property  | set name to cell type to index mapping       |
|  [11]   | `get_cells_type`     | method    | connectivity array for one cell type         |
|  [12]   | `get_cell_data`      | method    | cell-data array for one name and cell type   |
|  [13]   | `cell_sets_to_data`  | method    | promote a cell set into cell data            |
|  [14]   | `cell_data_to_sets`  | method    | promote cell data into a cell set            |
|  [15]   | `point_sets_to_data` | method    | promote a point set into point data          |
|  [16]   | `copy`               | method    | deep copy of the mesh                        |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: format dispatch
- rail: mesh-exchange — `file_format=None` triggers extension-based detection

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [ROLE]                                   |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `read(filename, file_format=None)`                                                  | reader         | parse any format into a `Mesh`           |
|  [02]   | `write(filename, mesh, file_format=None, **kwargs)`                                 | writer         | serialize a `Mesh` to any format         |
|  [03]   | `write_points_cells(filename, points, cells, point_data=None, cell_data=None, ...)` | writer         | write directly from points and cells     |
|  [04]   | `Mesh.read(path_or_buf, file_format=None)`                                          | classmethod    | construct a `Mesh` from a path or buffer |
|  [05]   | `Mesh.write(path_or_buf, file_format=None, **kwargs)`                               | method         | serialize this mesh to a path or buffer  |

[ENTRYPOINT_SCOPE]: format registry
- rail: mesh-exchange

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [ROLE]                                  |
| :-----: | :------------------------------------------------------------- | :------------- | :-------------------------------------- |
|  [01]   | `register_format(format_name, extensions, reader, writer_map)` | registry       | add a custom format reader and writer   |
|  [02]   | `deregister_format(format_name)`                               | registry       | remove a registered format              |
|  [03]   | `extension_to_filetypes`                                       | mapping        | extension to candidate format-name list |

## [04]-[IMPLEMENTATION_LAW]

[MESHIO_TOPOLOGY]:
- `cells` is a list of `CellBlock(cell_type, data, tags=None)`; `cell_type` is a string such as `triangle`, `quad`, `tetra`, `hexahedron`, `line`, `vertex`
- `cell_data` values parallel `cells`: a list with one array per `CellBlock`, indexed by block position
- `cells_dict` merges all blocks of one type into a single array; use it for read paths, not mutation
- format modules are exposed as submodules (`meshio.stl`, `meshio.vtu`, `meshio.gmsh`, `meshio.xdmf`, etc.) for direct per-format access
- `extension_to_filetypes` maps `.msh -> [ansys, gmsh]`, `.vtu -> [vtu]`, `.stl -> [stl]`, `.xdmf -> [xdmf]`; ambiguous extensions need an explicit `file_format`

[LOCAL_ADMISSION]:
- Construct a `Mesh` from a points array and a `cells` list of `(type, connectivity)` tuples or `CellBlock` instances; never assemble parallel dicts by hand.
- Route all on-disk conversion through `read`/`write`; let extension detection pick the format and pass `file_format` only to disambiguate.
- Read per-type connectivity through `cells_dict` or `get_cells_type`; keep the original `cells` block list for write fidelity and per-block `cell_data`.
- Format-specific keyword options (binary mode, compression) pass through `**kwargs` to `write`.

[RAIL_LAW]:
- Package: `meshio`
- Owns: unstructured mesh file exchange across the supported format set and the canonical in-memory `Mesh` shape
- Accept: `numpy` point arrays and `CellBlock` connectivity; format dispatch by extension or explicit `file_format`
- Reject: hand-rolled format parsers; parallel per-format mesh containers; wrapper-renames of `read`/`write`
