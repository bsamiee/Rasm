using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [TYPES] ---------------------------------------------------------------------------

internal interface ISlot : IDisposable {
    public void Evict();
}

// --- [SERVICES] ------------------------------------------------------------------------

internal sealed class Slot<TKey, TValue>(Action<TValue> release) : ISlot where TKey : notnull {
    private readonly Lock gate = new();
    private HashMap<TKey, TValue> live;
    private HashMap<TKey, TValue> aging;

    internal Seq<TValue> Values {
        get {
            lock (gate) return toSeq(live.Values) + toSeq(aging.Values);
        }
    }

    internal Option<TValue> Find(TKey key) {
        lock (gate) return live.Find(key) || aging.Find(key).Map(value => Kept(key, value));
    }

    internal TValue GetOrAdd(TKey key, Func<TValue> make) {
        lock (gate) return Find(key).IfNone(() => Added(key, make()));
    }

    internal Fin<TValue> GetOrAdd(TKey key, Func<Fin<TValue>> make) {
        lock (gate) return Find(key).Match(Some: Fin.Succ, None: () => make().Map(made => Added(key, made)));
    }

    public void Evict() {
        HashMap<TKey, TValue> dead;
        lock (gate) (dead, live, aging) = (aging, [], live);
        _ = toSeq(dead.Values).Iter(release);
    }

    public void Dispose() {
        HashMap<TKey, TValue> dead;
        lock (gate) (dead, live, aging) = (live + aging, [], []);
        _ = toSeq(dead.Values).Iter(release);
    }

    private TValue Kept(TKey key, TValue value) {
        (live, aging) = (live.AddOrUpdate(key, value), aging.Remove(key));
        return value;
    }

    private TValue Added(TKey key, TValue made) {
        live = live.AddOrUpdate(key, made);
        return made;
    }
}

internal sealed class PaintCache : IDisposable {

    // --- [STATE]

    private static readonly SKImageFilter Source = SKImageFilter.CreateOffset(0f, 0f);
    private readonly SKImageInfo target;
    private readonly SKSurfaceProperties raster;
    private readonly Option<GRRecordingContext> context;
    private readonly Decoding media;
    private readonly Seq<ISlot> sceneSlots;
    private readonly Seq<ISlot> frameSlots;
    private Option<(Scene Scene, HashSet<LayoutNode> Leaving, float Scale, HashSet<Seq<Key>> Moving, HashMap<(LayoutNode Layout, GlyphSource Source), LoadedMedia> Art)> retained;

    internal PaintCache(SKImageInfo target, SKSurfaceProperties raster, Shaping shaping, Option<GRRecordingContext> context) {
        (this.target, this.raster, this.context) = (target, raster, context);
        media = new Decoding(target, shaping, Offscreen);
        using SKColorSpace srgb = SKColorSpace.CreateSrgb();
        Space = target.ColorSpace ?? srgb;
        Raster = new TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
        Blobs = run => TextBlobs.GetOrAdd(run, () => {
            using SKFont font = new(run.Typeface, run.FontSize) { Edging = Raster.Edging, Hinting = Raster.Hinting, Subpixel = Raster.SubpixelPositioning };
            using SKTextBlobBuilder builder = new();
            builder.AddPositionedRun(run.Glyphs.Span, font, run.Positions.Span);
            return Optional(builder.Build());
        });
        sceneSlots = [Recordings, Leaves, TextOutlines, TextBlobs, Interiors, Contours, Sprites, Meshes, Labels, Mattes, Arts, Decoded];
        frameSlots = [Filters, Brushes, Backdrops, Images, Valued, Dots, Atlases, Fields, Shadows, Casts, Strokes, Fades, Opacities, Tints, Solids];
        Precision = target.BytesPerPixel < 8 ? SKCanvasSaveLayerRecFlags.F16ColorType : SKCanvasSaveLayerRecFlags.None;
    }

    internal TextPaintOptions Raster { get; }
    internal SKColorSpace Space { get; }
    internal SKCanvasSaveLayerRecFlags Precision { get; }
    internal Func<StatePalette, Func<Seq<int>, SKColorF>> Palettes { get; } = memo<StatePalette, Func<Seq<int>, SKColorF>>(static key => key.Samples);
    internal Func<Typeset.Run, Option<SKTextBlob>> Blobs { get; }
    internal Seq<Seq<Key>> Animated { get; private set; }
    internal HashSet<Seq<Key>> Moving => retained.Map(static held => held.Moving).IfNone([]);

    // --- [SLOTS]

    internal Slot<(LayoutNode Layout, Context Context, float Alpha, HashSet<Seq<Key>> Live), SKDrawable> Recordings { get; } = new(static value => value.Dispose());
    internal Slot<LayoutNode, Painter.Leaf> Leaves { get; } = new(static value => value.Dispose());
    internal Slot<Typeset, SKPath> TextOutlines { get; } = new(static value => value.Dispose());
    private Slot<Typeset.Run, Option<SKTextBlob>> TextBlobs { get; } = new(static value => value.Iter(static blob => blob.Dispose()));
    internal Slot<MarkKey, Outline.Traced> Interiors { get; } = new(static value => value.Dispose());
    internal Slot<MarkKey, Outline.Traced> Contours { get; } = new(static value => value.Dispose());
    internal Slot<(LayoutNode Layout, Option<Marker> Marker, float Diameter, float Scale), Option<SKImage>> Sprites { get; } = new(static value => value.Iter(static image => image.Dispose()));
    internal Slot<MarkKey, Seq<SKVertices>> Meshes { get; } = new(static value => value.Iter(static vertices => vertices.Dispose()));
    internal Slot<(Typeset Set, MarkKey Mark), Option<SKTextBlob>> Labels { get; } = new(static value => value.Iter(static blob => blob.Dispose()));
    internal Slot<(SKSize Size, Corners Corners, float Smoothing, float Scale), Option<SKImage>> Mattes { get; } = new(static value => value.Iter(static image => image.Dispose()));
    internal Slot<FillKey, Option<SKImageFilter>> Filters { get; } = new(static value => value.Iter(static filter => filter.Dispose()));
    internal Slot<FillKey, Option<SKPaint>> Brushes { get; } = new(static value => value.Iter(static paint => paint.Dispose()));
    internal Slot<FillKey, Option<SKImageFilter>> Backdrops { get; } = new(static value => value.Iter(static filter => filter.Dispose()));
    internal Slot<(LayoutNode Layout, Option<SKColorF> Color, bool AlphaOnly), SKPaint> Images { get; } = new(static value => value.Dispose());
    internal Slot<(LayoutNode Layout, SKImage Source, SKRect Target, SKSamplingOptions Sampling), Option<SKPaint>> Valued { get; } = new(static value => value.Iter(static paint => paint.Dispose()));
    internal Slot<(MarkKey Mark, Seq<SKPaint> Paints), SKPicture> Dots { get; } = new(static value => value.Dispose());
    internal Slot<(MarkKey Mark, Option<Marker> Marker, SKColorF Color), SKPicture> Atlases { get; } = new(static value => value.Dispose());
    internal Slot<(MarkKey Mark, Option<SKColorF> Lead), Option<SKPaint>> Fields { get; } = new(static value => value.Iter(static paint => paint.Dispose()));
    private Slot<ArtKey, LoadedMedia> Arts { get; } = new(static value => value.Dispose());
    private Slot<(ImageSource.Encoded Source, SKSizeI Pixels), Decoded> Decoded { get; } = new(static value => value.Dispose());
    private Slot<(Shadow Shadow, SKColorF Color), SKPaint> Shadows { get; } = new(static value => value.Dispose());
    private Slot<Seq<(Shadow Shadow, SKColorF Color)>, SKPaint> Casts { get; } = new(static value => value.Dispose());
    private Slot<(Stroke Stroke, SKColorF Color, float Width, uint Ordinal, float Clock, Option<SKBlender> Blend), SKPaint> Strokes { get; } = new(static value => value.Dispose());
    private Slot<(SKPoint From, SKPoint To, bool Before, bool After, float Edge), SKPaint> Fades { get; } = new(static value => value.Dispose());
    private Slot<(float Alpha, Option<SKImageFilter> Filter), SKPaint> Opacities { get; } = new(static value => value.Dispose());
    private Slot<(Option<SKPicture> Still, SKColorF Tint), SKPaint> Tints { get; } = new(static value => value.Dispose());
    private Slot<SKColorF, SKPaint> Solids { get; } = new(static value => value.Dispose());

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
            using SKPathEffect? effect = (stroke.Patterns.IsEmpty ? [] : stroke.Patterns[(int)(ordinal % (uint)stroke.Patterns.Count)])
                .Fold(Option<SKPathEffect>.None, (inner, pattern) => Some(inner.Match(
                    Some: held => {
                        using (held)
                        using (SKPathEffect outer = pattern.Effect(width, clock))
                            return SKPathEffect.CreateCompose(outer, held);
                    },
                    None: () => pattern.Effect(width, clock)))).ValueUnsafe();
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

    internal Option<SKPaint> Tint(LoadedMedia art, GlyphSource source, Func<Option<SKColorF>> color) =>
        source.Tinted
            ? color().Map(tint => Tints.GetOrAdd((art.Still, tint), () => {
                using SKColorFilter washed = Space.Tinted(tint, inverted: false);
                using SKShader? shader = art.Still.Map(static picture => picture.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, SKFilterMode.Linear, SKMatrix.Identity, picture.CullRect)).ValueUnsafe();
                return new SKPaint { IsAntialias = true, ColorFilter = washed, Shader = shader };
            }))
            : None;

    internal SKPaint Solid(SKColorF color) => Solids.GetOrAdd(color, () => new SKPaint { IsAntialias = true, ColorF = color });

    // --- [OFFSCREENS]

    internal Option<SKImage> Rasterized(SKSize size, float scale, SKColorType type, Action<SKCanvas> draw) {
        SKSizeI pixels = Capture.Pixels(size, scale);
        return Offscreen(new SKImageInfo(int.Max(1, pixels.Width), int.Max(1, pixels.Height), type, SKAlphaType.Premul, Space)).Map(surface => {
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

    private Option<SKSurface> Offscreen(SKImageInfo info) => Optional(context.Match(gpu => SKSurface.Create(gpu, budgeted: true, info, raster), () => SKSurface.Create(info, raster)));

    // --- [RETENTION]

    internal Fin<Unit> Retain(Scene scene, Seq<LayoutNode> leaving, float scale) {
        HashSet<LayoutNode> left = toHashSet(leaving);
        if (retained.Exists(held => ReferenceEquals(held.Scene, scene) && held.Leaving == left && held.Scale.CompareTo(scale) is 0)) return unit;
        Seq<LayoutNode> drawn = (scene.Layout.Cons(scene.Layers) + scene.Fragments.Values.ToSeq() + leaving).Bind(static layout => layout.Preorder).Strict();
        _ = drawn.Iter(layout => Leaves.Find(layout));
        _ = drawn.Choose(static layout => layout.TextNode).Bind(static text => text.Typeset.Lines).Bind(static line => line.Runs).Iter(held => TextBlobs.Find(held.Run));
        _ = sceneSlots.Add(scene.Outlines).Iter(static slot => slot.Evict());
        Seq<((LayoutNode Layout, GlyphSource Source) Shown, Fin<LoadedMedia> Media)> arts = (
            from layout in drawn
            from source in (layout.View is View.Glyph glyph ? Seq(glyph.Art) : [])
                + layout.Style.Fills.Bind(static fill => fill.Arts)
                + layout.Style.Filters.Bind(static filter => filter.Arts)
                + layout.View.Traced.ToSeq().Bind(static figure => figure.Art).Map(static marker => marker.Art)
            let under = layout.Inner.Background
            let theme = layout.Cascade.Theme
            let key = source.Animated
                ? new ArtKey(source, source.Shaded(under), None, [], [])
                : new ArtKey(source, source.Shaded(under),
                    source.Tinted ? None : theme.Color(layout.Style.Text, layout.Style).Map(resting => (SKColor)theme.Legible(layout.Style, under, resting, text: false)),
                    toHashMap(toSeq(source.Classes.AsIterable()).Choose(pair => theme.Color(pair.Value, layout.Style).Map(resolved => (pair.Key, (SKColor)resolved)))), theme.Chrome)
            select (Shown: (layout, source), Media: Arts.GetOrAdd(key, () => media.Load(key)))).Strict();
        Seq<(Seq<Key> Path, Fin<Decoded> Frames)> images = drawn.Choose(layout => layout.View is View.Image { Source: ImageSource.Encoded encoded } image
            ? Some(Decoding.Requested(layout, image, encoded, scale) switch { var key => (layout.Path, Decoded.GetOrAdd(key, () => media.Decode(key))) })
            : None).Strict();
        Seq<Fin<Outline>> outlines = drawn.Filter(static layout => layout.Style.Shape is not (Shape.Rounded or Shape.Capsule)).Map(layout => Outline.Of(scene.Outlines, layout, SKRect.Create(layout.Frame.Size), 0f)).Strict();
        return (arts.Traverse(static held => held.Media).As(), images.Traverse(static held => held.Frames).As(), outlines.Traverse(static held => held).As()).Apply((loaded, _, _) => {
            HashMap<(LayoutNode Layout, GlyphSource Source), LoadedMedia> art = toHashMap(arts.Map(static held => held.Shown).Zip(loaded));
            Animated = toSeq((toSeq(art.Filter(static shown => shown.Animated).Keys).Map(static shown => shown.Layout.Path) + images.Filter(static held => held.Frames.Exists(static frames => frames.Animated)).Map(static held => held.Path)).Distinct()).Strict();
            retained = (scene, left, scale, toHashSet(drawn.Filter(static layout => layout.Timed || layout.Style.Pointing).Map(static layout => layout.Path) + Animated), art);
            return unit;
        }).As();
    }

    internal void Framed(Painter.Pass pass) {
        _ = frameSlots.Iter(static slot => slot.Evict());
        _ = retained.Iter(held => toSeq(held.Art.Values).Filter(static shown => shown.Animated).Distinct().Iter(shown => shown.Seek(pass.Now)));
        _ = Leaves.Values.Iter(leaf => leaf.Framed(pass));
    }

    internal Option<LoadedMedia> Art(LayoutNode layout, GlyphSource source) => retained.Bind(held => held.Art.Find((layout, source)));

    internal Option<SKImage> Frame(LayoutNode layout, View.Image image, Duration now) =>
        image.Source.Switch(
            (Cache: this, Layout: layout, Image: image, Now: now),
            still: static (_, still) => Some(still.Image),
            encoded: static (held, encoded) =>
                from kept in held.Cache.retained
                from frames in held.Cache.Decoded.Find(Decoding.Requested(held.Layout, held.Image, encoded, kept.Scale))
                select frames.At(held.Now));

    public void Dispose() => _ = (sceneSlots + frameSlots).Iter(static slot => slot.Dispose());
}
