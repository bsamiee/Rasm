# [RASM_APPUI_API_HARFBUZZSHARP]

`HarfBuzzSharp` is the managed HarfBuzz binding: a `Blob` of face bytes builds a `Face`, a `Face` builds a scaled `Font`, and that `Font` shapes a `Buffer` of text into glyph ids, clusters, and advances under an explicit `Direction`/`Script`/`Language`/`ClusterLevel` segment policy plus an OpenType `Feature` set. It is the control altitude beneath `SkiaSharp.HarfBuzz`: the bridge's `SKShaper` reaches the same types with fixed segment properties and an empty feature array, so a run needing pinned script, feature values, or cluster policy binds these members directly. The assembly carries no glyph data — every face arrives through a caller-owned stream — and every call P/Invokes the `libHarfBuzzSharp` payload `api-harfbuzz-native.md` places.

## [01]-[PUBLIC_TYPES]

[FACE_TYPES]: the face-to-font admission chain, each an owned native handle released in reverse construction order

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------- | :------------ | :--------------------------------------- |
|  [01]   | `Blob : IDisposable` | class         | immutable face bytes over a lease        |
|  [02]   | `Face : IDisposable` | class         | parsed face, collection index selectable |
|  [03]   | `Font : IDisposable` | class         | scaled shaping font, the shape receiver  |

[SHAPING_TYPES]: the buffer and the segment-policy vocabulary a deterministic run pins

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :--------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Buffer : IDisposable` | class         | text in, glyph run out, per-shape scope                                 |
|  [02]   | `Direction`            | enum          | `Invalid`/`LeftToRight`/`RightToLeft`/`TopToBottom`/`BottomToTop`       |
|  [03]   | `Script`               | value struct  | ISO-15924 rows plus `HorizontalDirection` and `Parse`/`TryParse`        |
|  [04]   | `Language`             | class         | BCP-47 language handle; `(CultureInfo)`/`(string)` ctors, `Name`        |
|  [05]   | `ClusterLevel`         | enum          | `MonotoneGraphemes`/`MonotoneCharacters`/`Characters` merge policy      |
|  [06]   | `BufferFlags`          | enum          | text-edge and ignorable policy for a windowed run                       |
|  [07]   | `ContentType`          | enum          | `Invalid`/`Unicode`/`Glyphs` buffer content state                       |
|  [08]   | `UnicodeFunctions`     | class         | script, general category, mirroring, combining class, compose/decompose |
|  [09]   | `SerializeFormat`      | enum          | `Text`/`Json` glyph-dump encoding                                       |
|  [10]   | `SerializeFlag`        | enum          | dump inclusion mask — clusters, positions, extents, glyph flags         |

[GLYPH_TYPES]: the shaped-run payload, read as spans and never as the recopying array properties

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `GlyphInfo`     | value struct  | `Codepoint` glyph id, `Cluster` source index, `Mask`, `GlyphFlags` |
|  [02]   | `GlyphPosition` | value struct  | `XAdvance`/`YAdvance`, `XOffset`/`YOffset`                         |
|  [03]   | `GlyphFlags`    | enum          | `UnsafeToBreak`/`Defined` re-shape boundary evidence               |
|  [04]   | `Feature`       | value struct  | one OpenType feature, whole-run or range-scoped                    |
|  [05]   | `Tag`           | value struct  | four-character OpenType tag, `uint` on both ways                   |
|  [06]   | `GlyphExtents`  | value struct  | per-glyph ink box in design units                                  |
|  [07]   | `FontExtents`   | value struct  | `Ascender`/`Descender`/`LineGap` in design units                   |

[FACE_METADATA_TYPES]: the variation, colour-palette, and metrics vocabulary a face publishes about itself

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                     |
| :-----: | :-------------------------- | :------------ | :--------------------------------------------------------------- |
|  [01]   | `OpenTypeVarAxisInfo`       | value struct  | `Tag`, `MinValue`/`DefaultValue`/`MaxValue`, `NameId`, `Flags`   |
|  [02]   | `Variation`                 | value struct  | one `(Tag, float Value)` axis setting                            |
|  [03]   | `OpenTypeColorPaletteFlags` | enum          | `Default`/`UsableWithLightBackground`/`UsableWithDarkBackground` |
|  [04]   | `HBColor`                   | value struct  | one palette entry colour                                         |
|  [05]   | `OpenTypeMetrics`           | class         | face metrics reader over `OpenTypeMetricsTag`                    |
|  [06]   | `OpenTypeMetricsTag`        | enum          | ascender, cap height, x-height, underline, strikeout, caret rows |

## [02]-[ENTRYPOINTS]

[FACE_ENTRYPOINTS]: admit a face once per typeface and scale its font once; the design scale is the divisor every advance rescales through
- root: `Blob` -> `Face` -> `Font`

| [INDEX] | [SURFACE]              | [CALL]                                                                          |
| :-----: | :--------------------- | :------------------------------------------------------------------------------ |
|  [01]   | `Face`                 | `(Blob, int index)` collection index selects the face inside a `.ttc`           |
|  [02]   | `MakeImmutable`        | `()` on `Face` — freezes the face before any font binds it                      |
|  [03]   | `UnitsPerEm`           | `int` on `Face` — the face's OWN design scale; `GlyphCount` and `Tables` beside |
|  [04]   | `Font`                 | `(Face)` binds one shaping font to the frozen face                              |
|  [05]   | `SetScale`             | `(int x, int y)` on `Font` — the integer design scale advances divide by        |
|  [06]   | `SetFunctionsOpenType` | `()` on `Font` — selects the OpenType metric and glyph functions                |
|  [07]   | `Shape`                | `(Buffer, params Feature[])` on `Font` — the one native shape call              |
|  [08]   | `Shape`                | `(Buffer, IReadOnlyList<Feature>, IReadOnlyList<string> shapers)` pinned arm    |

[FACE_METADATA_ENTRYPOINTS]: what a face publishes about its own variation axes, colour palettes, and metrics
- root: `Face`, `Font`

| [INDEX] | [SURFACE]                     | [ROOT] | [CALL]                                                                         |
| :-----: | :---------------------------- | :----- | :----------------------------------------------------------------------------- |
|  [01]   | `HasVariationData`            | `Face` | `bool`; `VariationAxisCount` and `VariationAxisInfos` enumerate the axes       |
|  [02]   | `TryFindVariationAxis`        | `Face` | `(Tag, out OpenTypeVarAxisInfo)` single-axis lookup                            |
|  [03]   | `NamedInstanceCount`          | `Face` | `int`; `GetNamedInstanceDesignCoords(int)` reads one instance's coordinates    |
|  [04]   | `HasPalettes` `PaletteCount`  | `Face` | colour-face palette presence and arity                                         |
|  [05]   | `GetPaletteFlags`             | `Face` | `(int index) -> OpenTypeColorPaletteFlags` background suitability              |
|  [06]   | `GetPaletteColors`            | `Face` | `(int index[, Span<HBColor>]) -> HBColor[]` one palette's entries              |
|  [07]   | `HasColorLayers`              | `Face` | `bool`; `HasColorPng`/`HasColorSvg` beside it for the other colour formats     |
|  [08]   | `SetVariations`               | `Font` | `(ReadOnlySpan<Variation>)`; design and normalized coordinate twins beside it  |
|  [09]   | `SetVariationNamedInstance`   | `Font` | `(int instanceIndex)` binds a shipped named instance                           |
|  [10]   | `OpenTypeMetrics`             | `Font` | `TryGetPosition(OpenTypeMetricsTag, out int)` face metric in design units      |
|  [11]   | `TryGetHorizontalFontExtents` | `Font` | `(out FontExtents)`; the vertical twin and `GetFontExtentsForDirection` beside |
|  [12]   | `TryGetNominalGlyph`          | `Font` | `(int unicode, out uint glyph)` coverage probe without a shape                 |

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
|  [11]   | `SerializeGlyphs`        | `(Font, SerializeFormat, SerializeFlag)` glyph-flag-bearing JSON dump        |
|  [12]   | `ClearContents` `Reset`  | `()` reuse without a fresh buffer / full property reset                      |

[UNICODE_ENTRYPOINTS]: the classification surface only the shaping library publishes; itemization reads script here
- root: `UnicodeFunctions.Default`, `Script`

| [INDEX] | [SURFACE]             | [ROOT]             | [CALL]                                                       |
| :-----: | :-------------------- | :----------------- | :----------------------------------------------------------- |
|  [01]   | `GetScript`           | `UnicodeFunctions` | `(int unicode) -> Script` per-codepoint script resolution    |
|  [02]   | `GetGeneralCategory`  | `UnicodeFunctions` | `(int) -> UnicodeGeneralCategory`                            |
|  [03]   | `GetMirroring`        | `UnicodeFunctions` | `(int) -> int` mirrored codepoint under a right-to-left run  |
|  [04]   | `GetCombiningClass`   | `UnicodeFunctions` | `(int) -> UnicodeCombiningClass`                             |
|  [05]   | `HorizontalDirection` | `Script`           | `Direction` the script reads in; `Invalid` for a neutral row |
|  [06]   | `Parse` / `TryParse`  | `Script`           | `(string) -> Script` / `(string, out Script) -> bool`        |

[GLYPH_ENTRYPOINTS]: parallel reads index 1:1 by glyph; the span accessors read the native buffer in place
- root: `Buffer`

| [INDEX] | [SURFACE]              | [CALL]                                                                   |
| :-----: | :--------------------- | :----------------------------------------------------------------------- |
|  [01]   | `GetGlyphInfoSpan`     | `() -> ReadOnlySpan<GlyphInfo>` zero-allocation glyph ids and clusters   |
|  [02]   | `GetGlyphPositionSpan` | `() -> ReadOnlySpan<GlyphPosition>` zero-allocation advances and offsets |
|  [03]   | `GlyphInfos`           | `GlyphInfo[]` — recopies the whole run on every access                   |
|  [04]   | `GlyphPositions`       | `GlyphPosition[]` — recopies the whole run on every access               |

[FEATURE_ENTRYPOINTS]: mint an OpenType feature; the constructor arity is the scope, and `Tag` admission COERCES rather than reporting
- root: `Feature`, `Tag`

| [INDEX] | [ROOT]    | [CALL]                                                                              |
| :-----: | :-------- | :---------------------------------------------------------------------------------- |
|  [01]   | `Feature` | `(Tag)` feature over the whole run at its default value                             |
|  [02]   | `Feature` | `(Tag, uint value)` whole-run feature at an explicit value                          |
|  [03]   | `Feature` | `(Tag, uint value, uint start, uint end)` value scoped to a cluster window          |
|  [04]   | `Feature` | `Tag`/`Value`/`Start`/`End` settable slots on the built feature                     |
|  [05]   | `Tag`     | `Parse(string) -> Tag` — SILENT: null or empty yields `None`, longer truncates to 4 |
|  [06]   | `Tag`     | `(char, char, char, char)` literal four-character mint, no parse                    |
|  [07]   | `Tag`     | implicit `Tag` <-> `uint` both directions — the tag IS its packed integer           |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- The chain is one-directional and lifetime-ordered — a `Blob` borrows a caller-owned stream, a `Face` parses the blob, a `Font` binds the frozen face — so releasing forward strands the survivors and a blob outliving its backing stream reads valid bytes until the stream frees.
- `Font.Shape` writes glyph ids, clusters, advances, and offsets into the passed `Buffer`, so the buffer is the result carrier and a second shape over one buffer without a reset compounds the prior run.
- Advances leave in the integer design scale `SetScale` fixed, so every position rescales by `raster size / design scale`; setting that scale to the face's own `UnitsPerEm` makes the rescale exact where a shared constant quantizes a face of different units, and HarfBuzz shaping space is y-up while the canvas is y-down, so the vertical projection negates.
- `Tag` is a packed four-byte integer with implicit conversions both ways, so a tag literal, a parsed tag, and a `uint` constant are one value; `Parse` is the only string ingress and it NEVER reports failure — a null or empty string yields the none tag and a longer string truncates to four characters — so the four-character shape validates at the caller before the parse or a padded tag rides the native call resolving to nothing.
- The managed surface exposes NO layout-table feature enumeration, so whether a face implements a discretionary feature is proven by shaping a probe with the feature on and off and comparing glyph ids; a tag the face ignores is otherwise indistinguishable from one it applies.

[STACKING]:
- `SkiaSharp.HarfBuzz`(`api-skia-harfbuzz.md`): the bridge's `SKShaper` composes exactly these types with `GuessSegmentProperties` and an empty feature array, and its `BlobExtensions.ToHarfBuzzBlob` is the one admitted `SKStreamAsset`-to-`Blob` hop while `FontExtensions.GetScale`/`SetScale` carry the `SKSizeI` form of the design scale — a run needing pinned segment properties, feature values, or a cluster policy drops from `SKShaper` to `Font.Shape(Buffer, Feature[])` and keeps the bridge for the blob and scale hops.
- `SkiaSharp`(`api-skiasharp.md`): the shaped spans feed `SKTextBlobBuilder.AllocateRawPositionedRun` — an `SKRawRunBuffer<SKPoint>` — casting `GlyphInfo.Codepoint` to `ushort` for the glyph span and projecting `GlyphPosition` into the point span, and the built `SKTextBlob` draws through `SKCanvas.DrawText(SKTextBlob, x, y, SKPaint)`; `SKTypeface.VariationDesignParameters` and `Clone(SKFontArguments)` are the Skia-side twins of the variation and palette surface this assembly reads.
- `api-harfbuzz-native.md`: every member here P/Invokes the RID-selected `libHarfBuzzSharp` those packages place, so a missing native surfaces as a first-shape load fault and the load identity records once into the typography evidence stream.
- within-lib: `Theme/typography#SHAPING_RAIL` owns the one shaping rail — `FaceInstance` is the once-per-face-instance `Blob`/`Face`/`Font` capsule holding the face's own `UnitsPerEm` design scale, its palette election, and the probe-proved feature set; `FaceCabinet` keys those capsules; `TextItemizer` reads `UnicodeFunctions.Default.GetScript` and `Script.HorizontalDirection` for segmentation; `FeatureAdmission` is the one validated-then-`Tag.Parse` mint over both scopes; and `RunSpec` is the paragraph `Direction`/`Script`/`Language`/`ClusterLevel` row `Theme/locale#LOCALE_AXIS` supplies per culture.

[LOCAL_ADMISSION]:
- A face admits once per face INSTANCE — typeface, variation coordinates, palette — and its capsule holds the stream, blob, face, and font for the capsule's whole life; a per-draw face build reloads the font bytes at draw rate.
- `Tag.Parse` coerces silently, so tag admission validates the four-character shape at one owner and yields a typed rail value; a tag string reaching `Feature` unvalidated is the escape this admission exists to close.
- The span accessors are the read form; the array properties recopy the whole run on every access and admit only a diagnostic dump.
- `GlyphInfo.GlyphFlags` carries `UnsafeToBreak`, so a line breaker filters candidate boundaries on the shaper's own verdict rather than on source-text positions alone.
