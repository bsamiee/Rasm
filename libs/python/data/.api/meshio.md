# [PY_DATA_API_MESHIO]

`meshio` reads and writes unstructured mesh files across 30-plus formats behind one in-memory `Mesh` value carrying `points`, typed `CellBlock` connectivity, and named point/cell/field data. `read` and `write` dispatch on file extension or an explicit `file_format`, while `write_points_cells` writes directly from raw arrays; per-format submodules (`vtk`, `vtu`, `xdmf`, `gmsh`, `stl`, `ply`, `obj`, `med`, and others) each expose `read`/`write` plus their own type maps.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- module: `meshio`
- asset: pure Python
- rail: mesh file exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh value and failures
- rail: mesh file exchange

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [ROLE]                                                |
| :-----: | :----------- | :------------- | :---------------------------------------------------- |
|  [01]   | `Mesh`       | mesh value     | points, cell blocks, and named point/cell/field data  |
|  [02]   | `CellBlock`  | connectivity   | one `cell_type` with `data` array and optional `tags` |
|  [03]   | `ReadError`  | parse failure  | malformed or unsupported input file                   |
|  [04]   | `WriteError` | export failure | unwritable mesh for the target format                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level read and write
- rail: mesh file exchange

| [INDEX] | [SURFACE]                                                  | [ENTRY_FAMILY]  | [RAIL]                                       |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------------------------- |
|  [01]   | `read(filename, file_format=None)`                         | intake          | parse a file to `Mesh`, format auto-detected |
|  [02]   | `write(filename, mesh, file_format=None, **kwargs)`        | export          | serialize a `Mesh` to a file                 |
|  [03]   | `write_points_cells(filename, points, cells, ..., **kw)`   | export          | write directly from raw points and cells     |
|  [04]   | `register_format(format_name, extensions, reader, writer)` | format registry | add a custom format reader and writer map    |
|  [05]   | `deregister_format(format_name)`                           | format registry | remove a registered format                   |
|  [06]   | `extension_to_filetypes`                                   | format registry | `dict` of extension to candidate formats     |

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
|  [12]   | `Mesh.copy()`                          | clone          | deep copy of the mesh                      |

[ENTRYPOINT_SCOPE]: per-format submodules
- rail: mesh file exchange

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                |
| :-----: | :------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `vtk.read` / `vtk.write`               | format codec   | legacy VTK serial mesh                |
|  [02]   | `vtu.read` / `vtu.write`               | format codec   | XML VTK unstructured grid             |
|  [03]   | `xdmf.read` / `xdmf.write`             | format codec   | XDMF with HDF5 heavy data             |
|  [04]   | `xdmf.TimeSeriesReader` / `...Writer`  | format codec   | time-series XDMF streaming            |
|  [05]   | `gmsh.read` / `gmsh.write`             | format codec   | Gmsh `.msh` mesh                      |
|  [06]   | `gmsh.gmsh_to_meshio_type`             | type map       | Gmsh element-type to meshio cell type |
|  [07]   | `stl.read` / `stl.write`               | format codec   | STL triangle surface                  |
|  [08]   | `ply.read` / `ply.write`               | format codec   | PLY polygon mesh                      |
|  [09]   | `obj.read` / `obj.write`               | format codec   | Wavefront OBJ                         |
|  [10]   | `med.read` / `med.write`               | format codec   | Salome MED mesh                       |
|  [11]   | `abaqus` / `nastran` / `ansys` / `su2` | format codec   | FEA solver input decks                |
|  [12]   | `off` / `medit` / `tetgen` / `netgen`  | format codec   | surface and tetrahedral mesh formats  |

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

[RAIL_LAW]:
- Package: `meshio`
- Owns: unstructured mesh file read/write across solver, CAD, and visualization formats
- Accept: file paths or buffers with detectable or declared `file_format`, and in-memory `Mesh` values
- Reject: hand-rolled mesh parsers, per-format connectivity reimplementation, lossy array-only exchange that drops point/cell data
