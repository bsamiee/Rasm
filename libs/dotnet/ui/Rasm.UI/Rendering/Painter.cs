using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Painter {
    // --- [SCENE]
    internal sealed record Pass(PaintCache Cache, Scene Scene, ColorScheme Theme, HashMap<Washed, float> Washes, Animations Animations,
                               HashSet<Seq<Key>> Live, Duration Now) {
        internal float Clock => (float)Now.TotalSeconds;

        internal Option<Tween> Tweened(Laid laid) => Animations.Transitions.Find(laid.Path).Map(transition => transition.Tweened(laid, Now));
    }

    public static Fin<Unit> Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        return cache.Retain(mount).Map(_ => {
            HashMap<Washed, float> washes = mount.Washes(now);
            Pass pass = new(cache, scene, scene.Layout.Cascade.Theme, washes, mount.Animations, toHashSet(washes.Keys.ToSeq().Map(static key => key.Path)
                .Concat(scene.Timed.Map(static laid => laid.Path)).Bind(Scene.Above).Concat(mount.Animations.Transitions.Keys.ToSeq().Bind(static path => Scene.Above(path).Tail))), now);
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRegion(damage);
            canvas.Clear(SKColors.Transparent);
            Drawn(canvas, pass, scene.Layout, new Context(canvas.TotalMatrix, []), scene.Layout.Frame.Location, pass.Tweened(scene.Layout));
            _ = mount.Animations.Leaving.Values.Iter(left => {
                using SKAutoCanvasRestore clipped = new(canvas);
                canvas.ClipRect(left.Place.Clip, antialias: true);
                Drawn(canvas, pass, left.Last, new Context(canvas.TotalMatrix, []) { Plot = left.Place.Plot }, left.At, Some(new Tween(SKPoint.Empty, left.Last.Frame.Size, left.Opacity.Value(now))));
            });
            return washes.Filter(static (key, _) => key.Interaction.Ring).Iter((key, weight) => scene.Ring(key.Path).Iter(ring => Ringed(canvas, pass, ring, weight)));
        });
    }

    private static void Drawn(SKCanvas canvas, Pass pass, Laid laid, Context context, SKPoint at, Option<Tween> tween) {
        Context own = context.Within(at) with { Washes = Washed(pass, laid, context) };
        float alpha = tween.IsSome ? 1f : laid.Alpha;
        using SKAutoCanvasRestore restore = new(canvas, doSave: tween.IsSome || pass.Live.Contains(laid.Path));
        _ = tween.Iter(held => {
            canvas.Translate(held.Shift.X, held.Shift.Y);
            if (held.Size != laid.Frame.Size) canvas.ClipRect(SKRect.Create(at, held.Size), antialias: true);
            if (held.Alpha < 1f) _ = canvas.SaveLayer(Scene.Absolute(laid.LocalReach, at), pass.Cache.Tweened(laid, held.Alpha));
        });
        if (!pass.Live.Contains(laid.Path)) {
            canvas.DrawPicture(pass.Cache.Subtree(laid, own, alpha, recording => Self(recording, pass, laid, own, alpha)), at);
        } else if (!canvas.QuickReject(Scene.Absolute(laid.LocalReach, at))) {
            canvas.Translate(at.X, at.Y);
            Self(canvas, pass, laid, own, alpha);
        }
    }

    private static void Self(SKCanvas canvas, Pass pass, Laid laid, Context context, float alpha) {
        SKRect local = SKRect.Create(laid.Frame.Size);
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        Option<SKImageFilter> filter = laid.Node.Role.IsSome && !laid.Style.Filters.IsEmpty ? pass.Cache.Filtered(laid, laid.Style.Filters) : None;
        if (alpha < 1f || filter.IsSome) _ = canvas.SaveLayer(laid.LocalReach, pass.Cache.Veiled(laid, laid.Style.Role, alpha, filter));
        bool filled = laid.Node.Role.IsSome && laid.Node.Painting.AcceptsBackground && Background(canvas, pass, laid, context, local);
        SKColorF under = Styles.Grounded(laid.Cascade, laid.Style, laid.Node);
        Context inner = context with { Plot = Layouts.Plotted(laid) | context.Plot };
        Seq<Shade> stack = laid.Node.Role.IsSome && !filled ? Shadows(pass.Theme, laid.Style) : [];
        _ = stack.Filter(static held => !held.Shadow.Inside).Iter(held => Layered(canvas, pass.Cache.Shadowed(laid, laid.Style.Role, held.Shadow, held.Color), laid.LocalReach, Inside));
        Inside(canvas);
        _ = stack.Filter(static held => held.Shadow.Inside).Iter(held => Layered(canvas, pass.Cache.Shadowed(laid, laid.Style.Role, held.Shadow, held.Color), laid.LocalReach, Inside));

        void Inside(SKCanvas target) {
            Content(target, pass, laid, context, under, laid.ContentBox);
            if (laid.Children.IsEmpty) return;
            using SKAutoCanvasRestore masked = new(target, doSave: false);
            Option<SKPaint> fade = Faded(pass.Scene, laid, local).Map(held => pass.Cache.Masked(laid, held));
            if (fade.IsSome) _ = target.SaveLayer(local, paint: null);
            _ = toSeq(laid.Children.GroupAdjacent(child => laid.Node.Clips(child.Node))).Iter(run => {
                using SKAutoCanvasRestore clipped = new(target, doSave: run.Key);
                if (run.Key) pass.Cache.Outlined(laid, local, 0f).Clip(target, SKClipOperation.Intersect);
                _ = toSeq(run).Iter(child => Drawn(target, pass, child, inner, child.Frame.Location + pass.Scene.Shift(laid, child), pass.Tweened(child)));
            });
            _ = fade.Iter(mask => target.DrawRect(local, mask));
        }
    }

    private static Seq<Wash> Washed(Pass pass, Laid laid, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.Washes.Find(new Washed(laid.Path, interaction)).Map(weight => new Wash(interaction, weight))) switch {
            var own when own.IsEmpty && laid.Node.Role.IsNone => context.Washes,
            var own => own,
        };

    // --- [SHADOW]
    private static readonly SKPaint Silhouette = new() { IsAntialias = true };

    private sealed record Shade(Shadow Shadow, SKColorF Color);

    private static Seq<Shade> Shadows(ColorScheme theme, RoleStyle style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Color, style).Map(color => new Shade(shadow, color))).Strict();

    private static void Layered(SKCanvas canvas, SKPaint layer, SKRect reach, Action<SKCanvas> draw) {
        _ = canvas.SaveLayer(reach, layer);
        draw(canvas);
        canvas.Restore();
    }

    private static void Outlined(SKCanvas canvas, PaintCache cache, Laid laid, Role role, SKRect frame, SKRect reach, Seq<Shade> stack) =>
        _ = stack.Iter(held => {
            (float outset, Shadow graph) = held.Shadow.Outlined;
            Outline silhouette = cache.Outlined(laid, frame, outset);
            Layered(canvas, cache.Shadowed(laid, role, graph, held.Color), reach, target => silhouette.Draw(target, Silhouette));
        });

    // --- [BACKGROUND]
    private static readonly SKPaint Cleared = new() { IsAntialias = true, BlendMode = SKBlendMode.Clear };

    private static bool Background(SKCanvas canvas, Pass pass, Laid laid, Context context, SKRect frame) {
        RoleStyle style = laid.Style;
        Outline shape = pass.Cache.Outlined(laid, frame, 0f);
        Seq<(Material Fill, SKPaint Paint)> fills = style.Fills.Map((fill, index) => Filled(new Filling(pass, laid, context, frame, index, SKBlendMode.SrcOver, None), fill).Map(paint => (Fill: fill, Paint: paint))).Somes().Strict();
        Seq<Shade> stack = fills.IsEmpty ? [] : Shadows(pass.Theme, style);
        using (new SKAutoCanvasRestore(canvas)) {
            shape.Clip(canvas, SKClipOperation.Difference);
            Outlined(canvas, pass.Cache, laid, style.Role, frame, laid.LocalReach, stack.Filter(static held => !held.Shadow.Inside));
        }
        _ = fills.Iter(held => Covered(canvas, held.Fill, frame, clip => shape.Clip(clip, SKClipOperation.Intersect), () => shape.Draw(canvas, held.Paint)));
        Outlined(canvas, pass.Cache, laid, style.Role, frame, laid.LocalReach, stack.Filter(static held => held.Shadow.Inside));
        if (style.Strokes.IsEmpty) return !fills.IsEmpty;
        SKColorF under = laid.Cascade.Ground;
        bool bounding = laid.Node.Part.IsSome && style.Solid.Bind(value => pass.Theme.Color(value, style)).ForAll(fill => Styles.Ratio(fill, under) < pass.Theme.Contrast.Graphic);
        Outline rim = pass.Cache.Outlined(laid, frame, -style.HalfStroke);
        _ = style.Strokes.Map(static (stroke, index) => (Stroke: stroke, Index: index)).Iter(held => pass.Theme.Washed(held.Stroke.Color, style, context.Washes)
            .Map(color => bounding ? Styles.Legible(style, under, color, pass.Theme.Contrast.Graphic) : color)
            .Iter(color => rim.Draw(canvas, pass.Cache.Edged(laid, held.Index, color, style.Metrics[Metric.Stroke], [], SKBlendMode.SrcOver))));
        return !fills.IsEmpty;
    }

    internal static void Covered(SKCanvas canvas, Material material, SKRect bounds, Action<SKCanvas> clip, Action draw) {
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        _ = material.Blur
            .Iter(sigma => {
                _ = canvas.Save();
                clip(canvas);
                using SKImageFilter backdrop = SKImageFilter.CreateBlur(sigma, sigma, SKShaderTileMode.Clamp);
                _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = bounds, Backdrop = backdrop });
            });
        draw();
    }

    internal sealed record Filling(Pass Pass, Laid Laid, Context Context, SKRect Frame, int Index, SKBlendMode Blend, Option<Series> Series) {
        internal Option<SKColorF> Inked(ColorValue value) => Pass.Theme.Washed(value, Laid.Style, Context.Washes, Laid.Style.Look.Washes, Series);

        internal Option<SKPaint> Brushed(Material fill, Seq<Option<SKColorF>> colors, float scale, float clock, Func<Option<SKPaint>> make) =>
            Pass.Cache.Brushed(Laid, Index, new Brushing(fill, colors, Frame, Blend, scale, clock), make);

        internal Option<SKPaint> Colored(Material fill, ColorValue value) {
            Option<SKColorF> color = Inked(value);
            return Brushed(fill, [color], 0f, 0f, () => color.Map(ink => new SKPaint { IsAntialias = true, ColorF = ink, BlendMode = Blend }));
        }

        internal SKPaint Shaded(SKShader shader) {
            using SKShader owned = shader;
            return new SKPaint { IsAntialias = true, Shader = owned, BlendMode = Blend };
        }
    }

    internal static Option<SKPaint> Filled(Filling filling, Material material) =>
        material.Switch(
            filling,
            solid: static (held, solid) => held.Colored(solid, solid.Color),
            gradient: static (held, gradient) => {
                Seq<Option<SKColorF>> colors = gradient.Stops.Map(stop => held.Inked(stop.Color));
                return held.Brushed(gradient, colors, 0f, 0f, () => colors.Traverse(static color => color).As().Map(inks => held.Shaded(gradient.Shader(held.Frame, inks))));
            },
            drawn: static (held, drawn) => {
                HashMap<string, Option<SKColorF>> colors = drawn.Uniforms.Map(held.Inked);
                return held.Brushed(drawn, colors.Values.ToSeq(), drawn.Timed ? 0f : held.Context.Scale, drawn.Timed ? held.Pass.Clock : 0f,
                    () => held.Pass.Cache.Drawn(drawn, colors, held.Frame, held.Context.Scale, held.Pass.Clock).Map(held.Shaded));
            },
            backdrop: static (_, _) => Some(Cleared),
            blurred: static (held, blurred) => held.Colored(blurred, blurred.Tint));

    // --- [RING]
    private static void Ringed(SKCanvas canvas, Pass pass, Framed ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(ring.Clip, antialias: true);
        if (weight < 1f) _ = canvas.SaveLayer(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), pass.Cache.Veiled(ring.Laid, ring.Style.Role, weight, None));
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        SKRect shape = SKRect.Create(ring.Frame.Size);
        SKRect reach = SKRect.Inflate(shape, ring.Style.Overhang, ring.Style.Overhang);
        Seq<Shade> stack = Shadows(pass.Theme, ring.Style);
        using (new SKAutoCanvasRestore(canvas)) {
            pass.Cache.Outlined(ring.Laid, shape, 0f).Clip(canvas, SKClipOperation.Difference);
            Outlined(canvas, pass.Cache, ring.Laid, ring.Style.Role, shape, reach, stack.Filter(static held => !held.Shadow.Inside));
        }
        Outlined(canvas, pass.Cache, ring.Laid, ring.Style.Role, shape, reach, stack.Filter(static held => held.Shadow.Inside));
    }

    // --- [FADE]
    private static Option<Fade> Faded(Scene scene, Laid laid, SKRect viewport) =>
        from fade in laid.Node.Role.IsSome ? Some(laid.Style.Metrics[Metric.EdgeFade]) : None
        where fade > 0f
        from scroll in scene.Adjuster(laid.Path)
        let hidden = (Before: scroll.Value > scroll.Min, After: scroll.Value < scroll.Max)
        let length = scroll.Axis.Of(viewport.Size)
        where (hidden.Before || hidden.After) && length > 0f
        select new Fade(viewport.Location, (SKPoint)scroll.Axis.With(viewport.Location, scroll.Axis.Of(viewport.Location) + length), hidden.Before, hidden.After, float.Min(fade, length / 2f) / length);

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, Laid laid, Context context, SKColorF under, SKRect content) =>
        laid.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Under: under, Content: content),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Under, held.Content),
            text: static (held, text) => Texted(held.Canvas, held.Pass, text, held.Context, held.Under));

    private static void Contained(SKCanvas canvas, Pass pass, Laid.Box laid, Context context, SKColorF under, SKRect content) =>
        laid.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Laid: laid, Context: context, Under: under, Content: content),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marks.Marked(held.Canvas, held.Pass, held.Laid, held.Context, mark, plot)),
            glyph: static (held, glyph) => held.Pass.Theme.Washed(held.Laid.Style.Text, held.Laid.Style, held.Context.Washes)
                .Map(tint => Styles.Legible(held.Laid.Style, held.Under, tint, held.Pass.Theme.Contrast.Graphic))
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, held.Laid, glyph.Art, held.Under, held.Context.Pen(held.Laid.Style), tint, held.Content)),
            image: static (held, image) => Marks.Pictured(held.Canvas, held.Pass, held.Laid, held.Context, image, held.Content),
            slot: static (held, _) => held.Canvas.DrawRect(SKRect.Create(held.Laid.Frame.Size), Cleared));

    private static void Texted(SKCanvas canvas, Pass pass, Laid.Text laid, Context context, SKColorF under) =>
        _ = pass.Theme.Washed(laid.Style.Text, laid.Style, context.Washes).Map(ink => Styles.Legible(laid.Style, under, ink, pass.Theme.Contrast.Text))
            .Iter(ink => Typesetting.Paint(canvas, laid, (SKColor)ink, pass.Cache.Raster));
}
