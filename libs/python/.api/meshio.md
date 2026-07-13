# [PY_BRANCH_API_MESHIO]

`meshio` is the branch unstructured-mesh file-exchange substrate: one `Mesh` value carries points, typed `CellBlock` connectivity, point/cell data, sets, and field metadata across solver, visualization, CAD-adjacent, and FEM formats without per-format containers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- import: `meshio`
- owner: `shared`
- rail: mesh file exchange
- version: `5.3.5`
- license: `MIT`
- asset: pure Python; runtime dependencies `numpy>=1.20.0` and `rich`; HDF5/NetCDF formats require the `all` extra (`h5py`, `netCDF4`)
- capability: read/write dispatch across unstructured mesh formats, the canonical in-memory `Mesh` shape, cell-block topology, point/cell/field data, set/data conversions, format registry, Gmsh type mapping, XDMF time-series IO, and solver-format ingress/egress

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh value and failures
- rail: mesh file exchange

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                                                          |
| :-----: | :----------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Mesh`       | mesh value    | points, cells, point/cell/field data, sets, metadata            |
|  [02]   | `CellBlock`  | connectivity  | homogeneous cell type with connectivity array and optional tags |
|  [03]   | `ReadError`  | fault         | malformed or unsupported input file                             |
|  [04]   | `WriteError` | fault         | unsupported cell type or target format                          |

[PUBLIC_TYPE_SCOPE]: `Mesh` members
- rail: mesh file exchange

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [RAIL]                                             |
| :-----: | :------------------------------------------------- | :------------ | :------------------------------------------------- |
|  [01]   | `points`                                           | array         | `(n, dim)` float vertex coordinates                |
|  [02]   | `cells`                                            | blocks        | ordered `list[CellBlock]`                          |
|  [03]   | `point_data` / `cell_data`                         | data maps     | point-aligned arrays and block-aligned cell arrays |
|  [04]   | `field_data`                                       | metadata      | named scalar metadata                              |
|  [05]   | `point_sets` / `cell_sets`                         | sets          | named index-group mappings                         |
|  [06]   | `gmsh_periodic` / `info`                           | metadata      | format-carried Gmsh and free-form metadata         |
|  [07]   | `cells_dict` / `cell_data_dict` / `cell_sets_dict` | views         | type-keyed merged read views                       |
|  [08]   | `get_cells_type` / `get_cell_data`                 | accessor      | per-cell-type connectivity and data                |
|  [09]   | `cell_data_to_sets`                                | conversion    | cell data field to named sets                      |
|  [10]   | `cell_sets_to_data`                                | conversion    | named sets to cell data field                      |
|  [11]   | `point_data_to_sets`                               | conversion    | point data field to named sets                     |
|  [12]   | `point_sets_to_data`                               | conversion    | named sets to point data field                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, write, and registry
- rail: mesh file exchange

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `read(filename, file_format=None) -> Mesh`                     | intake         | parse a path or buffer into a `Mesh`      |
|  [02]   | `write(filename, mesh, file_format=None, **kwargs)`            | export         | serialize a `Mesh`                        |
|  [03]   | `write_points_cells(filename, points, cells, ...)`             | export         | write from arrays without explicit `Mesh` |
|  [04]   | `register_format(format_name, extensions, reader, writer_map)` | registry       | add a custom format reader/writer map     |
|  [05]   | `deregister_format(format_name)`                               | registry       | remove a registered format                |
|  [06]   | `extension_to_filetypes`                                       | registry       | extension-to-format candidate mapping     |

Full signature for the `...`-abbreviated writer:
- [03]-[WRITE_POINTS_CELLS]: `write_points_cells(filename, points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, file_format=None, **kwargs)`

[ENTRYPOINT_SCOPE]: per-format surfaces
- rail: mesh file exchange

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [RAIL]                        |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `vtk.read` / `vtk.write`                                                       | format codec   | legacy VTK serial mesh        |
|  [02]   | `vtu.read` / `vtu.write`                                                       | format codec   | XML VTK unstructured grid     |
|  [03]   | `xdmf.read` / `xdmf.write` / `xdmf.TimeSeriesReader` / `xdmf.TimeSeriesWriter` | format codec   | XDMF and time-series XDMF     |
|  [04]   | `gmsh.read` / `gmsh.write` / `gmsh_to_meshio_type` / `meshio_to_gmsh_type`     | format codec   | Gmsh mesh and element mapping |
|  [05]   | `stl` / `ply` / `obj` / `off` / `svg` / `wkt`                                  | format codec   | surface and geometry exchange |
|  [06]   | `med` / `abaqus` / `nastran` / `ansys` / `su2` / `permas` / `mdpa`             | format codec   | FEA and solver decks          |
|  [07]   | `tetgen` / `netgen` / `ugrid` / `cgns` / `exodus` / `h5m` / `hmf` / `dolfin`   | format codec   | HDF5/NetCDF and FE frameworks |

## [04]-[IMPLEMENTATION_LAW]

[MESH_TOPOLOGY]:
- `Mesh.points` is an `(n, 3)` or `(n, 2)` float array; `Mesh.cells` is an ordered list of `CellBlock(cell_type, data, tags=None)`.
- `cell_data` values parallel `cells`: one array per `CellBlock`, indexed by block position; `cells_dict` and `cell_data_dict` are read views and never mutation targets.
- `point_sets`/`cell_sets` and the four set/data promoters are the physical-group bridge. Gmsh tags become named sets or integer data fields and feed FEM assemblies without losing group names.
- `extension_to_filetypes` selects candidate formats by suffix; ambiguous extensions such as `.msh` need explicit `file_format`.
- HDF5-backed formats (`med`, `xdmf`, `h5m`, CGNS) require the optional HDF5/NetCDF stack before use.

[STACK_LAW]:
- data mesh IO: `read`/`write` and `Mesh` are the data mesh-file boundary; solver, CAD-adjacent, and visualization formats cross through one shape.
- compute FEM seam: `scikit-fem` interconverts with meshio `Mesh` values (`MeshTri`/`MeshTet` `from_meshio` and `save`); physical groups ride `cell_sets` or integer `cell_data` into basis and boundary-condition selection.
- numeric seam: `points` and `CellBlock.data` are NumPy buffers and feed `nanoarrow`, `narwhals`, `scikit-fem`, and field solvers without bespoke converters.
- geometry seam: a watertight `Trimesh` crosses as vertices/faces arrays and enters meshio as a triangle `Mesh` for FEM-format or visualization-format export.
- field seam: structured or solved field values attach as `point_data` and export through `.vtu` or `.xdmf` for ParaView inspection without a bespoke writer.

[RAIL_LAW]:
- Package: `meshio`
- Owns: branch unstructured mesh file exchange, the canonical `Mesh` shape, cell/set/data conversions, per-format codec dispatch, Gmsh type mapping, and XDMF time-series IO
- Accept: paths or buffers with detectable or declared `file_format`, NumPy point arrays, `CellBlock` connectivity, named point/cell data, physical-group sets, and FEM handoff through meshio values
- Reject: hand-rolled mesh parsers, per-format mesh containers, lossy array-only exchange that drops point/cell data, wrapper-renames of `read`/`write`, and mutation through merged read views
