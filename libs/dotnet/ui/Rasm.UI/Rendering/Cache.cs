using LanguageExt.ClassInstances;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Fade(SKPoint From, SKPoint To, bool Before, bool After, float Edge);

internal sealed record FillKey(Material Fill, Seq<Option<SKColorF>> Colors, SKRect Frame, SKBlendMode Blend, float Scale, float Clock);

internal sealed record Context(SKMatrix Device, Seq<StateLayer> StateLayers) {
    internal Option<PlotFrame> Plot { get; init; }

    internal Context CacheKey => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    internal float Scale => Device.MapRadius(1f);

    internal Context Translated(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    internal Pen Pen(RoleStyle style) => new(style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]) { Device = Device };

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    // --- [STATE]
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);
    private readonly Entries<Seq<Key>, Recording, SKPicture> subtrees = new(ReferenceEquals, static picture => picture.Dispose());
    private readonly Entries<Seq<Key>, Seq<Filter>, Option<SKImageFilter>> filters = new(Content, Released);
    private readonly Entries<ShadowKey, SKColorF, SKPaint> shadows = new(Content, static paint => paint.Dispose());
    private readonly Entries<OutlineKey, OutlineShape, Outline> outlines = new(Content, static outline => outline.Dispose());
    private readonly Entries<PaintSlot, FillKey, Option<SKPaint>> brushes = new(Content, Released);
    private readonly Entries<PaintSlot, StrokeKey, SKPaint> edges = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, Fade, SKPaint> fades = new(Content, static paint => paint.Dispose());
    private readonly Entries<RoleKey, LayerKey, SKPaint> veils = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, float, SKPaint> tweens = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, ImagePaintKey, SKPaint> images = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, TintKey, SKPaint> tints = new(Content, static paint => paint.Dispose());
    private readonly Seq<ISlot> slots;
    private readonly SKImageInfo target;
    private readonly SKSurfaceProperties raster;
    private readonly float density;
    private readonly Media media = new();
    private Option<RetainedScene> retained;

    public PaintCache(SKImageInfo target, SKSurfaceProperties raster, float density = 1f) {
        (this.target, this.raster, this.density) = (target, raster, density);
        slots = [subtrees, filters, shadows, outlines, brushes, edges, fades, veils, tweens, images, tints];
        Raster = new TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
    }

    public TextPaintOptions Raster { get; }

    // --- [RECORD]
    internal SKPicture Subtree(LayoutNode layout, Context context, float alpha, Action<SKCanvas> record) =>
        subtrees.GetOrAdd(layout.Path, layout, new Recording(context.CacheKey, alpha), () => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(layout.LocalSubtreeBounds, useRTree: true));
            return recorder.EndRecording();
        });

    internal Option<SKImageFilter> Filtered(LayoutNode layout, Seq<Filter> chain) => filters.GetOrAdd(layout.Path, layout, chain, () => Filter.Chained(chain));

    internal SKPaint Shadowed(LayoutNode layout, Role role, Shadow shadow, SKColorF color) =>
        shadows.GetOrAdd(new ShadowKey(layout.Path, role, shadow), layout, color, () => {
            using SKImageFilter graph = shadow.Filtered(color);
            return new SKPaint { ImageFilter = graph };
        });

    internal SKPaint LayerPaint(LayoutNode layout, Role role, float alpha, Option<SKImageFilter> filter) =>
        veils.GetOrAdd(new RoleKey(layout.Path, role), layout, new LayerKey(alpha, filter), () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha), ImageFilter = filter.ValueUnsafe() });

    internal SKPaint TweenPaint(LayoutNode layout, float alpha) => tweens.GetOrAdd(layout.Path, layout, alpha, () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha) });

    // --- [OUTLINE]
    internal Outline Outlined(LayoutNode layout, SKRect frame, float offset) =>
        outlines.GetOrAdd(new OutlineKey(layout.Path, offset), layout, new OutlineShape(layout.Corners, layout.Smoothing, frame), () => Outline.Of(layout.Corners, layout.Smoothing, frame, offset));

    // --- [PAINTS]
    internal Option<SKPaint> FillPaint(LayoutNode layout, int fill, FillKey key, Func<Option<SKPaint>> make) => brushes.GetOrAdd(new PaintSlot(layout.Path, fill), layout, key, make);

    internal SKPaint StrokePaint(LayoutNode layout, int stroke, SKColorF color, float width, Seq<float> rhythm, SKBlendMode blend) =>
        edges.GetOrAdd(new PaintSlot(layout.Path, stroke), layout, new StrokeKey(color, width, rhythm, blend), () => {
            using SKPathEffect? dash = rhythm.IsEmpty ? null : SKPathEffect.CreateDash([.. rhythm.Map(beat => beat * width)], 0f);
            return new SKPaint { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, StrokeCap = SKStrokeCap.Round, PathEffect = dash, BlendMode = blend };
        });

    internal SKPaint FadeMask(LayoutNode layout, Fade fade) =>
        fades.GetOrAdd(layout.Path, layout, fade, () => {
            using SKShader shader = SKShader.CreateLinearGradient(fade.From, fade.To, [fade.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, fade.After ? SKColors.Transparent : SKColors.Black],
                [0f, fade.Edge, 1f - fade.Edge, 1f], SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn };
        });

    internal SKPaint ImagePaint(LayoutNode layout, SKBlendMode blend, Option<SKColorF> color) =>
        images.GetOrAdd(layout.Path, layout, new ImagePaintKey(blend, color), () => color.Match(Some: held => new SKPaint { IsAntialias = true, BlendMode = blend, ColorF = held }, None: () => new SKPaint { IsAntialias = true, BlendMode = blend }));

    // --- [FILLS]
    internal Option<SKShader> DrawnShader(Material.Drawn drawn, HashMap<string, Option<SKColorF>> colors, SKRect frame, float scale, float clock) =>
        drawn.Timed
            ? Some(drawn.Shader(colors, frame, clock))
            : Rastered(SKRect.Create(frame.Size), SKMatrix.CreateScale(scale, scale), frame.Location, recording => {
                using SKShader shader = drawn.Shader(colors, SKRect.Create(frame.Size), clock);
                using SKPaint paint = new() { Shader = shader };
                recording.DrawRect(SKRect.Create(frame.Size), paint);
            });

    private Option<SKShader> Rastered(SKRect local, SKMatrix device, SKPoint at, Action<SKCanvas> record) {
        using SKPictureRecorder recorder = new();
        record(recorder.BeginRecording(local));
        using SKPicture drawn = recorder.EndRecording();
        SKMatrix grid = device.PostConcat(SKMatrix.CreateScale(density, density));
        SKRectI pixels = SKRectI.Ceiling(grid.MapRect(local), outwards: true);
        using SKSurface? surface = SKSurface.Create(target.WithSize(pixels.Width, pixels.Height), raster);
        return Optional(surface).Map(held => {
            held.Canvas.SetMatrix(grid.PostConcat(SKMatrix.CreateTranslation(-pixels.Left, -pixels.Top)));
            held.Canvas.DrawPicture(drawn);
            using SKImage image = held.Snapshot();
            return image.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, Sampling,
                SKMatrix.CreateTranslation(at.X, at.Y).PreConcat(grid.Invert()).PreConcat(SKMatrix.CreateTranslation(pixels.Left, pixels.Top)));
        });
    }

    // --- [GLYPH]
    internal void Glyph(SKCanvas canvas, LayoutNode layout, GlyphSource source, SKColorF under, Pen pen, SKColorF tint, SKRect content) {
        Art shown = Media.Resolved(layout, source, under);
        _ = media.Pictures.Find(shown).Iter(picture => {
            SKRect cull = picture.CullRect;
            SKRect fitted = ImageFit.Contain.Fitted(content, cull.Size);
            SKPoint origin = pen.Snapped(fitted.Location);
            (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
            SKMatrix place = SKMatrix.CreateScaleTranslation(x, y, origin.X - (cull.Left * x), origin.Y - (cull.Top * y));
            if (shown.Tinted) canvas.DrawRect(place.MapRect(cull), Tinted(layout, picture, tint, place));
            else canvas.DrawPicture(picture, in place);
        });
    }

    private SKPaint Tinted(LayoutNode layout, SKPicture picture, SKColorF tint, SKMatrix place) =>
        tints.GetOrAdd(layout.Path, layout, new TintKey(picture, tint, place), () => {
            using SKShader drawn = SKShader.CreatePicture(picture, SKShaderTileMode.Decal, SKShaderTileMode.Decal, SKFilterMode.Linear, place, picture.CullRect);
            using SKColorFilter ink = SKColorFilter.CreateColorMatrix([0f, 0f, 0f, 0f, tint.Red, 0f, 0f, 0f, 0f, tint.Green, 0f, 0f, 0f, 0f, tint.Blue, 0f, 0f, 0f, tint.Alpha, 0f]);
            using SKShader tinted = SKShader.CreateColorFilter(drawn, ink);
            return new SKPaint { IsAntialias = true, Shader = tinted };
        });

    // --- [RETENTION]
    internal Fin<Unit> Retain(Mount mount) {
        Scene scene = mount.Scene;
        HashSet<Seq<Key>> leaving = toHashSet(mount.Animations.Exiting.Keys);
        if (retained.Exists(held => ReferenceEquals(held.Scene, scene) && held.Leaving == leaving)) return unit;
        HashMap<Seq<Key>, LayoutNode> departed = toHashMap(mount.Animations.Exiting.Values.ToSeq().Bind(static left => Scene.Preorder(left.Last)).Map(static layout => (layout.Path, layout)));
        return media.Retain(scene.Order + departed.Values.ToSeq()).Map(_ => {
            retained = new RetainedScene(scene, leaving);
            return slots.Iter(slot => slot.Retain(path => scene.Index.Find(path) | departed.Find(path)));
        });
    }

    public void Dispose() {
        _ = slots.Iter(static slot => slot.Clear());
        media.Dispose();
    }

    private static bool Content(LayoutNode held, LayoutNode layout) => true;

    private static void Released<T>(Option<T> held) where T : IDisposable => _ = held.Iter(static value => value.Dispose());

    private interface ISlot {
        public void Retain(Func<Seq<Key>, Option<LayoutNode>> shown);
        public void Clear();
    }

    private sealed record Recording(Context Context, float Alpha);

    private sealed record ShadowKey(Seq<Key> Path, Role Role, Shadow Shadow);

    private sealed record OutlineKey(Seq<Key> Path, float Distance);

    private sealed record OutlineShape(Corners Corners, float Smoothing, SKRect Frame);

    private sealed record PaintSlot(Seq<Key> Path, int Index);

    private sealed record StrokeKey(SKColorF Color, float Width, Seq<float> Rhythm, SKBlendMode Blend);

    private sealed record RoleKey(Seq<Key> Path, Role Role);

    private sealed record LayerKey(float Alpha, Option<SKImageFilter> Filter);

    private sealed record ImagePaintKey(SKBlendMode Blend, Option<SKColorF> Color);

    private sealed record TintKey(SKPicture Art, SKColorF Color, SKMatrix Place);

    private sealed record RetainedScene(Scene Scene, HashSet<Seq<Key>> Leaving);

    private sealed class Entries<TSlot, TKey, TValue>(Func<LayoutNode, LayoutNode, bool> reuses, Action<TValue> release) : ISlot where TSlot : notnull {
        private HashMap<TSlot, Entry> held;

        internal TValue GetOrAdd(TSlot slot, LayoutNode layout, TKey key, Func<TValue> make) =>
            held.Find(slot).Filter(found => reuses(found.Layout, layout) && EqDefault<TKey>.Equals(found.Key, key)).Match(
                Some: static found => found.Value,
                None: () => {
                    TValue made = make();
                    _ = held.Find(slot).Iter(stale => release(stale.Value));
                    held = held.AddOrUpdate(slot, new Entry(layout, key, made));
                    return made;
                });

        void ISlot.Retain(Func<Seq<Key>, Option<LayoutNode>> shown) => KeepOnly(entry => shown(entry.Layout.Path).Exists(held => reuses(entry.Layout, held)));

        void ISlot.Clear() => KeepOnly(static _ => false);

        private sealed record Entry(LayoutNode Layout, TKey Key, TValue Value);

        private void KeepOnly(Func<Entry, bool> kept) {
            _ = held.Filter(entry => !kept(entry)).Values.Iter(entry => release(entry.Value));
            held = held.Filter(kept);
        }
    }
}
