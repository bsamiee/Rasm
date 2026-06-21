# [PY_GEOMETRY_API_MESHIO]

`meshio` supplies the unstructured-mesh exchange surface for the geometry mesh-io rail: a `Mesh` value of points plus typed `CellBlock` connectivity, the `read`/`write` polymorphic readers (path or in-memory buffer), the `topological_dimension` cell-type table, and the per-format submodules that drive read/write across `vtk`/`vtu`/`xdmf`/`gmsh`/`obj`/`ply`/`stl`/`off`/`med`/`abaqus`/`nastran`/`cgns`/`exodus`/`tetgen` (30-plus) with point/cell/field data and named sets. The package owner composes `meshio.read`, `Mesh`, and `meshio.write` into the mesh-io owner, and is the `meshio.Mesh` sink `pdal.Pipeline.get_meshio` already targets; it never re-implements the format parsing meshio owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- import: `import meshio`
- owner: `geometry`
- rail: mesh-io
- installed: `5.3.5`; license MIT; pure-Python `py3-none-any` wheel, `Requires-Python>=3.8`, deps `numpy>=1.20.0` + `rich` (CLI table output) — no native compile, so it rides the project-venv `numpy` floor on every interpreter (manifest declares bare `meshio`, no marker); `netCDF4`/`h5py` are `[all]`-extra optionals gating the `cgns`/`exodus`/`h5m`/`med`/`xdmf-HDF` paths
- entry points: console script `meshio` (CLI: `convert`/`info`/`compress`/`decompress`/`ascii`/`binary`) — library use does not invoke it
- capability: read/write of 30-plus mesh formats from path or buffer, points plus typed cell blocks, point/cell/field data arrays, named point and cell sets, set<->data projection, the `topological_dimension` cell-type-to-dim table, format auto-detection by extension, and a runtime format registry (`register_format`/`deregister_format`/`extension_to_filetypes`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh value and faults
- rail: mesh-io

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE] | [CAPABILITY]                                                |
| :-----: | :----------------------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Mesh`                   | mesh value     | points plus cell blocks with point/cell/field data, named sets, gmsh-periodic + info carriers |
|  [02]   | `CellBlock`              | cell block     | one cell type string (`triangle`/`tetra`/`quad`/`polygon`/`polyhedronN`) plus `data`/`tags`/`dim`; `len()` is cell count |
|  [03]   | `topological_dimension`  | `dict[str,int]`| cell-type-name -> spatial dimension (0..3) for 80-plus types incl. high-order and VTK-Lagrange cells |
|  [04]   | `ReadError`              | read fault     | unreadable or malformed source / unknown format / missing buffer format |
|  [05]   | `WriteError`             | write fault    | unwritable target, unknown format, or cell-array shape mismatch against `num_nodes_per_cell` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, write, and build
- rail: mesh-io

Read and write rows accept a filename OR an in-memory buffer (file-like), optional `file_format` (auto-detected from extension when absent; REQUIRED when a buffer is used), and format-specific keyword options forwarded to the per-format writer. `Mesh.write` is the bound mirror; `Mesh.read` is deprecated in favor of module `meshio.read`.

| [INDEX] | [SURFACE]                                                                                                                            | [CALL_SHAPE]                   | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------- | :----------------------------- | :------------------------------------------------- |
|  [01]   | `meshio.read(filename, file_format=None) -> Mesh`                                                                                     | path/buffer plus format policy | polymorphic intake; tries every format `extension_to_filetypes` maps the suffix to |
|  [02]   | `meshio.write(filename, mesh, file_format=None, **kwargs) -> None`                                                                    | path/buffer plus mesh plus opts| dispatch on `file_format` or extension; validates cell-array shapes pre-write |
|  [03]   | `meshio.write_points_cells(filename, points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, file_format=None, **kwargs)` | path plus arrays | array shortcut: builds a `Mesh` then writes        |
|  [04]   | `Mesh(points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, gmsh_periodic=None, info=None)` | points plus cells plus data | construct a mesh value; `cells` is a list of `(type, conn)` tuples or `CellBlock`s (legacy dict accepted) |
|  [05]   | `Mesh.write(path_or_buf, file_format=None, **kwargs)`                                                                                 | path/buffer plus format policy | bound write delegating to `meshio.write`           |
|  [06]   | `Mesh.copy() -> Mesh`                                                                                                                 | none                           | deep copy of the mesh value                        |
|  [07]   | `extension_to_filetypes -> dict[str, list[str]]`                                                                                      | module dict                    | suffix-to-format-list detection map (the auto-detect source) |
|  [08]   | `register_format(format_name, extensions: list[str], reader, writer_map) -> None`                                                     | name + exts + reader + writers | add a custom format: extends `extension_to_filetypes`/`reader_map`/`_writer_map` |
|  [09]   | `deregister_format(format_name) -> None`                                                                                              | format name                    | remove a registered format from all three registries |

[ENTRYPOINT_SCOPE]: mesh projections
- rail: mesh-io

`Mesh` projection methods reshape connectivity and data between block-list and dict views; the set<->data pair is bidirectional for both points and cells.

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]   | [CAPABILITY]                                  |
| :-----: | :----------------------------------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `Mesh.cells_dict -> dict[str, ndarray]`                | property       | cell-type to concatenated-connectivity map    |
|  [02]   | `Mesh.cell_data_dict -> dict[str, dict[str, ndarray]]` | property       | cell-data name to per-cell-type arrays         |
|  [03]   | `Mesh.cell_sets_dict -> dict[str, dict[str, ndarray]]` | property       | named cell-set membership, offset per block    |
|  [04]   | `Mesh.get_cells_type(cell_type) -> ndarray`            | cell type      | concatenated connectivity for one cell type    |
|  [05]   | `Mesh.get_cell_data(name, cell_type) -> ndarray`       | name plus type | cell-data array for one cell type              |
|  [06]   | `Mesh.cell_sets_to_data(data_name=None)`               | optional name  | cell sets -> int cell data (`-1` default)      |
|  [07]   | `Mesh.cell_data_to_sets(key)`                          | data key       | int cell data -> cell sets, then drops the data |
|  [08]   | `Mesh.point_sets_to_data(join_char='-')`               | join separator | point sets -> int point data (`-1` default)    |
|  [09]   | `Mesh.point_data_to_sets(key)`                         | data key       | int point data -> point sets, then drops the data |

[ENTRYPOINT_SCOPE]: per-format submodules
- rail: mesh-io

Each format is a submodule (`meshio.ply`, `meshio.vtk`, `meshio.xdmf`, `meshio.gmsh`, `meshio.obj`, `meshio.stl`, `meshio.med`, `meshio.abaqus`, `meshio.nastran`, `meshio.cgns`, `meshio.exodus`, `meshio.tetgen`, ...) exposing `read`/`write` plus the format-specific options the polymorphic `meshio.read`/`write` forward to. Reach these only to pass a format-specific keyword (e.g. binary-vs-ASCII, data-format), never as a parallel reader family — `meshio.read`/`write` already dispatch through `reader_map`/`_writer_map` to them.

| [INDEX] | [SUBMODULE FAMILY]                                                      | [CALL_SHAPE]      | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :---------------- | :-------------------------------------------- |
|  [01]   | `meshio.<format>.read(filename) -> Mesh`                               | path/buffer       | the parser `reader_map[<format>]` registers   |
|  [02]   | `meshio.<format>.write(filename, mesh, **opts)`                        | path + mesh + opts| the writer `_writer_map[<format>]` registers; opts are forwarded from `meshio.write(**kwargs)` |

## [04]-[IMPLEMENTATION_LAW]

[MESH_IO]:
- import: `import meshio` at boundary scope only; module-level import is banned by the manifest import policy.
- read axis: `meshio.read` is the polymorphic intake over a path or an in-memory file-like buffer; `file_format` selects a parser only when the extension is ambiguous (REQUIRED for buffers), resolved through `extension_to_filetypes` and `reader_map`, never a parallel reader function per format. The reader tries every format the suffix maps to before failing.
- mesh axis: one `Mesh` owns `points`, a list of `CellBlock` connectivity, `point_data`, `cell_data`, `field_data`, named `point_sets`/`cell_sets`, plus `gmsh_periodic`/`info` carriers; cell types stay typed strings (`triangle`/`tetra`/`quad`/`polygon`/`polyhedronN`), never positional arrays. `CellBlock.dim` is read from `topological_dimension`; `cells_dict`/`cell_data_dict` are the dict projections, and `cell_sets`<->`cell_data` / `point_sets`<->`point_data` round-trip through the four projection methods.
- write axis: `meshio.write` dispatches on `file_format` or extension and validates each `CellBlock` against `num_nodes_per_cell` before delegating to `_writer_map[<format>]` with the forwarded `**kwargs`; `write_points_cells` is the array shortcut that skips explicit `Mesh` construction.
- registry axis: `register_format(name, extensions, reader, writer_map)` extends `extension_to_filetypes`/`reader_map`/`_writer_map` at runtime; a custom format is three registry rows, never a subclass hierarchy. `deregister_format` removes them.
- stacking: this owner is the `meshio.Mesh` sink the PDAL pipeline produces — `pdal.Pipeline.get_meshio(idx)` returns exactly a `Mesh(points, [("triangle", faces)])`, so a `readers.<fmt> | filters.poisson` graph egresses straight into this rail. The reverse FEM/CAE export folds a `trimesh.Trimesh` (vertices/faces) into `Mesh(points, [("triangle", faces)])` then `meshio.write(buf, mesh, file_format=...)` to a `vtu`/`xdmf`/`med` buffer; the buffer-write path keeps the whole exchange in memory across the data-codec wire with no temp file.
- evidence: each read/write op captures point count, cell-block count per type, point-data/cell-data/field-data names, and output byte length as a mesh-io receipt.
- boundary: meshio owns FEM/CAE unstructured-mesh exchange (volumetric tetra/hex, named regions, solver fields). Triangular surface processing and GLB routes to `trimesh`, point-cloud pipelines to `pdal`/`open3d`, Rhino `.3dm` to `rhino3dm`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `meshio`
- Owns: read/write of 30-plus unstructured-mesh formats (path or buffer), typed cell blocks, point/cell/field data, named sets, set<->data projection, and a runtime format registry
- Accept: FEM/CAE mesh ingestion and export feeding the mesh-io and geometry owners; a `meshio.Mesh` handed from `pdal.Pipeline.get_meshio`; a `trimesh.Trimesh` folded into points + triangle CellBlock
- Reject: wrapper-renames of `read`/`write`; a hand-rolled `vtk`/`gmsh`/`xdmf` parser where meshio is admitted; positional cell arrays that drop the typed `CellBlock`; a per-format reader/writer function family where `meshio.read`/`write` dispatch through `reader_map`/`_writer_map`; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: `meshio==5.3.5` is pure-Python `py3-none-any`, MIT, deps `numpy>=1.20.0` + `rich`; it rides the project-venv `numpy` floor on every interpreter (no marker), so it loads on the cp315 core directly. The `cgns`/`exodus`/`h5m`/`med` and HDF-backed `xdmf` paths additionally require the `[all]`-extra `netCDF4`/`h5py`.
- members: introspected against the installed distribution source (`_mesh.py`, `_helpers.py`, `__init__.py`); every documented type, entrypoint, projection method, registry function, and format submodule resolves — no phantom. `register_format` arity, the `Mesh` `gmsh_periodic`/`info` constructor params, the buffer-read path, `point_data_to_sets`, and the `topological_dimension`/`num_nodes_per_cell` tables are source-confirmed.
