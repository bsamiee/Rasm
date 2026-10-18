using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record NumericPart<TValue>(
    TableCell Cell, bool Square, Func<Option<TValue>, IO<Unit>> Receive, Action<EventHandler<EditEventArgs<TValue>>> Add, Action<EventHandler<EditEventArgs<TValue>>> Remove)
    where TValue : notnull;

internal sealed record NumericCell<TValue, TKey>(Control Value, FieldFit Fit, Option<NumericPart<TValue>> Track, NumericPart<TValue> Field, NumberText<TKey> Text, Func<TValue, TKey> Key, (TKey Low, TKey High) Bounds)
    where TValue : notnull
    where TKey : notnull {
    public Seq<NumericPart<TValue>> Parts => Track.ToSeq().Add(Field);

    public IO<Unit> Show(Option<TValue> value) => Parts.TraverseM(part => part.Receive(value)).As().Map(unit);

    public IO<Unit> Edits(EventHandler<EditEventArgs<TValue>> handler, Action<IDisposable> retain) =>
        Parts.TraverseM(part => Subscriptions.Attach(part.Add, part.Remove, handler, retain)).As().Map(unit);

    public IO<Unit> Attached(IO<Unit> reset, CallbackSite site, Action<IDisposable> retain) =>
        (
            Track.Map(part => Subscriptions.Attach(
                handler => part.Cell.Control.MouseDoubleClick += handler, handler => part.Cell.Control.MouseDoubleClick -= handler,
                Callbacks.Handler<MouseEventArgs>(e => when(e.Buttons is MouseButtons.Primary, IO.lift(() => { e.Handled = true; }).Bind(_ => reset)).As(),
                    site with { Member = nameof(Control.MouseDoubleClick) }), retain)).ToSeq()
            + Track.Filter(static part => part.Square).Map(part => Subscriptions.Attach(
                handler => Field.Cell.Control.SizeChanged += handler, handler => Field.Cell.Control.SizeChanged -= handler,
                Callbacks.Handler<EventArgs>(_ => IO.lift(() => { part.Cell.Control.Width = Field.Cell.Control.Height; }),
                    site with { Member = nameof(Control.SizeChanged) }), retain)).ToSeq())
        .TraverseM(static attached => attached).As().Map(unit);

    public RowCells Cells<TRecord>(Option<Control> gate, Option<TValue> initial, RowEdit<TRecord> edit, CallbackSite site) where TRecord : notnull =>
        new(Value, gate, None, Some(Fit), [], [],
            new RowHelp(Some((Text.Shown(Some(Bounds.Low)), Text.Shown(Some(Bounds.High)))), initial.Map(value => Text.Shown(Some(Key(value))))),
            edit.Shown, Some<RowEdit>(edit), Some(ChoiceRows.Context(Value, [], [], site.Sink)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumericRows {
    // --- [CROSSINGS]
    public static TKey Narrowed<TKey>(double key) where TKey : struct, INumber<TKey> =>
        TKey.CreateSaturating(TKey.IsInteger(TKey.CreateSaturating(0.5)) ? double.Round(key, MidpointRounding.ToEven) : key);

    // --- [ROWS]
    public static ControlRow Numeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain, RowRules rules)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) =>
            from cell in Cell(domain, field.Wording.Shown(field.Caption, scope.Sink), scope.Sink, retain)
            let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Numeric))
            from cells in Bound(source, field, lens, cell, (edit, _) => cell.Attached(edit.Reset, site, retain), scope, site, retain)
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
        RowSource<TRecord> source, string caption, Lens<TRecord, TValue> lens, Presentation<TKey> presentation, RowEdit<TRecord> edit, Option<Control> pick, IPlugInSink sink, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, ValidationError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey> =>
        from cell in Cell(NumericDomain.Bounded<TValue, TKey>(presentation), Wording.English.Shown(caption, sink), sink, retain)
        let site = new CallbackSite(sink, typeof(NumericRows), nameof(Line))
        from edits in cell.Edits(Handled(lens, site)(edit), retain)
        from attached in cell.Attached(edit.Take(new Edit<TValue>.Commit(lens.Get(source.Default)), Into(lens)).Map(unit), site, retain)
        select (new RowLine(Some(caption), cell.Value, pick, Some(cell.Fit)), (Func<Option<TRecord>, IO<Unit>>)(held => cell.Show(held.Map(lens.Get))));

    // --- [REALIZE]
    private static Func<TValue, TRecord, Fin<TRecord>> Into<TRecord, TValue>(Lens<TRecord, TValue> lens) => (value, held) => lens.Set(value, held);

    private static Func<RowEdit<TRecord>, EventHandler<EditEventArgs<TValue>>> Handled<TRecord, TValue>(Lens<TRecord, TValue> lens, CallbackSite site)
        where TRecord : notnull
        where TValue : notnull =>
        edit => Callbacks.Handler<EditEventArgs<TValue>>(change => edit.Take(change.Edit, Into(lens)).Map(unit), site);

    private static IO<(NumberField<TValue, TKey> Field, NumericPart<TValue> Part)> Typed<TValue, TKey>(
        NumberText<TKey> text, Func<TKey, Fin<TValue>> create, Func<TValue, TKey> key, (TKey Low, TKey High) bounds, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : notnull, IComparable<TKey> =>
        DisposalOps.Acquire(IO.lift(() => new NumberField<TValue, TKey>(text, create, key, bounds, sink)), retain).Map(static field => (
            field,
            new NumericPart<TValue>(new TableCell(field), Square: false, field.Receive, handler => field.Edited += handler, handler => field.Edited -= handler)));

    private static IO<Option<NumericPart<TValue>>> Track<TValue, TKey>(ScalarText<TValue, TKey> text, string caption, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        text.Display.Turn
            ? DisposalOps.Acquire(IO.lift(() => new AngleDial<TValue, TKey>(sink, text, caption)), retain)
                .Map(static dial => Some(new NumericPart<TValue>(new TableCell(dial), Square: true, dial.Receive, handler => dial.Edited += handler, handler => dial.Edited -= handler)))
            : text.Domain.Presentation.Form.Map(
                track: DisposalOps.Acquire(IO.lift(() => new ParameterSlider<TValue, TKey>(sink, text, caption)), retain)
                    .Map(static slider => Some(new NumericPart<TValue>(new TableCell(slider, scaleWidth: true), Square: false, slider.Receive, handler => slider.Edited += handler, handler => slider.Edited -= handler))),
                field: IO.pure(Option<NumericPart<TValue>>.None));

    private static IO<NumericCell<TValue, TKey>> Cell<TValue, TKey>(NumericDomain<TValue, TKey> domain, string caption, IPlugInSink sink, Action<IDisposable> retain)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from text in IO.pure(NumberText.Scalar(domain, RowEdit.Varies))
        from typed in Typed(text, domain.Create, domain.Key, domain.Bounds, sink, retain)
        from track in Track(text, caption, sink, retain)
        from value in DisposalOps.Acquire(IO.lift(() => new TableLayout(new TableRow(track.Map(static part => part.Cell).ToSeq().Add(typed.Part.Cell)))), retain)
        from spaced in IO.lift(() => { value.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table); })
        select new NumericCell<TValue, TKey>(value, typed.Field.Fit, track, typed.Part, text, domain.Key, domain.Bounds);

    private static IO<NumberText<Length>> Measure(RhinoDoc doc, (Length Low, Length High) soft, bool modelUnits) =>
        DistanceDisplay.Read(doc, modelUnits).Map<NumberText<Length>>(display => new DistanceText(modelUnits ? doc.ModelUnits : doc.PageUnits, display, soft, RowEdit.Varies));

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
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from cell in Cell(domain, field.Wording.Shown(field.Caption, scope.Sink), scope.Sink, retain)
        from gate in DisposalOps.Acquire(IO.lift(static () => new CheckBox()), retain)
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(OptionalNumeric))
        let enabled = Prelude.lens(lens, Gated<TValue>.EnabledEntry.Lens)
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => gate.CheckedChanged += handler, handler => gate.CheckedChanged -= handler,
            edit => Callbacks.Handler<EventArgs>(_ => Optional(gate.Checked).Match(
                Some: state => edit.Take(new Edit<bool>.Commit(state), Into(enabled)).Map(unit),
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
