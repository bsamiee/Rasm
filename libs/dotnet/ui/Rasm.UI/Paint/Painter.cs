using Rasm.UI.Geometry;
using Rasm.UI.Glyphs;
using Rasm.UI.Layout;
using Rasm.UI.Scenes;
using Rasm.UI.Styling;
using Rasm.UI.Text;
using SkiaSharp;
using Svg.Skia;

namespace Rasm.UI.Paint;

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class PaintCache : IDisposable {
    private HashMap<string, (Signature Key, Theme Theme, float Scale, SKPicture Value)> pictures;
    private HashMap<string, (Signature Key, Theme Theme, float Scale, SKImage Value)> rasters;
    private LanguageExt.HashSet<string> seen;
    private HashMap<(GlyphSource Source, GlyphVariant Variant, SKColor Tint), SKSvg> glyphs;
    private LanguageExt.HashSet<(GlyphSource Source, GlyphVariant Variant, SKColor Tint)> tinted;

    public SKPicture Picture(Laid laid, Theme theme, float scale, SKRect cull, Action<SKCanvas> record) {
        seen = seen.TryAdd(laid.Id);
        (pictures, SKPicture picture) = Retained(() => pictures, laid, theme, scale, () => {
            using SKPictureRecorder recorder = new();
            record(recorder.BeginRecording(cull));
            return recorder.EndRecording();
        });
        return picture;
    }

    public SKImage Raster(Laid laid, Theme theme, float scale, Func<SKImage> draw) {
        seen = seen.TryAdd(laid.Id);
        (rasters, SKImage raster) = Retained(() => rasters, laid, theme, scale, draw);
        return raster;
    }

    public SKPicture Glyph(GlyphSource source, GlyphVariant variant, SKColor tint) {
        tinted = tinted.TryAdd((source, variant, tint));
        (glyphs, SKSvg svg) = glyphs.Find((source, variant, tint)).Match(
            Some: held => (glyphs, held),
            None: () => source.Load(variant, tint) switch {
                var made => (glyphs.Add((source, variant, tint), made), made),
            });
        return Optional(svg.Picture).ToFin(Error.New($"{source.Key} holds no drawable SVG")).ThrowIfFail();
    }

    public Unit Sweep() {
        (pictures, rasters, glyphs) = (Swept(pictures, seen, static entry => entry.Value), Swept(rasters, seen, static entry => entry.Value), Swept(glyphs, tinted, static svg => svg));
        (seen, tinted) = ([], []);
        return unit;
    }

    public void Dispose() {
        (pictures, rasters, glyphs) = (Swept(pictures, [], static entry => entry.Value), Swept(rasters, [], static entry => entry.Value), Swept(glyphs, [], static svg => svg));
        (seen, tinted) = ([], []);
    }

    private static (HashMap<string, (Signature Key, Theme Theme, float Scale, T Value)> Held, T Value) Retained<T>(Func<HashMap<string, (Signature Key, Theme Theme, float Scale, T Value)>> held, Laid laid, Theme theme, float scale, Func<T> make)
        where T : IDisposable =>
        (laid.Signature, held().Find(laid.Id)) switch {
            var (key, prior) => prior.Filter(entry => entry.Key == key && entry.Theme == theme && entry.Scale == scale).Match(
                Some: entry => (held(), entry.Value),
                None: () => make() switch {
                    var made => (Released(held(), prior).AddOrUpdate(laid.Id, (key, theme, scale, made)), made),
                }),
        };

    private static HashMap<string, (Signature Key, Theme Theme, float Scale, T Value)> Released<T>(HashMap<string, (Signature Key, Theme Theme, float Scale, T Value)> held, Option<(Signature Key, Theme Theme, float Scale, T Value)> stale)
        where T : IDisposable {
        _ = stale.Iter(static entry => entry.Value.Dispose());
        return held;
    }

    private static HashMap<TKey, TValue> Swept<TKey, TValue>(HashMap<TKey, TValue> held, LanguageExt.HashSet<TKey> kept, Func<TValue, IDisposable> owned) {
        _ = held.Filter((key, _) => !kept.Contains(key)).Values.Iter(value => owned(value).Dispose());
        return held.Filter((key, _) => kept.Contains(key));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Painter {
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();

    // --- [SCENE]
    public static IO<Unit> Paint(SKCanvas canvas, Scene scene, PaintCache cache, float scale) =>
        IO.lift(() => {
            Draw(canvas, scene.Theme, cache, scale, scene.Layout);
            return cache.Sweep();
        });

    private static void Draw(SKCanvas canvas, Theme theme, PaintCache cache, float scale, Laid laid) {
        SKRect bounds = laid.Bounds(theme);
        if (laid.Node is Node.Layer) {
            canvas.DrawImage(cache.Raster(laid, theme, scale, () => Rasterized(theme, cache, scale, laid, bounds)), bounds, new SKSamplingOptions(SKFilterMode.Linear));
            return;
        }
        canvas.DrawPicture(cache.Picture(laid, theme, scale, bounds, recording => Self(recording, theme, cache, scale, laid)));
        bool clipped = laid.Node is Node.Scroll;
        if (clipped) {
            _ = canvas.Save();
            canvas.ClipRect(laid.Frame, antialias: true);
        }
        _ = laid.Children.Iter(child => Draw(canvas, theme, cache, scale, child));
        if (clipped) canvas.Restore();
    }

    private static SKImage Rasterized(Theme theme, PaintCache cache, float scale, Laid laid, SKRect bounds) {
        using SKSurface surface = SKSurface.Create(new SKImageInfo((int)MathF.Ceiling(bounds.Width * scale), (int)MathF.Ceiling(bounds.Height * scale), SKImageInfo.PlatformColorType, SKAlphaType.Premul, Srgb));
        surface.Canvas.Scale(scale);
        surface.Canvas.Translate(-bounds.Left, -bounds.Top);
        Self(surface.Canvas, theme, cache, scale, laid);
        _ = laid.Children.Iter(child => Draw(surface.Canvas, theme, cache, scale, child));
        return surface.Snapshot();
    }

    // --- [NODE]
    private static void Self(SKCanvas canvas, Theme theme, PaintCache cache, float scale, Laid laid) {
        _ = laid.Look.Iter(look => Surface(canvas, theme, laid, look));
        _ = laid.Node.Switch(
            (Canvas: canvas, Theme: theme, Cache: cache, Scale: scale, Laid: laid, Inner: laid.Content),
            flex: static (_, _) => unit,
            grid: static (_, _) => unit,
            overlay: static (_, _) => unit,
            scroll: static (held, scroll) => Indicator(held.Canvas, held.Theme, held.Laid, scroll),
            split: static (held, split) => Divider(held.Canvas, held.Theme, held.Laid, split),
            dock: static (_, _) => unit,
            text: static (held, text) => Lettered(held.Canvas, held.Theme, held.Laid, held.Inner, text),
            glyph: static (held, glyph) => Glyphed(held.Canvas, held.Theme, held.Cache, held.Laid, held.Inner, glyph),
            shape: static (held, shape) => Mark(held.Canvas, held.Theme, held.Laid.Cascade.Family, held.Scale, shape.Figure.Mapped(held.Inner), shape.Style),
            image: static (held, image) => Pictured(held.Canvas, held.Inner, image),
            layer: static (_, _) => unit,
            widget: static (held, widget) => widget.Component.Marks(held.Inner).Iter(mark => Mark(held.Canvas, held.Theme, held.Laid.Cascade.Family, held.Scale, mark.Shape, mark.Style)),
            slot: static (held, _) => Punched(held.Canvas, held.Laid.Frame),
            styled: static (_, _) => unit);
    }

    private static void Surface(SKCanvas canvas, Theme theme, Laid laid, Look look) {
        Family family = laid.Cascade.Family;
        float radius = look.Radius.Of(theme.Metrics, laid.Frame.Size);
        if (look.Elevation != Elevation.Flat) {
            using SKImageFilter shadow = SKImageFilter.CreateDropShadowOnly(0f, look.Elevation.Offset * theme.Metrics.Unit, look.Elevation.Blur * theme.Metrics.Unit, look.Elevation.Blur * theme.Metrics.Unit,
                (SKColor)theme.Color(family, new Ink.Themed(ColorRole.Shadow, 1f)));
            using SKPaint cast = new() { IsAntialias = true, ImageFilter = shadow };
            canvas.DrawRoundRect(laid.Frame, radius, radius, cast);
        }
        using SKPaint fill = new() { IsAntialias = true };
        (bool filled, SKShader? owned) = look.Fill.Switch(
            (Theme: theme, Family: family, laid.Frame, Paint: fill),
            solid: static (held, solid) => (Colored(held.Paint, held.Theme.Color(held.Family, solid.Ink)), null),
            gradient: static (held, gradient) => Shaded(held.Paint, Gradient(held.Theme, held.Family, held.Frame, gradient)),
            drawn: static (held, drawn) => Shaded(held.Paint, Drawn(held.Theme, held.Family, held.Frame, drawn)),
            backdrop: static (held, backdrop) => (held.Theme.Display.ReduceTransparency ? Colored(held.Paint, held.Theme.Color(held.Family, backdrop.Fallback)) : Cleared(held.Paint), null));
        using SKShader? shader = owned;
        if (filled) canvas.DrawRoundRect(laid.Frame, radius, radius, fill);
        _ = look.StateLayer.Iter(layer => {
            using SKPaint wash = new() { IsAntialias = true };
            _ = Colored(wash, theme.Color(family, layer));
            canvas.DrawRoundRect(laid.Frame, radius, radius, wash);
        });
        _ = look.Border.Iter(border => {
            using SKPaint edge = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = border.Width };
            _ = Colored(edge, theme.Color(family, border.Ink));
            SKRect inset = SKRect.Inflate(laid.Frame, -border.Width / 2f, -border.Width / 2f);
            float rounding = float.Max(0f, radius - (border.Width / 2f));
            canvas.DrawRoundRect(inset, rounding, rounding, edge);
        });
    }

    // --- [MATERIAL]
    private static bool Colored(SKPaint paint, SKColorF color) {
        paint.SetColor(color, Srgb);
        return color.Alpha > 0f;
    }

    private static bool Cleared(SKPaint paint) {
        paint.BlendMode = SKBlendMode.Clear;
        return true;
    }

    private static Unit Punched(SKCanvas canvas, SKRect frame) {
        using SKPaint paint = new() { BlendMode = SKBlendMode.Clear };
        canvas.DrawRect(frame, paint);
        return unit;
    }

    private static (bool Filled, SKShader? Shader) Shaded(SKPaint paint, SKShader shader) {
        paint.Shader = shader;
        return (true, shader);
    }

    private static SKShader Gradient(Theme theme, Family family, SKRect frame, Material.Gradient gradient) {
        (float sin, float cos) = MathF.SinCos(gradient.Angle * MathF.PI / 180f);
        float reach = (MathF.Abs(cos * frame.Width) + MathF.Abs(sin * frame.Height)) / 2f;
        SKPoint center = new(frame.MidX, frame.MidY);
        return SKShader.CreateLinearGradient(
            new SKPoint(center.X - (cos * reach), center.Y - (sin * reach)),
            new SKPoint(center.X + (cos * reach), center.Y + (sin * reach)),
            [.. gradient.Stops.Map(stop => theme.Color(family, stop.Ink))],
            Srgb,
            [.. gradient.Stops.Map(static stop => stop.At)],
            SKShaderTileMode.Clamp);
    }

    private static SKShader Drawn(Theme theme, Family family, SKRect frame, Material.Drawn drawn) {
        using SKRuntimeShaderBuilder builder = SKRuntimeEffect.BuildShader(drawn.Shader);
        builder.Uniforms["size"] = new float[] { frame.Width, frame.Height };
        foreach ((string name, Ink ink) in drawn.Inks) {
            SKColorF color = theme.Color(family, ink);
            builder.Uniforms[name] = new float[] { color.Red, color.Green, color.Blue, color.Alpha };
        }
        return builder.Build(SKMatrix.CreateTranslation(frame.Left, frame.Top));
    }

    // --- [CONTENT]
    private static Unit Lettered(SKCanvas canvas, Theme theme, Laid laid, SKRect inner, Node.Text text) =>
        Paragraphs.Paint(canvas, theme, laid.Cascade, text, inner,
            (SKColor)theme.Color(laid.Cascade.Family, text.Ink.IfNone(laid.Cascade.Ink)), (SKColor)theme.Color(laid.Cascade.Family, new Ink.Themed(ColorRole.SelectedText, 1f)));

    private static Unit Glyphed(SKCanvas canvas, Theme theme, PaintCache cache, Laid laid, SKRect inner, Node.Glyph glyph) {
        GlyphSource source = laid.Cascade.Glyphs.Find(glyph.Affordance).ThrowIfFail();
        SKPicture picture = cache.Glyph(source, GlyphVariant.On(theme.Color(laid.Cascade.Family, new Ink.Themed(ColorRole.Ground, 1f))),
            (SKColor)theme.Color(laid.Cascade.Family, glyph.Ink.IfNone(laid.Cascade.Ink)));
        SKRect cull = picture.CullRect;
        SKRect fitted = inner.AspectFit(cull.Size);
        SKMatrix place = SKMatrix.CreateScale(fitted.Width / cull.Width, fitted.Height / cull.Height).PostConcat(SKMatrix.CreateTranslation(fitted.Left - (cull.Left * fitted.Width / cull.Width), fitted.Top - (cull.Top * fitted.Height / cull.Height)));
        canvas.DrawPicture(picture, in place);
        return unit;
    }

    private static Unit Pictured(SKCanvas canvas, SKRect inner, Node.Image image) {
        SKSize natural = new(image.Source.Width, image.Source.Height);
        SKRect target = image.Fit switch {
            ImageFit.Contain => inner.AspectFit(natural),
            ImageFit.Cover => inner.AspectFill(natural),
            ImageFit.Fill => inner,
            ImageFit.Actual => SKRect.Create(inner.Left, inner.Top, natural.Width, natural.Height),
        };
        _ = canvas.Save();
        canvas.ClipRect(inner, antialias: true);
        canvas.DrawImage(image.Source, target, new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));
        canvas.Restore();
        return unit;
    }

    private static Unit Divider(SKCanvas canvas, Theme theme, Laid laid, Node.Split split) =>
        laid.Children.Head.Iter(first => {
            float middle = Layouts.Divider(theme) / 2f;
            MarkShape rule = split.Axis is Orientation.Horizontal
                ? new MarkShape.Rule(new(first.Frame.Right + middle, laid.Frame.Top), new(first.Frame.Right + middle, laid.Frame.Bottom))
                : new MarkShape.Rule(new(laid.Frame.Left, first.Frame.Bottom + middle), new(laid.Frame.Right, first.Frame.Bottom + middle));
            _ = Mark(canvas, theme, laid.Cascade.Family, 1f, rule, new MarkStyle.Stroke(new Ink.Themed(ColorRole.Edge, 1f), theme.Metrics.Stroke, []));
        });

    private static Unit Indicator(SKCanvas canvas, Theme theme, Laid laid, Node.Scroll scroll) =>
        laid.Children.Head.Iter(content => {
            bool vertical = scroll.Axis is Orientation.Vertical;
            float view = vertical ? laid.Frame.Height : laid.Frame.Width;
            float extent = vertical ? content.Frame.Height : content.Frame.Width;
            float offset = vertical ? laid.Frame.Top - content.Frame.Top : laid.Frame.Left - content.Frame.Left;
            float thickness = Space.Tight.Of(theme.Metrics);
            if (extent <= view) return;
            float length = float.Max(theme.Metrics.Hit, view * view / extent);
            float start = (view - length) * offset / (extent - view);
            SKRect thumb = vertical
                ? SKRect.Create(laid.Frame.Right - (2f * thickness), laid.Frame.Top + start, thickness, length)
                : SKRect.Create(laid.Frame.Left + start, laid.Frame.Bottom - (2f * thickness), length, thickness);
            _ = Mark(canvas, theme, laid.Cascade.Family, 1f, new MarkShape.Band(thumb, thickness / 2f), new MarkStyle.Fill(new Ink.Themed(ColorRole.Secondary, 0.5f)));
        });

    // --- [MARKS]
    public static Unit Mark(SKCanvas canvas, Theme theme, Family family, float scale, MarkShape shape, MarkStyle style) =>
        style.Switch(
            (Canvas: canvas, Theme: theme, Family: family, Scale: scale, Shape: shape),
            fill: static (held, fill) => {
                using SKPath path = held.Shape.Snapped(held.Scale, 0f).Figure();
                using SKPaint paint = new() { IsAntialias = true };
                _ = Colored(paint, held.Theme.Color(held.Family, fill.Ink));
                held.Canvas.DrawPath(path, paint);
                return unit;
            },
            stroke: static (held, stroke) => {
                using SKPath path = held.Shape.Snapped(held.Scale, stroke.Width).Figure();
                using SKPathEffect? dash = held.Theme.Display.DifferentiateWithoutColor && !stroke.Dash.IsEmpty ? SKPathEffect.CreateDash([.. stroke.Dash], 0f) : null;
                using SKPaint paint = new() { IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = stroke.Width, StrokeCap = SKStrokeCap.Round, PathEffect = dash };
                SKColorF color = held.Theme.Color(held.Family, stroke.Ink);
                _ = Colored(paint, held.Theme.Display.IncreaseContrast ? color.WithAlpha(1f) : color);
                held.Canvas.DrawPath(path, paint);
                return unit;
            });
}
