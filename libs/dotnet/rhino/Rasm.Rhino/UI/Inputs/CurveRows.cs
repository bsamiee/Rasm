using System.Numerics;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.Resources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CurveLane<TRecord>(
    Key Key, string Caption, Lens<TRecord, Seq<Vector2>> Points, Func<TRecord, Func<float, float>> Fit, PlotPlane.Cartesian Plane, Option<IO<Seq<float>>> Underlay)
    where TRecord : notnull;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveRows {
    // --- [EDITORS]
    private static readonly Selection Unpicked = new Selection.Many([], None, None);

    public static ControlRow Editor<TRecord>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, IterableNE<CurveLane<TRecord>> lanes, RowRules rules, Option<Func<Key, IO<Seq<float>>>> pick = default)
        where TRecord : notnull =>
        ControlRow.Of(source, parameter, RowShape.Block, (scope, retain) => Edited(source, parameter, lanes, pick, scope, retain), rules);

    private static IO<RowCells> Edited<TRecord>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, IterableNE<CurveLane<TRecord>> lanes, Option<Func<Key, IO<Seq<float>>>> pick, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        DisposalOps.Acquire(owned =>
            from keyed in IO.pure(toHashMap(lanes.Map(static lane => (lane.Key, lane))))
            let caption = RowText.Localize(parameter.Text.Caption, scope.Sink).Local
            let site = new CallbackSite(scope.Sink, typeof(CurveRows), nameof(Editor))
            from input in Coordinates(lanes, "Input", static plane => plane.X, scope.Sink, owned)
            from output in Coordinates(lanes, "Output", static plane => plane.Y, scope.Sink, owned)
            from scened in SceneRow<TRecord, CurveShown>.Bind(
                source, IterableNE.create(parameter), new CurveShown(lanes.Head.Key, Unpicked, None),
                (row, record, shown) => Drawn(row, keyed, keyed[shown.Active], record.IfNone(source.Default), shown) switch {
                    var drawn => drawn with {
                        Conditions = record.IsNone ? drawn.Conditions.TryAdd(Condition.Mixed) : drawn.Conditions,
                        Accessible = drawn.Accessible.Combine(new Accessible { Label = $"{caption}: {RowText.Localize(keyed[shown.Active].Caption, scope.Sink).Local}" }),
                    },
                },
                (_, record, shown) => Anchored(keyed[shown.Active], record, shown.Picked) switch {
                    var anchor => input.Show(shown.Active, anchor.Map(static point => point.X)).Bind(_ => output.Show(shown.Active, anchor.Map(static point => point.Y))),
                },
                scope, site, owned)
            let row = scened.Row
            from entered in Callbacks.Each(Seq((input.Fields, Coordinate: Lens<Vector2, float>.New(static point => point.X, static x => point => point with { X = x })),
                    (output.Fields, Coordinate: Lens<Vector2, float>.New(static point => point.Y, static y => point => point with { Y = y })))
                .Bind(side => side.Fields.Map(field => Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h,
                    Callbacks.Handler<Edit<float>>(edit => Entered(row, keyed, side.Coordinate, edit), site with { Member = nameof(NumberField<,>.Edited) }), owned))))
            from strip in toSeq(lanes).Count is 1
                ? IO.pure(Option<Control>.None)
                : ChoiceRows.Strip(toSeq(lanes).Map(lane => new ChoiceItem<Key>(lane.Key, RowText.Localize(lane.Caption, scope.Sink).Local, None)),
                        key => Laned(row, keyed, key), scope.Sink, owned)
                    .Bind(held => held.Show(Some(lanes.Head.Key)).Map(_ => Some(held.Control)))
            from picked in pick.Traverse(read => CommandRows.Button(
                    new Command<RowRule, Invoke>.Run(
                        new CommandFace("curve-pick", "Pick Point", None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.SVGEditorEyeDropperSvg))), None,
                        new Invoke.Effect(_ => read(row.Held.Active).Bind(xs => Inserted(row, keyed, xs)))),
                    IconSlot.Small, scope, owned)).As()
            from body in strip.Match(
                Some: chooser => Containers.Frame(() => new StackLayout(new StackLayoutItem(chooser, HorizontalAlignment.Left), new StackLayoutItem(row.View, HorizontalAlignment.Stretch)) {
                    Orientation = Eto.Forms.Orientation.Vertical,
                    Spacing = RhinoLayout.StackedSpacing(Eto.Forms.Orientation.Vertical, RhinoLayout.SpacingType.Table),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                }, owned).Map(static stack => (Control)stack),
                None: () => IO.pure<Control>(row.View))
            select new RowCells(
                body, None, None, None, [],
                Seq(input.Line with { Pick = picked.Map(static button => button.Control) }, output.Line),
                new RowHelp(None, None), scened.Edit.Shown.Bind(_ => picked.Traverse(static button => button.Refresh).As()).Map(unit), Some<RowEdit>(scened.Edit),
                Some(ChoiceRows.Context(row.View, [Commands(source, row, keyed)], []))), retain);

    private static Node Drawn<TRecord>(SceneRow<TRecord, CurveShown> row, HashMap<Key, CurveLane<TRecord>> keyed, CurveLane<TRecord> lane, TRecord record, CurveShown shown)
        where TRecord : notnull =>
        new CurveEditor(lane.Plane, lane.Points.Get(record), lane.Fit(record), row.Edited<Seq<Vector2>>((points, held) => lane.Points.Set(points, held))) {
            References = toSeq(keyed.Values).Filter(other => other.Key != lane.Key && other.Plane == lane.Plane).Map(other => other.Fit(record)),
            Underlay = shown.Underlay,
            Picked = shown.Picked,
            Pick = Some<Func<Edit<Selection>, IO<Unit>>>(edit => row.Change(held => held with { Picked = edit.Carried.IfNone(held.Picked) })),
        }.Drawn;

    private static IO<Unit> Laned<TRecord>(SceneRow<TRecord, CurveShown> row, HashMap<Key, CurveLane<TRecord>> keyed, Key key) where TRecord : notnull =>
        from underlay in row.Held.Underlay.IsSome ? keyed[key].Underlay.Traverse(static read => read).As() : IO.pure(Option<Seq<float>>.None)
        from changed in row.Change(held => held with { Active = key, Picked = Unpicked, Underlay = underlay })
        select unit;

    private static Seq<Command<RowRule, Invoke>> Commands<TRecord>(RowSource<TRecord> source, SceneRow<TRecord, CurveShown> row, HashMap<Key, CurveLane<TRecord>> keyed)
        where TRecord : notnull =>
        [
            new Command<RowRule, Invoke>.Run(new CommandFace("curve-reset", "Reset Curve", None, None),
                Some(new RowRule([], [], _ => row.Shown.Map(shown => shown.Exists(record => keyed[row.Held.Active].Points switch {
                    var points => points.Get(record) != points.Get(source.Default),
                })))),
                new Invoke.Effect(_ => row.Edited<Unit>((_, record) => keyed[row.Held.Active].Points switch {
                        var points => points.Set(points.Get(source.Default), record),
                    })(new Edit<Unit>.Commit(unit))
                    .Bind(_ => row.Change(static held => held with { Picked = Unpicked })))),
            new Command<RowRule, Invoke>.Run(new CommandFace("curve-underlay", "Underlay", None, None),
                Some(new RowRule([], [], _ => IO.lift(() => keyed[row.Held.Active].Underlay.IsSome))),
                new Invoke.Effect(_ => (row.Held.Underlay.IsSome ? IO.pure(Option<Seq<float>>.None) : keyed[row.Held.Active].Underlay.Traverse(static read => read).As())
                    .Bind(underlay => row.Change(held => held with { Underlay = underlay })))) {
                Marked = new RowRule([], [], _ => IO.lift(() => row.Held.Underlay.IsSome)),
            },
        ];

    // --- [POINTS]
    private static IO<Unit> Inserted<TRecord>(SceneRow<TRecord, CurveShown> row, HashMap<Key, CurveLane<TRecord>> keyed, Seq<float> xs) where TRecord : notnull =>
        from lane in IO.pure(keyed[row.Held.Active])
        from shown in row.Shown
        let placed = shown.Map(record => xs.Fold((Points: lane.Points.Get(record), At: Option<int>.None), (held, x) =>
            Placed(held.Points, x, lane.Fit(record), lane.Plane).Match(Some: static inserted => (inserted.Points, Some(inserted.At)), None: () => held)))
        from written in placed.Filter(static held => held.At.IsSome).Traverse(held =>
            from taken in row.Edited<Unit>((_, record) => lane.Points.Set(held.Points, record))(new Edit<Unit>.Commit(unit))
            from chosen in row.Change(state => state with { Picked = held.At.Map(Lead).IfNone(Unpicked) })
            select unit).As()
        select unit;

    private static Option<(Seq<Vector2> Points, int At)> Placed(Seq<Vector2> points, float input, Func<float, float> fit, PlotPlane.Cartesian plane) =>
        plane.X.Held(input) switch {
            var x when points.Exists(point => point.X == x) => None,
            var x => points.Filter(point => point.X < x).Count switch {
                var at => Some((points.Take(at).Add(new Vector2(x, plane.Y.Held(fit(x)))) + points.Skip(at), at)),
            },
        };

    private static IO<Unit> Entered<TRecord>(SceneRow<TRecord, CurveShown> row, HashMap<Key, CurveLane<TRecord>> keyed, Lens<Vector2, float> coordinate, Edit<float> edit)
        where TRecord : notnull =>
        Index(row.Held.Picked).Traverse(index => Moving(row, keyed[row.Held.Active], index, coordinate, edit)).As().Map(unit);

    private static IO<Unit> Moving<TRecord>(SceneRow<TRecord, CurveShown> row, CurveLane<TRecord> lane, int index, Lens<Vector2, float> coordinate, Edit<float> edit) where TRecord : notnull =>
        row.Edited<float>((value, record) => lane.Points.Get(record) switch {
            var points => points.At(index).Map(point => lane.Points.Set(Moved(points, index, coordinate.Set(value, point), lane.Plane), record)).IfNone(record),
        })(edit);

    private static Seq<Vector2> Moved(Seq<Vector2> points, int index, Vector2 entered, PlotPlane.Cartesian plane) =>
        (Before: points.At(index - 1).Filter(_ => plane.X is not Axis.Periodic), After: points.At(index + 1).Filter(_ => plane.X is not Axis.Periodic)) switch {
            var near => points.Map((point, at) => at == index
                ? new Vector2(
                    float.Clamp(entered.X, near.Before.Map(static held => float.BitIncrement(held.X)).IfNone(plane.X.Range.Low), near.After.Map(static held => float.BitDecrement(held.X)).IfNone(plane.X.Range.High)),
                    plane.Y.Held(entered.Y))
                : point),
        };

    private static Option<Vector2> Anchored<TRecord>(CurveLane<TRecord> lane, Option<TRecord> record, Selection picked) where TRecord : notnull =>
        from held in record
        from index in Index(picked)
        from point in lane.Points.Get(held).At(index)
        select point;

    private static Selection Lead(int index) => (Key)new Key.Ordinal((uint)index) switch { var key => new Selection.Many([key], Some(key), Some(key)) };

    private static Option<int> Index(Selection picked) => picked.Lead.Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None);

    // --- [COORDINATES]
    private static IO<(RowLine Line, Func<Key, Option<float>, IO<Unit>> Show, Seq<NumberField<float, float>> Fields)> Coordinates<TRecord>(
        IterableNE<CurveLane<TRecord>> lanes, string caption, Func<PlotPlane.Cartesian, Axis> axis, IPlugInSink sink, Action<IDisposable> retain)
        where TRecord : notnull =>
        from fields in toSeq(lanes).TraverseM(lane => axis(lane.Plane).Range switch {
            var range => new ScalarText<float, float>(new NumericDomain<float, float>(static value => value, static value => value, (range.Low, range.High), new Presentation<float>()), RowEdit.Varies),
        } switch {
            var text => DisposalOps.Acquire(IO.lift(() => new NumberField<float, float>(text, text.Domain.Create, text.Domain.Key, text.Domain.Bounds, sink) { Enabled = false }), retain)
                .Map(field => (lane.Key, Field: field)),
        }).As()
        let keyed = toHashMap(fields)
        from panel in Containers.Frame(static () => new Panel(), retain)
        from placed in IO.lift(() => panel.Content = keyed[lanes.Head.Key])
        select (
            new RowLine(Some(caption), panel, None, Some(new FieldFit(panel, fields.Fold(0f, static (widest, held) => float.Max(widest, held.Field.Fit.Width))))),
            (Func<Key, Option<float>, IO<Unit>>)((key, value) => keyed[key] switch {
                var field => IO.lift(() => {
                    panel.Content = field;
                    field.Enabled = value.IsSome;
                }).Bind(_ => field.Receive(value)),
            }),
            fields.Map(static held => held.Field));

    private sealed record CurveShown(Key Active, Selection Picked, Option<Seq<float>> Underlay);
}
