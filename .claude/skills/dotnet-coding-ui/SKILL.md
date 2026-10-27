---
name: dotnet-coding-ui
description: "Use when drawing or hosting .NET UI, covering Skia surfaces, shaped text, vector art, color, and Eto controls"
---

# [DOTNET_CODING_UI]

Drawn and hosted .NET UI over SkiaSharp, HarfBuzzSharp, RichTextKit, Svg.Skia, Unicolour, and Eto.Forms, from a surface's pixel contract to the handler seam a platform fills. Eto members come from the `Eto` assembly RhinoCommon installs.

[REFERENCES]:
- [01]-[SKIA](references/skia.md): SkiaSharp ownership, color management, layers and filters, effects, pictures, GPU surfaces, text, and documents
- [02]-[TEXT](references/text.md): HarfBuzz objects and buffer protocol, features and variations, RichTextKit blocks, fallback, carets, and editing
- [03]-[ETO](references/eto.md): Eto handler seams, lifecycle, threading, scale, pixels, input, commands, layout, windows, and theme

## [01]-[NATIVE_OVER_DRAWN]

Drawn surfaces sit where no native control can (viewport overlay, plot, tile gallery, color or curve editor, manipulated handle), every other control is the host's and owns accessibility, IME, undo, focus order, and theme:

| [INDEX] | [NEED]                                         | [FORM]                                                           |
| :-----: | :--------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | Text and number entry                          | `TextBox`, `TextArea`, `NumericStepper`, host unit-parsing field |
|  [02]   | Captions, readouts, row labels                 | `Label` cells in a `TableLayout`                                 |
|  [03]   | Lists, trees, grids                            | `TreeGridView`, `GridView` with `CellFormatting` for emphasis    |
|  [04]   | Sections, tabs, panes, progress                | `Expander`, `TabControl`, `Splitter`, `ProgressBar`              |
|  [05]   | Actions in menus, toolbars, popups             | One `Command` into `MenuBar`, `ToolBar`, `ContextMenu`           |
|  [06]   | Toolbar in a panel or window                   | Eto `ToolBar`, a drawn toolbar stands over a viewport alone      |
|  [07]   | Overlay, gallery, plot, swatch or curve editor | `Drawable` holding one scene                                     |

- Drawn scenes hold one typed tree of parts, projected once to accessibility and tooltips, a second projection drifts
- Accessibility roles, actions, and values map once per part kind in the platform file, one native element per tree element
- Native controls inside a drawn scene are one case of the scene's placed-control type, held in a `PixelLayout` the scene moves on each reconcile
- Text entry inside a scene is a placed `TextBox`, a drawn editor stands only in an overlay no native field can enter
- Dialogs are host dialogs, a modal show runs only when a person attends the session and an unattended run answers `Errors.Cancelled`
- Native controls take theme through their own properties, a scene reads one theme value projected per `ThemeChanged` as a `Fin`
- Host theme colors enter the projection as `SKColorF`, `SystemColors.Highlight` supplies an accent where a host exposes no accent setting
- Icon pairs derive from `Assembly.GetManifestResourceNames()`, a `<name>_darkmode` resource beside `<name>` selects by theme
- Labels, undo names, and spoken values derive from the operation and its target in the host culture at realization, localized once per tree

## [02]-[SURFACES]

One surface type per scene owns its pixels and its `SKImageInfo` contract (device size, `SKColorType`, `SKAlphaType.Premul`, `SKColorSpace`) fixed before the first draw. Host adapters forward input and blit a snapshot:
- Raster serves panel UI repainted by damage region, GPU serves full-window animation, large images, and runtime shaders
- One scene paints raster and GPU targets through `SKCanvas`
- `SKSurface.Create` answers `null` for an info its backend refuses, an allocation returns a `Fault` under `Reason.Refused` naming the info and its host shows it
- `SKSurface.Create(info, pixels, rowBytes, releaseProc, context)` draws into host memory and releases it with the surface, one object owns both
- `SKPixmap(info, pointer, rowBytes)` views host bits without a copy, `SKSurface.Create(pixmap, props)` draws into them
- `Flush(submit: true)` submits GPU work, a raster surface draws synchronously and a `Flush` on it does nothing
- GPU snapshots are texture images, a per-frame CPU read of one blocks on `ReadPixels`
- Extended-range frames reach a screen through its platform's float-capable layer, an 8-bit host bitmap clips them on the blit
- Frame clocks run while a scene animates and pause on the first idle frame, a timer ticking an idle scene repaints nothing
- `Snapshot()` runs once per frame after its last draw, a snapshot alive during a later draw forces Skia to copy the surface
- Color flows by tags: sources hold their space, the destination holds the display space, Skia converts on draw with no managed loop
- Destination `ColorSpace` left `null` disables conversion for every source, a `null` source is read as sRGB
- Gamut tags derive from primaries through `SKColorSpacePrimaries.ToColorSpaceXyz(out toXyzD50)`, a D65-relative matrix tags the wrong white
- Scene-referred work draws into `SKColorType.RgbaF16` tagged `SKColorSpace.CreateSrgbLinear()`, values past 1.0 survive until the display blit
- Linear F16 frames drawn onto a display-tagged surface convert on their blit, the compositor owns that transfer
- Wide-gamut and float sources select the F16 target by `ColorSpace` and `ColorType` together, an 8-bit target clips both
- Sample buffers become one `SKImage.FromPixelCopy` per content change, owned and disposed by their producer
- Buffer-to-buffer conversion is `SKPixmap.ReadPixels`, an `SKImage` and `SKBitmap` round trip restates it
- Canvas size is device pixels, `Scale(scale)` once per frame maps logical units, strokes and font sizes stay logical
- Damage accumulates in an `SKRegion` through `Op(rect, SKRegionOperation.Union)`, `ClipRegion` restricts the repaint, `SetEmpty` closes the frame
- `SKRectI.Ceiling(rect, outwards: true)` bounds damage and clips, `SKRectI.Round(rect)` places a frame, hand floors and ceils restate them
- `ClipRegion` is device space and ignores the matrix, the damage clip precedes `Scale`
- Scrollable scenes lay out and paint the visible rect alone, a fitted height grown to content loads every tile
- `QuickReject(rect)` tests transformed geometry against the clip and skips a draw before its paint is built
- `SKSurfaceProperties(SKPixelGeometry)` enables LCD text on an opaque surface, a cleared transparent surface takes grayscale edging
- Text raster options derive from surface properties, `IsUseDeviceIndependentFonts` selects no hinting and `PixelGeometry.Unknown` grayscale edging
- Offscreen rasters take their size from the clip intersected with the draw bounds, an unbounded `MapRect` sizes a layer by content

```csharp
internal sealed class Frame : IDisposable {
    private readonly SKSurface surface;
    private readonly SKRegion damage;

    private Frame(SKSurface surface, SKImageInfo info) {
        this.surface = surface;
        damage = new SKRegion(info.Rect);
    }

    public static Fin<Frame> Allocated(SKImageInfo info, SKSurfaceProperties properties) =>
        Surfaced(info, () => SKSurface.Create(info, properties)).Map(made => new Frame(made, info));

    private static Fin<SKSurface> Surfaced(SKImageInfo info, Func<SKSurface> create) =>
        Optional(create()).ToFin(new Fault(string.Create(CultureInfo.InvariantCulture, $"{info.Width}x{info.Height} {info.ColorType}"), Reason.Refused));

    public void Damage(SKRect logical, float scale) =>
        damage.Op(SKRectI.Ceiling(SKMatrix.CreateScale(scale, scale).MapRect(logical), outwards: true), SKRegionOperation.Union);

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

## [03]-[PAINT]

Paints are style objects built once, every effect on one is a reference-counted handle the building scope owns, and a draw loop allocates nothing:
- Geometry is `SKRect`, `SKRoundRect`, `SKRegion`, and `SKMatrix`, a record of four edges or nine cells restates one and loses its members
- One outline (`SKRoundRect` or `SKPath`) per layout feeds fill, stroke, clip, shadow, focus ring, backdrop, and `Contains` hit test
- Strokes draw on the outline deflated by half the width (`Deflate`), corners stay concentric where a path offset distorts
- `SKRoundRect.SetRectRadii` scales over-long radii itself, `Inflate` and `Deflate` grow a rim or spread, a hand fit or offset restates them
- Continuous corners are a hand path, `SKPathEffect.CreateCorner` rounds with circular arcs
- `SKMatrix.TryInvert` runs once per matrix change and the inverse is held, a fresh inverse per mapped point pays the inversion per point
- `SaveLayer(limit, paint)` serves group alpha, a blend mode over a group, or an image filter
- Layers drop LCD text unless `SKCanvasSaveLayerRecFlags.PreserveLcdText` is set, grayscale edging inside layers avoids the mismatch
- `SKCanvasSaveLayerRec.Backdrop` filters what the live canvas already holds, inside a picture or offscreen it filters that recording alone
- Image filters on a paint run on an offscreen sized by their reach, mask filters run on coverage in place, one draw's blur is a mask blur
- Shadows are one `SKImageFilter` graph per shadow, built once per (shadow, color) and applied through a bounded layer around the outline draw
- Drop and inset are data on one shadow type (the alpha row sign and a final `SrcIn`), a renderer per case restates the graph
- Inset shadows invert an outline's alpha through a color matrix, then offset, blur, and `CreateBlendMode(SrcIn)` clip the result to its shape
- Spread re-outlines the shape, `CreateDilate` and `CreateErode` grow square corners
- Blur takes sigma, `SKMaskFilter.ConvertRadiusToSigma` maps a radius and `SKMatrix.MapRadius` scales a sigma under the transform
- Filters on a layer paint map sigma, offset, and morphology through the CTM, `SKImage.ApplyImageFilter` runs offline on the CPU with the CTM lost
- `CreateDropShadowOnly` and `SKColorFilter.CreateBlendMode` quantize to `SKColor`, a float color enters `CreateColorMatrix` in its last column
- `SKPaint.GetFastBounds` over the filter handle answers a filtered draw's reach, one probe per filter graph
- `ClipRoundRect` costs less than `ClipPath` over one shape, `antialias: true` buys a coverage mask, a device-snapped rect clips without one
- `IsDither` removes 8-bit banding in gradients
- Runtime effects compile once through `SKRuntimeEffect.BuildShader` into a static builder, each use binds its own uniforms and children into `Effect.ToShader`
- `WithLocalMatrix` on a built shader makes a second handle per use
- Pictures record once per content change through `SKPictureRecorder.BeginRecording(cullRect, useRTree: true)` and replay per frame
- `SKImage.FromPicture` rasterizes a picture when replay cost leads
- Outline keys hold corners, smoothing, and size, shadow keys add offset, sigma, and color
- Raster keys hold content, scale, color type, and color space, one key field per pixel input
- Scroll edge fades draw a `DstIn` gradient over the children's layer, one `SaveLayer` per faded container
- Placed native controls punch their frame with `SKBlendMode.Clear`, the host control fills the hole
- Series strokes take `SKPathEffect.CreateDash` rhythms scaled by the pen width, one effect per series per layout
- Rasters and scaled text blobs rebuild on a scale change, pictures, outlines, parsed art, and laid text survive it
- Reuse keys on content equality, a node holding delegates or handle references compares unequal every build and defeats every cache beneath it
- Animated colors enter through a layer paint or a color filter, a per-frame color inside a raster key re-rasterizes every frame
- Hover and press ramps precompute at style time into a short table, a memo keyed on each animated `SKColorF` grows per frame

```csharp
internal sealed class Outline : IDisposable {
    private readonly SKRoundRect shape;
    private readonly SKRoundRect rim;
    private readonly SKPath path;

    public Outline(SKRect frame, float radius, float strokeWidth) {
        shape = new SKRoundRect(frame, radius);
        rim = new SKRoundRect(shape);
        rim.Deflate(strokeWidth / 2f, strokeWidth / 2f);
        using SKPathBuilder builder = new();
        builder.AddRoundRect(shape);
        path = builder.Detach();
    }

    public void Fill(SKCanvas canvas, SKPaint paint) => canvas.DrawRoundRect(shape, paint);

    public void Stroke(SKCanvas canvas, SKPaint paint) => canvas.DrawRoundRect(rim, paint);

    public void Clip(SKCanvas canvas) => canvas.ClipRoundRect(shape, antialias: true);

    public bool Hit(SKPoint point) => path.Contains(point.X, point.Y);

    public void Dispose() {
        path.Dispose();
        rim.Dispose();
        shape.Dispose();
    }
}

internal static class Effects {
    private const string Source = "uniform float2 size; layout(color) uniform half4 tint; half4 main(float2 p) { float2 q = p / size - 0.5; half4 c = tint * half(dot(q, q)); return half4(c.rgb * c.a, c.a); }";
    private static readonly SKRuntimeShaderBuilder Vignette = SKRuntimeEffect.BuildShader(Source);

    public static SKShader Build(SKSize size, SKColorF tint, SKMatrix placement) {
        using SKRuntimeEffectUniforms uniforms = new(Vignette.Effect) { ["size"] = size, ["tint"] = tint };
        using SKRuntimeEffectChildren children = new(Vignette.Effect);
        return Vignette.Effect.ToShader(uniforms, children, placement);
    }
}
```

## [04]-[VECTOR_ART]

Vector art loads once into an `SKPicture` and draws at any scale, tint and theme enter at load, and the loader runs at realization off the paint path:
- `SvgService.Open(stream, parameters)` parses one document, `SKSvg.ToPicture(fragment, model, loader)` renders it through a shared `SkiaModel`
- `SkiaModel(SKSvgSettings)` and `SkiaSvgAssetLoader(model)` build once per target, `SKSvgSettings.Srgb` and `SrgbLinear` hold its `SKColorSpace`
- `SKSvg.CreateFromStream(stream, parameters)` parses and renders with its own model and loader into `Picture`, one instance per document
- `SKSvg.ReLoad(parameters)` rebuilds from the kept source when the static `SKSvg.CacheOriginalStream` is set, and throws otherwise
- `SKSvg.Draw(canvas, fragment, model, loader)` paints a document with no picture kept, a one-shot export takes it
- `SvgParameters(Entities, Css, CurrentColor, LoadOptions)` fills XML entities and injects a stylesheet before render
- `CurrentColor` resolves `currentColor` to a `System.Drawing.Color`
- Art holding its own colors takes its tint through `CurrentColor` at load
- Monochrome art tints as `SKShader.CreatePicture` under `CreateColorFilter(CreateColorMatrix)`, one shader per (asset, tint) drawn as a rect
- `SKColorFilter.CreateBlendMode(tint, SrcIn)` quantizes the tint to 8 bits, and a paint on `DrawPicture` opens a layer per draw
- `SKSvgSettings.TypefaceProviders` resolves fonts named in art, `StandaloneViewport` resolves percentage sizes of a root with no viewport
- Picture caches key on (asset, parameters), raster caches on (asset, parameters, scale) through `SKImage.FromPicture(picture, size)`
- `SKImage.FromPicture` tags sRGB alone, an F16 or wide-gamut raster of a picture draws it on an explicit surface
- Pictures that leave the scene stay cached until memory pressure, a hover that re-parses art thrashes
- `SvgService.GetDimensions(fragment, viewport)` reads intrinsic size and `viewBox`, the viewport resolves percentage sizes
- `HitTestElements(point, canvasMatrix)` and `HitTestTopmostElement(point, canvasMatrix)` hit test loaded art on an `SKSvg`
- Missing, unparsable, or empty art is a `Fault` with the asset as subject under `Reason.Missing`, `Unparsed`, or `Empty`, a throw at paint stops the frame
- Art loads while the scene is retained, the paint pass finds every picture present and parses none
- Host icons rasterize once per (asset, scale, theme) into an Eto `Icon(scale, bitmap)`, the previous icon is disposed on retheme, no PNG round trip

```csharp
internal sealed record ArtKey(Assembly Assembly, string Resource, SKColor Current);

internal sealed class Art {
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;
    private readonly AtomHashMap<ArtKey, Fin<SKPicture>> loaded = AtomHashMap<ArtKey, Fin<SKPicture>>();

    public Art(SKColorSpace display) {
        model = new SkiaModel(new SKSvgSettings { Srgb = display });
        loader = new SkiaSvgAssetLoader(model);
    }

    public Fin<SKPicture> Picture(ArtKey key) => loaded.FindOrAdd(key, () => Recorded(key));

    private Fin<SKPicture> Recorded(ArtKey key) {
        using Stream? stream = key.Assembly.GetManifestResourceStream(key.Resource);
        SvgParameters parameters = new(Entities: null, Css: null, Color.FromArgb(key.Current.Alpha, key.Current.Red, key.Current.Green, key.Current.Blue));
        return
            from source in Optional(stream).ToFin(new Fault(key.Resource, Reason.Missing))
            from document in Optional(SvgService.Open(source, parameters)).ToFin(new Fault(key.Resource, Reason.Unparsed))
            from picture in Optional(SKSvg.ToPicture(document, model, loader)).ToFin(new Fault(key.Resource, Reason.Empty))
            select picture;
    }
}
```

## [05]-[COLOR]

Unicolour states every color fact at design scale (swatch, palette, theme color, LUT bake) under one `Configuration`. Skia receives `SKColorF` with an `SKColorSpace`, a pixel pass bakes a matrix or curve once:
- `Configuration(rgbConfig, xyzConfig, ybrConfig, camConfig, dynamicRange, iccConfig)` fixes primaries, transfer, white, adaptation, viewing, range
- `Configuration.Default` is sRGB under D65 with `ChromaticAdaptation.Bradford`
- `ConvertToConfiguration` adapts the white point and re-encodes under the target primaries
- `RgbConfiguration.StandardRgb`, `DisplayP3`, `Rec2020`, `Rec2100Pq`, `Rec2100Hlg`, `Acescg` hold primaries, white, and transfer
- `RgbConfiguration.ToLinear` and `FromLinear` are `Func<double, DynamicRange, double>`
- `DynamicRange.Standard` and `High` set the luminance PQ and HLG encode against
- `Unicolour` constructs from a `ColourSpace` triplet, a hex string, `Chromaticity` with luminance, a CCT with `Locus`, a `Temperature`, or an `Spd`
- Instances allocate and evaluate representations on first read, one per swatch, style step, or table entry and none per pixel or frame
- `Blend(backdrop, BlendMode)` composites, `Palette(other, space, count)` returns a ramp
- `Mix(other, ColourSpace.Oklch, amount, HueSpan)` interpolates one step
- Interaction ramps are one `Palette` at style time indexed per frame, a `Mix` per frame allocates per hovered ink
- `Contrast(other)` is the WCAG ratio, a legibility floor lifts `Oklch.L` until the ratio holds and keeps hue and chroma
- Dark appearance derives from `Oklch.L` of the window fill below the window text, a host flag restates it
- `Difference(reference, DeltaE.Ok)` orders perceptual distance, `Ciede2000` and `Itp` serve print and HDR
- `MapToRgbGamut(GamutMap.OklchChromaReduction)` keeps hue and lightness, `RgbClipping` shifts both, `WxyPurityReduction` keeps dominant wavelength
- `IsInRgbGamut` tests before a bake
- `Temperature` on an instance answers `Cct` and `Duv` with `IsValid` within the locus distance
- `Simulate(Cvd, severity)` previews deficiency
- `Colourmaps.Viridis.Map(x)` and `Palette(count)` sample a perceptual map from `Wacton.Unicolour.Datasets`
- `Css.FromName(name)` answers a named color or `null`, `Css.All` lists the names
- Pixel passes derive a matrix from `RgbLinear` and `Xyz` of primaries and white, luma from `YbrConfiguration`, into `Vector4` lanes or a uniform
- Transfer tables derive from `ToLinear(value, dynamicRange)`
- Style colors are a typed union (fixed, host role, data series, mix, contrast, relative) declared in code beside the style that reads them
- Paint boundary: `SKColorF` from `Rgb` with the sRGB tag or from `RgbLinear` with the linear tag, `SKColor` from `Rgb.Byte255`
- `Unicolour.Hex` and `Rgb.Byte255.Hex` write `#RRGGBB` and answer `-` outside the RGB gamut, `MapToRgbGamut` precedes them
- HSL math stays in `ColourSpace.Hsl`
- RichTextKit `Style.TextColor` and `HaloColor` are 8-bit `SKColor`, a float color draws the block through a layer paint

```csharp
internal static class Swatches {
    public static SKColorF Srgb(Unicolour color) => new((float)color.Rgb.R, (float)color.Rgb.G, (float)color.Rgb.B, (float)color.Alpha.A);

    public static SKColorF Over(Unicolour color, Unicolour ground) => Srgb(color.Blend(ground, BlendMode.Normal).MapToRgbGamut(GamutMap.OklchChromaReduction));

    public static Seq<SKColorF> Ramp(Unicolour resting, Unicolour active, int steps) => toSeq(resting.Palette(active, ColourSpace.Oklab, steps)).Map(Srgb);
}
```

## [06]-[TEXT]

Text becomes pixels through one measured block per text, sealed style, and width. RichTextKit itemizes, shapes, breaks, lays out, and paints, HarfBuzz shapes a run that needs features or axes, `SKFont` rasterizes:
- Laid `TextBlock` instances cache per key and paint per frame, a block is device-free and survives a scale change
- Max-content is `MeasuredWidth` at `MaxWidth = null`, min-content the widest word between `WordBoundaryIndicies` carets, from one unbounded block
- Every `Layout` reshapes every run, the wrapped block replaces the unbounded one when the resolved width differs, none lays out twice at one width
- `Style.Seal()` fixes a style for sharing across blocks and as a cache key, `StyleManager.FromStyle` dedupes equal styles into one instance
- `TextPaintOptions` (`Edging`, `Hinting`, `SubpixelPositioning`, `Selection`) derives from surface properties and changes no layout
- Changed text color is a paint fact, `Copy` and `ApplyStyle` relayout the block
- Animated or contrast-adjusted text draws through a layer paint or color filter
- RichTextKit indexes are code points
- Word, caret, and line stops come from `WordBoundaryIndicies`, `CaretIndicies`, and `LineIndicies`
- `IStyle` holds no OpenType feature or axis, a featured run reshapes through HarfBuzz into `FontRun.Glyphs`, `GlyphPositions`, `Clusters`, `Width`
- Reshaped blocks paint themselves through `TextBlock.Paint`, a second painter over copied glyphs restates halo, underline, and selection
- Reshapes add the block's code points with the run as item (`AddUtf32(underlying, start, length)`), the run keeps its paragraph context
- Blocks hold one paragraph of one `Alignment`, the host culture decides `BaseDirection`
- `TextBlock.MeasuredOverhang` grows the invalidation rect, glyphs past the advance box repaint with it
- One `Font` per face and axis set, scaled at `UnitsPerEm`, serves every size through one multiply by `Size / UnitsPerEm`
- One reused `Buffer` per thread, cleared per run

```csharp
internal sealed record BlockKey(string Text, IStyle Face, Option<float> Width);

internal sealed class Blocks(SKSurfaceProperties raster) {
    private readonly TextPaintOptions options = new() {
        Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
        Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
    };
    private readonly StyleManager styles = new();
    private readonly AtomHashMap<BlockKey, TextBlock> laid = AtomHashMap<BlockKey, TextBlock>();

    public TextBlock Laid(string text, Style face, Option<float> width) {
        BlockKey key = new(text, styles.FromStyle(face), width);
        return laid.FindOrAdd(key, () => {
            TextBlock block = new() { MaxWidth = key.Width.ToNullable() };
            block.AddText(key.Text, key.Face);
            return block;
        });
    }

    public static float MinContent(TextBlock block) =>
        toSeq(block.WordBoundaryIndicies.Window(2)).Fold(0f, (widest, stops) => float.Max(widest, Caret(block, stops[1]) - Caret(block, stops[0])));

    public void Paint(SKCanvas canvas, TextBlock block, SKPoint origin) => block.Paint(canvas, origin, options);

    private static float Caret(TextBlock block, int index) => block.GetCaretInfo(new CaretPosition(index)).CaretXCoord;
}
```

## [07]-[HOST_SEAM]

Eto hosts the scene through one handler interface per capability, implemented in full per platform and resolved once at construction. Native casts sit inside one platform file:
- Platforms missing a member fail at launch, the compiler holds each platform to the interface
- Delegates registered per fact and resolved per call through `Find<T>()` are a service locator, absence on one platform shows as a refused event
- `Platform.Instance.IsMac`, `ControlObject`, `NativeHandle`, and `ToNative()` are read in the platform file alone, shared code reads the seam member
- Native behavior is a view subclass a handler installs from its platform file, a class swap on a toolkit-owned view breaks when the toolkit changes
- Pixel order derives from the host: `BitmapData.TranslateArgbToData` of opaque red answers red in the low byte for `SKColorType.Rgba8888`
- `Bgra8888` answers red in bits 16 to 23, alpha type derives from `BitmapData.PremultipliedAlpha`, a per-platform constant restates both
- Rasters cross Skia, Eto, and System.Drawing over locked pixels (`Bitmap.Lock`, `LockBits`) in one `Bgra8888` pair, a PNG round trip restates it
- Constructors build the control tree, one `Loaded` idiom over `LoadComplete` and `UnLoad` owns every host subscription, and a reload re-acquires
- `UITimer(handler)` and `Command(execute)` take their handler at construction, no subscription pair follows
- Device scale is the control's `ParentWindow.LogicalPixelSize`, a maximum over `Screen.Screens` mismatches every other monitor
- `LogicalPixelSizeChanged` on the window rebuilds device-sized caches and backings, `SizeChanged` alone misses a monitor move
- `Drawable.Height` written from a preferred size runs once per content change, a write per paint relayouts the parent per frame

`Pixels.Info` reads color type and alpha type from the locked `BitmapData`:

```csharp
internal sealed class SceneCanvas(Action<SKCanvas> draw) : Drawable {
    private Option<Bitmap> backing;

    protected override void OnPaint(PaintEventArgs e) {
        float scale = ParentWindow.LogicalPixelSize;
        Size pixels = Size.Ceiling(Size * scale);
        Bitmap target = backing.Filter(held => held.Size == pixels).IfNone(() => {
            _ = backing.Iter(static held => held.Dispose());
            Bitmap made = new(pixels, PixelFormat.Format32bppRgba);
            backing = made;
            return made;
        });
        using (BitmapData data = target.Lock()) {
            using SKSurface surface = SKSurface.Create(Pixels.Info(data, pixels), data.Data, data.ScanWidth);
            surface.Canvas.Scale(scale);
            draw(surface.Canvas);
        }
        e.Graphics.ImageInterpolation = ImageInterpolation.None;
        e.Graphics.DrawImage(target, new RectangleF(PointF.Empty, Size));
        base.OnPaint(e);
    }

    protected override void Dispose(bool disposing) {
        if (disposing) _ = backing.Iter(static held => held.Dispose());
        base.Dispose(disposing);
    }
}
```

## [08]-[ANTI_PATTERNS]

| [INDEX] | [WRONG_FORM]                                                        | [CORRECT_FORM]                                                   |
| :-----: | :------------------------------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | Drawn text field, tabs, expander, splitter, progress, or caption    | `TextBox`, `TabControl`, `Expander`, `Splitter`, `Label`         |
|  [02]   | Second scene raster host per surface or per host                    | One surface type owning allocation, damage, and paint            |
|  [03]   | Per-texel gamut or transfer loop in managed code before a blit      | Source and destination tagged, Skia converts on draw             |
|  [04]   | Target format selected by `ColorType` alone                         | `ColorType` and `ColorSpace` together select the F16 target      |
|  [05]   | `Snapshot()` between draws, or one snapshot per blit path           | One snapshot after the frame's last draw                         |
|  [06]   | `SKPaint`, `SKPath`, `SKFont`, or filter allocated in a draw loop   | Built per style or per layout, reused per draw                   |
|  [07]   | Shadow, clip, stroke, and hit test from separate geometry           | One outline feeding every derivation, cached per layout          |
|  [08]   | Inset shadow drawn as a drop shadow clipped `Difference`            | Inverted-alpha outline offset, blurred, and clipped `SrcIn`      |
|  [09]   | `SaveLayer()` around every group, or `DrawPicture` with a paint     | `SaveLayer(limit, paint)` for an effect, `null` picture paint    |
|  [10]   | `ClipPath(path, antialias: true)` per child on a plain rounded rect | `ClipRoundRect` once around the children                         |
|  [11]   | Backdrop blur recorded inside a picture or offscreen                | Backdrop filter on the live canvas alone                         |
|  [12]   | Cache key missing scale or color space, or keyed on a reference     | Key on content, scale, color type, and color space               |
|  [13]   | One cache cleared on every size or scale change                     | Device-free caches survive, device-dependent caches rebuild      |
|  [14]   | `Mix` or `Contrast` per hover frame, memo keyed on animated colors  | `Palette` table at style time, color through a layer paint       |
|  [15]   | `CreateShader` per frame, or a per-use builder disposing the effect | `BuildShader` once as a static, uniforms and children per use    |
|  [16]   | Gradient colors as `SKColor` on a linear target                     | `SKColorF[]` tagged with their `SKColorSpace`                    |
|  [17]   | `SrcIn` tint filter over art that holds its own colors              | `SvgParameters.CurrentColor` at load, tint for monochrome art    |
|  [18]   | SVG parsed on the paint thread, or rasterized per draw              | Picture at realization, image per (asset, parameters, scale)     |
|  [19]   | `new SKSvg()` per asset, `ReLoad` without `CacheOriginalStream`     | `SvgService.Open` into one `SkiaModel`, the toggle set first     |
|  [20]   | `new Unicolour` inside a pixel or sample loop                       | Matrix or curve derived once, baked into a span kernel or shader |
|  [21]   | Hand alpha composite, white point adaptation, or CCT fit            | `Blend`, `ConvertToConfiguration`, `Unicolour.Temperature`       |
|  [22]   | Hand RGB clipping of an out-of-gamut color                          | `MapToRgbGamut(GamutMap.OklchChromaReduction)`                   |
|  [23]   | Style file, decoder, or color string grammar                        | Typed color values declared in code beside their style           |
|  [24]   | `TextBlock` laid out per paint, or one block per candidate width    | Laid block cached by text, sealed style, and width               |
|  [25]   | `Copy` and `ApplyStyle` to recolor text per frame                   | Layer paint or color filter over the laid block                  |
|  [26]   | Whitespace scan or `Split(' ')` for wrap and word points            | `WordBoundaryIndicies`, `LineIndicies`                           |
|  [27]   | Glyph records copied out of `FontRun` into a second run model       | Reshaped glyphs written into `FontRun.Glyphs`, `GlyphPositions`  |
|  [28]   | `Blob`, `Face`, `Font`, or `Buffer` built per paint or weakly held  | One `Font` per face and axis set in a scene-owned cache          |
|  [29]   | `SKShaper.Shape` followed by a second shape for features            | One `Font.Shape(buffer, features)`                               |
|  [30]   | `Platform.Add<TDelegate>` per fact, `Find<T>()` at each call site   | One `IHandler` per capability, `CreateShared<T>()` once          |
|  [31]   | Pixel order as a registered per-platform constant                   | `TranslateArgbToData` of opaque red on the locked `BitmapData`   |
|  [32]   | `IsMac` branch or `ControlObject` cast in shared code               | Seam member the platform file implements                         |
|  [33]   | Runtime class swap on a toolkit-owned native view                   | Native view subclass in the platform file                        |
|  [34]   | `BitmapData` held for the bitmap's life, drawn while locked         | Lock, draw through Skia, dispose, then `Graphics.DrawImage`      |
|  [35]   | Logical size as the backing size, scale from the largest screen     | Logical size times the window's `LogicalPixelSize`               |
|  [36]   | `Thread` or `Task` body touching a control, `Invoke` from a worker  | `AsyncInvoke` or `InvokeAsync` onto the UI thread                |
|  [37]   | `UITimer` as an animation frame clock                               | Display link behind the seam, `UITimer` for cadence              |
|  [38]   | `LoadComplete` subscribed per control, realization in a constructor | One `Loaded` idiom subscribing and releasing per control         |
|  [39]   | One `Command` per menu item, button, and context item               | One `Command` converted to `MenuItem` and `ToolItem`             |
|  [40]   | Shortcut dispatched by hand in `KeyDown`                            | `Command.Shortcut`, `MapPlatformCommand` for edit commands       |
|  [41]   | Icon re-rasterized per cell bind, or `Icon.FromHandle(GetHicon())`  | `new Icon(scale, bitmap)` per (asset, scale, theme), disposed    |
|  [42]   | `Graphics` plots, captions, or glyphs beside a Skia scene           | One Skia scene in the `Drawable`, `Graphics` blits alone         |
