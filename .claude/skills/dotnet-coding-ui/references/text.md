# [TEXT]

HarfBuzzSharp and RichTextKit over SkiaSharp: the shaping pipeline, HarfBuzz objects and buffer protocol, features and variations, RichTextKit blocks, fallback, carets, and editing.

## [01]-[PIPELINE]

Text becomes glyphs in stages, each owned by one type and rerun only when its input changes:

| [INDEX] | [STAGE]  | [INPUT]                               | [OWNER]                                   | [OUTPUT]                            |
| :-----: | :------- | :------------------------------------ | :---------------------------------------- | :---------------------------------- |
|  [01]   | Itemize  | Code points, styles, base direction   | `TextBlock`, `FontMapper`, `FontFallback` | Runs of one script, level, and face |
|  [02]   | Shape    | One run, features, axes               | `Font.Shape(Buffer, Feature[])`           | Glyph ids, clusters, positions      |
|  [03]   | Break    | Shaped runs, `MaxWidth`               | `TextBlock.Layout`                        | Lines with run slices               |
|  [04]   | Position | Lines, `Alignment`, `LineHeight`      | `TextBlock`, `TextLine`, `FontRun`        | Glyph coordinates in block space    |
|  [05]   | Paint    | Positioned glyphs, `TextPaintOptions` | `TextBlock.Paint`, `SKFont`, `SKCanvas`   | Pixels                              |

- Text, style, width, or direction changes rerun every stage, `TextBlock.Layout` clears and rebuilds its font runs each time it runs
- Paint runs per frame and reshapes nothing
- Items are uniform in script, direction, language, face, and features
- RichTextKit runs every stage inside `TextBlock` with its own HarfBuzz shaper, bidi and line-break classes stay internal to it
- HarfBuzz caches a shape plan per face, segment properties, features, and axes, repeated runs with equal properties pay for glyph work alone
- `FontMapper.TypefaceFromStyle(style, ignoreFontVariants)` maps family, weight, width, and slant to an `SKTypeface`
- Overrides return app typefaces and fall through to `base`
- `TextBlock.FontMapper` sets a mapper per block, `FontMapper.Default` serves the rest

```csharp
internal sealed class Mapper(SKTypeface brand) : FontMapper {
    public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) =>
        string.Equals(style.FontFamily, brand.FamilyName, StringComparison.Ordinal) ? brand : base.TypefaceFromStyle(style, ignoreFontVariants);
}
```

## [02]-[HARFBUZZ_OBJECTS]

Native HarfBuzz objects reference-count, and a scene owns them for its lifetime:

| [INDEX] | [TYPE]    | [CONSTRUCTION]                                 | [HOLDS]                                     | [LIFETIME]              |
| :-----: | :-------- | :--------------------------------------------- | :------------------------------------------ | :---------------------- |
|  [01]   | `Blob`    | `ToHarfBuzzBlob()`, `Blob.FromFile`            | Bytes, `Length`, `FaceCount`, `IsImmutable` | Face construction       |
|  [02]   | `Face`    | `new Face(blob, index)`                        | Tables, `UnitsPerEm`, `GlyphCount`, axes    | Font construction       |
|  [03]   | `Font`    | `new Font(face)`, `new Font(parent)`           | Scale, functions, variation coordinates     | Scene per face and axes |
|  [04]   | `Buffer`  | `new Buffer()`                                 | Code points, then glyphs, properties        | Thread, cleared per run |
|  [05]   | `Feature` | `new Feature(tag, value, start, end)`, `Parse` | Tag, value, cluster range                   | Value                   |
|  [06]   | `Tag`     | `new Tag(c1, c2, c3, c4)`, `Tag.Parse`         | Four bytes                                  | Value                   |

- `SKTypeface.OpenStream(out index).ToHarfBuzzBlob()` wraps memory-backed font bytes without a copy as an immutable blob
- Release delegates dispose the stream with the blob
- `Face` is heavy and `Font` is light, one face per typeface
- `Face` keeps its own blob reference and `Font` its own face reference, blob and face handles dispose once the font exists
- `MakeImmutable()` on a `Blob`, `Face`, or `FontFunctions` lets threads share it and makes later setters fail silently
- `Font` and `Buffer` instances serve one thread
- `new Font(parent)` derives a sub-font that inherits the parent's functions and overrides scale or axes alone
- `Font.SetFunctionsOpenType()` reads advances and extents from the tables, `SetFontFunctions(FontFunctions)` installs a host's metrics callbacks
- `SetScale(x, y)` sets the integer units positions return in, a new font defaults to `UnitsPerEm`, and output divides by the same scale
- `Face.VariationAxisInfos`, `TryFindVariationAxis(tag, out info)`, `NamedInstanceCount`, and `GetNamedInstanceDesignCoords(index)` describe axes
- `Font.SetVariationCoordsDesign(coords)` takes design values in axis order, `SetVariationCoordsNormalized` takes 2.14 fixed point as `int`
- `Font.SetVariations(variations)` replaces every setting and resets unlisted axes, `SKTypeface.VariationDesignPosition` supplies the painted values
- `SetVariationNamedInstance(index)` selects a named instance, cached as a separate static font
- `OpenTypeMetrics.TryGetPosition(tag, out position)` reads x-height, cap height, and underline metrics in scale units
- `Font.TryGetHorizontalFontExtents` gives `Ascender`, `Descender`, and `LineGap`, line height is their sum scaled by `Size / UnitsPerEm`
- `Face.HasColorLayers`, `HasColorPng`, and `HasColorSvg` tell which color glyph format a face holds, a color filter on the paint flattens them

```csharp
internal sealed record ShaperKey(SKTypeface Typeface, Seq<Variation> Axes);

internal sealed class Shapers : IDisposable {
    private readonly AtomHashMap<ShaperKey, Font> fonts = AtomHashMap<ShaperKey, Font>();

    public HarfBuzzSharp.Buffer Buffer { get; } = new();

    public Font Shaper(SKTypeface typeface) {
        ShaperKey key = new(typeface, toSeq(typeface.VariationDesignPosition.Select(static at => new Variation { Tag = at.Axis, Value = at.Value })));
        return fonts.FindOrAdd(key, () => {
            using Blob blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
            using Face face = new(blob, index);
            Font font = new(face);
            font.SetFunctionsOpenType();
            font.SetVariations([.. key.Axes]);
            return font;
        });
    }

    public void Dispose() {
        _ = fonts.Values.Iter(static font => font.Dispose());
        Buffer.Dispose();
    }
}
```

## [03]-[BUFFER_PROTOCOL]

Buffers hold code points before `Shape` and glyphs after. Protocol around one shape decides edge correctness, positions return as integers in the `Font.SetScale` scale:
- `AddUtf32(text, itemOffset, itemLength)` adds one item with the whole text as context, HarfBuzz keeps five code points on each side and emits none
- Context keeps Arabic joining and mark attachment correct at run edges
- `AddUtf16(text)` on a slice loses the context and shapes the slice as a paragraph of its own
- `AddUtf32` and `AddCodepoints` make clusters code point indexes, `AddUtf16` makes them UTF-16 offsets
- Itemizers set `Direction`, `Script`, and `Language` before `Shape`
- `GuessSegmentProperties()` fills unset properties from content and the process locale, a fallback in place of an itemizer
- `Shape` throws when `Direction` is `Invalid` or `ContentType` is not `Unicode`, a shaped buffer reads `ContentType.Glyphs` until `ClearContents()`
- `ClearContents()` keeps `Direction`, `Script`, `Language`, `ClusterLevel`, and `Flags` for the next run, `Reset()` clears them
- `ClusterLevel.MonotoneGraphemes` merges marks into their base, `Characters` never merges
- `MonotoneCharacters` keeps one monotone cluster per character and serves a caret per character
- `BufferFlags.BeginningOfText` and `EndOfText` mark a run that opens or closes its paragraph for initial and final forms
- `BufferFlags.RemoveDefaultIgnorables` drops ZWJ and ZWNJ glyphs, `ProduceUnsafeToConcat` costs and is off by default
- Clusters are the caller's text indexes, fewer glyphs than code points mark a ligature or conjunct
- Right-to-left output comes back in visual order with descending clusters
- `GlyphInfo.Codepoint` is the glyph id after shaping, `Cluster` its text index, `GlyphFlags` its break safety
- `GlyphFlags.UnsafeToBreak` absent means a break at this cluster reshapes to the same glyphs
- Line breakers shape the paragraph once and split at cluster starts without `UnsafeToBreak`, reshaping lines broken at a flagged cluster alone
- `GetGlyphInfoSpan()` and `GetGlyphPositionSpan()` read native memory until the next buffer write, `GlyphInfos` and `GlyphPositions` copy
- `ReverseClusters()` turns right-to-left visual output back to logical order
- `NormalizeGlyphs()` gives canonical in-cluster order for comparing output, `SerializeGlyphs(font)` prints a run for a test
- `ReplacementCodepoint` (U+FFFD by default) replaces invalid input, `InvisibleGlyph` sets the glyph for hidden characters
- `SetScale(UnitsPerEm, UnitsPerEm)` returns font units, pixel values are `value * Size / UnitsPerEm`
- `SetScale(Size * 64, Size * 64)` returns 26.6 fixed point and `value / 64f` gives subpixel positions, `SetScale(Size, Size)` loses subpixel advances
- `XAdvance` accumulates into the pen, `YAdvance` does so for vertical scripts, `XOffset` and `YOffset` displace a glyph alone and move no pen
- HarfBuzz y grows up and Skia y grows down, a glyph origin is `(pen + XOffset * s, baseline - YOffset * s)`
- `SKFont.Size`, `ScaleX`, `SkewX`, and `Embolden` change rasterization alone, `SKFont.Hinting` stays `None` under a scaled canvas

`Reshape` writes a featured run back into the laid block, the block's code points supply the paragraph context and the line supplies the baseline:

```csharp
internal static class Reshaping {
    public static void Reshape(FontRun run, TextLine line, Font font, HarfBuzzSharp.Buffer buffer, Seq<Feature> features) {
        buffer.ClearContents();
        buffer.Direction = run.Direction is TextDirection.RTL ? Direction.RightToLeft : Direction.LeftToRight;
        buffer.AddUtf32(run.CodePoints.Underlying, run.CodePoints.Start, run.CodePoints.Length);
        buffer.GuessSegmentProperties();
        font.Shape(buffer, [.. features]);
        ReadOnlySpan<GlyphInfo> infos = buffer.GetGlyphInfoSpan();
        ReadOnlySpan<GlyphPosition> at = buffer.GetGlyphPositionSpan();
        float scale = run.Style.FontSize / run.Typeface.UnitsPerEm;
        float baseline = line.YCoord + line.BaseLine;
        ushort[] glyphs = new ushort[infos.Length];
        SKPoint[] points = new SKPoint[infos.Length];
        int[] clusters = new int[infos.Length];
        float pen = run.XCoord;
        for (int i = 0; i < infos.Length; i++) {
            glyphs[i] = (ushort)infos[i].Codepoint;
            clusters[i] = (int)infos[i].Cluster;
            points[i] = new SKPoint(pen + (at[i].XOffset * scale), baseline - (at[i].YOffset * scale));
            pen += at[i].XAdvance * scale;
        }
        run.Glyphs = new Slice<ushort>(glyphs);
        run.GlyphPositions = new Slice<SKPoint>(points);
        run.Clusters = new Slice<int>(clusters);
        run.Width = pen - run.XCoord;
    }
}
```

## [04]-[FEATURES_AND_VARIATIONS]

Features select substitutions and positions per run, variations select a design per font:
- `Feature.Parse` reads CSS grammar, `"tnum"` enables, `"liga=0"` or `"-liga"` disables
- `"aalt=2"` selects an alternate, `"ss01[3:5]"` bounds a feature to clusters 3 through 4
- `Feature(tag, value, start, end)` ranges are cluster values in the indexes passed to `AddUtf32`, a run-relative or glyph index misplaces the feature
- Overlapping same-tag features let a later feature win, one feature spans a whole run and a range serves one word
- Every run gets `ccmp`, `locl`, `mark`, `mkmk`, `rlig`, `abvm`, and `blwm`, user features override defaults
- Horizontal text adds `calt`, `clig`, `curs`, `dist`, `kern`, `liga`, and `rclt`
- `Font.Shape(buffer, features, shapers)` names shaper engines in order, `Font.SupportedShapers` lists them
- `"ot"` is the OpenType shaper, `"fallback"` maps cmap and advances alone
- Kerning and ligatures form inside one item alone, a style boundary mid-word loses the pairs, runs stay as long as their properties allow
- `Face.TryFindVariationAxis(tag, out info)` gives `MinValue`, `DefaultValue`, and `MaxValue` for clamping before a set
- Variations change advances, a `Font` per axis set shapes once and a slider over `wght` reshapes per step
- `SKTypeface` painting the glyphs holds the same variation, or outlines and advances disagree

```csharp
internal static class Axes {
    public static Variation Clamped(Face face, Tag axis, float value) =>
        new() { Tag = axis, Value = face.TryFindVariationAxis(axis, out OpenTypeVarAxisInfo info) ? float.Clamp(value, info.MinValue, info.MaxValue) : value };
}
```

## [05]-[RICHTEXTKIT_BLOCKS]

`StyledText` holds code points with style runs, `TextBlock` lays one paragraph out, and `RichString` builds blocks from a fluent description:
- `AddText(text, style)` appends a run from a `string`, `ReadOnlySpan<char>`, or `Slice<int>`
- `InsertText`, `DeleteText`, and `ApplyStyle` edit in code point positions
- `Layout()` runs when a measured read, `Paint`, `HitTest`, or `GetCaretInfo` finds the block stale
- Writes to `MaxWidth`, `RenderWidth`, `MaxHeight`, `MaxLines`, `EllipsisEnabled`, `Alignment`, `BaseDirection`, and `AddText` mark a block stale
- `MaxWidth` `null` lifts wrapping, `RenderWidth` sets the alignment width when `MaxWidth` is `null`
- `MaxLines` and `MaxHeight` truncate, `EllipsisEnabled` appends the ellipsis, `Truncated` reports one
- `MeasuredWidth` and `MeasuredHeight` are the advance box
- `MeasuredOverhang` and `MeasuredPadding` are per-edge glyph bounds past the box and space inside it, overhang extends invalidation and caret rects
- Blocks hold one paragraph, `Copy(from, length)` and `AddEllipsis()` serve multi-block flows, `Clear()` then `AddText` reuses one block
- `Style` setters throw `InvalidOperationException` once `Seal()` ran, `Modify(...)` derives a changed copy
- `StyleManager.FromStyle(style)` answers the one shared style equal to a given one, `StyleManager.Default` holds one manager per thread
- `IStyle` holds `FontFamily`, `FontSize`, `FontWeight`, `FontWidth`, `FontItalic`, `Underline`, `StrikeThrough`, and `LineHeight`
- `IStyle` holds `TextColor`, `BackgroundColor`, halo members, `LetterSpacing`, `FontVariant`, `TextDirection`, and `ReplacementCharacter`
- `ReplacementCharacter` substitutes every code point for a password field
- `FontVariant.SuperScript` and `SubScript` scale and shift the run and bump the weight
- `LineHeight` multiplies the natural height, `LetterSpacing` adds pixels per cluster
- `TextPaintOptions` defaults to `Antialias` edging, `Normal` hinting, and `SubpixelPositioning`
- `Selection` with `SelectionColor` paints a highlight behind a `TextRange`
- `FontRun.Glyphs`, `GlyphPositions`, `Clusters`, `Width`, and `XCoord` are `Slice` and `float` fields a reshape assigns, `MoveGlyphs` shifts them
- `FontRun.CodePoints` is a `Slice<int>` over the block's code points, `Underlying`, `Start`, and `Length` address the run inside its paragraph
- RichTextKit shapes each `FontRun` with one shaper per `SKTypeface` kept for the process, at scale 512 through `SetFunctionsOpenType`
- RichTextKit's shaper adds the run's code points alone, guesses script and language, and passes no features
- `TextBlock.ResetPooledMemory()` releases the pooled arrays large blocks grew
- `RichString.Revision` detects a change

```csharp
internal sealed class Paragraphs {
    private readonly StyleManager styles = new();

    public TextBlock Laid(string text, Style face, float width, TextAlignment alignment, int lines) {
        IStyle shared = styles.FromStyle(face);
        TextBlock block = new() { MaxWidth = width, Alignment = alignment, MaxLines = lines, EllipsisEnabled = true };
        block.AddText(text, shared);
        return block;
    }

    public static SKRect Invalidation(TextBlock block, SKPoint origin) {
        SKRect over = block.MeasuredOverhang;
        return new SKRect(origin.X - over.Left, origin.Y - over.Top, origin.X + block.MeasuredWidth + over.Right, origin.Y + block.MeasuredHeight + over.Bottom);
    }
}
```

## [06]-[FALLBACK_CARETS_AND_HITS]

Every code point a face lacks moves its run to another face once per layout, and lines and runs expose the geometry a caret or hit test reads:
- `FontFallback.GetFontRuns(codePoints, typeface, replacementCharacter)` splits at missing glyphs on grapheme boundaries and names a face per piece
- Spaces trigger no fallback
- `FontFallback.CharacterMatcher` is the static `ICharacterMatcher` behind fallback
- Default `CharacterMatcher` forwards to `SKFontManager.Default.MatchCharacter` with no language, CJK unification takes a matcher passing `bcp47`
- Runs shaped through HarfBuzz use one `Face`, fallback splits before the shape and each piece shapes with its own `Font`
- `Lines` and `LineIndicies` list laid lines and their start code points
- `TextLine` holds `YCoord`, `BaseLine`, `Height`, `MaxAscent`, `MaxDescent`, `Start`, `End`, `Runs`, and a read-only `Width`
- `FontRun.RelativeCodePointXCoords`, `Typeface`, `Style`, and `RunKind` (`Normal`, `Ellipsis`, `TrailingWhitespace`) describe a laid run
- `FontRun.GetXCoordOfCodePointIndex` gives a caret x inside the run
- `HitTest(x, y)` and `HitTestLine(line, x)` return `OverLine` and `OverCodePointIndex`, -1 outside the glyphs
- `ClosestLine`, `ClosestCodePointIndex`, and `AltCaretPosition` on a hit are always answered
- `CaretPosition(codePointIndex, altPosition)` with `altPosition` sits at a previous line's end on a wrap or across a bidi boundary
- `GetCaretInfo(position)` returns `CaretXCoord`, `LineIndex`, and a `CaretRectangle` sloped for italics
- `CaretIndicies` and `WordBoundaryIndicies` list legal stops in code points, `LookupCaretIndex(codePointIndex)` finds a position in them
- `TextRange.UpdateForEdit(offset, oldLength, newLength)` moves a range across an edit, `Clamp(maxCodePointIndex)` bounds one
- `CodePointToCharacterIndex` and `CharacterToCodePointIndex` on `StyledText` bridge host `string` offsets, `Utf32Buffer` holds the code points

```csharp
internal static class Carets {
    public static SKRect Rect(TextBlock block, int codePoint, bool alternate) => block.GetCaretInfo(new CaretPosition(codePoint, alternate)).CaretRectangle;

    public static CaretPosition Hit(TextBlock block, SKPoint point) => block.HitTest(point.X, point.Y).CaretPosition;

    public static int WordStart(TextBlock block, int codePoint) => toSeq(block.WordBoundaryIndicies).Filter(stop => stop <= codePoint).Last.IfNone(0);

    public static string Selected(TextBlock block, TextRange range, string text) => text[block.CodePointToCharacterIndex(range.Minimum)..block.CodePointToCharacterIndex(range.Maximum)];
}
```

## [07]-[EDITING]

`TextDocument` holds paragraphs, undo, and IME state for an editor no native field can host, and one `ITextDocumentView` per surface receives its changes:
- `RegisterView(view)` and `RevokeView(view)` attach a surface
- `Text`, `DefaultStyle`, `DefaultAlignment`, `PageWidth`, `LineWrap`, `SingleLineMode`, `PlainTextMode`, and `SetMargins` set content and page
- `ReplaceText(view, range, text, semantics, style)` is one edit
- `EditSemantics.Typing`, `Backspace`, `ForwardDelete`, and `Overtype` coalesce consecutive edits into one undo unit, `None` seals each
- `Navigate(position, kind, pageSize, ref ghostXCoord)` moves a caret by `NavigationKind` and keeps the column
- `GetSelectionRange(position, SelectionKind.Word)` selects around a caret
- `ITextDocumentView` receives `OnDocumentWillChange`, `OnDocumentChange(view, info)`, `OnDocumentDidChange`, `OnRedraw`, and `OnReset`
- `DocumentChangeInfo` holds `CodePointIndex`, `OldLength`, `NewLength`, `Semantics`, `IsUndoing`, and `ImeCaretOffset`
- `UndoManager.OpenGroup(description)` joins keystrokes into one unit, `Seal()` stops coalescing
- `CanUndo`, `CanRedo`, `UndoDescription`, `IsModified`, and `MarkUnmodified()` back the edit menu
- `StartImeComposition(view, selection)`, `UpdateImeComposition(view, styledText, caretOffset)`, and `FinishImeComposition(view)` bind IME events
- `Paint(canvas, fromYCoord, toYCoord, options)` paints the paragraphs intersecting a vertical window
- `MeasuredHeight` and `MeasuredContentWidth` size the scroll extent
- `PlainTextMode` with one `DefaultStyle` drops spans while editing, a styled editor keeps `PlainTextMode` off

```csharp
internal sealed class DocumentView : ITextDocumentView {
    private readonly TextDocument document = new() { PlainTextMode = true, LineWrap = true };
    private TextRange selection = new(0);

    public DocumentView(IStyle style, float width) {
        document.DefaultStyle = style;
        document.PageWidth = width;
        document.RegisterView(this);
    }

    public void Type(string text) => document.ReplaceText(this, selection, text, EditSemantics.Typing);

    public void Select(SelectionKind kind) => selection = document.GetSelectionRange(selection.CaretPosition, kind);

    public void Paint(SKCanvas canvas, float top, float bottom, TextPaintOptions options) => document.Paint(canvas, top, bottom, options);

    void ITextDocumentView.OnReset() => selection = new TextRange(0);

    void ITextDocumentView.OnRedraw() => selection = selection.Clamp(document.Length - 1);

    void ITextDocumentView.OnDocumentWillChange(ITextDocumentView view) => selection = selection.Clamp(document.Length - 1);

    void ITextDocumentView.OnDocumentChange(ITextDocumentView view, DocumentChangeInfo info) =>
        selection = ReferenceEquals(view, this) || info.IsUndoing ? new TextRange(info.CodePointIndex + info.NewLength) : selection.UpdateForEdit(info.CodePointIndex, info.OldLength, info.NewLength);

    void ITextDocumentView.OnDocumentDidChange(ITextDocumentView view) => selection = selection.Clamp(document.Length - 1);
}
```
