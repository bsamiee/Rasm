# [RASM_FABRICATION_API_DSTV_NET]

`DSTV.Net` is the DSTV / NC1 (Tekla / NC) steel-fabrication exchange READER beside the neutral G-code AST under the Fabrication cut-program owner. It parses a DSTV `.nc1` profile-cut program — the `ST` header block plus the hole (`BO`), numeration (`SI`), cut (`SC`), bend (`KA`), and contour (`AK`/`IK`/`KO`/`PU`) feature blocks — into an immutable `IDstv` record tree: a rich `IDstvHeader` steel-piece descriptor (profile code, section dimensions, end cuts, weight, steel grade, piece/order identifications) and an `IEnumerable<DstvElement>` of holes, slots, cuts, bends, numerations, and contours, every located element carrying a flange `FlCode` and `(XCoord, YCoord)`. Parsing is async (`ParseAsync` over a `string` or `TextReader`) and fails through a typed `ParseException` hierarchy that carries the source `LineNumber`, so the ingress lowers cleanly into the Fabrication `Fin`/`Validation` boundary and the band-2500 `FabricationFault` rail. The parsed holes/slots/contours map to the `Clipper2` (`api-clipper2`) polygon-algebra floor for nesting and projection and are content-addressed by `System.IO.Hashing` `XxHash128` (`api-hashing`); DSTV/NC1 EMISSION is the Fabrication posting owner's own dialect writer (DSTV.Net ships no writer — only a debug `ToSvg()` preview).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DSTV.Net`
- package: `DSTV.Net`
- version: `1.3.0` (centrally pinned)
- license: `Apache-2.0`
- assembly: `DSTV.Net`
- namespace roots: `DSTV.Net.Contracts`, `DSTV.Net.Data`, `DSTV.Net.Enums`, `DSTV.Net.Exceptions`, `DSTV.Net.Implementations`, `DSTV.Net.Extensions`, `DSTV.Net`
- asset: pure-managed AnyCPU IL, multi-target `net9.0` / `net8.0` / `net7.0` / `net6.0` / `netstandard2.0` (no native asset, no RID burden); the `net10.0` consumer binds `lib/net9.0/DSTV.Net.dll`
- rail: cut-program (DSTV/NC1 read leg)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parse contracts — `DSTV.Net.Contracts`
- rail: cut-program

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [CAPABILITY]                                                          |
| :-----: | :-------------- | :---------------- | :------------------------------------------------------------------ |
|  [01]   | `IDstvReader`   | reader contract   | `ParseAsync(string)` / `ParseAsync(TextReader)` → `Task<IDstv>`      |
|  [02]   | `IDstv`         | document contract | parsed program: `Header` (`IDstvHeader?`) + `Elements` (`IEnumerable<DstvElement>`) |
|  [03]   | `IDstvHeader`   | header contract   | the steel-piece `ST`-block descriptor (25 typed fields)             |
|  [04]   | `ISplitter`     | tokenizer contract| `string[] Split(string)` — a DSTV line-field splitting strategy     |

[PUBLIC_TYPE_SCOPE]: located-element record tree — `DSTV.Net.Data`
- rail: cut-program
- All feature elements are immutable C# `record`s rooted at `DstvElement`; `LocatedElement` adds the `(FlCode, XCoord, YCoord)` flange-coordinate base with a `Deconstruct`. Inheritance is load-bearing: `DstvSlot` IS a `DstvHole`, `DstvSkewedPoint` IS a `DstvContourPoint`.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]       | [CAPABILITY]                                                              |
| :-----: | :------------------ | :------------------ | :---------------------------------------------------------------------- |
|  [01]   | `DstvElement`       | record root         | abstract feature base; virtual `ToSvg()`                                |
|  [02]   | `LocatedElement`    | located base        | `(FlCode, XCoord, YCoord)` flange + coordinate, `Deconstruct`           |
|  [03]   | `DstvHole`          | feature record      | `(Diameter, Depth)` round hole (`BO` block); `CreateHole(string)`       |
|  [04]   | `DstvSlot`          | feature record      | `(SlotLength, SlotWidth, SlotAngle)` slotted hole — a `DstvHole` subtype |
|  [05]   | `DstvCut`           | feature record      | flame/saw cut (`SC` block); `CreateCut(string)`                         |
|  [06]   | `DstvBend`          | feature record      | bend line (`KA` block); `CreateBend(string)`                            |
|  [07]   | `DstvNumeration`    | feature record      | part-mark / numeration (`SI` block); `CreateNumeration(string)`         |
|  [08]   | `DstvContourPoint`  | contour vertex      | `(IsNotch, Radius)` outer/inner-contour vertex; `CreatePoint(string)`   |
|  [09]   | `DstvSkewedPoint`   | contour vertex      | bevelled vertex `(FirstAngle/Blunting, SecondAngle/Blunting)` — a `DstvContourPoint` subtype |
|  [10]   | `Contour`           | contour record      | `ContourType` + `IReadOnlyList<DstvContourPoint>`; `CreateSeveralContours(points, type)` |
|  [11]   | `DstvHeader`        | header record       | the `IDstvHeader` impl (the 25 `ST`-block fields)                       |
|  [12]   | `DstvRecord`        | document record     | the `IDstv` impl: `init` `Header` + `Elements`; whole-piece `ToSvg()`   |

[PUBLIC_TYPE_SCOPE]: header fields — `IDstvHeader` / `DstvHeader`
- rail: cut-program

| [INDEX] | [FIELD_GROUP]        | [FIELDS]                                                                                  |
| :-----: | :------------------- | :--------------------------------------------------------------------------------------- |
|  [01]   | identity             | `OrderIdentification`, `DrawingIdentification`, `PhaseIdentification`, `PieceIdentification`, `QuantityOfPieces` |
|  [02]   | profile              | `Profile`, `CodeProfile`, `SteelQuality`, `Length`, `SawLength`                           |
|  [03]   | section geometry     | `ProfileHeight`, `FlangeWidth`, `FlangeThickness`, `WebThickness`, `Radius`               |
|  [04]   | end cuts             | `WebStartCut`, `WebEndCut`, `FlangeStartCut`, `FlangeEndCut`                              |
|  [05]   | mass / surface       | `WeightByMeter`, `PaintingSurfaceByMeter`                                                 |
|  [06]   | free text            | `Text1InfoOnPiece` … `Text4InfoOnPiece`                                                   |

[PUBLIC_TYPE_SCOPE]: DSTV vocabularies — `DSTV.Net.Enums`
- rail: cut-program

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :------------ | :------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | `CodeProfile` | profile enum  | `I`/`L`/`U`/`B`(plate)/`RU`(round)/`RO`(round tube)/`M`(rect. tube)/`C`/`T`/`SO`(special), each `[Description]`-tagged |
|  [02]   | `ContourType` | contour enum  | `AK`(outer)/`IK`(inner)/`BO`/`SI`/`SC`/`KA`/`KO`/`PU`/`None` block-code discriminant      |

[PUBLIC_TYPE_SCOPE]: typed parse-error rail — `DSTV.Net.Exceptions`
- rail: cut-program
- `ParseException` is the abstract base (`: Exception`) carrying `int? LineNumber` from the reader context; every concrete case is a `ParseException` subtype, so the boundary catches one base and folds `LineNumber` into the `FabricationFault` diagnostic.

| [INDEX] | [SYMBOL]                       | [CAPABILITY]                                          |
| :-----: | :----------------------------- | :--------------------------------------------------- |
|  [01]   | `ParseException`               | abstract base; `LineNumber` source location          |
|  [02]   | `DstvParseException`           | general DSTV structural parse failure                |
|  [03]   | `MissingStartOfFileException`  | absent `ST` start-of-file block                       |
|  [04]   | `UnexpectedCharacterException` | malformed token character                            |
|  [05]   | `UnexpectedEndException`       | premature end of program                             |
|  [06]   | `IntegerParseException` / `DoubleParseException` | numeric field coercion failure       |
|  [07]   | `EnumParseException`           | unknown `CodeProfile`/`ContourType` code             |
|  [08]   | `TupleParseException`          | malformed coordinate/field tuple                     |
|  [09]   | `FreeTextTooLargeException`    | over-length `TextNInfoOnPiece` field                 |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: async parse — `DstvReader` (`IDstvReader`)
- rail: cut-program
- The sealed concrete reader is `DSTV.Net.Implementations.DstvReader`; the `ISplitter` strategies (`RoughSplitter`/`FineSplitter`/`DotSplitter`/`PositionNumericSplitter`) and the block sub-readers (`HeaderReader`/`BodyReader`/`ReaderContext`) live in the same `Implementations` namespace — `IDstvReader`/`IDstv`/`ISplitter` are the contracts a custom ingress composes against.

| [INDEX] | [SURFACE]                                | [ENTRY_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :--------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `new DstvReader()`                       | constructor    | the sealed default reader (`DSTV.Net.Implementations.DstvReader : IDstvReader`) |
|  [02]   | `ParseAsync(string dstvData)`            | parse          | parse an in-memory `.nc1` string → `Task<IDstv>`             |
|  [03]   | `ParseAsync(TextReader reader)`          | parse          | parse a streamed `TextReader` (file/network) → `Task<IDstv>` |

[ENTRYPOINT_SCOPE]: result navigation — `IDstv` / `DstvRecord`
- rail: cut-program

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [CAPABILITY]                                                   |
| :-----: | :------------------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `IDstv.Header` (`IDstvHeader?`)        | accessor       | the steel-piece descriptor (null if header-less fragment)    |
|  [02]   | `IDstv.Elements` (`IEnumerable<…>`)    | accessor       | lazy feature sequence — pattern-match per record subtype     |
|  [03]   | `Contour.PointList` / `.Points`        | accessor       | `IReadOnlyList<DstvContourPoint>` outer/inner boundary       |
|  [04]   | `DstvElement.ToSvg()` / `DstvRecord.ToSvg()` | preview  | per-feature / whole-piece SVG string (debug/visual preview)  |

[ENTRYPOINT_SCOPE]: record factories — `DSTV.Net.Data`
- rail: cut-program
- The per-block `Create*(string)` factories parse one DSTV data line into the typed record — the unit the reader composes; they are the seam a custom ingress reuses without re-tokenizing.

| [INDEX] | [SURFACE]                                          | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `DstvHole.CreateHole(string)`                      | parse a `BO` hole line                        |
|  [02]   | `DstvCut.CreateCut(string)`                        | parse an `SC` cut line                        |
|  [03]   | `DstvBend.CreateBend(string)`                      | parse a `KA` bend line                        |
|  [04]   | `DstvNumeration.CreateNumeration(string)`          | parse an `SI` numeration line                 |
|  [05]   | `DstvContourPoint.CreatePoint(string)`             | parse an `AK`/`IK` contour vertex line        |
|  [06]   | `Contour.CreateSeveralContours(points, ContourType)` | group vertices into closed contour records  |

## [04]-[IMPLEMENTATION_LAW]

[INGRESS_PROFILE]:
- entry: `DstvReader.ParseAsync` is the ONE ingress; it is async over both an in-memory `string` and a streamed `TextReader`, so a file-backed parse never pre-buffers the whole program.
- model: the result is an immutable `record` tree — `DstvRecord(Header, Elements)` with `init`-only members; consumers pattern-match the `DstvElement` sequence by record subtype (`DstvHole`/`DstvSlot`/`DstvCut`/`DstvBend`/`Contour`/`DstvNumeration`), never mutate it.
- vocabulary: `CodeProfile` and `ContourType` are the DSTV block codes; their `[Description]` metadata is the human label, the enum value the canonical discriminant.

[ERROR_RAIL]:
- every failure is a `ParseException` subtype carrying `LineNumber`; catch the abstract base ONCE at the boundary and lower it into a `LanguageExt` `Fin`/`Validation` failure rather than letting it escape — the `ParseException.LineNumber` becomes the band-2500 `FabricationFault` source location.
- numeric/enum/tuple coercion failures are distinct cases (`DoubleParseException`/`IntegerParseException`/`EnumParseException`/`TupleParseException`), so a diagnostic can name the exact malformed field class without string-matching a message.

[STACKING_SEAM]:
- the parsed `DstvHole`/`DstvSlot`/`Contour` `(XCoord, YCoord)` flange coordinates map to `Clipper2` `Path64`/`PathD` (`api-clipper2`): contour boundaries become subject paths and holes/slots become clip paths for the true-shape `Nesting/nfp` pack and the `Posting/projection` screen clip — DSTV.Net delivers the read geometry, `Clipper2` owns the polygon algebra.
- a parsed piece is content-addressed for the nesting/remnant lineage by `System.IO.Hashing` `XxHash128.HashToUInt128` (`api-hashing`) over the canonical header+feature bytes, so an identical `.nc1` re-parse keys to the same `Stock`/part identity.
- `ToSvg()` is a DEBUG/preview projection only; it is NOT the drafting rail — DXF/DWG read is `ACadSharp` and DXF write is the `Rasm.AppUi`/`netDxf` leg, neither routed through DSTV.Net.

[LOCAL_ADMISSION]:
- DSTV.Net is READ-ONLY: it has no `.nc1` writer. The Fabrication cut-program owner EMITS DSTV/NC1 through its own `PostDialect` writer beside the G-code AST; DSTV.Net supplies the inbound read model and the `IDstvHeader`/feature vocabulary the emitter mirrors, never the emission itself.
- boundary-map at the `DstvElement`/`IDstvHeader` seam: project the records into the kernel `Loop`/polygon and the `Process/family` profile/feature vocabulary at ingress; do not thread DSTV.Net record types deep into the toolpath/nesting kernels.
- `await` the `ParseAsync` task at the boundary and convert `Task<IDstv>` → `Fin<IDstv>`; the typed `ParseException` rail is the failure source, not a sentinel/null `Header`.

[RAIL_LAW]:
- Package: `DSTV.Net`
- Owns: DSTV / NC1 (Tekla / NC) steel-profile cut-program PARSING — the `ST` header descriptor and the hole/slot/cut/bend/numeration/contour feature record tree
- Accept: `DstvReader.ParseAsync` ingress over `string`/`TextReader`; the immutable `IDstv`/`DstvElement` record tree; the typed `ParseException` (`LineNumber`) rail folded into `Fin`/`FabricationFault`; the `Clipper2` polygon-algebra (`api-clipper2`) and `XxHash128` content-identity (`api-hashing`) seams
- Reject: treating DSTV.Net as a WRITER (it parses only; emission is the posting owner's `PostDialect`); `ToSvg()` as a drafting rail (use `ACadSharp`/`netDxf` for CAD); exception-escape past the boundary (lower `ParseException` into `Fin`); threading DSTV record types past the ingress boundary-map
