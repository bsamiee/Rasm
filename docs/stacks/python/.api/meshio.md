# [PY_COMPUTE_API_MESHIO]

`meshio` supplies a single in-memory `Mesh` container plus polymorphic `read`/`write` dispatch across ~40 unstructured-mesh formats for the compute mesh-exchange rail; points, typed `CellBlock` connectivity, and named point/cell data round-trip through one shape regardless of on-disk format, so a FEM assembler, a point-cloud meshing step, and a CAD/CFD exporter all meet at the same `Mesh` rather than per-format parsers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- version: `5.3.5`
- license: MIT
- import: `meshio`
- owner: `compute`
- rail: mesh-exchange
- asset: pure-Python; runtime deps `numpy>=1.20.0`, `rich`; HDF5/NetCDF formats (`med`, `xdmf`, `h5m`, CGNS) need the `all` extra (`h5py`, `netCDF4`)
- floor: unconditional manifest dependency (no interpreter gate; pure-Python, cp315-clean) — verify on sync

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
- rail: mesh-exchange — constructor `Mesh(points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, gmsh_periodic=None, info=None)` where `cells` is `dict[str, ArrayLike] | list[tuple[str, ArrayLike] | CellBlock]`

| [INDEX] | [SYMBOL]             | [KIND]    | [ROLE]                                                       |
| :-----: | :------------------- | :-------- | :----------------------------------------------------------- |
|  [01]   | `points`             | attribute | `(n, dim)` float array of vertex coordinates                 |
|  [02]   | `cells`              | attribute | list of `CellBlock` connectivity blocks                      |
|  [03]   | `point_data`         | attribute | name → per-point array mapping                               |
|  [04]   | `cell_data`          | attribute | name → list-of-arrays (one array per `CellBlock`)            |
|  [05]   | `field_data`         | attribute | name → metadata array mapping                                |
|  [06]   | `point_sets`         | attribute | name → point-index array mapping                             |
|  [07]   | `cell_sets`          | attribute | name → per-block index array list mapping                    |
|  [08]   | `gmsh_periodic`      | attribute | Gmsh periodic-link records (round-trips only for the Gmsh format) |
|  [09]   | `info`               | attribute | free-form format-carried metadata                            |
|  [10]   | `cells_dict`         | property  | cell type → merged connectivity array across all blocks      |
|  [11]   | `cell_data_dict`     | property  | name → cell type → array nested mapping                      |
|  [12]   | `cell_sets_dict`     | property  | set name → cell type → index mapping                         |
|  [13]   | `get_cells_type(cell_type)` | method | connectivity array for one cell type                    |
|  [14]   | `get_cell_data(name, cell_type)` | method | cell-data array for one name and cell type         |
|  [15]   | `cell_sets_to_data(data_name=None)` | method | promote cell sets into an integer `cell_data` field |
|  [16]   | `cell_data_to_sets(key)` | method | invert an integer `cell_data` field into named cell sets |
|  [17]   | `point_sets_to_data(join_char='-')` | method | promote point sets into integer `point_data`     |
|  [18]   | `point_data_to_sets(key)` | method | invert an integer `point_data` field into named point sets |
|  [19]   | `copy()`             | method    | deep copy of the mesh                                        |
|  [20]   | `read(path_or_buf, file_format=None)` | classmethod | construct a `Mesh` from a path or buffer        |
|  [21]   | `write(path_or_buf, file_format=None, **kwargs)` | method | serialize this mesh to a path or buffer       |

[PUBLIC_TYPE_SCOPE]: `CellBlock` members
- rail: mesh-exchange — constructor `CellBlock(cell_type, data, tags=None)`

| [INDEX] | [SYMBOL]    | [KIND]    | [ROLE]                                                            |
| :-----: | :---------- | :-------- | :---------------------------------------------------------------- |
|  [01]   | `type`      | attribute | cell-type string (e.g. `triangle`, `tetra`, `hexahedron`)         |
|  [02]   | `data`      | attribute | `(n_cells, nodes_per_cell)` int connectivity array                |
|  [03]   | `dim`       | attribute | topological dimension inferred from `type` (line→1, triangle→2, tetra→3) |
|  [04]   | `tags`      | attribute | optional list of region/group tag strings                         |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: format dispatch — `file_format=None` triggers extension-based detection
- rail: mesh-exchange

| [INDEX] | [SURFACE]                                                                                                                                | [ENTRY_FAMILY] | [ROLE]                                   |
| :-----: | :--------------------------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `read(filename, file_format=None)` → `Mesh`                                                                                              | reader         | parse any format into a `Mesh`           |
|  [02]   | `write(filename, mesh, file_format=None, **kwargs)`                                                                                      | writer         | serialize a `Mesh` to any format         |
|  [03]   | `write_points_cells(filename, points, cells, point_data=None, cell_data=None, field_data=None, point_sets=None, cell_sets=None, file_format=None, **kwargs)` | writer         | write directly from points + cells, skipping an explicit `Mesh` |

[ENTRYPOINT_SCOPE]: format registry and per-format submodules
- rail: mesh-exchange

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [ROLE]                                                       |
| :-----: | :--------------------------------------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `register_format(format_name, extensions, reader, writer_map)`         | registry       | add a custom format reader and per-extension writer map      |
|  [02]   | `deregister_format(format_name)`                                       | registry       | remove a registered format                                   |
|  [03]   | `extension_to_filetypes`                                               | mapping        | extension → candidate format-name list (drives auto-detect)  |
|  [04]   | `meshio.stl` / `.vtu` / `.vtk` / `.xdmf` / `.gmsh` / `.obj` / `.ply` / `.med` / `.nastran` / `.abaqus` / `.cgns` / `.exodus` / `.tetgen` / ... | submodule | direct per-format `read`/`write` when bypassing auto-detect; full set: abaqus, ansys, avsucd, cgns, dolfin, exodus, flac3d, gmsh, h5m, hmf, mdpa, med, medit, nastran, netgen, neuroglancer, obj, off, permas, ply, stl, su2, svg, tecplot, tetgen, ugrid, vtk, vtu, wkt, xdmf |

## [04]-[INTEGRATION_TOPOLOGY]

[STACKS_WITH]: meshio is the format-neutral seam between the geometry/CAD producers and the FEM/numerical consumers; one `Mesh` flows from generation to assembly to export without per-format glue.

| [INDEX] | [SIBLING]      | [SEAM]                                                                                                            |
| :-----: | :------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | `scikit-fem`   | `skfem.Mesh` interconverts with a meshio `Mesh` (`MeshTri`/`MeshTet` carry `from_meshio`/`save`); read a `.msh`/`.vtu`, hand the points + `cells_dict[cell_type]` to the FEM basis, write the solution `point_data` back to `.xdmf` |
|  [02]   | `numpy`        | `points` and `CellBlock.data` are plain `numpy` arrays; build a `Mesh` directly from a `(n,3)` vertex array and an `(m,3)` triangle index array with zero copy |
|  [03]   | `trimesh`/spatial | a watertight triangulation crosses the geometry-data wire as vertices/faces arrays, then enters meshio as `Mesh(verts, [("triangle", faces)])` for FEM-format export — meshio owns the unstructured-format encode that the in-memory `Trimesh` does not |
|  [04]   | `findiff`/field solvers | structured-grid results attach as `point_data` and export through `.vtu`/`.xdmf` for ParaView inspection without a bespoke writer |

[MESHIO_TOPOLOGY]:
- `cells` is a list of `CellBlock(cell_type, data, tags=None)`; `cell_type` is a string such as `triangle`, `quad`, `tetra`, `hexahedron`, `line`, `vertex`, `tetra10` (second-order).
- `cell_data` values parallel `cells`: a list with one array per `CellBlock`, indexed by block position — never a flat array; preserve block order on round-trip.
- `cells_dict` merges all blocks of one type into a single array for read paths; mutate the original `cells` block list, not the `cells_dict` view.
- `point_sets`/`cell_sets` and the four `*_to_data`/`*_to_sets` promoters convert between named index sets and integer label fields — use them to carry physical groups (Gmsh tags) through formats that only support integer labels.
- `extension_to_filetypes` maps e.g. `.msh → [ansys, gmsh]`, `.vtu → [vtu]`, `.stl → [stl]`, `.xdmf → [xdmf]`; ambiguous extensions (`.msh`, `.dat`) need an explicit `file_format`.

[LOCAL_ADMISSION]:
- Construct a `Mesh` from a points array and a `cells` list of `(type, connectivity)` tuples or `CellBlock` instances; never assemble parallel dicts by hand.
- Route all on-disk conversion through `read`/`write`; let extension detection pick the format and pass `file_format` only to disambiguate.
- Read per-type connectivity through `cells_dict` or `get_cells_type`; keep the original `cells` block list for write fidelity and per-block `cell_data`.
- Format-specific keyword options (binary mode, compression, `data_format`) pass through `**kwargs` to `write`; HDF5-backed formats (`med`, `xdmf`, `h5m`) require the `all` extra installed.

[RAIL_LAW]:
- Package: `meshio`
- Owns: unstructured mesh file exchange across the supported format set and the canonical in-memory `Mesh` shape
- Accept: `numpy` point arrays and `CellBlock` connectivity; format dispatch by extension or explicit `file_format`; named point/cell data and physical-group sets
- Reject: hand-rolled format parsers; parallel per-format mesh containers; wrapper-renames of `read`/`write`; mutation through the `cells_dict` read view
