---
name: dotnet-coding-ui
description: "Use when drawing or hosting .NET UI, covering Skia surfaces, shaped text, vector art, color, and Eto controls"
---

# [DOTNET_CODING_UI]

Covers drawn and hosted .NET UI, from a surface's pixel contract to the seam a platform fills, with SkiaSharp, HarfBuzzSharp, RichTextKit, Svg.Skia, Unicolour, and Eto.Forms as vocabulary.

[REFERENCES]:
- [01]-[SKIA](references/skia.md): Deep use of SkiaSharp, from color-managed frames and layer cost to pictures, runtime effects, and GPU sharing
- [02]-[TEXT](references/text.md): Shaping, fallback, and layout composing one measured block, from HarfBuzz buffers to RichTextKit carets and edits
- [03]-[ETO](references/eto.md): Eto handler seams, native reach in one platform file, pixel interop without copies, lifecycle, input, and commands

## [01]-[NATIVE_OVER_DRAWN]

Drawn surfaces sit where no native control can (a viewport overlay, a plot, a tile gallery, a color or curve editor, a manipulated handle), every other control is the host's, and one toolkit owns accessibility, IME, undo, focus order, and theme:

| [INDEX] | [NEED]                                              | [FORM]                                                           |
| :-----: | :-------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | Text and number entry                               | `TextBox`, `TextArea`, `NumericStepper`, host unit-parsing field |
|  [02]   | Captions, readouts, row labels                      | `Label` cells in a `TableLayout`                                 |
|  [03]   | Lists, trees, grids                                 | `TreeGridView`, `GridView` with `CellFormatting` for emphasis    |
|  [04]   | Sections, tabs, panes, progress                     | `Expander`, `TabControl`, `Splitter`, `ProgressBar`              |
|  [05]   | Actions in menus, toolbars, popups                  | One `Command` into `MenuBar`, `ToolBar`, `ContextMenu`           |
|  [06]   | Overlay rail, gallery, plot, swatch or curve editor | `Drawable` holding one scene                                     |

- Drawn scenes hold one typed tree of parts, and the host projects that tree once to accessibility and tooltips, a second projection drifts
- Native controls seat inside a drawn scene through a `PixelLayout` slot the scene moves on each reconcile, the scene paints around them
- Text entry inside a scene is a seated `TextBox`, a drawn editor stands only in an overlay no native field can enter
- Dialogs are host dialogs, a modal show runs only when a person attends the session and an unattended run answers `Errors.Cancelled`
- Native controls take theme through their own properties, a scene reads one theme value projected per `ThemeChanged` as a `Fin`
- Labels, undo names, and spoken values derive from the operation and its target in the host culture at realization, localized once per tree

## [02]-[SURFACES]

One surface per view owns its pixels and its contract (`SKImageInfo`: device size, `SKColorType`, `SKAlphaType.Premul`, `SKColorSpace`), fixed before the first draw, and the host adapter blits a snapshot and nothing else:
- Raster serves panel UI repainted by damage region, GPU serves full-window animation, large images, and runtime shaders
- One scene paints raster and GPU targets through `SKCanvas`
- GPU snapshots are texture images, a per-frame CPU read of one blocks on `ReadPixels`, a raster snapshot shares pixels copy-on-write
- `Snapshot()` runs once per frame after the last draw, a snapshot alive during the next draw forces Skia to copy the surface
- Color flows by tags: sources carry their space, the destination carries the display space, and Skia converts on draw with no managed loop
- Destination `ColorSpace` left `null` disables conversion for every source, a `null` source is read as sRGB
- Scene-referred work draws into `SKColorType.RgbaF16` tagged `SKColorSpace.CreateSrgbLinear()`, values past 1.0 survive until the display blit
- `CreateSrgb()` and `CreateSrgbLinear()` return one dispose-protected instance each, `CreateRgb(transferFn, toXyzD50)` names a display gamut from D50 matrices
- `SKColorSpace.Equal` compares content, handles of one space from separate factories are equal
- Wide-gamut and float sources select the F16 target by `ColorSpace` and `ColorType` together, an 8-bit target clips both
- `SKSurface.Create(info, pixels, rowBytes)` draws into host memory with no copy, `SKImage.FromPixels(pixmap, releaseProc)` wraps memory the delegate keeps alive
- Canvas size is device pixels, `Scale(scale)` once per frame maps logical units, strokes and font sizes stay logical, every rasterized cache key holds the scale
- Damage accumulates in an `SKRegion` of device rects (`Op(rect, SKRegionOperation.Union)`), `ClipRegion` restricts the repaint, `SetEmpty` closes the frame
- Scrollable scenes lay out and paint the visible rect alone, a fitted height grown to content loads every tile
- `QuickReject(rect)` skips a draw outside the clip before its paint is built
- `SKSurfaceProperties(SKPixelGeometry)` enables LCD text on an opaque surface, a cleared transparent surface takes grayscale edging
- Offscreen rasters take their size from the clip intersected with the draw bounds, an unbounded `MapRect` sizes a layer by content

```csharp
internal sealed class Frame(SKImageInfo info, SKSurfaceProperties properties) : IDisposable {
    private readonly SKSurface surface = SKSurface.Create(info, properties);
    private readonly SKRegion damage = new(info.Rect);

    public void Damage(SKRectI device) => damage.Op(device, SKRegionOperation.Union);

    public SKImage Paint(Action<SKCanvas> draw, float scale) {
        using (SKAutoCanvasRestore scope = new(surface.Canvas, doSave: true)) {
            surface.Canvas.ClipRegion(damage);
            surface.Canvas.Scale(scale);
            draw(surface.Canvas);
        }
        damage.SetEmpty();
        return surface.Snapshot();
    }

    public void Dispose() {
        damage.Dispose();
        surface.Dispose();
    }
}
```

Linear F16 frames drawn onto a display-tagged surface convert on the blit, the compositor owns the transfer.

## [03]-[PAINT]

Paints are style objects built once, every effect on one is a reference-counted handle the building scope owns, and a draw loop allocates nothing:
- Reference-counted handles (`SKImage`, `SKPicture`, `SKShader`, `SKColorFilter`, `SKImageFilter`, `SKMaskFilter`, `SKPathEffect`, `SKTypeface`, `SKTextBlob`) share across paints and frames
- Single-owner objects (`SKPaint`, `SKPath`, `SKRoundRect`, `SKRegion`, `SKFont`, `SKPathBuilder`) are built per style or per layout and reused per draw
- Assigning a handle to a paint property or an `SKImageInfo` adds Skia's own reference, the builder still disposes its handle, a picture keeps its own references
- `SKPaint` text members and `SKFilterQuality` overloads are error-level obsolete, `SKFont` owns glyphs and `SKSamplingOptions` owns resampling
- One laid outline (`SKRoundRect` or `SKPath` from the corner data) feeds fill, stroke, clip, shadow input, focus ring, and `Contains` hit test, derived once per layout
- Strokes draw on the outline deflated by half the width (`Deflate`), corners stay concentric where a path offset distorts
- Continuous corners are a hand path, `SKPathEffect.CreateCorner` rounds with circular arcs
- `SaveLayer(limit, paint)` exists for group alpha, a blend mode over a group, or an image filter, `limit` hints the size and a clip bounds it
- Layers drop LCD text unless `SKCanvasSaveLayerRecFlags.PreserveLcdText` is set, grayscale edging inside layers avoids the mismatch
- `SKCanvasSaveLayerRec.Backdrop` filters what the live canvas already holds, inside a picture or offscreen it filters that recording alone
- `DrawPicture(picture, in matrix, paint)` with a paint costs a layer, replay passes `null` when no group effect applies
- Image filters on a paint run on an offscreen sized by their reach, mask filters run on coverage in place, so one draw's blur is a mask blur
- Shadows of a group are `SKImageFilter.CreateDropShadowOnly` under the content, an inset shadow is the blur of the inverted outline clipped to the shape
- Spread re-outlines the shape, `CreateDilate` and `CreateErode` grow square corners
- Blur takes sigma, `SKMaskFilter.ConvertRadiusToSigma` maps a radius and `SKMatrix.MapRadius` scales a sigma under the transform
- `CreateDropShadowOnly` and `SKColorFilter.CreateBlendMode` take `SKColor`, a float-tagged shadow or tint enters through `SKShader.CreateColor(SKColorF, space)`
- `ClipRoundRect` costs less than `ClipPath` over one shape, `antialias: true` buys a coverage mask, a device-snapped rect clips without one
- Gradient `SKColorF[]` overloads tag their stops, interpolation runs in the destination space, so a linear-light blend draws on a linear-tagged target
- `IsDither` removes 8-bit banding in gradients
- `SKColorFilter.CreateColorMatrix` runs a 4×5 row-major matrix over unpremultiplied RGBA in the destination space, linear work brackets with `CreateSrgbToLinearGamma`
- Runtime effects compile once into a static `SKRuntimeEffect`, `SKRuntimeShaderBuilder` binds uniforms per use, `layout(color)` uniforms convert to the working space
- Pictures record once per content change (`SKPictureRecorder.BeginRecording(cullRect, useRTree: true)`) and replay per frame, `SKImage.FromPicture` rasterizes when replay cost leads
- Cache keys hold what changes pixels: outline (corners, smoothing, size), shadow (outline, offset, sigma, color)
- Raster keys hold content, scale, and color space, picture keys hold asset and parameters
- Device-dependent caches (rasters, scaled text blobs) rebuild on a scale change, device-free caches (pictures, outlines, parsed art) survive it
- Reuse keys on content equality, a node holding delegates or handle references compares unequal every build and defeats every cache beneath it
- Animated inks enter through a layer paint or a color filter, a per-frame ink inside a raster key re-rasterizes every frame
- Hover and press ramps precompute at style time into a short table, a memo keyed on each animated `SKColorF` grows per frame
- `SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear)` downsamples, `SKCubicResampler.Mitchell` upsamples, `Nearest` keeps pixel art

```csharp
internal sealed class Outline : IDisposable {
    private readonly SKRoundRect shape;
    private readonly SKPath path = new();

    public Outline(SKRect frame, float radius) {
        shape = new SKRoundRect(frame, radius);
        path.AddRoundRect(shape);
    }

    public void Fill(SKCanvas canvas, SKPaint paint) => canvas.DrawRoundRect(shape, paint);

    public void Stroke(SKCanvas canvas, SKPaint paint) {
        using SKRoundRect inner = new(shape);
        inner.Deflate(paint.StrokeWidth / 2f, paint.StrokeWidth / 2f);
        canvas.DrawRoundRect(inner, paint);
    }

    public void Clip(SKCanvas canvas) => canvas.ClipRoundRect(shape);

    public bool Hit(SKPoint point) => shape.Rect.Contains(point) && path.Contains(point.X, point.Y);

    public void Dispose() {
        path.Dispose();
        shape.Dispose();
    }
}

internal static class Effects {
    private const string Source = "uniform float2 size; uniform float strength; half4 main(float2 p) { float2 q = p / size - 0.5; return half4(0, 0, 0, strength * dot(q, q)); }";
    private static readonly SKRuntimeEffect Vignette = SKRuntimeEffect.CreateShader(Source, out string _);

    public static SKShader Build(SKSize size, float strength) {
        using SKRuntimeShaderBuilder builder = new(Vignette);
        builder.Uniforms["size"] = size;
        builder.Uniforms["strength"] = strength;
        return builder.Build();
    }
}
```

## [04]-[VECTOR_ART]

Vector art loads once into an `SKPicture` and draws at any scale, tint and theme enter at load, and the loader runs at realization off the paint path:
- `SKSvg.CreateFromStream(stream, parameters)` parses and renders, `Picture` holds the result, `ReLoad(parameters)` rebuilds from the kept source on a theme change
- `SvgParameters(entities, css, currentColor)` resolves `currentColor`, fills XML entities, and injects a stylesheet before render
- Art holding its own colors takes its tint through `CurrentColor` at load, `SKColorFilter.CreateBlendMode(tint, SKBlendMode.SrcIn)` flattens every color and fits monochrome art alone
- `SkiaModel(SKSvgSettings)` and `SkiaSvgAssetLoader(model)` are built once per process, `SKSvg.ToPicture(fragment, model, loader)` renders one document through them
- `SKSvgSettings.Srgb` and `SrgbLinear` are the spaces the renderer tags its work with, `TypefaceProviders` resolves fonts named in art
- Picture caches key on (asset, parameters), raster caches on (asset, parameters, scale) through `SKImage.FromPicture(picture, size)`
- Pictures that leave the scene stay cached until memory pressure, a hover that re-parses art thrashes
- `SvgService.GetDimensions(fragment, viewport)` reads intrinsic size and `viewBox`, the viewport resolves percentage sizes
- `HitTestElements(point, canvasMatrix)` and `HitTestTopmostElement` hit test loaded art
- Disposing the `SKSvg` releases its picture, the picture alone is not disposed
- Host icons rasterize once per (asset, scale, theme) into a host `Bitmap` through locked bits, the previous icon is disposed on retheme, no PNG round trip

```csharp
internal sealed class Glyphs : IDisposable {
    private readonly Dictionary<(string Asset, SKColor Tint), SKSvg> loaded = [];

    public SKPicture Picture(string asset, Stream source, SKColor tint) {
        if (loaded.TryGetValue((asset, tint), out SKSvg? cached)) return cached.Picture!;
        Color current = Color.FromArgb(tint.Alpha, tint.Red, tint.Green, tint.Blue);
        SKSvg svg = SKSvg.CreateFromStream(source, new SvgParameters(null, null, current));
        loaded[(asset, tint)] = svg;
        return svg.Picture!;
    }

    public void Dispose() {
        foreach (SKSvg svg in loaded.Values) svg.Dispose();
    }
}
```

## [05]-[COLOR]

Unicolour states every color fact at design scale (a swatch, a palette, a theme ink, a LUT bake) under one `Configuration`, Skia receives `SKColorF` with its `SKColorSpace`, and a pixel pass bakes a matrix or curve once:
- `Configuration(rgbConfig, xyzConfig, ybrConfig, camConfig, dynamicRange, iccConfig)` fixes primaries and transfer, white point and adaptation, viewing conditions, and luminance range
- `Configuration.Default` is sRGB under D65 with `ChromaticAdaptation.Bradford`, `ConvertToConfiguration` adapts the white point and re-encodes under the target primaries
- `RgbConfiguration.DisplayP3`, `Rec2020`, `Rec2100Pq`, `Rec2100Hlg`, `Acescg` carry primaries, white, and transfer, `DynamicRange.Standard` and `High` set the luminance PQ and HLG encode against
- `Unicolour` instances enter from a `ColourSpace` triplet, a hex string, `Chromaticity` with luminance, a CCT with `Locus`, or an `Spd`
- Each instance allocates and evaluates representations on first read, one exists per swatch, style step, or table entry and none per pixel or frame
- `Blend(backdrop, BlendMode)` composites, `Mix(other, ColourSpace.Oklch, amount, HueSpan)` interpolates, `Palette(other, space, count)` returns the steps
- `Contrast(other)` is the WCAG ratio, `Difference(reference, DeltaE.Ok)` orders perceptual distance, `Ciede2000` and `Itp` serve print and HDR
- `MapToRgbGamut(GamutMap.OklchChromaReduction)` keeps hue and lightness, `RgbClipping` shifts both, `WxyPurityReduction` keeps dominant wavelength
- `IsInRgbGamut` tests before a bake, `Temperature` answers `Cct` and `Duv` with `IsValid` within the locus distance, `Simulate(Cvd, severity)` previews deficiency
- `Colourmaps.Viridis.Map(t)` and `Palette(count)` sample a perceptual map, `Css` holds named colors as statics and `Css.All`
- Pixel passes derive the matrix from `RgbLinear` and `Xyz` of the primaries and white, and a transfer table from `ToLinear(value, DynamicRange)`, into `Vector4` lanes or an SkSL uniform
- Colors in a style file are a typed union (fixed, host role, mix, contrast, relative) under JSON polymorphism, a string grammar restates structure
- Paint boundary: `SKColorF` from `Rgb` with the sRGB tag or from `RgbLinear` with the linear tag, `SKColor` from `Rgb.Byte255`, HSL math stays in `ColourSpace.Hsl`
- Text inks through RichTextKit `Style.TextColor` are `SKColor`, an 8-bit quantization a design accepts or avoids by drawing the blob through a float paint

```csharp
internal static class Inks {
    private static readonly Configuration Display = new(RgbConfiguration.DisplayP3, XyzConfiguration.D65);

    public static SKColorF Over(Unicolour ink, Unicolour ground) {
        Unicolour composed = ink.Blend(ground, BlendMode.Normal).MapToRgbGamut(GamutMap.OklchChromaReduction);
        return new SKColorF((float)composed.Rgb.R, (float)composed.Rgb.G, (float)composed.Rgb.B, (float)composed.Alpha.A);
    }

    public static Unicolour Accent(Unicolour source) => source.ConvertToConfiguration(Display);
}
```

## [06]-[TEXT]

Text becomes pixels through one measured block per text, sealed style, width, and scale: RichTextKit itemizes, shapes, breaks, and lays out, HarfBuzz is reached directly for a run that needs features or axes, and `SKFont` rasterizes:
- `TextBlock` lays out on the first measured read and relays out after any property write, a laid block is cached and painted, not rebuilt per paint
- Measurement happens once at the target width, a measure at infinite width followed by a layout at the real width doubles every shape
- `Style.Seal()` fixes a style for sharing across blocks and as a cache key, `Modify` derives the next style
- `TextPaintOptions` holds paint-time facts (`Edging`, `Hinting`, `SubpixelPositioning`, `Selection`) and changes no layout
- Changed ink is a paint fact, `Copy` and `ApplyStyle` relayout the block, so animated or contrast-adjusted text draws through a layer paint or color filter
- `MeasuredOverhang` extends ink past the advance box for invalidation and caret rects, `RenderWidth` sets alignment width apart from `MaxWidth`
- Indexes are code points, `CodePointToCharacterIndex` and `CharacterToCodePointIndex` bridge host `string` offsets
- Word and line stops come from `WordBoundaryIndicies`, `CaretIndicies`, and `LineBreaker`, a whitespace scan misses Unicode breaking rules
- `FontMapper.TypefaceFromStyle` resolves family, weight, width, and slant, an override returns app typefaces and falls through to `base` for the rest
- `FontFallback.GetFontRuns` splits a run per missing glyph range through `SKFontManager.MatchCharacter`, `bcp47` steers CJK unification
- RichTextKit shapes with its own HarfBuzz font at a fixed scale and no features, a run needing `tnum`, `liga`, `smcp`, or a `wght` axis reshapes through HarfBuzz
- One `Font` per face and axis set, `SetScale(UnitsPerEm, UnitsPerEm)` returns font units, and one multiply by `Size / UnitsPerEm` yields subpixel positions
- One reused `Buffer` per thread, `ClearContents()` keeps `Direction`, `Script`, and `Language` for the next run
- `AddUtf16(text, itemOffset, itemLength)` shapes one item with the whole string as context, kerning and joins hold across run edges
- `Feature(tag, value, start, end)` bounds a feature to clusters, `YAdvance` joins `XAdvance` for vertical scripts, `GlyphFlags.UnsafeToBreak` marks a reshape point
- `Font.SetVariationCoordsDesign(coords)` sets axes, the `SKTypeface` painting the glyphs carries the same variation or outlines and advances disagree
- HarfBuzz y grows up and Skia y grows down, positioned glyphs enter `SKTextBlobBuilder.AllocatePositionedRun(font, count)` and the blob is the cache value
- `SKFont.LinearMetrics = true` keeps advances unrounded under a scale, `Edging.SubpixelAntialias` renders LCD text over an opaque surface alone
- Host text fields own caret, selection, IME, and undo, `TextDocument` with `ITextDocumentView` edits text where no native field can sit

```csharp
internal sealed class RunShaper : IDisposable {
    private readonly Blob blob;
    private readonly Face face;
    private readonly Font font;
    private readonly Buffer buffer = new();

    public RunShaper(SKTypeface typeface) {
        blob = typeface.OpenStream(out int index).ToHarfBuzzBlob();
        face = new Face(blob, index);
        font = new Font(face);
        font.SetFunctionsOpenType();
        font.SetScale(face.UnitsPerEm, face.UnitsPerEm);
    }

    public SKTextBlob Shape(string text, SKFont target, params Feature[] features) {
        buffer.ClearContents();
        buffer.AddUtf16(text);
        buffer.GuessSegmentProperties();
        font.Shape(buffer, features);
        float scale = target.Size / face.UnitsPerEm;
        ReadOnlySpan<GlyphInfo> infos = buffer.GetGlyphInfoSpan();
        ReadOnlySpan<GlyphPosition> positions = buffer.GetGlyphPositionSpan();
        using SKTextBlobBuilder builder = new();
        SKPositionedRunBuffer run = builder.AllocatePositionedRun(target, infos.Length);
        Span<ushort> glyphs = run.GetGlyphSpan();
        Span<SKPoint> points = run.GetPositionSpan();
        float pen = 0f;
        for (int i = 0; i < infos.Length; i++) {
            glyphs[i] = (ushort)infos[i].Codepoint;
            points[i] = new SKPoint(pen + (positions[i].XOffset * scale), -positions[i].YOffset * scale);
            pen += positions[i].XAdvance * scale;
        }
        return builder.Build();
    }

    public void Dispose() {
        buffer.Dispose();
        font.Dispose();
        face.Dispose();
        blob.Dispose();
    }
}
```

## [07]-[HOST_SEAM]

Eto hosts the scene through one handler interface per capability that each platform implements in full, resolved once at construction, with every native cast inside one platform file:
- Capabilities are `new interface IHandler : Control.IHandler` on a widget, `Platform.Add<T>` registers the platform class, `Create<T>()` resolves it once
- `Create` throws on an unregistered type, a platform missing a member fails at launch, and the compiler holds each platform to the interface
- Delegates registered per fact and resolved per call through `Find<T>()` are a service locator, their absence on one platform surfaces as a refused event
- `Platform.Instance.IsMac` and its siblings, `ControlObject`, `NativeHandle`, and `ToNative()` are read inside the platform file alone, shared code reads the seam member
- `Style.Add<THandler>(name, handler)` runs once per widget whose `Style` matches when its handler is created and sets what the handler's `Control` exposes
- Native behavior a view needs is a native view subclass in the platform file, a class swap on a toolkit-owned view breaks when the toolkit changes
- Constructors build the control tree, `OnLoadComplete` subscribes host events (`ParentWindow` is set), `OnUnLoad` releases them, and a reload re-acquires
- `Window.Closing` cancels through `CancelEventArgs.Cancel`, `Closed` finishes and releases
- `Application.Instance.IsUIThread` tests the thread, `EnsureUIThread()` throws under `UIThreadCheckMode.Error` (a debugger attached) and warns otherwise
- `AsyncInvoke(action)` posts a write from a worker, `InvokeAsync(func)` returns its `Task`, `Invoke` blocks the caller and deadlocks a UI thread waiting on it
- `UITimer.Interval` is seconds as `double` and paces UI cadence and debounce, a frame clock is the platform display link behind the seam, paused when nothing animates
- Device scale is the control's `ParentWindow.LogicalPixelSize`, a maximum over `Screen.Screens` mismatches every other monitor
- `LogicalPixelSizeChanged` on the window rebuilds device-sized caches and backings, `SizeChanged` alone misses a monitor move
- Backing bitmaps measure logical size times scale, `Bitmap(size, PixelFormat.Format32bppRgba)` and `Lock()` give `BitmapData` with `Data`, `ScanWidth`, and `PremultipliedAlpha`
- `SKSurface.Create(info, data.Data, data.ScanWidth)` draws into the locked bits, the seam derives `SKColorType` once from `TranslateArgbToData` of a pure red
- `BitmapData` disposes before `Graphics.DrawImage`, a platform publishes the pixels on unlock and a draw during the lock shows stale or undefined pixels
- `Graphics.ImageInterpolation = ImageInterpolation.None` blits a device-sized bitmap into logical bounds as a copy
- `TextInput` delivers typed text after layout and IME, `KeyDown` carries the physical key for navigation and sets `Handled` on consumed keys alone
- `CaptureMouse()` and `ReleaseMouseCapture()` bracket a drag so leaving the view keeps delivering moves, `MouseWheel` `Delta` is fractional lines on a trackpad
- `Application.Instance.CommonModifier` names the shortcut key, `Keys.KeyMask` and `Keys.ModifierMask` split `KeyData`
- One `Command` per action converts implicitly to `MenuItem` and `ToolItem`, `Shortcut` dispatches through the menu, `Enabled` written once updates every item
- `MapPlatformCommand("cut", command)` binds the platform edit commands, `CheckCommand` and `RadioCommand.Controller` cover toggles and groups
- `Application.Instance.Theme` and `ThemeChanged` signal appearance, icons re-rasterize per (scale, theme) and the previous `Icon` disposes
- `TableLayout` scale cells take remaining space, `SuspendLayout()` and `ResumeLayout()` bracket a batch of moves, a `Drawable` under `Scrollable` paints `VisibleRect` alone
- `Drawable.Height` written from a preferred size runs once per content change, a write per paint relayouts the parent per frame

```csharp
internal sealed class SceneCanvas(SKColorType order, Action<SKCanvas> draw) : Drawable {
    private Bitmap? backing;
    private Window? window;

    protected override void OnLoadComplete(EventArgs e) {
        base.OnLoadComplete(e);
        window = ParentWindow;
        window.LogicalPixelSizeChanged += Rescaled;
    }

    protected override void OnUnLoad(EventArgs e) {
        if (window is { } attached) attached.LogicalPixelSizeChanged -= Rescaled;
        window = null;
        base.OnUnLoad(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
        float scale = ParentWindow.LogicalPixelSize;
        Size pixels = new((int)MathF.Ceiling(Width * scale), (int)MathF.Ceiling(Height * scale));
        if (backing is null || backing.Size != pixels) {
            backing?.Dispose();
            backing = new Bitmap(pixels, PixelFormat.Format32bppRgba);
        }
        using (BitmapData data = backing.Lock()) {
            SKAlphaType alpha = data.PremultipliedAlpha ? SKAlphaType.Premul : SKAlphaType.Unpremul;
            SKImageInfo info = new(pixels.Width, pixels.Height, order, alpha);
            using SKSurface surface = SKSurface.Create(info, data.Data, data.ScanWidth);
            surface.Canvas.Scale(scale);
            draw(surface.Canvas);
        }
        e.Graphics.ImageInterpolation = ImageInterpolation.None;
        e.Graphics.DrawImage(backing, new RectangleF(0f, 0f, Width, Height));
    }

    private void Rescaled(object? sender, EventArgs e) => Invalidate();
}
```

## [08]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                                    |
| :-----: | :------------------------------------------------------------------ | :---------------------------------------------------------------- |
|  [01]   | Drawn text field, tabs, expander, splitter, progress, or caption    | `TextBox`, `TabControl`, `Expander`, `Splitter`, `Label`          |
|  [02]   | Second scene raster host per surface or per host                    | One surface type owning allocation, damage, and paint             |
|  [03]   | Per-texel gamut or transfer loop in managed code before a blit      | Source and destination tagged, Skia converts on draw              |
|  [04]   | Target format selected by `ColorType` alone                         | `ColorType` and `ColorSpace` together select the F16 target       |
|  [05]   | `Snapshot()` between draws, or one snapshot per blit path           | One snapshot after the frame's last draw                          |
|  [06]   | `SKPaint`, `SKPath`, `SKFont`, or filter allocated in a draw loop   | Built per style or per layout, reused per draw                    |
|  [07]   | Shadow, clip, stroke, and hit test from separate geometry           | One outline feeding every derivation, cached per layout           |
|  [08]   | Inset shadow drawn as a drop shadow clipped `Difference`            | Blur of the inverted outline clipped `Intersect` to the shape     |
|  [09]   | `SaveLayer()` around every group, or `DrawPicture` with a paint     | `SaveLayer(limit, paint)` for an effect, `null` picture paint     |
|  [10]   | `ClipPath(path, antialias: true)` per child on a plain rounded rect | `ClipRoundRect` once around the children                          |
|  [11]   | Backdrop blur recorded inside a picture or offscreen                | Backdrop filter on the live canvas alone                          |
|  [12]   | Cache key missing scale or color space, or keyed on a reference     | Key on content, scale, and target space                           |
|  [13]   | One cache cleared on every size or scale change                     | Device-free caches survive, device-dependent caches rebuild       |
|  [14]   | `Mix` or `Contrast` per hover frame, memo keyed on animated colors  | Ramp table at style time, ink through a layer paint               |
|  [15]   | `SKRuntimeEffect.CreateShader` per frame                            | Effect compiled once as a static, builder per use                 |
|  [16]   | `SKFilterQuality` overloads or `SKPaint.TextSize`                   | `SKSamplingOptions`, `SKFont`                                     |
|  [17]   | Gradient colors as `SKColor` on a linear target                     | `SKColorF[]` tagged with their `SKColorSpace`                     |
|  [18]   | `SrcIn` tint filter over art that holds its own colors              | `SvgParameters.CurrentColor` at load, tint for monochrome art     |
|  [19]   | SVG parsed on the paint thread, or rasterized per draw              | Picture at realization, image per (asset, parameters, scale)      |
|  [20]   | `new Unicolour` inside a pixel or sample loop                       | Matrix or curve derived once, baked into a span kernel or shader  |
|  [21]   | Hand alpha composite, white point adaptation, or CCT fit            | `Blend`, `ConvertToConfiguration`, `Temperature.FromChromaticity` |
|  [22]   | Hand RGB clipping of an out-of-gamut color                          | `MapToRgbGamut(GamutMap.OklchChromaReduction)`                    |
|  [23]   | CSS color string grammar in a style file                            | Typed ink union under JSON polymorphism                           |
|  [24]   | `TextBlock` laid out per paint, measured at infinity then at width  | Laid block cached by text, sealed style, width, and scale         |
|  [25]   | `Copy` and `ApplyStyle` to recolor text per frame                   | Layer paint or color filter over the laid block                   |
|  [26]   | Whitespace scan or `Split(' ')` for wrap and word points            | `WordBoundaryIndicies`, `LineBreaker`                             |
|  [27]   | Glyph positions scaled by hand after a shape at pixel scale         | `SetScale` to `UnitsPerEm`, one multiply by `Size / UnitsPerEm`   |
|  [28]   | `Blob`, `Face`, `Font`, or `Buffer` built per paint or weakly held  | One `Font` per face and axis set in a scene-owned cache           |
|  [29]   | `SKShaper.Shape` followed by a second shape for features            | One `Font.Shape(buffer, features)`                                |
|  [30]   | `Platform.Add<TDelegate>` per fact, `Find<T>()` at each call site   | One `IHandler` per capability, `Create<T>()` at construction      |
|  [31]   | `IsMac` branch or `ControlObject` cast in shared code               | Seam member the platform file implements                          |
|  [32]   | Runtime class swap on a toolkit-owned native view                   | Native view subclass in the platform file                         |
|  [33]   | `BitmapData` held for the bitmap's life, drawn while locked         | Lock, draw through Skia, dispose, then `Graphics.DrawImage`       |
|  [34]   | Logical size as the backing size, scale from the largest screen     | Logical size times the window's `LogicalPixelSize`                |
|  [35]   | `Thread` or `Task` body touching a control, `Invoke` from a worker  | `AsyncInvoke` or `InvokeAsync` onto the UI thread                 |
|  [36]   | `UITimer` as an animation frame clock                               | Display link behind the seam, `UITimer` for cadence               |
|  [37]   | Host subscriptions or realization in a constructor                  | `OnLoadComplete` subscribes and realizes, `OnUnLoad` releases     |
|  [38]   | One `Command` per menu item, button, and context item               | One `Command` converted to `MenuItem` and `ToolItem`              |
|  [39]   | Shortcut dispatched by hand in `KeyDown`                            | `Command.Shortcut`, `MapPlatformCommand` for edit commands        |
|  [40]   | Icon re-rasterized per cell bind, or replaced without disposal      | Icon per (asset, scale, theme), previous disposed on retheme      |
|  [41]   | `Graphics` plots, captions, or glyphs beside a Skia scene           | One Skia scene in the `Drawable`, `Graphics` blits alone          |
