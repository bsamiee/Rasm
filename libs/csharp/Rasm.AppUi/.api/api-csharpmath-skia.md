# [RASM_APPUI_API_CSHARPMATH_SKIA]

`CSharpMath.SkiaSharp` renders TeX-subset math and mixed math-text layout onto an `SKCanvas` through two painters — `MathPainter` for a `MathList` and `TextPainter` for a `TextAtom` run — each a thin `SKCanvas`/`SKColor` specialization of the backend-agnostic `Painter<TCanvas,TContent,TColor>` base in `CSharpMath.Rendering.FrontEnd`, which owns the LaTeX knob, the font-size and color algebra, the typed error rail, and the measure/draw contract; a `SkiaCanvas` adapter binds the base's `ICanvas` draw calls to real `SKCanvas`/`SKPaint` operations, and a `SkiaPath` translates glyph outlines into `SKPath`.

Its telos is one painter the typography Math arms set `LaTeX` on and draw: LaTeX parses through `CSharpMath.Atom.LaTeXParser` into a typed `Result<MathList>`, a parse failure lands in the painter's `ErrorMessage` instead of throwing, and the same painter encodes headless to an image `Stream` for capture. SkiaSharp binding resolves to the admitted SkiaSharp closure, so painters draw into the same `SKCanvas` an Avalonia Skia lease exposes.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSharpMath.SkiaSharp` over `CSharpMath.Rendering` and `CSharpMath`
- package: `CSharpMath.SkiaSharp` (SkiaSharp painter layer); `CSharpMath.Rendering` (backend-agnostic painters, keyboard, glyph engine), `CSharpMath.Editor` (edit commands), and `CSharpMath` (LaTeX parse, atom model, display tree, typed `Result`) are transitive
- license: MIT (`CSharpMath` family)
- assembly: `CSharpMath.SkiaSharp` (painters + `ICanvas`/`Path` adapters + color/encode extensions), `CSharpMath.Rendering` (`Painter` base, `MathKeyboard`, vendored `Typography.OpenFont` glyph engine), `CSharpMath` (`LaTeXParser`, atom/display model, `Result<T>`)
- namespace: `CSharpMath.SkiaSharp` (painters, `SkiaCanvas`, `SkiaPath`, `Extensions`), `CSharpMath.Rendering.FrontEnd` (`Painter`, `ICanvas`, `Path`, `MathKeyboard`, enums), `CSharpMath.Atom` (`LaTeXParser`, `LaTeXSettings`, `LineStyle`), `CSharpMath.Structures` (`Result<T>`)
- asset: runtime library (managed; Skia natives arrive through the central SkiaSharp pin, not this package)
- rail: typography

## [02]-[PUBLIC_TYPES]

[SKIASHARP_PAINTER_TYPES]: SkiaSharp-bound painters and draw adapters — `CSharpMath.SkiaSharp`.
- rail: typography

| [INDEX] | [SYMBOL]      | [KIND]                                  | [ROLE]                                           |
| :-----: | :------------ | :-------------------------------------- | :----------------------------------------------- |
|  [01]   | `MathPainter` | `class : MathPainter<SKCanvas,SKColor>` | LaTeX math-list painter over `SKCanvas`          |
|  [02]   | `TextPainter` | `class : TextPainter<SKCanvas,SKColor>` | mixed text-and-math run painter over `SKCanvas`  |
|  [03]   | `SkiaCanvas`  | `sealed : ICanvas`                      | `SKCanvas`/`SKPaint` draw adapter                |
|  [04]   | `SkiaPath`    | `sealed : Path`                         | glyph-outline translator into `SKPath`           |
|  [05]   | `Extensions`  | `static`                                | `SKColor`↔`Color` bridge and image-stream encode |

[FRONTEND_BASE_TYPES]: Backend-agnostic painter base, editing keyboard, canvas/path contracts, and layout enums — `CSharpMath.Rendering.FrontEnd`.
- rail: typography

| [INDEX] | [SYMBOL]                           | [KIND]                                     | [ROLE]                                        |
| :-----: | :--------------------------------- | :----------------------------------------- | :-------------------------------------------- |
|  [01]   | `Painter<TCanvas,TContent,TColor>` | `abstract : ICSharpMathAPI`                | painter knob, measure, and draw contract      |
|  [02]   | `MathPainter<TCanvas,TColor>`      | `abstract : Painter<…,MathList,…>`         | math-list painter base                        |
|  [03]   | `TextPainter<TCanvas,TColor>`      | `abstract : Painter<…,TextAtom,…>`         | text-atom painter base                        |
|  [04]   | `MathKeyboard`                     | `class : MathKeyboard<Fonts,Glyph>`        | interactive edit keyboard with caret draw     |
|  [05]   | `ICanvas`                          | `interface`                                | backend draw contract painters render through |
|  [06]   | `Path`                             | `abstract : IGlyphTranslator, IDisposable` | glyph-outline sink                            |
|  [07]   | `PainterConstants`                 | `static`                                   | `DefaultFontSize` (14), `LargerFontSize` (50) |
|  [08]   | `PaintStyle`                       | `enum : byte`                              | `Fill`, `Stroke`                              |
|  [09]   | `TextAlignment`                    | `enum : byte`                              | nine-way box alignment                        |
|  [10]   | `CaretShape`                       | `enum`                                     | `IBeam`, `UpArrow`                            |

[CORE_PARSE_TYPES]: LaTeX parse, layout style, and the typed result rail — `CSharpMath.Atom` and `CSharpMath.Structures`.
- rail: typography

| [INDEX] | [SYMBOL]        | [KIND]            | [ROLE]                                             |
| :-----: | :-------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `LaTeXParser`   | `class`           | LaTeX string → `Result<MathList>` and back         |
|  [02]   | `LaTeXSettings` | `static`          | command dictionary, placeholder, and font policy   |
|  [03]   | `LineStyle`     | `enum`            | `Display`, `Text`, `Script`, `ScriptScript`        |
|  [04]   | `Result<T>`     | `readonly struct` | `Error`-carrying success/failure rail with `Match` |

[ALIGNMENT_VALUES]: `TextAlignment` packs vertical (low two bits) and horizontal (bits 2-3) into one byte — `Center` is 0; `Top`/`Bottom` are 2/1; `Left`/`Right` are 8/4; the four corners combine them (`TopLeft` 10, `TopRight` 6, `BottomLeft` 9, `BottomRight` 5).

[RESULT_RAIL_SURFACE]: `Result<T>` carries a nullable `Error` string; `Ok`/error construct through implicit conversions from `T` or `string`, `Deconstruct(out T, out string?)` and `Match(Func<T,R>, Func<string,R>)` consume it, and `Bind` chains — a parse consumer never catches an exception.

## [03]-[ENTRYPOINTS]

[PAINTER_KNOBS]: `Painter<TCanvas,TContent,TColor>` owns the shared knob surface both painters inherit.
- rail: typography

| [INDEX] | [SURFACE]                                              | [RAIL]                                 |
| :-----: | :----------------------------------------------------- | :------------------------------------- |
|  [01]   | `LaTeX : string?` / `Content : TContent?`              | source LaTeX or pre-parsed content     |
|  [02]   | `FontSize : float` / `Magnification : float`           | type size and zoom factor              |
|  [03]   | `TextColor` / `HighlightColor` / `ErrorColor : TColor` | glyph, selection, and error colors     |
|  [04]   | `GlyphBoxColor : (TColor glyph, TColor textRun)?`      | per-box debug tint                     |
|  [05]   | `PaintStyle : PaintStyle` / `LineStyle : LineStyle`    | fill/stroke and display/script style   |
|  [06]   | `ErrorFontSize : float?` / `DisplayErrorInline : bool` | error render size and inline policy    |
|  [07]   | `ErrorMessage : string?` (get)                         | parse-failure message, null on success |
|  [08]   | `LocalTypefaces : IEnumerable<Typeface>`               | fallback font chain                    |
|  [09]   | `Measure(float width) : RectangleF`                    | measured bounds at a canvas width      |
|  [10]   | `WrapColor` / `UnwrapColor` / `WrapCanvas`             | backend color and canvas adapters      |
|  [11]   | `ShallowClone()`                                       | memberwise painter copy                |

[SKIA_DRAW_ENTRYPOINTS]: `MathPainter` and `TextPainter` own the `SKCanvas` draw and encode surface.
- rail: typography

| [INDEX] | [SURFACE]                                                                          | [OWNER]       | [RAIL]                           |
| :-----: | :--------------------------------------------------------------------------------- | :------------ | :------------------------------- |
|  [01]   | `AntiAlias : bool`                                                                 | both painters | `SKPaint.IsAntialias` toggle     |
|  [02]   | `Draw(SKCanvas, TextAlignment = Center, Thickness = default, float x = 0, y = 0)`  | `MathPainter` | aligned box draw                 |
|  [03]   | `Draw(SKCanvas, float x, float y)` / `Draw(SKCanvas, SKPoint)`                     | `MathPainter` | absolute-origin draw             |
|  [04]   | `DrawDisplay(IDisplay?, SKCanvas, …)`                                              | `MathPainter` | pre-measured display draw        |
|  [05]   | `Display : IDisplay?` / `Measure(float) : RectangleF`                              | `MathPainter` | display tree and measured bounds |
|  [06]   | `Draw(SKCanvas, TextAlignment = TopLeft, Thickness = default, float x = 0, y = 0)` | `TextPainter` | text-run aligned draw            |

[COLOR_AND_ENCODE]: `Extensions` bridges Skia color and encodes a `Painter<SKCanvas,TContent,SKColor>` headlessly; `DrawAsStream<TContent>` defaults width 2000, `SKEncodedImageFormat.Png`, quality 100, `TextAlignment.TopLeft`.
- rail: typography

| [INDEX] | [SURFACE]                                                                                   | [RAIL]                       |
| :-----: | :------------------------------------------------------------------------------------------ | :--------------------------- |
|  [01]   | `ToNative(this Color) : SKColor`                                                            | color-model bridge           |
|  [02]   | `FromNative(this SKColor) : Color`                                                          | color-model bridge           |
|  [03]   | `painter.DrawAsStream(float, SKEncodedImageFormat, int, TextAlignment) : Stream?`           | headless image-stream encode |

[EDIT_AND_PARSE]: `MathKeyboard` drives interactive editing and `LaTeXParser` owns parse-and-serialize.
- rail: typography

| [INDEX] | [SURFACE]                                                           | [OWNER]        | [RAIL]                       |
| :-----: | :------------------------------------------------------------------ | :------------- | :--------------------------- |
|  [01]   | `MathKeyboard(float fontSize = 14, double blinkMilliseconds = 800)` | `MathKeyboard` | caret-blinking edit keyboard |
|  [02]   | `DrawCaret(ICanvas, Color, CaretShape)`                             | `MathKeyboard` | caret render                 |
|  [03]   | `static MathListFromLaTeX(string) : Result<MathList>`               | `LaTeXParser`  | one-call LaTeX → math list   |
|  [04]   | `Build() : Result<MathList>`                                        | `LaTeXParser`  | instance parse               |
|  [05]   | `static MathListToLaTeX(MathList, StringBuilder? = null)`           | `LaTeXParser`  | math list → LaTeX serialize  |
|  [06]   | `static ParseColor(string? hexOrName) : Color?`                     | `LaTeXParser`  | LaTeX color-token parse      |

## [04]-[IMPLEMENTATION_LAW]

[MATH_TYPESET_LAW]:
- Package: `CSharpMath.SkiaSharp` over `CSharpMath.Rendering`
- Owns: TeX-subset math and mixed math-text layout, the painter knob surface (`LaTeX`, `FontSize`, `TextColor`, `LineStyle`, `Magnification`), measure and aligned draw onto an `SKCanvas`, and interactive math editing through `MathKeyboard`.
- Accept: a Math typography arm holds one painter, sets `LaTeX`, reads `Measure` for layout, and calls `Draw` into the leased `SKCanvas`; `LineStyle` selects display vs inline script sizing; `PainterConstants.DefaultFontSize`/`LargerFontSize` anchor the size ramp.
- Reject: a hand-rolled TeX box model or glyph layout when the painter owns it; a private `SKPaint`/`SKFont` math-render path bypassing `SkiaCanvas`; a literal font-size when `FontSize`/`Magnification` and the `PainterConstants` anchors parameterize it.

[TYPED_ERROR_LAW]:
- LaTeX parse is a typed rail: `LaTeXParser.MathListFromLaTeX(string)` returns `Result<MathList>`, and setting `Painter.LaTeX` routes failure into `ErrorMessage` (with `ErrorColor`/`ErrorFontSize`/`DisplayErrorInline` governing the rendered error) rather than throwing.
- Accept: consume the parse through `Result<T>.Match`/`Deconstruct` or observe `ErrorMessage` after assignment, folding a math-parse failure into the same Fin/typed-fault envelope the diagnostics rail carries.
- Reject: a `try`/`catch` around LaTeX assignment; treating a null `Display` as a thrown error when a non-null `ErrorMessage` is the failure signal.

[STACKING]:
- Stacks ONTO `api-skiasharp`: painters draw through `SKCanvas`/`SKPaint`/`SKPath`/`SKPoint`/`SKColor`, and `Extensions.ToNative`/`FromNative` bridge `SKColor` to the internal `Color`. Bind `SkiaCanvas(canvas, antiAlias)` over the `SKCanvas` an Avalonia Skia lease (`ISkiaSharpApiLease.SkCanvas`) exposes, so math composites into the same surface as `api-avalonia-skia` without a side bitmap.
- Stacks ONTO the capture rail: `DrawAsStream(width, SKEncodedImageFormat.Png, quality)` encodes a painter to an image `Stream` on the same `SKEncodedImageFormat` surface the raster-capture owner uses, so a math golden renders headlessly.
- Stacks ONTO the `typography` rail: `LocalTypefaces` accepts the `Typography.OpenFont.Typeface` chain the theme's embedded-font admission resolves, so math glyphs share the app's registered font set rather than a private math font.
