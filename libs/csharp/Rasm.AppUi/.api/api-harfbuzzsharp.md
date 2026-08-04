# [RASM_APPUI_API_HARFBUZZSHARP]

`HarfBuzzSharp` is the managed HarfBuzz binding: a `Blob` of face bytes builds a `Face`, a `Face` builds a scaled `Font`, and that `Font` shapes a `Buffer` of text into glyph ids, clusters, and advances under an explicit `Direction`/`Script`/`Language`/`ClusterLevel` segment policy plus an OpenType `Feature` set. It is the control altitude beneath `SkiaSharp.HarfBuzz`: the bridge's `SKShaper` reaches the same types with fixed segment properties and an empty feature array, so a run needing pinned script, feature values, or cluster policy binds these members directly. The assembly carries no glyph data — every face arrives through a caller-owned stream — and every call P/Invokes the `libHarfBuzzSharp` payload `api-harfbuzz-native.md` places.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HarfBuzzSharp`
- package: `HarfBuzzSharp` (MIT)
- floor: `net10.0` consumer (`lib/net10.0/HarfBuzzSharp.dll`)
- assembly: `HarfBuzzSharp`
- namespace: `HarfBuzzSharp`
- depends: no managed dependency; `SkiaSharp.HarfBuzz` carries the same assembly in its closure, so the bridge and a direct consumer never bind two shaping surfaces
- native: `libHarfBuzzSharp` (`api-harfbuzz-native.md`) — every member faults at first call on a missing-RID asset, never at compile
- rail: typography

## [02]-[PUBLIC_TYPES]

[FACE_TYPES]: the face-to-font admission chain, each an owned native handle released in reverse construction order

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :---------------------- | :------------ | :--------------------------------------- |
|  [01]   | `Blob : IDisposable`    | class         | immutable face bytes over a lease        |
|  [02]   | `Face : IDisposable`    | class         | parsed face, collection index selectable |
|  [03]   | `Font : IDisposable`    | class         | scaled shaping font, the shape receiver  |

[SHAPING_TYPES]: the buffer and the segment-policy vocabulary a deterministic run pins

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :--------------------- | :------------ | :--------------------------------------------- |
|  [01]   | `Buffer : IDisposable` | class         | text in, glyph run out, per-shape scope        |
|  [02]   | `Direction`            | enum          | `LeftToRight`/`RightToLeft` run axis           |
|  [03]   | `Script`               | value struct  | ISO-15924 script rows (`Latin`, `Arabic`, …)   |
|  [04]   | `Language`             | value struct  | BCP-47 language, `Default` the process culture |
|  [05]   | `ClusterLevel`         | enum          | `MonotoneGraphemes` cluster-merge policy       |

[GLYPH_TYPES]: the shaped-run payload, read as spans and never as the recopying array properties

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :-------------- | :------------ | :----------------------------------------------- |
|  [01]   | `GlyphInfo`     | value struct  | `Codepoint` glyph id, `Cluster` source index     |
|  [02]   | `GlyphPosition` | value struct  | `XAdvance`/`YAdvance`, `XOffset`/`YOffset`       |
|  [03]   | `GlyphFlags`    | enum          | `UnsafeToBreak` re-shape boundary evidence       |
|  [04]   | `Feature`       | value struct  | one OpenType feature, whole-run or range-scoped  |
|  [05]   | `Tag`           | value struct  | four-character OpenType tag, `uint` on both ways |

## [03]-[ENTRYPOINTS]

[FACE_ENTRYPOINTS]: admit a face once per typeface and scale its font once; the design scale is the divisor every advance rescales through
- root: `Blob` -> `Face` -> `Font`

| [INDEX] | [SURFACE]       | [CALL]                                                                    |
| :-----: | :-------------- | :------------------------------------------------------------------------ |
|  [01]   | `Face`          | `(Blob, int index)` collection index selects the face inside a `.ttc`     |
|  [02]   | `MakeImmutable` | `()` on `Face` — freezes the face before any font binds it                |
|  [03]   | `Font`          | `(Face)` binds one shaping font to the frozen face                        |
|  [04]   | `SetScale`      | `(int x, int y)` on `Font` — the integer design scale advances divide by  |
|  [05]   | `Shape`         | `(Buffer, Feature[])` on `Font` — the one native shape call               |

[BUFFER_ENTRYPOINTS]: the ordered buffer protocol — add text, pin the segment properties, shape, then read the spans
- root: `Buffer`

| [INDEX] | [SURFACE]                | [CALL]                                                                       |
| :-----: | :----------------------- | :--------------------------------------------------------------------------- |
|  [01]   | `AddUtf16`               | `(string)` whole-string ingress                                              |
|  [02]   | `AddUtf16`               | `(string, int itemOffset, int itemLength)` window ingress over live context  |
|  [03]   | `AddUtf8`                | `(string)` the UTF-8 ingress `SKShaper` takes                                |
|  [04]   | `Direction`              | settable run axis; unset drifts through `GuessSegmentProperties`             |
|  [05]   | `Script`                 | settable script row                                                          |
|  [06]   | `Language`               | settable language row; a locale changes the glyph stream for one text        |
|  [07]   | `ClusterLevel`           | settable cluster-merge policy, fixed before shaping                          |
|  [08]   | `Flags`                  | `BeginningOfText`/`EndOfText` edge contract for a windowed run               |
|  [09]   | `GuessSegmentProperties` | `()` infers direction, script, and language — drifts at mixed-script margins |
|  [10]   | `SerializeGlyphs`        | `()` textual shaped-run dump, the shaping-evidence channel                   |

[GLYPH_ENTRYPOINTS]: parallel reads index 1:1 by glyph; the span accessors read the native buffer in place
- root: `Buffer`

| [INDEX] | [SURFACE]                | [CALL]                                                                    |
| :-----: | :----------------------- | :------------------------------------------------------------------------ |
|  [01]   | `GetGlyphInfoSpan`       | `() -> ReadOnlySpan<GlyphInfo>` zero-allocation glyph ids and clusters    |
|  [02]   | `GetGlyphPositionSpan`   | `() -> ReadOnlySpan<GlyphPosition>` zero-allocation advances and offsets  |
|  [03]   | `GlyphInfos`             | `GlyphInfo[]` — recopies the whole run on every access                    |
|  [04]   | `GlyphPositions`         | `GlyphPosition[]` — recopies the whole run on every access                |

[FEATURE_ENTRYPOINTS]: mint an OpenType feature; the ctor arity is the scope, and `Tag` admission throws rather than reporting
- root: `Feature`, `Tag`

| [INDEX] | [ROOT]    | [CALL]                                                                          |
| :-----: | :-------- | :------------------------------------------------------------------------------ |
|  [01]   | `Feature` | `(Tag)` feature over the whole run at its default value                         |
|  [02]   | `Feature` | `(Tag, uint value)` whole-run feature at an explicit value                      |
|  [03]   | `Feature` | `(Tag, uint value, uint start, uint end)` value scoped to a cluster window      |
|  [04]   | `Feature` | `Tag`/`Value`/`Start`/`End` settable slots on the built feature                 |
|  [05]   | `Tag`     | `Parse(string) -> Tag` static admission — THROWS on a bad tag, no try-form      |
|  [06]   | `Tag`     | `(char, char, char, char)` literal four-character mint, no parse                |
|  [07]   | `Tag`     | implicit `Tag` <-> `uint` both directions — the tag IS its packed integer       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The chain is one-directional and lifetime-ordered — a `Blob` borrows a caller-owned stream, a `Face` parses the blob, a `Font` binds the frozen face — so releasing forward strands the survivors and a blob outliving its backing stream reads valid bytes until the stream frees.
- `Font.Shape` writes glyph ids, clusters, advances, and offsets into the passed `Buffer`, so the buffer is the result carrier and a second shape over one buffer without a reset compounds the prior run.
- Advances leave in the integer design scale `SetScale` fixed, so every position rescales by `raster size / design scale`; HarfBuzz shaping space is y-up while the canvas is y-down, so the vertical projection negates.
- `Tag` is a packed four-byte integer with implicit conversions both ways, so a tag literal, a parsed tag, and a `uint` constant are one value; `Parse` is the only string ingress and it throws, so the string boundary is the caller's rail.

[STACKING]:
- `SkiaSharp.HarfBuzz`(`api-skia-harfbuzz.md`): the bridge's `SKShaper` composes exactly these types with `GuessSegmentProperties` and an empty feature array, and its `BlobExtensions.ToHarfBuzzBlob` is the one admitted `SKStreamAsset`-to-`Blob` hop while `FontExtensions.GetScale`/`SetScale` carry the `SKSizeI` form of the design scale — a run needing pinned segment properties, feature values, or a cluster policy drops from `SKShaper` to `Font.Shape(Buffer, Feature[])` and keeps the bridge for the blob and scale hops.
- `SkiaSharp`(`api-skiasharp.md`): the shaped spans feed `SKTextBlobBuilder.AllocateRawPositionedRun`, casting `GlyphInfo.Codepoint` to `ushort` for the glyph span and projecting `GlyphPosition` into the point span, and the built `SKTextBlob` draws through `SKCanvas.DrawTextBlob`.
- `api-harfbuzz-native.md`: every member here P/Invokes the RID-selected `libHarfBuzzSharp` those packages place, so a missing native surfaces as a first-shape load fault and the load identity records once into the typography evidence stream.
- within-lib: `Theme/typography#SHAPING_RAIL` owns the one shaping rail — `FaceHandle` is the once-per-face `Blob`/`Face`/`Font` capsule, `FeatureAdmission` the one `Tag.Parse`-to-`Feature` mint over both scopes, and `RunSpec` the pinned `Direction`/`Script`/`Language`/`ClusterLevel` row `Theme/locale#LOCALE_AXIS` supplies per culture.

[LOCAL_ADMISSION]:
- A face admits once per typeface and its capsule holds the stream, blob, face, and font for the capsule's whole life; a per-draw face build reloads the font bytes at draw rate.
- `Tag.Parse` throws, so tag admission traps at one owner and yields a typed rail value; a tag string reaching `Feature` unparsed is the escape this admission exists to close.
- The span accessors are the read form; the array properties recopy the whole run on every access and admit only a diagnostic dump.

[RAIL_LAW]:
- Package: `HarfBuzzSharp`
- Owns: the control-altitude shaping surface — face admission, the pinned segment-property buffer protocol, the OpenType feature set, and the zero-allocation shaped-run read
- Accept: one capsule per face, an explicitly pinned `RunSpec` on every deterministic run, `Feature` values minted through one tag admission, and the span accessors for the glyph read
- Reject: `GuessSegmentProperties` on a deterministic or evidence-bearing run, pre-slicing a string where the item-window ingress preserves cross-boundary joining forms, the array glyph properties on a shaping path, a blob outliving its backing stream, and a raw `Feature` or unparsed tag constructed at a call site
