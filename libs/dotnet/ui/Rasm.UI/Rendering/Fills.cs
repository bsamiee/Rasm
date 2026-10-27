using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record FillContext(Painter.Pass Pass, LayoutNode Layout, Context Context, SKRect Frame, int Index) {
    internal Option<SKBlender> Blend { get; init; }
    internal Option<DataSeries> Series { get; init; }

    internal Option<SKColorF> Color(ColorValue value) => Context.Color(value, Layout, Layout.Style.Look.StateLayers, Series);

    internal float Metric(Metric metric) => Layout.Style.Metrics[metric];

    internal FillKey Key(float scale, float clock, Option<SKPoint> pointer) => new(Layout, Index, Context.StateLayers, Frame, scale, clock, pointer);

    internal FilterTarget Target => new(Pass.Cache.Space, Frame, Color, Metric, material => Fills.Shader(this, material)) { Corners = Layout.Corners, Clock = Pass.Clock, Pointer = Pass.PointerAt(Layout) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Fills {
    // --- [FILLS]
    internal static Option<Action<SKCanvas>> Fill(FillContext held, Material material, Outline shape) =>
        material.SwitchPartially(
            (Held: held, Shape: shape),
            @default: static (state, material) => state.Held.Pass.Cache.Brushes.GetOrAdd(state.Held.Key(0f, 0f, None), () => Shader(state.Held, material).Map(shader => Shaded(state.Held, shader, dither: material is Material.Gradient)))
                .Map(paint => Filled(state.Shape, paint)),
            drawn: static (state, drawn) => (drawn.Timed || drawn.Pointing) switch {
                var live => state.Held.Pass.Cache.Brushes.GetOrAdd(
                        live ? state.Held.Key(0f, drawn.Timed ? state.Held.Pass.Clock : 0f, Pointer(state.Held, drawn)) : state.Held.Key(state.Held.Context.Scale, 0f, None),
                        () => (live
                            ? Shader(state.Held, drawn)
                            : Shader(state.Held with { Frame = SKRect.Create(state.Held.Frame.Size) }, drawn).Bind(shader => state.Held.Pass.Cache.Rastered(shader, state.Held.Frame, state.Held.Context.Scale)))
                            .Map(shader => Shaded(state.Held, shader, dither: false)))
                    .Map(paint => Filled(state.Shape, paint)),
            },
            glass: static (state, glass) => Some<Action<SKCanvas>>(canvas => Glass(canvas, state.Held, glass, state.Shape)),
            hatch: static (state, hatch) => state.Held.Pass.Cache.Brushes.GetOrAdd(state.Held.Key(0f, 0f, None), () => state.Held.Color(hatch.Color).Map(color => {
                using SKPathEffect lines = hatch.Effect();
                return new SKPaint { IsAntialias = true, ColorF = color, PathEffect = lines, Blender = state.Held.Blend.ValueUnsafe() };
            }))
                .Map(paint => Clipped(state.Shape, canvas => state.Shape.Draw(canvas, paint))),
            skin: static (state, skin) => state.Held.Pass.Cache.Brushes.GetOrAdd(state.Held.Key(0f, 0f, None), () => Some(new SKPaint { IsAntialias = true, Blender = state.Held.Blend.ValueUnsafe() }))
                .Map(paint => Clipped(state.Shape, canvas => canvas.DrawImageNinePatch(skin.Source, skin.Center, state.Held.Frame, SKFilterMode.Linear, paint))));

    internal static Option<SKShader> Shader(FillContext held, Material material) =>
        material.SwitchPartially(
            held,
            @default: static (_, _) => Option<SKShader>.None,
            solid: static (state, solid) => state.Color(solid.Color).Map(static color => SKShader.CreateColor(color, SKColorSpace.CreateSrgb())),
            gradient: static (state, gradient) => gradient.Shader(state.Frame, state.Color),
            drawn: static (state, drawn) => Shader(state, drawn),
            noise: static (state, noise) => Some(noise.Shader(state.Frame)),
            image: static (state, image) => Some(image.Shader(state.Frame)),
            art: static (state, art) => art.Shader(state.Frame, glyph => state.Pass.Cache.Art(state.Layout, glyph).Bind(static media => media.Still)));

    // --- [GLASS]
    private static readonly SKRuntimeShaderBuilder Matte = SKRuntimeEffect.BuildShader("""
        uniform float2 size;
        uniform float4 corners;
        uniform float smoothing;
        half4 main(float2 p) {
            float2 c = p - (size * 0.5);
            float r = c.x > 0.0 ? (c.y > 0.0 ? corners.z : corners.y) : (c.y > 0.0 ? corners.w : corners.x);
            float2 q = abs(c) - (size * 0.5) + r;
            float2 m = max(q, 0.0);
            float k = 2.0 + (2.0 * smoothing);
            float d = pow(pow(m.x, k) + pow(m.y, k), 1.0 / k) + min(max(q.x, q.y), 0.0) - r;
            float2 g = (q.x > 0.0 && q.y > 0.0) ? pow(m, float2(k - 1.0)) : (q.x > q.y ? float2(1.0, 0.0) : float2(0.0, 1.0));
            float rim = max(max(max(corners.x, corners.y), max(corners.z, corners.w)), 1.0);
            float2 shift = normalize(g) * sign(c) * (1.0 - smoothstep(0.0, rim, -d));
            return half4(half2(0.5 - (0.5 * shift)), 0.5, 1.0);
        }
        """);

    private static void Glass(SKCanvas canvas, FillContext held, Material.Glass glass, Outline shape) {
        Option<SKImageFilter> backdrop = held.Pass.Cache.Backdrops.GetOrAdd(held.Key(held.Context.Scale, glass.Timed ? held.Pass.Clock : 0f, glass.Pointing ? held.Pass.PointerAt(held.Layout) : None), () => Backdrop(held, glass));
        shape.Clip(canvas, SKClipOperation.Intersect);
        _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = held.Frame, Backdrop = backdrop.ValueUnsafe(), Flags = held.Pass.Cache.Precision });
    }

    private static Option<SKImageFilter> Backdrop(FillContext held, Material.Glass glass) {
        Option<SKImageFilter> chained = Filter.Chained(glass.Backdrop, held.Target);
        return (from refraction in glass.Refraction
                from map in held.Pass.Cache.Mattes.GetOrAdd((held.Frame.Size, held.Layout.Corners, held.Layout.Smoothing, held.Context.Scale), () => Displacement(held))
                select Displaced(map, refraction, held.Frame, chained))
            | chained;
    }

    private static Option<SKImage> Displacement(FillContext held) {
        Corners corners = held.Layout.Corners;
        using SKRuntimeEffectUniforms uniforms = new(Matte.Effect) {
            ["size"] = held.Frame.Size, ["corners"] = new float[] { corners.TopLeft, corners.TopRight, corners.BottomRight, corners.BottomLeft }, ["smoothing"] = held.Layout.Smoothing,
        };
        using SKRuntimeEffectChildren children = new(Matte.Effect);
        using SKShader shader = Matte.Effect.ToShader(uniforms, children);
        using SKPaint paint = new() { Shader = shader };
        return held.Pass.Cache.Rasterized(held.Frame.Size, held.Context.Scale, SKColorType.RgbaF16, canvas => canvas.DrawRect(SKRect.Create(held.Frame.Size), paint));
    }

    private static SKImageFilter Displaced(SKImage map, float refraction, SKRect frame, Option<SKImageFilter> chained) {
        using SKImageFilter field = SKImageFilter.CreateImage(map, SKRect.Create(map.Width, map.Height), frame, new SKSamplingOptions(SKFilterMode.Linear));
        SKImageFilter displaced = SKImageFilter.CreateDisplacementMapEffect(SKColorChannel.R, SKColorChannel.G, refraction, field, chained.ValueUnsafe());
        _ = chained.Iter(static held => held.Dispose());
        return displaced;
    }

    // --- [PAINTS]
    private static SKPaint Shaded(FillContext held, SKShader shader, bool dither) {
        using SKShader owned = shader;
        return new SKPaint { IsAntialias = true, Shader = owned, IsDither = dither, Blender = held.Blend.ValueUnsafe() };
    }

    private static Option<SKShader> Shader(FillContext held, Material.Drawn drawn) =>
        drawn.Shader(held.Frame, held.Color, held.Metric, material => Shader(held, material), held.Layout.Corners, drawn.Timed ? held.Pass.Clock : 0f, Pointer(held, drawn));

    private static Option<SKPoint> Pointer(FillContext held, Material.Drawn drawn) => drawn.Pointing ? held.Pass.PointerAt(held.Layout) : None;

    private static Action<SKCanvas> Filled(Outline shape, SKPaint paint) => canvas => shape.Draw(canvas, paint);

    private static Action<SKCanvas> Clipped(Outline shape, Action<SKCanvas> draw) =>
        canvas => {
            using SKAutoCanvasRestore restore = new(canvas);
            shape.Clip(canvas, SKClipOperation.Intersect);
            draw(canvas);
        };
}
