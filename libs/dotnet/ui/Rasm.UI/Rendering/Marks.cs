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
    // --- [EFFECTS]
    internal const int Steps = byte.MaxValue + 1;
    private const int CellsPerMesh = ushort.MaxValue / 4;

    private static readonly int[] Quad = [0, 1, 2, 0, 2, 3];

    private static readonly SKRuntimeShaderBuilder Lookup = SKRuntimeEffect.BuildShader($$"""
        uniform shader values;
        uniform shader ramp;
        half4 main(float2 p) {
            half v = values.eval(p).a;
            return v < 0.0 ? half4(0.0) : ramp.eval(float2((v * {{Steps - 1}}.0) + 0.5, 0.5));
        }
        """);
    private static readonly SKRuntimeColorFilterBuilder Table = SKRuntimeEffect.BuildColorFilter($$"""
        uniform shader ramp;
        half4 main(half4 c) { return ramp.eval(float2((c.a * {{Steps - 1}}.0) + 0.5, 0.5)); }
        """);

    // --- [MARKS]
    internal static void Mark(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, Context context, Figure figure, PlotFrame frame) {
        RoleStyle style = layout.Style;
        PaintCache cache = pass.Cache;
        Pen pen = context.Pen(style);
        MarkKey key = new(figure, frame, pen);
        Option<SKColorF> lead = Lead(layout, context);
        if (figure is Figure.Field field) {
            Field(canvas, pass, layout, key, field, lead);
            return;
        }
        if (figure is Figure.Points { Marker.IsNone: true, Sizes.IsNone: true, Values.IsNone: true, Opacity.IsNone: true } dots) {
            Seq<SKPaint> paints = Edged(pass, layout, context, figure, frame, pen);
            canvas.DrawPicture(cache.Dots.GetOrAdd((key, paints), () => {
                Seq<SKPoint> points = dots.At.Map(frame.Point).Strict();
                using SKPictureRecorder recorder = new();
                SKCanvas recording = recorder.BeginRecording(Reach(points, pen.Stroke));
                _ = paints.Iter(paint => recording.DrawPoints(SKPointMode.Points, [.. points], paint));
                return recorder.EndRecording();
            }));
            return;
        }
        if (figure is Figure.Points sprites) {
            _ = lead.Iter(color => Sprites(canvas, pass, layout, key, color, sprites.Marker, Placed(sprites, frame, pen)));
            return;
        }
        if (!style.Fills.IsEmpty) {
            Outline.Traced interior = cache.Interiors.GetOrAdd(key, () => Outline.Of(path => figure.Interior(path, frame, pen)));
            Seq<Action<SKCanvas>> fills = style.Fills.Map((fill, index) => Fills.Fill(new FillContext(pass, layout, context, interior.Path.Bounds, index) { Blend = Some(style.Blend) }, fill, interior)).Somes().Strict();
            using SKAutoCanvasRestore covered = new(canvas);
            _ = fills.Iter(fill => fill(canvas));
        }
        Outline.Traced edge = cache.Contours.GetOrAdd(key, () => Outline.Of(path => figure.Edges(path, frame, pen)));
        _ = Edged(pass, layout, context, figure, frame, pen).Iter(paint => canvas.DrawPath(edge.Path, paint));
        Seq<(Marker Marker, SKPoint At, float Degrees)> placements = figure.Markers(edge.Path);
        _ = lead.Iter(color => toSeq(placements.GroupBy(static placed => placed.Marker)).Iter(group =>
            Sprites(canvas, pass, layout, key, color, Some(group.Key), toSeq(group).Map(static placed => (placed.At, placed.Degrees, Diameter: placed.Marker.Size, Value: Option<float>.None, Opacity: 1f)))));
    }

    private static Option<SKColorF> Lead(LayoutNode layout, Context context) =>
        (layout.Style.Strokes.Head.Map(static stroke => stroke.Color) | layout.Style.Solid).Bind(value => context.Color(value, layout));

    private static Seq<SKPaint> Edged(Painter.Pass pass, LayoutNode layout, Context context, Figure figure, PlotFrame frame, Pen pen) {
        float weight = figure.Weight(frame, pen);
        uint ordinal = layout.Node.Series.Map(static held => held.Ordinal).IfNone(0u);
        return layout.Style.Strokes.Choose(stroke => context.Color(stroke.Color, layout)
            .Map(color => pass.Cache.Stroke(stroke, color.WithAlpha(color.Alpha * weight), pen.Stroke, ordinal, pass.Clock, Some(layout.Style.Blend)))).Strict();
    }

    // --- [SPRITES]
    private static Seq<(SKPoint At, float Degrees, float Diameter, Option<float> Value, float Opacity)> Placed(Figure.Points points, PlotFrame frame, Pen pen) =>
        points.At.Map((at, index) => (At: frame.Point(at), Degrees: 0f,
            Diameter: points.Marker.Map(static marker => marker.Size).IfNone(() => points.Sizes.Bind(sizes => sizes.At(index)).Map(size => size * pen.Icon).IfNone(pen.Stroke)),
            Value: from values in points.Values
                   from extent in values.Extent
                   from value in values.At(index)
                   select (value - extent.From) / extent.Span,
            Opacity: points.Opacity.Bind(opacity => opacity.At(index)).IfNone(1f)));

    private static void Sprites(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, MarkKey key, SKColorF color, Option<Marker> marker,
        Seq<(SKPoint At, float Degrees, float Diameter, Option<float> Value, float Opacity)> placed) {
        float scale = key.Pen.Density;
        float diameter = placed.Map(static sprite => sprite.Diameter).Fold(0f, float.Max);
        Option<SKImage> sheet = pass.Cache.Sprites.GetOrAdd((layout, marker, diameter, scale), () => Sheet(pass, layout, marker, diameter, scale));
        _ = sheet.Iter(image => canvas.DrawPicture(pass.Cache.Atlases.GetOrAdd((key, marker, color), () => {
            using SKPictureRecorder recorder = new();
            recorder.BeginRecording(Reach(placed.Map(static sprite => sprite.At), diameter)).DrawAtlas(image,
                [.. placed.Map(_ => SKRect.Create(image.Width, image.Height))],
                [.. placed.Map(sprite => SKRotationScaleMatrix.CreateDegrees(sprite.Diameter / (diameter * scale), sprite.Degrees, sprite.At.X, sprite.At.Y, image.Width / 2f, image.Height / 2f))],
                [.. placed.Map(sprite => (SKColor)Colored(layout, color, sprite.Value).WithAlpha(color.Alpha * sprite.Opacity))],
                SKBlendMode.Modulate, new SKSamplingOptions(SKFilterMode.Linear), paint: null);
            return recorder.EndRecording();
        })));
    }

    private static SKRect Reach(Seq<SKPoint> points, float radius) {
        SKRect bounds = points.Fold(new SKRect(float.PositiveInfinity, float.PositiveInfinity, float.NegativeInfinity, float.NegativeInfinity),
            static (held, at) => new SKRect(float.Min(held.Left, at.X), float.Min(held.Top, at.Y), float.Max(held.Right, at.X), float.Max(held.Bottom, at.Y)));
        return SKRect.Inflate(bounds, radius, radius);
    }

    private static SKColorF Colored(LayoutNode layout, SKColorF color, Option<float> value) =>
        (from at in value
         from fraction in Fraction.Validated(at).ToOption()
         from ramp in layout.Style.Look.Scale
         from stop in ramp.At(fraction)
         from mapped in layout.Cascade.Theme.Color(stop, layout.Style)
         select mapped).IfNone(color);

    private static Option<SKImage> Sheet(Painter.Pass pass, LayoutNode layout, Option<Marker> marker, float diameter, float scale) =>
        pass.Cache.Rasterized(new SKSize(diameter, diameter), scale, SKColorType.RgbaF16, canvas => _ = marker.Match(
            Some: shown => pass.Cache.Art(layout, shown.Art).Iter(media => media.Draw(canvas, media.Fitted(SKRect.Create(diameter, diameter)), shown.Art.Tinted ? Some(pass.Cache.Tint(media, SKColors.White)) : None)),
            None: () => {
                using SKPaint disc = new() { IsAntialias = true, Color = SKColors.White };
                canvas.DrawCircle(diameter / 2f, diameter / 2f, diameter / 2f, disc);
                return unit;
            }));

    // --- [FIELDS]
    private static void Field(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, MarkKey key, Figure.Field field, Option<SKColorF> lead) =>
        _ = pass.Cache.Fields.GetOrAdd((key, lead), () => Ramp(layout.Style.Look.Scale.Match<Func<Fraction, Option<SKColorF>>>(
                Some: scale => fraction => scale.At(fraction).Bind(stop => layout.Cascade.Theme.Color(stop, layout.Style)),
                None: () => fraction => lead.Map(color => color.WithAlpha(color.Alpha * (float)fraction))))
            .Map(ramp => FieldPaint(ramp, field, key.Frame))).Iter(paint => {
                if (key.Frame.Plane.Separable) canvas.DrawPath(pass.Cache.Interiors.GetOrAdd(key, () => Outline.Of(path => field.Interior(path, key.Frame, key.Pen))).Path, paint);
                else _ = pass.Cache.Meshes.GetOrAdd(key, () => Meshes(field, key.Frame)).Iter(vertices => canvas.DrawVertices(vertices, SKBlendMode.Modulate, paint));
            });

    private static SKPaint FieldPaint(SKImage ramp, Figure.Field field, PlotFrame frame) {
        using SKImage owned = ramp;
        using SKShader colors = owned.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear));
        if (!frame.Plane.Separable) return new SKPaint { IsAntialias = true, Shader = colors };
        float[] fractions = Fractions(field);
        foreach (ref float fraction in fractions.AsSpan())
            if (!float.IsFinite(fraction)) fraction = -1f;
        Half[] cells = new Half[fractions.Length];
        TensorPrimitives.ConvertToHalf(fractions, cells);
        using SKImage values = SKImage.FromPixelCopy(new SKImageInfo(field.Columns, field.Rows, SKColorType.AlphaF16, SKAlphaType.Premul), MemoryMarshal.AsBytes(cells.AsSpan()));
        using SKShader raw = values.ToRawShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Nearest), frame.Matrix.PreConcat(SKMatrix.CreateScale(1f / field.Columns, 1f / field.Rows)));
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
        Seq<int> shown = toSeq(Range(0, fractions.Length)).Filter(index => float.IsFinite(fractions[index])).Strict();
        return toSeq(shown.Chunk(CellsPerMesh)).Map(cells => SKVertices.CreateCopy(SKVertexMode.Triangles,
            [.. cells.SelectMany(cell => Vertices(field, frame, cell))],
            [.. cells.SelectMany(cell => Enumerable.Repeat(new SKPoint((fractions[cell] * (Steps - 1)) + 0.5f, 0.5f), 4))],
            colors: null,
            [.. from cell in Enumerable.Range(0, cells.Length) from corner in Quad select (ushort)((cell * 4) + corner)])).Strict();
    }

    private static Seq<SKPoint> Vertices(Figure.Field field, PlotFrame frame, int cell) {
        (int row, int column) = int.DivRem(cell, field.Columns);
        (float left, float right) = ((float)column / field.Columns, (float)(column + 1) / field.Columns);
        (float top, float bottom) = ((float)row / field.Rows, (float)(row + 1) / field.Rows);
        return [frame.Projected(new Vector2(left, top)), frame.Projected(new Vector2(right, top)), frame.Projected(new Vector2(right, bottom)), frame.Projected(new Vector2(left, bottom))];
    }

    private static Option<SKImage> Ramp(Func<Fraction, Option<SKColorF>> at) {
        Seq<Option<SKColorF>> stops = toSeq(Range(0, Steps)).Map(step => Fraction.Validated(step / (Steps - 1f)).ToOption().Bind(at)).Strict();
        if (!stops.Exists(static stop => stop.IsSome)) return None;
        float[] channels = [.. stops.Map(static stop => stop.IfNone(SKColors.Transparent)).Bind(static color => Seq(color.Red, color.Green, color.Blue, color.Alpha))];
        return SKImage.FromPixelCopy(new SKImageInfo(Steps, 1, SKColorType.RgbaF32, SKAlphaType.Unpremul, SKColorSpace.CreateSrgb()), MemoryMarshal.AsBytes(channels.AsSpan()));
    }

    // --- [LABELS]
    internal static void Label(SKCanvas canvas, Painter.Pass pass, LayoutNode.Text layout, Context context, Figure.Label label, SKColorF color) =>
        _ = context.Plot.Iter(frame => {
            Pen pen = context.Pen(layout.Style);
            MarkKey key = new(label.Along, frame, pen);
            Outline.Traced along = pass.Cache.Contours.GetOrAdd(key, () => Outline.Of(path => label.Along.Edges(path, frame, pen)));
            _ = pass.Cache.Labels.GetOrAdd((layout.Typeset, key), () => Typesetting.Along(layout.Typeset, along.Path, label.Offset))
                .Iter(blob => canvas.DrawText(blob, 0f, 0f, pass.Cache.Solid(color)));
        });

    // --- [IMAGES]
    internal static void Image(SKCanvas canvas, Painter.Pass pass, LayoutNode layout, Context context, Node.Image image) {
        SKRect content = layout.ContentBox;
        _ = pass.Cache.Frame(layout, image, pass.Now).Iter(source => {
            SKRect target = image.Fit.Fitted(content, new SKSize(image.Source.Info.Width, image.Source.Info.Height));
            SKRect device = context.Device.MapRect(target);
            bool alphaOnly = source.ColorType is SKColorType.Alpha8 or SKColorType.Alpha16 or SKColorType.AlphaF16;
            Option<SKColorF> color = alphaOnly && image.Scale.IsNone ? Lead(layout, context) : None;
            SKPaint paint = pass.Cache.Images.GetOrAdd((layout, color, alphaOnly), () => {
                using SKColorFilter? ramp = image.Scale.Bind(scale => RampFilter(layout, scale, alphaOnly)).ValueUnsafe();
                return new SKPaint { IsAntialias = true, Blender = layout.Style.Blend, ColorF = color.IfNone(SKColors.Black), ColorFilter = ramp };
            });
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRect(content, antialias: true);
            canvas.DrawImage(source, target, image.Rendering.Sampling(MathF.Min(device.Width / source.Width, device.Height / source.Height)), paint);
        });
    }

    private static Option<SKColorFilter> RampFilter(LayoutNode layout, ColorRamp scale, bool alphaOnly) =>
        Ramp(fraction => scale.At(fraction).Bind(stop => layout.Cascade.Theme.Color(stop, layout.Style))).Map(lut => {
            using SKImage owned = lut;
            using SKShader colors = owned.ToShader(SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, new SKSamplingOptions(SKFilterMode.Linear));
            using SKRuntimeEffectUniforms uniforms = new(Table.Effect);
            using SKRuntimeEffectChildren children = new(Table.Effect) { ["ramp"] = colors };
            using SKColorFilter mapped = Table.Effect.ToColorFilter(uniforms, children);
            using SKColorFilter valued = SKColorFilter.CreateColorMatrix(alphaOnly
                ? [0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f]
                : [1f, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 1f, 0f, 0f, 0f, 0f]);
            return SKColorFilter.CreateCompose(mapped, valued);
        });
}
