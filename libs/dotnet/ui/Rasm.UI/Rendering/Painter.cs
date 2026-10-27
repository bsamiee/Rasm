using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Painter {
    // --- [SCENE]
    internal sealed record Pass(PaintCache Cache, Scene Scene, ColorScheme Theme, HashMap<InteractionState, float> StateLayers, Animations Animations,
                               HashSet<Seq<Key>> Live, Duration Now) {
        internal float Clock => (float)Now.TotalSeconds;

        internal Option<Tween> Tweened(LayoutNode layout) => Animations.Transitions.Find(layout.Path).Map(transition => transition.Tweened(layout, Now));
    }

    public static Fin<Unit> Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        return cache.Retain(mount).Map(_ => {
            HashMap<InteractionState, float> stateLayers = mount.StateLayers(now);
            Pass pass = new(cache, scene, scene.Layout.Cascade.Theme, stateLayers, mount.Animations, toHashSet(stateLayers.Keys.ToSeq().Map(static key => key.Path)
                .Concat(scene.Timed.Map(static layout => layout.Path)).Bind(Scene.AncestorsAndSelf).Concat(mount.Animations.Transitions.Keys.ToSeq().Bind(static path => Scene.AncestorsAndSelf(path).Tail))), now);
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRegion(damage);
            canvas.Clear(SKColors.Transparent);
            DrawNode(canvas, pass, scene.Layout, new Context(canvas.TotalMatrix, []), scene.Layout.Frame.Location, pass.Tweened(scene.Layout));
            _ = mount.Animations.Exiting.Values.Iter(left => {
                using SKAutoCanvasRestore clipped = new(canvas);
                canvas.ClipRect(left.Place.Clip, antialias: true);
                DrawNode(canvas, pass, left.Last, new Context(canvas.TotalMatrix, []) { Plot = left.Place.Plot }, left.At, Some(new Tween(SKPoint.Empty, left.Last.Frame.Size, left.Opacity.Value(now))));
            });
            return stateLayers.Filter(static (key, _) => key.Interaction.ShowsFocusRing).Iter((key, weight) => scene.FocusRing(key.Path).Iter(ring => FocusRing(canvas, pass, ring, weight)));
        });
    }

    private static void DrawNode(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKPoint at, Option<Tween> tween) {
        Context own = context.Translated(at) with { StateLayers = StateLayers(pass, layout, context) };
        float alpha = tween.IsSome ? 1f : layout.Alpha;
        using SKAutoCanvasRestore restore = new(canvas, doSave: tween.IsSome || pass.Live.Contains(layout.Path));
        _ = tween.Iter(held => {
            canvas.Translate(held.Shift.X, held.Shift.Y);
            if (held.Size != layout.Frame.Size) canvas.ClipRect(SKRect.Create(at, held.Size), antialias: true);
            if (held.Alpha < 1f) _ = canvas.SaveLayer(Scene.Absolute(layout.LocalSubtreeBounds, at), pass.Cache.TweenPaint(layout, held.Alpha));
        });
        if (!pass.Live.Contains(layout.Path)) {
            canvas.DrawPicture(pass.Cache.Subtree(layout, own, alpha, recording => DrawSelf(recording, pass, layout, own, alpha)), at);
        } else if (!canvas.QuickReject(Scene.Absolute(layout.LocalSubtreeBounds, at))) {
            canvas.Translate(at.X, at.Y);
            DrawSelf(canvas, pass, layout, own, alpha);
        }
    }

    private static void DrawSelf(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, float alpha) {
        SKRect local = SKRect.Create(layout.Frame.Size);
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        Option<SKImageFilter> filter = layout.Node.Role.IsSome && !layout.Style.Filters.IsEmpty ? pass.Cache.Filtered(layout, layout.Style.Filters) : None;
        if (alpha < 1f || filter.IsSome) _ = canvas.SaveLayer(layout.LocalSubtreeBounds, pass.Cache.LayerPaint(layout, layout.Style.Role, alpha, filter));
        bool filled = layout.Node.Role.IsSome && layout.Node.Painting.AcceptsBackground && Background(canvas, pass, layout, context, local);
        SKColorF under = Styles.Background(layout.Cascade, layout.Style, layout.Node);
        Context inner = context with { Plot = Layouts.Plotted(layout) | context.Plot };
        Seq<ResolvedShadow> stack = layout.Node.Role.IsSome && !filled ? Shadows(pass.Theme, layout.Style) : [];
        _ = stack.Filter(static held => !held.Shadow.Inside).Iter(held => Layered(canvas, pass.Cache.Shadowed(layout, layout.Style.Role, held.Shadow, held.Color), layout.LocalSubtreeBounds, Inside));
        Inside(canvas);
        _ = stack.Filter(static held => held.Shadow.Inside).Iter(held => Layered(canvas, pass.Cache.Shadowed(layout, layout.Style.Role, held.Shadow, held.Color), layout.LocalSubtreeBounds, Inside));

        void Inside(SKCanvas target) {
            Content(target, pass, layout, context, under, layout.ContentBox);
            if (layout.Children.IsEmpty) return;
            using SKAutoCanvasRestore masked = new(target, doSave: false);
            Option<SKPaint> fade = EdgeFade(pass.Scene, layout, local).Map(held => pass.Cache.FadeMask(layout, held));
            if (fade.IsSome) _ = target.SaveLayer(local, paint: null);
            _ = toSeq(layout.Children.GroupAdjacent(child => layout.Node.Clips(child.Node))).Iter(run => {
                using SKAutoCanvasRestore clipped = new(target, doSave: run.Key);
                if (run.Key) pass.Cache.Outlined(layout, local, 0f).Clip(target, SKClipOperation.Intersect);
                _ = toSeq(run).Iter(child => DrawNode(target, pass, child, inner, child.Frame.Location + pass.Scene.Shift(layout, child), pass.Tweened(child)));
            });
            _ = fade.Iter(mask => target.DrawRect(local, mask));
        }
    }

    private static Seq<StateLayer> StateLayers(Pass pass, LayoutNode layout, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.StateLayers.Find(new InteractionState(layout.Path, interaction)).Map(weight => new StateLayer(interaction, weight))) switch {
            var own when own.IsEmpty && layout.Node.Role.IsNone => context.StateLayers,
            var own => own,
        };

    // --- [SHADOW]
    private static readonly SKPaint Silhouette = new() { IsAntialias = true };

    private sealed record ResolvedShadow(Shadow Shadow, SKColorF Color);

    private static Seq<ResolvedShadow> Shadows(ColorScheme theme, RoleStyle style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Color, style).Map(color => new ResolvedShadow(shadow, color))).Strict();

    private static void Layered(SKCanvas canvas, SKPaint layer, SKRect reach, Action<SKCanvas> draw) {
        _ = canvas.SaveLayer(reach, layer);
        draw(canvas);
        canvas.Restore();
    }

    private static void ShapeShadows(SKCanvas canvas, PaintCache cache, LayoutNode layout, Role role, SKRect frame, SKRect reach, Seq<ResolvedShadow> stack) =>
        _ = stack.Iter(held => {
            (float outset, Shadow graph) = held.Shadow.Outlined;
            Outline silhouette = cache.Outlined(layout, frame, outset);
            Layered(canvas, cache.Shadowed(layout, role, graph, held.Color), reach, target => silhouette.Draw(target, Silhouette));
        });

    // --- [BACKGROUND]
    private static readonly SKPaint ClearPaint = new() { IsAntialias = true, BlendMode = SKBlendMode.Clear };

    private static bool Background(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKRect frame) {
        RoleStyle style = layout.Style;
        Outline shape = pass.Cache.Outlined(layout, frame, 0f);
        Seq<(Material Fill, SKPaint Paint)> fills = style.Fills.Map((fill, index) => FillPaint(new FillContext(pass, layout, context, frame, index, SKBlendMode.SrcOver, None), fill).Map(paint => (Fill: fill, Paint: paint))).Somes().Strict();
        Seq<ResolvedShadow> stack = fills.IsEmpty ? [] : Shadows(pass.Theme, style);
        using (new SKAutoCanvasRestore(canvas)) {
            shape.Clip(canvas, SKClipOperation.Difference);
            ShapeShadows(canvas, pass.Cache, layout, style.Role, frame, layout.LocalSubtreeBounds, stack.Filter(static held => !held.Shadow.Inside));
        }
        _ = fills.Iter(held => WithBackdropBlur(canvas, held.Fill, frame, clip => shape.Clip(clip, SKClipOperation.Intersect), () => shape.Draw(canvas, held.Paint)));
        ShapeShadows(canvas, pass.Cache, layout, style.Role, frame, layout.LocalSubtreeBounds, stack.Filter(static held => held.Shadow.Inside));
        if (style.Strokes.IsEmpty) return !fills.IsEmpty;
        SKColorF under = layout.Cascade.Background;
        bool bounding = layout.Node.Part.IsSome && style.Solid.Bind(value => pass.Theme.Color(value, style)).ForAll(fill => Styles.Ratio(fill, under) < pass.Theme.Contrast.Graphic);
        Outline rim = pass.Cache.Outlined(layout, frame, -style.HalfStroke);
        _ = style.Strokes.Map(static (stroke, index) => (Stroke: stroke, Index: index)).Iter(held => pass.Theme.WithStateLayers(held.Stroke.Color, style, context.StateLayers)
            .Map(color => bounding ? Styles.Legible(style, under, color, pass.Theme.Contrast.Graphic) : color)
            .Iter(color => rim.Draw(canvas, pass.Cache.StrokePaint(layout, held.Index, color, style.Metrics[Metric.Stroke], [], SKBlendMode.SrcOver))));
        return !fills.IsEmpty;
    }

    internal static void WithBackdropBlur(SKCanvas canvas, Material material, SKRect bounds, Action<SKCanvas> clip, Action draw) {
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

    internal sealed record FillContext(Pass Pass, LayoutNode Layout, Context Context, SKRect Frame, int Index, SKBlendMode Blend, Option<DataSeries> Series) {
        internal Option<SKColorF> Color(ColorValue value) => Pass.Theme.WithStateLayers(value, Layout.Style, Context.StateLayers, Layout.Style.Look.StateLayers, Series);

        internal Option<SKPaint> CachedPaint(Material fill, Seq<Option<SKColorF>> colors, float scale, float clock, Func<Option<SKPaint>> make) =>
            Pass.Cache.FillPaint(Layout, Index, new FillKey(fill, colors, Frame, Blend, scale, clock), make);

        internal Option<SKPaint> Colored(Material fill, ColorValue value) {
            Option<SKColorF> color = Color(value);
            return CachedPaint(fill, [color], 0f, 0f, () => color.Map(ink => new SKPaint { IsAntialias = true, ColorF = ink, BlendMode = Blend }));
        }

        internal SKPaint Shaded(SKShader shader) {
            using SKShader owned = shader;
            return new SKPaint { IsAntialias = true, Shader = owned, BlendMode = Blend };
        }
    }

    internal static Option<SKPaint> FillPaint(FillContext filling, Material material) =>
        material.Switch(
            filling,
            solid: static (held, solid) => held.Colored(solid, solid.Color),
            gradient: static (held, gradient) => {
                Seq<Option<SKColorF>> colors = gradient.Stops.Map(stop => held.Color(stop.Color));
                return held.CachedPaint(gradient, colors, 0f, 0f, () => colors.Traverse(static color => color).As().Map(inks => held.Shaded(gradient.Shader(held.Frame, inks))));
            },
            drawn: static (held, drawn) => {
                HashMap<string, Option<SKColorF>> colors = drawn.Uniforms.Map(held.Color);
                return held.CachedPaint(drawn, colors.Values.ToSeq(), drawn.Timed ? 0f : held.Context.Scale, drawn.Timed ? held.Pass.Clock : 0f,
                    () => held.Pass.Cache.DrawnShader(drawn, colors, held.Frame, held.Context.Scale, held.Pass.Clock).Map(held.Shaded));
            },
            backdrop: static (_, _) => Some(ClearPaint),
            blurred: static (held, blurred) => held.Colored(blurred, blurred.Tint));

    // --- [RING]
    private static void FocusRing(SKCanvas canvas, Pass pass, PlacedNode ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.ClipRect(ring.Clip, antialias: true);
        if (weight < 1f) _ = canvas.SaveLayer(SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), pass.Cache.LayerPaint(ring.Layout, ring.Style.Role, weight, None));
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        SKRect shape = SKRect.Create(ring.Frame.Size);
        SKRect reach = SKRect.Inflate(shape, ring.Style.Overhang, ring.Style.Overhang);
        Seq<ResolvedShadow> stack = Shadows(pass.Theme, ring.Style);
        using (new SKAutoCanvasRestore(canvas)) {
            pass.Cache.Outlined(ring.Layout, shape, 0f).Clip(canvas, SKClipOperation.Difference);
            ShapeShadows(canvas, pass.Cache, ring.Layout, ring.Style.Role, shape, reach, stack.Filter(static held => !held.Shadow.Inside));
        }
        ShapeShadows(canvas, pass.Cache, ring.Layout, ring.Style.Role, shape, reach, stack.Filter(static held => held.Shadow.Inside));
    }

    // --- [FADE]
    private static Option<Fade> EdgeFade(Scene scene, LayoutNode layout, SKRect viewport) =>
        from fade in layout.Node.Role.IsSome ? Some(layout.Style.Metrics[Metric.EdgeFade]) : None
        where fade > 0f
        from scroll in scene.Adjustment(layout.Path)
        let hidden = (Before: scroll.Value > scroll.Min, After: scroll.Value < scroll.Max)
        let length = scroll.Axis.Of(viewport.Size)
        where (hidden.Before || hidden.After) && length > 0f
        select new Fade(viewport.Location, (SKPoint)scroll.Axis.With(viewport.Location, scroll.Axis.Of(viewport.Location) + length), hidden.Before, hidden.After, float.Min(fade, length / 2f) / length);

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKColorF under, SKRect content) =>
        layout.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Under: under, Content: content),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Under, held.Content),
            text: static (held, text) => DrawText(held.Canvas, held.Pass, text, held.Context, held.Under));

    private static void Contained(SKCanvas canvas, Pass pass, LayoutNode.Box layout, Context context, SKColorF under, SKRect content) =>
        layout.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Layout: layout, Context: context, Under: under, Content: content),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marks.Mark(held.Canvas, held.Pass, held.Layout, held.Context, mark, plot)),
            glyph: static (held, glyph) => held.Pass.Theme.WithStateLayers(held.Layout.Style.Text, held.Layout.Style, held.Context.StateLayers)
                .Map(tint => Styles.Legible(held.Layout.Style, held.Under, tint, held.Pass.Theme.Contrast.Graphic))
                .Iter(tint => held.Pass.Cache.Glyph(held.Canvas, held.Layout, glyph.Art, held.Under, held.Context.Pen(held.Layout.Style), tint, held.Content)),
            image: static (held, image) => Marks.Image(held.Canvas, held.Pass, held.Layout, held.Context, image, held.Content),
            slot: static (held, _) => held.Canvas.DrawRect(SKRect.Create(held.Layout.Frame.Size), ClearPaint));

    private static void DrawText(SKCanvas canvas, Pass pass, LayoutNode.Text layout, Context context, SKColorF under) =>
        _ = pass.Theme.WithStateLayers(layout.Style.Text, layout.Style, context.StateLayers).Map(ink => Styles.Legible(layout.Style, under, ink, pass.Theme.Contrast.Text))
            .Iter(ink => Typesetting.Paint(canvas, layout, (SKColor)ink, pass.Cache.Raster));
}
