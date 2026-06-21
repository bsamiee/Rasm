# [PY_DATA_API_MESHIO]

`meshio` reads and writes unstructured mesh files across 30-plus formats behind one in-memory `Mesh` value carrying `points`, typed `CellBlock` connectivity, and named point/cell/field data. `read` and `write` dispatch on file extension or an explicit `file_format`, while `write_points_cells` writes directly from raw arrays; per-format submodules (`vtk`, `vtu`, `xdmf`, `gmsh`, `stl`, `ply`, `obj`, `med`, and others) each expose `read`/`write` plus their own type maps.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- module: `meshio`
- version: `5.3.5`
- license: MIT
- asset: pure Python
- rail: mesh file exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh value and failures
- rail: mesh file exchange

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [ROLE]                                                                  |
| :-----: | :----------- | :------------- | :---------------------------------------------------------------------- |
|  [01]   | `Mesh`       | mesh value     | points, cell blocks, and named point/cell/field data                    |
|  [02]   | `CellBlock`  | connectivity   | `type` cell-type string, `data` array, optional `tags`, derived `dim`   |
|  [03]   | `ReadError`  | parse failure  | malformed or unsupported input file                                     |
|  [04]   | `WriteError` | export failure | unwritable mesh for the target format                                   |

`CellBlock(cell_type, data, tags=None)` constructs from a cell-type string and a connectivity array; the instance exposes `.type` (the cell-type string), `.data` (the integer connectivity array), `.tags`, and `.dim` (intrinsic topological dimension). The constructor argument is `cell_type`; the read accessor is `.type`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level read and write
- rail: mesh file exchange

| [INDEX] | [SURFACE]                                                                                                      | [ENTRY_FAMILY]  | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `read(filename, file_format=None) -> Mesh`                                                                     | intake          | parse a file to `Mesh`, format auto-detected |
|  [02]   | `write(filename, mesh, file_format=None, **kwargs) -> None`                                                    | export          | serialize a `Mesh` to a file                 |
|  [03]   | `write_points_cells(filename, points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, file_format=None, **kwargs)` | export | write directly from raw points and cells     |
|  [04]   | `register_format(format_name, extensions, reader, writer_map) -> None`                                         | format registry | add a custom format with reader fn and per-extension `writer_map` |
|  [05]   | `deregister_format(format_name)`                                                                               | format registry | remove a registered format                   |
|  [06]   | `extension_to_filetypes`                                                                                       | format registry | `dict[str, list[str]]` of extension to candidate formats |

[ENTRYPOINT_SCOPE]: `Mesh` methods and accessors
- rail: mesh file exchange

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `Mesh(points, cells, point_data, ...)` | construct      | build a mesh from arrays and dicts         |
|  [02]   | `Mesh.read(path_or_buf, file_format)`  | intake         | classmethod parse to `Mesh`                |
|  [03]   | `Mesh.write(path_or_buf, file_format)` | export         | serialize this mesh                        |
|  [04]   | `Mesh.cells_dict`                      | view           | `{cell_type: data}` merged across blocks   |
|  [05]   | `Mesh.cell_data_dict`                  | view           | `{name: {cell_type: data}}` merged view    |
|  [06]   | `Mesh.cell_sets_dict`                  | view           | `{set_name: {cell_type: indices}}`         |
|  [07]   | `Mesh.get_cells_type(cell_type)`       | accessor       | stacked connectivity for one cell type     |
|  [08]   | `Mesh.get_cell_data(name, cell_type)`  | accessor       | named cell data for one cell type          |
|  [09]   | `Mesh.cell_data_to_sets(key)`          | convert        | derive cell sets from a cell-data column   |
|  [10]   | `Mesh.cell_sets_to_data(data_name)`    | convert        | derive a cell-data column from cell sets   |
|  [11]   | `Mesh.point_sets_to_data(data_name)`   | convert        | derive a point-data column from point sets |
|  [12]   | `Mesh.point_data_to_sets(key)`         | convert        | derive point sets from a point-data column |
|  [13]   | `Mesh.copy()`                          | clone          | deep copy of the mesh                      |

`Mesh` also carries the raw mutable state read at export: `points`, `cells` (`list[CellBlock]`), `point_data`, `cell_data`, `field_data`, `point_sets`, `cell_sets`, plus the format-carried `gmsh_periodic` and `info` slots. Construct with `Mesh(points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, gmsh_periodic=None, info=None)`; `cells` accepts a `{cell_type: data}` dict, a list of `(type, data)` tuples, or a list of `CellBlock`.

[ENTRYPOINT_SCOPE]: per-format submodules
- rail: mesh file exchange

Every per-format submodule exposes `read`/`write`; `read(filename)` and `write(filename, mesh)` mirror the top-level entries scoped to one format. Submodules carry their own bidirectional element-type maps where the format numbers cells.

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [RAIL]                                            |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------------------ |
|  [01]   | `vtk.read` / `vtk.write`                               | format codec   | legacy VTK serial mesh                            |
|  [02]   | `vtu.read` / `vtu.write`                               | format codec   | XML VTK unstructured grid                         |
|  [03]   | `xdmf.read` / `xdmf.write`                             | format codec   | XDMF with HDF5 heavy data                         |
|  [04]   | `xdmf.TimeSeriesReader` / `xdmf.TimeSeriesWriter`      | format codec   | time-series XDMF streaming (context-manager pair) |
|  [05]   | `gmsh.read` / `gmsh.write`                             | format codec   | Gmsh `.msh` mesh                                  |
|  [06]   | `gmsh.gmsh_to_meshio_type` / `gmsh.meshio_to_gmsh_type` | type map     | bidirectional Gmsh element-type <-> meshio cell type |
|  [07]   | `stl.read` / `stl.write`                               | format codec   | STL triangle surface                              |
|  [08]   | `ply.read` / `ply.write`                               | format codec   | PLY polygon mesh                                  |
|  [09]   | `obj.read` / `obj.write`                               | format codec   | Wavefront OBJ                                     |
|  [10]   | `med.read` / `med.write`                               | format codec   | Salome MED mesh                                   |
|  [11]   | `abaqus` / `nastran` / `ansys` / `su2` / `permas`      | format codec   | FEA solver input decks                            |
|  [12]   | `off` / `medit` / `tetgen` / `netgen` / `ugrid`        | format codec   | surface and tetrahedral mesh formats              |
|  [13]   | `cgns` / `exodus` / `h5m` / `hmf` / `flac3d` / `dolfin` | format codec  | HDF5/NetCDF and FE-framework mesh formats         |
|  [14]   | `avsucd` / `mdpa` / `tecplot` / `neuroglancer`         | format codec   | AVS-UCD, Kratos, Tecplot, Neuroglancer formats    |
|  [15]   | `svg.write` / `wkt.read` / `wkt.write`                 | format codec   | SVG export, WKT geometry text                     |

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- `Mesh.points` is an `(n, 3)` (or `(n, 2)`) float array; `Mesh.cells` is a list of `CellBlock`
- each `CellBlock` pairs one `cell_type` string (`triangle`, `quad`, `tetra`, `hexahedron`, `line`, `vertex`, ...) with an integer connectivity array
- `point_data` is `{name: array}` aligned to points; `cell_data` is `{name: [array_per_block]}` aligned to cell blocks
- `field_data`, `point_sets`, and `cell_sets` carry named scalar metadata and index groups
- `cells_dict`, `cell_data_dict`, and `cell_sets_dict` collapse the per-block lists into type-keyed dicts for read access
- format is selected by extension via `extension_to_filetypes`, overridden by `file_format`
- `_mesh.topological_dimension` maps each cell type to its intrinsic dimension

[LOCAL_ADMISSION]:
- Round-trip geometry through `Mesh`: `read` to a `Mesh`, transform connectivity and data dicts, then `write`.
- Construct meshes from raw arrays with `Mesh(points, cells, ...)` or write straight from arrays with `write_points_cells`.
- Use `cells_dict` and `cell_data_dict` for type-keyed access; preserve the per-block `cells`/`cell_data` lists for export.
- Reach for a format submodule (`xdmf.TimeSeriesWriter`, `gmsh.gmsh_to_meshio_type`) only for format-specific capabilities the top-level API does not expose.

[STACK]:
- mesh-codec spine: `meshio` is the multi-format file boundary of the data mesh rail — the in-memory triangulation (vertices `(n,3)` float array + `{cell_type: connectivity}`) crosses the geometry<->data seam as raw arrays, and `meshio.Mesh(points, cells)` / `write_points_cells` is the single owner that turns those arrays into any of the 30+ on-disk formats; `trimesh`/the kernel never touches a mesh file handle.
- arrow handoff: `Mesh.points` and the per-type `cells_dict` arrays are NumPy buffers, so they feed `nanoarrow.c_array_from_buffers` / `narwhals.from_numpy` directly when a mesh must be carried as columnar Arrow for a frame-shaped consumer; the point/cell-data dicts become named columns rather than a re-encode.
- time-series stack: `xdmf.TimeSeriesWriter` is a context manager that writes one topology once then appends per-step `point_data`/`cell_data` against shared HDF5 heavy data — use it for transient-field export rather than re-writing the full `Mesh` per step.
- tag stack: `cell_data_to_sets`/`cell_sets_to_data` and `point_data_to_sets`/`point_sets_to_data` are the bidirectional bridge between integer label columns and named index groups; convert at the boundary so internal code carries one representation, never both.

[RAIL_LAW]:
- Package: `meshio`
- Owns: unstructured mesh file read/write across solver, CAD, and visualization formats; the `Mesh` value and its set/data conversions; per-format element-type maps
- Accept: file paths or buffers with detectable or declared `file_format`, and in-memory `Mesh` values built from raw arrays
- Reject: hand-rolled mesh parsers; per-format connectivity reimplementation; lossy array-only exchange that drops point/cell data; a serialized-blob handoff where the geometry<->data seam should pass in-memory vertex/face arrays
