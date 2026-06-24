# [PY_ARTIFACTS_API_PYVISTA]

`pyvista` supplies the VTK-backed 3D scientific visualization surface for the artifacts visuals rail: a `DataSet` mesh family (`PolyData`, `UnstructuredGrid`, `StructuredGrid`, `RectilinearGrid`, `ImageData`, `MultiBlock`), a `Plotter` offscreen render owner, the `read`/`wrap` ingest functions, a geometric-source factory, and the dataset-method filter family (clip/clip_box/slice/threshold/contour/extract_surface/warp/glyph/streamlines/decimate/smooth/boolean) that drive offscreen mesh rendering, scalar-field visualization, mesh repair, and glTF/VRML/OBJ/HTML scene export. The package owner composes `wrap`/`read`, the `DataSet` mesh family, `Plotter.add_mesh`/`add_volume`/`screenshot`/`export_gltf`, and the filter family into the 3D scene owner; it removes any interactive window because `Plotter(off_screen=True)` over a software-GL backend renders host-free, and it never re-implements the VTK render engine, the demand-driven pipeline, or the file reader/writer pairs pyvista already wraps from `vtk`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pyvista`
- package: `pyvista`
- import: `pyvista` (conventionally `import pyvista as pv`)
- owner: `artifacts`
- rail: visuals
- installed: `0.48.4` on `vtk` `9.6.2`, reflected by source introspection on a CPython 3.12 environment
- license: MIT (pyvista) over `vtk` BSD-3-Clause; permissive, no copyleft gate
- abi: pure-Python over the `vtk` C++ native runtime (the ABI/native surface); hard `Requires-Dist` deps are `numpy>=1.21`, `matplotlib>=3.0.1` (colormaps/scalar bars), `scooby`, `pooch`, `pillow` (screenshot encode), `cyclopts>=4.0` (the 0.48 report/CLI layer), `typing-extensions>=4.10`. `meshio>=5.2` is the optional `[io]` extra (NOT a hard dep) — the manifest admits it separately so `from_meshio`/meshio-`wrap` resolve; `trimesh` is NOT a pyvista dep at all, the `wrap(trimesh.Trimesh)` path is a structural points/faces conversion. vtk constraint is `vtk!=9.4.0,!=9.4.1,>=9.2.2,<9.7.0`; vtk 9.6.2 ships cp39–cp314 wheels (incl. cp314t free-threaded) but NO cp315 wheel, so both `pyvista>=0.48.4` and `vtk>=9.6.2` are manifest-gated `python_version<'3.13'` — a conservative interpreter floor below the cp314 wheel ceiling; the cp315-core owner imports neither and crosses the runtime subprocess seam
- entry points: none (library only)
- capability: VTK-backed 3D mesh visualization — the `DataSet` mesh hierarchy, `read`/`wrap` ingest (including direct `trimesh.Trimesh`/`meshio.Mesh` wrap), offscreen software-GL rendering, scalar-field coloring with PBR/lighting, mesh filters (clip/clip_box/clip_scalar/slice/slice_orthogonal/threshold/contour/extract_surface/warp/glyph/streamlines/sample/cell-point data transfer), mesh repair (decimate/smooth/subdivide/fill_holes/clean) and CSG booleans, geometric sources, screenshot rasterization, mesh-file write (`save`), and glTF/VRML/OBJ/HTML scene export

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: dataset mesh family
- rail: visuals

| [INDEX] | [SYMBOL]           | [PACKAGE_ROLE]   | [CAPABILITY]                                       |
| :-----: | :----------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `DataSet`          | mesh base        | points/cells/scalar arrays plus the filter family   |
|  [02]   | `PolyData`         | surface mesh     | points/faces surface with decimate/smooth/boolean   |
|  [03]   | `UnstructuredGrid` | volume mesh      | arbitrary-cell unstructured volume                  |
|  [04]   | `StructuredGrid`   | structured grid  | curvilinear i/j/k grid                              |
|  [05]   | `RectilinearGrid`  | rectilinear grid | axis-aligned variable-spacing grid                  |
|  [06]   | `ImageData`        | uniform grid     | regular volumetric image grid (volume rendering)    |
|  [07]   | `MultiBlock`       | mesh container   | named collection of datasets; filters fan over blocks|
|  [08]   | `pyvista_ndarray`  | array view       | VTK-backed numpy array view over point/cell data    |

[PUBLIC_TYPE_SCOPE]: render and geometric-source types
- rail: visuals

| [INDEX] | [SYMBOL]   | [PACKAGE_ROLE] | [CAPABILITY]                              |
| :-----: | :--------- | :------------- | :---------------------------------------- |
|  [01]   | `Plotter`  | render owner    | add meshes/volumes/points, render offscreen, export |
|  [02]   | `Sphere`   | source         | parametric sphere mesh                    |
|  [03]   | `Cube`     | source         | box mesh (also `Box`/`Pyramid`)           |
|  [04]   | `Cylinder` | source         | cylinder mesh (also `Cone`/`Tube`)        |
|  [05]   | `Plane`    | source         | plane mesh (also `Disc`/`Circle`/`Polygon`/`Triangle`) |
|  [06]   | `Arrow`    | source         | arrow glyph mesh                          |
|  [07]   | `Line`     | source         | line/spline mesh (also `Spline`/`lines_from_points`) |
|  [08]   | `Text3D`   | source         | extruded 3D text mesh                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: ingest, render, capture
- rail: visuals

`read`/`wrap` build a dataset from a file, a VTK object, a numpy array, or a `trimesh`/`meshio` mesh; `Plotter` renders offscreen and `screenshot` rasterizes.

| [INDEX] | [SURFACE]             | [CALL_SHAPE]                                                                          | [CAPABILITY]                                |
| :-----: | :-------------------- | :------------------------------------------------------------------------------------ | :------------------------------------------ |
|  [01]   | `pyvista.read`        | `read(filename, force_ext=None, file_format=None, progress_bar=False, *, cls=None, validate=None) -> DataObject` | read a mesh from a file        |
|  [02]   | `pyvista.wrap`        | `wrap(dataset: vtkDataObject | DataObject | trimesh.Trimesh | meshio.Mesh | vtkAbstractArray | numpy.ndarray | None, *, validate=None) -> DataObject | pyvista_ndarray | None` | wrap a VTK object (zero-copy), an array (-> `pyvista_ndarray`), or a `trimesh`/`meshio` mesh (structural points/faces conversion) |
|  [03]   | `pyvista.from_meshio` | `from_meshio(mesh) -> UnstructuredGrid`                                                | import a meshio mesh                        |
|  [04]   | `Plotter`             | `Plotter(off_screen=None, ...)`                                                       | construct an offscreen render owner          |
|  [05]   | `Plotter.add_mesh`    | `add_mesh(mesh, scalars=None, cmap=None, clim=None, show_edges=None, opacity=None, pbr=None, metallic=None, roughness=None, ...) -> Actor` | add a mesh with scalar coloring/PBR |
|  [06]   | `Plotter.add_volume`  | `add_volume(volume, scalars=None, clim=None, opacity='linear', cmap=None, mapper=None, blending='composite', ...) -> Actor` | GPU/smart volume rendering    |
|  [07]   | `Plotter.add_points`  | `add_points(points, style='points', **kwargs) -> Actor` (also `add_lines`/`add_arrows`/`add_point_labels`) | add point/line/glyph actors    |
|  [08]   | `Plotter.screenshot`  | `screenshot(filename=None, transparent_background=None, return_img=True, window_size=None, scale=None) -> pyvista_ndarray | None` | rasterize the scene to PNG/array |
|  [09]   | `Plotter.show`        | `show(screenshot=False, window_size=None, return_img=False, cpos=None, ...) -> ...`   | render and optionally capture/return         |
|  [10]   | `Plotter` render control | `enable_anti_aliasing('ssaa'|'msaa'|'fxaa')` / `enable_ssao()` / `enable_depth_peeling()` / `enable_eye_dome_lighting()` / `enable_shadows()` / `enable_parallel_projection()` / `set_background(color, top=...)` / `camera_position` (property) / `add_axes()` / `add_scalar_bar()` / `add_text()` | publication-quality offscreen tuning: SSAA/SSAO/depth-peel/EDL/shadows, background, camera, axes, scalar bar, annotation |
|  [11]   | `Plotter` scene import | `import_gltf(filename)` / `import_obj(filename)` / `import_vrml(filename)`             | load an existing glTF/OBJ/VRML scene into the plotter for re-render/re-export round-trip |

[ENTRYPOINT_SCOPE]: filters, mesh repair, write, and scene export
- rail: visuals

Filters are `DataSet` methods returning a new dataset; `PolyData` adds mesh repair and CSG; `save`/`export_*` emit files.

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]                                                                                  | [CAPABILITY]                                            |
| :-----: | :------------------------ | :-------------------------------------------------------------------------------------------- | :------------------------------------------------------ |
|  [01]   | `DataSet.clip`            | `clip(normal=None, origin=None, invert=True, value=0.0, inplace=False, return_clipped=False, crinkle=False, plane=None)` | clip by a plane (also `clip_box`/`clip_scalar`) |
|  [02]   | `DataSet.slice`           | `slice(normal=None, origin=None, generate_triangles=False, contour=False, plane=None)`        | cut a planar slice (also `slice_orthogonal`)            |
|  [03]   | `DataSet.threshold`       | `threshold(value=None, scalars=None, invert=False, preference='cell', method='upper', component_mode='all', ...)` | extract cells in a value range          |
|  [04]   | `DataSet.contour`         | `contour(isosurfaces=10, scalars=None, method='contour' | 'marching_cubes' | 'flying_edges', rng=None, ...)` | extract isosurfaces           |
|  [05]   | `DataSet.extract_surface` | `extract_surface(pass_pointid=True, pass_cellid=True, nonlinear_subdivision=None) -> PolyData` | extract the boundary surface (precedes glTF export)     |
|  [06]   | `DataSet.warp_by_scalar`  | `warp_by_scalar(scalars=None, factor=1.0, normal=None, inplace=False)`                        | displace points by a scalar field (also `glyph`/`streamlines`/`sample`) |
|  [07]   | `PolyData.decimate`       | `decimate(target_reduction, volume_preservation=False, inplace=False, ...)`                   | reduce triangle count (also `smooth`/`subdivide`/`fill_holes`/`clean`) |
|  [08]   | `PolyData.boolean_union`  | `boolean_union(other_mesh, tolerance=1e-05)`                                                  | CSG union (also `boolean_difference`/`boolean_intersection`) |
|  [09]   | `PolyData.save`           | `save(filename, binary=True, texture=None, recompute_normals=True, compression='zlib', **writer_kwargs)` | write a mesh file (`.vtp`/`.ply`/`.stl`/`.obj`) |
|  [10]   | `Plotter.export_gltf`     | `export_gltf(filename, inline_data=True, rotate_scene=True, save_normals=True)`               | export the scene to glTF                                |
|  [11]   | `Plotter.export_html`     | `export_html(filename) -> io.StringIO | None`                                                | export an interactive HTML scene (`None` to StringIO)   |
|  [12]   | `Plotter.export_obj`      | `export_obj(filename)` / `export_vrml(filename)`                                             | export the scene to OBJ / VRML                          |

## [04]-[IMPLEMENTATION_LAW]

[SCENE_3D]:
- import: `import pyvista as pv` at module scope inside the gated-band worker only; the cp315-core owner imports neither `pyvista` nor `vtk` (no cp315 wheel) and dispatches the whole render onto `anyio.to_process.run_sync`, since the VTK render owns process-global GL state that must not enter the core interpreter.
- offscreen axis: select the osmesa/EGL software-GL backend and construct `Plotter(off_screen=True)`; the entire render runs with zero display/browser/GPU, never an interactive window — `show(screenshot=...)`/`screenshot(...)` rasterize and `show(interactive=...)` is host-only and never used on the host-free path.
- ingest axis: `wrap` is the canonical zero-copy entry — it accepts a VTK object, a numpy point/cell array, OR a geometry-tier `trimesh.Trimesh`/`meshio.Mesh` directly, so a mesh conditioned by the geometry owner crosses the wire as vertices/faces arrays and is wrapped to `PolyData` without a file round-trip; `read` is the file path. `from_meshio` is the meshio-specific row.
- mesh axis: the `DataSet` family (`PolyData`/`UnstructuredGrid`/`StructuredGrid`/`RectilinearGrid`/`ImageData`) is one mesh hierarchy; mesh kind is a subclass row, never a parallel per-source wrapper; `MultiBlock` collects named datasets, fans filters over its blocks, and `combine(merge_points=)`/`save(...)` merge or persist the whole collection.
- render axis: `Plotter.add_mesh(mesh, scalars=..., cmap=..., clim=..., pbr=..., metallic=..., roughness=...)` is the single mesh-add surface; scalar coloring and PBR material are add-mesh rows, never a parallel colored-mesh type. `add_volume(volume, mapper='gpu' | 'smart', opacity=..., blending=...)` owns volumetric `ImageData` rendering; `add_points`/`add_lines`/`add_arrows`/`add_point_labels` cover point clouds, vectors, and annotation.
- filter axis: `clip`/`clip_box`/`clip_scalar`/`slice`/`slice_orthogonal`/`threshold`/`contour`/`extract_surface`/`warp_by_scalar`/`glyph`/`streamlines`/`sample`/`cell_data_to_point_data` are dataset-method filters returning a NEW dataset; filters compose and never mutate the source unless `inplace=True`. Mesh repair (`decimate`/`smooth`/`subdivide`/`fill_holes`/`clean`) and CSG (`boolean_union`/`boolean_difference`/`boolean_intersection`) live on `PolyData`.
- export axis: `export_gltf(inline_data=, save_normals=)`/`export_vrml`/`export_obj`/`export_html` emit scene files; the VTK glTF exporter handles surface `PolyData`, so `extract_surface` precedes glTF export. The inverse `import_gltf`/`import_obj`/`import_vrml` load an existing scene into the plotter, so a render-tune-re-export round-trip stays in-process. `PolyData.save` writes a single mesh file (binary by default) where a full scene is not needed.
- evidence: each render captures point count, cell count, scalar-array name, render window size, output format, and output byte length as a visuals receipt.
- boundary: pyvista owns 3D scientific scene render and export. Integration: it sits downstream of the geometry tier — a `trimesh.Trimesh` is `wrap`ped directly (no mesh-file detour), and the heavy render is dispatched over the runtime `anyio.to_process` subprocess seam from cp315-core. Publication 2D plotting routes to `matplotlib` (also pyvista's colormap source), declarative charts route to `vl-convert-python`, raster encode routes to `pillow`, and the `.glb`/mesh-file codec is owned by the data tier's `MeshPayload`. Built on the `vtk` sibling owner, which carries the raw `vtkAlgorithm` pipeline this package wraps.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `pyvista`
- Owns: VTK-backed 3D mesh visualization, `read`/`wrap` ingest (including trimesh/meshio), offscreen software-GL render, scalar-field coloring with PBR, mesh filters, mesh repair and CSG booleans, geometric sources, screenshot rasterization, mesh-file write, glTF/VRML/OBJ/HTML export
- Accept: gated sub-3.13 worker render dispatched over the runtime `anyio.to_process` subprocess seam, wrapping the geometry tier's in-memory `trimesh.Trimesh`, feeding the scene and export-bundle owners
- Reject: wrapper-renames of `add_mesh`/`screenshot`; an interactive window on the host-free path; a mesh-file round-trip where `wrap(trimesh.Trimesh)` is zero-copy; a per-source mesh wrapper where the `DataSet` family exists; a `pyvista`/`vtk` import in the cp315-core process; identity minting the runtime owns
