# [RASM_FABRICATION_API_ACADSHARP]

`ACadSharp` is a pure-managed AnyCPU IL-only library reading and writing AutoCAD DXF (ASCII and binary) and DWG (AC1014 through AC1032) files; the fabrication folder consumes only its READ surface to admit external 2D part profiles into the canonical `Loop` vocabulary at the `geometry2d/profile-import#PROFILE_IMPORT` boundary. The primary entry points are the static `DxfReader.Read` and `DwgReader.Read` facades returning a `CadDocument` model root, whose `Entities`, `ModelSpace`, and `BlockRecords` collections carry the drawing geometry. Closed 2D profiles arrive as `LwPolyline` (lightweight polyline with per-vertex bulge), `Polyline2D`, `Line`, `Arc`, `Circle`, and `Spline` entities; the boundary tessellates each arc/bulge/spline span to `Loop` vertices at the owner's chord-deviation tolerance. No native asset and no RID burden — the package is managed IL, ALC-safe, and coexists with the Rhino-native file I/O the architecture keeps as the host-bound read path.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ACadSharp`
- package: `ACadSharp`
- assembly: `ACadSharp`
- namespace: `ACadSharp`
- asset: pure-managed AnyCPU IL (no native asset, no RID burden)
- rail: fabrication

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: document root and reader facades
- rail: fabrication

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY]   | [CAPABILITY]                                |
| :-----: | :------------------------ | :-------------- | :------------------------------------------ |
|   [1]   | `CadDocument`             | model root      | the read drawing — entities, blocks, tables |
|   [2]   | `DxfReader`               | static reader   | DXF (ASCII + binary) file read              |
|   [3]   | `DwgReader`               | static reader   | DWG (AC1014–AC1032) file read               |
|   [4]   | `NotificationEventHandler`| callback        | read-progress / warning notification sink   |

[PUBLIC_TYPE_SCOPE]: entity collections on the document
- rail: fabrication

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]    | [CAPABILITY]                              |
| :-----: | :------------- | :--------------- | :---------------------------------------- |
|   [1]   | `Entities`     | entity sequence  | all model-space entities (flat access)    |
|   [2]   | `ModelSpace`   | block record     | the model-space block and its entities    |
|   [3]   | `BlockRecords` | block table      | the block-record table for nested blocks  |

[PUBLIC_TYPE_SCOPE]: 2D profile entity types
- rail: fabrication

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]      | [CAPABILITY]                                       |
| :-----: | :------------ | :----------------- | :------------------------------------------------- |
|   [1]   | `LwPolyline`  | lightweight poly   | closed/open polyline with per-vertex bulge         |
|   [2]   | `Polyline2D`  | 2D polyline        | closed/open 2D polyline                            |
|   [3]   | `Line`        | line segment       | two-point straight segment                         |
|   [4]   | `Arc`         | circular arc       | center/radius/start-angle/end-angle arc            |
|   [5]   | `Circle`      | full circle        | center/radius closed circle                        |
|   [6]   | `Spline`      | NURBS spline       | control-point/knot spline (tessellated to vertices)|

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: file read — `DxfReader` / `DwgReader` facades
- rail: fabrication

| [INDEX] | [SURFACE]                                              | [ENTRY_FAMILY] | [CAPABILITY]                          |
| :-----: | :----------------------------------------------------- | :------------- | :------------------------------------ |
|   [1]   | `DxfReader.Read(string path, NotificationEventHandler)`| static read    | read a DXF file into a `CadDocument`  |
|   [2]   | `DwgReader.Read(string path, NotificationEventHandler)`| static read    | read a DWG file into a `CadDocument`  |

## [4]-[RESEARCH]

- [READER_OVERLOADS] The `DxfReader.Read`/`DwgReader.Read` overload set (a `Stream`-bearing overload beside the `string path` overload, the precise `NotificationEventHandler` delegate signature and whether it is an optional parameter, and the instance-reader `new DxfReader(path)` + `.Read()` form beside the static facade) is verified against the README static-`Read` example only; the exact overload list and the optional/required notification parameter need an `assay api` decompile of the installed `ACadSharp` distribution before a boundary fence transcribes the call.
- [VERTEX_ACCESS] The per-entity vertex/geometry accessors the boundary tessellates — `LwPolyline.Vertices` element type and its `Bulge`/`Location` members, the `Polyline2D` vertex collection, the `Arc.Center`/`Radius`/`StartAngle`/`EndAngle` members, the `Circle.Center`/`Radius` members, and whether ACadSharp ships a bulge-to-`Arc` conversion factory (the README names "generating arcs from bulges") or the boundary computes the bulge arc — are an unverified surface until decompiled; the boundary fence stays a RESEARCH item and never settles a vertex-access spelling before the catalogue confirms it.
- [BLOCK_TRAVERSAL] Whether closed profiles inside nested `BlockRecords` require explicit block-reference (`Insert`) resolution and transform composition, or `ModelSpace.Entities` already flattens them, is verified at decompile; the boundary admits only `ModelSpace`/`Entities` top-level closed profiles until the block-traversal contract is confirmed.

[RAIL_LAW]:
- Package: `ACadSharp`
- Owns: DXF/DWG file read into the `CadDocument` model and the 2D profile entity surface the `geometry2d/profile-import#PROFILE_IMPORT` boundary tessellates into `Loop` sets
- Accept: a file path (or stream) to a DXF/DWG, the `NotificationEventHandler` warning sink, and the `LwPolyline`/`Polyline2D`/`Line`/`Arc`/`Circle`/`Spline` closed-profile entities
- Reject: writing DWG/DXF from this folder (Rhino owns the host-bound native write; this boundary is read-only profile ingress), a `CadDocument` or an ACadSharp entity type escaping the boundary into a sibling kernel signature (a foreign CAD entity crosses into the canonical `Loop` at the one boundary, never travels the interior), and any vertex-access spelling settled as fence code before the `[4]-[RESEARCH]` decompile confirms it
