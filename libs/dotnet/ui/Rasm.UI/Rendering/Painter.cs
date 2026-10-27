using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;
using Topten.RichTextKit;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Painter {
    // --- [SCENE]
    internal sealed record Pass(PaintCache Cache, Scene Scene, HashMap<InteractionState, float> StateLayers, Animations Animations, HashSet<Seq<Key>> Live,
                               HashSet<Seq<Key>> Native, HashSet<Seq<Key>> Hovered, Option<SKPoint> Pointer, Duration Now) {
        internal bool Retained { get; init; } = true;

        internal float Clock => (float)Now.TotalSeconds;

        internal Option<Tween> Tweened(LayoutNode layout) => Animations.Tweened(layout, Now);

        internal Option<SKPoint> PointerAt(LayoutNode layout) =>
            Hovered.Contains(layout.Path) ? from at in Pointer from frame in Scene.Frame(layout.Path) select at - frame.Location : None;
    }

    internal static Fin<Unit> Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now) {
        Scene scene = mount.Scene;
        return cache.Retain(mount, canvas.TotalMatrix.MapRadius(1f)).Map(_ => {
            HashMap<InteractionState, float> stateLayers = mount.StateLayers(now);
            Animations animations = mount.Animations;
            Pass pass = new(cache, scene, stateLayers, animations,
                cache.Moving.Union(stateLayers.Keys.Map(static key => key.Path)).Union(animations.Transitions.Keys).Union(animations.Transforms.Keys).Union(animations.Planes.Keys).Union(animations.Figures.Keys),
                toHashSet(scene.Hosted.Choose(static hosted => hosted is Hosted.Backdrop ? Some(hosted.Path) : None)), toHashSet(mount.Hover), mount.Routing.Pointer, now);
            cache.Framed(pass);
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRegion(damage);
            canvas.Clear(SKColors.Transparent);
            DrawNode(canvas, pass, scene.Layout, new Context(canvas.TotalMatrix, []), scene.Layout.Frame.Location);
            _ = scene.Fragments.Iter((plot, fragment) => Fragment(canvas, pass, plot, fragment));
            _ = (toSeq(animations.Exiting.Values) + toSeq(animations.Departing.Values)).Iter(left => Departed(canvas, pass with { Retained = false }, left));
            _ = scene.Layers.Filter(layer => !scene.Hosted.Exists(hosted => hosted is Hosted.Popup && hosted.Path == layer.Path)).Iter(layer => DrawNode(canvas, pass, layer, new Context(canvas.TotalMatrix, []), layer.Frame.Location));
            return stateLayers.Filter(static (key, _) => key.Interaction.ShowsFocusRing).Iter((key, weight) => scene.FocusRing(key.Path).Iter(ring => FocusRing(canvas, pass, ring, weight)));
        });
    }

    private static void Fragment(SKCanvas canvas, Pass pass, Seq<Key> plot, LayoutNode fragment) =>
        _ = (from layout in pass.Scene.Index.Find(plot)
             from place in pass.Scene.Places.Find(plot)
             select (Layout: layout, Place: place)).Iter(held => {
                 SKRect frame = Scene.Absolute(held.Layout.Frame, held.Place.Origin);
                 using SKAutoCanvasRestore restore = new(canvas);
                 canvas.Concat(held.Place.Transform);
                 canvas.ClipRect(held.Layout.Node.Clips(fragment.Node) ? SKRect.Intersect(held.Place.Clip, frame) : held.Place.Clip, antialias: true);
                 DrawNode(canvas, pass, fragment, new Context(canvas.TotalMatrix, []) { Plot = Layouts.Plotted(held.Layout).Map(own => own.Translated(frame.Location)) | held.Place.Plot },
                     frame.Location + pass.Scene.Shift(held.Layout, fragment) + fragment.Frame.Location);
             });

    private static void Departed(SKCanvas canvas, Pass pass, ExitTransition left) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.Concat(left.Place.Transform);
        canvas.ClipRect(left.Place.Clip, antialias: true);
        DrawNode(canvas, pass, left.Last, new Context(canvas.TotalMatrix, []) { Plot = left.Place.Plot }, left.At, Some(left));
    }

    // --- [NODES]
    private static void DrawNode(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKPoint at, Option<ExitTransition> departing = default) {
        if (!pass.Retained || departing.IsSome) {
            using SKAutoCanvasRestore immediate = new(canvas);
            canvas.Translate(at.X, at.Y);
            DrawLive(canvas, pass, layout, context, at, departing);
            return;
        }
        if (pass.Live.Contains(layout.Path) || (layout.Node.Role.IsNone && !context.StateLayers.IsEmpty)) {
            Leaf leaf = pass.Cache.Leaves.GetOrAdd(layout.Path, layout, unit, () => new Leaf(layout, pass));
            leaf.Placed(context, at);
            canvas.DrawDrawable(leaf, at.X, at.Y);
            return;
        }
        Context own = context.Translated(at) with { StateLayers = StateLayers(pass, layout, context) };
        float alpha = layout.Alpha(pass.Cache.Space);
        SKDrawable recorded = pass.Cache.Recordings.GetOrAdd(layout.Path, layout, new Recording(own.CacheKey, alpha, Beneath(pass.Live, layout.Path)), () => {
            using SKPictureRecorder recorder = new();
            DrawSelf(recorder.BeginRecording(layout.LocalSubtreeBounds, useRTree: true), pass, layout, own, None, alpha);
            return recorder.EndRecordingAsDrawable();
        });
        using SKAutoCanvasRestore turned = new(canvas, doSave: layout.Transform.IsSome);
        canvas.Translate(at.X, at.Y);
        if (layout.Transform.Case is Matrix4x4 turn) canvas.Concat((SKMatrix44)turn);
        canvas.DrawDrawable(recorded, 0f, 0f);
    }

    private static void DrawLive(SKCanvas canvas, Pass pass, LayoutNode layout, Context parent, SKPoint at, Option<ExitTransition> departing) {
        Option<Tween> tween = departing.Map(left => new Tween(SKPoint.Empty, left.Last.Frame.Size, left.Opacity.Value(pass.Now))) | pass.Tweened(layout);
        float alpha = departing.IsSome || pass.Animations.Transitions.ContainsKey(layout.Path) ? tween.Map(static held => held.Alpha).IfNone(1f) : layout.Alpha(pass.Cache.Space);
        Option<Matrix4x4> turn = tween.Bind(static held => held.Transform).Map(matrix => Pivoted(layout, matrix)) | layout.Transform;
        using SKAutoCanvasRestore restore = new(canvas, doSave: tween.IsSome || turn.IsSome);
        _ = tween.Iter(held => {
            canvas.Translate(held.Shift.X, held.Shift.Y);
            if (held.Size != layout.Frame.Size) canvas.ClipRect(SKRect.Create(held.Size), antialias: true);
        });
        if (turn.Case is Matrix4x4 matrix) canvas.Concat((SKMatrix44)matrix);
        if (!canvas.QuickReject(layout.LocalSubtreeBounds))
            DrawSelf(canvas, pass, layout, parent.Translated(at) with { Device = canvas.TotalMatrix, StateLayers = StateLayers(pass, layout, parent) }, tween, alpha);
    }

    private static void DrawSelf(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, Option<Tween> tween, float alpha) {
        SKRect local = SKRect.Create(layout.Frame.Size);
        RoleStyle style = layout.Style;
        bool role = layout.Node.Role.IsSome;
        using SKAutoCanvasRestore restore = new(canvas, doSave: false);
        Option<SKImageFilter> filter = role && !style.Filters.IsEmpty
            ? pass.Cache.Filters.GetOrAdd(layout.Path, layout, local, () => Filter.Chained(style.Filters, new FillContext(pass, layout, context, local, 0).Target))
            : None;
        if (alpha < 1f || filter.IsSome)
            _ = canvas.SaveLayer(new SKCanvasSaveLayerRec {
                Bounds = layout.LocalSubtreeBounds, Paint = pass.Cache.Veil(layout, style.Role, alpha, filter),
                Flags = pass.Cache.Layer(style.Look.Blend.IsSome),
            });
        if (layout.Node.Link.Case is Uri link) {
            using SKData annotated = canvas.DrawUrlAnnotation(local, link.AbsoluteUri);
        }
        bool filled = role && layout.Node.Painting.AcceptsBackground && Background(canvas, pass, layout, context, local);
        SKColorF under = Styles.Background(layout.Cascade, style, layout.Node);
        Context inner = context with { Plot = tween.Bind(static held => held.Plane).Map(plane => plane.In(layout.Data)) | Layouts.Plotted(layout) | context.Plot };
        Seq<ResolvedShadow> casts = role && !filled ? Shadows(pass.Scene.Layout.Cascade.Theme, style) : [];
        if (!casts.IsEmpty) _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = layout.LocalSubtreeBounds, Paint = pass.Cache.Cast(layout, style.Role, casts), Flags = pass.Cache.Precision });
        Content(canvas, pass, layout, context, under, tween);
        if (layout.Children.IsEmpty) return;
        Option<SKPaint> fade = EdgeFade(pass.Scene, layout, local).Map(held => pass.Cache.FadeMask(layout, held));
        if (fade.IsSome) _ = canvas.SaveLayer(local, paint: null);
        _ = toSeq(layout.Children.GroupAdjacent(child => layout.Node.Clips(child.Node))).Iter(run => {
            using SKAutoCanvasRestore clipped = new(canvas, doSave: run.Key);
            if (run.Key) _ = pass.Cache.Outlined(layout, local, 0f).Iter(shape => shape.Clip(canvas, SKClipOperation.Intersect));
            _ = toSeq(run).Iter(child => DrawNode(canvas, pass, child, inner, child.Frame.Location + pass.Scene.Shift(layout, child)));
        });
        _ = fade.Iter(mask => canvas.DrawRect(local, mask));
    }

    private static Seq<StateLayer> StateLayers(Pass pass, LayoutNode layout, Context context) =>
        toSeq(Interaction.Items).Choose(interaction => pass.StateLayers.Find(new InteractionState(layout.Path, interaction)).Map(weight => new StateLayer(interaction, weight))) switch {
            var own when own.IsEmpty && layout.Node.Role.IsNone => context.StateLayers,
            var own => own,
        };

    private static HashSet<Seq<Key>> Beneath(HashSet<Seq<Key>> live, Seq<Key> path) => live.Filter(held => held.Count > path.Count && held.Take(path.Count) == path);

    private static Matrix4x4 Pivoted(LayoutNode layout, Matrix4x4 turn) {
        Vector2 pivot = new(layout.Frame.Width * (float)layout.Node.Align.X, layout.Frame.Height * (float)layout.Node.Align.Y);
        return Matrix4x4.CreateTranslation(-pivot.X, -pivot.Y, 0f) * turn * Matrix4x4.CreateTranslation(pivot.X, pivot.Y, 0f);
    }

    // --- [LEAVES]
    internal sealed class Leaf(LayoutNode layout, Pass frame) : SKDrawable {
        private Pass pass = frame;
        private Context parent = new(SKMatrix.Identity, []);
        private SKPoint at;

        internal LayoutNode Layout { get; } = layout;

        internal void Placed(Context context, SKPoint origin) => (parent, at) = (context, origin);

        internal void Framed(Pass next, bool changed) {
            pass = next;
            if (changed) NotifyDrawingChanged();
        }

        protected override void OnDraw(SKCanvas canvas) => DrawLive(canvas, pass, Layout, parent, at, None);

        protected override SKRect OnGetBounds() {
            SKRect reach = Layout.LocalSubtreeBounds;
            Option<Tween> tween = pass.Tweened(Layout);
            SKRect shifted = tween.Map(held => SKRect.Create(reach.Location + held.Shift, reach.Size)).IfNone(reach);
            SKRect turned = (tween.Bind(static held => held.Transform).Map(matrix => Pivoted(Layout, matrix)) | Layout.Transform).Map(matrix => ((SKMatrix44)matrix).Matrix.MapRect(reach)).IfNone(reach);
            return SKRect.Union(SKRect.Union(reach, shifted), turned);
        }
    }

    // --- [ELEVATION]
    private static readonly SKPaint ClearPaint = new() { IsAntialias = true, BlendMode = SKBlendMode.Clear };

    private static Seq<ResolvedShadow> Shadows(ColorScheme theme, RoleStyle style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Color, style).Map(color => new ResolvedShadow(shadow, color))).Strict();

    private static void Elevated(SKCanvas canvas, PaintCache cache, LayoutNode layout, RoleStyle style, SKRect frame, Seq<ResolvedShadow> stack, Seq<Action<SKCanvas>> fills) {
        Seq<ResolvedShadow> outer = stack.Filter(static held => !held.Shadow.Inside);
        Seq<ResolvedShadow> inner = stack.Filter(static held => held.Shadow.Inside);
        if (!outer.IsEmpty) {
            using SKAutoCanvasRestore beneath = new(canvas);
            _ = cache.Outlined(layout, frame, 0f).Iter(shape => shape.Clip(canvas, SKClipOperation.Difference));
            _ = outer.Iter(held => {
                using SKAutoCanvasRestore moved = new(canvas);
                canvas.Translate(held.Shadow.X, held.Shadow.Y);
                _ = cache.Outlined(layout, frame, held.Shadow.Spread).Iter(spread => spread.Draw(canvas, cache.Shadowed(layout, style.Role, held)));
            });
        }
        using (new SKAutoCanvasRestore(canvas)) _ = fills.Iter(fill => fill(canvas));
        if (inner.IsEmpty) return;
        using SKAutoCanvasRestore within = new(canvas);
        _ = cache.Outlined(layout, frame, 0f).Iter(shape => shape.Clip(canvas, SKClipOperation.Intersect));
        _ = inner.Iter(held => {
            float reach = (3f * held.Shadow.Blur) + MathF.Abs(held.Shadow.X) + MathF.Abs(held.Shadow.Y) + MathF.Abs(held.Shadow.Spread);
            using SKAutoCanvasRestore moved = new(canvas);
            canvas.Translate(held.Shadow.X, held.Shadow.Y);
            _ = cache.Outlined(layout, frame, -held.Shadow.Spread).Iter(spread => spread.Inset(canvas, SKRect.Inflate(frame, reach, reach), cache.Shadowed(layout, style.Role, held)));
        });
    }

    // --- [BACKGROUND]
    private static bool Background(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKRect frame) =>
        pass.Cache.Outlined(layout, frame, 0f).Exists(shape => Backed(canvas, pass, layout, context, frame, shape));

    private static bool Backed(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKRect frame, Outline shape) {
        RoleStyle style = layout.Style;
        ColorScheme theme = pass.Scene.Layout.Cascade.Theme;
        if (pass.Native.Contains(layout.Path)) {
            shape.Draw(canvas, ClearPaint);
            return true;
        }
        Seq<Action<SKCanvas>> fills = style.Fills.Map((fill, index) => Fills.Fill(new FillContext(pass, layout, context, frame, index), fill, shape)).Somes().Strict();
        Elevated(canvas, pass.Cache, layout, style, frame, fills.IsEmpty ? [] : Shadows(theme, style), fills);
        if (style.Strokes.IsEmpty) return !fills.IsEmpty;
        SKColorF under = layout.Cascade.Background;
        bool bounding = layout.Node.Part.IsSome && style.Solid.Bind(value => theme.Color(value, style)).ForAll(solid => Styles.Ratio(solid, under) < theme.Contrast.Graphic);
        _ = pass.Cache.Outlined(layout, frame, -style.HalfStroke).Iter(rim => style.Strokes.Map(static (stroke, index) => (Stroke: stroke, Index: index)).Iter(held =>
            theme.WithStateLayers(pass.Scene.ColorCache, held.Stroke.Color, style, context.StateLayers)
                .Map(color => bounding ? Styles.Legible(pass.Scene.ColorCache, style, under, color, theme.Contrast.Graphic) : color)
                .Iter(color => rim.Draw(canvas, PaintCache.StrokePaint(pass.Cache.Edges, new PaintSlot(layout.Path, held.Index), layout,
                    new StrokeKey(color, style.Metrics[Metric.Stroke], held.Stroke.Patterned(0u), held.Stroke.Timed ? pass.Clock : 0f, None))))));
        return !fills.IsEmpty;
    }

    // --- [RING]
    private static void FocusRing(SKCanvas canvas, Pass pass, PlacedNode ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.Concat(ring.Place.Transform);
        canvas.ClipRect(ring.Place.Clip, antialias: true);
        if (weight < 1f)
            _ = canvas.SaveLayer(new SKCanvasSaveLayerRec {
                Bounds = SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), Paint = pass.Cache.Veil(ring.Layout, ring.Style.Role, weight, None), Flags = pass.Cache.Precision,
            });
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        Elevated(canvas, pass.Cache, ring.Layout, ring.Style, SKRect.Create(ring.Frame.Size), Shadows(pass.Scene.Layout.Cascade.Theme, ring.Style), []);
    }

    // --- [FADE]
    private static Option<Fade> EdgeFade(Scene scene, LayoutNode layout, SKRect viewport) =>
        from fade in layout.Node.Role.IsSome ? Some(layout.Style.Metrics[Metric.EdgeFade]) : None
        where fade > 0f
        from scroll in scene.Adjustment(layout.Path)
        let hidden = (Before: scroll.Value > 0f, After: scroll.Value < scroll.Overflow.Max)
        let length = scroll.Axis.Of(viewport.Size)
        where (hidden.Before || hidden.After) && length > 0f
        select new Fade(viewport.Location, (SKPoint)scroll.Axis.With(viewport.Location, scroll.Axis.Of(viewport.Location) + length), hidden.Before, hidden.After, float.Min(fade, length / 2f) / length);

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKColorF under, Option<Tween> tween) =>
        layout.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Under: under, Tween: tween),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Under, held.Tween),
            text: static (held, text) => DrawText(held.Canvas, held.Pass, text, held.Context, held.Under));

    private static void Contained(SKCanvas canvas, Pass pass, LayoutNode.Box layout, Context context, SKColorF under, Option<Tween> tween) =>
        layout.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Layout: layout, Context: context, Under: under, Tween: tween),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marks.Mark(held.Canvas, held.Pass, held.Layout, held.Context, held.Tween.Bind(static tween => tween.Figure).IfNone(mark.Figure), mark.Series, plot)),
            glyph: static (held, glyph) => held.Pass.Scene.Layout.Cascade.Theme.WithStateLayers(held.Pass.Scene.ColorCache, held.Layout.Style.Text, held.Layout.Style, held.Context.StateLayers)
                .Map(resolved => Styles.Legible(held.Pass.Scene.ColorCache, held.Layout.Style, held.Under, resolved, held.Pass.Scene.Layout.Cascade.Theme.Contrast.Graphic))
                .Iter(tint => Glyph(held.Canvas, held.Pass, held.Layout, glyph.Art, held.Under, held.Context.Pen(held.Layout.Style), tint)),
            image: static (held, image) => Marks.Image(held.Canvas, held.Pass, held.Layout, held.Context, image),
            slot: static (held, _) => held.Canvas.DrawRect(SKRect.Create(held.Layout.Frame.Size), ClearPaint));

    private static void Glyph(SKCanvas canvas, Pass pass, LayoutNode layout, GlyphSource source, SKColorF under, Pen pen, SKColorF tint) {
        ArtKey key = Media.Keyed(layout, source, under, pass.Scene.ColorCache);
        _ = pass.Cache.Art(key).Iter(media => {
            SKRect cull = media.Bounds;
            SKRect fitted = ImageFit.Contain.Fitted(layout.ContentBox, cull.Size);
            SKPoint origin = pen.Snapped(fitted.Location);
            (float x, float y) = (fitted.Width / cull.Width, fitted.Height / cull.Height);
            media.Draw(canvas, SKMatrix.CreateScaleTranslation(x, y, origin.X - (cull.Left * x), origin.Y - (cull.Top * y)), key.Art.Tinted ? Some(pass.Cache.Tint(layout, tint)) : None);
        });
    }

    private static void DrawText(SKCanvas canvas, Pass pass, LayoutNode.Text layout, Context context, SKColorF under) =>
        _ = pass.Scene.Layout.Cascade.Theme.WithStateLayers(pass.Scene.ColorCache, layout.Style.Text, layout.Style, context.StateLayers)
            .Map(resolved => Styles.Legible(pass.Scene.ColorCache, layout.Style, under, resolved, pass.Scene.Layout.Cascade.Theme.Contrast.Text))
            .Iter(legible => {
                if (layout.Node is Node.Mark { Figure: Figure.Label label }) Marks.Label(canvas, pass, layout, context, label, legible);
                else if (layout.Typeset.Setting.Style.Stroked.Case is float width)
                    canvas.DrawPath(pass.Cache.TextOutlines.GetOrAdd(layout.Path, layout, new TextOutlineKey(layout.Typeset, layout.Origin), () => Typesetting.Outline(layout.Typeset, layout.Origin)),
                        PaintCache.StrokePaint(pass.Cache.TextStrokes, layout.Path, layout, new StrokeKey(legible, width, [], 0f, None)));
                else Block(pass, layout, (SKColor)legible).Paint(canvas, layout.Origin, Typesetting.Options(layout.Typeset, pass.Cache.Raster));
            });

    private static TextBlock Block(Pass pass, LayoutNode.Text layout, SKColor tint) =>
        tint == layout.TextStyle.TextColor
            ? layout.Typeset.Lines
            : pass.Cache.Texts.GetOrAdd(new TextKey(layout.Path, tint), layout, layout.Typeset, () => Typesetting.Tinted(pass.Scene.Shaping, layout.Typeset, tint));
}
