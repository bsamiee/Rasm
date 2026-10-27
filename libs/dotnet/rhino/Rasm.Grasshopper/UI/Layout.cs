using Eto.Drawing;
using Eto.Forms;
using Grasshopper2;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.SpecialObjects;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Snap;
using Grasshopper2.Undo;
using Grasshopper2.Undo.Actions;
using Nanoray.Kiwi;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Grasshopper.Doc;
using SuperLinq;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Move(IDocumentObject Object, PointF Pivot);

public sealed record Distribution(Orientation Axis, Seq<Guid> Objects);

public sealed record Arrangement(HashSet<Guid> Objects, SnappingSettings Settings, Seq<Distribution> Distributions, Option<RectangleF> Frame);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record SnapSession(SnappingConstraints Constraints, RectangleF Start, Option<IDocumentObject> Primary) {
    public static IO<SnapSession> Begin(Document document, RectangleF start, Seq<Guid> moving, Option<IDocumentObject> primary) =>
        IO.lift(() => new SnapSession(SnappingConstraints.CreateFromDocument(document, [.. moving]), start, primary));

    public IO<SizeF> Step(Canvas canvas, SizeF offset, bool orthogonal) =>
        IO.lift(() => {
            SnappingSettings settings = SnappingSettings.Current;
            RectangleF target = Start.Move(offset.Width, offset.Height);
            (Option<SnappingAction> edgeX, Option<SnappingAction> edgeY) = Settings.CanvasSnapToObjects.Value ? Snaps() : (None, None);
            bool wide = Math.Abs(offset.Width) > Math.Abs(offset.Height);
            PointF center = Eto.Drawing.Point.Round(Start.Center);
            Option<SnappingAction> x = orthogonal && !wide ? Some(SnappingAction.VerticalOrthoAction(center, canvas.VisibleFrame)) : edgeX;
            Option<SnappingAction> y = orthogonal && wide ? Some(SnappingAction.HorizontalOrthoAction(center, canvas.VisibleFrame)) : edgeY;
            canvas.SnapXAction = x.ValueUnsafe();
            canvas.SnapYAction = y.ValueUnsafe();
            return new SizeF(
                (orthogonal && !wide ? 0f : offset.Width) + x.Map(static action => action.ΔX).IfNone(0f),
                (orthogonal && wide ? 0f : offset.Height) + y.Map(static action => action.ΔY).IfNone(0f));

            (Option<SnappingAction> X, Option<SnappingAction> Y) Snaps() {
                Constraints.SnapRectangle(target, settings, canvas.VisibleFrame, out SnappingAction? snapX, out SnappingAction? snapY);
                return (Optional(snapX), Optional(SnappingAction.SmallerMagnitude(snapY, SnappingConstraints.SnapWires(Primary.ValueUnsafe(), target, settings))));
            }
        });

    public static IO<Unit> End(Canvas canvas) =>
        IO.lift(() => {
            canvas.SnapXAction = null;
            canvas.SnapYAction = null;
        });
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CanvasLayout {
    // --- [READS]
    private sealed record Footprint(IDocumentObject Object, int Order);

    private sealed record WireLevel(Guid Source, Guid Target, float Offset);

    private static Seq<Footprint> Footprints(Document document, HashSet<Guid> objects) =>
        toSeq(document.Objects.Forwards)
            .Map(static (item, order) => new Footprint(item, order))
            .Filter(entry => objects.Contains(entry.Object.InstanceId) && (entry.Object switch {
                GroupObject group => !group.Content.Any(),
                IPin pin => document.Objects.Find(pin.PinningTarget) is null,
                _ => true,
            }))
            .Strict();

    private static Seq<Guid> Movable(Document document, IDocumentObject item) =>
        item switch {
            IPin pin => Optional(document.Objects.Find(pin.PinningTarget)).Match(Some: target => Movable(document, target), None: () => Seq(item.InstanceId)),
            GroupObject group when group.Content.Any() => toSeq(group.Content).Bind(member => Movable(document, member)),
            _ => Seq(item.InstanceId),
        };

    private static Seq<Move> Changed(Seq<Footprint> rows, Func<Footprint, PointF> pivot) =>
        rows.Map(row => new Move(row.Object, pivot(row))).Filter(static move => move.Pivot != move.Object.Attributes.Pivot).Strict();

    // --- [MOVES]
    public static IO<bool> Moved(Document document, VerbNoun name, Seq<Move> moves) =>
        Edits.Commit(document, name, AutoSaveReason.ObjectChanged, actions =>
            from captured in IO.lift(() => moves.Iter(move => actions.Add(new PivotAction(move.Object))))
            from placed in IO.lift(() => moves.Iter(static move => move.Object.Attributes.Pivot = move.Pivot))
            select !moves.IsEmpty);

    public static IO<bool> Nudged(Document document, SizeF offset, VerbNoun name) =>
        IO.lift(() => Changed(Footprints(document, toHashSet(toSeq(document.Objects.SelectedObjects).Bind(item => Movable(document, item)))), row => row.Object.Attributes.Pivot + offset))
            .Bind(moves => Moved(document, name, moves));

    public static IO<bool> Snapped(Document document, HashSet<Guid> objects, SnapSpace space, VerbNoun name) =>
        IO.lift(() => Changed(Footprints(document, objects), row => {
            space.Snap(row.Object.Attributes.Pivot.X, row.Object.Attributes.Pivot.Y, out double x, out double y, out _);
            return new PointF((float)x, (float)y);
        })).Bind(moves => Moved(document, name, moves));

    public static IO<Unit> Nudge(Canvas canvas, IGrasshopperPlugin owner) =>
        Hooks.On<KeyEventArgs>(h => canvas.KeyDown += h, h => canvas.KeyDown -= h, args =>
            (from document in Optional(canvas.Document).Filter(_ => !args.Handled)
             from direction in args.Key switch {
                 Keys.Left => Some(new SizeF(-1f, 0f)),
                 Keys.Right => Some(new SizeF(1f, 0f)),
                 Keys.Up => Some(new SizeF(0f, -1f)),
                 Keys.Down => Some(new SizeF(0f, 1f)),
                 _ => Option<SizeF>.None,
             }
             from step in args.Modifiers switch {
                 Keys.None => Some(new SizeF(1f, 1f)),
                 Keys.Shift => Some(new SizeF(SnappingSettings.Current.HorizontalGapSize, SnappingSettings.Current.VerticalGapSize)),
                 _ => Option<SizeF>.None,
             }
             select Nudged(document, direction * step, new VerbNoun("Nudge", "Selection")))
            .Match(
                Some: nudged => nudged.Bind(moved => when(moved, IO.lift(() => {
                    args.Handled = true;
                    canvas.Invalidate();
                })).As()),
                None: static () => unitIO), owner);

    // --- [ARRANGEMENT]
    public static IO<bool> Arrange(Document document, Arrangement request, VerbNoun name, Option<Seq<WireEnds>> wires = default) =>
        from ranks in IO.lift(() => WireGraph.Of(document.Objects.Connectivity).Ranks())
        from rows in IO.lift(() => Footprints(document, request.Objects).Filter(static row => row.Object.Attributes.Snappable).Strict())
        from levels in IO.lift(() => wires.IfNone(() => toSeq(document.Objects.AllWires)).Choose(wire =>
            from outlet in Optional(document.Objects.FindParameter(wire.Source)?.Attributes as IParameterAttributes).Filter(static ends => ends.HasOutlet)
            from inlet in Optional(document.Objects.FindParameter(wire.Target)?.Attributes as IParameterAttributes).Filter(static ends => ends.HasInlet)
            from source in rows.Find(row => row.Object.InstanceId == Connectivity.GetConnectiveId(document, wire.Source))
            from target in rows.Find(row => row.Object.InstanceId == Connectivity.GetConnectiveId(document, wire.Target))
            where source.Object.InstanceId != target.Object.InstanceId
            select new WireLevel(source.Object.InstanceId, target.Object.InstanceId, outlet.Outlet.Y - source.Object.Attributes.Pivot.Y - (inlet.Inlet.Y - target.Object.Attributes.Pivot.Y))).Strict())
        let side = Pairs(rows, ranks, Orientation.Horizontal)
        let stack = Pairs(rows, ranks, Orientation.Vertical)
        let x = Solved(request, rows, side, stack, levels, Orientation.Horizontal)
        let y = Solved(request, rows, stack, side, levels, Orientation.Vertical)
        from moved in Moved(document, name, Changed(rows, row => new PointF(x[row.Object.InstanceId], y[row.Object.InstanceId])))
        select moved;

    private static Seq<(Footprint A, Footprint B)> Pairs(Seq<Footprint> rows, HashMap<Guid, int> ranks, Orientation axis) {
        return rows.Choose(a => Optional(rows.Filter(b => Precedence(b).CompareTo(Precedence(a)) > 0 && Adjacent(a.Object.Attributes.AggregateBounds, b.Object.Attributes.AggregateBounds)).MinBy(Precedence)).Map(b => (a, b)));

        (double Key, int Order) Precedence(Footprint row) =>
            axis is Orientation.Horizontal ? (ranks[row.Object.InstanceId], row.Order) : (row.Object.Attributes.AggregateBounds.MiddleY, row.Order);

        bool Adjacent(RectangleF a, RectangleF b) => axis is Orientation.Horizontal
            ? a.Top <= b.Bottom && b.Top <= a.Bottom && (a.Right < b.Left || b.Right < a.Left)
            : a.Left <= b.Right && b.Left <= a.Right;
    }

    private static HashMap<Guid, float> Solved(Arrangement request, Seq<Footprint> rows, Seq<(Footprint A, Footprint B)> along, Seq<(Footprint A, Footprint B)> across, Seq<WireLevel> wires, Orientation axis) {
        Func<PointF, float> coordinate = axis is Orientation.Horizontal ? static point => point.X : static point => point.Y;
        Func<RectangleF, float> low = axis is Orientation.Horizontal ? static box => box.Left : static box => box.Top;
        Func<RectangleF, float> high = axis is Orientation.Horizontal ? static box => box.Right : static box => box.Bottom;
        HashMap<Guid, Variable> pivots = toHashMap(rows.Map(static row => (row.Object.InstanceId, new Variable())));
        Seq<(SnappingRule Rule, Orientation Axis, Func<RectangleF, float> Edge)> rules = Seq<(SnappingRule, Orientation, Func<RectangleF, float>)>(
            (SnappingRule.AlignLeftEdges, Orientation.Horizontal, static box => box.Left),
            (SnappingRule.AlignMiddles, Orientation.Horizontal, static box => box.MiddleX),
            (SnappingRule.AlignRightEdges, Orientation.Horizontal, static box => box.Right),
            (SnappingRule.AlignTopEdges, Orientation.Vertical, static box => box.Top),
            (SnappingRule.AlignBottomEdges, Orientation.Vertical, static box => box.Bottom));
        Seq<(Footprint A, Footprint B, Func<RectangleF, float> Edge)> aligned =
            from pair in across
            from rule in toSeq(rules.Filter(rule => rule.Axis == axis && request.Settings.Rules.HasFlag(rule.Rule))
                .OrderBy(rule => Math.Abs(rule.Edge(pair.A.Object.Attributes.AggregateBounds) - rule.Edge(pair.B.Object.Attributes.AggregateBounds))).Take(1))
            select (pair.A, pair.B, rule.Edge);
        Option<int> gap = axis is Orientation.Horizontal
            ? Some(request.Settings.HorizontalGapSize).Filter(_ => request.Settings.SpaceHorizontalGaps)
            : Some(request.Settings.VerticalGapSize).Filter(_ => request.Settings.SpaceVerticalGaps);
        Seq<(Footprint A, Footprint B, Footprint C)> spread = request.Distributions.Filter(run => run.Axis == axis)
            .Map(run => run.Objects.Choose(id => rows.Find(row => row.Object.InstanceId == id)))
            .Bind(static run => toSeq(run.Window(3, static window => (window[0], window[1], window[2]))));
        Seq<WireLevel> levelled = axis is Orientation.Vertical && request.Settings.AlignWires ? wires : Seq<WireLevel>();
        Seq<(Guid A, Guid B)> links = along.Map(static pair => (pair.A.Object.InstanceId, pair.B.Object.InstanceId))
            .Concat(aligned.Map(static pair => (pair.A.Object.InstanceId, pair.B.Object.InstanceId)))
            .Concat(spread.Bind(static triple => Seq((triple.A.Object.InstanceId, triple.B.Object.InstanceId), (triple.B.Object.InstanceId, triple.C.Object.InstanceId))))
            .Concat(levelled.Map(static wire => (wire.Source, wire.Target)));
        IOrderedEnumerable<Footprint> ordered = rows.OrderBy(row => (coordinate(row.Object.Attributes.Pivot), row.Order));
        HashMap<Guid, double> strengths = toHashMap(
            from component in toSeq(rows.Map(static row => row.Object.InstanceId)
            .ToAdjacencyGraph(
                vertex => links.Filter(link => link.A == vertex).Map(static link => new SEquatableEdge<Guid>(link.A, link.B)),
                allowParallelEdges: false)
            .CondensateWeaklyConnected<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>().Vertices)
            let anchor = ordered.First(row => component.ContainsVertex(row.Object.InstanceId)).Object.InstanceId
            from vertex in toSeq(component.Vertices)
            select (vertex, vertex == anchor ? Strength.Medium : Strength.Weak / component.VertexCount));
        Solver solver = new();
        _ = along.Map(pair => Constraint.LessEqual(At(pair.A, high(pair.A.Object.Attributes.AggregateBounds)), At(pair.B, low(pair.B.Object.Attributes.AggregateBounds)), Strength.Required))
            .Concat(aligned.Map(pair => Constraint.Equal(At(pair.A, pair.Edge(pair.A.Object.Attributes.AggregateBounds)), At(pair.B, pair.Edge(pair.B.Object.Attributes.AggregateBounds)), Strength.Strong)))
            .Concat(from size in gap.ToSeq() from pair in along select Constraint.Equal(At(pair.B, low(pair.B.Object.Attributes.AggregateBounds)) - At(pair.A, high(pair.A.Object.Attributes.AggregateBounds)), size, Strength.Strong))
            .Concat(spread.Map(triple => Constraint.Equal(At(triple.B, low(triple.B.Object.Attributes.AggregateBounds)) - At(triple.A, high(triple.A.Object.Attributes.AggregateBounds)), At(triple.C, low(triple.C.Object.Attributes.AggregateBounds)) - At(triple.B, high(triple.B.Object.Attributes.AggregateBounds)), Strength.Strong)))
            .Concat(from frame in request.Frame.ToSeq() from row in rows from constraint in Seq(
                Constraint.GreaterEqual(At(row, low(row.Object.Attributes.AggregateBounds)), low(frame), Strength.Strong),
                Constraint.LessEqual(At(row, high(row.Object.Attributes.AggregateBounds)), high(frame), Strength.Strong)) select constraint)
            .Concat(levelled.Map(wire => Constraint.Equal(pivots[wire.Target] - pivots[wire.Source], wire.Offset, Strength.Medium)))
            .Concat(rows.Map(row => Constraint.Equal(pivots[row.Object.InstanceId], coordinate(row.Object.Attributes.Pivot), strengths[row.Object.InstanceId])))
            .Iter(solver.AddConstraint);
        solver.Solve();
        return pivots.Map(static pivot => (float)pivot.Value);

        Expression At(Footprint row, float edge) => pivots[row.Object.InstanceId] + (edge - coordinate(row.Object.Attributes.Pivot));
    }
}
