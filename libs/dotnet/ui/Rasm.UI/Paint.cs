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

    public float Scale => Device.MapRadius(1f);

    public Context Within(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    public Pen Pen(Style style) => new(Device, style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]);

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

internal sealed record PlacedImage(SKImage Image, SKRect Source, SKMatrix Local) {
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);

    public static Option<PlacedImage> Recorded(SKRect local, SKMatrix device, Action<SKCanvas> record) {
        using SKPictureRecorder recorder = new();
        record(recorder.BeginRecording(local));
        using SKPicture drawn = recorder.EndRecording();
        SKRectI pixels = SKRectI.Ceiling(device.MapRect(local), outwards: true);
        return Optional(SKImage.FromPicture(drawn, pixels.Size, device.PostConcat(SKMatrix.CreateTranslation(-pixels.Left, -pixels.Top))))
            .Map(image => new PlacedImage(image, SKRect.Create(pixels.Width, pixels.Height), device.Invert().PreConcat(SKMatrix.CreateTranslation(pixels.Left, pixels.Top))));
    }

    public Option<PlacedImage> Shadowed(Seq<(Shadow Shadow, SKColorF Color)> stack, float scale) {
        Seq<SKImageFilter> drops = stack.Map(cast => Dropped(cast.Shadow, (SKColor)cast.Color, scale)).Strict();
        using SKImageFilter merged = SKImageFilter.CreateMerge([.. drops]);
        _ = drops.Iter(static drop => drop.Dispose());
        SKRectI whole = SKRectI.Create(Image.Width, Image.Height);
        return Optional(Image.ApplyImageFilter(merged, whole, whole, out SKRectI subset, out SKPointI offset))
            .Map(filtered => new PlacedImage(filtered, subset, Local.PreConcat(SKMatrix.CreateTranslation(offset.X - subset.Left, offset.Y - subset.Top))));
    }

    public void Draw(SKCanvas canvas) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.Concat(Local);
        canvas.DrawImage(Image, Source, Source, Sampling);
    }

    public SKShader Shader(SKPoint at) => Image.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, Sampling, SKMatrix.CreateTranslation(at.X, at.Y).PreConcat(Local));

    private static SKImageFilter Dropped(Shadow shadow, SKColor color, float scale) {
        using SKImageFilter? grown = shadow.Spread > 0f ? SKImageFilter.CreateDilate(shadow.Spread * scale, shadow.Spread * scale) : null;
        return SKImageFilter.CreateDropShadowOnly(shadow.X * scale, shadow.Y * scale, shadow.Blur * scale, shadow.Blur * scale, color, grown);
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    // --- [STATE]
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;
    private readonly Entries<Seq<Key>, Context, SKPicture> subtrees = new(ReferenceEquals, static picture => picture.Dispose());
    private readonly Entries<Seq<Key>, Unit, Seq<(SKColor Ink, SKTextBlob Blob)>> blobs = new(ReferenceEquals, Released);
    private readonly Entries<(Seq<Key> Path, Role Role), (SKRect Shape, float Scale, float Radius, Seq<(Shadow Shadow, SKColorF Color)> Stack), Option<PlacedImage>> casts = new(Content, Released);
    private readonly Entries<(Seq<Key> Path, SKRuntimeEffect Effect), (HashMap<string, Option<SKColorF>> Inks, SKSize Size, float Scale), Option<PlacedImage>> fills = new(Content, Released);
    private Option<Scene> retained;
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
        subtrees.Kept(laid.Path, laid, context.Keyed, () => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(laid.LocalReach, useRTree: true));
            return recorder.EndRecording();
        });

    internal Seq<(SKColor Ink, SKTextBlob Blob)> Blobs(Laid.Text laid, Typeset.Featured set) =>
        blobs.Kept(laid.Path, laid, unit, () => toSeq(set.Runs.GroupBy(static run => run.Style.TextColor)).Choose(runs => Built(runs.Key, toSeq(runs))).Strict());

    private Option<(SKColor Ink, SKTextBlob Blob)> Built(SKColor ink, Seq<Glyphs> runs) {
        using SKTextBlobBuilder builder = new();
        _ = runs.Iter(run => {
            using SKFont font = new(run.Typeface, run.Style.FontSize) { Edging = Raster.Edging, Subpixel = Raster.SubpixelPositioning, Hinting = Raster.Hinting };
            builder.AddPositionedRun(run.Ids.AsSpan(), font, run.Points.AsSpan());
        });
        return Optional(builder.Build()).Map(blob => (Ink: ink, Blob: blob));
    }

    // --- [RASTER]
    internal void Casts(SKCanvas canvas, Laid laid, Style style, Context context, SKRect shape, float radius, Seq<(Shadow Shadow, SKColorF Color)> stack) =>
        _ = casts.Kept((laid.Path, style.Role), laid, (context.Keyed.Device.MapRect(shape), context.Scale, radius, stack), () =>
            PlacedImage.Recorded(SKRect.Inflate(shape, style.Overhang, style.Overhang), context.Keyed.Device, recording => {
                using SKRoundRect outline = new(shape, radius, radius);
                recording.ClipRoundRect(outline, SKClipOperation.Difference, antialias: true);
                _ = stack.Iter(cast => {
                    using SKMaskFilter? blur = cast.Shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, cast.Shadow.Blur) : null;
                    using SKPaint paint = new() { IsAntialias = true, ColorF = cast.Color, MaskFilter = blur };
                    using SKRoundRect spread = new(outline);
                    spread.Inflate(cast.Shadow.Spread, cast.Shadow.Spread);
                    spread.Offset(cast.Shadow.X, cast.Shadow.Y);
                    recording.DrawRoundRect(spread, paint);
                });
            })).Iter(raster => raster.Draw(canvas));

    internal Option<SKShader> Drawn(Laid laid, Material.Drawn drawn, HashMap<string, Option<SKColorF>> inks, SKRect frame, Context context, float clock) =>
        drawn.Timed
            ? Some(Shader(drawn, inks, frame, clock))
            : fills.Kept((laid.Path, drawn.Effect), laid, (inks, frame.Size, context.Scale), () =>
                PlacedImage.Recorded(SKRect.Create(frame.Size), SKMatrix.CreateScale(context.Scale, context.Scale), recording => {
                    using SKShader shader = Shader(drawn, inks, SKRect.Create(frame.Size), clock);
                    using SKPaint paint = new() { Shader = shader };
                    recording.DrawRect(SKRect.Create(frame.Size), paint);
                })).Map(raster => raster.Shader(frame.Location));

    private static SKShader Shader(Material.Drawn drawn, HashMap<string, Option<SKColorF>> inks, SKRect frame, float clock) {
        using SKRuntimeEffectUniforms uniforms = new(drawn.Effect);
        if (uniforms.Contains(Material.Drawn.Size)) uniforms[Material.Drawn.Size] = frame.Size;
        if (uniforms.Contains(Material.Drawn.Clock)) uniforms[Material.Drawn.Clock] = clock;
        _ = inks.Filter((name, _) => uniforms.Contains(name)).Iter((name, color) => color.Iter(held => uniforms[name] = held));
        using SKShader shader = drawn.Effect.ToShader(uniforms);
        return shader.WithLocalMatrix(SKMatrix.CreateTranslation(frame.Left, frame.Top));
    }

    // --- [GLYPH]
    internal void Glyph(SKCanvas canvas, GlyphSource source, SKColorF ground, Pen pen, SKColorF tint, SKRect content) {
        (SKPicture picture, bool tinted) = source.Shown(ground, Recorded);
        SKRect cull = picture.CullRect;
        SKRect fitted = ImageFit.Contain.Fitted(content, cull.Size);
        SKPoint origin = pen.Snapped(fitted.Location);
        (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
        SKMatrix place = SKMatrix.CreateScaleTranslation(x, y, origin.X - (cull.Left * x), origin.Y - (cull.Top * y));
        using SKColorFilter? filter = tinted ? SKColorFilter.CreateBlendMode((SKColor)tint, SKBlendMode.SrcIn) : null;
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

    // --- [RETENTION]
    internal void Retain(Scene scene) {
        if (retained.Exists(held => ReferenceEquals(held, scene))) return;
        retained = scene;
        LanguageExt.HashSet<(Assembly Assembly, string Resource)> shown = toHashSet(scene.Order.Choose(static laid => laid.Node.Glyphed).Bind(static glyph => glyph.Resources));
        subtrees.Retain(scene);
        blobs.Retain(scene);
        casts.Retain(scene);
        fills.Retain(scene);
        art = Released(art, (key, _) => shown.Contains(key), static picture => picture.Dispose());
    }

    public void Dispose() {
        subtrees.Clear();
        blobs.Clear();
        casts.Clear();
        fills.Clear();
        art = Released(art, static (_, _) => false, static picture => picture.Dispose());
    }

    private static bool Content(Laid held, Laid laid) => true;

    private static void Released(Seq<(SKColor Ink, SKTextBlob Blob)> built) => _ = built.Iter(static run => run.Blob.Dispose());

    private static void Released(Option<PlacedImage> placed) => _ = placed.Iter(static held => held.Image.Dispose());

    private static HashMap<TKey, TValue> Released<TKey, TValue>(HashMap<TKey, TValue> held, Func<TKey, TValue, bool> kept, Action<TValue> release) {
        _ = held.Filter((key, value) => !kept(key, value)).Values.Iter(release);
        return held.Filter(kept);
    }

    private sealed class Entries<TSlot, TKey, TValue>(Func<Laid, Laid, bool> reuses, Action<TValue> release) where TSlot : notnull {
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

        public void Retain(Scene scene) => Swept(entry => scene.Index.Find(entry.Laid.Path).Exists(shown => reuses(entry.Laid, shown)));

        public void Clear() => Swept(static _ => false);

        private void Swept(Func<(Laid Laid, TKey Key, TValue Value), bool> kept) {
            _ = held.Filter(entry => !kept(entry)).Values.Iter(entry => release(entry.Value));
            held = held.Filter(kept);
        }
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
        Context root = new(None, canvas.TotalMatrix, theme.Ground.Fill, []);
        Draw(canvas, pass, scene.Layout, root, scene.Layout.Frame.Location);
        _ = washes.Filter(static (key, _) => key.Interaction == Interaction.Focused).Iter((key, weight) => scene.Ring(key.Path).Iter(ring => Ringed(canvas, pass, root, ring, weight)));
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
        bool filled = laid.Node.Role.IsSome && laid.Node.Surfaced && Surface(canvas, pass, laid, context, local);
        Context grounded = context with { Ground = Ground(pass.Theme, laid).IfNone(context.Ground) };
        Context inner = grounded with { Plot = Layouts.Plotted(laid) | context.Plot };
        if (laid.Node.Role.IsSome && !filled) _ = Shadows(pass.Theme, laid.Style).Iter(stack => Cast(canvas, laid, context, stack, Inside));
        Inside(canvas);

        void Inside(SKCanvas target) {
            Content(target, pass, laid, grounded, laid.ContentBox);
            _ = laid.Children.Iter(child => {
                using SKAutoCanvasRestore clipped = new(target);
                if (laid.Node.Clips(child.Node)) target.ClipRect(local, antialias: true);
                Draw(target, pass, child, inner, child.Frame.Location + pass.Scene.Shift(laid, child));
            });
        }
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

    private static void Cast(SKCanvas canvas, Laid laid, Context context, Seq<(Shadow Shadow, SKColorF Color)> stack, Action<SKCanvas> content) =>
        _ = PlacedImage.Recorded(laid.LocalReach, context.Keyed.Device, content).Iter(source => {
            using SKImage drawn = source.Image;
            _ = source.Shadowed(stack, context.Scale).Iter(cast => {
                using SKImage shown = cast.Image;
                cast.Draw(canvas);
            });
        });

    private static Option<Seq<(Shadow Shadow, SKColorF Color)>> Shadows(Theme theme, Style style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Ink, style, None, None).Map(color => (Shadow: shadow, Color: color))).Strict() switch {
            { IsEmpty: true } => None,
            var stack => Some(stack),
        };

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

    private static bool Surface(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect frame) {
        Style style = laid.Style;
        float radius = style.Metrics[Metric.Radius];
        using SKRoundRect shape = new(frame, radius, radius);
        Seq<(Material Fill, SKPaint Paint)> fills = toSeq(style.Fills.Choose(fill => Filled(pass, laid, context, None, frame, fill).Map(paint => (Fill: fill, Paint: paint))).ToArray());
        if (!fills.IsEmpty) _ = Shadows(pass.Theme, style).Iter(stack => pass.Cache.Casts(canvas, laid, style, context, frame, radius, stack));
        _ = fills.Iter(held => Covered(canvas, held.Fill, frame, clip => clip.ClipRoundRect(shape, antialias: true), () => {
            using SKPaint owned = held.Paint;
            canvas.DrawRoundRect(shape, owned);
        }));
        _ = context.Washes.Filter(wash => style.Fills.ForAll(fill => Inks(fill).ForAll(ink => pass.Theme.Color(ink, style, Some(wash.Interaction), None).IsNone)))
            .Choose(wash => style.Look.Washes.Find(wash.Interaction).Bind(ink => pass.Theme.Color(ink, style, None, None)).Map(color => color.WithAlpha(color.Alpha * wash.Weight)))
            .Iter(color => {
                using SKPaint paint = Colored(color);
                canvas.DrawRoundRect(shape, paint);
            });
        _ = style.Strokes.Iter(stroke => Inked(pass.Theme, style, context.Washes, stroke.Ink, None).Iter(color => {
            using SKPaint edge = new() { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = style.Metrics[Metric.Stroke] };
            using SKRoundRect inset = new(shape);
            inset.Deflate(edge.StrokeWidth / 2f, edge.StrokeWidth / 2f);
            canvas.DrawRoundRect(inset, edge);
        }));
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

    private static Option<SKPaint> Filled(Pass pass, Laid laid, Context context, Option<Series> series, SKRect frame, Material material) =>
        material.Switch(
            (Pass: pass, Laid: laid, laid.Style, Context: context, Series: series, Frame: frame),
            solid: static (held, solid) => Inked(held.Pass.Theme, held.Style, held.Context.Washes, solid.Ink, held.Series).Map(Colored),
            gradient: static (held, gradient) => gradient.Stops.Traverse(stop => Inked(held.Pass.Theme, held.Style, held.Context.Washes, stop.Ink, held.Series)).As()
                .Map(colors => Shaded(Gradient(held.Frame, gradient, colors))),
            drawn: static (held, drawn) => held.Pass.Cache.Drawn(
                    held.Laid, drawn, drawn.Inks.Map(ink => Inked(held.Pass.Theme, held.Style, held.Context.Washes, ink, None)), held.Frame, held.Context, held.Pass.Clock)
                .Map(Shaded),
            backdrop: static (_, _) => Some(new SKPaint { IsAntialias = true, BlendMode = SKBlendMode.Clear }),
            blurred: static (held, blurred) => Inked(held.Pass.Theme, held.Style, held.Context.Washes, blurred.Tint, held.Series).Map(Colored));

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

    // --- [RING]
    private static void Ringed(SKCanvas canvas, Pass pass, Context root, (Laid Laid, SKRect Frame, SKRect Clip, Style Style) ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(ring.Clip, antialias: true);
        if (weight < 1f) Layer(canvas, SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), weight);
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        _ = Shadows(pass.Theme, ring.Style).Iter(stack => pass.Cache.Casts(
            canvas, ring.Laid, ring.Style, root.Within(ring.Frame.Location), SKRect.Create(ring.Frame.Size),
            ring.Laid.Node.Role.IsSome ? ring.Laid.Style.Metrics[Metric.Radius] : 0f, stack));
    }

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect content) =>
        laid.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Content: content),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Content),
            text: static (held, text) => Texted(held.Canvas, held.Pass, text, held.Context, held.Content));

    private static void Contained(SKCanvas canvas, Pass pass, Laid.Box laid, Context context, SKRect content) =>
        laid.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Laid: laid, Context: context, Content: content),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marked(held.Canvas, held.Pass, held.Laid, held.Context, mark, plot)),
            glyph: static (held, glyph) => Inked(held.Pass.Theme, held.Laid.Style, held.Context.Washes, held.Laid.Style.Text, None)
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, glyph.Art, held.Context.Ground, held.Context.Pen(held.Laid.Style), tint, held.Content)),
            image: static (held, image) => Pictured(held.Canvas, held.Pass.Theme, held.Laid.Style, held.Context, image, held.Content),
            slot: static (held, _) => {
                using SKPaint clear = new() { BlendMode = SKBlendMode.Clear };
                held.Canvas.DrawRect(SKRect.Create(held.Laid.Frame.Size), clear);
            });

    private static void Texted(SKCanvas canvas, Pass pass, Laid.Text laid, Context context, SKRect content) =>
        _ = Inked(pass.Theme, laid.Style, context.Washes, laid.Style.Text, None).Iter(ink => pass.Editing.Filter(held => held.Path == laid.Path).Match(
            Some: held => held.Editor.Paint(canvas, laid, content, pass.Cache.Raster,
                (SKColor)Solid(pass.Theme, Styles.Resolve(laid.Cascade, Role.Selection, laid.Style.Conditions)).IfNone(SKColorF.Empty), (SKColor)ink, pass.Active ? laid.Style.Metrics[Metric.Stroke] : 0f),
            None: () => Typesetting.Paint(canvas, pass.Cache, laid, (SKColor)ink)));

    private static void Pictured(SKCanvas canvas, Theme theme, Style style, Context context, Node.Image image, SKRect content) {
        SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Width, image.Source.Height));
        SKRect device = context.Device.MapRect(target);
        using SKPaint paint = new() { IsAntialias = true, BlendMode = style.Blend };
        _ = (image.Source.IsAlphaOnly ? (style.Strokes.Head.Map(static stroke => stroke.Ink) | style.Solid).Bind(ink => Inked(theme, style, context.Washes, ink, image.Series)) : None)
            .Iter(color => paint.ColorF = color);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.DrawImage(image.Source, target, image.Rendering.Sampling(MathF.Min(device.Width / image.Source.Width, device.Height / image.Source.Height)), paint);
    }

    // --- [MARKS]
    private static void Marked(SKCanvas canvas, Pass pass, Laid laid, Context context, Node.Mark mark, PlotFrame frame) {
        Style style = laid.Style;
        Pen pen = context.Pen(style);
        if (!style.Fills.IsEmpty) {
            using SKPath interior = Figure.Traced(builder => mark.Figure.Interior(builder, frame, pen));
            _ = style.Fills.Iter(fill => Filled(pass, laid, context, mark.Series, interior.Bounds, fill).Iter(paint => Covered(canvas, fill, interior.Bounds, held => held.ClipPath(interior, antialias: true), () => {
                using SKPaint owned = paint;
                owned.BlendMode = style.Blend;
                canvas.DrawPath(interior, owned);
            })));
        }
        Seq<(Stroke Stroke, SKColorF Color)> edges = style.Strokes.Choose(stroke => Inked(pass.Theme, style, context.Washes, stroke.Ink, mark.Series).Map(color => (Stroke: stroke, Color: color)));
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
