# [PY_ARTIFACTS_API_PYVISTA]

`pyvista` supplies the VTK-backed 3D scientific-visualization surface for the `scene` rail: a `DataSet` mesh family (`PolyData`/`UnstructuredGrid`/`StructuredGrid`/`RectilinearGrid`/`ImageData`/`MultiBlock`), a `Plotter` offscreen render owner, the `read`/`wrap` ingest pair, a geometric-source factory, and the dataset-method filter family (clip/clip_box/clip_scalar/slice/slice_orthogonal/threshold/contour/extract_surface/triangulate/warp_by_scalar/glyph/streamlines/sample/decimate/smooth/subdivide/fill_holes/clean/boolean) that drives offscreen mesh rendering, scalar-field visualization, mesh repair, and glTF/VRML/OBJ/HTML scene export. Its package owner composes `read`, the `DataSet` mesh family, `Plotter.add_mesh`/`add_volume`/`add_points`/`screenshot`/`camera_position`/the `enable_*` publication-quality family/`export_gltf`, and the filter family into the `scene/render#SCENE` `Scene3d` owner over the closed `SceneOp` (`Image`/`Export`/`Frames`/`Ingest`/`Compose`); the `scene/export#EXPORT` `SceneTarget` arms reach the `Plotter.export_*` exporters and the `Plotter.render_window` accessor, and the `surface_arrays` seam hands the triangulated surface's `.points`/`.regular_faces`/`.point_normals` numpy buffers to `scene/stage#STAGE` (`usd-core`). It removes any interactive window because `Plotter(off_screen=True)` over a software-GL backend renders host-free, and it never re-implements the VTK render engine, the demand-driven pipeline (`vtk` owns it — `libs/python/artifacts/.api/vtk.md`), or the file reader/writer pairs pyvista already wraps from `vtk`.

[ARTIFACT_STATUS]: docs/source-verified at `0.48.4`, NOT reflection-verified. pyvista is admitted under the `; python_version<'3.15'` cp-gate (`Directory`/`pyproject.toml`), ships no cp315 wheel, and is absent on this interpreter, so `assay api resolve pyvista` returns `unsupported` (no PYDIST source); every member, signature, and return type below is settled against the official PyVista `0.48.4` API documentation and the `scene/{render,export,stage}` design pages that compose it. Re-verify by reflection (`uv run --frozen python -m tools.assay api resolve pyvista`) once a cp315 wheel lands and the gate is removed.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvista`
- package: `pyvista`
- import: `pyvista` (conventionally `import pyvista as pv`)
- owner: `artifacts`
- rail: scene
- version: `0.48.4`
- marker: `; python_version<'3.15'` — cp-gated, no cp315 wheel; resolver (`uv lock`) is the ground truth, gate removed when a cp315 wheel lands
- target: gated sub-3.15 native-VTK worker — imported after `lane.offload(Kernel.of((WORKER_MODULE, "<kernel>"), KernelTrait.HOSTILE), ...)` crosses the process seam; co-resolves with `vtk` and `usd-core` on that worker
- license: MIT (pyvista) over `vtk` BSD-3-Clause; permissive, no copyleft gate
- entry points: none (library only)
- requires-for-html: `trame` (`Plotter.export_html` needs `trame` installed; the offscreen render/screenshot/glTF/VRML/OBJ paths do not)
- capability: VTK-backed 3D mesh visualization — the `DataSet` mesh hierarchy, `read`/`wrap` ingest (including zero-copy `trimesh.Trimesh`/`meshio.Mesh` wrap), offscreen software-GL rendering, scalar-field coloring with PBR/lighting, mesh filters (clip/clip_box/clip_scalar/slice/slice_orthogonal/threshold/contour/extract_surface/triangulate/warp/glyph/streamlines/sample/cell-point transfer), mesh repair (decimate/smooth/subdivide/fill_holes/clean) and CSG booleans, geometric sources, screenshot rasterization, mesh-file write (`save`), and glTF/VRML/OBJ/HTML scene export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset mesh family
- rail: scene — wraps `vtkmodules.vtkCommonDataModel` (`libs/python/artifacts/.api/vtk.md`); mesh kind is a subclass row, never a parallel per-source wrapper

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]   | [CAPABILITY]                                                                                  |
| :-----: | :----------------- | :--------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `DataSet`          | mesh base        | points/cells/scalar arrays + `DataObjectFilters`/`DataSetFilters`; `FieldFilter.apply` target |
|  [02]   | `PolyData`         | surface mesh     | points/faces surface; mesh repair + `boolean_*` CSG; glTF-export/USD-author surface           |
|  [03]   | `UnstructuredGrid` | volume mesh      | arbitrary-cell volume; `clip`/`clip_box`/`threshold` return this; the `from_meshio` target    |
|  [04]   | `StructuredGrid`   | structured grid  | curvilinear i/j/k grid                                                                        |
|  [05]   | `RectilinearGrid`  | rectilinear grid | axis-aligned variable-spacing grid                                                            |
|  [06]   | `ImageData`        | uniform grid     | regular volumetric image grid; the `Style.volume` `add_volume` target                         |
|  [07]   | `MultiBlock`       | mesh container   | named dataset collection; filters fan over blocks; `combine`/`save` merge or persist          |
|  [08]   | `pyvista_ndarray`  | array view       | zero-copy VTK numpy view over `.points`/`.point_normals`/point-cell; writes propagate         |

[PUBLIC_TYPE_SCOPE]: render owner and geometric-source family
- rail: scene — `Plotter` wraps the `vtkRenderer`/`vtkRenderWindow` stack; the sources wrap `vtkmodules.vtkFiltersSources`

| [INDEX] | [SYMBOL]   | [PACKAGE_ROLE] | [CAPABILITY]                                                                                        |
| :-----: | :--------- | :------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `Plotter`  | render owner   | add meshes/volumes/points; render offscreen; tune, capture, export; one render capsule per modality |
|  [02]   | `Sphere`   | source         | parametric sphere mesh (also `Icosphere`)                                                           |
|  [03]   | `Cube`     | source         | box mesh (also `Box`/`Pyramid`)                                                                     |
|  [04]   | `Cylinder` | source         | cylinder mesh (also `Cone`/`Tube`)                                                                  |
|  [05]   | `Plane`    | source         | plane mesh (also `Disc`/`Circle`/`Polygon`/`Triangle`)                                              |
|  [06]   | `Arrow`    | source         | arrow glyph mesh (the default `glyph` geometry)                                                     |
|  [07]   | `Line`     | source         | line/spline mesh (also `Spline`/`lines_from_points`)                                                |
|  [08]   | `Text3D`   | source         | extruded 3D text mesh                                                                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest, render, capture
- rail: scene — `read`/`wrap` admit ONCE at the worker; `Plotter` renders offscreen; `screenshot` rasterizes

`wrap` is the canonical zero-copy entry for a native VTK, numpy, `trimesh`, or `meshio` value; `scene/render_worker#WORKER` `_hydrate` constructs each admitted `SceneGrid` case directly, and `read` is the file path. `Plotter` keyword-only kwargs and the `screenshot(return_img=True)` raster array feed `media/video#MEDIA` through `VideoFrame.from_ndarray` with zero PNG round-trip.

| [INDEX] | [SURFACE]                                                       | [CAPABILITY]                                                      |
| :-----: | :-------------------------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | `pyvista.wrap(dataset, validate=None) -> DataSet`               | zero-copy admit; VTK/`numpy`/`trimesh`/`meshio`; no-op if wrapped |
|  [02]   | `pyvista.read(filename, force_ext=None, file_format=None, ...)` | read a mesh from a file (reader chosen by extension)              |
|  [03]   | `pyvista.from_meshio(mesh) -> UnstructuredGrid`                 | the meshio-specific import row `wrap` dispatches to               |
|  [04]   | `Plotter(off_screen=None, window_size=None, ...)`               | construct an offscreen render owner (`off_screen=True`)           |
|  [05]   | `Plotter.add_mesh(mesh, scalars=, cmap=, pbr=, ...) -> Actor`   | add a mesh with scalar coloring + PBR/lighting material band      |
|  [06]   | `Plotter.add_volume(volume, scalars=, opacity=, ...)`           | GPU/smart volume rendering of `ImageData`                         |
|  [07]   | `Plotter.add_points(points, style='points', **kwargs)`          | point-cloud actor; kwargs forward to `add_mesh`                   |
|  [08]   | `Plotter.screenshot(filename=None, return_img=True, ...)`       | rasterize; `return_img=True` -> rgb(a) array; `filename=` -> PNG  |
|  [09]   | `Plotter.camera_position` (property)                            | `[position, focal_point, view_up]` / preset / framed triple       |
|  [10]   | `Plotter.camera` (property) / `Camera.zoom(value)`              | live `Camera`; `zoom` scales view angle or parallel scale         |
|  [11]   | `Plotter.show(screenshot=False, cpos=None, ...)`                | render and optionally capture/return (`interactive=` host-only)   |

- [06]: `add_volume(volume, scalars=, opacity=, blending=, mapper=, shade=, cmap=, clim=)` — the `Style.volume` transfer band.
- [07]: `point_size=`/`render_points_as_spheres=` ride the forwarded kwargs into `add_mesh`.
- [08]: `screenshot(filename=None, transparent_background=None, return_img=True, window_size=None, scale=None)`.

[ENTRYPOINT_SCOPE]: publication-quality render control, scene import, raw-vtk bridge
- rail: scene — the `scene/render#SCENE` `RenderFeature` `_FEATURE` enable-table and the `scene/stage#STAGE` USD bridge

`set_background`/the `enable_*` family/`add_axes`/`add_scalar_bar`/`add_text` are the `RenderSpec.viewed` projection targets — eight parallel quality bools collapse into one `RenderFeature` `frozenset` folded over this table. `import_gltf`/`import_obj`/`import_vrml` keep a render-tune-re-export round-trip in-process. `render_window` is the `vtkRenderWindow` the `scene/stage#STAGE` source-build-gated `vtkUSDExporter` reads (the standard `vtk` wheel ships no `vtkmodules.vtkIOUSD`); the wheel-available USD path is the `surface_arrays` numpy seam, not the render window.

| [INDEX] | [SURFACE]                                                | [CAPABILITY]                                                                 |
| :-----: | :------------------------------------------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Plotter.enable_anti_aliasing(aa_type='ssaa', ...)`      | `'ssaa'`/`'msaa'`/`'fxaa'` `AntiAlias` source (`multi_samples` for `'msaa'`) |
|  [02]   | `Plotter.enable_ssao(radius=, bias=, kernel_size=, ...)` | screen-space ambient occlusion (`RenderFeature.SSAO`)                        |
|  [03]   | `Plotter` quality-toggle family                          | depth-peeling/EDL/shadows/parallel-projection/hidden-line `_FEATURE` rows    |
|  [04]   | `Plotter` scene-annotation family                        | background, axes widget, scalar legend, text — `RenderSpec.viewed`           |
|  [05]   | `Plotter.import_gltf` / `import_obj` / `import_vrml`     | load a glTF/OBJ/VRML scene for a re-render/re-export round-trip              |
|  [06]   | `Plotter.render_window -> vtkRenderWindow`               | the raw `vtkRenderWindow` the source-build-gated `vtkUSDExporter` reads      |
|  [07]   | `Plotter.close(render=False)`                            | deterministic VTK render-window/GL-context teardown (`try`/`finally`)        |
|  [08]   | `Plotter` standard-view family                           | the plan/elevation/section/axonometric preset viewpoints (`StandardView`)    |
|  [09]   | `Plotter.add_silhouette(mesh, ...) -> Actor`             | view-dependent silhouette linework (wraps `vtkPolyDataSilhouette`)           |
|  [10]   | `pyvista.Light(color=, intensity=, light_type=)`         | scene light; `Light.set_direction_angle(elev, azim)` aims the solar vector   |
|  [11]   | `Plotter.add_light(light)` / `Plotter(lighting=)`        | add a light; `'light kit'`/`'three lights'`/`'none'` rig (`LightPreset`)     |

- [03]: `enable_depth_peeling(number_of_peels=4, occlusion_ratio=0.0)` / `enable_eye_dome_lighting()` / `enable_shadows()` / `enable_parallel_projection()` / `enable_hidden_line_removal(all_renderers=True)` — order-independent transparency, point-cloud EDL, shadows, parallel projection, hidden-line removal.
- [04]: `set_background(color, top=None)` / `add_axes(...)` / `add_scalar_bar(title=None, ...)` / `add_text(text, position='upper_left', font_size=18)` — background gradient, orientation axes widget, scalar legend, annotation (`RenderFeature.AXES`/`SCALAR_BAR`).
- [08]: `view_xy(negative=False, render=True, bounds=None)` / `view_xz(...)` / `view_yz(...)` / `view_isometric(negative=False, render=True, bounds=None)` / `view_vector(vector, viewup=None)` — one call sets camera position + view-up parallel to a principal plane; `negative=True` flips to the opposite face.
- [09]: `add_silhouette(mesh, color=None, line_width=None, opacity=None, feature_angle=None, decimate=None)`.

[ENTRYPOINT_SCOPE]: dataset filters, the numpy mesh seam, mesh repair, write
- rail: scene — `DataObjectFilters`/`DataSetFilters` methods return a NEW dataset (no mutation unless `inplace=True`); `PolyData` adds repair/CSG; the `.points`/`.regular_faces`/`.point_normals` accessors are the `usd-core` author seam

One filter family is the `scene/render#SCENE` `FieldFilter` closed-payload `tagged_union` — a slice-then-threshold-then-glyph visualization is a `RenderSpec.filters` tuple folded by `functools.reduce`, never a parallel filtered-mesh type. Numpy mesh accessors are the bridge a `usd-core` author path consumes: the same buffers cross to `Vt.<Type>Array.FromNumpy` (`libs/python/artifacts/.api/usd-core.md`).

| [INDEX] | [SURFACE]                                                              | [CAPABILITY]                                                   |
| :-----: | :--------------------------------------------------------------------- | :------------------------------------------------------------- |
|  [01]   | `DataSet.clip(normal='x', invert=True, value=0.0, ...)`                | clip by a plane (`FieldFilter.Clip`)                           |
|  [02]   | `DataSet.clip_box(bounds=None, invert=True, ...) -> UnstructuredGrid`  | clip by an axis-aligned box (`FieldFilter.ClipBox`)            |
|  [03]   | `DataSet.clip_scalar(scalars=, both=False, ...) -> PolyData \| tuple`  | clip by scalar; `both=True` = both complements (`ClipScalar`)  |
|  [04]   | `DataSet.slice(normal='x', contour=False, ...) -> PolyData`            | cut a planar slice (`FieldFilter.Slice`)                       |
|  [05]   | `DataSet.slice_orthogonal(x=None, y=None, z=None, ...) -> MultiBlock`  | three cartesian-plane slices (`SliceOrthogonal`)               |
|  [06]   | `DataSet.threshold(value=None, scalars=None, ...) -> UnstructuredGrid` | extract cells in a value range (`FieldFilter.Threshold`)       |
|  [07]   | `DataSet.contour(isosurfaces=10, method='contour', ...) -> PolyData`   | isosurfaces (`contour`/`marching_cubes`/`flying_edges`)        |
|  [08]   | `DataSet.warp_by_scalar(scalars=None, factor=1.0, ...)`                | displace points by a scalar field (also `warp_by_vector`)      |
|  [09]   | `DataSet.glyph(orient=True, scale=True, factor=1.0, ...) -> PolyData`  | oriented/scaled glyph (default `Arrow`) per point              |
|  [10]   | `DataSet.streamlines(vectors=None, ...) -> PolyData`                   | integrate a vector field into streamline polylines             |
|  [11]   | `DataObjectFilters.extract_surface(...) -> PolyData`                   | boundary surface as `PolyData` (precedes glTF/USD export)      |
|  [12]   | `PolyDataFilters.triangulate(pass_verts=False, ...) -> PolyData`       | all-triangle mesh so `regular_faces` is uniform `(M,3)`        |
|  [13]   | `DataSet.sample` / `cell_data_to_point_data` / ...                     | transfer/resample fields; cell<->point attribute transfer      |
|  [14]   | `PolyData` mesh-repair family                                          | reduce/smooth/refine/repair a surface (`FieldFilter.Decimate`) |
|  [15]   | `PolyData` CSG boolean family                                          | watertight-surface CSG (also `+`/`-` operators)                |
|  [16]   | `PolyData.points` / `.regular_faces` / `.point_normals`                | numpy mesh buffers the `surface_arrays` seam reads             |
|  [17]   | `PolyDataFilters.extract_feature_edges(feature_angle=30.0, ...)`       | crease/boundary/non-manifold edges as line `PolyData`          |

- [13]: `sample(target, tolerance=None, pass_cell_data=True, pass_point_data=True, ...)` / `cell_data_to_point_data()` / `point_data_to_cell_data()`.
- [14]: `decimate(target_reduction, volume_preservation=False, attribute_error=False, inplace=False)` / `smooth(n_iter=20, ...)` / `subdivide(nsub, subfilter='linear')` / `fill_holes(hole_size)` / `clean(...)`.
- [15]: `boolean_union(other_mesh, tolerance=1e-05)` / `boolean_difference(...)` / `boolean_intersection(...)`; `PolyData.is_manifold` (property) gates watertight operands before a boolean.
- [16]: `.points -> pyvista_ndarray (N,3)` / `.regular_faces -> numpy.ndarray (M,k)` (raises `ValueError` if irregular) / `.point_normals -> pyvista_ndarray (N,3)` (auto-`compute_normals`) — fed to `usd-core` `Vt.<Type>Array.FromNumpy`.

[ENTRYPOINT_SCOPE]: scene-file export, mesh-file write, multiblock merge
- rail: scene — `Plotter.export_*` emit a rendered-scene file; `save`/`MultiBlock.combine` are the mesh-file/merge rows

`Plotter.export_*` and `save_graphic` are `ExportRow.plotter` write closures in the `scene/export#EXPORT` `ROW` correspondence. `Prepass.SURFACE` folds `FieldFilter.Surface()` into the `GLTF` row before its VTK exporter runs.

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                                                           |
| :-----: | :---------------------------------------------------------- | :--------------------------------------------------------------------- |
|  [01]   | `Plotter.export_gltf(filename, ...)`                        | export the scene to glTF (surface `PolyData` only)                     |
|  [02]   | `Plotter.export_vrml(filename)`                             | export the scene to VRML (`SceneTarget.VRML`)                          |
|  [03]   | `Plotter.export_obj(filename)`                              | export to OBJ — `.obj`+`.mtl`, captured by the `stream_zip` bundle     |
|  [04]   | `Plotter.export_html(filename) -> io.StringIO \| None`      | interactive HTML scene (requires `trame`; `None` returns a `StringIO`) |
|  [05]   | `PolyData.save(filename, binary=True, texture=None, ...)`   | write a single mesh file (`.vtp`/`.ply`/`.stl`/`.obj`)                 |
|  [06]   | `MultiBlock.combine(...) -> UnstructuredGrid` / `save(...)` | merge a block collection, or persist the whole `.vtm`                  |
|  [07]   | `Plotter.save_graphic(filename, raster=, painter=)`         | vector figure (`.svg`/`.eps`/`.ps`/`.pdf`/`.tex`)                      |
|  [08]   | `PolyData.from_regular_faces(points, faces)`                | surface from `(N,3)` points + `(M,k)` faces, `regular_faces` inverse   |

## [04]-[IMPLEMENTATION_LAW]

[SCENE_3D]:
- offscreen axis: configure an available headless OpenGL backend and construct `Plotter(off_screen=True)`; `screenshot(return_img=True)`/`screenshot(filename=)` rasterize without an interactive window. Bracket every plotter in `try`/`finally` `plotter.close()` so the native render window and GL context tear down deterministically.
- worker axis: `pyvista` and `vtk` import after `lane.offload(Kernel.of((WORKER_MODULE, "<kernel>"), KernelTrait.HOSTILE), ...)` crosses the process seam — the native-gated `(module, name)` Kernel form, `REFERENCE` by construction. Worker bodies stay module-level qualified-name functions, and the runtime producer imports neither native package.
- ingest axis: `SceneGrid.of_mesh`, `of_volume`, and `of_glb` admit cross-seam payloads; `_hydrate` selects `PolyData.from_regular_faces`, `ImageData`, or `read` once inside the worker. `wrap` remains the general zero-copy package entry for native VTK, numpy, `trimesh`, or `meshio` values, while `from_meshio` owns the meshio-specific path.
- mesh axis: the `DataSet` family (`PolyData`/`UnstructuredGrid`/`StructuredGrid`/`RectilinearGrid`/`ImageData`) is one mesh hierarchy; mesh kind is a subclass row, never a parallel per-source wrapper. `MultiBlock` collects named datasets, fans filters over its blocks, and `combine(merge_points=)`/`save(...)` merge or persist the whole collection. A mesh's geometry crosses to numpy through the `.points` `(N,3)` coordinate array, the `.regular_faces` `(M,k)` connectivity (uniform-mesh accessor, valid `(M,3)` after `triangulate`, raising `ValueError` on irregular faces), and the computed `.point_normals` view — the buffers the `scene/stage#STAGE` `usd-core` author path consumes.
- render axis: `add_mesh(mesh, scalars=, cmap=, clim=, pbr=, metallic=, roughness=, show_edges=, ambient=, diffuse=, specular=, smooth_shading=, culling=, style=, line_width=)` is the `Style.surface` fold, its `cmap=` accepting a colormap name or a color list; `add_volume` owns `Style.volume`; and `add_points`/`add_arrows`/`add_point_labels` own their closed style cases, `add_points` forwarding its kwargs to `add_mesh`. Publication toggles fold through the `RenderFeature` table. `camera_position` accepts `[position, focal_point, view_up]` and unifies `Camera.pose` with `OrbitPath`; `render_window` exposes the source-build-gated USD exporter handle.
- filter axis: `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`extract_surface`/`triangulate`/`warp_by_scalar`/`glyph`/`streamlines`/`sample`/`decimate` are `DataObjectFilters`/`DataSetFilters` methods returning a NEW dataset; filters compose and never mutate the source unless `inplace=True`. They are the `scene/render#SCENE` `FieldFilter` closed-payload `tagged_union` folded by `functools.reduce` over the `RenderSpec.filters` tuple — a slice-then-threshold-then-glyph chain is a tuple, never a per-filter mesh wrapper. Mind the return shifts: `clip_box`->`UnstructuredGrid`, `threshold`->`UnstructuredGrid`, `slice_orthogonal`->`MultiBlock`, `clip_scalar`/`slice`/`contour`/`extract_surface`->`PolyData`; `clip_scalar` selects complements with `both=`, NOT `return_clipped=`. Mesh repair (`decimate`/`smooth`/`subdivide`/`fill_holes`/`clean`) and CSG (`boolean_union`/`boolean_difference`/`boolean_intersection`) live on `PolyData`.
- export axis: `export_gltf(filename, inline_data=True, rotate_scene=True, save_normals=True)`, `export_vrml`/`export_obj`/`export_html` (filename-only), and `save_graphic` occupy `ExportRow.plotter` rows in `ROW`; `Prepass` and `Capture` carry their per-target differences. `export_obj` writes the `.obj`/`.mtl` bundle, `export_html` requires `trame`, and import methods own the round-trip inverse. USD targets author `surface_arrays` through `usd-core`; `vtkUSDExporter` remains a source-build-gated enhancement.
- evidence: `ArtifactReceipt.Scene.facts` carries image/frame dimensions, staged export point/cell counts, payload address, and USD `ComputeUsdStageStats`/axis/scale/extent evidence on the same band.

## [05]-[STACKING]

- `pyvista` is the high-level consumer of `vtk` (`libs/python/artifacts/.api/vtk.md`): it wraps the demand-driven pipeline, the dataset hierarchy, and the render stack into a numpy-native API, so the design stays on the pyvista surface for plotting/filters/camera and drops to raw `vtk` only through `Plotter.render_window` for the `vtkUSDExporter` the `scene/stage#STAGE` USD arm needs — never re-deriving a `vtkRenderWindow`/`vtkRenderer` pipeline pyvista already owns.
- universal-tier `numpy` (`libs/python/.api/numpy.md`) is the buffer substrate ON TOP of the folder `vtk`/`usd-core` packages: `wrap` admits a numpy vertex/scalar buffer zero-copy, `screenshot(return_img=True)` returns a numpy rgb(a) raster, `.points`/`.regular_faces`/`.point_normals` are numpy views, and the `scene/render#SCENE` `_orbit` camera walk is `numpy.linspace`/`cos`/`sin`/`linalg.norm` — the same numpy buffer a geometry/compute owner produced becomes mesh points, a scalar field, a captured frame, or a USD `Vt.<Type>Array.FromNumpy` source in one hop, no per-element loop.
- `scene/render#SCENE` drawing egress composes standard views, parallel projection, hidden-line removal, feature edges, silhouettes, and `save_graphic` vector output. Extracted line `PolyData.points` cross to `graphic/vector/path#PATH` as a data-parent edge.
- universal-tier `anyio` underlies `LanePolicy.offload`; every `Scene3d` arm crosses as a `KernelTrait.HOSTILE` kernel on the warm process pool under the lane capacity, so sub-3.15 `pyvista`/`vtk`/`pxr` imports stay worker-local. `SceneOp`/`FieldFilter` and `RuntimeRail` wrap provider values and faults.
- `media/container#CONTAINER` consumes `Frames` through `Media.of(frames)`; `screenshot(return_img=True)` rasters pass to `VideoFrame.from_ndarray(array, format="rgb24")` without a PNG intermediary.
- `scene/stage#STAGE` receives the staged `DataSet` through `surface_arrays(dataset, spec)` and passes triangulated numpy buffers to `usd-core`. `vtkUSDExporter` over `Plotter.render_window` remains source-build-gated.

[RAIL_LAW]:
- Package: `pyvista`
- Owns: VTK-backed 3D mesh visualization, `read`/`wrap` ingest, headless OpenGL render, scalar/PBR styling, filters, mesh repair, CSG, geometric sources, render controls, screenshots, numpy mesh views, mesh-file write, and scene export
- Accept: `LanePolicy` qualified-name process dispatch; `SceneGrid` admission over numpy/GLB material; `wrap` for native in-memory adapters; `FieldFilter` folds; `Image`/`Frames`; `ExportRow.plotter`; USD `surface_arrays` and source-built `render_window`
