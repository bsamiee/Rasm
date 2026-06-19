# [PY_ARTIFACTS_API_VTK]

`vtk` supplies the Visualization Toolkit demand-driven pipeline for the artifacts visuals rail: dataset types (`vtkPolyData`, `vtkImageData`, `vtkUnstructuredGrid`) flow through `vtkAlgorithm` source/filter stages connected by `GetOutputPort`/`SetInputConnection`, render through `vtkMapper`/`vtkActor`/`vtkRenderer`/`vtkRenderWindow`, and serialize through reader/writer pairs. The `vtkmodules` package partitions every class by capability module.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vtk`
- package: `vtk`
- module: `vtk` (aggregates `vtkmodules.*`)
- asset: C++ native runtime library with Python bindings
- rail: visuals

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset and core data
- rail: visuals — `vtkmodules.vtkCommonDataModel`, `vtkCommonCore`

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [ROLE]                                              |
| :-----: | :-------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `vtkPolyData`         | dataset       | points plus verts/lines/polys/strips (surface mesh) |
|  [02]   | `vtkImageData`        | dataset       | regular voxel/pixel grid                            |
|  [03]   | `vtkUnstructuredGrid` | dataset       | arbitrary heterogeneous cell mesh                   |
|  [04]   | `vtkStructuredGrid`   | dataset       | curvilinear structured grid                         |
|  [05]   | `vtkRectilinearGrid`  | dataset       | axis-aligned variable-spacing grid                  |
|  [06]   | `vtkPoints`           | array         | point coordinate container                          |
|  [07]   | `vtkCellArray`        | array         | cell connectivity container                         |
|  [08]   | `vtkDataArray`        | array base    | typed attribute array base                          |
|  [09]   | `vtkFloatArray`       | array         | float attribute array                               |
|  [10]   | `vtkIdList`           | array         | ordered point/cell id list                          |
|  [11]   | `vtkPointData`        | attribute set | per-point scalar/vector/normal arrays               |
|  [12]   | `vtkCellData`         | attribute set | per-cell attribute arrays                           |

[PUBLIC_TYPE_SCOPE]: pipeline algorithm bases
- rail: visuals — `vtkmodules.vtkCommonExecutionModel`
- type family: pipeline base

| [INDEX] | [SYMBOL]               | [ROLE]                               |
| :-----: | :--------------------- | :----------------------------------- |
|  [01]   | `vtkAlgorithm`         | demand-driven source/filter contract |
|  [02]   | `vtkPolyDataAlgorithm` | algorithms emitting `vtkPolyData`    |
|  [03]   | `vtkDataSetAlgorithm`  | algorithms over generic datasets     |

[PUBLIC_TYPE_SCOPE]: rendering and interaction
- rail: visuals — `vtkmodules.vtkRenderingCore`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [ROLE]                                  |
| :-----: | :-------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `vtkRenderer`               | render        | scene of actors, lights, camera         |
|  [02]   | `vtkRenderWindow`           | render        | window/offscreen render target          |
|  [03]   | `vtkRenderWindowInteractor` | interaction   | event loop and input handling           |
|  [04]   | `vtkActor`                  | prop          | renderable instance of mapped geometry  |
|  [05]   | `vtkMapper`                 | mapper base   | dataset-to-graphics mapping base        |
|  [06]   | `vtkPolyDataMapper`         | mapper        | maps `vtkPolyData` to primitives        |
|  [07]   | `vtkProperty`               | appearance    | color, opacity, shading, representation |
|  [08]   | `vtkCamera`                 | view          | viewpoint, projection, clipping         |
|  [09]   | `vtkLight`                  | lighting      | scene light source                      |
|  [10]   | `vtkLookupTable`            | color map     | scalar-to-color mapping table           |
|  [11]   | `vtkScalarBarActor`         | overlay       | color legend                            |
|  [12]   | `vtkAxesActor`              | overlay       | coordinate axes widget                  |

[PUBLIC_TYPE_SCOPE]: sources and filters
- rail: visuals — `vtkmodules.vtkFiltersSources`, `vtkFiltersCore`, `vtkFiltersGeneral`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [ROLE]                             |
| :-----: | :--------------------------- | :------------ | :--------------------------------- |
|  [01]   | `vtkSphereSource`            | source        | parametric sphere mesh             |
|  [02]   | `vtkConeSource`              | source        | cone mesh                          |
|  [03]   | `vtkCubeSource`              | source        | box mesh                           |
|  [04]   | `vtkCylinderSource`          | source        | cylinder mesh                      |
|  [05]   | `vtkPlaneSource`             | source        | planar grid mesh                   |
|  [06]   | `vtkContourFilter`           | filter        | iso-surface/iso-line extraction    |
|  [07]   | `vtkClipPolyData`            | filter        | clip surface by implicit function  |
|  [08]   | `vtkCutter`                  | filter        | cross-section by implicit function |
|  [09]   | `vtkGlyph3D`                 | filter        | place a glyph at each point        |
|  [10]   | `vtkThreshold`               | filter        | extract cells by scalar range      |
|  [11]   | `vtkSmoothPolyDataFilter`    | filter        | Laplacian surface smoothing        |
|  [12]   | `vtkDecimatePro`             | filter        | triangle-count reduction           |
|  [13]   | `vtkPolyDataNormals`         | filter        | compute point/cell normals         |
|  [14]   | `vtkTriangleFilter`          | filter        | triangulate polygons/strips        |
|  [15]   | `vtkTransformPolyDataFilter` | filter        | apply a `vtkTransform` to geometry |

[PUBLIC_TYPE_SCOPE]: I/O and geometry
- rail: visuals — `vtkmodules.vtkIOGeometry`, `vtkIOPLY`, `vtkIOXML`, `vtkCommonTransforms`

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [ROLE]                      |
| :-----: | :--------------------- | :------------ | :-------------------------- |
|  [01]   | `vtkSTLReader`         | reader        | read binary/ASCII STL       |
|  [02]   | `vtkSTLWriter`         | writer        | write STL                   |
|  [03]   | `vtkPLYReader`         | reader        | read PLY                    |
|  [04]   | `vtkPLYWriter`         | writer        | write PLY                   |
|  [05]   | `vtkOBJReader`         | reader        | read Wavefront OBJ          |
|  [06]   | `vtkXMLPolyDataReader` | reader        | read `.vtp` XML polydata    |
|  [07]   | `vtkXMLPolyDataWriter` | writer        | write `.vtp` XML polydata   |
|  [08]   | `vtkTransform`         | transform     | composable affine transform |
|  [09]   | `vtkMatrix4x4`         | math          | 4x4 homogeneous matrix      |
|  [10]   | `vtkPlane`             | implicit      | implicit plane function     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline connection (on `vtkAlgorithm`)
- rail: visuals — every source/filter/reader shares this contract

| [INDEX] | [SURFACE]                         | [ENTRY_FAMILY] | [ROLE]                                    |
| :-----: | :-------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `SetInputConnection(output_port)` | connect        | wire upstream output to this input        |
|  [02]   | `GetOutputPort()`                 | connect        | output port for downstream wiring         |
|  [03]   | `SetInputData(data_object)`       | connect        | feed an in-memory dataset directly        |
|  [04]   | `GetOutput()`                     | accessor       | output dataset after `Update`             |
|  [05]   | `Update()`                        | execute        | run the pipeline up to this stage         |
|  [06]   | `UpdateInformation()`             | execute        | propagate metadata without computing data |

[ENTRYPOINT_SCOPE]: dataset and array construction
- rail: visuals — `vtkPolyData`, `vtkPoints`, `vtkCellArray`, `vtkFloatArray`

| [INDEX] | [SURFACE]                                                   | [ENTRY_FAMILY] | [ROLE]                          |
| :-----: | :---------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `vtkPoints.InsertNextPoint(x, y, z)`                        | build          | append a coordinate             |
|  [02]   | `vtkCellArray.InsertNextCell(npts)` + `InsertCellPoint(id)` | build          | append a cell and its point ids |
|  [03]   | `vtkPolyData.SetPoints(points)`                             | assemble       | attach coordinates              |
|  [04]   | `vtkPolyData.SetPolys(cells)`                               | assemble       | attach polygon connectivity     |
|  [05]   | `vtkPolyData.GetPointData()` / `GetCellData()`              | accessor       | attribute-array sets            |
|  [06]   | `vtkFloatArray.SetName(name)` + `InsertNextValue(v)`        | build          | named scalar attribute array    |
|  [07]   | `vtkFloatArray.SetNumberOfComponents(n)`                    | configure      | per-tuple component count       |

[ENTRYPOINT_SCOPE]: rendering setup
- rail: visuals — `vtkRenderer`, `vtkRenderWindow`, `vtkActor`, `vtkCamera`

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [ROLE]                              |
| :-----: | :----------------------------------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `vtkPolyDataMapper.SetInputConnection(port)`                 | map            | bind geometry to a mapper           |
|  [02]   | `vtkPolyDataMapper.SetScalarRange(lo, hi)`                   | map            | scalar-to-color domain              |
|  [03]   | `vtkActor.SetMapper(mapper)`                                 | scene          | bind a mapper to an actor           |
|  [04]   | `vtkActor.GetProperty()`                                     | scene          | access color/opacity/representation |
|  [05]   | `vtkRenderer.AddActor(actor)`                                | scene          | add a prop to the scene             |
|  [06]   | `vtkRenderer.SetBackground(r, g, b)`                         | scene          | scene background color              |
|  [07]   | `vtkRenderer.ResetCamera()`                                  | view           | frame all actors                    |
|  [08]   | `vtkRenderWindow.AddRenderer(renderer)`                      | window         | attach a renderer                   |
|  [09]   | `vtkRenderWindow.SetSize(w, h)` + `Render()`                 | window         | size and draw                       |
|  [10]   | `vtkRenderWindow.SetOffScreenRendering(1)`                   | window         | headless render target              |
|  [11]   | `vtkRenderWindowInteractor.SetRenderWindow(win)` + `Start()` | window         | interactive event loop              |

[ENTRYPOINT_SCOPE]: file I/O
- rail: visuals — reader/writer pairs

| [INDEX] | [SURFACE]                                                                 | [ENTRY_FAMILY] | [ROLE]                      |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------------- |
|  [01]   | `vtkSTLReader.SetFileName(path)` + `Update()`                             | read           | load STL into `GetOutput()` |
|  [02]   | `vtkSTLWriter.SetInputConnection(port)` + `SetFileName(path)` + `Write()` | write          | save STL (ASCII or binary)  |
|  [03]   | `vtkXMLPolyDataWriter.SetDataModeToBinary()`                              | write          | binary XML output mode      |
|  [04]   | `vtkXMLPolyDataReader.SetFileName(path)` + `Update()`                     | read           | load `.vtp`                 |
|  [05]   | `vtkSTLWriter.SetFileTypeToBinary()` / `SetFileTypeToASCII()`             | write          | STL encoding mode           |

## [04]-[IMPLEMENTATION_LAW]

[VTK_TOPOLOGY]:
- demand-driven pipeline: stages connect with `downstream.SetInputConnection(upstream.GetOutputPort())`; nothing computes until a terminal `Update`, `Write`, or `Render` pulls data
- render stack order: source/filter -> `vtkPolyDataMapper` -> `vtkActor` -> `vtkRenderer` -> `vtkRenderWindow` -> `vtkRenderWindowInteractor`
- modules: classes live under `vtkmodules.<Module>` (`vtkCommonCore`, `vtkCommonDataModel`, `vtkFiltersCore`, `vtkRenderingCore`, `vtkIOGeometry`, `vtkIOXML`); the `vtk` package re-exports them flat
- object construction is always `vtkClass()` (no positional constructor args); configure through `Set*`/`Get*` methods
- `SetInputConnection` wires a live pipeline that re-executes on upstream change; `SetInputData` snapshots a static dataset
- offscreen rendering via `vtkRenderWindow.SetOffScreenRendering(1)` produces images without an interactor

[LOCAL_ADMISSION]:
- Wire stages with `SetInputConnection`/`GetOutputPort` for pipelines that re-execute; use `SetInputData` only for one-shot static datasets.
- Call a single terminal `Update`/`Write`/`Render`; never call `Update` on every intermediate stage.
- Build geometry through `vtkPoints` + `vtkCellArray` and attach via `SetPoints`/`SetPolys`; carry scalars in named `vtkFloatArray` on `GetPointData`/`GetCellData`.
- Import canonical classes from `vtk` (flat re-export) or from the specific `vtkmodules.<Module>` for narrower import surface.
- Render headless with `SetOffScreenRendering(1)`; reserve `vtkRenderWindowInteractor.Start()` for interactive sessions.

[RAIL_LAW]:
- Package: `vtk`
- Owns: 3D scientific visualization, the demand-driven dataset pipeline, geometry sources/filters, file I/O, and the rendering/interaction stack
- Accept: pipeline wiring via `SetInputConnection`/`GetOutputPort`; datasets built through `vtkPoints`/`vtkCellArray`/`vtkDataArray`
- Reject: hand-rolled mesh-processing or rendering loops where a VTK filter/mapper exists; wrapper-renames of the pipeline contract
