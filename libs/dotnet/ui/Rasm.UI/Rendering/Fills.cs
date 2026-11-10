using Rasm.UI.Layout;
using Rasm.UI.Media;
using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------

internal sealed record FillKey(LayoutNode Layout, int Index, HashMap<Interaction, float> StateLayers, SKRect Frame, float Scale, float Clock, Option<SKPoint> Pointer);

internal sealed record FillContext(Painter.Pass Pass, LayoutNode Layout, Context Context, SKRect Frame, int Index) {
    internal Option<SKBlender> Blend { get; init; }

    internal Option<SKColorF> Color(ColorValue value) => Context.Color(value, Layout, Layout.Style.Look.StateLayers);

    internal FillKey Key(float scale, bool timed, bool pointing) => new(Layout, Index, Context.StateLayers, Frame, scale, timed ? Pass.Clock : 0f, pointing ? Pass.PointerAt(Layout) : None);

    internal PaintTarget Target =>
        new(Pass.Cache.Space, Frame, Color, metric => Layout.Style.Metrics[metric], glyph => Pass.Cache.Art(Layout, glyph).Bind(static media => media.Still)) {
            Corners = Layout.Corners, Clock = Pass.Clock, Pointer = Pass.PointerAt(Layout),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class Fills {

    // --- [FILLS]

    internal static Option<Action<SKCanvas>> Fill(FillContext held, Material material, Outline shape) =>
        material is Material.Glass glass
            ? Some<Action<SKCanvas>>(canvas => Glass(canvas, held, glass, shape))
            : held.Pass.Cache.Brushes.GetOrAdd(held.Key(material is Material.Drawn { Timed: false, Pointing: false } ? held.Context.Scale : 0f, material.Timed, material.Pointing), () => Brush(held, material))
                .Map(paint => material.SwitchPartially<(Outline Shape, SKPaint Paint, SKRect Frame), Action<SKCanvas>>(
                    (shape, paint, held.Frame),
                    @default: static (state, _) => canvas => state.Shape.Draw(canvas, state.Paint),
                    hatch: static (state, _) => Clipped(state.Shape, canvas => state.Shape.Draw(canvas, state.Paint)),
                    skin: static (state, skin) => Clipped(state.Shape, canvas => canvas.DrawImageNinePatch(skin.Source, skin.Center, state.Frame, SKFilterMode.Linear, state.Paint))));

    private static Option<SKPaint> Brush(FillContext held, Material material) =>
        material.SwitchPartially(
            held,
            @default: static (state, material) => material.Shader(state.Target).Map(shader => Shaded(state, shader, dither: material is Material.Gradient)),
            drawn: static (state, drawn) => (drawn.Timed || drawn.Pointing
                    ? drawn.Shader(state.Target)
                    : drawn.Shader((state with { Frame = SKRect.Create(state.Frame.Size) }).Target).Bind(shader => state.Pass.Cache.Rastered(shader, state.Frame, state.Context.Scale)))
                .Map(shader => Shaded(state, shader, dither: false)),
            hatch: static (state, hatch) => state.Color(hatch.Color).Map(color => {
                using SKPathEffect lines = SKPathEffect.Create2DLine(hatch.Width, SKMatrix.CreateScale(hatch.Spacing, hatch.Spacing).PostConcat(SKMatrix.CreateRotationDegrees(hatch.Angle)));
                return new SKPaint { IsAntialias = true, ColorF = color, PathEffect = lines, Blender = state.Blend.ValueUnsafe() };
            }),
            skin: static (state, _) => Some(new SKPaint { IsAntialias = true, Blender = state.Blend.ValueUnsafe() }));

    // --- [GLASS]

    private const string Edge = """
        uniform float2 size;
        uniform float4 corners;
        uniform float smoothing;
        float3 edge(float2 p) {
            float2 c = p - (size * 0.5);
            float r = c.x > 0.0 ? (c.y > 0.0 ? corners.z : corners.y) : (c.y > 0.0 ? corners.w : corners.x);
            float2 q = abs(c) - (size * 0.5) + r;
            float2 m = max(q, 0.0);
            float k = 2.0 + (2.0 * smoothing);
            float d = pow(pow(m.x, k) + pow(m.y, k), 1.0 / k) + min(max(q.x, q.y), 0.0) - r;
            float2 g = (q.x > 0.0 && q.y > 0.0) ? pow(m, float2(k - 1.0)) : (q.x > q.y ? float2(1.0, 0.0) : float2(0.0, 1.0));
            return float3(normalize(g) * sign(c), d);
        }
        """;

    private static readonly SKRuntimeShaderBuilder Matte = SKRuntimeEffect.BuildShader(Edge + """
        half4 main(float2 p) {
            float3 e = edge(p);
            float rim = max(max(max(corners.x, corners.y), max(corners.z, corners.w)), 1.0);
            float2 shift = e.xy * (1.0 - smoothstep(0.0, rim, -e.z));
            return half4(half2(0.5 - (0.5 * shift)), 0.5, 1.0);
        }
        """);

    private static readonly SKRuntimeShaderBuilder Rim = SKRuntimeEffect.BuildShader(Edge + """
        half4 main(float2 p) {
            float2 n = edge(p).xy;
            return half4(half(n.y * n.y), 0.0, 0.0, 1.0);
        }
        """);

    private static readonly SKRuntimeBlenderBuilder Lightness = SKRuntimeEffect.BuildBlender("""
        uniform float2 side;
        uniform float2 end;
        half4 main(half4 src, half4 dst) {
            float3 c = float3(toLinearSrgb(dst.a > 0.0 ? dst.rgb / dst.a : half3(0.0)));
            float3 lms = pow(max(float3(dot(float3(0.4122214708, 0.5363325363, 0.0514459929), c), dot(float3(0.2119034982, 0.6806995451, 0.1073969566), c), dot(float3(0.0883024619, 0.2817188376, 0.6299787005), c)), 0.0), float3(1.0 / 3.0));
            float3 lab = float3(dot(float3(0.2104542553, 0.7936177850, -0.0040720468), lms), dot(float3(1.9779984951, -2.4285922050, 0.4505937099), lms), dot(float3(0.0259040371, 0.7827717662, -0.8086757660), lms));
            float2 curve = mix(side, end, float(src.r));
            float lightness = curve.x * pow(max(lab.x, 0.0), curve.y);
            float3 shifted = float3(lightness, lab.yz * (lab.x > 0.0 ? lightness / lab.x : 0.0));
            float3 root = float3(dot(float3(1.0, 0.3963377774, 0.2158037573), shifted), dot(float3(1.0, -0.1055613458, -0.0638541728), shifted), dot(float3(1.0, -0.0894841775, -1.2914855480), shifted));
            float3 cone = root * root * root;
            float3 rgb = float3(dot(float3(4.0767416621, -3.3077115913, 0.2309699292), cone), dot(float3(-1.2684380046, 2.6097574011, -0.3413193965), cone), dot(float3(-0.0041960863, -0.7034186147, 1.7076147010), cone));
            return half4(fromLinearSrgb(half3(rgb)) * dst.a, dst.a);
        }
        """);

    private static void Glass(SKCanvas canvas, FillContext held, Material.Glass glass, Outline shape) {
        _ = held.Pass.Cache.Brushes.GetOrAdd(held.Key(held.Context.Scale, timed: false, pointing: false), () => Rimmed(held, glass)).Iter(paint => {
            using SKAutoCanvasRestore outside = new(canvas);
            shape.Clip(canvas, SKClipOperation.Difference);
            _ = held.Pass.Outlined(held.Layout, held.Frame, Material.Glass.Band(held.Context.Scale)).Iter(band => band.Draw(canvas, paint));
        });
        Option<SKImageFilter> backdrop = held.Pass.Cache.Backdrops.GetOrAdd(held.Key(held.Context.Scale, glass.Timed, glass.Pointing), () => Filter.Chained(glass.Backdrop + (
            from refraction in glass.Refraction
            from map in held.Pass.Cache.Mattes.GetOrAdd((held.Frame.Size, held.Layout.Corners, held.Layout.Smoothing, held.Context.Scale), () => Displacement(held))
            select (Filter)new Filter.Displace(new Material.Image(map), refraction)).ToSeq(), held.Target));
        shape.Clip(canvas, SKClipOperation.Intersect);
        _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Backdrop = backdrop.ValueUnsafe(), Flags = held.Pass.Cache.Precision });
    }

    private static SKShader Edged(SKRuntimeEffect effect, FillContext held, SKMatrix local) {
        using SKRuntimeEffectUniforms uniforms = new(effect) { ["size"] = held.Frame.Size, ["corners"] = held.Layout.Corners.Clockwise.ToArray(), ["smoothing"] = held.Layout.Smoothing };
        using SKRuntimeEffectChildren children = new(effect);
        return effect.ToShader(uniforms, children, local);
    }

    private static Option<SKImage> Displacement(FillContext held) {
        using SKShader shader = Edged(Matte.Effect, held, SKMatrix.Identity);
        using SKPaint paint = new() { Shader = shader };
        return held.Pass.Cache.Rasterized(held.Frame.Size, held.Context.Scale, SKColorType.RgbaF16, canvas => canvas.DrawRect(SKRect.Create(held.Frame.Size), paint));
    }

    private static SKPaint Rimmed(FillContext held, Material.Glass glass) {
        (SKPoint side, SKPoint end) = (glass.Clear, held.Layout.Style.Conditions.Contains(Condition.Dark)) switch {
            (false, false) => (new SKPoint(0.8266f, 1.2402f), new SKPoint(0.8822f, 1.1543f)),
            (false, true) => (new SKPoint(0.3867f, 0.4890f), new SKPoint(0.5761f, 0.7615f)),
            (true, false) => (new SKPoint(0.7731f, 1.3750f), new SKPoint(1f, 1f)),
            (true, true) => (new SKPoint(0.6434f, 1.1787f), new SKPoint(1f, 1f)),
        };
        using SKShader shader = Edged(Rim.Effect, held, SKMatrix.CreateTranslation(held.Frame.Left, held.Frame.Top));
        using SKRuntimeEffectUniforms curves = new(Lightness.Effect) { ["side"] = side, ["end"] = end };
        using SKBlender blender = Lightness.Effect.ToBlender(curves);
        return new SKPaint { IsAntialias = true, Shader = shader, Blender = blender };
    }

    // --- [PAINTS]

    private static SKPaint Shaded(FillContext held, SKShader shader, bool dither) {
        using SKShader owned = shader;
        return new SKPaint { IsAntialias = true, Shader = owned, IsDither = dither, Blender = held.Blend.ValueUnsafe() };
    }

    private static Action<SKCanvas> Clipped(Outline shape, Action<SKCanvas> draw) =>
        canvas => {
            using SKAutoCanvasRestore restore = new(canvas);
            shape.Clip(canvas, SKClipOperation.Intersect);
            draw(canvas);
        };
}
