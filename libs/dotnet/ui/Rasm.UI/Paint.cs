using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Svg.Model.Services;
using Svg.Skia;
using Topten.RichTextKit;
using Wacton.Unicolour;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Context(Option<PlotFrame> Plot, SKMatrix Device, Seq<(Interaction Interaction, float Weight)> Washes) {
    public Context Keyed => this with { Device = Device with { TransX = Fractional(Device.TransX), TransY = Fractional(Device.TransY) } };

    public float Scale => Device.MapRadius(1f);

    public Context Within(SKPoint at) => this with { Device = Device.PreConcat(SKMatrix.CreateTranslation(at.X, at.Y)), Plot = Plot.Map(plot => plot.Translated(new SKPoint(-at.X, -at.Y))) };

    public Pen Pen(RoleStyle style) => new(Device, style.Metrics[Metric.Stroke], style.Metrics[Metric.Spacing]);

    private static float Fractional(float offset) => offset - MathF.Floor(offset);
}

internal sealed record PlacedImage(SKImage Image, SKRect Source, SKMatrix Local) {
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);

    public Option<PlacedImage> Shadowed(Seq<(Shadow Shadow, SKColorF Color)> stack) {
        if (stack.IsEmpty) return None;
        float scale = 1f / Local.MapRadius(1f);
        Seq<SKImageFilter> casts = stack.Map(cast => cast.Shadow.Filtered(cast.Color, scale)).Strict();
        using SKImageFilter merged = SKImageFilter.CreateMerge([.. casts]);
        _ = casts.Iter(static cast => cast.Dispose());
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
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    // --- [STATE]
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;
    private readonly Entries<Seq<Key>, Context, SKPicture> subtrees = new(ReferenceEquals, static picture => picture.Dispose());
    private readonly Entries<Seq<Key>, Unit, Seq<(IStyle Style, SKTextBlob Blob)>> blobs = new(ReferenceEquals, Released);
    private readonly Entries<Seq<Key>, Seq<Filter>, Option<SKImageFilter>> filters = new(Content, Released);
    private readonly Entries<(Seq<Key> Path, Role Role, SKClipOperation Clip), (SKRect Shape, float Scale, Corners Corners, float Smoothing, Seq<(Shadow Shadow, SKColorF Color)> Stack), Option<PlacedImage>> casts =
        new(Content, Released);
    private readonly Entries<(Seq<Key> Path, SKRuntimeEffect Effect), (HashMap<string, Option<SKColorF>> Colors, SKSize Size, float Scale), Option<PlacedImage>> fills = new(Content, Released);
    private readonly SKImageInfo target;
    private readonly SKSurfaceProperties raster;
    private readonly float density;
    private Option<Scene> retained;
    private HashMap<(Assembly Assembly, string Resource), SKPicture> art;

    public PaintCache(SKImageInfo target, SKSurfaceProperties raster, float density = 1f) {
        (this.target, this.raster, this.density) = (target, raster, density);
        model = new SkiaModel(new SKSvgSettings());
        loader = new SkiaSvgAssetLoader(model);
        Raster = new TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
    }

    public TextPaintOptions Raster { get; }

    // --- [RECORD]
    internal SKPicture Subtree(Laid laid, Context context, Action<SKCanvas> record) =>
        subtrees.Kept(laid.Path, laid, context.Keyed, () => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(laid.LocalReach, useRTree: true));
            return recorder.EndRecording();
        });

    internal Seq<(IStyle Style, SKTextBlob Blob)> Blobs(Laid.Text laid, Typeset.Featured set) =>
        blobs.Kept(laid.Path, laid, unit, () => toSeq(set.Runs.GroupBy(static run => run.Style)).Choose(runs => Built(runs.Key, toSeq(runs))).Strict());

    private Option<(IStyle Style, SKTextBlob Blob)> Built(IStyle style, Seq<Glyphs> runs) {
        using SKTextBlobBuilder builder = new();
        _ = runs.Iter(run => {
            using SKFont font = new(run.Typeface, run.Style.FontSize) { Edging = Raster.Edging, Subpixel = Raster.SubpixelPositioning, Hinting = Raster.Hinting };
            builder.AddPositionedRun(run.Ids.AsSpan(), font, run.Points.AsSpan());
        });
        return Optional(builder.Build()).Map(blob => (Style: style, Blob: blob));
    }

    internal Option<SKImageFilter> Filtered(Laid laid, Seq<Filter> chain) => filters.Kept(laid.Path, laid, chain, () => Filter.Chained(chain));

    // --- [RASTER]
    internal Option<PlacedImage> Rastered(SKRect local, SKMatrix device, Action<SKCanvas> record) {
        using SKPictureRecorder recorder = new();
        record(recorder.BeginRecording(local));
        using SKPicture drawn = recorder.EndRecording();
        SKMatrix grid = device.PostConcat(SKMatrix.CreateScale(density, density));
        SKRectI pixels = SKRectI.Ceiling(grid.MapRect(local), outwards: true);
        using SKSurface? surface = SKSurface.Create(target.WithSize(pixels.Width, pixels.Height), raster);
        return Optional(surface).Map(held => {
            held.Canvas.SetMatrix(grid.PostConcat(SKMatrix.CreateTranslation(-pixels.Left, -pixels.Top)));
            held.Canvas.DrawPicture(drawn);
            return new PlacedImage(held.Snapshot(), SKRect.Create(pixels.Width, pixels.Height), grid.Invert().PreConcat(SKMatrix.CreateTranslation(pixels.Left, pixels.Top)));
        });
    }

    internal void Casts(SKCanvas canvas, Laid laid, RoleStyle style, Context context, SKRect shape, SKClipOperation clip, Seq<(Shadow Shadow, SKColorF Color)> stack) =>
        _ = (stack.Filter(cast => cast.Shadow.Clip == clip) switch {
            { IsEmpty: true } => Option<PlacedImage>.None,
            var held => casts.Kept((laid.Path, style.Role, clip), laid, (context.Keyed.Device.MapRect(shape), context.Scale, laid.Corners, laid.Smoothing, held), () =>
                Rastered(SKRect.Inflate(shape, style.Overhang, style.Overhang), context.Keyed.Device, recording => {
                    using SKPath outline = laid.Outline(shape, 0f);
                    recording.ClipPath(outline, clip, antialias: true);
                    _ = held.Iter(cast => cast.Shadow.Cast(recording, offset => laid.Outline(shape, offset), cast.Color));
                })),
        }).Iter(raster => raster.Draw(canvas));

    internal Option<SKShader> Drawn(Laid laid, Material.Drawn drawn, HashMap<string, Option<SKColorF>> colors, SKRect frame, Context context, float clock) =>
        drawn.Timed
            ? Some(Shader(drawn, colors, frame, clock))
            : fills.Kept((laid.Path, drawn.Effect), laid, (colors, frame.Size, context.Scale), () =>
                Rastered(SKRect.Create(frame.Size), SKMatrix.CreateScale(context.Scale, context.Scale), recording => {
                    using SKShader shader = Shader(drawn, colors, SKRect.Create(frame.Size), clock);
                    using SKPaint paint = new() { Shader = shader };
                    recording.DrawRect(SKRect.Create(frame.Size), paint);
                })).Map(raster => raster.Shader(frame.Location));

    private static SKShader Shader(Material.Drawn drawn, HashMap<string, Option<SKColorF>> colors, SKRect frame, float clock) {
        using SKRuntimeEffectUniforms uniforms = new(drawn.Effect);
        if (uniforms.Contains(Material.Drawn.Size)) uniforms[Material.Drawn.Size] = frame.Size;
        if (uniforms.Contains(Material.Drawn.Clock)) uniforms[Material.Drawn.Clock] = clock;
        _ = colors.Filter((name, _) => uniforms.Contains(name)).Iter((name, color) => color.Iter(held => uniforms[name] = held));
        using SKShader shader = drawn.Effect.ToShader(uniforms);
        return shader.WithLocalMatrix(SKMatrix.CreateTranslation(frame.Left, frame.Top));
    }

    // --- [GLYPH]
    internal void Glyph(SKCanvas canvas, GlyphSource source, Ground ground, Pen pen, SKColorF tint, SKRect content) {
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
        filters.Retain(scene);
        art = Released(art, (key, _) => shown.Contains(key), static picture => picture.Dispose());
    }

    public void Dispose() {
        subtrees.Clear();
        blobs.Clear();
        casts.Clear();
        fills.Clear();
        filters.Clear();
        art = Released(art, static (_, _) => false, static picture => picture.Dispose());
    }

    private static bool Content(Laid held, Laid laid) => true;

    private static void Released(Seq<(IStyle Style, SKTextBlob Blob)> built) => _ = built.Iter(static run => run.Blob.Dispose());

    private static void Released(Option<SKImageFilter> chain) => _ = chain.Iter(static held => held.Dispose());

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

        public void Retain(Scene scene) => Pruned(entry => scene.Index.Find(entry.Laid.Path).Exists(shown => reuses(entry.Laid, shown)));

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
    private sealed record Pass(PaintCache Cache, Scene Scene, ColorScheme Theme, HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes, Option<(Seq<Key> Path, Editor Editor)> Editing, bool Active,
                               LanguageExt.HashSet<Seq<Key>> Live, float Clock);

    public static void Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        ColorScheme theme = scene.Layout.Cascade.Theme;
        HashMap<(Seq<Key> Path, Interaction Interaction), float> washes = mount.Washes(now);
        Option<(Seq<Key> Path, Editor Editor)> editing = from focus in mount.Focused from editor in mount.Editing select (focus, editor);
        cache.Retain(scene);
        Pass pass = new(cache, scene, theme, washes, editing, mount.Host.Visibility.Keyed,
            toHashSet(washes.Keys.ToSeq().Map(static key => key.Path).Concat(editing.ToSeq().Map(static held => held.Path)).Concat(scene.Timed.Map(static laid => laid.Path)).Bind(Scene.Above)),
            (float)now.TotalSeconds);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRegion(damage);
        canvas.Clear(SKColors.Transparent);
        Context root = new(None, canvas.TotalMatrix, []);
        Draw(canvas, pass, scene.Layout, root, scene.Layout.Frame.Location);
        _ = washes.Filter(static (key, _) => key.Interaction.Ring).Iter((key, weight) => scene.Ring(key.Path).Iter(ring => Ringed(canvas, pass, root, ring, weight)));
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
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        Option<SKImageFilter> filter = laid.Node.Role.IsSome && !laid.Style.Filters.IsEmpty ? pass.Cache.Filtered(laid, laid.Style.Filters) : None;
        if (laid.Alpha < 1f || filter.IsSome) Layer(canvas, laid.LocalReach, laid.Alpha, filter);
        bool filled = laid.Node.Role.IsSome && laid.Node.AcceptsBackground && Background(canvas, pass, laid, context, local);
        Ground under = Styles.Grounded(laid.Cascade, laid.Style, laid.Node);
        Context inner = context with { Plot = Layouts.Plotted(laid) | context.Plot };
        Seq<(Shadow Shadow, SKColorF Color)> stack = laid.Node.Role.IsSome && !filled ? Shadows(pass.Theme, laid.Style) : [];
        if (stack.IsEmpty) Inside(canvas);
        else Cast(canvas, pass.Cache, laid, context, stack, Inside);

        void Inside(SKCanvas target) {
            Content(target, pass, laid, context, under, laid.ContentBox);
            if (laid.Children.IsEmpty) return;
            using SKPath outline = laid.Outline(local, 0f);
            using SKAutoCanvasRestore masked = new(target, doSave: false);
            Option<(SKPoint From, SKPoint To, SKColor[] Alphas, float[] Stops)> fade = Faded(pass.Scene, laid, local);
            if (fade.IsSome) _ = target.SaveLayer(local, paint: null);
            _ = laid.Children.Iter(child => {
                using SKAutoCanvasRestore clipped = new(target);
                if (laid.Node.Clips(child.Node)) target.ClipPath(outline, antialias: true);
                Draw(target, pass, child, inner, child.Frame.Location + pass.Scene.Shift(laid, child));
            });
            _ = fade.Iter(held => {
                using SKShader shader = SKShader.CreateLinearGradient(held.From, held.To, held.Alphas, held.Stops, SKShaderTileMode.Clamp);
                using SKPaint mask = new() { Shader = shader, BlendMode = SKBlendMode.DstIn };
                target.DrawRect(local, mask);
            });
        }
    }

    private static Seq<(Interaction Interaction, float Weight)> Washed(Pass pass, Laid laid, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.Washes.Find((laid.Path, interaction)).Map(weight => (Interaction: interaction, Weight: weight))) switch {
            var own when own.IsEmpty && laid.Node.Role.IsNone => context.Washes,
            var own => own,
        };

    private static void Layer(SKCanvas canvas, SKRect bounds, float alpha, Option<SKImageFilter> filter) {
        using SKPaint veil = new() { ColorF = new SKColorF(0f, 0f, 0f, alpha) };
        _ = filter.Iter(held => veil.ImageFilter = held);
        _ = canvas.SaveLayer(bounds, veil);
    }

    private static Option<SKColorF> Solid(ColorScheme theme, RoleStyle style) => style.Solid.Bind(value => theme.Color(value, style, None, None));

    private static void Cast(SKCanvas canvas, PaintCache cache, Laid laid, Context context, Seq<(Shadow Shadow, SKColorF Color)> stack, Action<SKCanvas> content) =>
        _ = cache.Rastered(laid.LocalReach, context.Keyed.Device, content).Match(
            Some: source => {
                using SKImage drawn = source.Image;
                Shown(canvas, source, stack, SKClipOperation.Difference);
                content(canvas);
                Shown(canvas, source, stack, SKClipOperation.Intersect);
            },
            None: () => content(canvas));

    private static void Shown(SKCanvas canvas, PlacedImage source, Seq<(Shadow Shadow, SKColorF Color)> stack, SKClipOperation clip) =>
        _ = source.Shadowed(stack.Filter(cast => cast.Shadow.Clip == clip)).Iter(cast => {
            using SKImage shown = cast.Image;
            cast.Draw(canvas);
        });

    private static Seq<(Shadow Shadow, SKColorF Color)> Shadows(ColorScheme theme, RoleStyle style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Color, style, None, None).Map(color => (Shadow: shadow, Color: color))).Strict();

    // --- [COLOR]
    private static readonly HashMap<Interaction, ColorValue> Untargeted = [];

    private static Option<SKColorF> Resolved(ColorScheme theme, RoleStyle style, Seq<(Interaction Interaction, float Weight)> washes, HashMap<Interaction, ColorValue> targets, ColorValue value, Option<Series> series) =>
        washes.Fold(theme.Color(value, style, None, series), (rest, wash) =>
            from start in rest
            select (theme.Color(value, style, Some(wash.Interaction), series) | targets.Find(wash.Interaction).Bind(target => theme.Color(target, style, None, series)))
                .Map(end => start.ToUnicolour().Mix(end.ToUnicolour(), ColourSpace.Oklab, wash.Weight).MapToRgbGamut(GamutMap.OklchChromaReduction).ToSKColorF())
                .IfNone(start));

    private static SKPaint Colored(SKColorF color) => new() { IsAntialias = true, ColorF = color };

    // --- [BACKGROUND]
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    private static bool Background(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect frame) {
        RoleStyle style = laid.Style;
        using SKPath shape = laid.Outline(frame, 0f);
        Seq<(Material Fill, SKPaint Paint)> fills = toSeq(style.Fills.Choose(fill => Filled(pass, laid, context, None, frame, fill).Map(paint => (Fill: fill, Paint: paint))).ToArray());
        Seq<(Shadow Shadow, SKColorF Color)> stack = fills.IsEmpty ? [] : Shadows(pass.Theme, style);
        pass.Cache.Casts(canvas, laid, style, context, frame, SKClipOperation.Difference, stack);
        _ = fills.Iter(held => Covered(canvas, held.Fill, frame, clip => clip.ClipPath(shape, antialias: true), () => {
            using SKPaint owned = held.Paint;
            canvas.DrawPath(shape, owned);
        }));
        pass.Cache.Casts(canvas, laid, style, context, frame, SKClipOperation.Intersect, stack);
        if (style.Strokes.IsEmpty) return !fills.IsEmpty;
        Ground under = laid.Cascade.Ground;
        bool bounding = laid.Node.Part.IsSome && Solid(pass.Theme, style).ForAll(fill => Styles.Ratio(fill, under.Under) < pass.Theme.Contrast.Graphic);
        using SKPath rim = laid.Outline(frame, -style.HalfStroke);
        _ = style.Strokes.Iter(stroke => Resolved(pass.Theme, style, context.Washes, Untargeted, stroke.Color, None).Map(color => bounding ? Styles.Legible(style, under, color, pass.Theme.Contrast.Graphic) : color).Iter(color => {
            using SKPaint edge = new() { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = style.Metrics[Metric.Stroke] };
            canvas.DrawPath(rim, edge);
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
            solid: static (held, solid) => Resolved(held.Pass.Theme, held.Style, held.Context.Washes, held.Style.Look.Washes, solid.Color, held.Series).Map(Colored),
            gradient: static (held, gradient) => gradient.Stops.Traverse(stop => Resolved(held.Pass.Theme, held.Style, held.Context.Washes, held.Style.Look.Washes, stop.Color, held.Series)).As()
                .Map(colors => Shaded(Gradient(held.Frame, gradient, colors))),
            drawn: static (held, drawn) => held.Pass.Cache.Drawn(
                    held.Laid, drawn, drawn.Uniforms.Map(value => Resolved(held.Pass.Theme, held.Style, held.Context.Washes, held.Style.Look.Washes, value, None)), held.Frame, held.Context, held.Pass.Clock)
                .Map(Shaded),
            backdrop: static (_, _) => Some(new SKPaint { IsAntialias = true, BlendMode = SKBlendMode.Clear }),
            blurred: static (held, blurred) => Resolved(held.Pass.Theme, held.Style, held.Context.Washes, held.Style.Look.Washes, blurred.Tint, held.Series).Map(Colored));

    private static SKPaint Shaded(SKShader shader) {
        using SKShader owned = shader;
        return new SKPaint { IsAntialias = true, Shader = owned };
    }

    private static SKShader Gradient(SKRect frame, Material.Gradient gradient, Seq<SKColorF> colors) {
        (float sin, float cos) = MathF.SinCos(float.DegreesToRadians(gradient.Angle));
        float reach = (MathF.Abs(cos * frame.Width) + MathF.Abs(sin * frame.Height)) / 2f;
        SKPoint center = new(frame.MidX, frame.MidY);
        return SKShader.CreateLinearGradient(
            new SKPoint(center.X - (cos * reach), center.Y - (sin * reach)),
            new SKPoint(center.X + (cos * reach), center.Y + (sin * reach)),
            [.. colors],
            Srgb,
            [.. gradient.Stops.Map(static stop => (float)stop.At)],
            SKShaderTileMode.Clamp);
    }

    // --- [RING]
    private static void Ringed(SKCanvas canvas, Pass pass, Context root, (Laid Laid, SKRect Frame, SKRect Clip, RoleStyle Style) ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(ring.Clip, antialias: true);
        if (weight < 1f) Layer(canvas, SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), weight, None);
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        (Context at, SKRect shape, Seq<(Shadow Shadow, SKColorF Color)> stack) = (root.Within(ring.Frame.Location), SKRect.Create(ring.Frame.Size), Shadows(pass.Theme, ring.Style));
        pass.Cache.Casts(canvas, ring.Laid, ring.Style, at, shape, SKClipOperation.Difference, stack);
        pass.Cache.Casts(canvas, ring.Laid, ring.Style, at, shape, SKClipOperation.Intersect, stack);
    }

    // --- [FADE]
    private static Option<(SKPoint From, SKPoint To, SKColor[] Alphas, float[] Stops)> Faded(Scene scene, Laid laid, SKRect viewport) =>
        from fade in laid.Node.Role.IsSome ? Some(laid.Style.Metrics[Metric.EdgeFade]) : None
        where fade > 0f
        from scroll in scene.Adjuster(laid.Path).Bind(static adjuster => adjuster.Switch(scroll: static scroll => Some(scroll), split: static _ => Option<Adjuster.Scroll>.None))
        let hidden = (Before: scroll.Value > scroll.Min, After: scroll.Value < scroll.Max)
        let length = scroll.Axis.Of(viewport.Size)
        where (hidden.Before || hidden.After) && length > 0f
        let edge = float.Min(fade, length / 2f) / length
        select (From: viewport.Location, To: (SKPoint)scroll.Axis.With(viewport.Location, scroll.Axis.Of(viewport.Location) + length),
            Alphas: new[] { hidden.Before ? SKColors.Transparent : SKColors.Black, SKColors.Black, SKColors.Black, hidden.After ? SKColors.Transparent : SKColors.Black },
            Stops: new[] { 0f, edge, 1f - edge, 1f });

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, Laid laid, Context context, Ground under, SKRect content) =>
        laid.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Under: under, Content: content),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Under, held.Content),
            text: static (held, text) => Texted(held.Canvas, held.Pass, text, held.Context, held.Under, held.Content));

    private static void Contained(SKCanvas canvas, Pass pass, Laid.Box laid, Context context, Ground under, SKRect content) =>
        laid.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Laid: laid, Context: context, Under: under, Content: content),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marked(held.Canvas, held.Pass, held.Laid, held.Context, mark, plot)),
            glyph: static (held, glyph) => Resolved(held.Pass.Theme, held.Laid.Style, held.Context.Washes, Untargeted, held.Laid.Style.Text, None)
                .Map(tint => Styles.Legible(held.Laid.Style, held.Under, tint, held.Pass.Theme.Contrast.Graphic))
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, glyph.Art, held.Under, held.Context.Pen(held.Laid.Style), tint, held.Content)),
            image: static (held, image) => Pictured(held.Canvas, held.Pass.Theme, held.Laid.Style, held.Context, image, held.Content),
            slot: static (held, _) => {
                using SKPaint clear = new() { BlendMode = SKBlendMode.Clear };
                held.Canvas.DrawRect(SKRect.Create(held.Laid.Frame.Size), clear);
            });

    private static void Texted(SKCanvas canvas, Pass pass, Laid.Text laid, Context context, Ground under, SKRect content) =>
        _ = Resolved(pass.Theme, laid.Style, context.Washes, Untargeted, laid.Style.Text, None).Map(ink => Styles.Legible(laid.Style, under, ink, pass.Theme.Contrast.Text)).Iter(ink => pass.Editing.Filter(held => held.Path == laid.Path).Match(
            Some: held => held.Editor.Paint(canvas, laid, content, pass.Cache.Raster,
                (SKColor)Solid(pass.Theme, Styles.Resolve(laid.Cascade, Role.Selection, laid.Style.Conditions)).IfNone(SKColorF.Empty), (SKColor)ink, pass.Active ? laid.Style.Metrics[Metric.Stroke] : 0f),
            None: () => Typesetting.Paint(canvas, pass.Cache, laid, (SKColor)ink)));

    private static void Pictured(SKCanvas canvas, ColorScheme theme, RoleStyle style, Context context, Node.Image image, SKRect content) {
        SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Width, image.Source.Height));
        SKRect device = context.Device.MapRect(target);
        using SKPaint paint = new() { IsAntialias = true, BlendMode = style.Blend };
        _ = (image.Source.IsAlphaOnly ? (style.Strokes.Head.Map(static stroke => stroke.Color) | style.Solid).Bind(value => Resolved(theme, style, context.Washes, Untargeted, value, image.Series)) : None)
            .Iter(color => paint.ColorF = color);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.DrawImage(image.Source, target, image.Rendering.Sampling(MathF.Min(device.Width / image.Source.Width, device.Height / image.Source.Height)), paint);
    }

    // --- [MARKS]
    private static void Marked(SKCanvas canvas, Pass pass, Laid laid, Context context, Node.Mark mark, PlotFrame frame) {
        RoleStyle style = laid.Style;
        Pen pen = context.Pen(style);
        if (!style.Fills.IsEmpty) {
            using SKPath interior = Figure.Traced(builder => mark.Figure.Interior(builder, frame, pen));
            _ = style.Fills.Iter(fill => Filled(pass, laid, context, mark.Series, interior.Bounds, fill).Iter(paint => Covered(canvas, fill, interior.Bounds, held => held.ClipPath(interior, antialias: true), () => {
                using SKPaint owned = paint;
                owned.BlendMode = style.Blend;
                canvas.DrawPath(interior, owned);
            })));
        }
        Seq<(Stroke Stroke, SKColorF Color)> edges = style.Strokes.Choose(stroke => Resolved(pass.Theme, style, context.Washes, Untargeted, stroke.Color, mark.Series).Map(color => (Stroke: stroke, Color: color)));
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
