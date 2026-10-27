using System.Collections;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface ISlot : IDisposable {
    public void Sweep();
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Context(SKMatrix Device) {
    internal HashMap<Interaction, float> StateLayers { get; init; }
    internal Option<PlotFrame> Plot { get; init; }

    internal Context CacheKey => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    internal float Scale => Device.MapRadius(1f);

    internal Context Translated(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    internal Option<SKColorF> Color(ColorValue value, LayoutNode layout, HashMap<Interaction, ColorValue> targets = default, Option<DataSeries> series = default) =>
        layout.Cascade.Theme.WithStateLayers(value, layout.Style, StateLayers, targets, series);

    internal Pen Pen(RoleStyle style) => new(style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]) { Device = Device, Icon = style.Metrics[Metric.Icon] };

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

internal sealed record FillKey(LayoutNode Layout, int Index, HashMap<Interaction, float> StateLayers, SKRect Frame, float Scale, float Clock, Option<SKPoint> Pointer);

internal sealed record MarkKey(Figure Figure, PlotFrame Frame, Pen Pen);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Slot<TKey, TValue> : ISlot where TKey : notnull {
    private readonly AtomHashMap<TKey, Entry> held = AtomHashMap<TKey, Entry>();
    private int generation;

    internal Seq<TValue> Values => held.Values.ToSeq().Map(static entry => entry.Value);

    internal Option<TValue> Find(TKey key) => held.Find(key).Map(entry => entry.Touched(generation));

    internal TValue GetOrAdd(TKey key, Func<TValue> make) => Find(key).IfNone(() => Added(key, make()));

    internal Fin<TValue> GetOrAdd(TKey key, Func<Fin<TValue>> make) => Find(key).Match(Some: Fin.Succ, None: () => make().Map(made => Added(key, made)));

    public void Sweep() {
        _ = held.Filter(entry => !entry.Live(generation)).Iter((key, entry) => {
            _ = held.Remove(key);
            _ = Release(entry.Value);
        });
        generation++;
    }

    public void Dispose() {
        _ = held.Values.Iter(static entry => Release(entry.Value));
        _ = held.Clear();
    }

    private TValue Added(TKey key, TValue made) {
        _ = held.AddOrUpdate(key, new Entry(made, generation));
        return made;
    }

    private static Unit Release(object? value) {
        switch (value) {
            case IDisposable owned:
                owned.Dispose();
                return unit;
            case IOptional optional:
                return optional.MatchUntyped(Release, static () => unit);
            case IEnumerable items:
                return toSeq(items.Cast<object?>()).Fold(unit, static (_, item) => Release(item));
            default:
                return unit;
        }
    }

    private sealed class Entry(TValue value, int generation) {
        private int touched = generation;

        internal TValue Value { get; } = value;

        internal TValue Touched(int current) {
            touched = current;
            return Value;
        }

        internal bool Live(int current) => touched == current;
    }
}

internal sealed class PaintCache : IDisposable {
    // --- [STATE]
    private sealed record Retained(Scene Scene, HashSet<LayoutNode> Leaving, float Scale, HashSet<Seq<Key>> Moving, HashMap<(LayoutNode Layout, GlyphSource Source), LoadedMedia> Art);

    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();
    private static readonly SKImageFilter Source = SKImageFilter.CreateOffset(0f, 0f);
    private readonly SKImageInfo target;
    private readonly Func<SKImageInfo, Option<SKSurface>> offscreen;
    private readonly Media media;
    private readonly Seq<ISlot> scened;
    private readonly Seq<ISlot> framed;
    private Option<Retained> retained;

    internal PaintCache(SKImageInfo target, SKSurfaceProperties raster, Shaping shaping, Func<SKImageInfo, Option<SKSurface>> offscreen) {
        (this.target, this.offscreen) = (target, offscreen);
        media = new Media(target, shaping, offscreen);
        Space = target.ColorSpace ?? Srgb;
        Precision = target.BytesPerPixel < 8 ? SKCanvasSaveLayerRecFlags.F16ColorType : SKCanvasSaveLayerRecFlags.None;
        Raster = new TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
        scened = [Recordings, Leaves, TextOutlines, Interiors, Contours, Sprites, Meshes, Labels, Mattes, Arts, Decoded];
        framed = [Filters, Brushes, Backdrops, Images, Dots, Atlases, Fields, Shadows, Casts, Strokes, Fades, Opacities, Tints, Solids];
    }

    internal TextPaintOptions Raster { get; }
    internal SKColorSpace Space { get; }
    internal SKCanvasSaveLayerRecFlags Precision { get; }
    internal Seq<Seq<Key>> Animated { get; private set; }
    internal HashSet<Seq<Key>> Moving => retained.Map(static held => held.Moving).IfNone([]);

    // --- [SLOTS]
    internal Slot<(LayoutNode Layout, Context Context, float Alpha, HashSet<Seq<Key>> Live), SKDrawable> Recordings { get; } = new();
    internal Slot<LayoutNode, Painter.Leaf> Leaves { get; } = new();
    internal Slot<FillKey, Option<SKImageFilter>> Filters { get; } = new();
    internal Slot<FillKey, Option<SKPaint>> Brushes { get; } = new();
    internal Slot<FillKey, Option<SKImageFilter>> Backdrops { get; } = new();
    internal Slot<(LayoutNode Layout, Option<SKColorF> Color, bool AlphaOnly), SKPaint> Images { get; } = new();
    internal Slot<Typeset, SKPath> TextOutlines { get; } = new();
    internal Slot<MarkKey, Outline.Traced> Interiors { get; } = new();
    internal Slot<MarkKey, Outline.Traced> Contours { get; } = new();
    internal Slot<(MarkKey Mark, Seq<SKPaint> Paints), SKPicture> Dots { get; } = new();
    internal Slot<(LayoutNode Layout, Option<Marker> Marker, float Diameter, float Scale), Option<SKImage>> Sprites { get; } = new();
    internal Slot<(MarkKey Mark, Option<Marker> Marker, SKColorF Color), SKPicture> Atlases { get; } = new();
    internal Slot<(MarkKey Mark, Option<SKColorF> Lead), Option<SKPaint>> Fields { get; } = new();
    internal Slot<MarkKey, Seq<SKVertices>> Meshes { get; } = new();
    internal Slot<(Typeset Set, MarkKey Mark), Option<SKTextBlob>> Labels { get; } = new();
    internal Slot<(SKSize Size, Corners Corners, float Smoothing, float Scale), Option<SKImage>> Mattes { get; } = new();
    private Slot<(Shadow Shadow, SKColorF Color), SKPaint> Shadows { get; } = new();
    private Slot<Seq<(Shadow Shadow, SKColorF Color)>, SKPaint> Casts { get; } = new();
    private Slot<(Stroke Stroke, SKColorF Color, float Width, uint Ordinal, float Clock, Option<SKBlender> Blend), SKPaint> Strokes { get; } = new();
    private Slot<(SKPoint From, SKPoint To, bool Before, bool After, float Edge), SKPaint> Fades { get; } = new();
    private Slot<(float Alpha, Option<SKImageFilter> Filter), SKPaint> Opacities { get; } = new();
    private Slot<(Option<SKPicture> Still, SKColorF Tint), SKPaint> Tints { get; } = new();
    private Slot<SKColorF, SKPaint> Solids { get; } = new();
    private Slot<ArtKey, LoadedMedia> Arts { get; } = new();
    private Slot<(ImageSource.Encoded Source, SKSizeI Pixels), Decoded> Decoded { get; } = new();

    // --- [LAYERS]
    internal SKPaint Opacity(float alpha, Option<SKImageFilter> filter) =>
        Opacities.GetOrAdd((alpha, filter), () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha), ImageFilter = filter.ValueUnsafe() });

    internal SKPaint Cast(Seq<(Shadow Shadow, SKColorF Color)> stack) =>
        Casts.GetOrAdd(stack, () => {
            Seq<SKImageFilter> graph = stack.Filter(static held => !held.Shadow.Inside).Map(held => held.Shadow.Filtered(held.Color, Space)).Strict().Add(Source)
                + stack.Filter(static held => held.Shadow.Inside).Map(held => held.Shadow.Filtered(held.Color, Space)).Strict();
            using SKImageFilter merged = SKImageFilter.CreateMerge([.. graph]);
            _ = graph.Filter(static filter => !ReferenceEquals(filter, Source)).Iter(static filter => filter.Dispose());
            return new SKPaint { ImageFilter = merged };
        });

    internal SKPaint Shadowed((Shadow Shadow, SKColorF Color) held) =>
        Shadows.GetOrAdd(held, () => {
            using SKMaskFilter? blur = held.Shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, held.Shadow.Blur) : null;
            return new SKPaint { IsAntialias = true, ColorF = held.Color, MaskFilter = blur };
        });

    internal SKCanvasSaveLayerRecFlags Layer(bool previous) => previous ? (SKCanvasSaveLayerRecFlags)((int)Precision | (int)SKCanvasSaveLayerRecFlags.InitializeWithPrevious) : Precision;

    // --- [PAINTS]
    internal SKPaint Stroke(Stroke stroke, SKColorF color, float width, uint ordinal, float clock, Option<SKBlender> blend) =>
        Strokes.GetOrAdd((stroke, color, width, ordinal, stroke.Timed ? clock : 0f, blend), () => {
            using SKPathEffect? effect = Styling.Stroke.Effect(stroke.Patterned(ordinal), width, clock).ValueUnsafe();
            return new SKPaint {
                IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, StrokeCap = stroke.Cap, StrokeJoin = stroke.Join, PathEffect = effect, Blender = blend.ValueUnsafe(),
            };
        });

    internal SKPaint FadeMask((SKPoint From, SKPoint To, bool Before, bool After, float Edge) fade) =>
        Fades.GetOrAdd(fade, () => {
            using SKShader shader = SKShader.CreateLinearGradient(fade.From, fade.To, [fade.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, fade.After ? SKColors.Transparent : SKColors.Black],
                [0f, fade.Edge, 1f - fade.Edge, 1f], SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn };
        });

    internal SKPaint Tint(LoadedMedia art, SKColorF tint) =>
        Tints.GetOrAdd((art.Still, tint), () => {
            using SKColorFilter washed = Space.ColorMatrix([0f, 0f, 0f, 0f, tint.Red, 0f, 0f, 0f, 0f, tint.Green, 0f, 0f, 0f, 0f, tint.Blue, 0f, 0f, 0f, tint.Alpha, 0f]);
            using SKShader? shader = art.Still.Map(static picture => picture.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKFilterMode.Linear, SKMatrix.Identity, picture.CullRect)).ValueUnsafe();
            return new SKPaint { IsAntialias = true, ColorFilter = washed, Shader = shader };
        });

    internal SKPaint Solid(SKColorF color) => Solids.GetOrAdd(color, () => new SKPaint { IsAntialias = true, ColorF = color });

    // --- [OFFSCREENS]
    internal Option<SKImage> Rasterized(SKSize size, float scale, SKColorType type, Action<SKCanvas> draw) {
        SKSizeI pixels = SKRectI.Ceiling(SKRect.Create(size.Width * scale, size.Height * scale), outwards: true).Size;
        return offscreen(new SKImageInfo(int.Max(1, pixels.Width), int.Max(1, pixels.Height), type, SKAlphaType.Premul, Space)).Map(surface => {
            using SKSurface held = surface;
            held.Canvas.Scale(scale);
            draw(held.Canvas);
            return held.Snapshot();
        });
    }

    internal Option<SKShader> Rastered(SKShader drawn, SKRect frame, float scale) {
        using SKShader owned = drawn;
        using SKPaint paint = new() { Shader = owned };
        return Rasterized(frame.Size, scale, target.ColorType, canvas => canvas.DrawRect(SKRect.Create(frame.Size), paint)).Map(image => {
            using SKImage held = image;
            return held.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear), SKMatrix.CreateScaleTranslation(1f / scale, 1f / scale, frame.Left, frame.Top));
        });
    }

    // --- [RETENTION]
    internal Fin<Unit> Retain(Mount mount, float scale) {
        Scene scene = mount.Scene;
        Seq<LayoutNode> leaving = mount.Animations.Leaving.Map(static left => left.Last);
        HashSet<LayoutNode> left = toHashSet(leaving);
        if (retained.Exists(held => ReferenceEquals(held.Scene, scene) && held.Leaving == left && held.Scale.CompareTo(scale) is 0)) return unit;
        Seq<LayoutNode> drawn = (Seq(scene.Layout) + scene.Layers + scene.Fragments.Values.ToSeq() + leaving).Bind(Scene.Preorder).Strict();
        _ = drawn.Iter(layout => Leaves.Find(layout));
        _ = scened.Add(scene.Outlines).Iter(static slot => slot.Sweep());
        Seq<((LayoutNode Layout, GlyphSource Source) Shown, Fin<LoadedMedia> Media)> arts = (
            from layout in drawn
            from source in Sources(layout)
            let key = Media.Keyed(layout, source)
            select (Shown: (layout, source), Media: Arts.GetOrAdd(key, () => media.Load(key)))).Strict();
        Seq<(Seq<Key> Path, Fin<Decoded> Frames)> images = drawn.Choose(layout => layout.Node is Node.Image { Source: ImageSource.Encoded encoded } image
            ? Some(Decoding(layout, image, encoded, scale) switch { var key => (layout.Path, Decoded.GetOrAdd(key, () => media.Decode(key))) })
            : None).Strict();
        Seq<Fin<Outline>> outlines = drawn.Filter(static layout => layout.Style.Shape is not Shape.Rounded).Map(layout => Outline.Of(scene.Outlines, layout, SKRect.Create(layout.Frame.Size), 0f)).Strict();
        return (arts.Traverse(static held => held.Media).As(), images.Traverse(static held => held.Frames).As(), outlines.Traverse(static held => held).As()).Apply((loaded, _, _) => {
            HashMap<(LayoutNode Layout, GlyphSource Source), LoadedMedia> art = toHashMap(arts.Map(static held => held.Shown).Zip(loaded));
            Animated = toSeq((toSeq(art.Filter(static shown => shown.Animated).Keys).Map(static shown => shown.Layout.Path) + images.Filter(static held => held.Frames.Exists(static frames => frames.Animated)).Map(static held => held.Path)).Distinct()).Strict();
            retained = new Retained(scene, left, scale, toHashSet(drawn.Filter(static layout => layout.Timed || layout.Style.Pointing).Map(static layout => layout.Path) + Animated), art);
            return unit;
        }).As();
    }

    internal void Framed(Painter.Pass pass) {
        _ = framed.Iter(static slot => slot.Sweep());
        _ = retained.Iter(held => toSeq(held.Art.Values).Filter(static shown => shown.Animated).Distinct().Iter(shown => shown.Seek(pass.Now)));
        _ = Leaves.Values.Iter(leaf => leaf.Framed(pass));
    }

    internal Option<LoadedMedia> Art(LayoutNode layout, GlyphSource source) => retained.Bind(held => held.Art.Find((layout, source)));

    internal Option<SKImage> Frame(LayoutNode layout, Node.Image image, Duration now) =>
        image.Source.Switch(
            (Cache: this, Layout: layout, Image: image, Now: now),
            still: static (_, still) => Some(still.Image),
            encoded: static (held, encoded) =>
                from kept in held.Cache.retained
                from frames in held.Cache.Decoded.Find(Decoding(held.Layout, held.Image, encoded, kept.Scale))
                select frames.At(held.Now));

    public void Dispose() => _ = (scened + framed).Iter(static slot => slot.Dispose());

    private static Seq<GlyphSource> Sources(LayoutNode layout) =>
        (layout.Node is Node.Glyph glyph ? Seq(glyph.Art) : [])
        + layout.Style.Fills.Bind(static fill => fill.Arts)
        + layout.Style.Filters.Bind(static filter => filter.Arts)
        + layout.Node.Traced.ToSeq().Bind(static figure => figure.Art).Map(static marker => marker.Art);

    private static (ImageSource.Encoded Source, SKSizeI Pixels) Decoding(LayoutNode layout, Node.Image image, ImageSource.Encoded encoded, float scale) {
        SKRect fitted = image.Fit.Fitted(layout.ContentBox, new SKSize(encoded.Info.Width, encoded.Info.Height));
        return (encoded, SKRectI.Ceiling(SKRect.Create(fitted.Width * scale, fitted.Height * scale), outwards: true).Size);
    }
}
