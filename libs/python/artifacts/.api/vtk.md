# [PY_ARTIFACTS_API_VTK]

`vtk` supplies the Visualization Toolkit demand-driven pipeline for the artifacts visuals rail: dataset types (`vtkPolyData`, `vtkImageData`, `vtkUnstructuredGrid`) flow through `vtkAlgorithm` source/filter stages connected by `GetOutputPort`/`SetInputConnection`, render through `vtkMapper`/`vtkActor`/`vtkRenderer`/`vtkRenderWindow`, and serialize through reader/writer pairs. The `vtkmodules` package partitions every class by capability module.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vtk`
- package: `vtk`
- module: `vtk` (aggregates `vtkmodules.*`)
- license: `BSD-3-Clause`
- consumer: `pyvista` is the in-repo high-level consumer — vtk is the engine pyvista wraps for the artifacts visuals rail; design pages compose pyvista's pythonic API and drop to raw `vtk` only for pipeline stages pyvista does not expose
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

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]               | [ROLE]                                                                                                                                                            |
| :-----: | :--------------------------- | :-------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `vtkSphereSource`            | source                      | parametric sphere mesh                                                                                                                                            |
|  [02]   | `vtkConeSource`              | source                      | cone mesh                                                                                                                                                         |
|  [03]   | `vtkCubeSource`              | source                      | box mesh                                                                                                                                                          |
|  [04]   | `vtkCylinderSource`          | source                      | cylinder mesh                                                                                                                                                     |
|  [05]   | `vtkPlaneSource`             | source                      | planar grid mesh                                                                                                                                                  |
|  [06]   | `vtkContourFilter`           | filter                      | iso-surface/iso-line extraction                                                                                                                                   |
|  [07]   | `vtkClipPolyData`            | filter                      | clip surface by implicit function                                                                                                                                 |
|  [08]   | `vtkCutter`                  | filter                      | cross-section by implicit function                                                                                                                                |
|  [09]   | `vtkGlyph3D`                 | filter                      | place a glyph at each point                                                                                                                                       |
|  [10]   | `vtkThreshold`               | filter                      | extract cells by scalar range                                                                                                                                     |
|  [11]   | `vtkSmoothPolyDataFilter`    | filter                      | Laplacian surface smoothing                                                                                                                                       |
|  [12]   | `vtkDecimatePro`             | filter                      | triangle-count reduction                                                                                                                                          |
|  [13]   | `vtkPolyDataNormals`         | filter                      | compute point/cell normals                                                                                                                                        |
|  [14]   | `vtkTriangleFilter`          | filter                      | triangulate polygons/strips                                                                                                                                       |
|  [15]   | `vtkTransformPolyDataFilter` | filter                      | apply a `vtkTransform` to geometry                                                                                                                                |
|  [16]   | `vtkFeatureEdges`            | filter (`vtkFiltersCore`)   | extract boundary / feature-angle / non-manifold / manifold edges as line `vtkPolyData` — the crease-and-outline linework the plan/section drawing egress consumes |
|  [17]   | `vtkPolyDataSilhouette`      | filter (`vtkFiltersHybrid`) | view-dependent silhouette outline of a surface relative to a `vtkCamera` — the true occluding-contour line set for a projected drawing                            |

[PUBLIC_TYPE_SCOPE]: I/O and geometry
- rail: visuals — `vtkmodules.vtkIOGeometry`, `vtkIOPLY`, `vtkIOXML`, `vtkCommonTransforms`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [ROLE]                                               |
| :-----: | :----------------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `vtkSTLReader`           | reader        | read binary/ASCII STL                                |
|  [02]   | `vtkSTLWriter`           | writer        | write STL                                            |
|  [03]   | `vtkPLYReader`           | reader        | read PLY                                             |
|  [04]   | `vtkPLYWriter`           | writer        | write PLY                                            |
|  [05]   | `vtkOBJReader`           | reader        | read Wavefront OBJ                                   |
|  [06]   | `vtkXMLPolyDataReader`   | reader        | read `.vtp` XML polydata                             |
|  [07]   | `vtkXMLPolyDataWriter`   | writer        | write `.vtp` XML polydata                            |
|  [08]   | `vtkTransform`           | transform     | composable affine transform                          |
|  [09]   | `vtkMatrix4x4`           | math          | 4x4 homogeneous matrix                               |
|  [10]   | `vtkPlane`               | implicit      | implicit plane function                              |
|  [11]   | `vtkWindowToImageFilter` | capture       | grab a render window's framebuffer as `vtkImageData` |
|  [12]   | `vtkPNGWriter`           | writer        | write captured framebuffer to PNG (artifact export)  |

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

[INTEGRATION]:
- pyvista composition: `pyvista.wrap(polydata)` adopts a `vtkPolyData`/`vtkImageData` into a `pyvista.DataSet` with NumPy-array attribute access; conversely a `pyvista.PolyData` exposes its underlying `vtkPolyData` for a filter pyvista does not wrap. Stay on the pyvista surface for plotting, scalar bars, and camera; drop to raw `vtk` only for the missing filter/mapper, then re-wrap.
- artifact export: for the artifacts visuals rail run `vtkRenderWindow.SetOffScreenRendering(1)` (or pyvista `off_screen=True`) and capture the framebuffer to a PNG via `vtkWindowToImageFilter` -> `vtkPNGWriter`, feeding the artifacts image/download owner. Never spin a `vtkRenderWindowInteractor` event loop in a headless export path.
- scene-export surface: the official `vtk` wheel ships the `vtkIOExport` scene exporters (`vtkGLTFExporter`/`vtkOBJExporter`/`vtkVRMLExporter`/`vtkX3DExporter`, reached through pyvista `Plotter.export_gltf`/`export_obj`/`export_vrml`) but no USD exporter — there is no `vtkmodules.vtkIOUSD` module in the wheel (VTK's USD I/O requires a source build against OpenUSD), so USD/USDZ authoring is owned by `usd-core` (`pxr`), not `vtk`.
- silhouette/feature-edge surface (`scene/render#SCENE` V9 `FieldFilter` cases): `vtkFeatureEdges` extracts crease and outline linework — `SetBoundaryEdges(bool)`/`SetFeatureEdges(bool)`/`SetNonManifoldEdges(bool)`/`SetManifoldEdges(bool)` gate the edge classes and `SetFeatureAngle(deg)` sets the crease threshold; `vtkPolyDataSilhouette` extracts the true view-dependent occluding contour — `SetCamera(vtkCamera)` binds the viewpoint (`SetDirection`/`SetDirectionToCameraOrigin` selects camera-vs-origin), `SetEnableFeatureAngle(int)`/`SetFeatureAngle(deg)` add crease edges, `SetBorderEdges(int)` adds open borders. Both emit line `vtkPolyData` that `vtk_to_numpy`/`pyvista.wrap` hands to the drawing egress as extracted vectors — the plan/section linework reaching sheets, never a rasterize-then-trace.
- GL2PS 3D→vector target: `vtkGL2PSExporter` (module `vtkmodules.vtkIOExportGL2PS`, reached through the `Plotter.render_window` bridge) writes a rendered scene to real vector formats — `SetRenderWindow(win)`, `SetFileFormatToPS()`/`ToEPS()`/`ToPDF()`/`ToSVG()`/`ToTeX()`, `SetFilePrefix(path)`, `SetSortToBSP()`/`SetSortToSimple()` for depth ordering, `SetCompress(bool)`, `Write()` — the vector-output path for the V9 scene-to-vector target where a raster framebuffer does lose the linework.
- numpy seam: `vtkmodules.util.numpy_support.numpy_to_vtk`/`vtk_to_numpy` is the zero-copy bridge between a universal-tier `numpy` (`libs/python/.api/numpy.md`) mesh/scalar array and a `vtkDataArray`; build attribute arrays from canonical NumPy buffers through it rather than per-element `InsertNextValue` loops for large datasets — the same `numpy` buffer a geometry/compute owner produced becomes point coordinates (`numpy_to_vtk` -> `vtkPoints.SetData`) or a named scalar field in one call.
- artifact PNG seam: the headless export is `vtkRenderWindow.SetOffScreenRendering(1)` -> `Render()` -> `vtkWindowToImageFilter.SetInput(window)` + `Update()` -> `vtkPNGWriter.SetInputConnection(w2i.GetOutputPort())` + `SetFileName`/`Write()`, or `vtk_to_numpy` on the `vtkWindowToImageFilter` output to hand the framebuffer to the `pillow`/image artifacts owner as a `numpy` RGB array — never an interactor loop in the export path.

[RAIL_LAW]:
- Package: `vtk`
- Owns: 3D scientific visualization, the demand-driven dataset pipeline, geometry sources/filters, file I/O, and the rendering/interaction stack — as the native engine under pyvista
- Accept: pipeline wiring via `SetInputConnection`/`GetOutputPort`; datasets built through `vtkPoints`/`vtkCellArray`/`vtkDataArray` or the `numpy_support` zero-copy bridge; offscreen render-to-image for artifact export; `vtkFeatureEdges`/`vtkPolyDataSilhouette` line extraction and `vtkGL2PSExporter` vector output for the `scene/render#SCENE` V9 drawing egress; raw-vtk drop-down for filters pyvista does not wrap
