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
    internal VectorAxis(RowField<TRecord> field, Func<IPlugInSink, int, IO<AxisCell<TRecord>>> realize) => (Field, Realize) = (field, realize);

    public RowField<TRecord> Field { get; }
    internal Func<IPlugInSink, int, IO<AxisCell<TRecord>>> Realize { get; }
}

public sealed record PositionPick<TRecord>(StageInput Stage, string Caption, Func<TRecord, System.Drawing.Point, PixelExtent, Fin<TRecord>> Placed) where TRecord : notnull;

internal sealed record NumericPart<TValue>(
    TableCell Cell, bool Square, Func<Option<TValue>, IO<Unit>> Receive, Action<EventHandler<Edit<TValue>>> Add, Action<EventHandler<Edit<TValue>>> Remove)
    where TValue : notnull;

internal sealed record NumericCell<TValue, TKey>(Control Value, FieldFit Fit, Option<NumericPart<TValue>> Track, NumericPart<TValue> Field, NumberText<TKey> Text)
    where TValue : notnull, IMinMaxValue<TValue>, IConvertible<TKey>
    where TKey : notnull {
    public Seq<NumericPart<TValue>> Parts => Track.ToSeq().Add(Field);

    public IO<Unit> Show(Option<TValue> value) => Parts.TraverseM(part => part.Receive(value)).As().Map(static _ => unit);

    public IO<Seq<IDisposable>> Edits(EventHandler<Edit<TValue>> handler) =>
        DisposalOps.AcquireAll(Parts.Map(part => Subscriptions.Attach(part.Add, part.Remove, handler)), DisposalOps.Release);

    public IO<IDisposable> Attached(IO<Unit> reset, CallbackSite site) =>
        DisposalOps.AcquireAll(
            Track.Map(part => Subscriptions.Attach(
                handler => part.Cell.Control.MouseDoubleClick += handler, handler => part.Cell.Control.MouseDoubleClick -= handler,
                Callbacks.Handler<MouseEventArgs>(e => when(e.Buttons == MouseButtons.Primary, IO.lift(() => { e.Handled = true; }).Bind(_ => reset)).As(),
                    site with { Member = nameof(Control.MouseDoubleClick) }))).ToSeq()
            + Track.Filter(static part => part.Square).Map(part => Subscriptions.Attach(
                handler => Field.Cell.Control.SizeChanged += handler, handler => Field.Cell.Control.SizeChanged -= handler,
                Callbacks.Handler<EventArgs>(_ => IO.lift(() => { part.Cell.Control.Width = Field.Cell.Control.Height; }),
                    site with { Member = nameof(Control.SizeChanged) }))).ToSeq(),
            DisposalOps.Release)
        .Map(held => DisposalOps.Composite(held, site));

    public RowCells Cells<TRecord>(Option<Control> gate, Option<TValue> initial, RowEdit<TRecord> edit, Seq<IDisposable> held, CallbackSite site) where TRecord : notnull =>
        new(Value, gate, None, Some(Fit), [], [],
            new RowHelp(Some((Text.Shown(Some(TValue.MinValue.ToValue())), Text.Shown(Some(TValue.MaxValue.ToValue())))), initial.Map(value => Text.Shown(Some(value.ToValue())))),
            edit.Shown, Some<RowEdit>(edit), Some(ChoiceRows.Context(Value, [], [], site.Sink)), DisposalOps.Composite(held, site));
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
    public static ControlRow Bounded<TRecord, TValue, TKey, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Inline, scope =>
            from cell in Cell<TValue, TKey, TError>(presentation, Wording.English.Shown(field.Caption, scope.Sink), scope.Sink)
            let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Bounded))
            from cells in Bound(source, field, lens, cell, (edit, _) => cell.Attached(edit.Reset, site), scope, site)
            select cells, rules);

    public static ControlRow OptionalBounded<TRecord, TValue, TKey, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, Presentation<TValue, TKey> presentation, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Inline, scope => Toggled<TRecord, TValue, TKey, TError>(source, field, lens, presentation, scope), rules);

    public static ControlRow Distance<TRecord, TValue, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Inline, scope => Measured<TRecord, TValue, TError>(source, field, lens, soft, modelUnits, scope), rules);

    public static ControlRow Vector<TRecord>(
        RowSource<TRecord> source, string caption, string help, IterableNE<VectorAxis<TRecord>> axes, Option<PositionPick<TRecord>> pick, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, axes.Map(static axis => axis.Field), caption, help, RowShape.Inline, scope => Vectored(source, axes, pick, scope), rules);

    public static VectorAxis<TRecord> Axis<TRecord, TValue, TKey, TError>(RowField<TRecord> field, Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation, string letter)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        new(field, (sink, index) =>
            from typed in Typed<TValue, TKey, TError>(NumberText.Scalar(presentation, RowEdit.Varies), sink)
            from label in IO.lift(() => {
                typed.Part.Cell.Control.TabIndex = index;
                return RhinoLayout.NewLabel(letter);
            })
            let handled = memo(Handled(lens, new CallbackSite(sink, typeof(NumericRows), nameof(Vector))))
            select new AxisCell<TRecord>(
                label, typed.Fit, held => typed.Part.Receive(held.Map(lens.Get)), edit => typed.Part.Add(handled(edit)), edit => typed.Part.Remove(handled(edit))));

    // --- [LINES]
    public static IO<(RowLine Line, Func<Option<TRecord>, IO<Unit>> Show, IDisposable Release)> Line<TRecord, TValue, TKey, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Presentation<TValue, TKey> presentation, RowEdit<TRecord> edit, Option<Control> pick, IPlugInSink sink)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        from cell in Cell<TValue, TKey, TError>(presentation, Wording.English.Shown(field.Caption, sink), sink)
        let site = new CallbackSite(sink, typeof(NumericRows), nameof(Line))
        from edits in cell.Edits(Handled(lens, site)(edit))
        from attached in DisposalOps.OnFailure(cell.Attached(edit.Take(new Edit<TValue>.Commit(lens.Get(source.Default)), Into(lens)), site), DisposalOps.Release(edits))
        select (new RowLine(Some(field.Caption), cell.Value, pick, Some(cell.Fit)), (Func<Option<TRecord>, IO<Unit>>)(held => cell.Show(held.Map(lens.Get))),
            DisposalOps.Composite(edits.Add(attached), site));

    // --- [REALIZE]
    private static Func<TValue, TRecord, Fin<TRecord>> Into<TRecord, TValue>(Lens<TRecord, TValue> lens) => (value, held) => Fin.Succ(lens.Set(value, held));

    private static Func<RowEdit<TRecord>, EventHandler<Edit<TValue>>> Handled<TRecord, TValue>(Lens<TRecord, TValue> lens, CallbackSite site)
        where TRecord : notnull
        where TValue : notnull =>
        edit => Callbacks.Handler<Edit<TValue>>(change => edit.Take(change, Into(lens)), site);

    private static IO<(NumericPart<TValue> Part, FieldFit Fit)> Typed<TValue, TKey, TError>(NumberText<TKey> text, IPlugInSink sink)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : notnull, IComparable<TKey>
        where TError : Error, IValidationError<TError> =>
        IO.lift(() => new NumberField<TValue, TKey, TError>(text, sink)).Map(field => (
            new NumericPart<TValue>(new TableCell(field), Square: false, field.Receive, handler => field.Edited += handler, handler => field.Edited -= handler),
            new FieldFit(field, text.TextWidth(field.Font) + (2f * FieldInset))));

    private static IO<Option<NumericPart<TValue>>> Track<TValue, TKey, TError>(Presentation<TValue, TKey> presentation, NumberText<TKey> text, string caption, IPlugInSink sink)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        Quantities.Scalar(presentation).Turn
            ? IO.lift(() => new AngleDial<TValue, TKey, TError>(sink, text, caption))
                .Map(static dial => Some(new NumericPart<TValue>(new TableCell(dial), Square: true, dial.Receive, handler => dial.Edited += handler, handler => dial.Edited -= handler)))
            : presentation.Form.Map(
                track: IO.lift(() => new ParameterSlider<TValue, TKey, TError>(presentation, text, sink))
                    .Map(static slider => Some(new NumericPart<TValue>(new TableCell(slider, scaleWidth: true), Square: false, slider.Receive, handler => slider.Edited += handler, handler => slider.Edited -= handler))),
                field: IO.pure(Option<NumericPart<TValue>>.None));

    private static IO<NumericCell<TValue, TKey>> Cell<TValue, TKey, TError>(Presentation<TValue, TKey> presentation, string caption, IPlugInSink sink)
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        NumberText.Scalar(presentation, RowEdit.Varies) switch {
            var text =>
                from typed in Typed<TValue, TKey, TError>(text, sink)
                from track in Track<TValue, TKey, TError>(presentation, text, caption, sink)
                from value in IO.lift(() => new TableLayout(new TableRow(track.Map(static part => part.Cell).ToSeq().Add(typed.Part.Cell))) { Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table) })
                select new NumericCell<TValue, TKey>(value, typed.Fit, track, typed.Part, text),
        };

    private static IO<(NumericPart<TValue> Part, Func<NumericPart<TValue>, IO<Unit>> Swap)> Swappable<TValue>(NumericPart<TValue> first) where TValue : notnull =>
        from panel in IO.lift(() => new Panel { Content = first.Cell.Control })
        from held in IO.lift(() => Atom((Before: first, After: first)))
        select (new NumericPart<TValue>(new TableCell(panel), Square: false,
                    value => held.ValueIO.Bind(state => state.After.Receive(value)), handler => held.Value.After.Add(handler), handler => held.Value.After.Remove(handler)),
                (Func<NumericPart<TValue>, IO<Unit>>)(next => held.SwapIO(state => (state.After, next)).Bind(moved => IO.lift(() => {
                    panel.Content = moved.After.Cell.Control;
                    moved.Before.Cell.Control.Dispose();
                }))));

    private static IO<NumberText<Length>> Measure(RhinoDoc doc, (Length Low, Length High) soft, bool modelUnits) =>
        DistanceDisplay.Read(doc, modelUnits).Map(display => NumberText.Distance(modelUnits ? doc.ModelUnits : doc.PageUnits, display, soft, RowEdit.Varies));

    private static IO<RowCells> Bound<TRecord, TValue, TKey>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, NumericCell<TValue, TKey> cell,
        Func<RowEdit<TRecord>, EventHandler<Edit<TValue>>, IO<IDisposable>> follow, RowScope scope, CallbackSite site)
        where TRecord : notnull
        where TValue : notnull, IMinMaxValue<TValue>, IConvertible<TKey>
        where TKey : notnull =>
        from handled in IO.pure(memo(Handled(lens, site)))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => cell.Parts.Iter(part => part.Add(handler)), handler => cell.Parts.Iter(part => part.Remove(handler)),
            handled, held => cell.Show(held.Map(lens.Get)), scope, site)
        from followed in DisposalOps.OnFailure(follow(bound.Edit, handled(bound.Edit)), IO.lift(bound.Release.Dispose))
        select cell.Cells(None, Some(lens.Get(source.Default)), bound.Edit, Seq(bound.Release, followed), site);

    private static IO<RowCells> Toggled<TRecord, TValue, TKey, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<TValue>> lens, Presentation<TValue, TKey> presentation, RowScope scope)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, TKey, TError>, IConvertible<TKey>, IMinMaxValue<TValue>
        where TKey : struct, INumber<TKey>
        where TError : Error, IValidationError<TError> =>
        from cell in Cell<TValue, TKey, TError>(presentation, Wording.English.Shown(field.Caption, scope.Sink), scope.Sink)
        from gate in IO.lift(static () => new CheckBox())
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(OptionalBounded))
        let enabled = Prelude.lens(lens, Gated<TValue>.EnabledEntry.Lens)
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            handler => gate.CheckedChanged += handler, handler => gate.CheckedChanged -= handler,
            edit => Callbacks.Handler<EventArgs>(_ => Optional(gate.Checked).Match(
                Some: state => edit.Take(new Edit<bool>.Commit(state), Into(enabled)),
                None: static () => IO.pure(unit)), site),
            held => held.Map(lens.Get) switch {
                var gated => IO.lift(() => {
                    (gate.ThreeState, gate.Checked) = (gated.IsNone, gated.Map(static state => state.Enabled).ToNullable());
                    cell.Value.Enabled = gate.Enabled && gated.ForAll(static state => state.Enabled);
                }).Bind(_ => cell.Show(gated.Map(static state => state.Value))),
            }, scope, site)
        from edits in DisposalOps.OnFailure(cell.Edits(Handled(Prelude.lens(lens, Gated<TValue>.ValueEntry.Lens), site)(bound.Edit)), IO.lift(bound.Release.Dispose))
        from attached in DisposalOps.OnFailure(cell.Attached(bound.Edit.Reset, site), DisposalOps.Release(bound.Release.Cons(edits)))
        select cell.Cells(Some<Control>(gate), lens.Get(source.Default).Active, bound.Edit, bound.Release.Cons(edits).Add(attached), site);

    private static IO<RowCells> Measured<TRecord, TValue, TError>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowScope scope)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, TError>, IConvertible<Length>, IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        from doc in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
        from text in Measure(doc, soft, modelUnits)
        from typed in Typed<TValue, Length, TError>(text, scope.Sink)
        from slot in Swappable(typed.Part)
        let cell = new NumericCell<TValue, Length>(slot.Part.Cell.Control, typed.Fit with { Field = slot.Part.Cell.Control }, None, slot.Part, text)
        from cells in Bound(source, field, lens, cell,
            (edit, handler) => EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber).Inline(changed =>
                Subscriptions.Detached(slot.Part.Add, slot.Part.Remove, handler,
                        Measure(doc, soft, modelUnits).Bind(next => Typed<TValue, Length, TError>(next, scope.Sink)).Bind(next => slot.Swap(next.Part)))
                    .Bind(_ => edit.Shown), scope.Sink),
            scope, new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Distance)))
        select cells;

    private static IO<RowCells> Vectored<TRecord>(RowSource<TRecord> source, IterableNE<VectorAxis<TRecord>> axes, Option<PositionPick<TRecord>> pick, RowScope scope)
        where TRecord : notnull =>
        from cells in toSeq(axes).Map((axis, index) => axis.Realize(scope.Sink, index)).TraverseM(static cell => cell).As()
        from value in IO.lift(() => new TableLayout(new TableRow(cells.Bind(static cell => Seq(new TableCell(cell.Letter), new TableCell(cell.Fit.Field))))) {
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table),
        })
        let site = new CallbackSite(scope.Sink, typeof(NumericRows), nameof(Vector))
        let fit = Optional(cells.Map(static cell => cell.Fit).MaxBy(static fit => fit.Width))
        let widen = fun((Control widest) => IO.lift(() => cells.Iter(cell => cell.Fit.Field.Width = widest.Width)))
        from bound in RowEdit.Bind<TRecord, Func<RowEdit<TRecord>>>(source, axes.Map(static axis => axis.Field),
            handler => cells.Iter(cell => cell.Add(handler())), handler => cells.Iter(cell => cell.Remove(handler())),
            static edit => () => edit, held => cells.TraverseM(cell => cell.Show(held)).As().Map(static _ => unit), scope, site)
        from placed in DisposalOps.OnFailure(
            pick.Traverse(held => ButtonRows.Place(held.Stage, held.Caption,
                (pixel, extent) => scope.Read(source).Bind(record => bound.Edit.Commit(held.Placed(record, pixel, extent))), scope)).As(),
            IO.lift(bound.Release.Dispose))
        let taken = bound.Release.Cons(placed.Map(static held => held.Release).ToSeq())
        from followed in DisposalOps.OnFailure(
            fit.Traverse(widest => Subscriptions.Attach(
                handler => widest.Field.SizeChanged += handler, handler => widest.Field.SizeChanged -= handler,
                Callbacks.Handler<EventArgs>(_ => widen(widest.Field), site with { Member = nameof(Control.SizeChanged) }))).As(),
            DisposalOps.Release(taken))
        select new RowCells(
            value, None, placed.Map(static held => (Control)held.Control), fit, [], [], new RowHelp(None, None), bound.Edit.Shown, Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(value, [], [], scope.Sink)), DisposalOps.Composite(taken + followed.ToSeq(), site));
}
