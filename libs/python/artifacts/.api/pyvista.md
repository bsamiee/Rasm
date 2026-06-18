# [PY_ARTIFACTS_API_PYVISTA]

`pyvista` supplies the VTK-backed 3D scientific visualization surface for the artifacts visuals rail: a `DataSet` mesh family (`PolyData`, `UnstructuredGrid`, `StructuredGrid`, `ImageData`), a `Plotter` render owner, a geometric-source factory, and the filter family (clip, slice, threshold, contour, extract-surface) that drive offscreen mesh rendering, scalar-field visualization, and glTF/VRML/OBJ scene export. The package owner composes the `DataSet` mesh family, `Plotter.add_mesh`/`screenshot`/`export_gltf`, and the filter family into the 3D scene owner; it never re-implements the VTK render engine pyvista already owns.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvista`
- package: `pyvista`
- import: `pyvista`
- owner: `artifacts`
- rail: visuals
- installed: `0.48.4` (on `vtk` `9.6.2`) reflected via `python -c "import pyvista"` on the gated `python_version<'3.13'` sub-3.13 companion floor; the cp315-core owner imports neither and crosses the runtime subprocess seam
- entry points: none (library only)
- capability: VTK-backed 3D mesh visualization — dataset mesh types, offscreen rendering, scalar-field coloring, mesh filters (clip/slice/threshold/contour/extract-surface/warp/glyph/streamlines), geometric sources, mesh decimate/smooth/boolean, screenshot rasterization, glTF/VRML/OBJ/HTML scene export

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset mesh family
- rail: visuals

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]   | [CAPABILITY]                                |
| :-----: | :----------------- | :--------------- | :------------------------------------------ |
|   [1]   | `DataSet`          | mesh base        | points, cells, scalar arrays, filter family |
|   [2]   | `PolyData`         | surface mesh     | points/faces surface with mesh ops          |
|   [3]   | `UnstructuredGrid` | volume mesh      | arbitrary-cell unstructured volume          |
|   [4]   | `StructuredGrid`   | structured grid  | curvilinear i/j/k grid                      |
|   [5]   | `RectilinearGrid`  | rectilinear grid | axis-aligned variable-spacing grid          |
|   [6]   | `ImageData`        | uniform grid     | regular volumetric image grid               |
|   [7]   | `MultiBlock`       | mesh container   | named collection of datasets                |
|   [8]   | `pyvista_ndarray`  | array view       | VTK-backed numpy array view                 |

[PUBLIC_TYPE_SCOPE]: render and geometric-source types
- rail: visuals

| [INDEX] | [SYMBOL]   | [PACKAGE_ROLE] | [CAPABILITY]                         |
| :-----: | :--------- | :------------- | :----------------------------------- |
|   [1]   | `Plotter`  | render owner   | add meshes, render offscreen, export |
|   [2]   | `Sphere`   | source         | parametric sphere mesh               |
|   [3]   | `Cube`     | source         | box mesh (also `Box`)                |
|   [4]   | `Cylinder` | source         | cylinder mesh (also `Cone`/`Tube`)   |
|   [5]   | `Plane`    | source         | plane mesh (also `Disc`/`Circle`)    |
|   [6]   | `Arrow`    | source         | arrow glyph mesh                     |
|   [7]   | `Line`     | source         | line/spline mesh (also `Spline`)     |
|   [8]   | `Text3D`   | source         | extruded 3D text mesh                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: read, wrap, and render
- rail: visuals

Render rows take a `DataSet` mesh and offscreen output policy; `read`/`wrap` build a dataset from file or array.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                       | [CAPABILITY]                         |
| :-----: | :-------------------- | :------------------------------------------------- | :----------------------------------- |
|   [1]   | `pyvista.read`        | `read(filename, force_ext=None, file_format=None)` | read a mesh from a file              |
|   [2]   | `pyvista.wrap`        | VTK object or numpy array                          | wrap a VTK/array source as a dataset |
|   [3]   | `pyvista.from_meshio` | meshio mesh                                        | import a meshio mesh                 |
|   [4]   | `Plotter`             | `Plotter(off_screen=...)`                          | construct an offscreen render owner  |
|   [5]   | `Plotter.add_mesh`    | `add_mesh(mesh, scalars=None, cmap=None, ...)`     | add a mesh with scalar coloring      |
|   [6]   | `Plotter.add_volume`  | volume mesh plus opacity/cmap                      | volume rendering                     |
|   [7]   | `Plotter.screenshot`  | `screenshot(filename=None, window_size=None, ...)` | rasterize the scene to PNG/array     |
|   [8]   | `Plotter.show`        | offscreen plus `screenshot=` policy                | render and optionally capture        |

[ENTRYPOINT_SCOPE]: filters and scene export
- rail: visuals

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                   | [CAPABILITY]                                               |
| :-----: | :------------------------ | :--------------------------------------------- | :--------------------------------------------------------- |
|   [1]   | `DataSet.clip`            | plane normal/origin policy                     | clip by a plane                                            |
|   [2]   | `DataSet.slice`           | normal/origin policy                           | cut a planar slice (also `slice_orthogonal`)               |
|   [3]   | `DataSet.threshold`       | scalar range                                   | extract cells in a value range                             |
|   [4]   | `DataSet.contour`         | isovalue list plus scalars                     | extract isosurfaces                                        |
|   [5]   | `DataSet.extract_surface` | no-arg surface extract                         | extract the boundary surface                               |
|   [6]   | `DataSet.warp_by_scalar`  | scalar plus factor                             | displace points by a scalar field                          |
|   [7]   | `Plotter.export_gltf`     | `export_gltf(filename, inline_data=True, ...)` | export the scene to glTF                                   |
|   [8]   | `Plotter.export_vrml`     | filename target                                | export the scene to VRML (also `export_obj`/`export_html`) |

## [4]-[IMPLEMENTATION_LAW]

[SCENE_3D]:
- import: `import pyvista as pv` at module scope inside the gated-band worker only; the cp315-core owner imports neither `pyvista` nor `vtk` and dispatches the whole render onto `anyio.to_process.run_sync`.
- offscreen axis: select the osmesa/EGL software-GL backend and construct `Plotter(off_screen=True)`; the entire render runs with zero display/browser/GPU, never an interactive window on the host-free path.
- mesh axis: the `DataSet` family (`PolyData`/`UnstructuredGrid`/`StructuredGrid`/`RectilinearGrid`/`ImageData`) is one mesh hierarchy; mesh kind is a subclass row, never a parallel per-source wrapper; `MultiBlock` collects named datasets.
- render axis: `Plotter.add_mesh(mesh, scalars=..., cmap=..., clim=...)` is the single mesh-add surface; scalar coloring is an add-mesh row, never a parallel colored-mesh type; `screenshot`/`show(screenshot=...)` rasterize offscreen.
- filter axis: `clip`/`slice`/`threshold`/`contour`/`extract_surface`/`warp_by_scalar`/`glyph`/`streamlines` are dataset-method filters returning a new dataset; filters compose, never mutate the source.
- export axis: `export_gltf`/`export_vrml`/`export_obj`/`export_html` emit scene files; the VTK glTF exporter handles `PolyData`, so surface extraction precedes glTF export.
- evidence: each render captures point count, cell count, scalar-array name, render window size, output format, and output byte length as a visuals receipt.
- boundary: pyvista owns 3D scientific scene render and export; publication 2D plotting routes to `matplotlib`; declarative charts route to `vl-convert-python`; raster output routes to `pillow`.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyvista`
- Owns: VTK-backed 3D mesh visualization, offscreen render, scalar-field coloring, mesh filters, geometric sources, mesh decimate/smooth/boolean, screenshot rasterization, glTF/VRML/OBJ/HTML export
- Accept: gated sub-3.13 worker render dispatched over the runtime subprocess seam feeding the scene and export-bundle owners
- Reject: wrapper-renames of `add_mesh`/`screenshot`; an interactive window on the host-free path; a per-source mesh wrapper where the `DataSet` family exists; a `pyvista`/`vtk` import in the cp315-core process
