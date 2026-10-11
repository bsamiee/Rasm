using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rhino;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record VectorAxis<TRecord> where TRecord : notnull {
    internal VectorAxis(RowField<TRecord> field, Func<IPlugInSink, int, Action<IDisposable>, IO<AxisCell<TRecord>>> realize) => (Field, Realize) = (field, realize);

    public RowField<TRecord> Field { get; }
    internal Func<IPlugInSink, int, Action<IDisposable>, IO<AxisCell<TRecord>>> Realize { get; }
}

public sealed record PositionPick<TRecord>(StageInput Stage, string Caption, Func<TRecord, (double X, double Y), PixelExtent, Fin<TRecord>> Placed) where TRecord : notnull;

internal sealed record NumericPart<TValue>(
    TableCell Cell, bool Square, Func<Option<TValue>, IO<Unit>> Receive, Action<EventHandler<EditEventArgs<TValue>>> Add, Action<EventHandler<EditEventArgs<TValue>>> Remove)
    where TValue : notnull;

internal sealed record NumericCell<TValue, TKey>(Control Value, FieldFit Fit, Option<NumericPart<TValue>> Track, NumericPart<TValue> Field, NumberText<TKey> Text, Func<TValue, TKey> Key, (TKey Low, TKey High) Bounds)
    where TValue : notnull
    where TKey : notnull {
    public Seq<NumericPart<TValue>> Parts => Track.ToSeq().Add(Field);

    public IO<Unit> Show(Option<TValue> value) => Parts.TraverseM(part => part.Receive(value)).As().Map(static _ => unit);

    public IO<Unit> Edits(EventHandler<EditEventArgs<TValue>> handler, Action<IDisposable> retain) =>
        Parts.TraverseM(part => Subscriptions.Attach(part.Add, part.Remove, handler, retain)).As().Map(static _ => unit);

    public IO<Unit> Attached(IO<Unit> reset, CallbackSite site, Action<IDisposable> retain) =>
        (
            Track.Map(part => Subscriptions.Attach(
                handler => part.Cell.Control.MouseDoubleClick += handler, handler => part.Cell.Control.MouseDoubleClick -= handler,
                Callbacks.Handler<MouseEventArgs>(e => when(e.Buttons == MouseButtons.Primary, IO.lift(() => { e.Handled = true; }).Bind(_ => reset)).As(),
                    site with { Member = nameof(Control.MouseDoubleClick) }), retain)).ToSeq()
            + Track.Filter(static part => part.Square).Map(part => Subscriptions.Attach(
                handler => Field.Cell.Control.SizeChanged += handler, handler => Field.Cell.Control.SizeChanged -= handler,
                Callbacks.Handler<EventArgs>(_ => IO.lift(() => { part.Cell.Control.Width = Field.Cell.Control.Height; }),
                    site with { Member = nameof(Control.SizeChanged) }), retain)).ToSeq())
        .TraverseM(static attached => attached).As().Map(static _ => unit);

    public RowCells Cells<TRecord>(Option<Control> gate, Option<TValue> initial, RowEdit<TRecord> edit, CallbackSite site) where TRecord : notnull =>
        new(Value, gate, None, Some(Fit), [], [],
            new RowHelp(Some((Text.Shown(Some(Bounds.Low)), Text.Shown(Some(Bounds.High)))), initial.Map(value => Text.Shown(Some(Key(value))))),
            edit.Shown, Some<RowEdit>(edit), Some(ChoiceRows.Context(Value, [], [], site.Sink)));
}

internal sealed record AxisCell<TRecord>(Label Letter, FieldFit Fit, Func<Option<TRecord>, IO<Unit>> Show, Action<RowEdit<TRecord>> Add, Action<RowEdit<TRecord>> Remove)
    where TRecord : notnull;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumericRows {
    // --- [METRICS]
    internal const float FieldInset = 5f;

    // --- [CROSSINGS]
    public static TKey Narrowed<TKey>(double key) where TKey : struct, INumber<TKey> =>
        TKey.CreateSaturating(TKey.IsInteger(TKey.CreateSaturating(0.5)) ? double.Round(key, MidpointRounding.ToEven) : key);

    // --- [ROWS]
    public static ControlRow Numeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation, RowRules rules)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) =>
            from cell in Cell(presentation, create, key, bounds, Wording.English.Shown(field.Caption, scope.Sink), scope.Sink, retain)
            let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Numeric))
            from cells in Bound(source, field, lens, cell, (edit, _) => cell.Attached(edit.Reset, site, retain), scope, site, retain)
            select cells, rules);

    public static ControlRow OptionalNumeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation, RowRules rules)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Toggled(source, field, lens, create, key, bounds, presentation, scope, retain), rules);

    public static ControlRow Distance<TRecord, TValue, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Measured<TRecord, TValue, TError>(source, field, lens, soft, modelUnits, scope, retain), rules);

    public static ControlRow Vector<TRecord>(
        RowSource<TRecord> source, string caption, string help, IterableNE<VectorAxis<TRecord>> axes, Option<PositionPick<TRecord>> pick, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, axes.Map(static axis => axis.Field), caption, help, RowShape.Inline, (scope, retain) => Vectored(source, axes, pick, scope, retain), rules);

    public static VectorAxis<TRecord> Axis<TRecord, TValue, TKey, TError>(RowField<TRecord> field, Lens<TRecord, TValue> lens, Presentation<TKey> presentation, string letter)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        new(field, (sink, index, retain) =>
            from typed in Typed(NumberText.Scalar(presentation, (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), RowEdit.Varies), Conversions.Validated<TValue, TKey, TError>, static value => value.ToValue(), (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), sink, retain)
            from label in DisposalOps.Acquire(IO.lift(() => {
                typed.Part.Cell.Control.TabIndex = index;
                return RhinoLayout.NewLabel(letter);
            }), static value => IO.lift(value.Dispose), retain)
            let handled = memo(Handled(lens, new CallbackSite(sink, typeof(NumericRows), nameof(Vector))))
            select new AxisCell<TRecord>(
                label, typed.Fit, held => typed.Part.Receive(held.Map(lens.Get)), edit => typed.Part.Add(handled(edit)), edit => typed.Part.Remove(handled(edit))));

    // --- [LINES]
    public static IO<(RowLine Line, Func<Option<TRecord>, IO<Unit>> Show)> Line<TRecord, TValue, TKey, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Presentation<TKey> presentation, RowEdit<TRecord> edit, Option<Control> pick, IPlugInSink sink, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        from cell in Cell(presentation, Conversions.Validated<TValue, TKey, TError>, static value => value.ToValue(), (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), Wording.English.Shown(field.Caption, sink), sink, retain)
        let site = new CallbackSite(sink, typeof(NumericRows), nameof(Line))
        from edits in cell.Edits(Handled(lens, site)(edit), retain)
        from attached in cell.Attached(edit.Take(new Edit<TValue>.Commit(lens.Get(source.Default)), Into(lens)).Map(static _ => unit), site, retain)
        select (new RowLine(Some(field.Caption), cell.Value, pick, Some(cell.Fit)), (Func<Option<TRecord>, IO<Unit>>)(held => cell.Show(held.Map(lens.Get))));

    // --- [REALIZE]
    private static Func<TValue, TRecord, Fin<TRecord>> Into<TRecord, TValue>(Lens<TRecord, TValue> lens) => (value, held) => Fin.Succ(lens.Set(value, held));

    private static Func<RowEdit<TRecord>, EventHandler<EditEventArgs<TValue>>> Handled<TRecord, TValue>(Lens<TRecord, TValue> lens, CallbackSite site)
        where TRecord : notnull
        where TValue : notnull =>
        edit => Callbacks.Handler<EditEventArgs<TValue>>(change => edit.Take(change.Edit, Into(lens)).Map(static _ => unit), site);

    private static IO<(NumericPart<TValue> Part, FieldFit Fit)> Typed<TValue, TKey>(NumberText<TKey> text, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : notnull, IComparable<TKey> =>
        DisposalOps.Acquire(IO.lift(() => new NumberField<TValue, TKey>(text, create, key, bounds, sink)), static value => IO.lift(value.Dispose), retain).Map(field => (
            new NumericPart<TValue>(new TableCell(field), Square: false, field.Receive, handler => field.Edited += handler, handler => field.Edited -= handler),
            new FieldFit(field, text.TextWidth(field.Font) + (2f * FieldInset))));

    private static IO<Option<NumericPart<TValue>>> Track<TValue, TKey>(Presentation<TKey> presentation, NumberText<TKey> text, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, string caption, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        Quantities.Scalar(presentation, bounds).Turn
            ? DisposalOps.Acquire(IO.lift(() => new AngleDial<TValue, TKey>(sink, text, caption, create, key, bounds)), static value => IO.lift(value.Dispose), retain)
                .Map(static dial => Some(new NumericPart<TValue>(new TableCell(dial), Square: true, dial.Receive, handler => dial.Edited += handler, handler => dial.Edited -= handler)))
            : presentation.Form.Map(
                track: DisposalOps.Acquire(IO.lift(() => new ParameterSlider<TValue, TKey>(sink, presentation, text, caption, create, key, bounds)), static value => IO.lift(value.Dispose), retain)
                    .Map(static slider => Some(new NumericPart<TValue>(new TableCell(slider, scaleWidth: true), Square: false, slider.Receive, handler => slider.Edited += handler, handler => slider.Edited -= handler))),
                field: IO.pure(Option<NumericPart<TValue>>.None));

    private static IO<NumericCell<TValue, TKey>> Cell<TValue, TKey>(Presentation<TKey> presentation, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, string caption, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        NumberText.Scalar(presentation, bounds, RowEdit.Varies) switch {
            var text =>
                from typed in Typed(text, create, key, bounds, sink, retain)
                from track in Track(presentation, text, create, key, bounds, caption, sink, retain)
                from value in DisposalOps.Acquire(IO.lift(() => new TableLayout(new TableRow(track.Map(static part => part.Cell).ToSeq().Add(typed.Part.Cell)))), static value => IO.lift(value.Dispose), retain)
                from spaced in IO.lift(() => { value.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table); })
                select new NumericCell<TValue, TKey>(value, typed.Fit, track, typed.Part, text, key, bounds),
        };

    private static IO<(NumericPart<TValue> Part, FieldFit Fit, Func<Func<Action<IDisposable>, IO<NumericPart<TValue>>>, IO<Unit>> Swap)> Swappable<TValue>(
        Func<Action<IDisposable>, IO<(NumericPart<TValue> Part, FieldFit Fit)>> make, Action<IDisposable> retain) where TValue : notnull =>
        from held in IO.lift(static () => Ref((Part: Some(Option<NumericPart<TValue>>.None), Releases: Seq<IDisposable>())))
        from panel in DisposalOps.Acquire(IO.lift(() => new Panel { Tag = held }), value =>
            atomicIO(() => { held.Value = (None, held.Value.Releases); return unit; })
                .Bind(_ => IO.lift(() => { value.Content = null; value.Dispose(); }))
                .Bind(_ => atomicIO(() => {
                    Seq<IDisposable> releases = held.Value.Releases;
                    held.Value = (None, []);
                    return releases;
                })).Bind(DisposalOps.Release), retain)
        let publish = fun((NumericPart<TValue> next, IDisposable lease) => Published(held, panel, next, lease))
        from initial in DisposalOps.Replace(make, (typed, lease) => publish(typed.Part, lease))
        select (new NumericPart<TValue>(new TableCell(panel), Square: false,
                    value => held.ValueIO.Bind(static state => IO.lift(state.Part.Bind(static part => part).ToFin(Errors.Cancelled))).Bind(part => part.Receive(value)),
                    handler => held.Value.Part.Bind(static part => part).ToFin(Errors.Cancelled).ThrowIfFail().Add(handler),
                    handler => held.Value.Part.Bind(static part => part).Iter(part => part.Remove(handler))),
                initial.Fit,
                (Func<Func<Action<IDisposable>, IO<NumericPart<TValue>>>, IO<Unit>>)(replace =>
                    DisposalOps.Replace(replace, publish).Map(static _ => unit)));

    private static IO<Unit> Published<TValue>(
        Ref<(Option<Option<NumericPart<TValue>>> Part, Seq<IDisposable> Releases)> held, Panel panel, NumericPart<TValue> next, IDisposable lease) where TValue : notnull {
        (Option<Option<NumericPart<TValue>>> Part, Seq<IDisposable> Releases) prior = atomic(() => {
            (Option<Option<NumericPart<TValue>>> Part, Seq<IDisposable> Releases) current = held.Value;
            _ = current.Part.ToFin(Errors.Cancelled).ThrowIfFail();
            held.Value = (current.Part, current.Releases.Add(lease));
            return current;
        });
        return DisposalOps.OnFailure(
            IO.lift(() => { panel.Content = next.Cell.Control; }),
            IO.lift(() => prior.Part.Bind(static part => part).Match(
                Some: part => { panel.Content = part.Cell.Control; }, None: () => panel.Content = null))
                .Bind(_ => atomicIO(() => {
                    (Option<Option<NumericPart<TValue>>> Part, Seq<IDisposable> Releases) current = held.Value;
                    held.Value = (current.Part.Bind(_ => prior.Part), current.Releases.Filter(owned => !ReferenceEquals(owned, lease)));
                    return unit;
                }))
                .Bind(_ => DisposalOps.Release(Seq(lease))))
            .Bind(_ => atomicIO(() => {
                (Option<Option<NumericPart<TValue>>> Part, Seq<IDisposable> Releases) current = held.Value;
                held.Value = (current.Part.Map(_ => Some(next)), current.Releases.Filter(owned => ReferenceEquals(owned, lease)));
                return unit;
            }))
            .Bind(_ => DisposalOps.Release(prior.Releases));
    }

    private static IO<NumberText<Length>> Measure(RhinoDoc doc, (Length Low, Length High) soft, bool modelUnits) =>
        DistanceDisplay.Read(doc, modelUnits).Map(display => NumberText.Distance(modelUnits ? doc.ModelUnits : doc.PageUnits, display, soft, RowEdit.Varies));

    private static IO<RowCells> Bound<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, NumericCell<TValue, TKey> cell,
        Func<RowEdit<TRecord>, EventHandler<EditEventArgs<TValue>>, IO<Unit>> follow, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : notnull =>
        from handled in IO.pure(memo(Handled(lens, site)))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => cell.Parts.Iter(part => part.Add(handler)), handler => cell.Parts.Iter(part => part.Remove(handler)),
            handled, held => cell.Show(held.Map(lens.Get)), scope, site, retain)
        from followed in follow(bound, handled(bound))
        select cell.Cells(None, Some(lens.Get(source.Default)), bound, site);

    private static IO<RowCells> Toggled<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, Presentation<TKey> presentation, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from cell in Cell(presentation, create, key, bounds, Wording.English.Shown(field.Caption, scope.Sink), scope.Sink, retain)
        from gate in DisposalOps.Acquire(IO.lift(static () => new CheckBox()), static value => IO.lift(value.Dispose), retain)
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(OptionalNumeric))
        let enabled = Prelude.lens(lens, Gated<TValue>.EnabledEntry.Lens)
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => gate.CheckedChanged += handler, handler => gate.CheckedChanged -= handler,
            edit => Callbacks.Handler<EventArgs>(_ => Optional(gate.Checked).Match(
                Some: state => edit.Take(new Edit<bool>.Commit(state), Into(enabled)).Map(static _ => unit),
                None: static () => IO.pure(unit)), site),
            held => held.Map(lens.Get) switch {
                var gated => IO.lift(() => {
                    (gate.ThreeState, gate.Checked) = (gated.IsNone, gated.Map(static state => state.Enabled).ToNullable());
                    cell.Value.Enabled = gate.Enabled && gated.ForAll(static state => state.Enabled);
                }).Bind(_ => cell.Show(gated.Map(static state => state.Value))),
            }, scope, site, retain)
        from edits in cell.Edits(Handled(Prelude.lens(lens, Gated<TValue>.ValueEntry.Lens), site)(bound), retain)
        from attached in cell.Attached(bound.Reset, site, retain)
        select cell.Cells(Some<Control>(gate), lens.Get(source.Default).Active, bound, site);

    private static IO<RowCells> Measured<TRecord, TValue, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
        from text in Measure(doc, soft, modelUnits)
        from slot in Swappable(keep => Typed(text, Conversions.Validated<TValue, Length, TError>, static value => value.ToValue(), (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), scope.Sink, keep), retain)
        let cell = new NumericCell<TValue, Length>(slot.Part.Cell.Control, slot.Fit with { Field = slot.Part.Cell.Control }, None, slot.Part, text, static value => value.ToValue(), (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()))
        from cells in Bound(source, field, lens, cell,
            (edit, handler) => EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber).Inline(_ =>
                Subscriptions.Detached(slot.Part.Add, slot.Part.Remove, handler,
                        Measure(doc, soft, modelUnits).Bind(next => slot.Swap(keep => Typed(next, Conversions.Validated<TValue, Length, TError>, static value => value.ToValue(), (TValue.MinValue.ToValue(), TValue.MaxValue.ToValue()), scope.Sink, keep).Map(static typed => typed.Part))))
                    .Bind(_ => edit.Shown), scope.Sink, retain),
            scope, new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Distance)), retain)
        select cells;

    private static IO<RowCells> Vectored<TRecord>(RowSource<TRecord> source, IterableNE<VectorAxis<TRecord>> axes, Option<PositionPick<TRecord>> pick, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from cells in toSeq(axes).Map((axis, index) => axis.Realize(scope.Sink, index, retain)).TraverseM(static cell => cell).As()
        from value in DisposalOps.Acquire(IO.lift(() => new TableLayout(new TableRow(cells.Bind(static cell => Seq(new TableCell(cell.Letter), new TableCell(cell.Fit.Field)))))), static value => IO.lift(value.Dispose), retain)
        from spaced in IO.lift(() => { value.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table); })
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Vector))
        let fit = Optional(cells.Map(static cell => cell.Fit).MaxBy(static fit => fit.Width))
        let widen = fun((Control widest) => IO.lift(() => cells.Iter(cell => cell.Fit.Field.Width = widest.Width)))
        from bound in RowEdit.Bind<TRecord, Func<RowEdit<TRecord>>>(source, axes.Map(static axis => axis.Field),
            handler => cells.Iter(cell => cell.Add(handler())), handler => cells.Iter(cell => cell.Remove(handler())),
            static edit => () => edit, held => cells.TraverseM(cell => cell.Show(held)).As().Map(static _ => unit), scope, site, retain)
        from placed in pick.Traverse(held => ButtonRows.Place(held.Stage, held.Caption,
                (pixel, extent) => scope.Read(source).Bind(record => bound.Commit(held.Placed(record, pixel, extent))).Map(static _ => unit), scope, retain)).As()
        from followed in
            fit.Traverse(widest => Subscriptions.Attach(
                handler => widest.Field.SizeChanged += handler, handler => widest.Field.SizeChanged -= handler,
                Callbacks.Handler<EventArgs>(_ => widen(widest.Field), site with { Member = nameof(Control.SizeChanged) }), retain)).As()
        select new RowCells(
            value, None, placed.Map(static held => (Control)held), fit, [], [], new RowHelp(None, None), bound.Shown, Some<RowEdit>(bound),
            Some(ChoiceRows.Context(value, [], [], scope.Sink)));
}
