# [RASM_BIM_API_DOTBIM]

`dotbim` is the pure-managed read+write codec for the `.bim` open exchange format: one flat `System.Text.Json` document pooling shared meshes beneath placed `Element` instances that reference a mesh by id and carry a rigid placement. Instancing is the distinguishing law — N repeated objects are N lightweight `Element` placements over one shared `Mesh`, the low-ceremony interchange beside heavyweight authoring formats. This codec owns the `.bim` object graph and file round-trip alone; it never tessellates, validates semantics, or owns materials.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dotbim`
- package: `dotbim` (MIT)
- assembly: `dotbim`
- namespace: `dotbim`
- asset: `netstandard2.0` single TFM; the `net10.0` consumer binds `lib/netstandard2.0` forward
- serialization: `System.Text.Json`; every public member carries a snake_case `[JsonPropertyName]`
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root, shared mesh pool, placed instance

| [INDEX] | [SYMBOL]  | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :-------- | :------------ | :--------------------------------- |
|  [01]   | `File`    | class         | document root over the mesh pool   |
|  [02]   | `Mesh`    | class         | shared geometry pooled by `MeshId` |
|  [03]   | `Element` | class         | one placement of one pooled mesh   |

[File]: `Meshes` `Elements` `SchemaVersion` `Info`
[Mesh]: `MeshId` `Coordinates` `Indices`
[Element]: `MeshId` `Vector` `Rotation` `Guid` `Type` `Color` `FaceColors` `Info`

[PUBLIC_TYPE_SCOPE]: placement and color value types

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :--------- | :------------ | :----------------------------- |
|  [01]   | `Vector`   | struct        | translation in model space     |
|  [02]   | `Rotation` | struct        | unit-quaternion orientation    |
|  [03]   | `Color`    | struct        | RGBA channels bounded `0..255` |

[Vector]: `X` `Y` `Z`
[Rotation]: `Qx` `Qy` `Qz` `Qw`
[Color]: `R` `G` `B` `A`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file round-trip

`File.Read` and `File.Save` are the entire codec; no reader, writer, or options type exists.

| [INDEX] | [SURFACE]                   | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------- | :------- | :------------------------------------- |
|  [01]   | `File.Read(string) -> File` | static   | deserialize a `.bim` file to the graph |
|  [02]   | `File.Save(string, bool)`   | instance | serialize the graph to a `.bim` path   |

- `File.Save`: `format` (default `true`) selects `JsonSerializer` `WriteIndented`; both members reject a non-`.bim` path with `ArgumentException`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Geometry lives once in `File.Meshes`; an `Element` references a `Mesh` by `MeshId` under a rigid transform, so decode resolves an element to its mesh by matching `Element.MeshId == Mesh.MeshId`.
- Placement composes `Vector` translation with `Rotation`, a unit quaternion (`Qx,Qy,Qz,Qw`) folded into a rigid 4×4 to bake an element into world space; `Rotation` is never read as Euler angles.
- `Mesh.Coordinates` is a flat `double` XYZ stream and `Mesh.Indices` a flat `int` triangle-corner stream: vertex `i` is `Coordinates[3i..3i+3]`, face `f` is `Indices[3f..3f+3]`.
- Property setters validate on build: `Color` channels reject outside `0..255`, `Mesh.MeshId` rejects `< 0`, and `Element.Guid` rejects a malformed GUID with `ArgumentException`, so a structurally-invalid graph fails before `Save`.
- `Element.Color` is the whole-object color; `Element.FaceColors` is the optional per-face RGBA override stream and wins over `Color` where present.

[STACKING]:
- `SharpGLTF`(`.api/api-sharpgltf.md`), `AssimpNetter`(`.api/api-assimpnetter.md`), `Alimer.MeshOptimizer`(`.api/api-alimer-meshoptimizer.md`), `Openize.Drako`(`.api/api-openize-drako.md`): every mesh sibling meets `dotbim` at the canonical triangle mesh, so a `.bim` model re-exports as glTF through the shared carrier and its flat `Coordinates`/`Indices` soup feeds the meshopt and Draco compressors before a heavier export.
- `GeometryGymIFC`(`.api/api-geometrygym-ifc.md`), `USD`(`.api/api-usd.md`): `dotbim` is the preview and issue-payload exchange beside the authoritative semantic model; an `Element.Info` bag carries round-trip metadata (IFC class, name, property snapshot) so a `.bim` export of an IFC view preserves the semantic tags as strings and re-binds them on re-import.
- `System.IO.Hashing`(`libs/csharp/.api/api-hashing.md`): a `File.Save` produces canonical `.bim` UTF-8 bytes that feed the snapshot content key, and the element `Guid` is the stable per-object identity across re-exports.
- within-library: a `Mesh` decodes into the canonical kernel mesh and an `Element` (mesh ref, `Vector`/`Rotation` placement, `Guid`, `Type`, `Color`, `Info`) maps onto a `BimElement` at the `Exchange/import` boundary keyed by `Guid`; `dotbim.*` types never leak past the codec.

[LOCAL_ADMISSION]:
- `.bim` import enters through `File.Read`, resolving each `Element` to its pooled `Mesh` by `MeshId` and baking the `Vector`/`Rotation` placement; export enters through a canonical-to-`File` build — mesh pooling by shared geometry, placement decomposition into `Vector` and quaternion `Rotation`, `Info`-bag metadata projection — then `File.Save`.

[RAIL_LAW]:
- Package: `dotbim`
- Owns: the `.bim` open-format read+write — the shared `Mesh` pool, the placed `Element` model, and the `System.Text.Json` file round-trip
- Accept: lightweight mesh-and-metadata interchange, preview and issue payloads, instancing-preserving model exchange beside IFC/glTF/USD
- Reject: BREP/parametric/schema geometry, tessellation, material and appearance systems, IFC semantics, and any `dotbim.*` type crossing the codec boundary
