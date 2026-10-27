using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Marks {
    // --- [KEYS]
    internal sealed record SpriteKey(Option<Marker> Marker, float Diameter, float Scale);

    internal sealed record AtlasKey(MarkKey Mark, SKColorF Color);

    internal sealed record DotsKey(MarkKey Mark, Seq<SKPaint> Paints);

    internal sealed record FieldKey(MarkKey Mark, Option<SKColorF> Color);

    internal sealed record LabelKey(Typeset Set, MarkKey Mark);

    private sealed record Sprite(SKPoint At, float Degrees, float Diameter, Option<float> Value, float Opacity);

    private const int Steps = 256;
    private const int CellsPerMesh = ushort.MaxValue / 4;

    private static readonly int[] Quad = [0, 1, 2, 0, 2, 3];

    private static readonly SKSamplingOptions Nearest = new(SKFilterMode.Nearest, SKMipmapMode.None);
    private static readonly SKSamplingOptions Smooth = new(SKFilterMode.Linear);
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();
    private static readonly SKRuntimeShaderBuilder Lookup = SKRuntimeEffect.BuildShader("""
        uniform shader values;
        uniform shader ramp;
        half4 main(float2 p) {
            half v = values.eval(p).a;
            return v < 0.0 ? half4(0.0) : ramp.eval(float2((v * 255.0) + 0.5, 0.5));
        }
        """);
    private static readonly SKRuntimeColorFilterBuilder Table = SKRuntimeEffect.BuildColorFilter("""
        uniform shader ramp;
        half4 main(half4 c) { return ramp.eval(float2((c.a * 255.0) + 0.5, 0.5)); }
        """);

    // --- [MARKS]
    internal static Seq<Marker> Markers(Figure figure) =>
        figure.Switch(
            rule: static _ => Seq<Marker>(),
            path: static path => path.Start.ToSeq() + path.End.ToSeq(),
            region: static _ => [],
            curve: static _ => [],
            graph: static _ => [],
            grid: static _ => [],
            points: static points => points.Marker.ToSeq(),
            bars: static _ => [],
            box: static _ => [],
            field: static _ => [],
            label: static label => Markers(label.Along));

    internal static void Mark(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, Context context, Figure figure, Option<DataSeries> series, PlotFrame frame) {
        RoleStyle style = layout.Style;
        PaintCache cache = pass.Cache;
        Pen pen = context.Pen(style);
        MarkKey key = new(figure, frame, pen);
        Option<SKColorF> lead = (style.Strokes.Head.Map(static stroke => stroke.Color) | style.Solid).Bind(value => pass.Scene.Layout.Cascade.Theme.WithStateLayers(pass.Scene.ColorCache, value, style, context.StateLayers, series: series));
        if (figure is Figure.Field field) {
            Field(canvas, pass, layout, key, field, style.Look.Scale, lead);
            return;
        }
        if (figure is Figure.Points { Marker.IsNone: true, Sizes.IsNone: true, Values.IsNone: true, Opacity.IsNone: true } dots) {
            Seq<SKPaint> paints = Edged(pass, layout, context, series, figure, frame, pen);
            canvas.DrawPicture(cache.Dots.GetOrAdd(layout.Path, layout, new DotsKey(key, paints), () => {
                Seq<SKPoint> points = dots.At.Map(frame.Point).Strict();
                using SKPictureRecorder recorder = new();
                SKCanvas recording = recorder.BeginRecording(Reach(points, pen.Stroke));
                _ = paints.Iter(paint => recording.DrawPoints(SKPointMode.Points, [.. points], paint));
                return recorder.EndRecording();
            }));
            return;
        }
        if (figure is Figure.Points sprites) {
            _ = lead.Iter(color => Atlased(canvas, pass, layout, key, color, Placed(sprites, frame, pen)));
            return;
        }
        if (!style.Fills.IsEmpty) {
            Outline interior = cache.Interiors.GetOrAdd(layout.Path, layout, key, () => Outline.Of(path => figure.Interior(path, frame, pen)));
            Seq<Action<SKCanvas>> fills = style.Fills.Map((fill, index) => Fills.Fill(new FillContext(pass, layout, context, interior.Path.Bounds, index) { Blend = Some(style.Blend), Series = series }, fill, interior)).Somes().Strict();
            using SKAutoCanvasRestore covered = new(canvas);
            _ = fills.Iter(fill => fill(canvas));
        }
        Outline edge = cache.Contours.GetOrAdd(layout.Path, layout, key, () => Outline.Of(path => figure.Edges(path, frame, pen)));
        _ = Edged(pass, layout, context, series, figure, frame, pen).Iter(paint => canvas.DrawPath(edge.Path, paint));
        Seq<MarkerPlacement> placements = figure.Markers(frame, pen, edge.Path);
        if (!placements.IsEmpty)
            _ = lead.Iter(color => Atlased(canvas, pass, layout, key, color, placements.Map(static placed => new Sprite(placed.At, placed.Degrees, placed.Marker.Size, None, 1f))));
    }

    private static Seq<SKPaint> Edged(Painter.Pass pass, LayoutNode layout, Context context, Option<DataSeries> series, Figure figure, PlotFrame frame, Pen pen) {
        RoleStyle style = layout.Style;
        float weight = figure.Weight(frame, pen);
        uint ordinal = series.Map(static held => held.Ordinal).IfNone(0u);
        return style.Strokes.Map((stroke, index) => pass.Scene.Layout.Cascade.Theme.WithStateLayers(pass.Scene.ColorCache, stroke.Color, style, context.StateLayers, series: series).Map(color =>
            PaintCache.StrokePaint(pass.Cache.Edges, new PaintSlot(layout.Path, index), layout,
                new StrokeKey(color.WithAlpha(color.Alpha * weight), pen.Stroke, stroke.Patterned(ordinal), stroke.Timed ? pass.Clock : 0f, Some(style.Blend))))).Somes().Strict();
    }

    // --- [ATLAS]
    private static Seq<Sprite> Placed(Figure.Points points, PlotFrame frame, Pen pen) =>
        points.At.Map((at, index) => new Sprite(frame.Point(at), 0f,
            points.Marker.Map(static marker => marker.Size).IfNone(() => points.Sizes.Bind(sizes => sizes.At(index)).Map(size => size * pen.Icon).IfNone(pen.Stroke)),
            from values in points.Values
            from extent in values.Extent
            from value in values.At(index)
            select (value - extent.From) / extent.Span,
            points.Opacity.Bind(opacity => opacity.At(index)).IfNone(1f)));

    private static void Atlased(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, MarkKey key, SKColorF color, Seq<Sprite> placed) {
        float scale = key.Pen.Density;
        Option<Marker> marker = Markers(key.Figure).Head;
        float diameter = placed.Map(static sprite => sprite.Diameter).Fold(0f, float.Max);
        Option<SKImage> sheet = pass.Cache.Sprites.GetOrAdd(layout.Path, layout, new SpriteKey(marker, diameter, scale), () => Sheet(pass, layout, marker, diameter, scale));
        _ = sheet.Iter(image => canvas.DrawPicture(pass.Cache.Atlases.GetOrAdd(layout.Path, layout, new AtlasKey(key, color), () => {
            using SKPictureRecorder recorder = new();
            recorder.BeginRecording(Reach(placed.Map(static sprite => sprite.At), diameter)).DrawAtlas(image,
                [.. placed.Map(_ => SKRect.Create(image.Width, image.Height))],
                [.. placed.Map(sprite => SKRotationScaleMatrix.CreateDegrees(sprite.Diameter / (diameter * scale), sprite.Degrees, sprite.At.X, sprite.At.Y, image.Width / 2f, image.Height / 2f))],
                [.. placed.Map(sprite => (SKColor)Colored(layout.Style, color, sprite).WithAlpha(color.Alpha * sprite.Opacity))],
                SKBlendMode.Modulate, Smooth, paint: null);
            return recorder.EndRecording();
        })));
    }

    private static SKRect Reach(Seq<SKPoint> points, float radius) {
        SKRect bounds = points.Fold(new SKRect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity),
            static (held, at) => new SKRect(float.Min(held.Left, at.X), float.Min(held.Top, at.Y), float.Max(held.Right, at.X), float.Max(held.Bottom, at.Y)));
        return SKRect.Inflate(bounds, radius, radius);
    }

    private static SKColorF Colored(RoleStyle style, SKColorF color, Sprite sprite) =>
        (from value in sprite.Value
         from fraction in Fraction.Validated(value).ToOption()
         from ramp in style.Look.Scale
         from mapped in ramp.At(fraction)
         select mapped).IfNone(color);

    private static Option<SKImage> Sheet(Painter.Pass pass, LayoutNode layout, Option<Marker> marker, float diameter, float scale) {
        SKSizeI pixels = SKRectI.Ceiling(SKRect.Create(diameter * scale, diameter * scale), outwards: true).Size;
        return pass.Cache.Offscreen(new SKImageInfo(int.Max(1, pixels.Width), int.Max(1, pixels.Height), SKColorType.RgbaF16, SKAlphaType.Premul, pass.Cache.Space)).Map(surface => {
            using SKSurface held = surface;
            held.Canvas.Scale(scale);
            SKRect cell = SKRect.Create(diameter, diameter);
            _ = marker.Match(
                Some: shown => {
                    ArtKey art = Media.Keyed(layout, shown.Art, Styles.Background(layout.Cascade, layout.Style, layout.Node), pass.Scene.ColorCache);
                    _ = pass.Cache.Art(art).Iter(media => {
                        SKRect cull = media.Bounds;
                        SKRect fitted = ImageFit.Contain.Fitted(cell, cull.Size);
                        (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
                        media.Draw(held.Canvas, SKMatrix.CreateScaleTranslation(x, y, fitted.Left - (cull.Left * x), fitted.Top - (cull.Top * y)), art.Art.Tinted ? Some(pass.Cache.Tint(layout, SKColors.White)) : None);
                    });
                },
                None: () => {
                    using SKPaint disc = new() { IsAntialias = true, Color = SKColors.White };
                    held.Canvas.DrawCircle(diameter / 2f, diameter / 2f, diameter / 2f, disc);
                });
            return held.Snapshot();
        });
    }

    // --- [FIELDS]
    private static void Field(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, MarkKey key, Figure.Field field, Option<ColorRamp> scale, Option<SKColorF> lead) =>
        _ = pass.Cache.Fields.GetOrAdd(layout.Path, layout, new FieldKey(key, lead), () => Ramp(scale, lead).Map(ramp => Paved(ramp, field, key.Frame))).Iter(paint => {
            if (key.Frame.Plane.Separable) canvas.DrawPath(pass.Cache.Interiors.GetOrAdd(layout.Path, layout, key, () => Outline.Of(path => field.Interior(path, key.Frame, key.Pen))).Path, paint);
            else _ = pass.Cache.Meshes.GetOrAdd(layout.Path, layout, key, () => Meshes(field, key.Frame)).Iter(vertices => canvas.DrawVertices(vertices, SKBlendMode.Modulate, paint));
        });

    private static SKPaint Paved(SKImage ramp, Figure.Field field, PlotFrame frame) {
        using SKImage owned = ramp;
        using SKShader colors = ramp.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, Smooth);
        if (!frame.Plane.Separable) return new SKPaint { IsAntialias = true, Shader = colors };
        Half[] cells = [.. Fractions(field).Select(static fraction => (Half)(float.IsFinite(fraction) ? fraction : -1f))];
        using SKImage values = SKImage.FromPixelCopy(new SKImageInfo(field.Columns, field.Rows, SKColorType.AlphaF16, SKAlphaType.Premul), MemoryMarshal.AsBytes(cells.AsSpan()));
        using SKShader raw = values.ToRawShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, Nearest, frame.Matrix.PreConcat(SKMatrix.CreateScale(1f / field.Columns, 1f / field.Rows)));
        using SKRuntimeEffectUniforms uniforms = new(Lookup.Effect);
        using SKRuntimeEffectChildren children = new(Lookup.Effect) { ["values"] = raw, ["ramp"] = colors };
        using SKShader shader = Lookup.Effect.ToShader(uniforms, children);
        return new SKPaint { IsAntialias = true, Shader = shader };
    }

    private static float[] Fractions(Figure.Field field) {
        float[] fractions = field.Values.Values.ToArray();
        (float from, float span) = field.Values.Extent.Map(static range => (range.From, range.Span)).IfNone((0f, 0f));
        TensorPrimitives.Subtract(fractions, from, fractions);
        TensorPrimitives.Multiply(fractions, span > 0f ? 1f / span : 0f, fractions);
        return fractions;
    }

    private static Seq<SKVertices> Meshes(Figure.Field field, PlotFrame frame) {
        float[] fractions = Fractions(field);
        Seq<int> shown = toSeq(Enumerable.Range(0, fractions.Length)).Filter(index => float.IsFinite(fractions[index])).Strict();
        return toSeq(shown.Chunk(CellsPerMesh)).Map(cells => SKVertices.CreateCopy(SKVertexMode.Triangles,
            [.. cells.SelectMany(cell => Corners(field, frame, cell))],
            [.. cells.SelectMany(cell => Enumerable.Repeat(new SKPoint((fractions[cell] * (Steps - 1)) + 0.5f, 0.5f), 4))],
            colors: null,
            [.. from cell in Enumerable.Range(0, cells.Length) from corner in Quad select (ushort)((cell * 4) + corner)])).Strict();
    }

    private static Seq<SKPoint> Corners(Figure.Field field, PlotFrame frame, int cell) {
        (int row, int column) = int.DivRem(cell, field.Columns);
        (float left, float right) = ((float)column / field.Columns, (float)(column + 1) / field.Columns);
        (float top, float bottom) = ((float)row / field.Rows, (float)(row + 1) / field.Rows);
        return [frame.Projected(new Vector2(left, top)), frame.Projected(new Vector2(right, top)), frame.Projected(new Vector2(right, bottom)), frame.Projected(new Vector2(left, bottom))];
    }

    private static Option<SKImage> Ramp(Option<ColorRamp> scale, Option<SKColorF> lead) {
        Func<Fraction, Option<SKColorF>> at = scale.Match<Func<Fraction, Option<SKColorF>>>(
            Some: static ramp => ramp.At,
            None: () => fraction => lead.Map(color => color.WithAlpha(color.Alpha * (float)fraction)));
        Seq<Option<SKColorF>> stops = toSeq(Enumerable.Range(0, Steps)).Map(step => Fraction.Validated(step / (Steps - 1f)).ToOption().Bind(at)).Strict();
        return stops.Exists(static stop => stop.IsSome) ? Some(Lut(stops.Map(static stop => stop.IfNone(SKColors.Transparent)))) : None;
    }

    private static SKImage Lut(Seq<SKColorF> colors) {
        float[] channels = [.. colors.Bind(static color => Seq(color.Red, color.Green, color.Blue, color.Alpha))];
        return SKImage.FromPixelCopy(new SKImageInfo(colors.Count, 1, SKColorType.RgbaF32, SKAlphaType.Unpremul, Srgb), MemoryMarshal.AsBytes(channels.AsSpan()));
    }

    // --- [LABELS]
    internal static void Label(SKCanvas canvas, Painter.Pass pass, LayoutNode.Text layout, Context context, Figure.Label label, SKColorF color) =>
        _ = context.Plot.Iter(frame => {
            Pen pen = context.Pen(layout.Style);
            MarkKey key = new(label.Along, frame, pen);
            Outline along = pass.Cache.Contours.GetOrAdd(layout.Path, layout, key, () => Outline.Of(path => label.Along.Edges(path, frame, pen)));
            _ = pass.Cache.Labels.GetOrAdd(layout.Path, layout, new LabelKey(layout.Typeset, key), () => Typesetting.Along(layout.Typeset, along.Path, label.Offset))
                .Iter(blob => canvas.DrawText(blob, 0f, 0f, pass.Cache.Solid(layout, color)));
        });

    // --- [IMAGES]
    internal static void Image(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, Context context, Node.Image image) {
        RoleStyle style = layout.Style;
        SKRect content = layout.ContentBox;
        _ = pass.Cache.Frame(layout, image.Source, pass.Now).Iter(source => {
            SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Info.Width, image.Source.Info.Height));
            SKRect device = context.Device.MapRect(target);
            bool alphaOnly = source.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16;
            Option<SKColorF> color = alphaOnly && image.Scale.IsNone
                ? (style.Strokes.Head.Map(static stroke => stroke.Color) | style.Solid).Bind(value => pass.Scene.Layout.Cascade.Theme.WithStateLayers(pass.Scene.ColorCache, value, style, context.StateLayers, series: image.Series))
                : None;
            SKPaint paint = pass.Cache.Images.GetOrAdd(layout.Path, layout, new ImagePaintKey(color, image.Scale), () => Imaged(pass.Cache, style, color, image.Scale, alphaOnly));
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRect(content, antialias: true);
            canvas.DrawImage(source, target, image.Rendering.Sampling(MathF.Min(device.Width / source.Width, device.Height / source.Height)), paint);
        });
    }

    private static SKPaint Imaged(PaintCache cache, RoleStyle style, Option<SKColorF> color, Option<ColorRamp> scale, bool alphaOnly) {
        using SKColorFilter? table = scale.Bind(ramp => Tabled(cache, ramp, alphaOnly)).ValueUnsafe();
        return new SKPaint { IsAntialias = true, Blender = style.Blend, ColorF = color.IfNone(SKColors.Black), ColorFilter = table };
    }

    private static Option<SKColorFilter> Tabled(PaintCache cache, ColorRamp ramp, bool alphaOnly) {
        using SKColorFilter valued = SKColorFilter.CreateColorMatrix(alphaOnly
            ? [0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f]
            : [1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f]);
        return Ramp(Some(ramp), None).Map(lut => {
            using SKImage owned = lut;
            using SKColorFilter mapped = cache.Precision is SKCanvasSaveLayerRecFlags.None ? Looked(lut) : Tabulated(ramp);
            return SKColorFilter.CreateCompose(mapped, valued);
        });
    }

    private static SKColorFilter Looked(SKImage lut) {
        using SKShader colors = lut.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, Smooth);
        using SKRuntimeEffectUniforms uniforms = new(Table.Effect);
        using SKRuntimeEffectChildren children = new(Table.Effect) { ["ramp"] = colors };
        return Table.Effect.ToColorFilter(uniforms, children);
    }

    private static SKColorFilter Tabulated(ColorRamp ramp) {
        Seq<SKColor> stops = toSeq(Enumerable.Range(0, Steps)).Map(step => Fraction.Validated(step / (Steps - 1f)).ToOption().Bind(ramp.At).Map(static stop => (SKColor)stop).IfNone(SKColors.Transparent)).Strict();
        return SKColorFilter.CreateTable([.. stops.Map(static stop => stop.Alpha)], [.. stops.Map(static stop => stop.Red)], [.. stops.Map(static stop => stop.Green)], [.. stops.Map(static stop => stop.Blue)]);
    }
}
