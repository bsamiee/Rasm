# [PY_BRANCH_API_MESHIO]

`meshio` owns branch unstructured-mesh file exchange: one `Mesh` value carries points, typed `CellBlock` connectivity, point/cell/field data, and named sets across solver, visualization, CAD-adjacent, and FEM formats through a single shape, no per-format container.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio` (`MIT`)
- module: `meshio`
- asset: pure-Python; `Mesh` point/cell arrays are NumPy buffers
- rail: mesh file exchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh value and failures

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Mesh`       | mesh value    | points, cells, point/cell/field data, sets, metadata            |
|  [02]   | `CellBlock`  | connectivity  | homogeneous cell type with connectivity array and optional tags |
|  [03]   | `ReadError`  | fault         | malformed or unsupported input file                             |
|  [04]   | `WriteError` | fault         | unsupported cell type or target format                          |

[PUBLIC_TYPE_SCOPE]: `Mesh` members

| [INDEX] | [SYMBOL]                                           | [TYPE_FAMILY] | [CAPABILITY]                                       |
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

| [INDEX] | [SURFACE]                                                      | [ENTRY_FAMILY] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `read(filename, file_format=None) -> Mesh`                     | intake         | parse a path or buffer into a `Mesh`         |
|  [02]   | `write(filename, mesh, file_format=None, **kwargs)`            | export         | serialize a `Mesh`                           |
|  [03]   | `write_points_cells(filename, points, cells, ...)`             | export         | write from arrays without an explicit `Mesh` |
|  [04]   | `register_format(format_name, extensions, reader, writer_map)` | registry       | add a custom format reader/writer map        |
|  [05]   | `deregister_format(format_name)`                               | registry       | remove a registered format                   |
|  [06]   | `extension_to_filetypes`                                       | registry       | extension-to-format candidate mapping        |

- `write_points_cells` carries: `point_data`, `cell_data`, `field_data`, `point_sets`, `cell_sets`, `file_format`.

[ENTRYPOINT_SCOPE]: per-format surfaces

| [INDEX] | [SURFACE]                                                                      | [ENTRY_FAMILY] | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `vtk.read` / `vtk.write`                                                       | format codec   | legacy VTK serial mesh        |
|  [02]   | `vtu.read` / `vtu.write`                                                       | format codec   | XML VTK unstructured grid     |
|  [03]   | `xdmf.read` / `xdmf.write` / `xdmf.TimeSeriesReader` / `xdmf.TimeSeriesWriter` | format codec   | XDMF and time-series XDMF     |
|  [04]   | `gmsh.read` / `gmsh.write` / `gmsh_to_meshio_type` / `meshio_to_gmsh_type`     | format codec   | Gmsh mesh and element mapping |
|  [05]   | `stl` / `ply` / `obj` / `off` / `svg` / `wkt`                                  | format codec   | surface and geometry exchange |
|  [06]   | `med` / `abaqus` / `nastran` / `ansys` / `su2` / `permas` / `mdpa`             | format codec   | FEA and solver decks          |
|  [07]   | `tetgen` / `netgen` / `ugrid` / `cgns` / `exodus` / `h5m` / `hmf` / `dolfin`   | format codec   | HDF5/NetCDF and FE frameworks |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Mesh.points` is an `(n, 3)` or `(n, 2)` float array; `Mesh.cells` is an ordered list of `CellBlock(cell_type, data, tags=None)`.
- `cell_data` values parallel `cells` one array per `CellBlock` indexed by block position; `cells_dict` and `cell_data_dict` are read views, never mutation targets.
- `point_sets` / `cell_sets` and the four set/data promoters bridge physical groups: Gmsh tags become named sets or integer data fields and feed FEM assemblies without losing group names.
- `extension_to_filetypes` selects candidate formats by suffix; an ambiguous extension such as `.msh` needs explicit `file_format`.

[STACKING]:
- `numpy`(`.api/numpy.md`): `Mesh.points` is an `(n, 3)` / `(n, 2)` float `ndarray` and each `CellBlock.data` an `ndarray`, read zero-copy by the numeric rail through `ascontiguousarray` and the buffer protocol before FEM assembly or field-solver hand-off.
- data mesh-IO boundary: `read` / `write` over one `Mesh` cross solver, CAD-adjacent, and visualization formats through a single shape.
- `scikit-fem` FEM seam: `MeshTri` / `MeshTet` `from_meshio` and `save` interconvert with a `Mesh`; physical groups ride `cell_sets` or integer `cell_data` into basis and boundary-condition selection.
- `Trimesh` geometry seam: a watertight mesh crosses as vertex/face arrays and enters as a triangle `Mesh` for FEM-format or visualization-format export.
- field seam: solved field values attach as `point_data` and export through `.vtu` or `.xdmf` for ParaView inspection.

[LOCAL_ADMISSION]:
- `meshio` is the sole mesh-file exchange owner; a new format enters through `register_format`, never a bespoke parser or per-format container.
- HDF5-backed formats (`med`, `xdmf`, `h5m`, CGNS) bind the optional HDF5/NetCDF stack.

[RAIL_LAW]:
- Package: `meshio`
- Owns: unstructured mesh file exchange, the canonical `Mesh` shape, cell/set/data conversions, per-format codec dispatch, Gmsh type mapping, and XDMF time-series IO
- Accept: paths or buffers with detected or declared `file_format`, NumPy point arrays, `CellBlock` connectivity, named point/cell data, physical-group sets, and FEM handoff through `Mesh` values
- Reject: hand-rolled mesh parsers, per-format mesh containers, array-only exchange dropping point/cell data, wrapper-renames of `read` / `write`, and mutation through merged read views
