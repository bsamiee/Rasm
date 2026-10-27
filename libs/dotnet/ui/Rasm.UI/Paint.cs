using System.Drawing;
using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using SuperLinq;
using Svg.Model;
using Svg.Model.Services;
using Svg.Skia;
using Topten.RichTextKit;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Pen(float Stroke, float Spacing) {
    private readonly Option<SKMatrix> inverse = Some(SKMatrix.Identity);

    public static Pen Hit { get; } = new(0f, 0f);

    public SKMatrix Device { get; init { field = value; inverse = value.TryInvert(out SKMatrix inverted) ? Some(inverted) : None; } } = SKMatrix.Identity;

    internal SKPoint Snapped(SKPoint local) => Device.MapPoint(local) switch { var device => Local(new SKPoint(Grid(device.X, 0f), Grid(device.Y, 0f)), local) };

    internal (SKPoint From, SKPoint To) Snapped(SKPoint from, SKPoint to) =>
        (Start: Device.MapPoint(from), End: Device.MapPoint(to), Half: MathF.Round(Device.MapRadius(Stroke), MidpointRounding.ToEven) / 2f) switch {
            var held when held.Start.X.CompareTo(held.End.X) is 0 =>
                (Local(held.Start with { X = Grid(held.Start.X, held.Half) }, from), Local(held.End with { X = Grid(held.End.X, held.Half) }, to)),
            var held when held.Start.Y.CompareTo(held.End.Y) is 0 =>
                (Local(held.Start with { Y = Grid(held.Start.Y, held.Half) }, from), Local(held.End with { Y = Grid(held.End.Y, held.Half) }, to)),
            _ => (from, to),
        };

    private static float Grid(float device, float offset) => MathF.Round(device - offset, MidpointRounding.ToEven) + offset;

    private SKPoint Local(SKPoint device, SKPoint local) => inverse.Map(held => held.MapPoint(device)).IfNone(local);
}

internal sealed record Context(SKMatrix Device, Seq<(Interaction Interaction, float Weight)> Washes) {
    public Option<PlotFrame> Plot { get; init; }

    public Context Keyed => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    public float Scale => Device.MapRadius(1f);

    public Context Within(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    public Pen Pen(RoleStyle style) => new(style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]) { Device = Device };

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    // --- [STATE]
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);
    private readonly SkiaModel model = new(new SKSvgSettings());
    private readonly SkiaSvgAssetLoader loader;
    private readonly Entries<Seq<Key>, (Context Context, float Alpha), SKPicture> subtrees = new(ReferenceEquals, static picture => picture.Dispose());
    private readonly Entries<Seq<Key>, Seq<Filter>, Option<SKImageFilter>> filters = new(Content, Released);
    private readonly Entries<(Seq<Key> Path, Role Role, Shadow Shadow), SKColorF, SKPaint> shadows = new(Content, static paint => paint.Dispose());
    private readonly Entries<(Seq<Key> Path, float Offset), (Corners Corners, float Smoothing, SKRect Frame), Outline> outlines = new(Content, static outline => outline.Dispose());
    private readonly Entries<(Seq<Key> Path, int Fill), (Material Fill, Seq<Option<SKColorF>> Colors, SKRect Frame, SKBlendMode Blend, float Scale, float Clock), Option<SKPaint>> brushes =
        new(Content, Released);
    private readonly Entries<(Seq<Key> Path, int Stroke), (SKColorF Color, float Width, Seq<float> Rhythm, SKBlendMode Blend), SKPaint> edges = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, (SKPoint From, SKPoint To, bool Before, bool After, float Edge), SKPaint> fades = new(Content, static paint => paint.Dispose());
    private readonly Entries<(Seq<Key> Path, Role Role), (float Alpha, Option<SKImageFilter> Filter), SKPaint> veils = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, float, SKPaint> tweens = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, (SKBlendMode Blend, Option<SKColorF> Color), SKPaint> images = new(Content, static paint => paint.Dispose());
    private readonly Entries<Seq<Key>, (SKPicture Art, SKColorF Tint, SKMatrix Place), SKPaint> tints = new(Content, static paint => paint.Dispose());
    private readonly Seq<ISlot> slots;
    private readonly SKImageInfo target;
    private readonly SKSurfaceProperties raster;
    private readonly float density;
    private Option<(Scene Scene, LanguageExt.HashSet<Seq<Key>> Leaving)> retained;
    private HashMap<(Assembly Assembly, string Resource, Option<SKColor> Current), SKPicture> art;

    public PaintCache(SKImageInfo target, SKSurfaceProperties raster, float density = 1f) {
        (this.target, this.raster, this.density) = (target, raster, density);
        loader = new SkiaSvgAssetLoader(model);
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
        subtrees.Kept(laid.Path, laid, (context.Keyed, alpha), () => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(laid.LocalReach, useRTree: true));
            return recorder.EndRecording();
        });

    internal Option<SKImageFilter> Filtered(Laid laid, Seq<Filter> chain) => filters.Kept(laid.Path, laid, chain, () => Filter.Chained(chain));

    internal SKPaint Shadowed(Laid laid, Role role, Shadow shadow, SKColorF color) =>
        shadows.Kept((laid.Path, role, shadow), laid, color, () => {
            using SKImageFilter graph = shadow.Filtered(color);
            return new SKPaint { ImageFilter = graph };
        });

    internal SKPaint Veiled(Laid laid, Role role, float alpha, Option<SKImageFilter> filter) =>
        veils.Kept((laid.Path, role), laid, (alpha, filter), () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha), ImageFilter = filter.ValueUnsafe() });

    internal SKPaint Tweened(Laid laid, float alpha) => tweens.Kept(laid.Path, laid, alpha, () => new SKPaint { ColorF = new SKColorF(0f, 0f, 0f, alpha) });

    // --- [OUTLINE]
    internal Outline Outlined(Laid laid, SKRect frame, float offset) =>
        outlines.Kept((laid.Path, offset), laid, (laid.Corners, laid.Smoothing, frame), () => Outline.Of(laid.Corners, laid.Smoothing, frame, offset));

    // --- [PAINTS]
    internal Option<SKPaint> Brushed(Laid laid, int fill, (Material Fill, Seq<Option<SKColorF>> Colors, SKRect Frame, SKBlendMode Blend, float Scale, float Clock) key, Func<Option<SKPaint>> make) =>
        brushes.Kept((laid.Path, fill), laid, key, make);

    internal SKPaint Edged(Laid laid, int stroke, SKColorF color, float width, Seq<float> rhythm, SKBlendMode blend) =>
        edges.Kept((laid.Path, stroke), laid, (color, width, rhythm, blend), () => {
            using SKPathEffect? dash = rhythm.IsEmpty ? null : SKPathEffect.CreateDash([.. rhythm.Map(beat => beat * width)], 0f);
            return new SKPaint { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = width, StrokeCap = SKStrokeCap.Round, PathEffect = dash, BlendMode = blend };
        });

    internal SKPaint Masked(Laid laid, (SKPoint From, SKPoint To, bool Before, bool After, float Edge) fade) =>
        fades.Kept(laid.Path, laid, fade, () => {
            using SKShader shader = SKShader.CreateLinearGradient(fade.From, fade.To, [fade.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, fade.After ? SKColors.Transparent : SKColors.Black],
                [0f, fade.Edge, 1f - fade.Edge, 1f], SKShaderTileMode.Clamp);
            return new SKPaint { Shader = shader, BlendMode = SKBlendMode.DstIn };
        });

    internal SKPaint Imaged(Laid laid, SKBlendMode blend, Option<SKColorF> color) =>
        images.Kept(laid.Path, laid, (blend, color), () => color.Match(Some: held => new SKPaint { IsAntialias = true, BlendMode = blend, ColorF = held }, None: () => new SKPaint { IsAntialias = true, BlendMode = blend }));

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
    internal void Glyph(SKCanvas canvas, Laid laid, GlyphSource source, Ground under, Pen pen, SKColorF tint, SKRect content) {
        ((Assembly Assembly, string Resource, Option<SKColor> Current) key, bool tinted) = Shown(laid, source, under);
        _ = art.Find(key).Iter(picture => {
            SKRect cull = picture.CullRect;
            SKRect fitted = ImageFit.Contain.Fitted(content, cull.Size);
            SKPoint origin = pen.Snapped(fitted.Location);
            (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
            SKMatrix place = SKMatrix.CreateScaleTranslation(x, y, origin.X - (cull.Left * x), origin.Y - (cull.Top * y));
            if (tinted) canvas.DrawRect(place.MapRect(cull), Tinted(laid, picture, tint, place));
            else canvas.DrawPicture(picture, in place);
        });
    }

    private SKPaint Tinted(Laid laid, SKPicture picture, SKColorF tint, SKMatrix place) =>
        tints.Kept(laid.Path, laid, (picture, tint, place), () => {
            using SKShader drawn = SKShader.CreatePicture(picture, SKShaderTileMode.Decal, SKShaderTileMode.Decal, SKFilterMode.Linear, place, picture.CullRect);
            using SKColorFilter ink = SKColorFilter.CreateColorMatrix([0f, 0f, 0f, 0f, tint.Red, 0f, 0f, 0f, 0f, tint.Green, 0f, 0f, 0f, 0f, tint.Blue, 0f, 0f, 0f, tint.Alpha, 0f]);
            using SKShader tinted = SKShader.CreateColorFilter(drawn, ink);
            return new SKPaint { IsAntialias = true, Shader = tinted };
        });

    private static ((Assembly Assembly, string Resource, Option<SKColor> Current) Key, bool Tinted) Shown(Laid laid, GlyphSource source, Ground under) =>
        source.Shown(under.Under) switch {
            var (assembly, resource, tinted) => ((assembly, resource, tinted ? None : laid.Cascade.Theme.Color(laid.Style.Text, laid.Style)
                .Map(ink => (SKColor)Styles.Legible(laid.Style, under, ink, laid.Cascade.Theme.Contrast.Graphic))), tinted),
        };

    private Fin<SKPicture> Recorded((Assembly Assembly, string Resource, Option<SKColor> Current) glyph) {
        using Stream? stream = glyph.Assembly.GetManifestResourceStream(glyph.Resource);
        return from held in Optional(stream).ToFin(new UIFault(glyph.Resource, UIReason.Missing))
               from document in Optional(SvgService.Open(held, new SvgParameters(Entities: null, Css: null, CurrentColor: glyph.Current.Map(static ink => Color.FromArgb(ink.Alpha, ink.Red, ink.Green, ink.Blue)).ToNullable())))
                   .ToFin(new UIFault(glyph.Resource, UIReason.Unreadable))
               from picture in Optional(SKSvg.ToPicture(document, model, loader)).ToFin(new UIFault(glyph.Resource, UIReason.Unreadable))
               select picture;
    }

    // --- [RETENTION]
    internal Fin<Unit> Retain(Mount mount) {
        Scene scene = mount.Scene;
        LanguageExt.HashSet<Seq<Key>> leaving = toHashSet(mount.Animations.Leaving.Keys);
        if (retained.Exists(held => ReferenceEquals(held.Scene, scene) && held.Leaving == leaving)) return unit;
        HashMap<Seq<Key>, Laid> departed = toHashMap(mount.Animations.Leaving.Values.ToSeq().Bind(static left => Scene.Preorder(left.Last)).Map(static laid => (laid.Path, laid)));
        Seq<(Assembly Assembly, string Resource, Option<SKColor> Current)> shown = (scene.Order + departed.Values.ToSeq())
            .Choose(static laid => laid.Node.Glyphed.Map(glyph => Shown(laid, glyph, Styles.Grounded(laid.Cascade, laid.Style, laid.Node)).Key)).Distinct().Strict();
        LanguageExt.HashSet<(Assembly Assembly, string Resource, Option<SKColor> Current)> kept = toHashSet(shown);
        Seq<((Assembly Assembly, string Resource, Option<SKColor> Current) Key, Fin<SKPicture> Picture)> loaded =
            shown.Filter(key => !art.ContainsKey(key)).Map(key => (Key: key, Picture: Recorded(key))).Strict();
        art = Released(art, (key, _) => kept.Contains(key), static picture => picture.Dispose())
            .AddRange(loaded.Choose(static held => held.Picture.ToOption().Map(picture => (held.Key, picture))));
        return loaded.Traverse(static held => held.Picture).As().Map(_ => {
            retained = (scene, leaving);
            return slots.Iter(slot => slot.Retain(path => scene.Index.Find(path) | departed.Find(path)));
        });
    }

    public void Dispose() {
        _ = slots.Iter(static slot => slot.Clear());
        art = Released(art, static (_, _) => false, static picture => picture.Dispose());
    }

    private static bool Content(Laid held, Laid laid) => true;

    private static void Released<T>(Option<T> held) where T : IDisposable => _ = held.Iter(static value => value.Dispose());

    private static HashMap<TKey, TValue> Released<TKey, TValue>(HashMap<TKey, TValue> held, Func<TKey, TValue, bool> kept, Action<TValue> release) {
        _ = held.Filter((key, value) => !kept(key, value)).Values.Iter(release);
        return held.Filter(kept);
    }

    private interface ISlot {
        public void Retain(Func<Seq<Key>, Option<Laid>> shown);
        public void Clear();
    }

    private sealed class Entries<TSlot, TKey, TValue>(Func<Laid, Laid, bool> reuses, Action<TValue> release) : ISlot where TSlot : notnull {
        private HashMap<TSlot, (Laid Laid, TKey Key, TValue Value)> held;

        public TValue Kept(TSlot slot, Laid laid, TKey key, Func<TValue> make) =>
            held.Find(slot).Filter(found => reuses(found.Laid, laid) && EqualityComparer<TKey>.Default.Equals(found.Key, key)).Match(
                Some: static found => found.Value,
                None: () => {
                    TValue made = make();
                    _ = held.Find(slot).Iter(stale => release(stale.Value));
                    held = held.AddOrUpdate(slot, (laid, key, made));
                    return made;
                });

        public void Retain(Func<Seq<Key>, Option<Laid>> shown) => Pruned(entry => shown(entry.Laid.Path).Exists(held => reuses(entry.Laid, held)));

        public void Clear() => Pruned(static _ => false);

        private void Pruned(Func<(Laid Laid, TKey Key, TValue Value), bool> kept) {
            _ = held.Filter(entry => !kept(entry)).Values.Iter(entry => release(entry.Value));
            held = held.Filter(kept);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Painter {
    // --- [SCENE]
    private sealed record Pass(PaintCache Cache, Scene Scene, ColorScheme Theme, HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes, Animations Animations,
                               LanguageExt.HashSet<Seq<Key>> Live, Duration Now) {
        public float Clock => (float)Now.TotalSeconds;

        public Option<(SKPoint Shift, SKSize Size, float Alpha)> Tweened(Laid laid) => Animations.Transitions.Find(laid.Path).Map(transition => transition.Tweened(laid, Now));
    }

    public static Fin<Unit> Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        return cache.Retain(mount).Map(_ => {
            HashMap<(Seq<Key> Path, Interaction Interaction), float> washes = mount.Washes(now);
            Pass pass = new(cache, scene, scene.Layout.Cascade.Theme, washes, mount.Animations, toHashSet(washes.Keys.ToSeq().Map(static key => key.Path)
                .Concat(scene.Timed.Map(static laid => laid.Path)).Bind(Scene.Above).Concat(mount.Animations.Transitions.Keys.ToSeq().Bind(static path => Scene.Above(path).Tail))), now);
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRegion(damage);
            canvas.Clear(SKColors.Transparent);
            Drawn(canvas, pass, scene.Layout, new Context(canvas.TotalMatrix, []), scene.Layout.Frame.Location, pass.Tweened(scene.Layout));
            _ = mount.Animations.Leaving.Values.Iter(left => {
                using SKAutoCanvasRestore clipped = new(canvas);
                canvas.ClipRect(left.Place.Clip, antialias: true);
                Drawn(canvas, pass, left.Last, new Context(canvas.TotalMatrix, []) { Plot = left.Place.Plot }, left.At, Some((SKPoint.Empty, left.Last.Frame.Size, left.Opacity.Value(now))));
            });
            return washes.Filter(static (key, _) => key.Interaction.Ring).Iter((key, weight) => scene.Ring(key.Path).Iter(ring => Ringed(canvas, pass, ring, weight)));
        });
    }

    private static void Drawn(SKCanvas canvas, Pass pass, Laid laid, Context context, SKPoint at, Option<(SKPoint Shift, SKSize Size, float Alpha)> tween) {
        Context own = context.Within(at) with { Washes = Washed(pass, laid, context) };
        float alpha = tween.IsSome ? 1f : laid.Alpha;
        using SKAutoCanvasRestore restore = new(canvas, doSave: tween.IsSome || pass.Live.Contains(laid.Path));
        _ = tween.Iter(held => {
            canvas.Translate(held.Shift.X, held.Shift.Y);
            if (held.Size != laid.Frame.Size) canvas.ClipRect(SKRect.Create(at, held.Size), antialias: true);
            if (held.Alpha < 1f) _ = canvas.SaveLayer(Scene.Absolute(laid.LocalReach, at), pass.Cache.Tweened(laid, held.Alpha));
        });
        if (!pass.Live.Contains(laid.Path)) {
            canvas.DrawPicture(pass.Cache.Subtree(laid, own, alpha, recording => Self(recording, pass, laid, own, alpha)), at);
        } else if (!canvas.QuickReject(Scene.Absolute(laid.LocalReach, at))) {
            canvas.Translate(at.X, at.Y);
            Self(canvas, pass, laid, own, alpha);
        }
    }

    private static void Self(SKCanvas canvas, Pass pass, Laid laid, Context context, float alpha) {
        SKRect local = SKRect.Create(laid.Frame.Size);
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        Option<SKImageFilter> filter = laid.Node.Role.IsSome && !laid.Style.Filters.IsEmpty ? pass.Cache.Filtered(laid, laid.Style.Filters) : None;
        if (alpha < 1f || filter.IsSome) _ = canvas.SaveLayer(laid.LocalReach, pass.Cache.Veiled(laid, laid.Style.Role, alpha, filter));
        bool filled = laid.Node.Role.IsSome && laid.Node.Painting.AcceptsBackground && Background(canvas, pass, laid, context, local);
        Ground under = Styles.Grounded(laid.Cascade, laid.Style, laid.Node);
        Context inner = context with { Plot = Layouts.Plotted(laid) | context.Plot };
        Seq<(Shadow Shadow, SKColorF Color)> stack = laid.Node.Role.IsSome && !filled ? Shadows(pass.Theme, laid.Style) : [];
        _ = stack.Filter(static held => !held.Shadow.Inside).Iter(held => Layered(canvas, pass.Cache.Shadowed(laid, laid.Style.Role, held.Shadow, held.Color), laid.LocalReach, Inside));
        Inside(canvas);
        _ = stack.Filter(static held => held.Shadow.Inside).Iter(held => Layered(canvas, pass.Cache.Shadowed(laid, laid.Style.Role, held.Shadow, held.Color), laid.LocalReach, Inside));

        void Inside(SKCanvas target) {
            Content(target, pass, laid, context, under, laid.ContentBox);
            if (laid.Children.IsEmpty) return;
            using SKAutoCanvasRestore masked = new(target, doSave: false);
            Option<SKPaint> fade = Faded(pass.Scene, laid, local).Map(held => pass.Cache.Masked(laid, held));
            if (fade.IsSome) _ = target.SaveLayer(local, paint: null);
            _ = toSeq(laid.Children.GroupAdjacent(child => laid.Node.Clips(child.Node))).Iter(run => {
                using SKAutoCanvasRestore clipped = new(target, doSave: run.Key);
                if (run.Key) pass.Cache.Outlined(laid, local, 0f).Clip(target, SKClipOperation.Intersect);
                _ = toSeq(run).Iter(child => Drawn(target, pass, child, inner, child.Frame.Location + pass.Scene.Shift(laid, child), pass.Tweened(child)));
            });
            _ = fade.Iter(mask => target.DrawRect(local, mask));
        }
    }

    private static Seq<(Interaction Interaction, float Weight)> Washed(Pass pass, Laid laid, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.Washes.Find((laid.Path, interaction)).Map(weight => (Interaction: interaction, Weight: weight))) switch {
            var own when own.IsEmpty && laid.Node.Role.IsNone => context.Washes,
            var own => own,
        };

    // --- [SHADOW]
    private static readonly SKPaint Silhouette = new() { IsAntialias = true };

    private static Seq<(Shadow Shadow, SKColorF Color)> Shadows(ColorScheme theme, RoleStyle style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Color, style).Map(color => (Shadow: shadow, Color: color))).Strict();

    private static void Layered(SKCanvas canvas, SKPaint layer, SKRect reach, Action<SKCanvas> draw) {
        _ = canvas.SaveLayer(reach, layer);
        draw(canvas);
        canvas.Restore();
    }

    private static void Outlined(SKCanvas canvas, PaintCache cache, Laid laid, Role role, SKRect frame, SKRect reach, Seq<(Shadow Shadow, SKColorF Color)> stack) =>
        _ = stack.Iter(held => {
            (float outset, Shadow graph) = held.Shadow.Outlined;
            Outline silhouette = cache.Outlined(laid, frame, outset);
            Layered(canvas, cache.Shadowed(laid, role, graph, held.Color), reach, target => silhouette.Draw(target, Silhouette));
        });

    // --- [BACKGROUND]
    private static readonly SKPaint Cleared = new() { IsAntialias = true, BlendMode = SKBlendMode.Clear };

    private static bool Background(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect frame) {
        RoleStyle style = laid.Style;
        Outline shape = pass.Cache.Outlined(laid, frame, 0f);
        Seq<(Material Fill, SKPaint Paint)> fills = style.Fills.Map((fill, index) => Filled(new Filling(pass, laid, context, frame, index, SKBlendMode.SrcOver, None), fill).Map(paint => (Fill: fill, Paint: paint))).Somes().Strict();
        Seq<(Shadow Shadow, SKColorF Color)> stack = fills.IsEmpty ? [] : Shadows(pass.Theme, style);
        using (new SKAutoCanvasRestore(canvas)) {
            shape.Clip(canvas, SKClipOperation.Difference);
            Outlined(canvas, pass.Cache, laid, style.Role, frame, laid.LocalReach, stack.Filter(static held => !held.Shadow.Inside));
        }
        _ = fills.Iter(held => Covered(canvas, held.Fill, frame, clip => shape.Clip(clip, SKClipOperation.Intersect), () => shape.Draw(canvas, held.Paint)));
        Outlined(canvas, pass.Cache, laid, style.Role, frame, laid.LocalReach, stack.Filter(static held => held.Shadow.Inside));
        if (style.Strokes.IsEmpty) return !fills.IsEmpty;
        Ground under = laid.Cascade.Ground;
        bool bounding = laid.Node.Part.IsSome && style.Solid.Bind(value => pass.Theme.Color(value, style)).ForAll(fill => Styles.Ratio(fill, under.Under) < pass.Theme.Contrast.Graphic);
        Outline rim = pass.Cache.Outlined(laid, frame, -style.HalfStroke);
        _ = style.Strokes.Map(static (stroke, index) => (Stroke: stroke, Index: index)).Iter(held => pass.Theme.Washed(held.Stroke.Color, style, context.Washes)
            .Map(color => bounding ? Styles.Legible(style, under, color, pass.Theme.Contrast.Graphic) : color)
            .Iter(color => rim.Draw(canvas, pass.Cache.Edged(laid, held.Index, color, style.Metrics[Metric.Stroke], [], SKBlendMode.SrcOver))));
        return !fills.IsEmpty;
    }

    private static void Covered(SKCanvas canvas, Material material, SKRect bounds, Action<SKCanvas> clip, Action draw) {
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        _ = material.SwitchPartially(@default: static _ => Option<float>.None, blurred: static blurred => Some(blurred.Sigma))
            .Iter(sigma => {
                _ = canvas.Save();
                clip(canvas);
                using SKImageFilter backdrop = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp);
                _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = bounds, Backdrop = backdrop });
            });
        draw();
    }

    private sealed record Filling(Pass Pass, Laid Laid, Context Context, SKRect Frame, int Index, SKBlendMode Blend, Option<Series> Series) {
        public Option<SKColorF> Inked(ColorValue value) => Pass.Theme.Washed(value, Laid.Style, Context.Washes, Laid.Style.Look.Washes, Series);

        public Option<SKPaint> Brushed(Material fill, Seq<Option<SKColorF>> colors, float scale, float clock, Func<Option<SKPaint>> make) =>
            Pass.Cache.Brushed(Laid, Index, (fill, colors, Frame, Blend, scale, clock), make);

        public Option<SKPaint> Colored(Material fill, ColorValue value) =>
            Inked(value) switch { var color => Brushed(fill, [color], 0f, 0f, () => color.Map(ink => new SKPaint { IsAntialias = true, ColorF = ink, BlendMode = Blend })) };

        public SKPaint Shaded(SKShader shader) {
            using SKShader owned = shader;
            return new SKPaint { IsAntialias = true, Shader = owned, BlendMode = Blend };
        }
    }

    private static Option<SKPaint> Filled(Filling filling, Material material) =>
        material.Switch(
            filling,
            solid: static (held, solid) => held.Colored(solid, solid.Color),
            gradient: static (held, gradient) => gradient.Stops.Map(stop => held.Inked(stop.Color)) switch {
                var colors => held.Brushed(gradient, colors, 0f, 0f, () => colors.Traverse(static color => color).As().Map(inks => held.Shaded(gradient.Shader(held.Frame, inks)))),
            },
            drawn: static (held, drawn) => drawn.Uniforms.Map(held.Inked) switch {
                var colors => held.Brushed(drawn, colors.Values.ToSeq(), drawn.Timed ? 0f : held.Context.Scale, drawn.Timed ? held.Pass.Clock : 0f,
                    () => held.Pass.Cache.Drawn(drawn, colors, held.Frame, held.Context.Scale, held.Pass.Clock).Map(held.Shaded)),
            },
            backdrop: static (_, _) => Some(Cleared),
            blurred: static (held, blurred) => held.Colored(blurred, blurred.Tint));

    // --- [RING]
    private static void Ringed(SKCanvas canvas, Pass pass, (Laid Laid, SKRect Frame, SKRect Clip, RoleStyle Style) ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(ring.Clip, antialias: true);
        if (weight < 1f) _ = canvas.SaveLayer(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), pass.Cache.Veiled(ring.Laid, ring.Style.Role, weight, None));
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        SKRect shape = SKRect.Create(ring.Frame.Size);
        SKRect reach = SKRect.Inflate(shape, ring.Style.Overhang, ring.Style.Overhang);
        Seq<(Shadow Shadow, SKColorF Color)> stack = Shadows(pass.Theme, ring.Style);
        using (new SKAutoCanvasRestore(canvas)) {
            pass.Cache.Outlined(ring.Laid, shape, 0f).Clip(canvas, SKClipOperation.Difference);
            Outlined(canvas, pass.Cache, ring.Laid, ring.Style.Role, shape, reach, stack.Filter(static held => !held.Shadow.Inside));
        }
        Outlined(canvas, pass.Cache, ring.Laid, ring.Style.Role, shape, reach, stack.Filter(static held => held.Shadow.Inside));
    }

    // --- [FADE]
    private static Option<(SKPoint From, SKPoint To, bool Before, bool After, float Edge)> Faded(Scene scene, Laid laid, SKRect viewport) =>
        from fade in laid.Node.Role.IsSome ? Some(laid.Style.Metrics[Metric.EdgeFade]) : None
        where fade > 0f
        from scroll in scene.Adjuster(laid.Path)
        let hidden = (Before: scroll.Value > scroll.Min, After: scroll.Value < scroll.Max)
        let length = scroll.Axis.Of(viewport.Size)
        where (hidden.Before || hidden.After) && length > 0f
        select (From: viewport.Location, To: (SKPoint)scroll.Axis.With(viewport.Location, scroll.Axis.Of(viewport.Location) + length), hidden.Before, hidden.After,
            Edge: float.Min(fade, length / 2f) / length);

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, Laid laid, Context context, Ground under, SKRect content) =>
        laid.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Under: under, Content: content),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Under, held.Content),
            text: static (held, text) => Texted(held.Canvas, held.Pass, text, held.Context, held.Under));

    private static void Contained(SKCanvas canvas, Pass pass, Laid.Box laid, Context context, Ground under, SKRect content) =>
        laid.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Laid: laid, Context: context, Under: under, Content: content),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marked(held.Canvas, held.Pass, held.Laid, held.Context, mark, plot)),
            glyph: static (held, glyph) => held.Pass.Theme.Washed(held.Laid.Style.Text, held.Laid.Style, held.Context.Washes)
                .Map(tint => Styles.Legible(held.Laid.Style, held.Under, tint, held.Pass.Theme.Contrast.Graphic))
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, held.Laid, glyph.Art, held.Under, held.Context.Pen(held.Laid.Style), tint, held.Content)),
            image: static (held, image) => Pictured(held.Canvas, held.Pass, held.Laid, held.Context, image, held.Content),
            slot: static (held, _) => held.Canvas.DrawRect(SKRect.Create(held.Laid.Frame.Size), Cleared));

    private static void Texted(SKCanvas canvas, Pass pass, Laid.Text laid, Context context, Ground under) =>
        _ = pass.Theme.Washed(laid.Style.Text, laid.Style, context.Washes).Map(ink => Styles.Legible(laid.Style, under, ink, pass.Theme.Contrast.Text))
            .Iter(ink => Typesetting.Paint(canvas, laid, (SKColor)ink, pass.Cache.Raster));

    private static void Pictured(SKCanvas canvas, Pass pass, Laid laid, Context context, Node.Image image, SKRect content) {
        RoleStyle style = laid.Style;
        SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Width, image.Source.Height));
        SKRect device = context.Device.MapRect(target);
        Option<SKColorF> color = image.Source.IsAlphaOnly ? (style.Strokes.Head.Map(static stroke => stroke.Color) | style.Solid).Bind(value => pass.Theme.Washed(value, style, context.Washes, series: image.Series)) : None;
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.DrawImage(image.Source, target, image.Rendering.Sampling(MathF.Min(device.Width / image.Source.Width, device.Height / image.Source.Height)), pass.Cache.Imaged(laid, style.Blend, color));
    }

    // --- [MARKS]
    private static void Marked(SKCanvas canvas, Pass pass, Laid laid, Context context, Node.Mark mark, PlotFrame frame) {
        RoleStyle style = laid.Style;
        Pen pen = context.Pen(style);
        if (!style.Fills.IsEmpty) {
            using SKPath interior = Figure.Traced(builder => mark.Figure.Interior(builder, frame, pen));
            _ = style.Fills.Map(static (fill, index) => (Fill: fill, Index: index)).Iter(held => Filled(new Filling(pass, laid, context, interior.Bounds, held.Index, style.Blend, mark.Series), held.Fill)
                .Iter(paint => Covered(canvas, held.Fill, interior.Bounds, clip => clip.ClipPath(interior, antialias: true), () => canvas.DrawPath(interior, paint))));
        }
        Seq<(Stroke Stroke, SKColorF Color, int Index)> edges = style.Strokes.Map((stroke, index) => pass.Theme.Washed(stroke.Color, style, context.Washes, series: mark.Series)
            .Map(color => (Stroke: stroke, Color: color, Index: index))).Somes().Strict();
        if (edges.IsEmpty) return;
        using SKPath edge = Figure.Traced(builder => mark.Figure.Edges(builder, frame, pen));
        _ = edges.Iter(held => canvas.DrawPath(edge, pass.Cache.Edged(laid, held.Index, held.Color, pen.Stroke, held.Stroke.Rhythm(mark.Series.Map(static series => series.Ordinal).IfNone(0u)), style.Blend)));
    }
}
