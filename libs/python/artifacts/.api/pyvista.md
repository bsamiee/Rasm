# [PY_ARTIFACTS_API_PYVISTA]

`pyvista` wraps the VTK render engine into a numpy-native 3D scientific-visualization surface for the `scene` rail: a `DataSet` mesh hierarchy, an offscreen `Plotter` render owner, the `read`/`wrap` ingest pair, geometric sources, and the dataset filter family for clipping, slicing, contouring, mesh repair, and CSG. It renders host-free through `Plotter(off_screen=True)` over a software-GL backend and never re-implements the demand-driven VTK pipeline or the reader/writer pairs `vtk` owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvista`
- package: `pyvista` (MIT, over `vtk` BSD-3-Clause)
- module: `pyvista` (import as `pv`)
- rail: scene
- depends: `trame` gates `Plotter.export_html`; the offscreen render, screenshot, and glTF/VRML/OBJ paths need none

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset mesh family — mesh kind is a `DataSet` subclass row

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]    | [CAPABILITY]                                                               |
| :-----: | :----------------- | :--------------- | :------------------------------------------------------------------------- |
|  [01]   | `DataSet`          | mesh base        | points/cells/scalar arrays; filter-method base                             |
|  [02]   | `PolyData`         | surface mesh     | points/faces surface; mesh repair + `boolean_*` CSG; glTF/USD source       |
|  [03]   | `UnstructuredGrid` | volume mesh      | arbitrary-cell volume; `clip`/`clip_box`/`threshold` return type           |
|  [04]   | `StructuredGrid`   | structured grid  | curvilinear i/j/k grid                                                     |
|  [05]   | `RectilinearGrid`  | rectilinear grid | axis-aligned variable-spacing grid                                         |
|  [06]   | `ImageData`        | uniform grid     | regular volumetric grid; `add_volume` target                               |
|  [07]   | `MultiBlock`       | mesh container   | named dataset collection; filters fan over blocks; `combine`/`save`        |
|  [08]   | `pyvista_ndarray`  | array view       | zero-copy VTK numpy view over `.points`/`.point_normals`; writes propagate |

[PUBLIC_TYPE_SCOPE]: render owner and geometric-source family

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :--------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `Plotter`  | render owner  | add meshes/volumes/points, render offscreen, tune, capture, export |
|  [02]   | `Sphere`   | source        | parametric sphere mesh (also `Icosphere`)                          |
|  [03]   | `Cube`     | source        | box mesh (also `Box`/`Pyramid`)                                    |
|  [04]   | `Cylinder` | source        | cylinder mesh (also `Cone`/`Tube`)                                 |
|  [05]   | `Plane`    | source        | plane mesh (also `Disc`/`Circle`/`Polygon`/`Triangle`)             |
|  [06]   | `Arrow`    | source        | arrow glyph mesh (default `glyph` geometry)                        |
|  [07]   | `Line`     | source        | line/spline mesh (also `Spline`/`lines_from_points`)               |
|  [08]   | `Text3D`   | source        | extruded 3D text mesh                                              |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest, render, capture

`wrap` is the zero-copy entry for a native VTK, numpy, `trimesh`, or `meshio` value; `read` is the file path; `from_meshio` owns the meshio-specific import. `screenshot(return_img=True)` returns an rgb(a) numpy raster with no PNG round-trip.

| [INDEX] | [SURFACE]                                                    | [CAPABILITY]                                                      |
| :-----: | :----------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `pyvista.wrap(dataset, validate=None) -> DataSet`            | zero-copy admit; VTK/`numpy`/`trimesh`/`meshio`; no-op if wrapped |
|  [02]   | `pyvista.read(filename, force_ext=None, file_format=None)`   | read a mesh from a file (reader chosen by extension)              |
|  [03]   | `pyvista.from_meshio(mesh) -> UnstructuredGrid`              | the meshio import row `wrap` dispatches to                        |
|  [04]   | `Plotter(off_screen=None, window_size=None)`                 | construct an offscreen render owner (`off_screen=True`)           |
|  [05]   | `Plotter.add_mesh(mesh, scalars=, cmap=, pbr=) -> Actor`     | add a mesh with scalar coloring + PBR/lighting material band      |
|  [06]   | `Plotter.add_volume(volume, scalars=, opacity=, blending=)`  | GPU/smart volume rendering of `ImageData`                         |
|  [07]   | `Plotter.add_points(points, style='points', **kwargs)`       | point-cloud actor; kwargs forward to `add_mesh`                   |
|  [08]   | `Plotter.screenshot(filename=None, return_img=True, scale=)` | rasterize; `return_img=True` -> rgb(a) array, `filename=` -> PNG  |
|  [09]   | `Plotter.camera_position` (property)                         | `[position, focal_point, view_up]` / preset / framed triple       |
|  [10]   | `Plotter.camera` (property) / `Camera.zoom(value)`           | live `Camera`; `zoom` scales view angle or parallel scale         |
|  [11]   | `Plotter.show(screenshot=False, cpos=None)`                  | render and optionally capture/return (`interactive=` host-only)   |

[ENTRYPOINT_SCOPE]: publication-quality render control, scene import, raw-vtk bridge

`set_background`, the `enable_*` family, `add_axes`/`add_scalar_bar`/`add_text`, and the standard-view family tune the offscreen render. `import_gltf`/`import_obj`/`import_vrml` keep a re-render/re-export round-trip in-process. `render_window` exposes the raw `vtkRenderWindow` the source-build-gated `vtkUSDExporter` reads; the wheel-available USD path is the numpy buffer seam, not the render window.

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                                                    |
| :-----: | :------------------------------------------------------ | :-------------------------------------------------------------- |
|  [01]   | `Plotter.enable_anti_aliasing(aa_type='ssaa')`          | `'ssaa'`/`'msaa'`/`'fxaa'` (`multi_samples` for `'msaa'`)       |
|  [02]   | `Plotter.enable_ssao(radius=, bias=, kernel_size=)`     | screen-space ambient occlusion                                  |
|  [03]   | `Plotter` quality-toggle family                         | depth-peeling/EDL/shadows/parallel-projection/hidden-line       |
|  [04]   | `Plotter` scene-annotation family                       | background, axes widget, scalar legend, text                    |
|  [05]   | `Plotter.import_gltf` / `import_obj` / `import_vrml`    | load a glTF/OBJ/VRML scene for a re-render/re-export round-trip |
|  [06]   | `Plotter.render_window -> vtkRenderWindow`              | the raw window the source-gated `vtkUSDExporter` reads          |
|  [07]   | `Plotter.close(render=False)`                           | deterministic render-window/GL-context teardown                 |
|  [08]   | `Plotter` standard-view family                          | plan/elevation/section/axonometric preset viewpoints            |
|  [09]   | `Plotter.add_silhouette(mesh, feature_angle=) -> Actor` | view-dependent silhouette linework (`vtkPolyDataSilhouette`)    |
|  [10]   | `pyvista.Light(color=, intensity=, light_type=)`        | scene light; `set_direction_angle(elev, azim)` aims the vector  |
|  [11]   | `Plotter.add_light(light)` / `Plotter(lighting=)`       | add a light; `'light kit'`/`'three lights'`/`'none'` rig        |

- [03]: `enable_depth_peeling` `enable_eye_dome_lighting` `enable_shadows` `enable_parallel_projection` `enable_hidden_line_removal`.
- [04]: `set_background` `add_axes` `add_scalar_bar` `add_text`.
- [08]: `view_xy` `view_xz` `view_yz` `view_isometric` `view_vector` — one call sets camera position and view-up parallel to a principal plane; `negative=True` flips to the opposite face.

[ENTRYPOINT_SCOPE]: dataset filters, the numpy mesh seam, mesh repair, write

Filter methods return a NEW dataset, mutating the source only under `inplace=True`; `PolyData` adds mesh repair and CSG. Accessors `.points`/`.regular_faces`/`.point_normals` are the numpy buffer seam the `usd-core` author path reads.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                                              |
| :-----: | :---------------------------------------------------------------- | :-------------------------------------------------------- |
|  [01]   | `DataSet.clip(normal='x', invert=True, value=0.0)`                | clip by a plane                                           |
|  [02]   | `DataSet.clip_box(bounds=None, invert=True) -> UnstructuredGrid`  | clip by an axis-aligned box                               |
|  [03]   | `DataSet.clip_scalar(scalars=, both=False) -> PolyData \| tuple`  | clip by scalar; `both=True` returns both complements      |
|  [04]   | `DataSet.slice(normal='x', contour=False) -> PolyData`            | cut a planar slice                                        |
|  [05]   | `DataSet.slice_orthogonal(x=None, y=None, z=None) -> MultiBlock`  | three cartesian-plane slices                              |
|  [06]   | `DataSet.threshold(value=None, scalars=None) -> UnstructuredGrid` | extract cells in a value range                            |
|  [07]   | `DataSet.contour(isosurfaces=10, method='contour') -> PolyData`   | isosurfaces (`contour`/`marching_cubes`/`flying_edges`)   |
|  [08]   | `DataSet.warp_by_scalar(scalars=None, factor=1.0)`                | displace points by a scalar (also `warp_by_vector`)       |
|  [09]   | `DataSet.glyph(orient=True, scale=True, factor=1.0) -> PolyData`  | oriented/scaled glyph (default `Arrow`) per point         |
|  [10]   | `DataSet.streamlines(vectors=None) -> PolyData`                   | integrate a vector field into streamline polylines        |
|  [11]   | `DataObjectFilters.extract_surface() -> PolyData`                 | boundary surface as `PolyData` (precedes glTF/USD export) |
|  [12]   | `PolyDataFilters.triangulate(pass_verts=False) -> PolyData`       | all-triangle mesh so `regular_faces` is uniform `(M,3)`   |
|  [13]   | `DataSet.sample` / `cell_data_to_point_data`                      | transfer/resample fields; cell<->point attribute transfer |
|  [14]   | `PolyData` mesh-repair family                                     | reduce/smooth/refine/repair a surface                     |
|  [15]   | `PolyData` CSG boolean family                                     | watertight-surface CSG (also `+`/`-` operators)           |
|  [16]   | `PolyData.points` / `.regular_faces` / `.point_normals`           | numpy mesh buffers the `usd-core` author seam reads       |
|  [17]   | `PolyDataFilters.extract_feature_edges(feature_angle=30.0)`       | crease/boundary/non-manifold edges as line `PolyData`     |

- [13]: `sample` `cell_data_to_point_data` `point_data_to_cell_data`.
- [14]: `decimate(target_reduction)` `smooth(n_iter=20)` `subdivide(nsub)` `fill_holes(hole_size)` `clean`.
- [15]: `boolean_union` `boolean_difference` `boolean_intersection`; `PolyData.is_manifold` gates watertight operands before a boolean.
- [16]: `.regular_faces` raises `ValueError` on irregular faces; `.point_normals` auto-runs `compute_normals`.

[ENTRYPOINT_SCOPE]: scene-file export, mesh-file write, multiblock merge

| [INDEX] | [SURFACE]                                              | [CAPABILITY]                                                         |
| :-----: | :----------------------------------------------------- | :------------------------------------------------------------------- |
|  [01]   | `Plotter.export_gltf(filename, inline_data=True)`      | export the scene to glTF (surface `PolyData` only)                   |
|  [02]   | `Plotter.export_vrml(filename)`                        | export the scene to VRML                                             |
|  [03]   | `Plotter.export_obj(filename)`                         | export to OBJ (`.obj` + `.mtl`)                                      |
|  [04]   | `Plotter.export_html(filename) -> io.StringIO \| None` | interactive HTML scene (requires `trame`; `None` -> a `StringIO`)    |
|  [05]   | `PolyData.save(filename, binary=True, texture=None)`   | write a single mesh file (`.vtp`/`.ply`/`.stl`/`.obj`)               |
|  [06]   | `MultiBlock.combine() -> UnstructuredGrid` / `save()`  | merge a block collection, or persist the whole `.vtm`                |
|  [07]   | `Plotter.save_graphic(filename, raster=, painter=)`    | vector figure (`.svg`/`.eps`/`.ps`/`.pdf`/`.tex`)                    |
|  [08]   | `PolyData.from_regular_faces(points, faces)`           | surface from `(N,3)` points + `(M,k)` faces, `regular_faces` inverse |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- offscreen host-free: `Plotter(off_screen=True)` over a headless software-GL backend renders without an interactive window; `screenshot(return_img=True)`/`screenshot(filename=)` rasterize the framebuffer. Bracket every plotter in `try`/`finally` `plotter.close()` for deterministic render-window and GL-context teardown.
- worker-local: `pyvista` and `vtk` import only inside the offloaded native worker process; the runtime producer imports neither.
- one mesh hierarchy: mesh kind is a `DataSet` subclass, never a parallel per-source wrapper; `MultiBlock` collects named datasets and fans filters over its blocks.
- filters return a new dataset, mutating only under `inplace=True`, so a slice-then-threshold-then-glyph visualization composes as a chain over one dataset, never a per-filter mesh type. Return shifts: `clip_box`/`threshold` -> `UnstructuredGrid`, `slice_orthogonal` -> `MultiBlock`, `clip_scalar`/`slice`/`contour`/`extract_surface` -> `PolyData`; `clip_scalar` selects complements with `both=`, not `return_clipped=`.
- numpy geometry seam: `.points` `(N,3)` coordinates, `.regular_faces` `(M,k)` connectivity (valid `(M,3)` after `triangulate`, `ValueError` on irregular faces), and computed `.point_normals` are the buffers the USD author path consumes.

[STACKING]:
- `vtk`(`.api/vtk.md`): `pyvista` wraps the demand-driven pipeline, dataset hierarchy, and render stack into a numpy-native API; it drops to raw `vtk` only through `Plotter.render_window` for the source-gated `vtkUSDExporter`, never re-deriving a `vtkRenderWindow`/`vtkRenderer` pipeline `pyvista` already owns.
- `numpy`(`libs/python/.api/numpy.md`): `wrap` admits a vertex/scalar buffer zero-copy, `screenshot(return_img=True)` returns an rgb(a) raster, and `.points`/`.regular_faces`/`.point_normals` are numpy views, so one buffer becomes mesh points, a scalar field, a captured frame, or a USD source in one hop.
- `usd-core`(`.api/usd-core.md`): triangulated `.points`/`.regular_faces`/`.point_normals` cross to `Vt.<Type>Array.FromNumpy`; the `vtkUSDExporter` over `Plotter.render_window` stays source-build-gated.
- `anyio`(`libs/python/.api/anyio.md`): every offloaded arm crosses as a `KernelTrait.HOSTILE` kernel on the warm process pool, keeping the sub-3.15 `pyvista`/`vtk`/`pxr` imports worker-local.
- within-lib `scene`: the `scene/render` `Scene3d` owner composes `wrap`, `add_mesh`/`add_volume`/`add_points`, the filter family, the `enable_*` render controls, and `export_*`; `screenshot(return_img=True)` rasters pass to `media` `VideoFrame.from_ndarray(format="rgb24")` with no PNG intermediary, and `scene/stage` reads the numpy buffer seam.

[LOCAL_ADMISSION]:
- Construct `Plotter(off_screen=True)` and rasterize with `screenshot`; never spin an interactive window in the headless path.
- Stay on the pyvista surface for meshes, filters, camera, and export; drop to raw `vtk` only for `Plotter.render_window`.
- Import `pyvista`/`vtk` inside the offloaded worker; the runtime producer imports neither native package.
- Chain filters as one composed sequence over a dataset; carry large mesh/scalar buffers through the numpy seam, never a per-element loop.

[RAIL_LAW]:
- Package: `pyvista`
- Owns: VTK-backed 3D mesh visualization, `read`/`wrap` ingest, headless render, scalar/PBR styling, filters, mesh repair, CSG, geometric sources, render controls, screenshots, numpy mesh views, mesh-file write, and scene export
- Accept: qualified-name process offload; `wrap` for native in-memory adapters; composed filter chains; screenshot rasters and glTF/VRML/OBJ/HTML export; the USD numpy-buffer seam and source-built `render_window`
- Reject: a re-derived `vtkRenderWindow`/`vtkRenderer` pipeline, a per-filter mesh wrapper type, an interactive event loop in a headless path, and a per-element buffer copy where the numpy seam is zero-copy
