using LanguageExt.ClassInstances;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using SkiaSharp;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Fade(SKPoint From, SKPoint To, bool Before, bool After, float Edge);

internal sealed record Brushing(Material Fill, Seq<Option<SKColorF>> Colors, SKRect Frame, SKBlendMode Blend, float Scale, float Clock);

internal sealed record Context(SKMatrix Device, Seq<Wash> Washes) {
    internal Option<PlotFrame> Plot { get; init; }

    internal Context Keyed => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    internal float Scale => Device.MapRadius(1f);

    internal Context Within(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    internal Pen Pen(RoleStyle style) => new(style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]) { Device = Device };

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    // --- [STATE]
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);
    private readonly Entries<Seq<Key>, Recording, SKPicture> subtrees = new(ReferenceEquals, static picture => picture.Dispose());
    private readonly Entries<Seq<Key>, Seq<Filter>, Option<SKImageFilter>> filters = new(Content, Released);
    private readonly Entries<Cast, SKColorF, SKPaint> shadows = new(Content, static paint => paint.Dispose());
    private readonly Entries<Offset, Shape, Outline> outlines = new(Content, static outline => outline.Dispose());
    private readonly Entries<Layer, Brushing, Option<SKPaint>> brushes = new(Content, Released);
    private readonly Entries<Layer, Edge, SKPaint> edges = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, Fade, SKPaint> fades = new(Content, static paint => paint.Dispose());
    private readonly Entries<Roled, Veil, SKPaint> veils = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, float, SKPaint> tweens = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, Imaging, SKPaint> images = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, Tint, SKPaint> tints = new(Content, static paint => paint.Dispose());
    private readonly Seq<ISlot> slots;
    private readonly SKImageInfo target;
    private readonly SKSurfaceProperties raster;
    private readonly float density;
    private readonly Media media = new();
    private Option<Retained> retained;

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
    internal SKPicture Subtree(Laid laid, Context context, float alpha, Action<SKCanvas> record) =>
        subtrees.Kept(laid.Path, laid, new Recording(context.Keyed, alpha), () => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(laid.LocalReach, useRTree: true));
            return recorder.EndRecording();
        });

    internal Option<SKImageFilter> Filtered(Laid laid, Seq<Filter> chain) => filters.Kept(laid.Path, laid, chain, () => Filter.Chained(chain));

    internal SKPaint Shadowed(Laid laid, Role role, Shadow shadow, SKColorF color) =>
        shadows.Kept(new Cast(laid.Path, role, shadow), laid, color, () => {
            using SKImageFilter graph = shadow.Filtered(color);
            return new SKPaint { ImageFilter = graph };
        });

    internal SKPaint Veiled(Laid laid, Role role, float alpha, Option<SKImageFilter> filter) =>
        veils.Kept(new Roled(laid.Path, role), laid, new Veil(alpha, filter), () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha), ImageFilter = filter.ValueUnsafe() });

    internal SKPaint Tweened(Laid laid, float alpha) => tweens.Kept(laid.Path, laid, alpha, () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha) });

    // --- [OUTLINE]
    internal Outline Outlined(Laid laid, SKRect frame, float offset) =>
        outlines.Kept(new Offset(laid.Path, offset), laid, new Shape(laid.Corners, laid.Smoothing, frame), () => Outline.Of(laid.Corners, laid.Smoothing, frame, offset));

    // --- [PAINTS]
    internal Option<SKPaint> Brushed(Laid laid, int fill, Brushing key, Func<Option<SKPaint>> make) => brushes.Kept(new Layer(laid.Path, fill), laid, key, make);

    internal SKPaint Edged(Laid laid, int stroke, SKColorF color, float width, Seq<float> rhythm, SKBlendMode blend) =>
        edges.Kept(new Layer(laid.Path, stroke), laid, new Edge(color, width, rhythm, blend), () => {
            using SKPathEffect? dash = rhythm.IsEmpty ? null : SKPathEffect.CreateDash([.. rhythm.Map(beat => beat * width)], 0f);
            return new SKPaint { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, StrokeCap = SKStrokeCap.Round, PathEffect = dash, BlendMode = blend };
        });

    internal SKPaint Masked(Laid laid, Fade fade) =>
        fades.Kept(laid.Path, laid, fade, () => {
            using SKShader shader = SKShader.CreateLinearGradient(fade.From, fade.To, [fade.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, fade.After ? SKColors.Transparent : SKColors.Black],
                [0f, fade.Edge, 1f - fade.Edge, 1f], SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn };
        });

    internal SKPaint Imaged(Laid laid, SKBlendMode blend, Option<SKColorF> color) =>
        images.Kept(laid.Path, laid, new Imaging(blend, color), () => color.Match(Some: held => new SKPaint { IsAntialias = true, BlendMode = blend, ColorF = held }, None: () => new SKPaint { IsAntialias = true, BlendMode = blend }));

    // --- [FILLS]
    internal Option<SKShader> Drawn(Material.Drawn drawn, HashMap<string, Option<SKColorF>> colors, SKRect frame, float scale, float clock) =>
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
    internal void Glyph(SKCanvas canvas, Laid laid, GlyphSource source, SKColorF under, Pen pen, SKColorF tint, SKRect content) {
        Art shown = Media.Shown(laid, source, under);
        _ = media.Pictures.Find(shown).Iter(picture => {
            SKRect cull = picture.CullRect;
            SKRect fitted = ImageFit.Contain.Fitted(content, cull.Size);
            SKPoint origin = pen.Snapped(fitted.Location);
            (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
            SKMatrix place = SKMatrix.CreateScaleTranslation(x, y, origin.X - (cull.Left * x), origin.Y - (cull.Top * y));
            if (shown.Tinted) canvas.DrawRect(place.MapRect(cull), Tinted(laid, picture, tint, place));
            else canvas.DrawPicture(picture, in place);
        });
    }

    private SKPaint Tinted(Laid laid, SKPicture picture, SKColorF tint, SKMatrix place) =>
        tints.Kept(laid.Path, laid, new Tint(picture, tint, place), () => {
            using SKShader drawn = SKShader.CreatePicture(picture, SKShaderTileMode.Decal, SKShaderTileMode.Decal, SKFilterMode.Linear, place, picture.CullRect);
            using SKColorFilter ink = SKColorFilter.CreateColorMatrix([0f, 0f, 0f, 0f, tint.Red, 0f, 0f, 0f, 0f, tint.Green, 0f, 0f, 0f, 0f, tint.Blue, 0f, 0f, 0f, tint.Alpha, 0f]);
            using SKShader tinted = SKShader.CreateColorFilter(drawn, ink);
            return new SKPaint { IsAntialias = true, Shader = tinted };
        });

    // --- [RETENTION]
    internal Fin<Unit> Retain(Mount mount) {
        Scene scene = mount.Scene;
        HashSet<Seq<Key>> leaving = toHashSet(mount.Animations.Leaving.Keys);
        if (retained.Exists(held => ReferenceEquals(held.Scene, scene) && held.Leaving == leaving)) return unit;
        HashMap<Seq<Key>, Laid> departed = toHashMap(mount.Animations.Leaving.Values.ToSeq().Bind(static left => Scene.Preorder(left.Last)).Map(static laid => (laid.Path, laid)));
        return media.Retain(scene.Order + departed.Values.ToSeq()).Map(_ => {
            retained = new Retained(scene, leaving);
            return slots.Iter(slot => slot.Retain(path => scene.Index.Find(path) | departed.Find(path)));
        });
    }

    public void Dispose() {
        _ = slots.Iter(static slot => slot.Clear());
        media.Dispose();
    }

    private static bool Content(Laid held, Laid laid) => true;

    private static void Released<T>(Option<T> held) where T : IDisposable => _ = held.Iter(static value => value.Dispose());

    private interface ISlot {
        public void Retain(Func<Seq<Key>, Option<Laid>> shown);
        public void Clear();
    }

    private sealed record Recording(Context Context, float Alpha);

    private sealed record Cast(Seq<Key> Path, Role Role, Shadow Shadow);

    private sealed record Offset(Seq<Key> Path, float Distance);

    private sealed record Shape(Corners Corners, float Smoothing, SKRect Frame);

    private sealed record Layer(Seq<Key> Path, int Index);

    private sealed record Edge(SKColorF Color, float Width, Seq<float> Rhythm, SKBlendMode Blend);

    private sealed record Roled(Seq<Key> Path, Role Role);

    private sealed record Veil(float Alpha, Option<SKImageFilter> Filter);

    private sealed record Imaging(SKBlendMode Blend, Option<SKColorF> Color);

    private sealed record Tint(SKPicture Art, SKColorF Color, SKMatrix Place);

    private sealed record Retained(Scene Scene, HashSet<Seq<Key>> Leaving);

    private sealed class Entries<TSlot, TKey, TValue>(Func<Laid, Laid, bool> reuses, Action<TValue> release) : ISlot where TSlot : notnull {
        private HashMap<TSlot, Entry> held;

        internal TValue Kept(TSlot slot, Laid laid, TKey key, Func<TValue> make) =>
            held.Find(slot).Filter(found => reuses(found.Laid, laid) && EqDefault<TKey>.Equals(found.Key, key)).Match(
                Some: static found => found.Value,
                None: () => {
                    TValue made = make();
                    _ = held.Find(slot).Iter(stale => release(stale.Value));
                    held = held.AddOrUpdate(slot, new Entry(laid, key, made));
                    return made;
                });

        void ISlot.Retain(Func<Seq<Key>, Option<Laid>> shown) => Pruned(entry => shown(entry.Laid.Path).Exists(held => reuses(entry.Laid, held)));

        void ISlot.Clear() => Pruned(static _ => false);

        private sealed record Entry(Laid Laid, TKey Key, TValue Value);

        private void Pruned(Func<Entry, bool> kept) {
            _ = held.Filter(entry => !kept(entry)).Values.Iter(entry => release(entry.Value));
            held = held.Filter(kept);
        }
    }
}
