using Rasm.UI.Styling;
using SkiaSharp;

namespace Rasm.UI.Rendering;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record FillContext(Painter.Pass Pass, LayoutNode Layout, Context Context, SKRect Frame, int Index) {
    internal Option<SKBlender> Blend { get; init; }
    internal Option<DataSeries> Series { get; init; }

    internal Option<SKColorF> Color(ColorValue value) => Pass.Scene.Layout.Cascade.Theme.WithStateLayers(Pass.Scene.ColorCache, value, Layout.Style, Context.StateLayers, Layout.Style.Look.StateLayers, Series);

    internal FillKey Key(float scale, float clock, Option<SKPoint> pointer) => new(Context.StateLayers, Frame, scale, clock, pointer);

    internal FilterTarget Target => new(Pass.Cache.Space, Frame, Color, child => child.Shader(Frame, material => Fills.Shader(this, material)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Fills {
    private static readonly SKColorSpace Srgb = SKColorSpace.CreateSrgb();
    private static readonly SKSamplingOptions Sampling = new(SKFilterMode.Linear);

    // --- [FILLS]
    internal static Option<Action<SKCanvas>> Fill(FillContext held, Material material, Outline shape) =>
        material.Switch(
            (Held: held, Shape: shape),
            solid: static (state, solid) => Painted(state.Held, state.Held.Key(0f, 0f, None), () => state.Held.Color(solid.Color).Map(color => new SKPaint { IsAntialias = true, ColorF = color, Blender = state.Held.Blend.ValueUnsafe() }))
                .Map(paint => Covered(state.Shape, paint)),
            gradient: static (state, gradient) => Painted(state.Held, state.Held.Key(0f, 0f, None), () => gradient.Shader(state.Held.Frame, state.Held.Color).Map(shader => Shaded(state.Held, shader, dither: true)))
                .Map(paint => Covered(state.Shape, paint)),
            drawn: static (state, drawn) => Painted(state.Held, Live(drawn) ? state.Held.Key(0f, drawn.Timed ? state.Held.Pass.Clock : 0f, Pointer(state.Held, drawn)) : state.Held.Key(state.Held.Context.Scale, 0f, None),
                    () => Live(drawn)
                        ? Some(Shaded(state.Held, Shader(state.Held, drawn), dither: false))
                        : state.Held.Pass.Cache.Rastered(Shader(state.Held with { Frame = SKRect.Create(state.Held.Frame.Size) }, drawn), state.Held.Frame, state.Held.Context.Scale).Map(shader => Shaded(state.Held, shader, dither: false)))
                .Map(paint => Covered(state.Shape, paint)),
            glass: static (state, glass) => Some<Action<SKCanvas>>(canvas => Glazed(canvas, state.Held, glass, state.Shape)),
            pattern: static (state, pattern) => Painted(state.Held, state.Held.Key(0f, 0f, None), () => pattern.Shader(state.Held.Frame, glyph => Picture(state.Held, glyph)).Map(shader => Shaded(state.Held, shader, dither: false)))
                .Map(paint => Covered(state.Shape, paint)),
            hatch: static (state, hatch) => Painted(state.Held, state.Held.Key(0f, 0f, None), () => state.Held.Color(hatch.Color).Map(color => {
                using SKPathEffect lines = hatch.Effect();
                return new SKPaint { IsAntialias = true, ColorF = color, PathEffect = lines, Blender = state.Held.Blend.ValueUnsafe() };
            }))
                .Map(paint => Clipped(state.Shape, canvas => state.Shape.Draw(canvas, paint))),
            skin: static (state, skin) => Painted(state.Held, state.Held.Key(0f, 0f, None), () => Some(new SKPaint { IsAntialias = true, Blender = state.Held.Blend.ValueUnsafe() }))
                .Map(paint => Clipped(state.Shape, canvas => canvas.DrawImageNinePatch(skin.Art, skin.Center, state.Held.Frame, SKFilterMode.Linear, paint))));

    internal static SKShader Shader(FillContext held, Material material) =>
        material.SwitchPartially(
            held,
            @default: static (_, _) => Option<SKShader>.None,
            solid: static (state, solid) => state.Color(solid.Color).Map(static color => SKShader.CreateColor(color, Srgb)),
            gradient: static (state, gradient) => gradient.Shader(state.Frame, state.Color),
            drawn: static (state, drawn) => Some(Shader(state, drawn)),
            pattern: static (state, pattern) => pattern.Shader(state.Frame, glyph => Picture(state, glyph)))
        .IfNone(SKShader.CreateEmpty);

    // --- [GLASS]
    private static void Glazed(SKCanvas canvas, FillContext held, Material.Glass glass, Outline shape) {
        PaintCache cache = held.Pass.Cache;
        Option<SKImageFilter> backdrop = cache.Backdrops.GetOrAdd(new PaintSlot(held.Layout.Path, held.Index), held.Layout, held.Key(held.Context.Scale, 0f, None), () => Backdrop(held, glass));
        _ = canvas.Save();
        shape.Clip(canvas, SKClipOperation.Intersect);
        _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = held.Frame, Backdrop = backdrop.ValueUnsafe(), Flags = cache.Precision });
    }

    private static Option<SKImageFilter> Backdrop(FillContext held, Material.Glass glass) {
        Option<SKImageFilter> chained = Filter.Chained(glass.Backdrop, held.Target);
        Option<SKImage> matte = glass.Refraction.IsSome
            ? held.Pass.Cache.Matte(held.Layout, new MatteKey(held.Frame.Size, held.Layout.Corners, held.Layout.Smoothing, held.Context.Scale))
            : None;
        return (from refraction in glass.Refraction
                from map in matte
                select Displaced(map, refraction, held.Frame, chained))
            | chained;
    }

    private static SKImageFilter Displaced(SKImage map, float refraction, SKRect frame, Option<SKImageFilter> chained) {
        using SKImageFilter field = SKImageFilter.CreateImage(map, SKRect.Create(map.Width, map.Height), frame, Sampling);
        SKImageFilter displaced = SKImageFilter.CreateDisplacementMapEffect(SKColorChannel.R, SKColorChannel.G, refraction, field, chained.ValueUnsafe());
        _ = chained.Iter(static held => held.Dispose());
        return displaced;
    }

    // --- [PAINTS]
    private static Option<SKPaint> Painted(FillContext held, FillKey key, Func<Option<SKPaint>> make) =>
        held.Pass.Cache.Brushes.GetOrAdd(new PaintSlot(held.Layout.Path, held.Index), held.Layout, key, make);

    private static SKPaint Shaded(FillContext held, SKShader shader, bool dither) {
        using SKShader owned = shader;
        return new SKPaint { IsAntialias = true, Shader = owned, IsDither = dither, Blender = held.Blend.ValueUnsafe() };
    }

    private static SKShader Shader(FillContext held, Material.Drawn drawn) =>
        drawn.Shader(held.Frame, held.Color, metric => held.Layout.Style.Metrics[metric], child => child.Shader(held.Frame, material => Shader(held, material)),
            held.Layout.Corners, drawn.Timed ? held.Pass.Clock : 0f, Pointer(held, drawn));

    private static bool Live(Material.Drawn drawn) => drawn.Timed || drawn.Pointing;

    private static Option<SKPoint> Pointer(FillContext held, Material.Drawn drawn) => drawn.Pointing ? held.Pass.PointerAt(held.Layout) : None;

    private static Option<SKPicture> Picture(FillContext held, GlyphSource glyph) =>
        held.Pass.Cache.Art(Media.Keyed(held.Layout, glyph, Styles.Background(held.Layout.Cascade, held.Layout.Style, held.Layout.Node), held.Pass.Scene.ColorCache)).Bind(static media => media.Still);

    private static Action<SKCanvas> Covered(Outline shape, SKPaint paint) => canvas => shape.Draw(canvas, paint);

    private static Action<SKCanvas> Clipped(Outline shape, Action<SKCanvas> draw) =>
        canvas => {
            using SKAutoCanvasRestore restore = new(canvas);
            shape.Clip(canvas, SKClipOperation.Intersect);
            draw(canvas);
        };
}
