# [RASM_BIM_API_PLY_NET]

`Ply.Net` is the dedicated pure-managed PLY codec for the `Model/exchange#FORMAT`
MeshText `InterchangeCodec` PLY leg, superseding the hand-rolled `PlyReader`. The
unifying primitive is an immutable record graph — `Header` (`Format` + ordered
`Element` list) describes the file, `Dataset` (`Header` + lazy `ElementData`
sequence) carries the decoded payload — split across a header-only fast path
(`ParseHeader`) and a chunked streaming body (`Parse(..., maxChunkSize)`). Every
property value lands as a typed `System.Array` reachable by name, so a vertex
buffer or face index list is materialized once and handed to the Compute
tessellation seam without an intermediate parser DTO.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Ply.Net`
- package: `Ply.Net`
- license: Apache-2.0 (per-component)
- assembly: `Ply.Net`
- namespace: `Ply.Net`
- asset: netstandard2.0 single TFM; the net10.0 consumer binds `lib/netstandard2.0` (IL-only AnyCPU, no `runtimes/` folder, ALC-safe)
- asset: single BCL transitive (`System.Collections.Immutable`); zero third-party managed deps
- rail: interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: header-descriptor record graph
- package: `Ply.Net`
- namespace: `Ply.Net`
- rail: interchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:--------- |:---------- |:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [01] | `Header` | interchange | `record Header(Format Format, ImmutableList<Element> Elements, ImmutableList<string> HeaderLines, long DataOffset)`; typed element accessors `Vertex`/`Face`/`Edge`/`Material`/`Cell` (`Element?` `SingleOrDefault` by `ElementType`) |
| [02] | `Element` | interchange | `record Element(ElementType Type, string Name, int Count, ImmutableList<Property> Properties)`; `ContainsListProperty` predicate; `Add(Property)` builder returning a new `Element` |
| [03] | `Property` | interchange | `record Property(DataType DataType, string Name, DataType ListCountType)`; `IsListProperty => ListCountType != DataType.Undefined` discriminates the `vertex_indices`-style list column |

[PUBLIC_TYPE_SCOPE]: decoded-payload record graph
- package: `Ply.Net`
- namespace: `Ply.Net`
- rail: interchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:------------- |:---------- |:----------------------------------------------------------------------------------------------------------------------------------------------- |
| [01] | `Dataset` | interchange | `record Dataset(Header Header, IEnumerable<ElementData> Data)`; the `Data` sequence is lazy/streamed, one `ElementData` per declared `Element` |
| [02] | `ElementData` | interchange | `record ElementData(Element Element, ImmutableList<PropertyData> Data)`; `this[string propertyName]` indexer resolves a column's `PropertyData` |
| [03] | `PropertyData` | interchange | `record PropertyData(Property Property, Array Data)`; `Data` is a typed CLR `System.Array` (e.g. `float[]` for `Float32`, `int[][]` for a list column) materialized once |

[PUBLIC_TYPE_SCOPE]: codec discriminants
- package: `Ply.Net`
- namespace: `Ply.Net`
- rail: interchange

| [INDEX] | [SYMBOL] | [RAIL] | [CAPABILITY] |
|:-----: |:------------ |:---------- |:----------------------------------------------------------------------------------------------------------------------------------------- |
| [01] | `Format` | interchange | `Undefined`, `Ascii`, `BinaryLittleEndian`, `BinaryBigEndian` — the full ascii/binary LE+BE matrix the FORMAT row claims, read off `Header.Format` |
| [02] | `DataType` | interchange | `Undefined`, `Int8`, `UInt8`, `Int16`, `UInt16`, `Int32`, `UInt32`, `Int64`, `UInt64`, `Float32`, `Float64` — the per-property scalar width |
| [03] | `ElementType` | interchange | `Vertex`, `Face`, `Edge`, `Material`, `Cell`, `UserDefined` — the element classification driving `Header.Vertex`/`Face`/… accessors |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: PlyParser — parse operations
- package: `Ply.Net`
- namespace: `Ply.Net`
- rail: interchange

`PlyParser` is the sole static entrypoint; every overload accepts an optional
`Action<string>? log` diagnostic sink.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------- |:----------------------------------------------------------- |:----------------------------------------------------------------------- |
| [01] | `PlyParser.ParseHeader` | `(Stream f, Action<string>? log)` → `Header` | header-only decode; leaves the stream at `Header.DataOffset` for a custom body read |
| [02] | `PlyParser.ParseHeader` | `(string filename, Action<string>? log = null)` → `Header` | header-only decode by path |
| [03] | `PlyParser.Parse` | `(Header header, Stream f, Action<string>? log)` → `Dataset` | full body decode against an already-parsed header |
| [04] | `PlyParser.Parse` | `(Header header, Stream f, int maxChunkSize, Action<string>? log)` → `Dataset` | chunked body decode bounding peak memory by `maxChunkSize` for large meshes |
| [05] | `PlyParser.Parse` | `(Stream f, int maxChunkSize, Action<string>? log = null)` → `Dataset` | one-shot header+body decode from a stream |
| [06] | `PlyParser.Parse` | `(string filename, int maxChunkSize, Action<string>? log = null)` → `Dataset` | one-shot decode by path |

## [04]-[IMPLEMENTATION_LAW]

[IDENTITY_PROFILE]:
- namespace: `Ply.Net`
- header root: `Header` (`Format` + ordered `Element` list)
- payload root: `Dataset` (`Header` + lazy `ElementData` sequence)
- column root: `PropertyData.Data` typed `System.Array`
- receipt root: format, element counts, and property data-type widths

[CODEC_COMPOSE]:
- format coverage: `Format` carries all four members the FORMAT row's "ascii/binary LE+BE" claim requires; the importer reads `Header.Format` and never branches on a hand-detected magic, retiring `PlyReader`'s endian fork.
- typed column read: a vertex block is `Header.Vertex` -> `ElementData["x"]`/`["y"]`/`["z"]` whose `PropertyData.Data` is a `Float32`-typed `float[]`; the face block is `Header.Face` -> `ElementData["vertex_indices"]` whose `Property.IsListProperty` is true and whose `Data` is the jagged index array — exactly the `Face + vertex_indices list` shape the pin admits.
- streaming bound: `Parse(stream, maxChunkSize)` decodes the body in bounded chunks so a multi-million-vertex scan PLY never inflates a single contiguous buffer; the header-only `ParseHeader` path lets the FORMAT `Detect` row classify a candidate file by element roster without decoding the body.
- mesh-text sibling split: `Ply.Net` owns ONLY PLY; OBJ/STL/OFF stay on `geometry3Sharp`, glTF on `SharpGLTF`, FBX/Collada/3MF on `AssimpNetter`. One `InterchangeCodec` row per format; no codec reaches across the MeshText leg boundary.
- compute seam handoff: the decoded `float[]` vertex array and jagged index array feed the `Model/exchange#TESSELLATION` `TessellationRequest` hop to the Compute companion rail directly — the typed `System.Array` is the buffer, never re-boxed into a parser DTO before crossing the wire.

[LOCAL_ADMISSION]:
- `Ply.Net` decodes and inspects PLY bytes only; it carries no mesh algebra, no normal/tangent generation, and no write-side encoder.
- Frame normalization, unit coercion, and the canonical `BimElement` projection are downstream FORMAT/IMPORT concerns, never the codec's.
- `Format`, per-`Element` count, and per-`Property` `DataType` are receipt facts the IMPORT fold records.

[RAIL_LAW]:
- Package: `Ply.Net`
- Owns: managed PLY ascii/binary read decode
- Accept: the MeshText `InterchangeCodec` PLY import leg
- Reject: OBJ/STL/glTF/FBX/3MF formats (sibling codecs), mesh-processing algebra, PLY write
