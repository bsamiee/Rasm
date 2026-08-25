# [PY_CAD_API_TRIMESH]

`trimesh` contributes one public glTF decode to the OCCT provider: the emitted GLB is re-read as a scene graph, and its placements, unique mesh faces, closure, and volume become the census the unary reply carries. Decode is the whole role here — mesh conditioning, boolean work, and file authoring stay with the geometry branch's own registration of this distribution.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `trimesh` (MIT)
- module: `trimesh`
- namespaces: `trimesh`, `trimesh.scene`, `trimesh.graph`, `trimesh.util`
- depends: `numpy` for vertex and face buffers; `networkx` as the component engine `trimesh.graph.split` dispatches to
- rail: emitted-artifact census

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: decoded scene carriers

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :----------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Scene`      | class         | decoded glTF scene: geometries by name over one transform graph |
|  [02]   | `SceneGraph` | class         | node and transform tree reached as a `Scene` instance attribute |
|  [03]   | `Trimesh`    | class         | one mesh carrying vertices, faces, closure, and signed volume   |
|  [04]   | `Geometry`   | interface     | base of every scene value; only `Trimesh` carries triangles     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decode, traversal, and census

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :---------------------------------------------------------------- | :------- | :---------------------------------------------- |
|  [01]   | `load_scene(file_obj, file_type=, resolver=, allow_remote=)`      | static   | decode a container without conditioning it      |
|  [02]   | `Scene.graph.nodes_geometry`                                      | property | node names carrying geometry, one per placement |
|  [03]   | `Scene.graph.to_edgelist()`                                       | instance | parent, child, and attribute triples            |
|  [04]   | `Scene.graph[node]`                                               | operator | matrix and geometry name for one placed node    |
|  [05]   | `Scene.geometry`                                                  | property | unique decoded geometries keyed by name         |
|  [06]   | `Scene.to_mesh()`                                                 | instance | apply every placement into one mesh             |
|  [07]   | `Trimesh.merge_vertices(merge_tex=, merge_norm=, digits_vertex=)` | instance | weld coincident vertices in place               |
|  [08]   | `Trimesh.split(only_watertight=)`                                 | instance | connected components as independent meshes      |
|  [09]   | `Trimesh.is_watertight`                                           | property | every edge paired exactly twice                 |
|  [10]   | `Trimesh.volume`                                                  | property | signed enclosed volume, closed meshes only      |
|  [11]   | `Trimesh.faces`                                                   | property | triangle index array whose length is the count  |

- `load_scene`: `file_type` is declared while `process` rides `**kwargs` unvalidated, so a misspelling there silently conditions the scene.
- `Scene.graph`: reads as an instance attribute; reaching it on the class raises `AttributeError`.
- `Trimesh.merge_vertices`: mutates in place and returns `None`.
- `Trimesh.split`: delegates to `trimesh.graph.split`, which raises `ImportError` where neither engine resolves.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Decode admits emitted bytes untouched under `process=False`, so a census counts what was written rather than what conditioning produced.
- Placement count collapses each `from_gltf_primitive` child onto its parent, because one mesh split across material primitives is one placement.
- Triangle count sums face-array lengths once across unique geometries, so an instanced geometry counts once however many nodes place it.
- Closure and volume read the FLATTENED scene: placements apply first, then welding removes the bit-identical seams glTF float32 promotion introduces.
- Welding passes explicit texture, normal, and vertex-digit arguments rather than inheriting the mutable `tol.merge` default.
- Component split is engine-backed and never pure-Python, so the engine is declared runtime closure rather than an optional extra.
- Per-body volume sums absolute component volumes, so an inverted component contributes magnitude instead of cancelling a sibling into a forged zero.

[STACKING]:
- `cadquery-ocp`(`.api/cadquery-ocp.md`): `RWGltf_CafWriter.Perform` writes the XCAF document to a provider-owned path and `load_scene` re-reads exactly those bytes, so the two meet at the file alone and no OCCT handle crosses.
- within-lib `metrology/census` owner: folds decode, placement collapse, face sum, weld, and split into the one census both native legs read, so no sibling page opens a scene of its own.

[LOCAL_ADMISSION]:
- `trimesh` is admitted here for emitted-artifact census alone; repair, boolean work, and authoring route to geometry's registration at its own stratum.

[RAIL_LAW]:
- Package: `trimesh`
- Owns: glTF decode into a placement graph and unique mesh set, transform flattening, coincident-vertex welding, component separation, and the closure and signed-volume reads a census publishes
- Accept: emitted GLB bytes on a provider-owned path, and the flattened mesh a placement walk produces
- Reject: a private glTF parser, `load(force=...)` erasing the static return kind, `Scene.dump`, `merge_primitives=True` over material-free OCCT primitives, source-shape estimates standing in for an emitted census, and a sentinel count published where decode refused
