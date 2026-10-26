using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rasm.UI.Styling;
using Rhino;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record NumericPart<TValue>(
    TableCell Cell, bool Square, Func<Option<TValue>, IO<Unit>> Receive, Action<EventHandler<Drawing.EditEventArgs<TValue>>> Add, Action<EventHandler<Drawing.EditEventArgs<TValue>>> Remove)
    where TValue : notnull;

internal sealed record NumericCell<TValue, TKey>(Control Value, FieldFit Fit, Option<NumericPart<TValue>> Track, NumericPart<TValue> Field, NumberText<TKey> Text, Func<TValue, TKey> Key, (TKey Low, TKey High) Bounds)
    where TValue : notnull
    where TKey : notnull {
    public Seq<NumericPart<TValue>> Parts => Track.ToSeq().Add(Field);

    public IO<Unit> Show(Option<TValue> value) => Parts.TraverseM(part => part.Receive(value)).As().Map(unit);

    public IO<Unit> Edits(EventHandler<Drawing.EditEventArgs<TValue>> handler, Action<IDisposable> retain) =>
        Parts.TraverseM(part => Subscriptions.Attach(part.Add, part.Remove, handler, retain)).As().Map(unit);

    public IO<Unit> Attached(CallbackSite site, Action<IDisposable> retain) =>
        Track.Filter(static part => part.Square).Traverse(part => Subscriptions.Attach(
            handler => Field.Cell.Control.SizeChanged += handler, handler => Field.Cell.Control.SizeChanged -= handler,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => { part.Cell.Control.Width = Field.Cell.Control.Height; }),
                site with { Member = nameof(Control.SizeChanged) }), retain)).As().Map(static _ => unit);

    public RowCells Cells<TRecord>(Option<Control> gate, Option<TValue> initial, RowEdit<TRecord> edit) where TRecord : notnull =>
        new(Value, gate, None, Some(Fit), [], [],
            new RowHelp(Some((Text.Shown(Some(Bounds.Low)), Text.Shown(Some(Bounds.High)))), initial.Map(value => Text.Shown(Some(Key(value))))),
            edit.Shown, Some<RowEdit>(edit), Some(ChoiceRows.Context(Value, [], [])));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumericRows {
    // --- [CROSSINGS]
    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));
    private static readonly AxisRange Compass = AxisRange.Create(MathF.PI / 2f, (MathF.PI / 2f) - MathF.Tau);

    public static TKey Narrowed<TKey>(double key) where TKey : struct, INumber<TKey> =>
        TKey.CreateSaturating(TKey.IsInteger(TKey.CreateSaturating(0.5)) ? double.Round(key, MidpointRounding.ToEven) : key);

    // --- [ROWS]
    public static ControlRow Numeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain, RowRules rules)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) =>
            from cell in Cell(domain, lens.Get(source.Default), RowText.Localize(field.Caption, scope.Sink).Local, scope, retain)
            let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Numeric))
            from cells in Bound(source, field, lens, cell, (_, _) => cell.Attached(site, retain), scope, site, retain)
            select cells, rules);

    public static ControlRow OptionalNumeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain, RowRules rules)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Toggled(source, field, lens, domain, scope, retain), rules);

    public static ControlRow Distance<TRecord, TValue>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, ValidationError>, IConvertible<Length>, IMinMaxValue<TValue> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Measured(source, field, lens, soft, modelUnits, scope, retain), rules);

    // --- [LINES]
    public static IO<(RowLine Line, Func<Option<TRecord>, IO<Unit>> Show)> Line<TRecord, TValue, TKey>(
        RowSource<TRecord> source, string caption, Lens<TRecord, TValue> lens, Presentation<TKey> presentation, RowEdit<TRecord> edit, Option<Control> pick, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, ValidationError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey> =>
        from cell in Cell(NumericDomain.Bounded<TValue, TKey>(presentation), lens.Get(source.Default), RowText.Localize(caption, scope.Sink).Local, scope, retain)
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Line))
        from edits in cell.Edits(Handled(lens, site)(edit), retain)
        from attached in cell.Attached(site, retain)
        select (new RowLine(Some(caption), cell.Value, pick, Some(cell.Fit)), (Func<Option<TRecord>, IO<Unit>>)(held => cell.Show(held.Map(lens.Get))));

    // --- [HANDLES]
    public static Node Slider<TValue, TKey>(ScalarText<TValue, TKey> text, TValue value, Option<TValue> rest, Func<Edit<TValue>, IO<Unit>> edited, string caption)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        text.Track(text.Domain.Key(value)) switch {
            var track => new PlotPlane.Track(Rasm.UI.Orientation.Horizontal, new Axis.Scale(
                AxisRange.Create(float.CreateSaturating(track.Low), float.CreateSaturating(track.High)),
                (shown, range) => (float)text.Domain.Presentation.Position(TKey.CreateSaturating(shown), (TKey.CreateSaturating(range.From), TKey.CreateSaturating(range.To))),
                (fraction, range) => (float)text.Domain.Presentation.Key(fraction, (TKey.CreateSaturating(range.From), TKey.CreateSaturating(range.To)), text.Domain.Bounds))) switch {
                var plane => new Handles<TValue>(value, Points<TValue>.Scalar(
                        plane,
                        Lens<TValue, float>.New(held => float.CreateSaturating(text.Domain.Key(held)), shown => held => text.Domain.Create(Narrowed<TKey>(shown)).IfFail(held)),
                        shown => text.Shown(Some(Narrowed<TKey>(shown)))),
                    edited) {
                    Rest = rest,
                }.Drawn(plane, new Node.Box { Role = Role.Track, Width = Grown }.Cons(text.Domain.Presentation.Stops.Match(
                    Some: _ => Seq<Node>(Ramps.Field(plane, at => text.Domain.Presentation.Fill(at.X, text.Domain.Bounds).IfNone(Vector4.Zero), Gamut.StandardRgb)),
                    None: () => ValidationResults.ToFin(AxisRange.Validate(
                            text.Domain.Presentation.Origin.Map(float.CreateSaturating).IfNone(plane.Axis.Range.From), float.CreateSaturating(text.Domain.Key(value)), out AxisRange? span), span)
                        .ToOption().Map(static region => (Node)new Node.Mark(new Figure.Region(Some(region), None)) { Role = Role.Span }).ToSeq()))) with {
                    Width = Grown,
                    Height = Sizing.Fit.Content,
                    Accessible = new Accessible { Label = caption },
                },
            },
        };

    public static Node Dial<TValue, TKey>(ScalarText<TValue, TKey> text, TValue value, Option<TValue> rest, Func<Edit<TValue>, IO<Unit>> edited, string caption)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ((int)double.Round(double.Tau / text.Display.Step, MidpointRounding.ToEven), float.CreateSaturating(text.Domain.Key(value))) switch {
            var (snaps, angle) => new PlotPlane.Polar(new Axis.Periodic(Compass, snaps, snaps), None) switch {
                var plane => new Handles<TValue>(value, Points<TValue>.Fixed(
                        [Lens<TValue, Vector2>.New(held => new Vector2(float.CreateSaturating(text.Domain.Key(held)), 1f), at => held => text.Domain.Create(Wrapped(text, at.X)).IfFail(held))],
                        at => text.Shown(Some(Wrapped(text, at.X)))),
                    edited) {
                    Rest = rest,
                }.Drawn(plane, [
                    new Node.Mark(new Figure.Region(None, None)) { Role = Role.Track },
                    new Node.Mark(new Figure.Rule(new Vector2(angle, 0f), new Vector2(angle, 1f))) { Role = Role.Span },
                ]) with {
                    Width = Grown,
                    Aspect = Factor.Create(1f),
                    Accessible = new Accessible { Label = caption },
                },
            },
        };

    private static TKey Wrapped<TValue, TKey>(ScalarText<TValue, TKey> text, double angle)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        double.CreateSaturating(text.Domain.Bounds.Low) switch {
            var low => Narrowed<TKey>(angle - (double.Tau * double.Floor((angle - low) / double.Tau))),
        };

    // --- [REALIZE]
    private static Func<TValue, TRecord, Fin<TRecord>> Into<TRecord, TValue>(Lens<TRecord, TValue> lens) => (value, held) => lens.Set(value, held);

    private static Func<RowEdit<TRecord>, EventHandler<Drawing.EditEventArgs<TValue>>> Handled<TRecord, TValue>(Lens<TRecord, TValue> lens, CallbackSite site)
        where TRecord : notnull
        where TValue : notnull =>
        edit => Callbacks.Handler<Drawing.EditEventArgs<TValue>>(change => edit.Take(change.Edit, Into(lens)).Map(unit), site);

    private static Drawing.Edit<TValue> Hosted<TValue>(Edit<TValue> change) where TValue : notnull =>
        change.Switch<Drawing.Edit<TValue>>(
            preview: static held => new Drawing.Edit<TValue>.Preview(held.Value),
            step: static held => new Drawing.Edit<TValue>.Step(held.Value),
            commit: static held => new Drawing.Edit<TValue>.Commit(held.Value),
            cancel: static _ => new Drawing.Edit<TValue>.Cancel());

    private static IO<(NumberField<TValue, TKey> Field, NumericPart<TValue> Part)> Typed<TValue, TKey>(
        NumberText<TKey> text, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : notnull, IComparable<TKey> =>
        DisposalOps.Acquire(IO.lift(() => new NumberField<TValue, TKey>(text, create, key, bounds, sink)), retain).Map(static field => (
            field,
            new NumericPart<TValue>(new TableCell(field), Square: false, field.Receive, handler => field.Edited += handler, handler => field.Edited -= handler)));

    private static IO<Option<NumericPart<TValue>>> Track<TValue, TKey>(ScalarText<TValue, TKey> text, TValue rest, string caption, RowScope scope, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        (text.Display.Turn
            ? Some<(Func<ScalarText<TValue, TKey>, TValue, Option<TValue>, Func<Edit<TValue>, IO<Unit>>, string, Node> Draw, bool Square)>((Dial, true))
            : text.Domain.Presentation.Form.Map(
                track: Some<(Func<ScalarText<TValue, TKey>, TValue, Option<TValue>, Func<Edit<TValue>, IO<Unit>>, string, Node> Draw, bool Square)>((Slider, false)),
                field: None))
        .Traverse(form =>
            from handlers in atomIO(Seq<EventHandler<Drawing.EditEventArgs<TValue>>>())
            from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            let edited = (Func<Edit<TValue>, IO<Unit>>)(change => IO.lift(() => handlers.Value.Iter(handler => handler(view, new Drawing.EditEventArgs<TValue>(Hosted(change))))))
            let shown = (Func<Option<TValue>, IO<Unit>>)(value => view.Show(value.Match(
                Some: held => form.Draw(text, held, Some(rest), edited, caption),
                None: () => form.Draw(text, rest, Some(rest), edited, caption) switch { var mixed => mixed with { Conditions = mixed.Conditions.TryAdd(Condition.Mixed) } })))
            from initial in shown(None)
            select new NumericPart<TValue>(
                new TableCell(view, scaleWidth: !form.Square), form.Square, shown,
                handler => handlers.Swap(held => held.Add(handler)), handler => handlers.Swap(held => held.Filter(each => each != handler))))
        .As();

    private static IO<NumericCell<TValue, TKey>> Cell<TValue, TKey>(NumericDomain<TValue, TKey> domain, TValue rest, string caption, RowScope scope, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from text in IO.pure(NumberText.Scalar(domain, RowEdit.Varies))
        from typed in Typed(text, domain.Create, domain.Key, domain.Bounds, scope.Sink, retain)
        from track in Track(text, rest, caption, scope, retain)
        from value in Containers.Frame(() => new TableLayout(new TableRow(track.Map(static part => part.Cell).ToSeq().Add(typed.Part.Cell))), retain)
        from spaced in IO.lift(() => { value.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table); })
        select new NumericCell<TValue, TKey>(value, typed.Field.Fit, track, typed.Part, text, domain.Key, domain.Bounds);

    private static IO<NumberText<Length>> Measure(RhinoDoc doc, (Length Low, Length High) soft, bool modelUnits) =>
        DistanceDisplay.Read(doc, modelUnits).Map<NumberText<Length>>(display => new DistanceText(modelUnits ? doc.ModelUnits : doc.PageUnits, display, soft, RowEdit.Varies));

    private static IO<RowCells> Bound<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, NumericCell<TValue, TKey> cell,
        Func<RowEdit<TRecord>, EventHandler<Drawing.EditEventArgs<TValue>>, IO<Unit>> follow, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : notnull =>
        from handled in IO.pure(memo(Handled(lens, site)))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => cell.Parts.Iter(part => part.Add(handler)), handler => cell.Parts.Iter(part => part.Remove(handler)),
            handled, held => cell.Show(held.Map(lens.Get)), scope, site, retain)
        from followed in follow(bound, handled(bound))
        select cell.Cells(None, Some(lens.Get(source.Default)), bound);

    private static IO<RowCells> Toggled<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from cell in Cell(domain, lens.Get(source.Default).Value, RowText.Localize(field.Caption, scope.Sink).Local, scope, retain)
        from gate in DisposalOps.Acquire(IO.lift(static () => new CheckBox()), retain)
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(OptionalNumeric))
        let enabled = Prelude.lens(lens, Gated<TValue>.EnabledEntry.Lens)
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => gate.CheckedChanged += handler, handler => gate.CheckedChanged -= handler,
            edit => Callbacks.Handler<EventArgs>(_ => Optional(gate.Checked).Match(
                Some: state => edit.Take(new Drawing.Edit<bool>.Commit(state), Into(enabled)).Map(unit),
                None: static () => IO.pure(unit)), site),
            held => held.Map(lens.Get) switch {
                var gated => IO.lift(() => {
                    (gate.ThreeState, gate.Checked) = (gated.IsNone, gated.Map(static state => state.Enabled).ToNullable());
                    cell.Value.Enabled = gate.Enabled && gated.ForAll(static state => state.Enabled);
                }).Bind(_ => cell.Show(gated.Map(static state => state.Value))),
            }, scope, site, retain)
        from edits in cell.Edits(Handled(Prelude.lens(lens, Gated<TValue>.ValueEntry.Lens), site)(bound), retain)
        from attached in cell.Attached(site, retain)
        select cell.Cells(Some<Control>(gate), lens.Get(source.Default).Active, bound);

    private static IO<RowCells> Measured<TRecord, TValue>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, ValidationError>, IConvertible<Length>, IMinMaxValue<TValue> =>
        from doc in scope.DocumentIO
        from text in Measure(doc, soft, modelUnits)
        let bounds = (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue())
        from typed in Typed(text, ValidationResults.Validated<TValue, Length>, static value => value.ToValue(), bounds, scope.Sink, retain)
        let cell = new NumericCell<TValue, Length>(typed.Field, typed.Field.Fit, None, typed.Part, text, static value => value.ToValue(), bounds)
        from cells in Bound(source, field, lens, cell,
            (_, _) => EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber).Inline(_ => Measure(doc, soft, modelUnits).Bind(typed.Field.Retext), scope.Sink, retain),
            scope, new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Distance)), retain)
        select cells;
}
