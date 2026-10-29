using System.Globalization;
using Eto.Drawing;
using Eto.Forms;
using Grasshopper2.Doc;
using Grasshopper2.Extensions;
using Grasshopper2.Parameters;
using Grasshopper2.SpecialObjects;
using Grasshopper2.UI;
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
[Union]
public abstract partial record Placement {
    private Placement(string verb, Func<string, Nomen> face) => (Verb, Face) = (verb, face(verb));

    public string Verb { get; }

    public Nomen Face { get; }

    public sealed record Align(Orientation Axis, float Edge) : Placement("Align", verb =>
        (Axis is Orientation.Horizontal ? Seq("left", "centre", "right") : Seq("top", "middle", "bottom"))[int.Clamp((int)MathF.Round(Edge * 2f, MidpointRounding.ToEven), 0, 2)] switch {
            var side => new Nomen($"{verb} {CultureInfo.InvariantCulture.TextInfo.ToTitleCase(side)}", $"{verb} the selected objects to the {side}."),
        });

    public sealed record Space(Orientation Axis, Option<int> Gap) : Placement(Gap.IsSome ? "Stack" : "Distribute", verb =>
        new Nomen($"{verb} {Axis}", $"{verb} the selected objects {(Axis is Orientation.Horizontal ? "horizontally" : "vertically")}."));

    public sealed record Level() : Placement("Level", static _ => new Nomen("Level Wires", "Move the selected objects so their wires run straight."));

    public sealed record Tidy() : Placement("Tidy", static verb => new Nomen(verb, "Space, align and level the selected objects by the snapping rules."));

    public sealed record Layered() : Placement("Layer", static verb => new Nomen(verb, "Lay the selected objects out in data-flow layers."));

    public sealed record Frame(RectangleF Bounds) : Placement("Frame", static verb => new Nomen(verb, "Keep the selected objects inside a frame."));

    public sealed record Grid(SnapSpace Snaps) : Placement("Snap", static verb => new Nomen(verb, "Snap the selected objects to a snap space."));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Placements {
    // --- [COMMANDS]
    private static readonly Seq<(SnappingRule Rule, Placement.Align Align)> Edges = Seq(
        (SnappingRule.AlignLeftEdges, new Placement.Align(Orientation.Horizontal, 0f)),
        (SnappingRule.AlignMiddles, new Placement.Align(Orientation.Horizontal, 0.5f)),
        (SnappingRule.AlignRightEdges, new Placement.Align(Orientation.Horizontal, 1f)),
        (SnappingRule.AlignTopEdges, new Placement.Align(Orientation.Vertical, 0f)),
        (SnappingRule.AlignBottomEdges, new Placement.Align(Orientation.Vertical, 1f)));

    public static Seq<Command> Commands =>
        (Edges.Map(static edge => (Func<SnappingSettings, Placement>)(_ => edge.Align))
         + toSeq(Enum.GetValues<Orientation>()).Bind(static axis => Seq<Func<SnappingSettings, Placement>>(
             _ => new Placement.Space(axis, None),
             settings => new Placement.Space(axis, Some(Gap(axis, settings)))))
         + Seq<Func<SnappingSettings, Placement>>(static _ => new Placement.Level(), static _ => new Placement.Tidy(), static _ => new Placement.Layered()))
        .Map(static place => Chrome.Run(
            place(SnappingSettings.Default).Face,
            canvas => IO.lift(static () => SnappingSettings.Current).Bind(settings => Selected(place(settings))(canvas)),
            enabled: static canvas => canvas.Document is { Objects.SelectedCount: > 0 }));

    public static Func<Canvas, IO<Unit>> Selected(Placement placement) =>
        canvas =>
            from document in IO.lift(() => Optional(canvas.Document).ToFin(new GrasshopperFault(placement.Verb, GrasshopperReason.Missing)))
            from arranged in Arrange(document, toSeq(document.Objects.SelectedObjects).Map(static item => item.InstanceId), Seq(placement))
            select unit;

    // --- [NUDGING]
    public static IO<Unit> Nudging(Canvas canvas, IO<bool> enabled) =>
        from gesture in use(IO.lift(static () => Ref(Option<(Document Document, HashMap<Guid, PivotAction> Started)>.None)), Settled)
        from pressed in Hooks.On<KeyEventArgs>(h => canvas.KeyDown += h, h => canvas.KeyDown -= h, args => Offset(args).Filter(_ => !args.Handled).Match(
            Some: offset =>
                from moved in when(gesture.Value.Exists(held => !ReferenceEquals(held.Document, canvas.Document)), Settled(gesture)).As()
                from allowed in enabled
                from nudged in IO.lift(() => Optional(canvas.Document).Filter(_ => allowed && canvas.AllowedActions.AllowDrag).Iter(document => Nudged(canvas, document, offset, gesture, args)))
                select unit,
            None: static () => unitIO))
        from released in Hooks.On<KeyEventArgs>(h => canvas.KeyUp += h, h => canvas.KeyUp -= h, args => when(Direction(args.Key).IsSome, Settled(gesture)).As())
        from blurred in Hooks.On<EventArgs>(h => canvas.LostFocus += h, h => canvas.LostFocus -= h, _ => Settled(gesture))
        select unit;

    private static Option<SizeF> Direction(Keys key) =>
        key switch {
            Keys.Left => Some(new SizeF(-1f, 0f)),
            Keys.Right => Some(new SizeF(1f, 0f)),
            Keys.Up => Some(new SizeF(0f, -1f)),
            Keys.Down => Some(new SizeF(0f, 1f)),
            _ => None,
        };

    private static Option<SizeF> Offset(KeyEventArgs args) =>
        from direction in Direction(args.Key)
        from step in args.Modifiers switch {
            Keys.None => Some(new SizeF(1f, 1f)),
            Keys.Shift => Some(new SizeF(Gap(Orientation.Horizontal, SnappingSettings.Current), Gap(Orientation.Vertical, SnappingSettings.Current))),
            _ => Option<SizeF>.None,
        }
        select direction * step;

    private static Unit Nudged(Canvas canvas, Document document, SizeF offset, Ref<Option<(Document Document, HashMap<Guid, PivotAction> Started)>> gesture, KeyEventArgs args) {
        Seq<IDocumentObject> rows = Rows(document, toHashSet(toSeq(document.Objects.SelectedObjects).Bind(item => Movable(document, item))));
        if (rows.IsEmpty) return unit;
        _ = atomic(() => gesture.Swap(held => Some((document, rows.Fold(
            held.Map(static started => started.Started).IfNone(HashMap<Guid, PivotAction>()),
            static (started, row) => started.ContainsKey(row.InstanceId) ? started : started.Add(row.InstanceId, new PivotAction(row)))))));
        _ = rows.Iter(row => row.Attributes.Pivot += offset);
        args.Handled = true;
        canvas.Invalidate();
        return unit;
    }

    private static IO<Unit> Settled(Ref<Option<(Document Document, HashMap<Guid, PivotAction> Started)>> gesture) =>
        from held in IO.lift(() => atomic(() => (Prior: gesture.Value, Next: gesture.Swap(static _ => None)).Prior))
        from committed in held.Match(
            Some: static started => Edits.Commit(started.Document, new VerbNoun("Nudge", started.Started.Count is 1 ? "Object" : "Objects"), actions =>
                IO.lift(() => started.Started.Values.Iter(action => actions.Add(action)))),
            None: static () => unitIO)
        select unit;

    // --- [ARRANGEMENT]
    public static IO<bool> Arrange(Document document, Seq<Guid> objects, Seq<Placement> placements) =>
        IO.lift(() => Solved(
                document,
                Rows(document, toHashSet(objects.Choose(id => Optional(document.Objects.Find(id))).Bind(item => Movable(document, item)))).Filter(static row => row.Attributes.Snappable).Strict(),
                placements))
            .Bind(moves => Placed(document, placements is [var single] ? single.Verb : "Arrange", moves));

    private static Seq<Guid> Movable(Document document, IDocumentObject item) =>
        item switch {
            IPin pin => Optional(Pins.FindObject(pin, document)).Filter(target => Pins.BothAgree(pin, target)).Match(Some: target => Movable(document, target), None: () => Seq(item.InstanceId)),
            GroupObject group when group.Content.Any() => toSeq(group.Content).Bind(member => Movable(document, member)),
            _ => Seq(item.InstanceId),
        };

    private static Seq<IDocumentObject> Rows(Document document, HashSet<Guid> objects) =>
        toSeq(document.Objects.Forwards).Filter(item => objects.Contains(item.InstanceId)).Strict();

    private static Seq<(Guid Source, Guid Target, float Offset)> Levels(Document document, HashMap<Guid, IDocumentObject> lookup) =>
        toSeq(document.Objects.AllWires).Choose(wire =>
            from outlet in Optional(document.Objects.FindParameter(wire.Source)?.Attributes as IParameterAttributes).Filter(static ends => ends.HasOutlet)
            from inlet in Optional(document.Objects.FindParameter(wire.Target)?.Attributes as IParameterAttributes).Filter(static ends => ends.HasInlet)
            from source in lookup.Find(Connectivity.GetConnectiveId(document, wire.Source))
            from target in lookup.Find(Connectivity.GetConnectiveId(document, wire.Target))
            where source.InstanceId != target.InstanceId
            select (source.InstanceId, target.InstanceId, outlet.Outlet.Y - source.Attributes.Pivot.Y - (inlet.Inlet.Y - target.Attributes.Pivot.Y))).Strict();

    private static Seq<(IDocumentObject Object, PointF Pivot)> Solved(Document document, Seq<IDocumentObject> rows, Seq<Placement> placements) {
        HashMap<Guid, IDocumentObject> lookup = toHashMap(rows.Map(static row => (row.InstanceId, row)));
        Memo<Seq<Seq<IDocumentObject>>> layers = memo(() =>
            WireGraph.Of(document.Objects.Connectivity).Layers().Map(layer => layer.Choose(id => lookup.Find(id))).Filter(static layer => !layer.IsEmpty).Strict());
        Memo<Seq<(Guid Source, Guid Target, float Offset)>> levels = memo(() => Levels(document, lookup));
        SnappingSettings settings = SnappingSettings.Current;
        HashMap<Guid, float> x = Along(Orientation.Horizontal, rows, placements, settings, layers, levels);
        HashMap<Guid, float> y = Along(Orientation.Vertical, rows, placements, settings, layers, levels);
        return rows.Map(row => (row, new PointF(x[row.InstanceId], y[row.InstanceId]))).Strict();
    }

    private static HashMap<Guid, float> Along(Orientation axis, Seq<IDocumentObject> rows, Seq<Placement> placements, SnappingSettings settings, Memo<Seq<Seq<IDocumentObject>>> layers, Memo<Seq<(Guid Source, Guid Target, float Offset)>> levels) {
        HashMap<Guid, Variable> pivots = toHashMap(rows.Map(static row => (row.InstanceId, new Variable())));
        Seq<(Constraint Rule, Seq<Guid> Linked)> rules = placements.Bind(placement => placement.Switch(
            (Axis: axis, Rows: rows, Pivots: pivots, Settings: settings, Layers: layers, Levels: levels),
            align: static (s, p) => Aligned(s.Axis, s.Rows.Filter(_ => p.Axis == s.Axis), s.Pivots, p.Edge),
            space: static (s, p) => Spaced(s.Axis, toSeq(s.Rows.Filter(_ => p.Axis == s.Axis).OrderBy(row => Edge(s.Axis, row.Attributes.AggregateBounds, 0f))), s.Pivots, p.Gap),
            level: static (s, _) => s.Axis is Orientation.Vertical ? Levelled(s.Pivots, s.Levels.Value) : Seq<(Constraint Rule, Seq<Guid> Linked)>(),
            tidy: static (s, _) => Tidied(s.Axis, s.Rows, s.Pivots, s.Settings)
                + (s.Axis is Orientation.Vertical && s.Settings.AlignWires ? Levelled(s.Pivots, s.Levels.Value) : Seq<(Constraint Rule, Seq<Guid> Linked)>()),
            layered: static (s, _) => Layered(s.Axis, s.Layers.Value, s.Pivots, s.Settings),
            frame: static (s, p) => Framed(s.Axis, s.Rows, s.Pivots, p.Bounds),
            grid: static (s, p) => Snapped(s.Axis, s.Rows, s.Pivots, p.Snaps))).Strict();
        AdjacencyGraph<Guid, SEquatableEdge<Guid>> linked = rules.Bind(static rule => toSeq(rule.Linked.Window(2, static pair => new SEquatableEdge<Guid>(pair[0], pair[1])))).ToAdjacencyGraph<Guid, SEquatableEdge<Guid>>(allowParallelEdges: false);
        _ = linked.AddVertexRange(rows.Map(static row => row.InstanceId));
        IOrderedEnumerable<IDocumentObject> ordered = rows.OrderBy(row => Coordinate(axis, row.Attributes.Pivot));
        HashMap<Guid, double> strengths = toHashMap(
            from component in toSeq(linked.CondensateWeaklyConnected<Guid, SEquatableEdge<Guid>, AdjacencyGraph<Guid, SEquatableEdge<Guid>>>().Vertices)
            let anchor = ordered.First(row => component.ContainsVertex(row.InstanceId)).InstanceId
            from vertex in toSeq(component.Vertices)
            select (vertex, vertex == anchor ? Strength.Medium : Strength.Weak / component.VertexCount));
        Solver solver = new();
        _ = rules.Map(static rule => rule.Rule)
            .Concat(rows.Map(row => Constraint.Equal(pivots[row.InstanceId], Coordinate(axis, row.Attributes.Pivot), strengths[row.InstanceId])))
            .Iter(solver.AddConstraint);
        solver.Solve();
        return pivots.Map(static pivot => (float)pivot.Value);
    }

    // --- [CONSTRAINTS]
    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Aligned(Orientation axis, Seq<IDocumentObject> rows, HashMap<Guid, Variable> pivots, float edge) =>
        rows.Map(row => Edge(axis, row.Attributes.AggregateBounds, edge)).Fold((Low: float.MaxValue, High: float.MinValue), static (range, value) => (float.Min(range.Low, value), float.Max(range.High, value))) switch {
            var range => rows.Map(row => (Constraint.Equal(Side(pivots, axis, row, edge), range.Low + ((range.High - range.Low) * edge), Strength.Strong), Seq<Guid>())),
        };

    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Spaced(Orientation axis, Seq<IDocumentObject> ordered, HashMap<Guid, Variable> pivots, Option<int> gap) =>
        gap.Match(
            Some: size => toSeq(ordered.Window(2, static pair => (pair[0], pair[1])))
                .Map(pair => (Constraint.Equal(Side(pivots, axis, pair.Item2, 0f) - Side(pivots, axis, pair.Item1, 1f), size, Strength.Strong), Seq(pair.Item1.InstanceId, pair.Item2.InstanceId))),
            None: () => toSeq(ordered.Window(3, static triple => (triple[0], triple[1], triple[2])))
                .Map(triple => (Constraint.Equal(Side(pivots, axis, triple.Item2, 0f) - Side(pivots, axis, triple.Item1, 1f), Side(pivots, axis, triple.Item3, 0f) - Side(pivots, axis, triple.Item2, 1f), Strength.Strong), Seq(triple.Item1.InstanceId, triple.Item2.InstanceId, triple.Item3.InstanceId)))
                + (ordered.Count > 2 ? Seq(ordered.Head, ordered.Last).Somes() : Seq<IDocumentObject>())
                    .Map(end => (Constraint.Equal(pivots[end.InstanceId], Coordinate(axis, end.Attributes.Pivot), Strength.Strong), Seq<Guid>())));

    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Levelled(HashMap<Guid, Variable> pivots, Seq<(Guid Source, Guid Target, float Offset)> levels) =>
        levels.Map(wire => (Constraint.Equal(pivots[wire.Target] - pivots[wire.Source], wire.Offset, Strength.Medium), Seq(wire.Source, wire.Target)));

    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Tidied(Orientation axis, Seq<IDocumentObject> rows, HashMap<Guid, Variable> pivots, SnappingSettings settings) =>
        Pairs(rows, axis) switch {
            var along => along.Map(pair => (Constraint.LessEqual(Side(pivots, axis, pair.A, 1f), Side(pivots, axis, pair.B, 0f), Strength.Required), Seq(pair.A.InstanceId, pair.B.InstanceId)))
                + along.Filter(_ => axis is Orientation.Horizontal ? settings.SpaceHorizontalGaps : settings.SpaceVerticalGaps)
                    .Map(pair => (Constraint.Equal(Side(pivots, axis, pair.B, 0f) - Side(pivots, axis, pair.A, 1f), Gap(axis, settings), Strength.Strong), Seq(pair.A.InstanceId, pair.B.InstanceId)))
                + Pairs(rows, axis is Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal).Choose(pair => Nearest(axis, pair.A, pair.B, settings)
                    .Map(edge => (Constraint.Equal(Side(pivots, axis, pair.A, edge), Side(pivots, axis, pair.B, edge), Strength.Strong), Seq(pair.A.InstanceId, pair.B.InstanceId)))),
        };

    private static Option<float> Nearest(Orientation axis, IDocumentObject a, IDocumentObject b, SnappingSettings settings) =>
        Edges
            .Filter(edge => edge.Align.Axis == axis && settings.Rules.HasFlag(edge.Rule))
            .Map(edge => (Fraction: edge.Align.Edge, Delta: Math.Abs(Edge(axis, a.Attributes.AggregateBounds, edge.Align.Edge) - Edge(axis, b.Attributes.AggregateBounds, edge.Align.Edge))))
            .Filter(candidate => candidate.Delta <= settings.EdgeRadius)
            .Strict() switch {
                { IsEmpty: true } => None,
                var near => Some(near.MinBy(static candidate => candidate.Delta).Fraction),
            };

    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Layered(Orientation axis, Seq<Seq<IDocumentObject>> layers, HashMap<Guid, Variable> pivots, SnappingSettings settings) =>
        axis is Orientation.Horizontal
            ? toSeq(layers.Window(2, static window => (Left: window[0], Right: window[1]))).Bind(adjacent => new Variable() switch {
                var boundary => adjacent.Left.Map(a => (Constraint.LessEqual(Side(pivots, axis, a, 1f), boundary, Strength.Strong), Seq(a.InstanceId, adjacent.Right[0].InstanceId)))
                    + adjacent.Right.Map(b => (Constraint.GreaterEqual(Side(pivots, axis, b, 0f) - boundary, Gap(axis, settings), Strength.Strong), Seq(adjacent.Left[0].InstanceId, b.InstanceId))),
            })
              + (from layer in layers
                 from neighbours in toSeq(layer.Window(2, static window => (window[0], window[1])))
                 select (Constraint.Equal(Side(pivots, axis, neighbours.Item1, 0f), Side(pivots, axis, neighbours.Item2, 0f), Strength.Strong), Seq(neighbours.Item1.InstanceId, neighbours.Item2.InstanceId)))
            : layers.Bind(layer => Spaced(axis, layer, pivots, Some(Gap(axis, settings))));

    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Framed(Orientation axis, Seq<IDocumentObject> rows, HashMap<Guid, Variable> pivots, RectangleF bounds) =>
        rows.Bind(row => Seq(
            (Constraint.GreaterEqual(Side(pivots, axis, row, 0f), Edge(axis, bounds, 0f), Strength.Strong), Seq<Guid>()),
            (Constraint.LessEqual(Side(pivots, axis, row, 1f), Edge(axis, bounds, 1f), Strength.Strong), Seq<Guid>())));

    private static Seq<(Constraint Rule, Seq<Guid> Linked)> Snapped(Orientation axis, Seq<IDocumentObject> rows, HashMap<Guid, Variable> pivots, SnapSpace snaps) =>
        rows.Map(row => {
            snaps.Snap(row.Attributes.Pivot.X, row.Attributes.Pivot.Y, out double x, out double y, out _);
            return (Constraint.Equal(pivots[row.InstanceId], axis is Orientation.Horizontal ? x : y, Strength.Strong), Seq<Guid>());
        });

    private static Seq<(IDocumentObject A, IDocumentObject B)> Pairs(Seq<IDocumentObject> rows, Orientation axis) {
        HashMap<Guid, int> order = toHashMap(rows.Map(static (row, index) => (row.InstanceId, index)));
        return rows.Choose(a => Optional(rows.Filter(b => Precedence(b).CompareTo(Precedence(a)) > 0 && Adjacent(a.Attributes.AggregateBounds, b.Attributes.AggregateBounds)).MinBy(Precedence)).Map(b => (a, b))).Strict();

        (float Key, int Order) Precedence(IDocumentObject row) =>
            (axis is Orientation.Horizontal ? row.Attributes.AggregateBounds.Left : row.Attributes.AggregateBounds.MiddleY, order[row.InstanceId]);

        bool Adjacent(RectangleF a, RectangleF b) => axis is Orientation.Horizontal
            ? a.Top <= b.Bottom && b.Top <= a.Bottom && (a.Right < b.Left || b.Right < a.Left)
            : a.Left <= b.Right && b.Left <= a.Right;
    }

    private static float Edge(Orientation axis, RectangleF box, float fraction) =>
        axis is Orientation.Horizontal ? box.Left + (box.Width * fraction) : box.Top + (box.Height * fraction);

    private static float Coordinate(Orientation axis, PointF point) => axis is Orientation.Horizontal ? point.X : point.Y;

    private static int Gap(Orientation axis, SnappingSettings settings) => axis is Orientation.Horizontal ? settings.HorizontalGapSize : settings.VerticalGapSize;

    private static Expression Side(HashMap<Guid, Variable> pivots, Orientation axis, IDocumentObject row, float fraction) =>
        pivots[row.InstanceId] + (Edge(axis, row.Attributes.AggregateBounds, fraction) - Coordinate(axis, row.Attributes.Pivot));

    // --- [MOVES]
    private static IO<bool> Placed(Document document, string verb, Seq<(IDocumentObject Object, PointF Pivot)> moves) =>
        moves.Filter(static move => move.Pivot != move.Object.Attributes.Pivot).Strict() switch {
            { IsEmpty: true } => IO.pure(value: false),
            var changed => Edits.Commit(document, new VerbNoun(verb, changed.Count is 1 ? "Object" : "Objects"), actions => IO.lift(() => {
                _ = changed.Iter(move => actions.Add(new PivotAction(move.Object)));
                _ = changed.Iter(static move => move.Object.Attributes.Pivot = move.Pivot);
                return true;
            })),
        };
}
