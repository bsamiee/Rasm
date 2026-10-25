using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Svg.Model.Services;
using Svg.Skia;
using Wacton.Unicolour;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Context(Option<PlotFrame> Plot, SKMatrix Device, SKColorF Ground, Seq<(Interaction Interaction, float Weight)> Washes) {
    public Context Keyed => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    public Context Within(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    public Pen Pen(Style style) => new(Device, style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]);

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    // --- [STATE]
    private static readonly Unicolour White = new(ColourSpace.Rgb, 1.0, 1.0, 1.0);
    private static readonly Unicolour Black = new(ColourSpace.Rgb, 0.0, 0.0, 0.0);
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;
    private Option<Scene> retained;
    private HashMap<Seq<Key>, (Laid Laid, Context Key, SKPicture Value)> subtrees;
    private HashMap<Seq<Key>, (Laid Laid, Unit Key, Seq<(SKColor Ink, SKTextBlob Blob)> Value)> blobs;
    private HashMap<(Assembly Assembly, string Resource), SKPicture> art;

    public PaintCache(SKSurfaceProperties raster) {
        model = new SkiaModel(new SKSvgSettings());
        loader = new SkiaSvgAssetLoader(model);
        Raster = new Topten.RichTextKit.TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
    }

    public Topten.RichTextKit.TextPaintOptions Raster { get; }

    // --- [RECORD]
    internal SKPicture Subtree(Laid laid, Context context, Action<SKCanvas> record) =>
        Kept(subtrees, laid, context.Keyed).IfNone(() => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(laid.LocalReach, useRTree: true));
            SKPicture made = recorder.EndRecording();
            subtrees = Stored(subtrees, laid, context.Keyed, made, static picture => picture.Dispose());
            return made;
        });

    internal Seq<(SKColor Ink, SKTextBlob Blob)> Blobs(Laid.Text laid, Typeset.Featured set) =>
        Kept(blobs, laid, unit).IfNone(() => {
            Seq<(SKColor Ink, SKTextBlob Blob)> made = toSeq(set.Runs.GroupBy(static run => run.Style.TextColor)).Choose(runs => Built(runs.Key, toSeq(runs))).Strict();
            blobs = Stored(blobs, laid, unit, made, Released);
            return made;
        });

    private Option<(SKColor Ink, SKTextBlob Blob)> Built(SKColor ink, Seq<Glyphs> runs) {
        using SKTextBlobBuilder builder = new();
        _ = runs.Iter(run => {
            using SKFont font = new(run.Typeface, run.Style.FontSize) { Edging = Raster.Edging, Subpixel = Raster.SubpixelPositioning, Hinting = Raster.Hinting };
            builder.AddPositionedRun(run.Ids.AsSpan(), font, run.Points.AsSpan());
        });
        return Optional(builder.Build()).Map(blob => (Ink: ink, Blob: blob));
    }

    // --- [GLYPH]
    internal void Glyph(SKCanvas canvas, GlyphSource source, SKColorF ground, Pen pen, SKColorF tint, SKRect content) {
        (SKPicture picture, Option<SKColorF> ink) = source.Switch(
            (Cache: this, Lit: ground.ToUnicolour() switch { var under => under.Contrast(White) > under.Contrast(Black) }, Tint: tint),
            tinted: static (held, tinted) => (held.Cache.Recorded(tinted.Assembly, tinted.Resource), Some(held.Tint)),
            paired: static (held, paired) => (held.Cache.Recorded(paired.Assembly, held.Lit ? paired.Dark : paired.Light), Option<SKColorF>.None),
            picture: static (held, picture) => (picture.Art, Some(held.Tint)));
        SKRect cull = picture.CullRect;
        SKRect fitted = ImageFit.Contain.Fitted(content, cull.Size);
        SKPoint origin = pen.Snapped(fitted.Location);
        (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
        SKMatrix place = SKMatrix.CreateScaleTranslation(x, y, origin.X - (cull.Left * x), origin.Y - (cull.Top * y));
        using SKColorFilter? filter = ink.Map<SKColorFilter?>(static color => SKColorFilter.CreateBlendMode((SKColor)color, SKBlendMode.SrcIn)).ValueUnsafe();
        using SKPaint paint = new() { ColorFilter = filter };
        canvas.DrawPicture(picture, in place, paint);
    }

    private SKPicture Recorded(Assembly assembly, string resource) =>
        art.Find((assembly, resource)).IfNone(() => {
            using System.IO.Stream stream = assembly.GetManifestResourceStream(resource) ?? throw new InvalidOperationException($"{assembly.GetName().Name} embeds no {resource}");
            SKPicture made = SKSvg.ToPicture(SvgService.Open(stream) ?? throw new InvalidOperationException($"{resource} in {assembly.GetName().Name} holds no SVG document"), model, loader)
                ?? throw new InvalidOperationException($"{resource} in {assembly.GetName().Name} draws nothing");
            art = art.Add((assembly, resource), made);
            return made;
        });

    private static Seq<(Assembly Assembly, string Resource)> Resources(GlyphSource source) =>
        source.Switch(
            tinted: static tinted => Seq((tinted.Assembly, tinted.Resource)),
            paired: static paired => Seq((paired.Assembly, paired.Light), (paired.Assembly, paired.Dark)),
            picture: static _ => Seq<(Assembly, string)>());

    // --- [RETENTION]
    internal void Retain(Scene scene) {
        if (retained.Exists(held => ReferenceEquals(held, scene))) return;
        retained = scene;
        LanguageExt.HashSet<(Assembly Assembly, string Resource)> shown = toHashSet(scene.Order.Choose(static laid => laid.Node.Glyphed).Bind(Resources));
        subtrees = Released(subtrees, (path, entry) => Current(scene, path, entry.Laid), static entry => entry.Value.Dispose());
        blobs = Released(blobs, (path, entry) => Current(scene, path, entry.Laid), static entry => Released(entry.Value));
        art = Released(art, (key, _) => shown.Contains(key), static picture => picture.Dispose());
    }

    public void Dispose() {
        subtrees = Released(subtrees, static (_, _) => false, static entry => entry.Value.Dispose());
        blobs = Released(blobs, static (_, _) => false, static entry => Released(entry.Value));
        art = Released(art, static (_, _) => false, static picture => picture.Dispose());
    }

    private static bool Current(Scene scene, Seq<Key> path, Laid laid) => scene.Index.Find(path).Exists(shown => ReferenceEquals(shown, laid));

    private static Option<TValue> Kept<TKey, TValue>(HashMap<Seq<Key>, (Laid Laid, TKey Key, TValue Value)> held, Laid laid, TKey key) =>
        held.Find(laid.Path).Filter(found => ReferenceEquals(found.Laid, laid) && EqualityComparer<TKey>.Default.Equals(found.Key, key)).Map(static found => found.Value);

    private static HashMap<Seq<Key>, (Laid Laid, TKey Key, TValue Value)> Stored<TKey, TValue>(
        HashMap<Seq<Key>, (Laid Laid, TKey Key, TValue Value)> held, Laid laid, TKey key, TValue made, Action<TValue> release) {
        _ = held.Find(laid.Path).Iter(stale => release(stale.Value));
        return held.AddOrUpdate(laid.Path, (laid, key, made));
    }

    private static void Released(Seq<(SKColor Ink, SKTextBlob Blob)> built) => _ = built.Iter(static run => run.Blob.Dispose());

    private static HashMap<TKey, TValue> Released<TKey, TValue>(HashMap<TKey, TValue> held, Func<TKey, TValue, bool> kept, Action<TValue> release) {
        _ = held.Filter((key, value) => !kept(key, value)).Values.Iter(release);
        return held.Filter(kept);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Painter {
    // --- [SCENE]
    private sealed record Pass(PaintCache Cache, Scene Scene, Theme Theme, HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes, Option<(Seq<Key> Path, Editor Editor)> Editing, bool Active,
                               LanguageExt.HashSet<Seq<Key>> Live, float Clock);

    public static void Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        Theme theme = scene.Layout.Cascade.Theme;
        HashMap<(Seq<Key> Path, Interaction Interaction), float> washes = mount.Washes(now);
        Option<(Seq<Key> Path, Editor Editor)> editing = from focus in mount.Focused from editor in mount.Editing select (focus, editor);
        cache.Retain(scene);
        Pass pass = new(cache, scene, theme, washes, editing, mount.Host.Visibility == Visibility.Active,
            toHashSet(washes.Keys.ToSeq().Map(static key => key.Path).Concat(editing.ToSeq().Map(static held => held.Path)).Concat(scene.Timed.Map(static laid => laid.Path)).Bind(Scene.Above)),
            (float)now.TotalSeconds);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRegion(damage);
        canvas.Clear(SKColors.Transparent);
        Draw(canvas, pass, scene.Layout, new Context(None, canvas.TotalMatrix, theme.Ground.Fill, []), scene.Layout.Frame.Location);
        _ = washes.Filter(static (key, _) => key.Interaction == Interaction.Focused).Iter((key, weight) => scene.Ring(key.Path).Iter(ring => Ringed(canvas, theme, ring, weight)));
    }

    private static void Draw(SKCanvas canvas, Pass pass, Laid laid, Context context, SKPoint at) {
        Context own = context.Within(at) with { Washes = Washed(pass, laid, context) };
        if (!pass.Live.Contains(laid.Path)) {
            canvas.DrawPicture(pass.Cache.Subtree(laid, own, recording => Self(recording, pass, laid, own)), at);
        } else if (!canvas.QuickReject(laid.LocalReach switch { var local => SKRect.Create(local.Location + at, local.Size) })) {
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.Translate(at.X, at.Y);
            Self(canvas, pass, laid, own);
        }
    }

    private static void Self(SKCanvas canvas, Pass pass, Laid laid, Context context) {
        SKRect local = SKRect.Create(laid.Frame.Size);
        float alpha = (laid.Style.Veiled && !laid.Cascade.Enclosing.Veiled ? pass.Theme.Veil : 1f) * laid.Node.Alpha;
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        if (alpha < 1f) Layer(canvas, laid.LocalReach, alpha);
        bool filled = laid.Node.Role.IsSome && laid.Node.Surfaced && Surface(canvas, pass.Theme, laid.Style, context.Washes, local, pass.Clock);
        if (laid.Node.Role.IsSome && !filled) Cast(canvas, pass.Theme, laid.Style, laid.LocalReach);
        Context grounded = context with { Ground = Ground(pass.Theme, laid).IfNone(context.Ground) };
        Content(canvas, pass, laid, grounded, laid.ContentBox);
        Context inner = grounded with { Plot = Layouts.Plotted(laid) | context.Plot };
        _ = laid.Children.Iter(child => {
            using SKAutoCanvasRestore clipped = new(canvas);
            if (laid.Node.Clips(child.Node)) canvas.ClipRect(local, antialias: true);
            Draw(canvas, pass, child, inner, child.Frame.Location + pass.Scene.Shift(laid, child));
        });
    }

    private static Seq<(Interaction Interaction, float Weight)> Washed(Pass pass, Laid laid, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.Washes.Find((laid.Path, interaction)).Map(weight => (Interaction: interaction, Weight: weight))) switch {
            var own when own.IsEmpty && laid.Node.Role.IsNone => context.Washes,
            var own => own,
        };

    private static void Layer(SKCanvas canvas, SKRect bounds, float alpha) {
        using SKPaint veil = new() { ColorF = new SKColorF(0f, 0f, 0f, alpha) };
        _ = canvas.SaveLayer(bounds, veil);
    }

    private static Option<SKColorF> Ground(Theme theme, Laid laid) => laid.Node.Role.IsSome ? Solid(theme, laid.Style).Filter(static color => color.Alpha >= 1f) : None;

    private static Option<SKColorF> Solid(Theme theme, Style style) => style.Solid.Bind(ink => theme.Color(ink, style, None, None));

    // --- [COLOR]
    private static Option<SKColorF> Inked(Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Ink ink, Option<Series> series) =>
        washes.Fold(theme.Color(ink, style, None, series), (rest, wash) =>
            from start in rest
            select theme.Color(ink, style, Some(wash.Interaction), series)
                .Map(end => start.ToUnicolour().Mix(end.ToUnicolour(), ColourSpace.Oklab, wash.Weight).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF())
                .IfNone(start));

    private static SKPaint Colored(SKColorF color) => new() { IsAntialias = true, ColorF = color };

    // --- [SURFACE]
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    private static bool Surface(SKCanvas canvas, Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, SKRect frame, float clock) {
        using SKRoundRect shape = new(frame, style.Metrics[Metric.Radius], style.Metrics[Metric.Radius]);
        Seq<(Material Fill, SKPaint Paint)> fills = toSeq(style.Fills.Choose(fill => Filled(theme, style, washes, None, frame, fill, clock).Map(paint => (Fill: fill, Paint: paint))).ToArray());
        if (!fills.IsEmpty) Casts(canvas, theme, style, shape);
        _ = fills.Iter(held => Covered(canvas, held.Fill, frame, clip => clip.ClipRoundRect(shape, antialias: true), () => {
            using SKPaint owned = held.Paint;
            canvas.DrawRoundRect(shape, owned);
        }));
        _ = washes.Filter(wash => style.Fills.ForAll(fill => Inks(fill).ForAll(ink => theme.Color(ink, style, Some(wash.Interaction), None).IsNone)))
            .Choose(wash => style.Look.Washes.Find(wash.Interaction).Bind(ink => theme.Color(ink, style, None, None)).Map(color => color.WithAlpha(color.Alpha * wash.Weight)))
            .Iter(color => {
                using SKPaint paint = Colored(color);
                canvas.DrawRoundRect(shape, paint);
            });
        _ = style.Strokes.Iter(stroke => Inked(theme, style, washes, stroke.Ink, None).Iter(color => {
            using SKPaint edge = new() { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = style.Metrics[Metric.Stroke] };
            using SKRoundRect inset = new(shape);
            inset.Deflate(edge.StrokeWidth / 2f, edge.StrokeWidth / 2f);
            canvas.DrawRoundRect(inset, edge);
        }));
        return !fills.IsEmpty;
    }

    private static void Casts(SKCanvas canvas, Theme theme, Style style, SKRoundRect shape) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRoundRect(shape, SKClipOperation.Difference, antialias: true);
        _ = style.Shadows.Iter(shadow => theme.Color(shadow.Ink, style, None, None).Iter(color => {
            using SKMaskFilter? blur = shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadow.Blur) : null;
            using SKPaint cast = new() { IsAntialias = true, ColorF = color, MaskFilter = blur };
            using SKRoundRect spread = new(shape);
            spread.Inflate(shadow.Spread, shadow.Spread);
            spread.Offset(shadow.X, shadow.Y);
            canvas.DrawRoundRect(spread, cast);
        }));
    }

    private static void Cast(SKCanvas canvas, Theme theme, Style style, SKRect bounds) {
        Seq<SKImageFilter> casts = toSeq(style.Shadows.Choose(shadow => theme.Color(shadow.Ink, style, None, None).Map(color => Cast(shadow, (SKColor)color))).ToArray());
        if (casts.IsEmpty) return;
        using SKImageFilter source = SKImageFilter.CreateOffset(0f, 0f);
        using SKImageFilter merged = SKImageFilter.CreateMerge([.. casts, source]);
        using SKPaint layer = new() { ImageFilter = merged };
        _ = casts.Iter(static cast => cast.Dispose());
        _ = canvas.SaveLayer(bounds, layer);
    }

    private static SKImageFilter Cast(Shadow shadow, SKColor color) {
        using SKImageFilter? grown = shadow.Spread > 0f ? SKImageFilter.CreateDilate(shadow.Spread, shadow.Spread) : null;
        return SKImageFilter.CreateDropShadowOnly(shadow.X, shadow.Y, shadow.Blur, shadow.Blur, color, grown);
    }

    private static void Covered(SKCanvas canvas, Material material, SKRect bounds, Action<SKCanvas> clip, Action draw) {
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        _ = material.Switch(
                solid: static _ => None, gradient: static _ => None, drawn: static _ => None, backdrop: static _ => None, blurred: static blurred => Some(blurred.Sigma))
            .Iter(sigma => {
                _ = canvas.Save();
                clip(canvas);
                using SKImageFilter backdrop = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp);
                _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = bounds, Backdrop = backdrop });
            });
        draw();
    }

    private static Option<SKPaint> Filled(Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Option<Series> series, SKRect frame, Material material, float clock) =>
        material.Switch(
            (Theme: theme, Style: style, Washes: washes, Series: series, Frame: frame, Clock: clock),
            solid: static (held, solid) => Inked(held.Theme, held.Style, held.Washes, solid.Ink, held.Series).Map(Colored),
            gradient: static (held, gradient) => gradient.Stops.Traverse(stop => Inked(held.Theme, held.Style, held.Washes, stop.Ink, held.Series)).As()
                .Map(colors => Shaded(Gradient(held.Frame, gradient, colors))),
            drawn: static (held, drawn) => Some(Shaded(Drawn(held.Theme, held.Style, held.Washes, held.Frame, drawn, held.Clock))),
            backdrop: static (_, _) => Some(new SKPaint { IsAntialias = true, BlendMode = SKBlendMode.Clear }),
            blurred: static (held, blurred) => Inked(held.Theme, held.Style, held.Washes, blurred.Tint, held.Series).Map(Colored));

    private static Seq<Ink> Inks(Material material) =>
        material.Switch(
            solid: static solid => Seq(solid.Ink),
            gradient: static gradient => gradient.Stops.Map(static stop => stop.Ink),
            drawn: static drawn => toSeq(drawn.Inks.Values),
            backdrop: static _ => Seq<Ink>(),
            blurred: static blurred => Seq(blurred.Tint));

    private static SKPaint Shaded(SKShader shader) {
        using SKShader owned = shader;
        return new SKPaint { IsAntialias = true, Shader = owned };
    }

    private static SKShader Gradient(SKRect frame, Material.Gradient gradient, Seq<SKColorF> colors) {
        (float sin, float cos) = MathF.SinCos(gradient.Angle * MathF.PI / 180f);
        float reach = (MathF.Abs(cos * frame.Width) + MathF.Abs(sin * frame.Height)) / 2f;
        SKPoint center = new(frame.MidX, frame.MidY);
        return SKShader.CreateLinearGradient(
            new SKPoint(center.X - (cos * reach), center.Y - (sin * reach)),
            new SKPoint(center.X + (cos * reach), center.Y + (sin * reach)),
            [.. colors],
            Srgb,
            [.. gradient.Stops.Map(static stop => stop.At)],
            SKShaderTileMode.Clamp);
    }

    private static SKShader Drawn(Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, SKRect frame, Material.Drawn drawn, float clock) {
        using SKRuntimeEffectUniforms uniforms = new(drawn.Effect);
        if (uniforms.Contains(Material.Drawn.Size)) uniforms[Material.Drawn.Size] = frame.Size;
        if (uniforms.Contains(Material.Drawn.Clock)) uniforms[Material.Drawn.Clock] = clock;
        _ = drawn.Inks.Filter((name, _) => uniforms.Contains(name)).Iter((name, ink) => Inked(theme, style, washes, ink, None).Iter(color => uniforms[name] = color));
        using SKShader shader = drawn.Effect.ToShader(uniforms);
        return shader.WithLocalMatrix(SKMatrix.CreateTranslation(frame.Left, frame.Top));
    }

    // --- [RING]
    private static void Ringed(SKCanvas canvas, Theme theme, (Laid Laid, SKRect Frame, SKRect Clip, Style Style) ring, float weight) {
        float radius = ring.Laid.Node.Role.IsSome ? ring.Laid.Style.Metrics[Metric.Radius] : 0f;
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(ring.Clip, antialias: true);
        if (weight < 1f) Layer(canvas, SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), weight);
        using SKRoundRect shape = new(ring.Frame, radius, radius);
        Casts(canvas, theme, ring.Style, shape);
    }

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect content) =>
        laid.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Content: content),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Content),
            text: static (held, text) => Texted(held.Canvas, held.Pass, text, held.Context, held.Content));

    private static void Contained(SKCanvas canvas, Pass pass, Laid.Box laid, Context context, SKRect content) =>
        laid.Node.Switch(
            (Canvas: canvas, Pass: pass, Laid: laid, Context: context, Content: content),
            box: static (_, _) => { },
            flex: static (_, _) => { },
            grid: static (_, _) => { },
            lazy: static (_, _) => { },
            overlay: static (_, _) => { },
            choice: static (_, _) => { },
            scroll: static (_, _) => { },
            split: static (_, _) => { },
            plot: static (_, _) => { },
            pin: static (_, _) => { },
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marked(held.Canvas, held.Pass.Theme, held.Laid.Style, held.Context.Washes, mark, held.Context.Pen(held.Laid.Style), plot, held.Pass.Clock)),
            text: static (_, _) => { },
            glyph: static (held, glyph) => Inked(held.Pass.Theme, held.Laid.Style, held.Context.Washes, held.Laid.Style.Text, None)
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, glyph.Art, held.Context.Ground, held.Context.Pen(held.Laid.Style), tint, held.Content)),
            image: static (held, image) => Pictured(held.Canvas, held.Pass.Theme, held.Laid.Style, held.Context.Washes, image, held.Content),
            slot: static (held, _) => {
                using SKPaint clear = new() { BlendMode = SKBlendMode.Clear };
                held.Canvas.DrawRect(SKRect.Create(held.Laid.Frame.Size), clear);
            },
            styled: static (_, _) => { });

    private static void Texted(SKCanvas canvas, Pass pass, Laid.Text laid, Context context, SKRect content) =>
        _ = Inked(pass.Theme, laid.Style, context.Washes, laid.Style.Text, None).Iter(ink => pass.Editing.Filter(held => held.Path == laid.Path).Match(
            Some: held => held.Editor.Paint(canvas, laid, content, pass.Cache.Raster,
                (SKColor)Solid(pass.Theme, Styles.Resolve(laid.Cascade, Role.Selection, laid.Style.Conditions)).IfNone(SKColorF.Empty), (SKColor)ink, pass.Active ? laid.Style.Metrics[Metric.Stroke] : 0f),
            None: () => Typesetting.Paint(canvas, pass.Cache, laid, (SKColor)ink)));

    private static void Pictured(SKCanvas canvas, Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Node.Image image, SKRect content) {
        SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Width, image.Source.Height));
        using SKPaint paint = new() { IsAntialias = true, BlendMode = style.Blend };
        _ = (image.Source.IsAlphaOnly ? (style.Strokes.Head.Map(static stroke => stroke.Ink) | style.Solid).Bind(ink => Inked(theme, style, washes, ink, image.Series)) : None)
            .Iter(color => paint.ColorF = color);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.DrawImage(image.Source, target, image.Sampling, paint);
    }

    // --- [MARKS]
    private static void Marked(SKCanvas canvas, Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Node.Mark mark, Pen pen, PlotFrame frame, float clock) {
        if (!style.Fills.IsEmpty) {
            using SKPath interior = Figure.Traced(builder => mark.Figure.Interior(builder, frame, pen));
            _ = style.Fills.Iter(fill => Filled(theme, style, washes, mark.Series, interior.Bounds, fill, clock).Iter(paint => Covered(canvas, fill, interior.Bounds, held => held.ClipPath(interior, antialias: true), () => {
                using SKPaint owned = paint;
                owned.BlendMode = style.Blend;
                canvas.DrawPath(interior, owned);
            })));
        }
        Seq<(Stroke Stroke, SKColorF Color)> edges = style.Strokes.Choose(stroke => Inked(theme, style, washes, stroke.Ink, mark.Series).Map(color => (Stroke: stroke, Color: color)));
        if (!edges.IsEmpty) {
            using SKPath edge = Figure.Traced(builder => mark.Figure.Edges(builder, frame, pen));
            _ = edges.Iter(held => {
                Seq<float> rhythm = held.Stroke.Rhythm(mark.Series.Map(static series => series.Ordinal).IfNone(0u));
                using SKPathEffect? dash = rhythm.IsEmpty ? null : SKPathEffect.CreateDash([.. rhythm.Map(beat => beat * pen.Stroke)], 0f);
                using SKPaint paint = new() {
                    IsAntialias = true, ColorF = held.Color, Style = SKPaintStyle.Stroke, StrokeWidth = pen.Stroke, StrokeCap = SKStrokeCap.Round, PathEffect = dash, BlendMode = style.Blend,
                };
                canvas.DrawPath(edge, paint);
            });
        }
    }
}
