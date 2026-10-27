using LanguageExt.ClassInstances;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [TYPES] ---------------------------------------------------------------------------
internal interface ISlot {
    public void Retain(Func<Seq<Key>, Option<LayoutNode>> shown);
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

internal sealed record Recording(Context Context, float Alpha, HashSet<Seq<Key>> Live);

internal sealed record PaintSlot(Seq<Key> Path, int Index);

internal sealed record RoleKey(Seq<Key> Path, Role Role);

internal sealed record ShadowKey(Seq<Key> Path, Role Role, Shadow Shadow);

internal sealed record OutlineKey(Seq<Key> Path, float Offset);

internal sealed record OutlineShape(Shape Shape, Corners Corners, float Smoothing, SKRect Frame);

internal sealed record FillKey(Seq<StateLayer> StateLayers, SKRect Frame, float Scale, float Clock, Option<SKPoint> Pointer);

internal sealed record StrokeKey(SKColorF Color, float Width, Seq<PathEffect> Patterns, float Clock, Option<SKBlender> Blend);

internal sealed record LayerKey(float Alpha, Option<SKImageFilter> Filter);

internal sealed record MatteKey(SKSize Size, Corners Corners, float Smoothing, float Scale);

internal sealed record TextKey(Seq<Key> Path, SKColor Tint);

internal sealed record TextOutlineKey(Typeset Set, SKPoint Origin);

internal sealed record ImagePaintKey(Option<SKColorF> Color, Option<ColorRamp> Scale);

internal sealed record MarkKey(Figure Figure, PlotFrame Frame, Pen Pen);

internal sealed record ShownMedia(Seq<Key> Path, LoadedMedia Media);

internal sealed record RetainedScene(Scene Scene, HashSet<Seq<Key>> Leaving, float Scale, HashSet<Seq<Key>> Moving, Seq<ShownMedia> Motion);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class Entries<TSlot, TKey, TValue>(bool byReference) : ISlot where TSlot : notnull {
    private HashMap<TSlot, Entry> held;

    internal Func<TValue, Seq<IDisposable>> Holds { get; init; } = static _ => [];

    internal Seq<TValue> Values => held.Values.ToSeq().Map(static entry => entry.Value);

    internal Option<TValue> Find(TSlot slot) => held.Find(slot).Map(static entry => entry.Value);

    internal TValue GetOrAdd(TSlot slot, LayoutNode layout, TKey key, Func<TValue> make) =>
        held.Find(slot).Filter(found => Reuses(found.Layout, layout) && EqDefault<TKey>.Equals(found.Key, key)).Match(
            Some: found => {
                if (!ReferenceEquals(found.Layout, layout)) held = held.SetItem(slot, found with { Layout = layout });
                return found.Value;
            },
            None: () => {
                TValue made = make();
                _ = held.Find(slot).Iter(stale => Release(stale.Value));
                held = held.AddOrUpdate(slot, new Entry(layout, key, made));
                return made;
            });

    void ISlot.Retain(Func<Seq<Key>, Option<LayoutNode>> shown) => KeepOnly(entry => shown(entry.Layout.Path).Exists(next => Reuses(entry.Layout, next)));

    void ISlot.Clear() => KeepOnly(static _ => false);

    private bool Reuses(LayoutNode before, LayoutNode after) => !byReference || ReferenceEquals(before, after);

    private void KeepOnly(Func<Entry, bool> kept) {
        _ = held.Filter(entry => !kept(entry)).Values.Iter(entry => Release(entry.Value));
        held = held.Filter(kept);
    }

    private void Release(TValue value) => _ = Holds(value).Iter(static resource => resource.Dispose());

    private sealed record Entry(LayoutNode Layout, TKey Key, TValue Value);
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
        slots = [Recordings, Leaves, Filters, Shadows, Casts, Outlines, Brushes, Backdrops, Mattes, Edges, Fades, Veils, Images, Tints, Solids, Texts, TextOutlines, TextStrokes,
            Interiors, Contours, Dots, Sprites, Atlases, Fields, Meshes, Labels, Arts, Decoded];
    }

    internal Func<SKImageInfo, Option<SKSurface>> Offscreen { get; }
    internal TextPaintOptions Raster { get; }
    internal SKColorSpace Space { get; }
    internal SKCanvasSaveLayerRecFlags Precision { get; }
    internal Seq<Seq<Key>> Animated { get; private set; }
    internal HashSet<Seq<Key>> Moving => retained.Map(static held => held.Moving).IfNone([]);

    // --- [SLOTS]
    internal Entries<Seq<Key>, Recording, SKDrawable> Recordings { get; } = new(byReference: true) { Holds = static held => [held] };
    internal Entries<Seq<Key>, Unit, Painter.Leaf> Leaves { get; } = new(byReference: true) { Holds = static held => [held] };
    internal Entries<Seq<Key>, SKRect, Option<SKImageFilter>> Filters { get; } = new(byReference: true) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<ShadowKey, SKColorF, SKPaint> Shadows { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<RoleKey, Seq<ResolvedShadow>, SKPaint> Casts { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<OutlineKey, OutlineShape, Fin<Outline>> Outlines { get; } = new(byReference: false) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<PaintSlot, FillKey, Option<SKPaint>> Brushes { get; } = new(byReference: true) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<PaintSlot, FillKey, Option<SKImageFilter>> Backdrops { get; } = new(byReference: true) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<Seq<Key>, MatteKey, Option<SKImage>> Mattes { get; } = new(byReference: false) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<PaintSlot, StrokeKey, SKPaint> Edges { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, Fade, SKPaint> Fades { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<RoleKey, LayerKey, SKPaint> Veils { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, ImagePaintKey, SKPaint> Images { get; } = new(byReference: true) { Holds = static held => [held] };
    internal Entries<Seq<Key>, SKColorF, SKPaint> Tints { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, SKColorF, SKPaint> Solids { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<TextKey, Typeset, TextBlock> Texts { get; } = new(byReference: false);
    internal Entries<Seq<Key>, TextOutlineKey, SKPath> TextOutlines { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, StrokeKey, SKPaint> TextStrokes { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, MarkKey, Outline> Interiors { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, MarkKey, Outline> Contours { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, Marks.DotsKey, SKPicture> Dots { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, Marks.SpriteKey, Option<SKImage>> Sprites { get; } = new(byReference: false) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<Seq<Key>, Marks.AtlasKey, SKPicture> Atlases { get; } = new(byReference: false) { Holds = static held => [held] };
    internal Entries<Seq<Key>, Marks.FieldKey, Option<SKPaint>> Fields { get; } = new(byReference: true) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<Seq<Key>, MarkKey, Seq<SKVertices>> Meshes { get; } = new(byReference: false) { Holds = static meshes => [.. meshes] };
    internal Entries<Seq<Key>, Marks.LabelKey, Option<SKTextBlob>> Labels { get; } = new(byReference: false) { Holds = static held => [.. held.ToSeq()] };
    internal Entries<ArtKey, HashMap<string, SKColor>, Fin<LoadedMedia>> Arts { get; } = new(byReference: false) { Holds = static loaded => [.. loaded.ToSeq()] };
    internal Entries<Seq<Key>, DecodeKey, Fin<Decoded>> Decoded { get; } = new(byReference: false) { Holds = static decoded => [.. decoded.ToSeq()] };

    // --- [LAYERS]
    internal SKPaint Veil(LayoutNode layout, Role role, float alpha, Option<SKImageFilter> filter) =>
        Veils.GetOrAdd(new RoleKey(layout.Path, role), layout, new LayerKey(alpha, filter), () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha), ImageFilter = filter.ValueUnsafe() });

    internal SKPaint Cast(LayoutNode layout, Role role, Seq<ResolvedShadow> stack) =>
        Casts.GetOrAdd(new RoleKey(layout.Path, role), layout, stack, () => {
            Seq<SKImageFilter> graph = stack.Filter(static held => !held.Shadow.Inside).Map(held => held.Shadow.Filtered(held.Color, Space)).Strict().Add(Source)
                + stack.Filter(static held => held.Shadow.Inside).Map(held => held.Shadow.Filtered(held.Color, Space)).Strict();
            using SKImageFilter merged = SKImageFilter.CreateMerge([.. graph]);
            _ = graph.Filter(static filter => !ReferenceEquals(filter, Source)).Iter(static filter => filter.Dispose());
            return new SKPaint { ImageFilter = merged };
        });

    internal SKPaint Shadowed(LayoutNode layout, Role role, ResolvedShadow held) =>
        Shadows.GetOrAdd(new ShadowKey(layout.Path, role, held.Shadow), layout, held.Color, () => {
            using SKMaskFilter? blur = held.Shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, held.Shadow.Blur) : null;
            return new SKPaint { IsAntialias = true, ColorF = held.Color, MaskFilter = blur };
        });

    // --- [OUTLINES]
    internal Option<Outline> Outlined(LayoutNode layout, SKRect frame, float offset) => Lowered(layout, frame, offset).ToOption();

    private Fin<Outline> Lowered(LayoutNode layout, SKRect frame, float offset) =>
        Outlines.GetOrAdd(new OutlineKey(layout.Path, offset), layout, new OutlineShape(layout.Style.Shape, layout.Corners, layout.Smoothing, frame),
            () => Outline.Of(layout.Style.Shape, layout.Corners, layout.Smoothing, frame, offset));

    // --- [PAINTS]
    internal static SKPaint StrokePaint<TSlot>(Entries<TSlot, StrokeKey, SKPaint> slot, TSlot at, LayoutNode layout, StrokeKey key) where TSlot : notnull =>
        slot.GetOrAdd(at, layout, key, () => {
            using SKPathEffect? effect = Stroke.Effect(key.Patterns, key.Width, key.Clock).ValueUnsafe();
            return new SKPaint { IsAntialias = true, ColorF = key.Color, Style = SKPaintStyle.Stroke, StrokeWidth = key.Width, StrokeCap = SKStrokeCap.Round, PathEffect = effect, Blender = key.Blend.ValueUnsafe() };
        });

    internal SKPaint FadeMask(LayoutNode layout, Fade fade) =>
        Fades.GetOrAdd(layout.Path, layout, fade, () => {
            using SKShader shader = SKShader.CreateLinearGradient(fade.From, fade.To, [fade.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, fade.After ? SKColors.Transparent : SKColors.Black],
                [0f, fade.Edge, 1f - fade.Edge, 1f], SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn };
        });

    internal SKPaint Tint(LayoutNode layout, SKColorF tint) =>
        Tints.GetOrAdd(layout.Path, layout, tint, () => {
            using SKColorFilter tintFilter = TintMatrix(tint);
            return new SKPaint { IsAntialias = true, ColorFilter = tintFilter };
        });

    internal SKPaint Solid(LayoutNode layout, SKColorF color) => Solids.GetOrAdd(layout.Path, layout, color, () => new SKPaint { IsAntialias = true, ColorF = color });

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

    internal Option<SKImage> Matte(LayoutNode layout, MatteKey key) =>
        Mattes.GetOrAdd(layout.Path, layout, key, () => {
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
        Seq<LayoutNode> current = (Seq(scene.Layout) + scene.Layers + scene.Fragments.Values.ToSeq()).Bind(Scene.Preorder).Strict();
        Seq<LayoutNode> departed = (toSeq(mount.Animations.Exiting.Values) + toSeq(mount.Animations.Departing.Values)).Map(static left => left.Last).Bind(Scene.Preorder).Strict();
        Seq<LayoutNode> drawn = current + departed;
        HashMap<Seq<Key>, LayoutNode> shown = toHashMap(departed.Map(static layout => (layout.Path, layout))).AddOrUpdateRange(current.Map(static layout => (layout.Path, layout)));
        Seq<(Seq<Key> Path, Fin<LoadedMedia> Media)> arts = drawn.Bind(layout => Loaded(layout, scene.ColorCache)).Strict();
        Seq<(Seq<Key> Path, Fin<Decoded> Frames)> images = drawn.Choose(layout => Decode(layout, scale)).Strict();
        Seq<Fin<Outline>> outlines = drawn.Filter(static layout => layout.Style.Shape is not Shape.Rounded).Map(layout => Lowered(layout, SKRect.Create(layout.Frame.Size), 0f)).Strict();
        return (arts.Traverse(static held => held.Media).As(), images.Traverse(static held => held.Frames).As(), outlines.Traverse(static held => held).As()).Apply((_, _, _) => {
            Seq<ShownMedia> motion = arts.Choose(static held => held.Media.ToOption().Filter(static media => media.Animated).Map(media => new ShownMedia(held.Path, media))).Strict();
            Animated = toSeq(motion.Map(static held => held.Path) + images.Filter(static held => held.Frames.Exists(static frames => frames.Animated)).Map(static held => held.Path)).Distinct().Strict();
            retained = new RetainedScene(scene, leaving, scale, toHashSet(drawn.Filter(static layout => layout.Timed || layout.Style.Pointing).Map(static layout => layout.Path) + Animated), motion);
            return slots.Iter(slot => slot.Retain(path => shown.Find(path)));
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
        HashMap<string, SKColor> chrome = Media.Chrome(layout.Cascade.Theme, layout.Style);
        Seq<GlyphSource> sources = layout.Node.GlyphArt.ToSeq()
            + layout.Style.Fills.Choose(static fill => fill is Material.Pattern { Source: PatternSource.Art art } ? Some(art.Glyph) : None)
            + layout.Node.Traced.ToSeq().Bind(Marks.Markers).Map(static marker => marker.Art);
        return sources.Map(source => Media.Keyed(layout, source, under, colorCache)).Map(key => (layout.Path, Arts.GetOrAdd(key, layout, chrome, () => media.Load(key, chrome))));
    }

    private Option<(Seq<Key> Path, Fin<Decoded> Frames)> Decode(LayoutNode layout, float scale) =>
        layout.Node is Node.Image { Source: ImageSource.Encoded encoded } image
            ? new DecodeKey(encoded, Pixels(image, encoded, layout.ContentBox, scale)) switch {
                var key => Some((layout.Path, Decoded.GetOrAdd(layout.Path, layout, key, () => media.Decode(key)))),
            }
            : None;

    internal Option<SKImage> Frame(LayoutNode layout, ImageSource source, Duration now) =>
        source.Switch(
            (Cache: this, Layout: layout, Now: now),
            still: static (_, still) => Some(still.Image),
            encoded: static (held, _) => held.Cache.Decoded.Find(held.Layout.Path).Bind(static frames => frames.ToOption()).Map(frames => frames.At(held.Now)));

    internal Option<LoadedMedia> Art(ArtKey key) => Arts.Find(key).Bind(static loaded => loaded.ToOption());

    private static SKSizeI Pixels(Node.Image image, ImageSource source, SKRect content, float scale) {
        SKRect fitted = image.Fit.Fitted(content, new SKSize(source.Info.Width, source.Info.Height));
        return SKRectI.Ceiling(SKRect.Create(fitted.Width * scale, fitted.Height * scale), outwards: true).Size;
    }
}
