# [RASM_BIM_API_DOTBIM]

`dotbim` is the pure-managed read+write codec for the `.bim` open exchange format — a single
flat JSON document of a shared `Mesh` pool (raw triangle soup: flat `Coordinates` + `Indices`)
plus placed `Element` instances that reference a mesh by id and carry a rigid placement
(`Vector` translation + quaternion `Rotation`), a `Guid`, a free `Type` string, an RGBA
`Color`, optional per-face colors, and a `string→string` `Info` bag. It is the
instancing-friendly, low-ceremony interchange beside the heavyweight authoring formats:
geometry is mesh-only (no BREP, no parametrics, no schema graph), the whole model serializes
through `System.Text.Json`, and identity is the element `Guid`. The codec owns the object
graph and the `.bim` file round-trip; it does not tessellate, validate semantics, or own
materials.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `dotbim`
- package: `dotbim`
- license: MIT
- assembly: `dotbim`
- namespace: `dotbim`
- asset: `netstandard2.0` only; the `net10.0` consumer binds `lib/netstandard2.0` (single TFM, binds forward)
- serialization: `System.Text.Json` (every public member carries a snake_case `[JsonPropertyName]`; the `.bim` wire is STJ, not Newtonsoft)
- transitive-floor: `System.Text.Json` (; inbox-superseded on `net10.0`)
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document, mesh, instance
- namespace: `dotbim`
- rail: interchange

The wire is deliberately flat: `File` owns a `Meshes` pool and an `Elements` list; an
`Element` is one placement of one pooled mesh (the instancing seam — many elements share one
`Mesh` by `MeshId`).

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:--------- |:---------- |:-------------------------------------------------------------------------------------------------------------------------------------- |
| [01] | `File` | interchange | document root: `SchemaVersion` (`"schema_version"`), `Meshes` (`List<Mesh>`), `Elements` (`List<Element>`), `Info` (`Dictionary<string,string>`); instance `Save(path, format)` + static `Read(path)` |
| [02] | `Mesh` | interchange | shared geometry: `MeshId` (`"mesh_id"`, the pool key — setter throws `ArgumentException` when `< 0`), `Coordinates` (`List<double>`, flat XYZ triples), `Indices` (`List<int>`, triangle vertex indices) |
| [03] | `Element` | interchange | placed instance: `MeshId` (mesh reference), `Vector` (translation), `Rotation` (quaternion), `Guid` (validated identity), `Type`, `Color`, `FaceColors` (`List<int>`, per-face RGBA stream), `Info` (`Dictionary<string,string>`) |

[PUBLIC_TYPE_SCOPE]: placement and color value types
- namespace: `dotbim`
- rail: interchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:---------- |:---------- |:---------------------------------------------------------------------------------------------------------- |
| [01] | `Vector` | interchange | `struct` translation: `X`/`Y`/`Z` (`double`, `"x"`/`"y"`/`"z"`) — the element origin in model space |
| [02] | `Rotation` | interchange | `struct` orientation as a quaternion: `Qx`/`Qy`/`Qz`/`Qw` (`double`, `"qx"`…`"qw"`) — NOT Euler angles |
| [03] | `Color` | interchange | `struct` RGBA: `R`/`G`/`B`/`A` (`int`, `"r"`…`"a"`) — each setter throws `ArgumentException` outside `0..255` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file round-trip
- namespace: `dotbim`
- rail: interchange

The codec is the two `File` members; there is no separate reader/writer/options type. Both
enforce the `.bim` extension and route through `System.Text.Json`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------- |:---------------------------------------- |:------------------------------------------------------------------------------------------- |
| [01] | `File.Read` | `static (string path) → File` | `JsonSerializer.Deserialize<File>` of the file text; throws `ArgumentException` unless `path` ends `.bim` |
| [02] | `File.Save` | `(string path, bool format = true)` | `JsonSerializer.Serialize` (`WriteIndented = format`) to `path`; throws unless `path` ends `.bim` |

## [04]-[IMPLEMENTATION_LAW]

[INSTANCING_LAW]:
- geometry lives once in `File.Meshes`; an `Element` is a lightweight placement referencing `Mesh.MeshId` plus a rigid transform — N repeated objects are N `Element`s over one shared `Mesh`, never N copied vertex buffers. Decode resolves an element to its mesh by matching `Element.MeshId == Mesh.MeshId`.
- the placement is `Vector` (translation) + `Rotation` (unit quaternion `Qx,Qy,Qz,Qw`); compose them into a 4×4 (or rigid) transform to bake an element into world space. `Rotation` is a quaternion — never read it as Euler angles.
- `Mesh.Coordinates` is a flat `double` stream of XYZ triples; `Mesh.Indices` is a flat `int` stream of triangle corners (3 per face). Vertex `i` is `Coordinates[3i..3i+3]`; face `f` is `Indices[3f..3f+3]`.

[COLOR_LAW]:
- the codec validates at the property setter, not only at parse: `Color` channels (`int`) reject outside `0..255`, `Mesh.MeshId` rejects `< 0`, and `Element.Guid` rejects a malformed GUID — each throws `ArgumentException`, so building a `File` graph through the typed setters fails a structurally-invalid model before `Save`, never on a downstream consumer.
- Element-level `Color` is the whole-object color; `Element.FaceColors` is the optional per-face override stream (RGBA quadruples aligned to the mesh faces) and wins over `Color` where present.

[INTEGRATION_STACK]:
- canonical-carrier leg: a `Mesh` (coordinates+indices) decodes into the canonical kernel mesh and an `Element` (mesh ref + `Vector`/`Rotation` placement + `Guid` + `Type` + `Color` + `Info`) maps onto a `BimElement` instance with its placement transform at the `Exchange/import` boundary; `dotbim.*` types never leak past the codec — internal code holds canonical Bim shapes keyed by the element `Guid`.
- mesh-codec siblings: `dotbim` is the lightweight JSON sibling of the binary mesh exchanges — `SharpGLTF` (`api-sharpgltf`, glTF/GLB with PBR materials + scene graph), `AssimpNetter` (`api-assimpnetter`, the many-format importer), and the compression legs `Alimer.MeshOptimizer` (`api-alimer-meshoptimizer`) / `Openize.Drako` (`api-openize-drako`); all four meet at the same canonical triangle mesh, so a model imported as `.bim` re-exports as glTF (and vice-versa) through the shared carrier, and `dotbim`'s flat soup is the form to feed the optimizer/Draco legs before a heavier export.
- IFC-beside leg: `dotbim` is the preview/issue-payload/low-friction exchange that rides beside the authoritative `GeometryGym` IFC semantic model (`api-geometrygym-ifc`) and `USD` (`api-usd`) — an `Element.Info` bag carries the round-trip metadata (IFC class, name, property snapshot) so a `.bim` export of an IFC view preserves the semantic tags as strings without re-encoding the IFC graph; on re-import the `Info` keys re-bind to the canonical element.
- identity leg: a `File.Save` produces canonical `.bim` JSON whose UTF-8 bytes feed `System.IO.Hashing` `XxHash3`/`XxHash128` (`api-hashing`) for the snapshot content key, joining the IFC/glTF/USD exports on one content-identity rail; the element `Guid` is the stable per-object identity across re-exports.

[LOCAL_ADMISSION]:
- `.bim` import enters through `File.Read`, resolves each `Element` to its pooled `Mesh` by `MeshId`, bakes the `Vector`/`Rotation` placement, and maps onto canonical Bim carriers keyed by `Guid`.
- `.bim` export enters through a canonical→`File` build (mesh pooling by shared geometry, element placement decomposition into `Vector`+quaternion `Rotation`, `Info`-bag metadata projection) then `File.Save(path, format)`.

[RAIL_LAW]:
- Package: `dotbim`
- Owns: the `.bim` open-format read+write — the shared `Mesh` pool, the placed `Element` instance model (transform + `Guid` + `Type` + `Color`/`FaceColors` + `Info`), and the `System.Text.Json` file round-trip
- Accept: lightweight mesh+metadata interchange, preview/issue payloads, instancing-preserving external model exchange beside IFC/glTF/USD
- Reject: BREP/parametric/schema geometry (mesh-only), tessellation (consumes meshes, never builds them), material/appearance systems (glTF/USD own PBR), IFC semantics (GeometryGym), and leaking `dotbim.*` types past the codec boundary
