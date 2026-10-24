using System.Reflection;
using Rasm.UI.Styling;
using SkiaSharp;
using Svg.Model.Services;
using Svg.Skia;
using Wacton.Unicolour;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record Context(Option<(PlotPlane Plane, SKMatrix Frame)> Plane, SKColorF Ground, Seq<(Interaction Interaction, float Weight)> Washes);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    private static readonly Unicolour White = new(ColourSpace.Rgb, 1.0, 1.0, 1.0);
    private static readonly Unicolour Black = new(ColourSpace.Rgb, 0.0, 0.0, 0.0);
    private readonly SkiaModel model;
    private readonly SkiaSvgAssetLoader loader;
    private HashMap<Seq<Key>, (Laid Laid, Context Context, SKPicture Picture)> subtrees;
    private HashMap<(Assembly Assembly, string Resource), SKPicture> art;

    public PaintCache(Func<GlyphRole, GlyphSource> glyphs, float scale, SKSurfaceProperties raster) {
        model = new SkiaModel(new SKSvgSettings());
        loader = new SkiaSvgAssetLoader(model);
        Glyphs = glyphs;
        Scale = scale;
        Raster = new Topten.RichTextKit.TextPaintOptions {
            Edging = raster.PixelGeometry is SKPixelGeometry.Unknown ? SKFontEdging.Antialias : SKFontEdging.SubpixelAntialias,
            Hinting = raster.IsUseDeviceIndependentFonts ? SKFontHinting.None : SKFontHinting.Slight,
            SubpixelPositioning = true,
        };
    }

    public Func<GlyphRole, GlyphSource> Glyphs { get; }
    public float Scale { get; }
    public Topten.RichTextKit.TextPaintOptions Raster { get; }

    // --- [RECORD]
    internal SKPicture Subtree(Laid laid, Context context, Func<SKRect> cull, Action<SKCanvas> record) {
        Option<(Laid Laid, Context Context, SKPicture Picture)> held = subtrees.Find(laid.Path);
        return held.Filter(entry => ReferenceEquals(entry.Laid, laid) && entry.Context == context).Map(static entry => entry.Picture).IfNone(() => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(cull(), useRTree: true));
            SKPicture made = recorder.EndRecording();
            _ = held.Iter(static entry => entry.Picture.Dispose());
            subtrees = subtrees.AddOrUpdate(laid.Path, (laid, context, made));
            return made;
        });
    }

    // --- [GLYPH]
    public void Glyph(SKCanvas canvas, Either<GlyphRole, GlyphSource> glyph, SKColorF ground, SKColorF tint, SKRect content) {
        GlyphSource source = glyph.IfLeft(Glyphs);
        SKPicture picture = Recorded(source.Assembly, Ink.Colour(ground) switch {
            var under when under.Contrast(White) > under.Contrast(Black) => source.Dark.IfNone(source.Light),
            _ => source.Light,
        });
        SKRect cull = picture.CullRect;
        SKRect fitted = content.AspectFit(cull.Size);
        (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
        SKMatrix place = SKMatrix.CreateScaleTranslation(x, y, fitted.Left - (cull.Left * x), fitted.Top - (cull.Top * y));
        using SKColorFilter? ink = source.Dark.IsNone ? SKColorFilter.CreateBlendMode((SKColor)tint, SKBlendMode.SrcIn) : null;
        using SKPaint? tinted = ink is null ? null : new SKPaint { ColorFilter = ink };
        canvas.DrawPicture(picture, in place, tinted);
    }

    private SKPicture Recorded(Assembly assembly, string resource) =>
        art.Find((assembly, resource)).IfNone(() => {
            using Stream stream = assembly.GetManifestResourceStream(resource) ?? throw new InvalidOperationException($"{assembly.GetName().Name} embeds no {resource}");
            SKPicture made = SKSvg.ToPicture(SvgService.Open(stream) ?? throw new InvalidOperationException($"{resource} in {assembly.GetName().Name} holds no SVG document"), model, loader)
                ?? throw new InvalidOperationException($"{resource} in {assembly.GetName().Name} draws nothing");
            art = art.Add((assembly, resource), made);
            return made;
        });

    // --- [RETENTION]
    public void Retain(Scene scene) {
        LanguageExt.HashSet<(Assembly Assembly, string Resource)> shown = toHashSet(
            from source in scene.Order.Choose(laid => laid.Node is Node.Glyph glyph ? Some(glyph.Art.IfLeft(Glyphs)) : None)
            from resource in source.Dark.ToSeq().Add(source.Light)
            select (source.Assembly, resource));
        subtrees = Released(subtrees, (path, entry) => scene.Index.Find(path).Exists(laid => ReferenceEquals(laid, entry.Laid)), static entry => entry.Picture.Dispose());
        art = Released(art, (key, _) => shown.Contains(key), static picture => picture.Dispose());
    }

    public void Dispose() {
        subtrees = Released(subtrees, static (_, _) => false, static entry => entry.Picture.Dispose());
        art = Released(art, static (_, _) => false, static picture => picture.Dispose());
    }

    private static HashMap<TKey, TValue> Released<TKey, TValue>(HashMap<TKey, TValue> held, Func<TKey, TValue, bool> kept, Action<TValue> release) {
        _ = held.Filter((key, value) => !kept(key, value)).Values.Iter(release);
        return held.Filter(kept);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Painter {
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    private sealed record Pass(PaintCache Cache, Scene Scene, Theme Theme, HashMap<(Seq<Key> Path, Interaction Interaction), float> Washes, Option<(Seq<Key> Path, Editor Editor)> Editing, bool Active,
                               LanguageExt.HashSet<Seq<Key>> Live, float Clock);

    // --- [SCENE]
    public static void Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        Theme theme = scene.Layout.Cascade.Theme;
        HashMap<(Seq<Key> Path, Interaction Interaction), float> washes = mount.Washes(now);
        Pass pass = new(cache, scene, theme, washes, mount.Editing, mount.Host.Active,
            toHashSet(washes.Keys.ToSeq().Bind(static key => Scene.Above(key.Path).Tail)
                .Concat(mount.Editing.ToSeq().Bind(static held => Scene.Above(held.Path)))
                .Concat(scene.Order.Filter(static laid => laid.Timed).Bind(static laid => Scene.Above(laid.Path)))),
            (float)now.TotalSeconds);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRegion(damage);
        canvas.Scale(cache.Scale);
        canvas.Clear(SKColors.Transparent);
        Draw(canvas, pass, scene.Layout, new Context(None, theme.Chrome[(Role.Panel, Condition.Plain, Interaction.Rest, Channel.Fill)], []), scene.Layout.Frame.Location);
    }

    private static void Draw(SKCanvas canvas, Pass pass, Laid laid, Context context, SKPoint at) {
        Context own = context with { Washes = Washed(pass, laid, context) };
        if (!pass.Live.Contains(laid.Path)) {
            canvas.DrawPicture(pass.Cache.Subtree(laid, own, () => Local(laid), recording => Self(recording, pass, laid, own)), at);
        } else if (!canvas.QuickReject(Local(laid) switch { var local => SKRect.Create(local.Location + at, local.Size) })) {
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.Translate(at.X, at.Y);
            Self(canvas, pass, laid, own);
        }
    }

    private static void Self(SKCanvas canvas, Pass pass, Laid laid, Context context) {
        SKRect local = SKRect.Create(laid.Frame.Size);
        float alpha = (laid.Style.Veiled && !laid.Cascade.Enclosing.Veiled ? pass.Theme.Veil : 1f) * (laid.Node is Node.Styled styled ? (float)styled.Opacity : 1f);
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        if (alpha < 1f) Layer(canvas, Local(laid), alpha);
        if (laid.Node.Role.IsSome && laid.Node is not (Node.Mark or Node.Image)) Surface(canvas, pass.Theme, laid.Style, laid is Laid.Text ? [] : laid.Style.Look.Shadows, context.Washes, local, pass.Clock);
        Context grounded = context with { Ground = Ground(pass.Theme, laid).IfNone(context.Ground) };
        _ = Content(canvas, pass, laid, grounded, Layouts.Content(laid)).Iter(static draw => draw());
        Option<(PlotPlane Plane, SKMatrix Frame)> plot = Layouts.Plotted(laid) | context.Plane;
        _ = laid.Children.Iter(child => {
            SKPoint at = child.Frame.Location + pass.Scene.Shift(laid, child);
            using SKAutoCanvasRestore clipped = new(canvas);
            if (Scene.Clips(laid, child)) canvas.ClipRect(local, antialias: true);
            Draw(canvas, pass, child, grounded with { Plane = plot.Map(held => (held.Plane, held.Frame.PostConcat(SKMatrix.CreateTranslation(-at.X, -at.Y)))) }, at);
        });
    }

    private static Seq<(Interaction Interaction, float Weight)> Washed(Pass pass, Laid laid, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.Washes.Find((laid.Path, interaction)).Map(weight => (Interaction: interaction, Weight: weight))) switch {
            var own when own.IsEmpty && laid.Node.Role.IsNone => context.Washes,
            var own => own,
        };

    private static SKRect Local(Laid laid) => laid.Reach switch { var reach => SKRect.Create(reach.Location - laid.Frame.Location, reach.Size) };

    private static void Layer(SKCanvas canvas, SKRect bounds, float alpha) {
        using SKPaint veil = new() { ColorF = new SKColorF(0f, 0f, 0f, alpha) };
        _ = canvas.SaveLayer(bounds, veil);
    }

    private static Option<SKColorF> Ground(Theme theme, Laid laid) => laid.Node.Role.IsSome ? Solid(theme, laid.Style).Filter(static color => color.Alpha >= 1f) : None;

    private static Option<SKColorF> Solid(Theme theme, Style style) => style.Look.Solid.Bind(ink => theme.Color(ink, style, Interaction.Rest, None));

    // --- [COLOR]
    private static Option<SKColorF> Inked(Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Ink ink, Option<Series> series) =>
        washes.Fold(theme.Color(ink, style, Interaction.Rest, series), (rest, wash) =>
            from start in rest
            from end in theme.Color(ink, style, wash.Interaction, series)
            select Ink.Rgb(Ink.Colour(start).Mix(Ink.Colour(end), ColourSpace.Oklab, wash.Weight).MapToRgbGamut(GamutMap.OklchChromaReduction)));

    private static SKPaint Colored(SKColorF color) => new() { IsAntialias = true, ColorF = color };

    // --- [SURFACE]
    private static void Surface(SKCanvas canvas, Theme theme, Style style, Seq<Shadow> shadows, Seq<(Interaction Interaction, float Weight)> washes, SKRect frame, float clock) {
        using SKRoundRect shape = new(frame, style.Metrics[Metric.Radius], style.Metrics[Metric.Radius]);
        _ = shadows.Iter(shadow => theme.Color(shadow.Ink, style, Interaction.Rest, None).Iter(color => {
            using SKMaskFilter? blur = shadow.Blur > 0f ? SKMaskFilter.CreateBlur(SKBlurStyle.Normal, shadow.Blur) : null;
            using SKPaint cast = new() { IsAntialias = true, ColorF = color, MaskFilter = blur };
            using SKRoundRect spread = new(shape);
            spread.Inflate(shadow.Spread, shadow.Spread);
            spread.Offset(shadow.X, shadow.Y);
            canvas.DrawRoundRect(spread, cast);
        }));
        _ = style.Look.Fill.Bind(fill => Filled(theme, style, washes, None, frame, fill, clock)).Iter(fill => {
            using SKPaint paint = fill;
            canvas.DrawRoundRect(shape, paint);
        });
        _ = washes.Filter(wash => style.Look.Fill.ForAll(fill => Inks(fill).ForAll(ink => theme.Color(ink, style, wash.Interaction, None) == theme.Color(ink, style, Interaction.Rest, None))))
            .Choose(wash => style.Look.Washes.Find(wash.Interaction).Bind(ink => theme.Color(ink, style, Interaction.Rest, None)).Map(color => color.WithAlpha(color.Alpha * wash.Weight)))
            .Iter(color => {
                using SKPaint paint = Colored(color);
                canvas.DrawRoundRect(shape, paint);
            });
        _ = style.Look.Stroke.Bind(stroke => Inked(theme, style, washes, stroke.Ink, None)).Iter(color => {
            using SKPaint edge = new() { IsAntialias = true, ColorF = color, Style = SKPaintStyle.Stroke, StrokeWidth = style.Metrics[Metric.Stroke] };
            using SKRoundRect inset = new(shape);
            inset.Deflate(edge.StrokeWidth / 2f, edge.StrokeWidth / 2f);
            canvas.DrawRoundRect(inset, edge);
        });
    }

    private static Option<SKPaint> Filled(Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Option<Series> series, SKRect frame, Material material, float clock) =>
        material.Switch(
            (Theme: theme, Style: style, Washes: washes, Series: series, Frame: frame, Clock: clock),
            solid: static (held, solid) => Inked(held.Theme, held.Style, held.Washes, solid.Ink, held.Series).Map(Colored),
            gradient: static (held, gradient) => gradient.Stops.Traverse(stop => Inked(held.Theme, held.Style, held.Washes, stop.Ink, held.Series)).As()
                .Map(colors => Shaded(Gradient(held.Frame, gradient, colors))),
            drawn: static (held, drawn) => Some(Shaded(Drawn(held.Theme, held.Style, held.Washes, held.Frame, drawn, held.Clock))),
            backdrop: static (_, _) => Some(new SKPaint { IsAntialias = true, BlendMode = SKBlendMode.Clear }));

    private static Seq<Ink> Inks(Material material) =>
        material.Switch(
            solid: static solid => Seq(solid.Ink),
            gradient: static gradient => gradient.Stops.Map(static stop => stop.Ink),
            drawn: static drawn => toSeq(drawn.Inks.Values),
            backdrop: static _ => Seq<Ink>());

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
        using SKRuntimeEffectUniforms uniforms = new(drawn.Effect) { ["size"] = frame.Size };
        if (drawn.Timed) uniforms[Material.Drawn.Clock] = clock;
        using SKRuntimeEffectChildren children = new(drawn.Effect);
        _ = drawn.Inks.Iter((name, ink) => Inked(theme, style, washes, ink, None).Iter(color => uniforms[name] = color));
        return drawn.Effect.ToShader(uniforms, children, SKMatrix.CreateTranslation(frame.Left, frame.Top));
    }

    // --- [CONTENT]
    private static Option<Action> Content(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect content) =>
        laid.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Laid: laid, Context: context, Content: content),
            @default: static (_, _) => Option<Action>.None,
            mark: static (held, mark) => Some<Action>(() => held.Context.Plane.Iter(plot => Marked(held.Canvas, held.Pass.Theme, held.Laid.Style, held.Context.Washes, mark, held.Pass.Cache.Scale, plot.Plane, plot.Frame, held.Pass.Clock))),
            text: static (held, _) => Some<Action>(() => Optional(held.Laid as Laid.Text).Iter(set => Texted(held.Canvas, held.Pass, set, held.Context, held.Content))),
            glyph: static (held, glyph) => Some<Action>(() => Inked(held.Pass.Theme, held.Laid.Style, held.Context.Washes, held.Laid.Style.Look.Text, None)
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, glyph.Art, held.Context.Ground, tint, held.Content))),
            image: static (held, image) => Some(() => Pictured(held.Canvas, held.Pass.Theme, held.Laid.Style, held.Context.Washes, image, held.Content)),
            slot: static (held, _) => Some(() => {
                using SKPaint clear = new() { BlendMode = SKBlendMode.Clear };
                held.Canvas.DrawRect(SKRect.Create(held.Laid.Frame.Size), clear);
            }));

    private static void Texted(SKCanvas canvas, Pass pass, Laid.Text laid, Context context, SKRect content) =>
        _ = Inked(pass.Theme, laid.Style, context.Washes, laid.Style.Look.Text, None).Iter(ink => pass.Editing.Filter(held => held.Path == laid.Path).Match(
            Some: held => held.Editor.Paint(canvas, laid, content, pass.Cache.Raster,
                (SKColor)Solid(pass.Theme, Styles.Resolve(laid.Cascade, Role.Selection, laid.Style.Conditions)).IfNone(SKColorF.Empty), (SKColor)ink, pass.Active ? laid.Style.Metrics[Metric.Stroke] : 0f),
            None: () => Shadowed(canvas, pass.Theme, laid.Style, () => Typesetting.Paint(canvas, laid, (SKColor)ink, pass.Cache.Raster))));

    private static void Shadowed(SKCanvas canvas, Theme theme, Style style, Action draw) {
        Seq<SKImageFilter> casts = style.Look.Shadows.Choose(shadow => theme.Color(shadow.Ink, style, Interaction.Rest, None).Map(color => Cast(shadow, (SKColor)color)));
        if (!casts.IsEmpty) {
            using SKImageFilter merged = SKImageFilter.CreateMerge([.. casts]);
            using SKPaint layer = new() { ImageFilter = merged };
            _ = casts.Iter(static cast => cast.Dispose());
            int saved = canvas.SaveLayer(layer);
            draw();
            canvas.RestoreToCount(saved);
        }
        draw();
    }

    private static SKImageFilter Cast(Shadow shadow, SKColor color) {
        using SKImageFilter? grown = shadow.Spread > 0f ? SKImageFilter.CreateDilate(shadow.Spread, shadow.Spread) : null;
        return SKImageFilter.CreateDropShadowOnly(shadow.X, shadow.Y, shadow.Blur, shadow.Blur, color, grown);
    }

    private static void Pictured(SKCanvas canvas, Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Node.Image image, SKRect content) {
        SKSize natural = new(image.Source.Width, image.Source.Height);
        SKRect target = image.Fit switch {
            ImageFit.Stretch => content,
            ImageFit.Contain => content.AspectFit(natural),
            ImageFit.Cover => content.AspectFill(natural),
        };
        using SKPaint paint = new() { IsAntialias = true, BlendMode = style.Look.Blend };
        _ = (image.Source.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16
                ? (style.Look.Stroke.Map(static stroke => stroke.Ink) | style.Look.Solid)
                    .Bind(ink => Inked(theme, style, washes, ink, image.Series))
                : None)
            .Iter(color => paint.ColorF = color);
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(content, antialias: true);
        canvas.DrawImage(image.Source, target, image.Sampling, paint);
    }

    // --- [MARKS]
    private static void Marked(SKCanvas canvas, Theme theme, Style style, Seq<(Interaction Interaction, float Weight)> washes, Node.Mark mark, float scale, PlotPlane plane, SKMatrix frame, float clock) {
        float width = style.Metrics[Metric.Stroke];
        _ = style.Look.Fill.Iter(fill => {
            using SKPath path = Figure.Traced(builder => mark.Figure.Interior(builder, plane, frame, scale));
            _ = Filled(theme, style, washes, mark.Series, path.Bounds, fill, clock).Iter(held => {
                using SKPaint paint = held;
                paint.BlendMode = style.Look.Blend;
                canvas.DrawPath(path, paint);
            });
        });
        _ = (from stroke in style.Look.Stroke from color in Inked(theme, style, washes, stroke.Ink, mark.Series) select (Stroke: stroke, Color: color)).Iter(held => {
            using SKPath path = Figure.Traced(builder => mark.Figure.Edges(builder, plane, frame, scale, width, style.Metrics[Metric.Spacing]));
            Seq<float> rhythm = held.Stroke.Rhythm(mark.Series.Map(static series => series.Ordinal).IfNone(0u));
            using SKPathEffect? dash = rhythm.IsEmpty ? null : SKPathEffect.CreateDash([.. rhythm.Map(beat => beat * width)], 0f);
            using SKPaint paint = new() {
                IsAntialias = true, ColorF = held.Color, Style = SKPaintStyle.Stroke, StrokeWidth = width, StrokeCap = SKStrokeCap.Round, PathEffect = dash, BlendMode = style.Look.Blend,
            };
            canvas.DrawPath(path, paint);
        });
    }
}
