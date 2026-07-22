# [RASM_FABRICATION_API_DSTV_NET]

`DSTV.Net` owns the DSTV / NC1 (Tekla / NC) steel-fabrication READ leg under the Fabrication cut-program owner: `DstvReader.ParseAsync` lowers an `.nc1` profile-cut program — the `ST` steel-piece header with the hole (`BO`), slot, cut (`SC`), bend (`KA`), numeration (`SI`), and contour (`AK`/`IK`/`KO`/`PU`) feature blocks — into an immutable `IDstv` record tree, and every failure is a `ParseException` subtype carrying the source `LineNumber`. It reads only: `.nc1` emission is the Fabrication posting owner's own `PostDialect` writer, and `ToSvg()` is a debug preview, never a drafting rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DSTV.Net`
- package: `DSTV.Net` (`Apache-2.0`)
- assembly: `DSTV.Net`
- namespace: `DSTV.Net.Contracts`, `DSTV.Net.Data`, `DSTV.Net.Enums`, `DSTV.Net.Exceptions`, `DSTV.Net.Implementations`
- asset: pure-managed AnyCPU IL, no native or RID burden; the `net10.0` consumer binds `lib/net9.0/DSTV.Net.dll`
- rail: cut-program (DSTV/NC1 read leg)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: parse contracts — `DSTV.Net.Contracts`

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]      | [CAPABILITY]                                                                        |
| :-----: | :------------ | :----------------- | :---------------------------------------------------------------------------------- |
|  [01]   | `IDstvReader` | reader contract    | `ParseAsync(string)` / `ParseAsync(TextReader)` → `Task<IDstv>`                     |
|  [02]   | `IDstv`       | document contract  | parsed program: `Header` (`IDstvHeader?`) + `Elements` (`IEnumerable<DstvElement>`) |
|  [03]   | `IDstvHeader` | header contract    | the steel-piece `ST`-block descriptor                                               |
|  [04]   | `ISplitter`   | tokenizer contract | `string[] Split(string)` — a DSTV line-field splitting strategy                     |

[PUBLIC_TYPE_SCOPE]: located-element record tree — `DSTV.Net.Data`

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY]   | [CAPABILITY]                                                                                 |
| :-----: | :----------------- | :-------------- | :------------------------------------------------------------------------------------------- |
|  [01]   | `DstvElement`      | record base     | feature record base; virtual `ToSvg()`                                                       |
|  [02]   | `LocatedElement`   | located base    | abstract `(FlCode, XCoord, YCoord)` flange + coordinate, `Deconstruct`                       |
|  [03]   | `DstvHole`         | feature record  | `(Diameter, Depth)` round hole (`BO` block); `CreateHole(string)`                            |
|  [04]   | `DstvSlot`         | feature record  | `(SlotLength, SlotWidth, SlotAngle)` slotted hole — a `DstvHole` subtype                     |
|  [05]   | `DstvCut`          | feature record  | flame/saw cut (`SC` block); `CreateCut(string)`                                              |
|  [06]   | `DstvBend`         | feature record  | bend line (`KA` block); `CreateBend(string)`                                                 |
|  [07]   | `DstvNumeration`   | feature record  | part-mark / numeration (`SI` block); `CreateNumeration(string)`                              |
|  [08]   | `DstvContourPoint` | contour vertex  | `(IsNotch, Radius)` outer/inner-contour vertex; `CreatePoint(string)`                        |
|  [09]   | `DstvSkewedPoint`  | contour vertex  | bevelled vertex `(FirstAngle/Blunting, SecondAngle/Blunting)` — a `DstvContourPoint` subtype |
|  [10]   | `Contour`          | contour record  | `ContourType` + `IReadOnlyList<DstvContourPoint>`; `CreateSeveralContours(points, type)`     |
|  [11]   | `DstvHeader`       | header record   | the `IDstvHeader` impl of the `ST`-block descriptor                                          |
|  [12]   | `DstvRecord`       | document record | the `IDstv` impl: `init` `Header` + `Elements`; whole-piece `ToSvg()`                        |

[PUBLIC_TYPE_SCOPE]: header fields — `IDstvHeader` / `DstvHeader`

| [INDEX] | [FIELD_GROUP]    | [FIELDS]                                                                                                         |
| :-----: | :--------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | identity         | `OrderIdentification`, `DrawingIdentification`, `PhaseIdentification`, `PieceIdentification`, `QuantityOfPieces` |
|  [02]   | profile          | `Profile`, `CodeProfile`, `SteelQuality`, `Length`, `SawLength`                                                  |
|  [03]   | section geometry | `ProfileHeight`, `FlangeWidth`, `FlangeThickness`, `WebThickness`, `Radius`                                      |
|  [04]   | end cuts         | `WebStartCut`, `WebEndCut`, `FlangeStartCut`, `FlangeEndCut`                                                     |
|  [05]   | mass / surface   | `WeightByMeter`, `PaintingSurfaceByMeter`                                                                        |
|  [06]   | free text        | `Text1InfoOnPiece` … `Text4InfoOnPiece`                                                                          |

[PUBLIC_TYPE_SCOPE]: DSTV vocabularies — `DSTV.Net.Enums`

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                                              |
| :-----: | :------------ | :------------ | :---------------------------------------------------------------------------------------- |
|  [01]   | `CodeProfile` | profile enum  | `I`/`L`/`U`/`B`(plate)/`RU`(round)/`RO`(round tube)/`M`(rect. tube)/`C`/`T`/`SO`(special) |
|  [02]   | `ContourType` | contour enum  | `AK`(outer)/`IK`(inner)/`BO`/`SI`/`SC`/`KA`/`KO`/`PU`/`None` block-code discriminant      |

Every `CodeProfile` member carries a `[Description]` label; the enum value is the canonical discriminant.

[PUBLIC_TYPE_SCOPE]: typed parse-error rail — `DSTV.Net.Exceptions`

| [INDEX] | [SYMBOL]                                         | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------- | :----------------------------------------------- |
|  [01]   | `ParseException`                                 | abstract base; `int? LineNumber` source location |
|  [02]   | `DstvParseException`                             | general DSTV structural parse failure            |
|  [03]   | `MissingStartOfFileException`                    | absent `ST` start-of-file block                  |
|  [04]   | `UnexpectedCharacterException`                   | malformed token character                        |
|  [05]   | `UnexpectedEndException`                         | premature end of program                         |
|  [06]   | `IntegerParseException` / `DoubleParseException` | numeric field coercion failure                   |
|  [07]   | `EnumParseException<TEnum>`                      | unknown `CodeProfile`/`ContourType` code         |
|  [08]   | `TupleParseException<TType>`                     | malformed coordinate/field tuple                 |
|  [09]   | `FreeTextTooLargeException`                      | over-length `TextNInfoOnPiece` field             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: async parse — `DstvReader` (`IDstvReader`)

| [INDEX] | [SURFACE]                | [SHAPE]  | [CAPABILITY]                                                      |
| :-----: | :----------------------- | :------- | :---------------------------------------------------------------- |
|  [01]   | `new DstvReader()`       | ctor     | the sealed default reader (`DSTV.Net.Implementations.DstvReader`) |
|  [02]   | `ParseAsync(string)`     | instance | parse an in-memory `.nc1` string → `Task<IDstv>`                  |
|  [03]   | `ParseAsync(TextReader)` | instance | parse a streamed `TextReader` (file/network) → `Task<IDstv>`      |

[ENTRYPOINT_SCOPE]: result navigation — `IDstv` / `DstvRecord`

| [INDEX] | [SURFACE]                       | [SHAPE]  | [CAPABILITY]                                                |
| :-----: | :------------------------------ | :------- | :---------------------------------------------------------- |
|  [01]   | `IDstv.Header` (`IDstvHeader?`) | property | the steel-piece descriptor (null on a header-less fragment) |
|  [02]   | `IDstv.Elements`                | property | lazy `IEnumerable<DstvElement>` — pattern-match per subtype |
|  [03]   | `Contour.PointList` / `.Points` | property | `IReadOnlyList<DstvContourPoint>` outer/inner boundary      |
|  [04]   | `DstvElement.ToSvg()`           | instance | per-feature / whole-piece SVG string (debug preview)        |

[ENTRYPOINT_SCOPE]: record factories — `DSTV.Net.Data`. Each `Create*` is a static factory parsing one DSTV data line into the typed record — the seam a custom ingress reuses without re-tokenizing.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------- | :----------------------------------------- |
|  [01]   | `DstvHole.CreateHole(string)`                        | parse a `BO` hole line                     |
|  [02]   | `DstvCut.CreateCut(string)`                          | parse an `SC` cut line                     |
|  [03]   | `DstvBend.CreateBend(string)`                        | parse a `KA` bend line                     |
|  [04]   | `DstvNumeration.CreateNumeration(string)`            | parse an `SI` numeration line              |
|  [05]   | `DstvContourPoint.CreatePoint(string)`               | parse an `AK`/`IK` contour vertex line     |
|  [06]   | `Contour.CreateSeveralContours(points, ContourType)` | group vertices into closed contour records |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `DstvReader.ParseAsync` is the ONE ingress, async over both an in-memory `string` and a streamed `TextReader`, so a file-backed parse never pre-buffers the whole program; the result is an immutable `init`-only `record` tree consumers pattern-match by `DstvElement` subtype, never mutate.
- `CodeProfile` and `ContourType` are the DSTV block-code discriminants; the `[Description]` metadata is the human label, the enum value the canonical discriminant.
- numeric/enum/tuple coercion failures are distinct `ParseException` cases (`DoubleParseException`/`IntegerParseException`/`EnumParseException<TEnum>`/`TupleParseException<TType>`), so a diagnostic names the exact malformed field class without string-matching a message.

[STACKING]:
- `Clipper2`(`.api/api-clipper2`): parsed `DstvHole`/`DstvSlot`/`Contour` `(XCoord, YCoord)` flange coordinates map to `Path64`/`PathD` — contour boundaries become subject paths and holes/slots clip paths for the true-shape `Nesting/nfp` pack and the `Documentation/projection` screen clip; DSTV.Net delivers read geometry, `Clipper2` owns the polygon algebra.
- `System.IO.Hashing`(`.api/api-hashing`): `XxHash128.HashToUInt128` content-addresses a parsed piece over its canonical header+feature bytes, so an identical `.nc1` re-parse keys to the same `Stock`/part identity for the nesting/remnant lineage.
- within-lib: the per-block `Create*` factories are the seam a custom Fabrication ingress reuses without re-tokenizing; records project into the kernel `Loop`/polygon and the `Process/family` profile/feature vocabulary at ingress.

[LOCAL_ADMISSION]:
- DSTV.Net is READ-ONLY with no `.nc1` writer; the Fabrication cut-program owner emits DSTV/NC1 through its own `PostDialect` writer beside the G-code AST, mirroring the `IDstvHeader`/feature vocabulary DSTV.Net supplies.
- boundary-map at the `DstvElement`/`IDstvHeader` seam: project records into the kernel `Loop`/polygon and `Process/family` vocabulary at ingress, never thread DSTV.Net record types into the toolpath/nesting kernels.
- `await` `ParseAsync` at the boundary and convert `Task<IDstv>` → `Fin<IDstv>`, catching the abstract `ParseException` once and lowering its `LineNumber` into the `FabricationFault` rail; the typed rail is the failure source, never a sentinel or null `Header`.

[RAIL_LAW]:
- Package: `DSTV.Net`
- Owns: DSTV / NC1 (Tekla / NC) steel-profile cut-program PARSING — the `ST` header descriptor and the hole/slot/cut/bend/numeration/contour feature record tree
- Accept: `DstvReader.ParseAsync` over `string`/`TextReader`; the immutable `IDstv`/`DstvElement` tree; the typed `ParseException` (`LineNumber`) rail folded into `Fin`/`FabricationFault`; the `Clipper2` polygon-algebra and `XxHash128` content-identity seams
- Reject: DSTV.Net as a writer (emission is the posting owner's `PostDialect`); `ToSvg()` as a drafting rail; exception escape past the boundary; threading DSTV record types past the ingress boundary-map
