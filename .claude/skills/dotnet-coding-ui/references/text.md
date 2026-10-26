# [TEXT]

Deep use of HarfBuzzSharp and RichTextKit over SkiaSharp: how itemization, shaping, breaking, and positioning compose into one measured block, what each engine owns, and the traps at run edges, in feature ranges, and under scale.

## [01]-[PIPELINE]

Text becomes glyphs in stages, each owned by one type and rerun only when its input changes:

| [INDEX] | [STAGE]  | [INPUT]                               | [OWNER]                                   | [OUTPUT]                            |
| :-----: | :------- | :------------------------------------ | :---------------------------------------- | :---------------------------------- |
|  [01]   | Itemize  | Code points, styles, base direction   | `Bidi`, `FontFallback`, style runs        | Runs of one script, level, and face |
|  [02]   | Shape    | One run, features, axes               | `Font.Shape(Buffer, Feature[])`           | Glyph ids, clusters, positions      |
|  [03]   | Break    | Shaped runs, `MaxWidth`               | `LineBreaker`, `TextBlock.Layout`         | Lines with run slices               |
|  [04]   | Position | Lines, `Alignment`, `LineHeight`      | `TextBlock`, `TextLine`, `FontRun`        | Glyph coordinates in block space    |
|  [05]   | Paint    | Positioned glyphs, `TextPaintOptions` | `SKTextBlobBuilder`, `SKFont`, `SKCanvas` | Pixels                              |

- Text, style, width, or direction changes rerun itemize, break, and position
- Face, size, feature, or axis changes rerun shape, paint runs per frame
- Items are uniform in script, direction, language, face, and features, every boundary between items is a kerning and ligature boundary
- RichTextKit runs every stage inside `TextBlock` with its own HarfBuzz shaper
- Runs needing features, axes, language-specific forms, or paragraph context shape through HarfBuzz directly and enter a blob
- HarfBuzz caches a shape plan per face, segment properties, features, and axes, so repeated runs with equal properties pay for glyph work alone

## [02]-[HARFBUZZ_OBJECTS]

Native HarfBuzz objects reference-count, and a scene owns them for its lifetime:

| [INDEX] | [TYPE]    | [CONSTRUCTION]                                 | [HOLDS]                                     | [LIFETIME]              |
| :-----: | :-------- | :--------------------------------------------- | :------------------------------------------ | :---------------------- |
|  [01]   | `Blob`    | `ToHarfBuzzBlob()`, `Blob.FromFile`            | Bytes, `Length`, `FaceCount`, `IsImmutable` | Scene                   |
|  [02]   | `Face`    | `new Face(blob, index)`                        | Tables, `UnitsPerEm`, `GlyphCount`, axes    | Scene                   |
|  [03]   | `Font`    | `new Font(face)`, `new Font(parent)`           | Scale, functions, variation coordinates     | Scene per axis set      |
|  [04]   | `Buffer`  | `new Buffer()`                                 | Code points, then glyphs, properties        | Thread, cleared per run |
|  [05]   | `Feature` | `new Feature(tag, value, start, end)`, `Parse` | Tag, value, cluster range                   | Value                   |
|  [06]   | `Tag`     | `new Tag('t', 'n', 'u', 'm')`, `Tag.Parse`     | Four bytes                                  | Value                   |

- `SKTypeface.OpenStream(out index).ToHarfBuzzBlob()` wraps memory-backed font bytes without a copy and marks the blob immutable, the release delegate disposes the stream
- `Face` is heavy and `Font` is light, one face per typeface and one font per size or axis set, a face keeps its own blob reference after the blob handle disposes
- `MakeImmutable()` on a `Blob`, `Face`, or `FontFunctions` lets threads share it and makes later setters fail silently, a `Font` and a `Buffer` serve one thread
- `new Font(parent)` derives a sub-font that inherits the parent's functions and overrides scale or axes alone
- `Font.SetFunctionsOpenType()` reads advances and extents from the tables, `SetFontFunctions(FontFunctions)` installs a host's metrics callbacks
- `SetScale(x, y)` sets the integer units positions return in, a new font defaults to `UnitsPerEm`, and output must divide by the same scale
- `Face.VariationAxisInfos`, `TryFindVariationAxis(tag, out info)`, `NamedInstanceCount`, and `GetNamedInstanceDesignCoords(index)` describe a variable font
- `Font.SetVariationCoordsDesign(coords)` takes design values in axis order, `SetVariationCoordsNormalized` takes 2.14 fixed point, `SetVariations(variations)` replaces every setting and resets unlisted axes
- `SetVariationNamedInstance(index)` selects a named instance, and a named instance behaves as a separate static font for caching
- `OpenTypeMetrics.TryGetPosition(tag, out position)` reads x-height, cap height, and underline metrics in scale units, `TryGetHorizontalFontExtents` gives `Ascender`, `Descender`, and `LineGap`
- `Face.HasColorLayers`, `HasColorPng`, and `HasColorSvg` tell which color glyph format a face carries, a color filter on the paint flattens them

## [03]-[BUFFER_PROTOCOL]

Buffers hold code points before `Shape` and glyphs after, and the protocol around one shape decides edge correctness:
- `AddUtf16(text, itemOffset, itemLength)` adds one item with the string as context, HarfBuzz reads five code points on each side and emits none
- Context keeps Arabic joining and mark attachment correct at run edges
- `AddUtf16(text)` on a slice loses that context and shapes the slice as a paragraph of its own, `AddUtf32` and `AddCodepoints` make clusters code point indexes
- `Direction`, `Script`, and `Language` are set by the itemizer before `Shape`, `GuessSegmentProperties()` fills unset ones from content and the process locale as a fallback, not an itemizer
- `Shape` throws when `Direction` is `Invalid` or `ContentType` is not `Unicode`, a shaped buffer reads `ContentType.Glyphs` until `ClearContents()`
- `ClearContents()` keeps `Direction`, `Script`, `Language`, `ClusterLevel`, and `Flags` for the next run, `Reset()` clears them
- `ClusterLevel.MonotoneGraphemes` merges marks into their base, `MonotoneCharacters` keeps one cluster per character while staying monotone and serves a caret per character, `Characters` never merges
- `BufferFlags.BeginningOfText` and `EndOfText` mark a run that opens or closes its paragraph for initial and final forms, `RemoveDefaultIgnorables` drops ZWJ and ZWNJ glyphs, `ProduceUnsafeToConcat` costs and is off by default
- Clusters are the caller's text indexes, fewer glyphs than code points mark a ligature or conjunct
- Right-to-left output comes back in visual order with descending clusters
- `GlyphInfo.Codepoint` is the glyph id after shaping, `Cluster` the text index, `GlyphFlags.UnsafeToBreak` absent means a break at this cluster reshapes to the same glyphs
- Line breakers shape the paragraph once and split at cluster starts without `UnsafeToBreak`, reshaping lines broken at a flagged cluster alone
- `GetGlyphInfoSpan()` and `GetGlyphPositionSpan()` read native memory until the next buffer write, `GlyphInfos` and `GlyphPositions` copy
- `ReverseClusters()` turns right-to-left visual output back to logical order, `NormalizeGlyphs()` gives canonical in-cluster order for comparing output, `SerializeGlyphs(font)` prints a run for a test
- `ReplacementCodepoint` (U+FFFD by default) replaces invalid input, `InvisibleGlyph` sets the glyph for hidden characters

## [04]-[FEATURES_AND_VARIATIONS]

Features select substitutions and positions per run, variations select a design per font:
- `Feature.Parse` reads the CSS grammar: `"tnum"` enables, `"liga=0"` or `"-liga"` disables, `"aalt=2"` picks an alternate, `"ss01[3:5]"` bounds to clusters 3 through 4
- `Feature(tag, value, start, end)` ranges are cluster values in the indexes passed to `AddUtf16`, a run-relative or glyph index misplaces the feature
- Overlapping same-tag features let the later feature win, a feature over the whole run is the common form and a range serves one word
- Every run gets `ccmp`, `locl`, `mark`, `mkmk`, `rlig`, `abvm`, and `blwm`, horizontal text adds `calt`, `clig`, `curs`, `dist`, `kern`, `liga`, and `rclt`, user features override defaults
- `Font.Shape(buffer, features, shapers)` names shaper engines in order, `"ot"` is the OpenType shaper and `"fallback"` maps cmap and advances alone, `Font.SupportedShapers` lists them
- Kerning and ligatures form inside one item alone, a style boundary mid-word loses the pairs, so runs stay as long as their properties allow
- `Face.TryFindVariationAxis(tag, out info)` gives `MinValue`, `DefaultValue`, and `MaxValue` for clamping before a set
- Variations change advances, so a `Font` per axis set shapes once and a slider over `wght` reshapes per step
- `SKTypeface` painting the glyphs carries the same variation, or outlines and advances disagree

## [05]-[POSITIONS_TO_PIXELS]

Positions return as integers in the scale `Font.SetScale` set, and the paint stage converts them once:
- `SetScale(UnitsPerEm, UnitsPerEm)` returns font units, pixel values are `value * Size / UnitsPerEm`, a shape serves every size of one face and feature set
- `SetScale(Size * 64, Size * 64)` returns 26.6 fixed point and `value / 64f` gives subpixel positions, `SetScale(Size, Size)` returns whole pixels and loses subpixel advances
- `XAdvance` accumulates into the pen, `YAdvance` does the same for vertical scripts, `XOffset` and `YOffset` displace the glyph alone and leave the pen in place
- HarfBuzz y grows up and Skia y grows down, a glyph origin is `(pen + XOffset * s, -YOffset * s)`
- `SKTextBlobBuilder.AllocatePositionedRun(font, count)` takes one `SKFont` per run (one typeface, size, and paint style), `GetGlyphSpan()` and `GetPositionSpan()` fill it
- `AllocateHorizontalRun(font, count, y)` takes x positions alone where every glyph sits on one baseline, `AddPositionedRun(glyphs, font, positions)` copies spans in one call
- `SKTextBlob.Bounds` gives conservative ink bounds for invalidation, `GetIntercepts(upper, lower, intervals, paint)` gives x ranges an underline skips
- `SKFont.Size`, `ScaleX`, `SkewX`, and `Embolden` change rasterization alone, layout metrics come from `GetFontMetrics(out metrics)` or `Font.TryGetHorizontalFontExtents`
- `SKFontMetrics.Ascent` is negative, `Descent` positive, `Leading` the line gap, `UnderlinePosition` and `UnderlineThickness` are nullable
- `SKFont.Subpixel` positions origins at fractions, `LinearMetrics = true` returns unhinted advances so layout at scale 1 and 2 agree, `Hinting` stays `None` under a scaled canvas

```csharp
internal static class Baselines {
    public static float LineHeight(SKFont font) {
        font.GetFontMetrics(out SKFontMetrics metrics);
        return metrics.Descent - metrics.Ascent + metrics.Leading;
    }
}
```

## [06]-[RICHTEXTKIT_BLOCKS]

`StyledText` holds code points with style runs, `TextBlock` lays one paragraph out, and `RichString` builds blocks from a fluent description:
- `AddText(text, style)` appends a run from a `string`, `ReadOnlySpan<char>`, or `Slice<int>`, `InsertText`, `DeleteText`, and `ApplyStyle` edit in code point positions
- `Layout()` runs when a measured read, `Paint`, `HitTest`, or `GetCaretInfo` finds the block stale, writes to `MaxWidth`, `RenderWidth`, `MaxHeight`, `MaxLines`, `EllipsisEnabled`, `Alignment`, `BaseDirection`, and `AddText` mark it stale
- `MaxWidth` `null` lifts wrapping, `RenderWidth` sets the alignment width when `MaxWidth` is `null`, `MaxLines` and `MaxHeight` truncate and `EllipsisEnabled` appends the ellipsis
- `MeasuredWidth` and `MeasuredHeight` are the advance box, `MeasuredOverhang` and `MeasuredPadding` are per-edge amounts of ink past the box and space inside it, `Truncated` reports an ellipsis
- Blocks hold one paragraph, `Copy(from, length)` and `AddEllipsis()` serve multi-block flows, `Clear()` then `AddText` reuses one block
- `Style` setters throw once `Seal()` ran, `Modify(...)` derives a changed copy, `StyleManager` dedupes equivalent styles through `FromStyle`
- `IStyle` holds `FontFamily`, `FontSize`, `FontWeight`, `FontWidth`, `FontItalic`, `Underline`, `StrikeThrough`, `LineHeight`, `TextColor`, `BackgroundColor`, halo, `LetterSpacing`, `FontVariant`, `TextDirection`, and `ReplacementCharacter`, and no OpenType feature or axis
- `TextColor` and `HaloColor` are `SKColor`, an 8-bit ink, `ReplacementCharacter` substitutes every code point for a password field
- `FontVariant.SuperScript` and `SubScript` scale and shift the run and bump the weight, `LineHeight` multiplies the natural height, `LetterSpacing` adds pixels per cluster
- `TextPaintOptions` defaults to `Antialias` edging with `SubpixelPositioning`, `Selection` with `SelectionColor` paints a highlight behind a `TextRange`, `FontRun` caches an `SKTextBlob` per run at paint
- RichTextKit shapes each `FontRun` with one shaper per `SKTypeface` kept for the process, at scale 512 through `SetFunctionsOpenType`
- RichTextKit's shaper adds the run's code points alone, guesses script and language, and passes no features
- `TextBlock.ResetPooledMemory()` releases the pooled arrays large blocks grew, `RichString.Revision` detects a change

## [07]-[FALLBACK_CARETS_AND_HITS]

Every code point a face lacks moves its run to another face once per layout, and lines and runs expose the geometry a caret or hit test reads:
- `FontMapper.TypefaceFromStyle(style, ignoreFontVariants)` maps family, weight, width, and slant to an `SKTypeface`, an override returns app typefaces and falls through to `base`
- `TextBlock.FontMapper` sets a mapper per block, `FontMapper.Default` serves the rest
- `FontFallback.GetFontRuns(codePoints, typeface, replacementCharacter)` splits at missing glyphs on grapheme boundaries and names a face per piece, spaces never trigger fallback
- `FontFallback.CharacterMatcher` is the static `ICharacterMatcher` behind fallback, the default forwards to `SKFontManager.Default.MatchCharacter` with no language, so CJK unification by language needs a matcher that passes `bcp47`
- Runs shaped through HarfBuzz use one `Face`, fallback splits before the shape and each piece shapes with its own `Font`
- `Lines` and `LineIndicies` list laid lines and their start code points, `TextLine` holds `YCoord`, `BaseLine`, `Height`, `Width`, `MaxAscent`, `MaxDescent`, `Start`, `End`, and `Runs`
- `FontRun` holds `Glyphs`, `GlyphPositions`, `Clusters`, `RelativeCodePointXCoords`, `Typeface`, `Style`, and `RunKind` (`Normal`, `Ellipsis`, `TrailingWhitespace`), `GetXCoordOfCodePointIndex` gives a caret x inside it
- `HitTest(x, y)` and `HitTestLine(line, x)` return `OverLine` and `OverCodePointIndex` (-1 outside ink) with `ClosestLine`, `ClosestCodePointIndex`, and `AltCaretPosition` always answered
- `CaretPosition(codePointIndex, altPosition)` with `altPosition` sits at the end of the previous line on a wrap or the other side of a bidi boundary, `GetCaretInfo(position)` returns `CaretXCoord` and a `CaretRectangle` sloped for italics
- `CaretIndicies` and `WordBoundaryIndicies` list legal stops in code points, `LookupCaretIndex(codePointIndex)` finds a position in them, `TextRange.UpdateForEdit(offset, oldLength, newLength)` moves a range across an edit
- `CodePointToCharacterIndex` and `CharacterToCodePointIndex` on `StyledText` bridge host `string` offsets, `Utf32Buffer` holds the code points

## [08]-[EDITING]

`TextDocument` holds paragraphs, undo, and IME state for an editor no native field can host, and one `ITextDocumentView` per surface receives its changes:
- `RegisterView(view)` and `RevokeView(view)` attach a surface, `Text`, `DefaultStyle`, `DefaultAlignment`, `PageWidth`, `LineWrap`, `SingleLineMode`, and `PlainTextMode` set content and behavior
- `ReplaceText(view, range, text, semantics, style)` is one edit, `EditSemantics.Typing`, `Backspace`, `ForwardDelete`, and `Overtype` coalesce consecutive edits into one undo unit and `None` seals each
- `Navigate(position, kind, pageSize, ref ghostXCoord)` moves a caret by `NavigationKind` and keeps the column, `GetSelectionRange(position, SelectionKind.Word)` selects around a caret
- `ITextDocumentView` receives `OnDocumentWillChange`, `OnDocumentChange(view, info)`, `OnDocumentDidChange`, `OnRedraw`, and `OnReset`, `DocumentChangeInfo` carries `CodePointIndex`, `OldLength`, `NewLength`, `Semantics`, and `IsUndoing`
- `UndoManager.OpenGroup(description)` joins keystrokes into one unit, `Seal()` stops coalescing, `CanUndo`, `CanRedo`, `UndoDescription`, `IsModified`, and `MarkUnmodified()` back the edit menu
- `StartImeComposition(view, selection)`, `UpdateImeComposition(view, text, caretOffset)`, and `FinishImeComposition(view)` bind to the host's composition events
- `Paint(canvas, fromYCoord, toYCoord, options)` paints the paragraphs intersecting a vertical window, `MeasuredHeight` and `MeasuredContentWidth` size the scroll extent
- `PlainTextMode` with one `DefaultStyle` drops spans while editing, a styled editor keeps `PlainTextMode` off
