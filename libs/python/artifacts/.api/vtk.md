# [PY_ARTIFACTS_API_VTK]

`vtk` is the Visualization Toolkit demand-driven pipeline engine under the `scene` rail: dataset types (`vtkPolyData`, `vtkImageData`, `vtkUnstructuredGrid`) flow through `vtkAlgorithm` source/filter stages wired by `GetOutputPort`/`SetInputConnection`, render through the `vtkMapper` -> `vtkActor` -> `vtkRenderer` -> `vtkRenderWindow` stack, and serialize through reader/writer pairs. `pyvista` wraps this engine; a design page drops to raw `vtk` only for a stage `pyvista` does not expose. Every class lives under `vtkmodules.<Module>`, re-exported flat from `vtk`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `vtk`
- package: `vtk` (BSD-3-Clause)
- module: `vtk` (aggregates `vtkmodules.*`)
- rail: scene

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset and core data — `vtkmodules.vtkCommonDataModel`/`vtkCommonCore`

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [CAPABILITY]                                        |
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

[PUBLIC_TYPE_SCOPE]: pipeline algorithm bases — `vtkmodules.vtkCommonExecutionModel`

| [INDEX] | [SYMBOL]               | [CAPABILITY]                         |
| :-----: | :--------------------- | :----------------------------------- |
|  [01]   | `vtkAlgorithm`         | demand-driven source/filter contract |
|  [02]   | `vtkPolyDataAlgorithm` | algorithms emitting `vtkPolyData`    |
|  [03]   | `vtkDataSetAlgorithm`  | algorithms over generic datasets     |

[PUBLIC_TYPE_SCOPE]: rendering and interaction — `vtkmodules.vtkRenderingCore`

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                            |
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

[PUBLIC_TYPE_SCOPE]: sources and filters — `vtkmodules.vtkFiltersSources`/`vtkFiltersCore`/`vtkFiltersGeneral`

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]               | [CAPABILITY]                                                            |
| :-----: | :--------------------------- | :-------------------------- | :---------------------------------------------------------------------- |
|  [01]   | `vtkSphereSource`            | source                      | parametric sphere mesh                                                  |
|  [02]   | `vtkConeSource`              | source                      | cone mesh                                                               |
|  [03]   | `vtkCubeSource`              | source                      | box mesh                                                                |
|  [04]   | `vtkCylinderSource`          | source                      | cylinder mesh                                                           |
|  [05]   | `vtkPlaneSource`             | source                      | planar grid mesh                                                        |
|  [06]   | `vtkContourFilter`           | filter                      | iso-surface/iso-line extraction                                         |
|  [07]   | `vtkClipPolyData`            | filter                      | clip surface by implicit function                                       |
|  [08]   | `vtkCutter`                  | filter                      | cross-section by implicit function                                      |
|  [09]   | `vtkGlyph3D`                 | filter                      | place a glyph at each point                                             |
|  [10]   | `vtkThreshold`               | filter                      | extract cells by scalar range                                           |
|  [11]   | `vtkSmoothPolyDataFilter`    | filter                      | Laplacian surface smoothing                                             |
|  [12]   | `vtkDecimatePro`             | filter                      | triangle-count reduction                                                |
|  [13]   | `vtkPolyDataNormals`         | filter                      | compute point/cell normals                                              |
|  [14]   | `vtkTriangleFilter`          | filter                      | triangulate polygons/strips                                             |
|  [15]   | `vtkTransformPolyDataFilter` | filter                      | apply a `vtkTransform` to geometry                                      |
|  [16]   | `vtkFeatureEdges`            | filter (`vtkFiltersCore`)   | boundary/feature/non-manifold/manifold edge extraction to `vtkPolyData` |
|  [17]   | `vtkPolyDataSilhouette`      | filter (`vtkFiltersHybrid`) | view-dependent silhouette outline relative to a `vtkCamera`             |

[PUBLIC_TYPE_SCOPE]: I/O and geometry — `vtkmodules.vtkIOGeometry`/`vtkIOPLY`/`vtkIOXML`/`vtkCommonTransforms`

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                         |
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
|  [12]   | `vtkPNGWriter`           | writer        | write a captured framebuffer to PNG                  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: pipeline connection on `vtkAlgorithm` — every source/filter/reader shares it

| [INDEX] | [SURFACE]                         | [CAPABILITY]                              |
| :-----: | :-------------------------------- | :---------------------------------------- |
|  [01]   | `SetInputConnection(output_port)` | wire upstream output to this input        |
|  [02]   | `GetOutputPort()`                 | output port for downstream wiring         |
|  [03]   | `SetInputData(data_object)`       | feed an in-memory dataset directly        |
|  [04]   | `GetOutput()`                     | output dataset after `Update`             |
|  [05]   | `Update()`                        | run the pipeline up to this stage         |
|  [06]   | `UpdateInformation()`             | propagate metadata without computing data |

[ENTRYPOINT_SCOPE]: dataset and array construction — `vtkPolyData`/`vtkPoints`/`vtkCellArray`/`vtkFloatArray`

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------- | :------------------------------ |
|  [01]   | `vtkPoints.InsertNextPoint(x, y, z)`                        | append a coordinate             |
|  [02]   | `vtkCellArray.InsertNextCell(npts)` + `InsertCellPoint(id)` | append a cell and its point ids |
|  [03]   | `vtkPolyData.SetPoints(points)`                             | attach coordinates              |
|  [04]   | `vtkPolyData.SetPolys(cells)`                               | attach polygon connectivity     |
|  [05]   | `vtkPolyData.GetPointData()` / `GetCellData()`              | attribute-array sets            |
|  [06]   | `vtkFloatArray.SetName(name)` + `InsertNextValue(v)`        | named scalar attribute array    |
|  [07]   | `vtkFloatArray.SetNumberOfComponents(n)`                    | per-tuple component count       |

[ENTRYPOINT_SCOPE]: rendering setup — `vtkRenderer`/`vtkRenderWindow`/`vtkActor`/`vtkCamera`

| [INDEX] | [SURFACE]                                                    | [CAPABILITY]                        |
| :-----: | :----------------------------------------------------------- | :---------------------------------- |
|  [01]   | `vtkPolyDataMapper.SetInputConnection(port)`                 | bind geometry to a mapper           |
|  [02]   | `vtkPolyDataMapper.SetScalarRange(lo, hi)`                   | scalar-to-color domain              |
|  [03]   | `vtkActor.SetMapper(mapper)`                                 | bind a mapper to an actor           |
|  [04]   | `vtkActor.GetProperty()`                                     | access color/opacity/representation |
|  [05]   | `vtkRenderer.AddActor(actor)`                                | add a prop to the scene             |
|  [06]   | `vtkRenderer.SetBackground(r, g, b)`                         | scene background color              |
|  [07]   | `vtkRenderer.ResetCamera()`                                  | frame all actors                    |
|  [08]   | `vtkRenderWindow.AddRenderer(renderer)`                      | attach a renderer                   |
|  [09]   | `vtkRenderWindow.SetSize(w, h)` + `Render()`                 | size and draw                       |
|  [10]   | `vtkRenderWindow.SetOffScreenRendering(1)`                   | headless render target              |
|  [11]   | `vtkRenderWindowInteractor.SetRenderWindow(win)` + `Start()` | interactive event loop              |

[ENTRYPOINT_SCOPE]: file I/O — reader/writer pairs

| [INDEX] | [SURFACE]                                                                 | [CAPABILITY]                |
| :-----: | :------------------------------------------------------------------------ | :-------------------------- |
|  [01]   | `vtkSTLReader.SetFileName(path)` + `Update()`                             | load STL into `GetOutput()` |
|  [02]   | `vtkSTLWriter.SetInputConnection(port)` + `SetFileName(path)` + `Write()` | save STL (ASCII or binary)  |
|  [03]   | `vtkXMLPolyDataWriter.SetDataModeToBinary()`                              | binary XML output mode      |
|  [04]   | `vtkXMLPolyDataReader.SetFileName(path)` + `Update()`                     | load `.vtp`                 |
|  [05]   | `vtkSTLWriter.SetFileTypeToBinary()` / `SetFileTypeToASCII()`             | STL encoding mode           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `downstream.SetInputConnection(upstream.GetOutputPort())` wires a live pipeline that re-executes on upstream change, and nothing computes until a terminal `Update`/`Write`/`Render` pulls; `SetInputData` snapshots a static dataset instead.
- Construction is always `vtkClass()` with no positional args, configured through `Set*`/`Get*`.
- `SetOffScreenRendering(1)` renders headless without an interactor.

[STACKING]:
- `pyvista`(`.api/pyvista.md`): wraps the demand-driven pipeline, dataset hierarchy, and render stack numpy-native; `pyvista.wrap` adopts a `vtkPolyData`/`vtkImageData` and a `pyvista.PolyData` exposes its underlying `vtkPolyData`. Stay on the pyvista surface for plotting, camera, and export; drop to raw `vtk` only for a filter or mapper pyvista lacks, then re-wrap.
- `numpy`(`libs/python/.api/numpy.md`): `vtkmodules.util.numpy_support.numpy_to_vtk`/`vtk_to_numpy` is the zero-copy bridge — a NumPy buffer becomes point coordinates (`numpy_to_vtk` -> `vtkPoints.SetData`) or a named scalar field in one call, never a per-element `InsertNextValue` loop over a large dataset.
- `usd-core`(`.api/usd-core.md`): the wheel ships the `vtkIOExport` scene exporters (`vtkGLTFExporter`/`vtkOBJExporter`/`vtkVRMLExporter`/`vtkX3DExporter`) but no `vtkmodules.vtkIOUSD` — VTK USD I/O needs a source build against OpenUSD, so USD/USDZ authoring belongs to `usd-core` (`pxr`) across the numpy buffer seam.
- within-lib `scene` crease linework: `vtkFeatureEdges` extracts crease/outline edges — `SetBoundaryEdges`/`SetFeatureEdges`/`SetNonManifoldEdges`/`SetManifoldEdges` gate the edge classes, `SetFeatureAngle(deg)` sets the crease threshold — emitting line `vtkPolyData` that `vtk_to_numpy`/`pyvista.wrap` hands the drawing egress as extracted vectors.
- within-lib `scene` silhouette linework: `vtkPolyDataSilhouette` extracts the view-dependent occluding contour — `SetCamera` binds the viewpoint, `SetDirectionToCameraOrigin` selects camera-vs-origin, `SetEnableFeatureAngle`/`SetFeatureAngle` add creases, `SetBorderEdges` adds open borders — emitting the same line `vtkPolyData`.
- within-lib `scene` vector export: `vtkGL2PSExporter` (`vtkmodules.vtkIOExportGL2PS`, over `Plotter.render_window`) writes a rendered scene to real vector formats — `SetRenderWindow(win)`, `SetFileFormatToPS`/`ToEPS`/`ToPDF`/`ToSVG`/`ToTeX`, `SetFilePrefix(path)`, `SetSortToBSP`/`ToSimple` depth ordering, `SetCompress`, `Write` — the path where a raster framebuffer loses the linework.
- within-lib `scene` render egress: the headless PNG hop is `SetOffScreenRendering(1)` -> `Render()` -> `vtkWindowToImageFilter.SetInput(window)` + `Update()` -> `vtkPNGWriter.SetInputConnection(w2i.GetOutputPort())` + `Write()`, or `vtk_to_numpy` on the `vtkWindowToImageFilter` output hands the framebuffer to the `pillow` image owner as an RGB array.

[LOCAL_ADMISSION]:
- Call one terminal `Update`/`Write`/`Render`, never `Update` on every intermediate stage.
- Build geometry through `vtkPoints`/`vtkCellArray` attached with `SetPoints`/`SetPolys`, and carry scalars in a named `vtkFloatArray` on `GetPointData`/`GetCellData` or the `numpy_support` zero-copy bridge for large buffers.
- Import from `vtk` flat, or from a specific `vtkmodules.<Module>` for a narrower import surface.

[RAIL_LAW]:
- Package: `vtk`
- Owns: 3D scientific visualization, the demand-driven dataset pipeline, geometry sources/filters, file I/O, and the rendering/interaction stack — the native engine under pyvista
- Accept: pipeline wiring via `SetInputConnection`/`GetOutputPort`; datasets built through `vtkPoints`/`vtkCellArray`/`vtkDataArray` or the `numpy_support` bridge; offscreen render-to-image; `vtkFeatureEdges`/`vtkPolyDataSilhouette` line extraction and `vtkGL2PSExporter` vector output for the scene drawing egress; a raw-vtk drop-down for a filter pyvista lacks
- Reject: a per-element buffer loop where the `numpy_support` bridge is zero-copy; an `Update` on every intermediate stage; an interactor event loop in a headless export path; a re-derived reader/writer or `vtkRenderWindow`/`vtkRenderer` stack pyvista and vtk already own
