using System.Numerics;
using Eto.Forms;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Numeric;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.RhinoCommon.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Plotting;
using Rhino.Resources;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CurveLane(Key Key, string Caption, Seq<Vector2> Points, Func<float, float> Fit, PlotPlane.Cartesian Plane, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public Option<IO<Seq<float>>> Underlay { get; init; }
    public Option<Seq<Vector2>> Rest { get; init; }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveRows {
    // --- [EDITORS]
    private static readonly Selection Unpicked = new Selection.Many([]);
    private static readonly Lens<Vector2, float> Across = Lens<Vector2, float>.New(static point => point.X, static x => point => point with { X = x });
    private static readonly Lens<Vector2, float> Upward = Lens<Vector2, float>.New(static point => point.Y, static y => point => point with { Y = y });

    public static ControlRow Editor(string label, IterableNE<CurveLane> lanes, Option<Func<Key, IO<Seq<float>>>> pick = default) =>
        new ControlRow.Bound<(IterableNE<CurveLane> Lanes, Option<Func<Key, IO<Seq<float>>>> Pick)>(label, RowShape.Block, (lanes, pick), static (latest, scope) =>
            from row in latest
            from held in atomIO(new CurveShown(row.Row.Lanes.Head.Key, Unpicked, None))
            let keyed = latest.Map(static current => toHashMap(current.Row.Lanes.Map(static lane => (lane.Key, lane))))
            from view in use(() => new SceneView(scope, new Node.Box()))
            from input in Coordinates(row.Row.Lanes, static plane => plane.X, edit => Entered(keyed, held, Across, edit), scope.Owner)
            from output in Coordinates(row.Row.Lanes, static plane => plane.Y, edit => Entered(keyed, held, Upward, edit), scope.Owner)
            let shown =
                from current in latest
                from lanes in keyed
                from state in held.ValueIO
                let lane = lanes[state.Active]
                from drew in view.Show(new CurveEditor(lane.Plane, lane.Points, lane.Fit, lane.Edited) {
                    References = toSeq(lanes.Values).Filter(other => other.Key != lane.Key && other.Plane == lane.Plane).Map(static other => other.Fit),
                    Underlay = state.Underlay,
                    Picked = state.Picked,
                    Pick = edit => Some(held.SwapIO(prior => prior with { Picked = edit.Value.IfNone(prior.Picked) }).Map(unit)),
                }.Drawn switch {
                    var drawn => drawn with {
                        Accessible = drawn.Accessible.Combine(new Accessible { Label = $"{RowText.Localize(current.Label, scope.Owner).Local}: {RowText.Localize(lane.Caption, scope.Owner).Local}" }),
                    },
                })
                let anchor = Index(state.Picked).Bind(index => lane.Points.At(index))
                from inputs in input.Show(state.Active, anchor.Map(static point => point.X))
                from outputs in output.Show(state.Active, anchor.Map(static point => point.Y))
                select unit
            from strip in toSeq(row.Row.Lanes).Count is 1
                ? IO.pure(Option<Control>.None)
                : ChoiceRows.Strip(toSeq(row.Row.Lanes).Map(lane => new ChoiceItem<Key>(lane.Key, RowText.Localize(lane.Caption, scope.Owner).Local)),
                        key => Laned(keyed, held, key).Bind(_ => shown), scope.Owner)
                    .Bind(chosen => from shown in chosen.Show(row.Row.Lanes.Head.Key) select Some(chosen.Control))
            from picked in row.Row.Pick.Traverse(read => CommandRows.Button(
                    new Command<RowRule, Invoke>.Run(
                        new CommandFace("curve-pick", "Pick Point") { Glyph = Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.SVGEditorEyeDropperSvg)) }, None,
                        new Invoke.Effect(_ => held.ValueIO.Bind(state => read(state.Active)).Bind(xs => Inserted(keyed, held, xs)).Bind(_ => shown))),
                    IconSlot.Small, scope)).As()
            from body in strip.Match(
                Some: chooser => Containers.Frame(() => new StackLayout(new StackLayoutItem(chooser, HorizontalAlignment.Left), new StackLayoutItem(view, HorizontalAlignment.Stretch)) {
                    Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Table),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                }).Map(static stack => (Control)stack),
                None: () => IO.pure<Control>(view))
            select new RowCells(body, shown.Bind(_ => picked.Traverse(static button => button.Refresh).As()).Map(unit)) {
                Pick = picked.Map(static button => button.Control),
                Lines = Seq(new RowLine(Some("Input"), input.Panel), new RowLine(Some("Output"), output.Panel)),
                Menu = [Commands(keyed, held, shown)],
            });

    private static IO<Unit> Laned(IO<HashMap<Key, CurveLane>> keyed, Atom<CurveShown> held, Key key) =>
        from lanes in keyed
        from state in held.ValueIO
        from underlay in state.Underlay.IsSome ? lanes[key].Underlay.Traverse(static read => read).As() : IO.pure(Option<Seq<float>>.None)
        from changed in held.SwapIO(prior => prior with { Active = key, Picked = Unpicked, Underlay = underlay })
        select unit;

    private static Seq<Command<RowRule, Invoke>> Commands(IO<HashMap<Key, CurveLane>> keyed, Atom<CurveShown> held, IO<Unit> shown) =>
        (from state in held.ValueIO from lanes in keyed select lanes[state.Active]) switch {
            var active => [
                new Command<RowRule, Invoke>.Run(new CommandFace("curve-reset", "Reset Curve"),
                    Some(new RowRule(_ => active.Map(static lane => lane.Rest.Exists(rest => rest != lane.Points)))),
                    new Invoke.Effect(_ =>
                        from lane in active
                        from reset in lane.Rest.Traverse(rest => lane.Edited(Edit.Commit(rest))).As()
                        from cleared in held.SwapIO(static prior => prior with { Picked = Unpicked })
                        from redrawn in shown
                        select unit)),
                new Command<RowRule, Invoke>.Run(new CommandFace("curve-underlay", "Underlay"),
                    Some(new RowRule(_ => active.Map(static lane => lane.Underlay.IsSome))),
                    new Invoke.Effect(_ =>
                        from lane in active
                        from state in held.ValueIO
                        from underlay in state.Underlay.IsSome ? IO.pure(Option<Seq<float>>.None) : lane.Underlay.Traverse(static read => read).As()
                        from changed in held.SwapIO(prior => prior with { Underlay = underlay })
                        from redrawn in shown
                        select unit)) {
                    Marked = new RowRule(_ => held.ValueIO.Map(static state => state.Underlay.IsSome)),
                },
            ],
        };

    // --- [POINTS]
    private static IO<Unit> Inserted(IO<HashMap<Key, CurveLane>> keyed, Atom<CurveShown> held, Seq<float> xs) =>
        from lanes in keyed
        from state in held.ValueIO
        let lane = lanes[state.Active]
        let placed = xs.Fold((lane.Points, At: Option<int>.None), (inserted, x) =>
            Placed(inserted.Points, x, lane.Fit, lane.Plane).Match(Some: static next => (next.Points, Some(next.At)), None: () => inserted))
        from written in placed.At.Traverse(at =>
            from taken in lane.Edited(Edit.Commit(placed.Points))
            from chosen in held.SwapIO(prior => prior with { Picked = Lead(at) })
            select unit).As()
        select unit;

    private static Option<(Seq<Vector2> Points, int At)> Placed(Seq<Vector2> points, float input, Func<float, float> fit, PlotPlane.Cartesian plane) =>
        plane.X.Held(input) switch {
            var x when points.Exists(point => point.X == x) => None,
            var x => points.Filter(point => point.X < x).Count switch {
                var at => Some((points.Take(at).Add(new Vector2(x, plane.Y.Held(fit(x)))) + points.Skip(at), at)),
            },
        };

    private static IO<Unit> Entered(IO<HashMap<Key, CurveLane>> keyed, Atom<CurveShown> held, Lens<Vector2, float> coordinate, Edit<float> edit) =>
        from lanes in keyed
        from state in held.ValueIO
        let lane = lanes[state.Active]
        from moved in Index(state.Picked).Traverse(index => lane.Edited(edit.Map(value =>
            lane.Points.At(index).Map(point => Moved(lane.Points, index, coordinate.Set(value, point), lane.Plane)).IfNone(lane.Points)).As())).As()
        select unit;

    private static Seq<Vector2> Moved(Seq<Vector2> points, int index, Vector2 entered, PlotPlane.Cartesian plane) =>
        (Before: points.At(index - 1).Filter(_ => plane.X is not Axis.Periodic), After: points.At(index + 1).Filter(_ => plane.X is not Axis.Periodic)) switch {
            var near => points.Map((point, at) => at == index
                ? new Vector2(
                    float.Clamp(entered.X, near.Before.Map(static held => float.BitIncrement(held.X)).IfNone(plane.X.Domain.Low), near.After.Map(static held => float.BitDecrement(held.X)).IfNone(plane.X.Domain.High)),
                    plane.Y.Held(entered.Y))
                : point),
        };

    private static Selection.Many Lead(int index) => (Key)new Key.Ordinal((uint)index) switch { var key => new Selection.Many([key]) { Anchor = Some(key), Focus = Some(key) } };

    private static Option<int> Index(Selection picked) => picked.Lead.Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None);

    // --- [COORDINATES]
    private static IO<(Control Panel, Func<Key, Option<float>, IO<Unit>> Show)> Coordinates(
        IterableNE<CurveLane> lanes, Func<PlotPlane.Cartesian, Axis> axis, Func<Edit<float>, IO<Unit>> edited, IPlugInLifetime owner) =>
        from fields in toSeq(lanes).TraverseM(lane => axis(lane.Plane) switch {
            var held => use(() => new NumberField<float>(new ScalarText(held, RowText.Culture), (held.Domain.Low, held.Domain.High), edited, owner) { Enabled = false })
                .Map(field => (lane.Key, Field: field)),
        }).As()
        let keyed = toHashMap(fields)
        from panel in Containers.Frame(static () => new Panel())
        from placed in IO.lift(() => panel.Content = keyed[lanes.Head.Key])
        select (
            (Control)panel,
            (Func<Key, Option<float>, IO<Unit>>)((key, value) => keyed[key] switch {
                var field => IO.lift(() => {
                    panel.Content = field;
                    field.Enabled = value.IsSome;
                }).Bind(_ => value.Traverse(field.Receive).As().Map(unit)),
            }));

    private sealed record CurveShown(Key Active, Selection Picked, Option<Seq<float>> Underlay);
}
