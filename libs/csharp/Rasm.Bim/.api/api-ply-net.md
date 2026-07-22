# [RASM_BIM_API_PLY_NET]

`Ply.Net` owns pure-managed PLY decode: the static `PlyParser` container nests an immutable record graph — `Header` describes the file, `Dataset` carries the lazily-streamed decoded payload — and every property value materializes once as a typed `System.Array` reachable by name, grounding the `ply-net` interchange codec's import leg. `ParseHeader` classifies a candidate by element roster without touching the body; `Parse` decodes header-plus-body in bounded chunks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Ply.Net`
- package: `Ply.Net` (Apache-2.0, aardvark-platform; `licenseUrl` form, no SPDX expression)
- assembly: `Ply.Net`
- namespace: `Ply.Net`
- asset: netstandard2.0 single TFM; the net10 consumer binds `lib/netstandard2.0` — IL-only AnyCPU, no `runtimes/` folder, ALC-safe
- depends: single BCL transitive `System.Collections.Immutable`; zero third-party managed deps
- rail: interchange

## [02]-[PUBLIC_TYPES]

`PlyParser` nests the record graph and codec enums as a static container; every symbol below qualifies as `PlyParser.<Symbol>`.

[PUBLIC_TYPE_SCOPE]: header-descriptor record graph

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :--------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `Header`   | record        | file descriptor: `Format`, ordered `Element` list, `HeaderLines`, body `DataOffset` |
|  [02]   | `Element`  | record        | one element block: `ElementType`, name, row `Count`, `Property` column list         |
|  [03]   | `Property` | record        | one column: `DataType`, name, `ListCountType` list-column width                     |

- `Header.Vertex`/`Face`/`Edge`/`Material`/`Cell`: resolve an `Element?` by `ElementType` via `SingleOrDefault`.
- `Element.ContainsListProperty` predicate; `Element.Add(Property)` returns a new `Element`.
- `Property.IsListProperty`: `ListCountType != DataType.Undefined` marks a `vertex_indices`-style list column.

[PUBLIC_TYPE_SCOPE]: decoded-payload record graph

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                                     |
| :-----: | :------------- | :------------ | :------------------------------------------------------------------------------- |
|  [01]   | `Dataset`      | record        | `Header` + lazily-streamed `ElementData` sequence, one per `Element`             |
|  [02]   | `ElementData`  | record        | one element's decoded columns; `this[string]` resolves a `PropertyData?` by name |
|  [03]   | `PropertyData` | record        | one column's `Property` + typed `System.Array`, materialized once                |

[PUBLIC_TYPE_SCOPE]: codec discriminants

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                      |
| :-----: | :------------ | :------------ | :------------------------------------------------ |
|  [01]   | `Format`      | enum          | binary/ascii encoding, read off `Header.Format`   |
|  [02]   | `DataType`    | enum          | per-column scalar width                           |
|  [03]   | `ElementType` | enum          | element-block kind; drives the `Header` accessors |

[FORMAT]: `Undefined` `Ascii` `BinaryLittleEndian` `BinaryBigEndian`
[DATA_TYPE]: `Undefined` `Int8` `UInt8` `Int16` `UInt16` `Int32` `UInt32` `Int64` `UInt64` `Float32` `Float64`
[ELEMENT_TYPE]: `Vertex` `Face` `Edge` `Material` `Cell` `UserDefined`

## [03]-[ENTRYPOINTS]

`PlyParser` is the sole static entrypoint; every overload also takes an optional `Action<string>? log` sink, elided below.

| [INDEX] | [SURFACE]                                 | [SHAPE] | [CAPABILITY]                                                    |
| :-----: | :---------------------------------------- | :------ | :-------------------------------------------------------------- |
|  [01]   | `PlyParser.ParseHeader(Stream) -> Header` | static  | header-only decode; leaves the stream at `Header.DataOffset`    |
|  [02]   | `PlyParser.ParseHeader(string) -> Header` | static  | header-only decode by path                                      |
|  [03]   | `PlyParser.Parse(Stream, int) -> Dataset` | static  | one-shot header+body decode; `int` bounds peak memory per chunk |
|  [04]   | `PlyParser.Parse(string, int) -> Dataset` | static  | one-shot decode by path                                         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Header` roots the file descriptor and `Dataset` roots the decoded payload; every column lands once as a typed `System.Array` at `PropertyData.Data`, reachable by name.
- `Dataset.Data` streams lazily over the parse stream, so a decode materializes it once before any `Header.Vertex`/`Face` lookup; a second enumeration re-reads the advanced stream and strands every column.
- Each column types to its matching `System.Array` — `float[]` for `Float32`, `double[]` for `Float64`, integer-width arrays for the integer types — and a `Face` `vertex_indices` list column decodes to a jagged `int[][]`.

[STACKING]:
- `Exchange/import#IMPORT_RAIL` `BimIo.Ply`: `PlyParser.Parse(stream, maxChunkSize)` decodes into `Dataset`, `Header.Vertex` x/y/z and `Header.Face` `vertex_indices` typed columns fan-triangulate onto the canonical triangle-soup `ImportedGeometry`; `ParseHeader` alone feeds `Detect` classification by element roster without a body decode.
- `Exchange/format#FORMAT_AXIS` `InterchangeCodec.Ply`: the `ply-net` codec row owns the extension-keyed codec map; `Ply.Net` grounds only the import-only `.ply` leg, every sibling format on its own codec row.

[LOCAL_ADMISSION]:
- `Ply.Net` decodes and inspects PLY bytes only — no mesh algebra, no normal/tangent generation, no write-side encoder.
- Frame normalization, unit coercion, and the canonical `BimElement` projection are downstream import concerns.
- `Format`, per-`Element` `Count`, and per-`Property` `DataType` are the receipt facts the import fold records.

[RAIL_LAW]:
- Package: `Ply.Net`
- Owns: managed PLY ascii/binary read decode over the immutable `Header`/`Dataset` record graph
- Accept: the `InterchangeCodec.Ply` (`ply-net`) import-only `.ply` leg
- Reject: sibling mesh/scene formats on their own codec rows, mesh-processing algebra, PLY write, a hand-rolled PLY endian/tokenizer parse
