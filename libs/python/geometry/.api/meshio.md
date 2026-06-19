# [PY_GEOMETRY_API_MESHIO]

`meshio` supplies the unstructured-mesh exchange surface for the geometry mesh-io rail: a `Mesh` value of points plus typed `CellBlock` connectivity, the `read`/`write` polymorphic readers, and the per-format submodules that drive read/write across `VTK`/`VTU`/`XDMF`/`GMSH`/`OBJ`/`PLY`/`STL`/`OFF`/`MED`/`Abaqus`/`Nastran` with point/cell data fields. The package owner composes `meshio.read`, `Mesh`, and `meshio.write` into the mesh-io owner; it never re-implements format parsing meshio already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `meshio`
- package: `meshio`
- import: `import meshio`
- owner: `geometry`
- rail: mesh-io
- installed: `5.3.5` reflected via `python -c "import meshio"` on cp313
- entry points: none (library only)
- capability: read/write of 30-plus mesh formats, points plus typed cell blocks, point/cell/field data arrays, point and cell sets, named cell groups, format auto-detection by extension, and a dynamic format registry

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: mesh value and faults
- rail: mesh-io

| [INDEX] | [SYMBOL]     | [PACKAGE_ROLE] | [CAPABILITY]                                                |
| :-----: | :----------- | :------------- | :---------------------------------------------------------- |
|  [01]   | `Mesh`       | mesh value     | points plus cell blocks with point/cell/field data and sets |
|  [02]   | `CellBlock`  | cell block     | one cell type (`triangle`/`tetra`/`quad`) plus connectivity |
|  [03]   | `ReadError`  | read fault     | unreadable or malformed source                              |
|  [04]   | `WriteError` | write fault    | unwritable target or unsupported cell type                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, write, and build
- rail: mesh-io

Read and write rows share filename, optional `file_format` (auto-detected from extension when absent), and format-specific keyword options; `Mesh.read`/`Mesh.write` mirror the module functions as bound methods.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                    | [CAPABILITY]                      |
| :-----: | :-------------------------- | :------------------------------ | :-------------------------------- |
|  [01]   | `meshio.read`               | filename plus format policy     | read any format into a `Mesh`     |
|  [02]   | `meshio.write`              | filename plus mesh plus format  | write a `Mesh` to any format      |
|  [03]   | `meshio.write_points_cells` | filename plus points plus cells | write directly from arrays        |
|  [04]   | `Mesh`                      | points plus cells plus data     | construct a mesh value            |
|  [05]   | `Mesh.read`                 | filename plus format policy     | bound read constructor            |
|  [06]   | `Mesh.write`                | filename plus format policy     | bound write                       |
|  [07]   | `extension_to_filetypes`    | none                            | extension-to-format detection map |
|  [08]   | `register_format`           | name plus extensions plus io    | add a custom format               |
|  [09]   | `deregister_format`         | format name                     | remove a registered format        |

[ENTRYPOINT_SCOPE]: mesh projections
- rail: mesh-io

`Mesh` projection methods reshape connectivity and data between block-list and dict views.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]   | [CAPABILITY]                       |
| :-----: | :------------------------ | :------------- | :--------------------------------- |
|  [01]   | `Mesh.cells_dict`         | property       | cell type to connectivity map      |
|  [02]   | `Mesh.cell_data_dict`     | property       | cell-data name to per-type arrays  |
|  [03]   | `Mesh.get_cells_type`     | cell type      | connectivity for one cell type     |
|  [04]   | `Mesh.get_cell_data`      | name plus type | cell-data array for one type       |
|  [05]   | `Mesh.cell_sets_dict`     | property       | named cell-set membership map      |
|  [06]   | `Mesh.cell_sets_to_data`  | optional name  | sets projected to cell data        |
|  [07]   | `Mesh.cell_data_to_sets`  | data key       | cell data projected to sets        |
|  [08]   | `Mesh.point_sets_to_data` | optional name  | point sets projected to point data |

## [04]-[IMPLEMENTATION_LAW]

[MESH_IO]:
- import: `import meshio` at boundary scope only; module-level import is banned by the manifest import policy.
- read axis: `meshio.read` is the polymorphic intake; `file_format` selects a parser only when the extension is ambiguous, resolved through `extension_to_filetypes`, never a parallel reader function per format.
- mesh axis: one `Mesh` owns `points`, a list of `CellBlock` connectivity, `point_data`, `cell_data`, `field_data`, and named `point_sets`/`cell_sets`; cell types stay typed strings (`triangle`, `tetra`, `quad`), never positional arrays.
- write axis: `meshio.write` dispatches on `file_format` or extension; `write_points_cells` is the array shortcut that skips `Mesh` construction.
- registry axis: `register_format`/`deregister_format` mutate the format table at runtime; a custom format is a registry row, never a subclass hierarchy.
- evidence: each read/write op captures point count, cell-block count per type, point-data and cell-data field names, and output byte length as a mesh-io receipt.
- boundary: meshio owns FEM/CAE unstructured-mesh exchange; triangular surface processing routes to `trimesh`, point clouds to `open3d`, Rhino `.3dm` to `rhino3dm`; live UI stays outside this package.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `meshio`
- Owns: read/write of 30-plus unstructured-mesh formats, typed cell blocks, point/cell/field data, named sets, and a runtime format registry
- Accept: FEM/CAE mesh ingestion and export feeding the mesh-io and geometry owners
- Reject: wrapper-renames of `read`/`write`; a hand-rolled `VTK`/`GMSH`/`XDMF` parser where meshio is admitted; positional cell arrays that drop the typed `CellBlock`; identity minting the runtime owns

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `meshio==5.3.5` ships pure-Python wheels but rides the `numpy` native floor, and the `>=3.15` project venv carries no scientific companions, so the project-venv `assay api query` resolves no source there
- members: verified by introspection against the installed cp313 distribution; every documented type, entrypoint, projection method, and format submodule resolves — no phantom
