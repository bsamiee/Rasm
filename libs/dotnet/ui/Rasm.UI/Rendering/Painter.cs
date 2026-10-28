using System.Numerics;
using Rasm.UI.Plotting;
using Rasm.UI.Styling;
using Rasm.UI.Typography;
using SkiaSharp;
using SuperLinq;

namespace Rasm.UI.Rendering;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Painter {
    // --- [SCENE]
    internal sealed record Pass(PaintCache Cache, Scene Scene, HashMap<InteractionState, float> StateLayers, Animations Animations, HashSet<Seq<Key>> Live, Seq<Seq<Key>> Hovered, Option<SKPoint> Pointer, Duration Now) {
        internal float Clock => (float)Now.TotalSeconds;

        internal Option<SKPoint> PointerAt(LayoutNode layout) =>
            Hovered.Exists(hovered => hovered == layout.Path) ? from at in Pointer from frame in Scene.Frame(layout.Path) select at - frame.Location : None;

        internal Option<Outline> Outlined(LayoutNode layout, SKRect frame, float offset) => Outline.Of(Scene.Outlines, layout, frame, offset).ToOption();
    }

    internal static Fin<Unit> Paint(SKCanvas canvas, PaintCache cache, Mount mount, SKRegion damage, Duration now, Option<Seq<Key>> popup) {
        Scene scene = mount.Scene;
        return cache.Retain(mount, canvas.TotalMatrix.MapRadius(1f)).Map(_ => {
            HashMap<InteractionState, float> stateLayers = mount.StateLayers(now);
            Animations animations = mount.Animations;
            Pass pass = new(cache, scene, stateLayers, animations,
                cache.Moving.Union(stateLayers.Keys.Map(static key => key.Path)).Union(animations.Transitions.Keys).Union(animations.Transforms.Keys).Union(animations.Planes.Keys).Union(animations.Figures.Keys),
                mount.Hover, mount.Routing.Pointer, now);
            Option<(LayoutNode Layer, SKPoint Origin)> shown =
                from path in popup
                from layer in scene.Layers.Find(held => held.Path == path)
                from hosted in mount.Hosted.Find(held => held is Hosted.Popup && held.Path == path)
                select (layer, hosted.Frame.Location);
            bool Shown(Seq<Key> path) => popup.Match(Some: root => Under(path, root), None: () => !scene.Popups.Exists(root => Under(path, root)));
            cache.Framed(pass);
            using SKAutoCanvasRestore restore = new(canvas);
            canvas.ClipRegion(damage);
            canvas.Clear(SKColors.Transparent);
            if (popup.IsSome && shown.IsNone) return unit;
            (LayoutNode root, SKPoint origin) = shown.IfNone((scene.Layout, SKPoint.Empty));
            canvas.Translate(-origin.X, -origin.Y);
            DrawNode(canvas, pass, root, new Context(canvas.TotalMatrix), root.Frame.Location);
            _ = scene.Fragments.Filter((plot, _) => Shown(plot)).Iter((plot, fragment) => Fragment(canvas, pass, plot, fragment));
            _ = animations.Leaving.Filter(left => Shown(left.Last.Path)).Iter(left => Departed(canvas, pass, left));
            _ = scene.Layers.Filter(held => popup.IsNone && !scene.Popups.Contains(held.Path)).Iter(held => DrawNode(canvas, pass, held, new Context(canvas.TotalMatrix), held.Frame.Location));
            return stateLayers.Filter((key, _) => key.Interaction == Interaction.Focused && Shown(key.Path)).Iter((key, weight) => scene.FocusRing(key.Path).Iter(ring => FocusRing(canvas, pass, ring, weight)));
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
                 DrawNode(canvas, pass, fragment, new Context(canvas.TotalMatrix) { Plot = Layouts.Plotted(held.Layout).Map(own => own.Translated(frame.Location)) | held.Place.Plot },
                     frame.Location + pass.Scene.Shift(held.Layout, fragment) + fragment.Frame.Location);
             });

    private static void Departed(SKCanvas canvas, Pass pass, ExitTransition left) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.Concat(left.Place.Transform);
        canvas.ClipRect(left.Place.Clip, antialias: true);
        canvas.Translate(left.At.X, left.At.Y);
        DrawLive(canvas, pass, left.Last, new Context(canvas.TotalMatrix) { Plot = left.Place.Plot }, left.At, Some(left));
    }

    private static bool Under(Seq<Key> path, Seq<Key> root) => path.Count >= root.Count && path.Take(root.Count) == root;

    // --- [NODES]
    private static void DrawNode(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKPoint at) {
        if (pass.Live.Contains(layout.Path) || (layout.Node.Role.IsNone && !context.StateLayers.IsEmpty)) {
            Leaf leaf = pass.Cache.Leaves.GetOrAdd(layout, () => new Leaf(layout, pass));
            leaf.Placed(context, at);
            canvas.DrawDrawable(leaf, at.X, at.Y);
            return;
        }
        Context own = context.Translated(at) with { StateLayers = StateLayers(pass, layout, context) };
        float alpha = layout.Alpha(pass.Cache.Space);
        SKDrawable recorded = pass.Cache.Recordings.GetOrAdd((layout, own.CacheKey, alpha, pass.Live.Filter(held => held != layout.Path && Under(held, layout.Path))), () => {
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
        Option<Tween> tween = departing.Map(left => new Tween(left.Last.Frame, left.Opacity.Value(pass.Now))) | pass.Animations.Tweened(layout, pass.Now);
        float alpha = departing.IsSome || pass.Animations.Transitions.ContainsKey(layout.Path) ? tween.Map(static held => held.Alpha).IfNone(1f) : layout.Alpha(pass.Cache.Space);
        Option<Matrix4x4> turn = Turn(layout, tween);
        using SKAutoCanvasRestore restore = new(canvas, doSave: tween.IsSome || turn.IsSome);
        _ = tween.Iter(held => {
            SKPoint shift = held.Shift(layout);
            canvas.Translate(shift.X, shift.Y);
            if (held.Frame.Size != layout.Frame.Size) canvas.ClipRect(SKRect.Create(held.Frame.Size), antialias: true);
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
            ? new FillContext(pass, layout, context, local, 0) switch {
                var fill => pass.Cache.Filters.GetOrAdd(fill.Key(0f, layout.Timed ? pass.Clock : 0f, style.Pointing ? pass.PointerAt(layout) : None), () => Filter.Chained(style.Filters, fill.Target)),
            }
            : None;
        if (alpha < 1f || filter.IsSome)
            _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = layout.LocalSubtreeBounds, Paint = pass.Cache.Opacity(alpha, filter), Flags = pass.Cache.Layer(style.Look.Blend.IsSome) });
        if (layout.Node.Link.Case is Uri link) {
            using SKData annotated = canvas.DrawUrlAnnotation(local, link.AbsoluteUri);
        }
        bool filled = role && layout.Node.Painting.AcceptsBackground && pass.Outlined(layout, local, 0f).Exists(shape => Background(canvas, pass, layout, context, local, shape));
        Seq<(Shadow Shadow, SKColorF Color)> casts = role && !filled ? Shadows(layout.Cascade.Theme, style) : [];
        if (!casts.IsEmpty) _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = layout.LocalSubtreeBounds, Paint = pass.Cache.Cast(casts), Flags = pass.Cache.Precision });
        Content(canvas, pass, layout, context, Styles.Background(layout.Cascade, style, layout.Node), tween);
        if (layout.Children.IsEmpty) return;
        Context inner = context with { Plot = tween.Bind(static held => held.Plane).Map(plane => plane.In(layout.Data)) | Layouts.Plotted(layout) | context.Plot };
        Option<SKPaint> fade = EdgeFade(pass, layout, local);
        if (fade.IsSome) _ = canvas.SaveLayer(local, paint: null);
        _ = toSeq(layout.Children.GroupAdjacent(child => layout.Node.Clips(child.Node))).Iter(run => {
            using SKAutoCanvasRestore clipped = new(canvas, doSave: run.Key);
            if (run.Key) _ = pass.Outlined(layout, local, 0f).Iter(shape => shape.Clip(canvas, SKClipOperation.Intersect));
            _ = toSeq(run).Iter(child => DrawNode(canvas, pass, child, inner, child.Frame.Location + pass.Scene.Shift(layout, child)));
        });
        _ = fade.Iter(mask => canvas.DrawRect(local, mask));
    }

    private static HashMap<Interaction, float> StateLayers(Pass pass, LayoutNode layout, Context context) =>
        toHashMap(toSeq(Interaction.Items).Choose(interaction => pass.StateLayers.Find(new InteractionState(layout.Path, interaction)).Map(weight => (interaction, weight)))) switch {
            var own when own.IsEmpty && layout.Node.Role.IsNone => context.StateLayers,
            var own => own,
        };

    private static Option<Matrix4x4> Turn(LayoutNode layout, Option<Tween> tween) => tween.Bind(static held => held.Transform).Map(matrix => layout.Turned(matrix, Vector2.Zero)) | layout.Transform;

    // --- [LEAVES]
    internal sealed class Leaf(LayoutNode layout, Pass frame) : SKDrawable {
        private Pass pass = frame;
        private Context parent = new(SKMatrix.Identity);
        private SKPoint at;

        internal void Placed(Context context, SKPoint origin) => (parent, at) = (context, origin);

        internal void Framed(Pass next) => pass = next;

        protected override void OnDraw(SKCanvas canvas) => DrawLive(canvas, pass, layout, parent, at, None);

        protected override SKRect OnGetBounds() {
            SKRect reach = layout.LocalSubtreeBounds;
            Option<Tween> tween = pass.Animations.Tweened(layout, pass.Now);
            SKRect shifted = tween.Map(held => SKRect.Create(reach.Location + held.Shift(layout), reach.Size)).IfNone(reach);
            SKRect turned = Turn(layout, tween).Map(matrix => ((SKMatrix44)matrix).Matrix.MapRect(reach)).IfNone(reach);
            return SKRect.Union(SKRect.Union(reach, shifted), turned);
        }
    }

    // --- [ELEVATION]
    private static readonly SKPaint ClearPaint = new() { IsAntialias = true, BlendMode = SKBlendMode.Clear };

    private static Seq<(Shadow Shadow, SKColorF Color)> Shadows(Theme theme, RoleStyle style) =>
        style.Shadows.Choose(shadow => theme.Color(shadow.Color, style).Map(color => (Shadow: shadow, Color: color))).Strict();

    private static void Elevated(SKCanvas canvas, Pass pass, LayoutNode layout, SKRect frame, Seq<(Shadow Shadow, SKColorF Color)> stack, Seq<Action<SKCanvas>> fills) {
        Seq<(Shadow Shadow, SKColorF Color)> outer = stack.Filter(static held => !held.Shadow.Inside);
        Seq<(Shadow Shadow, SKColorF Color)> inner = stack.Filter(static held => held.Shadow.Inside);
        if (!outer.IsEmpty) {
            using SKAutoCanvasRestore beneath = new(canvas);
            _ = pass.Outlined(layout, frame, 0f).Iter(shape => shape.Clip(canvas, SKClipOperation.Difference));
            _ = outer.Iter(held => {
                using SKAutoCanvasRestore moved = new(canvas);
                canvas.Translate(held.Shadow.X, held.Shadow.Y);
                _ = pass.Outlined(layout, frame, held.Shadow.Spread).Iter(spread => spread.Draw(canvas, pass.Cache.Shadowed(held)));
            });
        }
        using (new SKAutoCanvasRestore(canvas)) _ = fills.Iter(fill => fill(canvas));
        if (inner.IsEmpty) return;
        using SKAutoCanvasRestore within = new(canvas);
        _ = pass.Outlined(layout, frame, 0f).Iter(shape => shape.Clip(canvas, SKClipOperation.Intersect));
        _ = inner.Iter(held => {
            float reach = Shadow.Outset([held.Shadow with { Inside = false, Spread = MathF.Abs(held.Shadow.Spread) }]);
            using SKAutoCanvasRestore moved = new(canvas);
            canvas.Translate(held.Shadow.X, held.Shadow.Y);
            _ = pass.Outlined(layout, frame, -held.Shadow.Spread).Iter(spread => spread.Inset(canvas, SKRect.Inflate(frame, reach, reach), pass.Cache.Shadowed(held)));
        });
    }

    // --- [BACKGROUND]
    private static bool Background(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKRect frame, Outline shape) {
        RoleStyle style = layout.Style;
        if (pass.Scene.Backdrops.Contains(layout.Path)) {
            shape.Draw(canvas, ClearPaint);
            return true;
        }
        Seq<Action<SKCanvas>> fills = style.Fills.Map((fill, index) => Fills.Fill(new FillContext(pass, layout, context, frame, index), fill, shape)).Somes().Strict();
        Elevated(canvas, pass, layout, frame, fills.IsEmpty ? [] : Shadows(layout.Cascade.Theme, style), fills);
        if (style.Strokes.IsEmpty) return !fills.IsEmpty;
        SKColorF under = layout.Cascade.Background;
        bool bounding = layout.Node.Part.IsSome && style.Solid.Bind(value => layout.Cascade.Theme.Color(value, style)).ForAll(solid => Styles.Ratio(solid, under) < layout.Cascade.Theme.Floor(text: false));
        _ = pass.Outlined(layout, frame, -style.HalfStroke).Iter(rim => style.Strokes.Iter(stroke =>
            context.Color(stroke.Color, layout)
                .Map(color => bounding ? layout.Cascade.Theme.Legible(style, under, color, text: false) : color)
                .Iter(color => rim.Draw(canvas, pass.Cache.Stroke(stroke, color, style.Metrics[Metric.Stroke], 0u, pass.Clock, None)))));
        return !fills.IsEmpty;
    }

    // --- [RING]
    private static void FocusRing(SKCanvas canvas, Pass pass, PlacedNode ring, float weight) {
        using SKAutoCanvasRestore restore = new(canvas);
        canvas.Concat(ring.Place.Transform);
        canvas.ClipRect(ring.Place.Clip, antialias: true);
        if (weight < 1f)
            _ = canvas.SaveLayer(new SKCanvasSaveLayerRec { Bounds = SKRect.Inflate(ring.Frame, ring.Style.Overhang, ring.Style.Overhang), Paint = pass.Cache.Opacity(weight, None), Flags = pass.Cache.Precision });
        canvas.Translate(ring.Frame.Left, ring.Frame.Top);
        Elevated(canvas, pass, ring.Layout, SKRect.Create(ring.Frame.Size), Shadows(ring.Layout.Cascade.Theme, ring.Style), []);
    }

    // --- [FADE]
    private static Option<SKPaint> EdgeFade(Pass pass, LayoutNode layout, SKRect viewport) =>
        from fade in layout.Node.Role.IsSome ? Some(layout.Style.Metrics[Metric.EdgeFade]) : None
        where fade > 0f
        from scroll in pass.Scene.Scroller(layout.Path)
        let hidden = (Before: scroll.Value > 0f, After: scroll.Value < scroll.Overflow.Max)
        let length = scroll.Axis.Of(viewport.Size)
        where (hidden.Before || hidden.After) && length > 0f
        select pass.Cache.FadeMask((viewport.Location, (SKPoint)scroll.Axis.With(viewport.Location, scroll.Axis.Of(viewport.Location) + length), hidden.Before, hidden.After, float.Min(fade, length / 2f) / length));

    // --- [CONTENT]
    private static void Content(SKCanvas canvas, Pass pass, LayoutNode layout, Context context, SKColorF under, Option<Tween> tween) =>
        layout.Switch(
            (Canvas: canvas, Pass: pass, Context: context, Under: under, Tween: tween),
            box: static (held, box) => Contained(held.Canvas, held.Pass, box, held.Context, held.Under, held.Tween),
            text: static (held, text) => DrawText(held.Canvas, held.Pass, text, held.Context, held.Under));

    private static void Contained(SKCanvas canvas, Pass pass, LayoutNode.Box layout, Context context, SKColorF under, Option<Tween> tween) =>
        layout.Node.SwitchPartially(
            (Canvas: canvas, Pass: pass, Layout: layout, Context: context, Under: under, Tween: tween),
            mark: static (held, mark) => held.Context.Plot.Iter(plot => Marks.Mark(held.Canvas, held.Pass, held.Layout, held.Context, held.Tween.Bind(static tween => tween.Figure).IfNone(mark.Figure), plot)),
            glyph: static (held, glyph) => held.Pass.Cache.Art(held.Layout, glyph.Art).Iter(media => {
                SKRect fitted = media.Fitted(held.Layout.ContentBox);
                media.Draw(held.Canvas, SKRect.Create(held.Context.Pen(held.Layout.Style).Snapped(fitted.Location), fitted.Size), glyph.Art.Tinted
                    ? held.Context.Color(held.Layout.Style.Text, held.Layout).Map(resolved => held.Pass.Cache.Tint(media, held.Layout.Cascade.Theme.Legible(held.Layout.Style, held.Under, resolved, text: false)))
                    : None);
            }),
            image: static (held, image) => Marks.Image(held.Canvas, held.Pass, held.Layout, held.Context, image),
            slot: static (held, _) => held.Canvas.DrawRect(SKRect.Create(held.Layout.Frame.Size), ClearPaint));

    private static void DrawText(SKCanvas canvas, Pass pass, LayoutNode.Text layout, Context context, SKColorF under) =>
        _ = context.Color(layout.Style.Text, layout).Map(resolved => layout.Cascade.Theme.Legible(layout.Style, under, resolved, text: true)).Iter(legible => {
            if (layout.Node is Node.Mark { Figure: Figure.Label label }) Marks.Label(canvas, pass, layout, context, label, legible);
            else if (layout.Typeset.Style.Stroked.Case is float width) {
                using SKAutoCanvasRestore placed = new(canvas);
                canvas.Translate(layout.Origin.X, layout.Origin.Y);
                canvas.DrawPath(pass.Cache.TextOutlines.GetOrAdd(layout.Typeset, () => Typesetting.Outline(layout.Typeset)), pass.Cache.Stroke(new Stroke(layout.Style.Text), legible, width, 0u, 0f, None));
            } else Typesetting.Paint(canvas, layout.Typeset, layout.Origin, pass.Cache.Raster, (SKColor)legible);
        });
}
