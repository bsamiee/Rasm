# [RASM_APPUI_API_CSHARPMATH_SKIA]

`CSharpMath.SkiaSharp` renders TeX-subset math and mixed math-text runs onto an `SKCanvas` through `MathPainter` (a `MathList`) and `TextPainter` (a `TextAtom` run), each a `SKCanvas`/`SKColor` binding of the backend-agnostic `Painter<TCanvas,TContent,TColor>` base that owns the knob surface, the typed `Result` parse rail, and the measure/draw contract. A Math typography arm sets `LaTeX`, reads `Measure`, and draws into a leased `SKCanvas`; a parse failure lands in `ErrorMessage` instead of throwing, and the same painter encodes headless to an image `Stream` for capture.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `CSharpMath.SkiaSharp` over `CSharpMath.Rendering` and `CSharpMath`
- package: `CSharpMath.SkiaSharp` (MIT, CSharpMath) direct; `CSharpMath.Rendering`, `CSharpMath.Editor`, and `CSharpMath` transitive
- assembly: `CSharpMath.SkiaSharp` (painters, `ICanvas`/`Path` adapters, color/encode extensions), `CSharpMath.Rendering` (`Painter` base, `MathKeyboard`, vendored `Typography.OpenFont` glyph engine), `CSharpMath` (`LaTeXParser`, atom/display model, `Result<T>`)
- namespace: `CSharpMath.SkiaSharp`, `CSharpMath.Rendering.FrontEnd` (`Painter`, `ICanvas`, `Path`, `MathKeyboard`, enums), `CSharpMath.Atom` (`LaTeXParser`, `LaTeXSettings`, `LineStyle`), `CSharpMath.Structures` (`Result<T>`)
- rail: typography

## [02]-[PUBLIC_TYPES]

[SKIASHARP_PAINTER_TYPES]: SkiaSharp-bound painters and draw adapters — `CSharpMath.SkiaSharp`.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                       |
| :-----: | :------------ | :------------ | :------------------------------------------------- |
|  [01]   | `MathPainter` | class         | `SKCanvas`/`SKColor` binding of the math-list base |
|  [02]   | `TextPainter` | class         | `SKCanvas`/`SKColor` binding of the text-atom base |
|  [03]   | `SkiaCanvas`  | sealed class  | `SKCanvas`/`SKPaint` draw adapter                  |
|  [04]   | `SkiaPath`    | sealed class  | glyph-outline translator into `SKPath`             |
|  [05]   | `Extensions`  | static class  | `SKColor`↔`Color` bridge and image-stream encode   |

[FRONTEND_BASE_TYPES]: Backend-agnostic painter base, editing keyboard, canvas/path contracts, and layout enums — `CSharpMath.Rendering.FrontEnd`.

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                        |
| :-----: | :--------------------------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Painter<TCanvas,TContent,TColor>` | abstract      | knob, measure, and draw contract (`ICSharpMathAPI`) |
|  [02]   | `MathPainter<TCanvas,TColor>`      | abstract      | math-list painter base (`TContent`=`MathList`)      |
|  [03]   | `TextPainter<TCanvas,TColor>`      | abstract      | text-atom painter base (`TContent`=`TextAtom`)      |
|  [04]   | `MathKeyboard`                     | class         | interactive edit keyboard with caret draw           |
|  [05]   | `ICanvas`                          | interface     | backend draw contract painters render through       |
|  [06]   | `Path`                             | abstract      | glyph-outline sink (`IGlyphTranslator`)             |
|  [07]   | `PainterConstants`                 | static class  | `DefaultFontSize` 14, `LargerFontSize` 50           |
|  [08]   | `PaintStyle`                       | enum          | `Fill`, `Stroke`                                    |
|  [09]   | `TextAlignment`                    | flags enum    | nine-way box alignment                              |
|  [10]   | `CaretShape`                       | enum          | `IBeam`, `UpArrow`                                  |

[CORE_PARSE_TYPES]: LaTeX parse, layout style, and the typed result rail — `CSharpMath.Atom` and `CSharpMath.Structures`.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]                                               |
| :-----: | :-------------- | :-------------- | :--------------------------------------------------------- |
|  [01]   | `LaTeXParser`   | class           | LaTeX string ↔ `Result<MathList>`                          |
|  [02]   | `LaTeXSettings` | static class    | command dictionary, placeholder, font policy, `ParseColor` |
|  [03]   | `LineStyle`     | enum            | `Display`, `Text`, `Script`, `ScriptScript`                |
|  [04]   | `Result<T>`     | readonly struct | `Error`-carrying success/failure rail with `Match`/`Bind`  |

[ALIGNMENT_VALUES]: `TextAlignment` is a `[Flags] byte` packing vertical (`Top` 2, `Bottom` 1, `Center` 0) in the low two bits and horizontal (`Left` 8, `Right` 4) in bits 2-3; the four corners OR the two (`TopLeft` 10, `TopRight` 6, `BottomLeft` 9, `BottomRight` 5).

[RESULT_RAIL_SURFACE]: `Result<T>` carries a nullable `Error` string, constructs through implicit conversions from `T` or `string`, is consumed by `Deconstruct(out T, out string?)` and `Match(Func<T,R>, Func<string,R>)`, and chains through `Bind` — a parse consumer never catches an exception.

## [03]-[ENTRYPOINTS]

[PAINTER_KNOBS]: `Painter<TCanvas,TContent,TColor>` owns the shared knob surface both painters inherit.

| [INDEX] | [SURFACE]                                              | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :----------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `LaTeX : string?` / `Content : TContent?`              | property | source LaTeX or pre-parsed content                |
|  [02]   | `FontSize : float` / `Magnification : float`           | property | type size and zoom, `Magnification` default 1     |
|  [03]   | `TextColor` / `HighlightColor` / `ErrorColor : TColor` | property | glyph, selection, and error colors                |
|  [04]   | `GlyphBoxColor : (TColor glyph, TColor textRun)?`      | property | per-box debug tint                                |
|  [05]   | `PaintStyle : PaintStyle` / `LineStyle : LineStyle`    | property | fill/stroke and display/script style              |
|  [06]   | `ErrorFontSize : float?` / `DisplayErrorInline : bool` | property | error size and inline policy, inline default true |
|  [07]   | `ErrorMessage : string?` (get)                         | property | parse-failure message, null on success            |
|  [08]   | `LocalTypefaces : IEnumerable<Typeface>`               | property | fallback font chain                               |
|  [09]   | `Measure(float) : RectangleF`                          | instance | measured bounds at a canvas width                 |
|  [10]   | `WrapColor` / `UnwrapColor` / `WrapCanvas`             | instance | backend color and canvas adapters                 |
|  [11]   | `ShallowClone() : Painter`                             | instance | memberwise painter copy                           |

[SKIA_DRAW_ENTRYPOINTS]: `MathPainter` and `TextPainter` own the `SKCanvas` draw surface, `AntiAlias` default true on both.

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :--------------------------------------------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `AntiAlias : bool`                                                     | property | `SKPaint.IsAntialias` toggle, both painters        |
|  [02]   | `MathPainter.Draw(SKCanvas, TextAlignment, Thickness, float, float)`   | instance | aligned box draw, alignment default `Center`       |
|  [03]   | `MathPainter.Draw(SKCanvas, float, float)` / `Draw(SKCanvas, SKPoint)` | instance | absolute-origin draw                               |
|  [04]   | `MathPainter.DrawDisplay(IDisplay?, SKCanvas, …)`                      | instance | pre-measured display draw                          |
|  [05]   | `MathPainter.Display : IDisplay?`                                      | property | typeset display tree                               |
|  [06]   | `TextPainter.Draw(SKCanvas, TextAlignment, Thickness, float, float)`   | instance | text-run aligned draw, alignment default `TopLeft` |

[COLOR_AND_ENCODE]: `Extensions` bridges Skia color and encodes a painter headless; `DrawAsStream` defaults width 2000, `SKEncodedImageFormat.Png`, quality 100, `TextAlignment.TopLeft`.

| [INDEX] | [SURFACE]                                                                           | [SHAPE] | [CAPABILITY]                 |
| :-----: | :---------------------------------------------------------------------------------- | :------ | :--------------------------- |
|  [01]   | `ToNative(this Color) : SKColor`                                                    | static  | color-model bridge           |
|  [02]   | `FromNative(this SKColor) : Color`                                                  | static  | color-model bridge           |
|  [03]   | `DrawAsStream<TContent>(float, SKEncodedImageFormat, int, TextAlignment) : Stream?` | static  | headless image-stream encode |

[EDIT_AND_PARSE]: `MathKeyboard` drives interactive editing; `LaTeXParser` and `LaTeXSettings` own parse, serialize, and color-token resolution.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :--------------------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `MathKeyboard(float = 14, double = 800)`                   | ctor     | caret-blinking edit keyboard |
|  [02]   | `MathKeyboard.DrawCaret(ICanvas, Color, CaretShape)`       | instance | caret render                 |
|  [03]   | `LaTeXParser.MathListFromLaTeX(string) : Result<MathList>` | static   | one-call LaTeX → math list   |
|  [04]   | `LaTeXParser.Build() : Result<MathList>`                   | instance | instance parse               |
|  [05]   | `LaTeXParser.MathListToLaTeX(MathList, StringBuilder?)`    | static   | math list → LaTeX serialize  |
|  [06]   | `LaTeXSettings.ParseColor(string?) : Color?`               | static   | LaTeX color-token parse      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Painters render through the base `ICanvas` contract, `SkiaCanvas` binding it to `SKCanvas`/`SKPaint`; no math-render path reaches `SKPaint`/`SKFont` directly.
- LaTeX parse is a typed rail: `LaTeXParser.MathListFromLaTeX` returns `Result<MathList>`, and setting `Painter.LaTeX` routes a parse failure into `ErrorMessage` under `ErrorColor`/`ErrorFontSize`/`DisplayErrorInline`, never a throw; a null `Display` beside a non-null `ErrorMessage` is the failure signal.

[STACKING]:
- `api-skiasharp`(`.api/api-skiasharp.md`): painters draw through `SKCanvas`/`SKPaint`/`SKPath`/`SKPoint`/`SKColor` and `Extensions.ToNative`/`FromNative` bridge `SKColor`↔`Color`; `DrawAsStream(width, SKEncodedImageFormat.Png, quality)` encodes a painter headless on the same `SKEncodedImageFormat` surface the raster-capture owner shares, so a math golden renders without a live host.
- `api-avalonia-skia`(`.api/api-avalonia-skia.md`): `SkiaCanvas(SKCanvas, antiAlias)` binds over the `SKCanvas` an `ISkiaSharpApiLease.SkCanvas` exposes, so math composites into the leased surface without a side bitmap.
- Within-lib typography: `LocalTypefaces` accepts the `Typography.OpenFont.Typeface` chain the theme's embedded-font admission resolves, so math glyphs share the app's registered font set rather than a private math font.

[LOCAL_ADMISSION]:
- `CSharpMath.SkiaSharp` is the branch's sole math typesetter; a Math typography arm holds one painter, sets `LaTeX`, reads `Measure`, and draws into the leased `SKCanvas`, `LineStyle` selecting display vs inline sizing and `PainterConstants.DefaultFontSize`/`LargerFontSize` anchoring the size ramp.

[RAIL_LAW]:
- Package: `CSharpMath.SkiaSharp`
- Owns: TeX-subset math and mixed math-text layout, the painter knob surface (`LaTeX`, `FontSize`, `Magnification`, `TextColor`, `LineStyle`), measure and aligned `Draw` onto an `SKCanvas`, interactive editing through `MathKeyboard`, and the typed `Result`/`ErrorMessage` parse rail.
- Accept: one painter per Math arm, its parse consumed through `Result<T>.Match`/`Deconstruct` or an observed `ErrorMessage` folded into the diagnostics rail's typed-fault envelope; `FontSize`/`Magnification` and the `PainterConstants` anchors parameterize size.
- Reject: a hand-rolled TeX box model or glyph layout; a private `SKPaint`/`SKFont` math path bypassing `SkiaCanvas`; a `try`/`catch` around `LaTeX` assignment; a literal font size.
