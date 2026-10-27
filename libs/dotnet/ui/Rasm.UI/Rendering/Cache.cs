using System.Collections;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface ISlot {
    public void Sweep();
    public void Clear();
}

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Context(SKMatrix Device, Seq<StateLayer> StateLayers) {
    internal Option<PlotFrame> Plot { get; init; }

    internal Context CacheKey => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    internal float Scale => Device.MapRadius(1f);

    internal Context Translated(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    internal Pen Pen(RoleStyle style) => new(style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]) { Device = Device, Icon = style.Metrics[Metric.Icon] };

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

internal sealed record Fade(SKPoint From, SKPoint To, bool Before, bool After, float Edge);

internal sealed record ResolvedShadow(Shadow Shadow, SKColorF Color);

internal sealed record Recording(LayoutNode Layout, Context Context, float Alpha, HashSet<Seq<Key>> Live);

internal sealed record VeilKey(LayoutNode Layout, Role Role, float Alpha, Option<SKImageFilter> Filter);

internal sealed record OutlineKey(Shape Shape, Corners Corners, float Smoothing, SKRect Frame, float Offset);

internal sealed record FillKey(LayoutNode Layout, int Index, Seq<StateLayer> StateLayers, SKRect Frame, float Scale, float Clock, Option<SKPoint> Pointer);

internal sealed record StrokeKey(SKColorF Color, float Width, Seq<PathEffect> Patterns, float Clock, Option<SKBlender> Blend);

internal sealed record MatteKey(SKSize Size, Corners Corners, float Smoothing, float Scale);

internal sealed record TextKey(Typeset Set, SKColor Tint);

internal sealed record TextOutlineKey(Typeset Set, SKPoint Origin);

internal sealed record ImagePaintKey(SKBlender Blend, Option<SKColorF> Color, Option<ColorRamp> Scale, bool AlphaOnly);

internal sealed record MarkKey(Figure Figure, PlotFrame Frame, Pen Pen);

internal sealed record ShownMedia(Seq<Key> Path, LoadedMedia Media);

internal sealed record RetainedScene(Scene Scene, HashSet<Seq<Key>> Leaving, float Scale, HashSet<Seq<Key>> Moving, Seq<ShownMedia> Motion);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Slot<TKey, TValue> : ISlot where TKey : notnull {
    private readonly AtomHashMap<TKey, Entry> held = AtomHashMap<TKey, Entry>();
    private int generation;

    internal Seq<TValue> Values => held.Values.ToSeq().Map(static entry => entry.Value);

    internal Option<TValue> Find(TKey key) => held.Find(key).Map(entry => entry.Touched(generation));

    internal TValue GetOrAdd(TKey key, Func<TValue> make) => Find(key).IfNone(() => Added(key, make()));

    internal Fin<TValue> GetOrAdd(TKey key, Func<Fin<TValue>> make) => Find(key).Match(Some: Fin.Succ, None: () => make().Map(made => Added(key, made)));

    void ISlot.Sweep() {
        _ = held.Filter(entry => !entry.Live(generation)).Iter((key, entry) => {
            _ = held.Remove(key);
            _ = Release(entry.Value);
        });
        generation++;
    }

    void ISlot.Clear() {
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
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);
    private static readonly SKImageFilter Source = SKImageFilter.CreateOffset(0f, 0f);
    private readonly SKImageInfo target;
    private readonly Media media;
    private readonly Seq<ISlot> slots;
    private Option<RetainedScene> retained;

    internal PaintCache(SKImageInfo target, SKSurfaceProperties raster, Shaping shaping, Func<SKImageInfo, Option<SKSurface>> offscreen) {
        (this.target, Offscreen) = (target, offscreen);
        media = new Media(target, shaping, offscreen);
        Space = target.ColorSpace ?? Srgb;
        Precision = target.BytesPerPixel < 8 ? SKCanvasSaveLayerRecFlags.F16ColorType : SKCanvasSaveLayerRecFlags.None;
        Raster = new TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
        slots = [Recordings, Leaves, Filters, Shadows, Casts, Outlines, Brushes, Backdrops, Mattes, Strokes, Fades, Veils, Images, Tints, Solids, Texts, TextOutlines, Interiors, Contours, Dots, Sprites,
            Atlases, Fields, Meshes, Labels, Arts, Decoded];
    }

    internal Func<SKImageInfo, Option<SKSurface>> Offscreen { get; }
    internal TextPaintOptions Raster { get; }
    internal SKColorSpace Space { get; }
    internal SKCanvasSaveLayerRecFlags Precision { get; }
    internal Seq<Seq<Key>> Animated { get; private set; }
    internal HashSet<Seq<Key>> Moving => retained.Map(static held => held.Moving).IfNone([]);

    // --- [SLOTS]
    internal Slot<Recording, SKDrawable> Recordings { get; } = new();
    internal Slot<LayoutNode, Painter.Leaf> Leaves { get; } = new();
    internal Slot<(LayoutNode Layout, SKRect Frame), Option<SKImageFilter>> Filters { get; } = new();
    internal Slot<ResolvedShadow, SKPaint> Shadows { get; } = new();
    internal Slot<Seq<ResolvedShadow>, SKPaint> Casts { get; } = new();
    internal Slot<OutlineKey, Outline> Outlines { get; } = new();
    internal Slot<FillKey, Option<SKPaint>> Brushes { get; } = new();
    internal Slot<FillKey, Option<SKImageFilter>> Backdrops { get; } = new();
    internal Slot<MatteKey, Option<SKImage>> Mattes { get; } = new();
    internal Slot<StrokeKey, SKPaint> Strokes { get; } = new();
    internal Slot<Fade, SKPaint> Fades { get; } = new();
    internal Slot<VeilKey, SKPaint> Veils { get; } = new();
    internal Slot<ImagePaintKey, SKPaint> Images { get; } = new();
    internal Slot<SKColorF, SKPaint> Tints { get; } = new();
    internal Slot<SKColorF, SKPaint> Solids { get; } = new();
    internal Slot<TextKey, TextBlock> Texts { get; } = new();
    internal Slot<TextOutlineKey, SKPath> TextOutlines { get; } = new();
    internal Slot<MarkKey, Outline> Interiors { get; } = new();
    internal Slot<MarkKey, Outline> Contours { get; } = new();
    internal Slot<Marks.DotsKey, SKPicture> Dots { get; } = new();
    internal Slot<Marks.SpriteKey, Option<SKImage>> Sprites { get; } = new();
    internal Slot<Marks.AtlasKey, SKPicture> Atlases { get; } = new();
    internal Slot<Marks.FieldKey, Option<SKPaint>> Fields { get; } = new();
    internal Slot<MarkKey, Seq<SKVertices>> Meshes { get; } = new();
    internal Slot<Marks.LabelKey, Option<SKTextBlob>> Labels { get; } = new();
    internal Slot<ArtKey, LoadedMedia> Arts { get; } = new();
    internal Slot<DecodeKey, Decoded> Decoded { get; } = new();

    // --- [LAYERS]
    internal SKPaint Veil(LayoutNode layout, Role role, float alpha, Option<SKImageFilter> filter) =>
        Veils.GetOrAdd(new VeilKey(layout, role, alpha, filter), () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha), ImageFilter = filter.ValueUnsafe() });

    internal SKPaint Cast(Seq<ResolvedShadow> stack) =>
        Casts.GetOrAdd(stack, () => {
            Seq<SKImageFilter> graph = stack.Filter(static held => !held.Shadow.Inside).Map(held => held.Shadow.Filtered(held.Color, Space)).Strict().Add(Source)
                + stack.Filter(static held => held.Shadow.Inside).Map(held => held.Shadow.Filtered(held.Color, Space)).Strict();
            using SKImageFilter merged = SKImageFilter.CreateMerge([.. graph]);
            _ = graph.Filter(static filter => !ReferenceEquals(filter, Source)).Iter(static filter => filter.Dispose());
            return new SKPaint { ImageFilter = merged };
        });

    internal SKPaint Shadowed(ResolvedShadow held) =>
        Shadows.GetOrAdd(held, () => {
            using SKMaskFilter? blur = held.Shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, held.Shadow.Blur) : null;
            return new SKPaint { IsAntialias = true, ColorF = held.Color, MaskFilter = blur };
        });

    // --- [OUTLINES]
    internal Option<Outline> Outlined(LayoutNode layout, SKRect frame, float offset) => Lowered(layout, frame, offset).ToOption();

    private Fin<Outline> Lowered(LayoutNode layout, SKRect frame, float offset) =>
        Outlines.GetOrAdd(new OutlineKey(layout.Style.Shape, layout.Corners, layout.Smoothing, frame, offset), () => Outline.Of(layout.Style.Shape, layout.Corners, layout.Smoothing, frame, offset));

    // --- [PAINTS]
    internal SKPaint Stroke(StrokeKey key) =>
        Strokes.GetOrAdd(key, () => {
            using SKPathEffect? effect = Styling.Stroke.Effect(key.Patterns, key.Width, key.Clock).ValueUnsafe();
            return new SKPaint { IsAntialias = true, ColorF = key.Color, Style = SKPaintStyle.Stroke, StrokeWidth = key.Width, StrokeCap = SKStrokeCap.Round, PathEffect = effect, Blender = key.Blend.ValueUnsafe() };
        });

    internal SKPaint FadeMask(Fade fade) =>
        Fades.GetOrAdd(fade, () => {
            using SKShader shader = SKShader.CreateLinearGradient(fade.From, fade.To, [fade.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, fade.After ? SKColors.Transparent : SKColors.Black],
                [0f, fade.Edge, 1f - fade.Edge, 1f], SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn };
        });

    internal SKPaint Tint(SKColorF tint) =>
        Tints.GetOrAdd(tint, () => {
            using SKColorFilter tintFilter = TintMatrix(tint);
            return new SKPaint { IsAntialias = true, ColorFilter = tintFilter };
        });

    internal SKPaint Solid(SKColorF color) => Solids.GetOrAdd(color, () => new SKPaint { IsAntialias = true, ColorF = color });

    internal SKColorFilter TintMatrix(SKColorF tint) => Space.ColorMatrix([0f, 0f, 0f, 0f, tint.Red, 0f, 0f, 0f, 0f, tint.Green, 0f, 0f, 0f, 0f, tint.Blue, 0f, 0f, 0f, tint.Alpha, 0f]);

    // --- [OFFSCREENS]
    internal Option<SKShader> Rastered(SKShader drawn, SKRect frame, float scale) {
        using SKShader owned = drawn;
        SKRect local = SKRect.Create(frame.Size);
        SKMatrix grid = SKMatrix.CreateScale(scale, scale);
        SKRectI pixels = SKRectI.Ceiling(grid.MapRect(local), outwards: true);
        return Offscreen(target.WithSize(pixels.Width, pixels.Height)).Map(surface => {
            using SKSurface held = surface;
            using SKPaint paint = new() { Shader = owned };
            held.Canvas.SetMatrix(grid.PostConcat(SKMatrix.CreateTranslation(-pixels.Left, -pixels.Top)));
            held.Canvas.DrawRect(local, paint);
            using SKImage image = held.Snapshot();
            return image.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, Sampling,
                SKMatrix.CreateTranslation(frame.Left, frame.Top).PreConcat(grid.Invert()).PreConcat(SKMatrix.CreateTranslation(pixels.Left, pixels.Top)));
        });
    }

    internal Option<SKImage> Matte(MatteKey key) =>
        Mattes.GetOrAdd(key, () => {
            SKSizeI pixels = SKRectI.Ceiling(SKRect.Create(key.Size.Width * key.Scale, key.Size.Height * key.Scale), outwards: true).Size;
            return Offscreen(new SKImageInfo(pixels.Width, pixels.Height, SKColorType.RgbaF16, SKAlphaType.Premul, Space)).Map(surface => {
                using SKSurface held = surface;
                using SKShader shader = Refraction.Shader(key.Size, key.Corners, key.Smoothing);
                using SKPaint paint = new() { Shader = shader };
                held.Canvas.Scale(key.Scale);
                held.Canvas.DrawRect(SKRect.Create(key.Size), paint);
                return held.Snapshot();
            });
        });

    internal SKCanvasSaveLayerRecFlags Layer(bool previous) => previous ? (SKCanvasSaveLayerRecFlags)((int)Precision | (int)SKCanvasSaveLayerRecFlags.InitializeWithPrevious) : Precision;

    // --- [RETENTION]
    internal Fin<Unit> Retain(Mount mount, float scale) {
        Scene scene = mount.Scene;
        HashSet<Seq<Key>> leaving = toHashSet(mount.Animations.Exiting.Keys).Union(mount.Animations.Departing.Keys);
        if (retained.Exists(held => ReferenceEquals(held.Scene, scene) && held.Leaving == leaving && held.Scale.CompareTo(scale) is 0)) return unit;
        _ = slots.Iter(static slot => slot.Sweep());
        Seq<LayoutNode> current = (Seq(scene.Layout) + scene.Layers + scene.Fragments.Values.ToSeq()).Bind(Scene.Preorder).Strict();
        Seq<LayoutNode> departed = (toSeq(mount.Animations.Exiting.Values) + toSeq(mount.Animations.Departing.Values)).Map(static left => left.Last).Bind(Scene.Preorder).Strict();
        Seq<LayoutNode> drawn = current + departed;
        Seq<(Seq<Key> Path, Fin<LoadedMedia> Media)> arts = drawn.Bind(layout => Loaded(layout, scene.ColorCache)).Strict();
        Seq<(Seq<Key> Path, Fin<Decoded> Frames)> images = drawn.Choose(layout => Decode(layout, scale)).Strict();
        Seq<Fin<Outline>> outlines = drawn.Filter(static layout => layout.Style.Shape is not Shape.Rounded).Map(layout => Lowered(layout, SKRect.Create(layout.Frame.Size), 0f)).Strict();
        return (arts.Traverse(static held => held.Media).As(), images.Traverse(static held => held.Frames).As(), outlines.Traverse(static held => held).As()).Apply((_, _, _) => {
            Seq<ShownMedia> motion = arts.Choose(static held => held.Media.ToOption().Filter(static media => media.Animated).Map(media => new ShownMedia(held.Path, media))).Strict();
            Animated = toSeq(motion.Map(static held => held.Path) + images.Filter(static held => held.Frames.Exists(static frames => frames.Animated)).Map(static held => held.Path)).Distinct().Strict();
            retained = new RetainedScene(scene, leaving, scale, toHashSet(drawn.Filter(static layout => layout.Timed || layout.Style.Pointing).Map(static layout => layout.Path) + Animated), motion);
            return unit;
        }).As();
    }

    internal void Framed(Painter.Pass pass) {
        Seq<ShownMedia> motion = retained.Map(static held => held.Motion).IfNone([]);
        Seq<LoadedMedia> moved = motion.Map(static held => held.Media).Distinct().Filter(media => media.Seek(pass.Now)).Strict();
        HashSet<Seq<Key>> still = toHashSet(motion.Filter(held => !moved.Exists(media => ReferenceEquals(media, held.Media))).Map(static held => held.Path));
        _ = Leaves.Values.Iter(leaf => leaf.Framed(pass, !still.Contains(leaf.Layout.Path)));
    }

    public void Dispose() => _ = slots.Iter(static slot => slot.Clear());

    private Seq<(Seq<Key> Path, Fin<LoadedMedia> Media)> Loaded(LayoutNode layout, ColorCache colorCache) {
        SKColorF under = Styles.Background(layout.Cascade, layout.Style, layout.Node);
        Seq<GlyphSource> sources = layout.Node.GlyphArt.ToSeq()
            + layout.Style.Fills.Bind(static fill => fill.Arts)
            + layout.Style.Filters.Bind(static filter => filter.Arts)
            + layout.Node.Traced.ToSeq().Bind(static figure => figure.Art).Map(static marker => marker.Art);
        return sources.Map(source => Media.Keyed(layout, source, under, colorCache)).Map(key => (layout.Path, Arts.GetOrAdd(key, () => media.Load(key))));
    }

    private Option<(Seq<Key> Path, Fin<Decoded> Frames)> Decode(LayoutNode layout, float scale) =>
        layout.Node is Node.Image { Source: ImageSource.Encoded encoded } image
            ? new DecodeKey(encoded, Pixels(image, encoded, layout.ContentBox, scale)) switch {
                var key => Some((layout.Path, Decoded.GetOrAdd(key, () => media.Decode(key)))),
            }
            : None;

    internal Option<SKImage> Frame(LayoutNode layout, Node.Image image, Duration now, float scale) =>
        image.Source.Switch(
            (Cache: this, Layout: layout, Image: image, Now: now, Scale: scale),
            still: static (_, still) => Some(still.Image),
            encoded: static (held, encoded) => held.Cache.Decoded.Find(new DecodeKey(encoded, Pixels(held.Image, encoded, held.Layout.ContentBox, held.Scale))).Map(frames => frames.At(held.Now)));

    private static SKSizeI Pixels(Node.Image image, ImageSource source, SKRect content, float scale) {
        SKRect fitted = image.Fit.Fitted(content, new SKSize(source.Info.Width, source.Info.Height));
        return SKRectI.Ceiling(SKRect.Create(fitted.Width * scale, fitted.Height * scale), outwards: true).Size;
    }
}
