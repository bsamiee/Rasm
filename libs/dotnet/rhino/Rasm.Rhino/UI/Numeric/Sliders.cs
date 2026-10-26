using System.Numerics;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino;
using SkiaSharp;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record NumericTrack<TKey>(Func<Option<TKey>, Func<Edit<float>, IO<Unit>>, Node> Drawn, Func<float, TKey> Keyed, bool Square) where TKey : struct, INumber<TKey>;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sliders {
    // --- [ROWS]
    public static ControlRow Numeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain, RowRules rules, Seq<Node> ground = default)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, parameter, RowShape.Inline, (scope, retain) => Cell(source, parameter, lens, domain, ground, static _ => true, scope, retain), rules);

    public static ControlRow OptionalNumeric<TRecord, TValue, TKey>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain, RowRules rules, Seq<Node> ground = default)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        ControlRow.Of(source, parameter, RowShape.Inline, (scope, retain) =>
            from gate in ToggleRows.Boxed(source, parameter, Prelude.lens(lens, Gated<TValue>.EnabledEntry.Lens), scope, retain)
            from cells in Cell(source, parameter, Prelude.lens(lens, Gated<TValue>.ValueEntry.Lens), domain, ground, record => lens.Get(record).Enabled, scope, retain)
            select cells with { Gate = Some(gate.Value), Show = gate.Show.Bind(_ => cells.Show) }, rules);

    public static Option<ControlRow> Grounded<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Seq<Node> ground, RowRules rules) where TRecord : notnull =>
        parameter.Kind.Accept(new Grounding<TRecord>(source, parameter, ground, rules));

    public static ControlRow Distance<TRecord, TValue>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, (Length Low, Length High) soft, bool modelUnits, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Length, ValidationError>, IConvertible<Length>, IMinMaxValue<TValue> =>
        ControlRow.Of(source, parameter, RowShape.Inline, (scope, retain) =>
            from doc in scope.DocumentIO
            from text in Measure(doc, soft, modelUnits)
            let bounds = (Low: TValue.MinValue.ToValue(), High: TValue.MaxValue.ToValue())
            let site = new CallbackSite(scope.Sink, typeof(Sliders), nameof(Distance))
            from field in DisposalOps.Acquire(IO.lift(() => new NumberField<TValue, Length>(text, ValidationResults.Validated<TValue, Length>, static value => value.ToValue(), bounds, scope.Sink)), retain)
            from edit in Typed(source, parameter, lens, field, static _ => true, site, scope, retain)
            from retexted in EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber).Inline(_ => Measure(doc, soft, modelUnits).Bind(field.Retext), scope.Sink, retain)
            select Cells(field, field, text, bounds, lens.Get(source.Default).ToValue(), edit), rules);

    // --- [TRACKS]
    private static readonly AxisRange Compass = AxisRange.Create(MathF.PI / 2f, (MathF.PI / 2f) - MathF.Tau);

    private static Option<NumericTrack<TKey>> Tracked<TValue, TKey>(ScalarText<TValue, TKey> text, TKey rest, Seq<Node> ground, string caption)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        text.Display.Turn
            ? AxisRange.TryCreate(float.CreateSaturating(text.Domain.Bounds.Low), float.CreateSaturating(text.Domain.Bounds.High), out AxisRange? turn)
                ? Some(Turned(text, new Axis.Periodic(turn, 0, 0), rest, caption))
                : None
            : text.Domain.Presentation.Placed(rest, text.Domain.Bounds).Map(axis => Slid(text, axis, rest, ground, caption));

    private static NumericTrack<TKey> Slid<TValue, TKey>(ScalarText<TValue, TKey> text, Axis rested, TKey rest, Seq<Node> ground, string caption)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        new((shown, edited) => shown.IfNone(rest) switch {
            var key => new Slider(new PlotPlane.Track(Orientation.Horizontal, text.Domain.Presentation.Placed(key, text.Domain.Bounds).IfNone(rested)), float.CreateSaturating(key), edited) {
                Rest = Some(float.CreateSaturating(rest)),
                Ground = ground,
                Spoken = value => text.Display.Text(value),
            } switch {
                var slider => Labeled(text.Domain.Presentation.Origin.Match(Some: origin => slider with { Origin = Some(float.CreateSaturating(origin)) }, None: () => slider).Drawn, shown, caption),
            },
        }, static value => Presentation<TKey>.Keyed(value), Square: false);

    private static NumericTrack<TKey> Turned<TValue, TKey>(ScalarText<TValue, TKey> text, Axis.Periodic turn, TKey rest, string caption)
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        (int)double.Round(double.Tau / text.Display.Step, MidpointRounding.ToEven) switch {
            var snaps => new(
                (shown, edited) => Labeled(new Dial(new Axis.Periodic(Compass, snaps, snaps), float.CreateSaturating(shown.IfNone(rest)), edited) {
                    Rest = Some(float.CreateSaturating(rest)),
                    Spoken = angle => text.Display.Text(turn.Held(angle)),
                }.Drawn, shown, caption),
                angle => Presentation<TKey>.Keyed(turn.Held(angle)),
                Square: true),
        };

    private static Node Labeled<TKey>(Node drawn, Option<TKey> shown, string caption) =>
        drawn with {
            Accessible = drawn.Accessible.Combine(new Accessible { Label = caption }),
            Conditions = shown.IsNone ? drawn.Conditions.TryAdd(Condition.Mixed) : drawn.Conditions,
        };

    // --- [REALIZE]
    private static IO<RowCells> Cell<TRecord, TValue, TKey>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain, Seq<Node> ground, Func<TRecord, bool> enabled,
        RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from text in IO.pure(new ScalarText<TValue, TKey>(domain, RowEdit.Varies))
        let rest = domain.Key(lens.Get(source.Default))
        let site = new CallbackSite(scope.Sink, typeof(Sliders), nameof(Numeric))
        from field in DisposalOps.Acquire(IO.lift(() => new NumberField<TValue, TKey>(text, domain.Create, domain.Key, domain.Bounds, scope.Sink)), retain)
        from value in Tracked(text, rest, ground, RowText.Localize(parameter.Text.Caption, scope.Sink).Local).Match(
            Some: track => Scened(source, parameter, lens, domain, field, track, enabled, site, scope, retain),
            None: () => Typed(source, parameter, lens, field, enabled, site, scope, retain).Map(edit => (Control: (Eto.Forms.Control)field, Edit: edit)))
        select Cells(value.Control, field, text, domain.Bounds, rest, value.Edit);

    private static IO<(Eto.Forms.Control Control, RowEdit<TRecord> Edit)> Scened<TRecord, TValue, TKey>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain, NumberField<TValue, TKey> field, NumericTrack<TKey> track,
        Func<TRecord, bool> enabled, CallbackSite site, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : struct, INumber<TKey> =>
        from scened in SceneRow<TRecord, Unit>.Bind(source, IterableNE.create(parameter), unit,
            (row, record, _) => track.Drawn(record.Map(held => domain.Key(lens.Get(held))), row.Edited<float>((shown, held) => domain.Create(track.Keyed(shown)).Map(value => lens.Set(value, held)))),
            (row, record, _) => Shown(field, record.Map(lens.Get), [field, row.View], record.ForAll(enabled)),
            scope, site, retain)
        from typed in Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h,
            Callbacks.Handler(scened.Row.Edited<TValue>((value, held) => lens.Set(value, held)), site), retain)
        from squared in when(track.Square, Subscriptions.Attach(h => field.SizeChanged += h, h => field.SizeChanged -= h,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => { scened.Row.View.Width = field.Height; }), site), retain)).As()
        from line in RowGrid.Lined([new Eto.Forms.TableCell(scened.Row.View, scaleWidth: !track.Square), new Eto.Forms.TableCell(field)], retain)
        select (line, scened.Edit);

    private static IO<RowEdit<TRecord>> Typed<TRecord, TValue, TKey>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, NumberField<TValue, TKey> field, Func<TRecord, bool> enabled,
        CallbackSite site, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : notnull
        where TKey : notnull, IComparable<TKey> =>
        RowEdit.Bind(source, IterableNE.create(parameter), h => field.Edited += h, h => field.Edited -= h,
            edit => Callbacks.Handler<Edit<TValue>>(change => edit.Take(change, (value, held) => lens.Set(value, held)).Map(unit), site),
            held => Shown(field, held.Map(lens.Get), [field], held.ForAll(enabled)), scope, site, retain);

    private static IO<Unit> Shown<TValue, TKey>(NumberField<TValue, TKey> field, Option<TValue> value, Seq<Eto.Forms.Control> gated, bool enabled)
        where TValue : notnull
        where TKey : notnull, IComparable<TKey> =>
        from received in field.Receive(value)
        from toggled in IO.lift(() => gated.Iter(control => control.Enabled = enabled))
        select unit;

    private static RowCells Cells<TRecord, TValue, TKey>(Eto.Forms.Control value, NumberField<TValue, TKey> field, NumberText<TKey> text, (TKey Low, TKey High) bounds, TKey rest, RowEdit<TRecord> edit)
        where TRecord : notnull
        where TValue : notnull
        where TKey : notnull, IComparable<TKey> =>
        new(value, None, None, Some(field.Fit), [], [], new RowHelp(Some((text.Shown(Some(bounds.Low)), text.Shown(Some(bounds.High)))), Some(text.Shown(Some(rest)))),
            edit.Shown, Some<RowEdit>(edit), Some(ChoiceRows.Context(value, [], [])));

    private static IO<NumberText<Length>> Measure(RhinoDoc doc, (Length Low, Length High) soft, bool modelUnits) =>
        DistanceDisplay.Read(doc, modelUnits).Map<NumberText<Length>>(display => new DistanceText(modelUnits ? doc.ModelUnits : doc.PageUnits, display, soft, RowEdit.Varies));

    // --- [VISITORS]
    private sealed class Grounding<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Seq<Node> ground, RowRules rules)
        : IStateParameterVisitor<TRecord, Option<ControlRow>> where TRecord : notnull {
        public Option<ControlRow> Numeric<TValue, TKey>(Lens<TRecord, TValue> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            Sliders.Numeric(source, parameter, lens, domain, rules, ground);

        public Option<ControlRow> OptionalNumeric<TValue, TKey>(Lens<TRecord, Gated<TValue>> lens, NumericDomain<TValue, TKey> domain)
            where TValue : notnull
            where TKey : struct, INumber<TKey> =>
            Sliders.OptionalNumeric(source, parameter, lens, domain, rules, ground);

        public Option<ControlRow> Choice<TValue>(Lens<TRecord, TValue> lens) where TValue : ISmartEnum<string, TValue, ValidationError> => None;

        public Option<ControlRow> OptionalChoice<TValue>(Lens<TRecord, Option<TValue>> lens, string absent) where TValue : ISmartEnum<string, TValue, ValidationError> => None;

        public Option<ControlRow> Variant<TValue>(Lens<TRecord, TValue> lens) where TValue : class => None;

        public Option<ControlRow> Enumerated<TEnum>(Lens<TRecord, TEnum> lens) where TEnum : struct, Enum => None;

        public Option<ControlRow> Record<TNested>(Lens<TRecord, TNested> lens) where TNested : IStateRecord<TNested> => None;

        public Option<ControlRow> OptionalRecord<TNested>(Lens<TRecord, Gated<TNested>> lens) where TNested : IStateRecord<TNested> => None;

        public Option<ControlRow> Toggle(Lens<TRecord, bool> lens) => None;

        public Option<ControlRow> Raw<TRaw>(Lens<TRecord, TRaw> lens) where TRaw : notnull, ISpanParsable<TRaw> => None;

        public Option<ControlRow> OptionalRaw<TRaw>(Lens<TRecord, Option<TRaw>> lens) where TRaw : notnull, ISpanParsable<TRaw> => None;

        public Option<ControlRow> Color(Lens<TRecord, SKColorF> lens) => None;

        public Option<ControlRow> OptionalColor(Lens<TRecord, Gated<SKColorF>> lens) => None;

        public Option<ControlRow> Gradient(Lens<TRecord, ColorRamp> lens) => None;

        public Option<ControlRow> Colors<TValue>(Lens<TRecord, TValue> lens) where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> => None;

        public Option<ControlRow> Keyed<TValue, TRaw>(Lens<TRecord, TValue> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            None;

        public Option<ControlRow> OptionalKeyed<TValue, TRaw>(Lens<TRecord, Option<TValue>> lens)
            where TValue : IObjectFactory<TValue, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            None;

        public Option<ControlRow> Loaded<TValue, TKey, TRaw>(Lens<TRecord, Option<TValue>> lens, Func<TKey, IO<TValue>> load, Func<TValue, TKey> key)
            where TKey : IObjectFactory<TKey, TRaw, ValidationError>, IConvertible<TRaw>
            where TRaw : notnull, ISpanParsable<TRaw> =>
            None;

        public Option<ControlRow> Opaque<TValue>(Lens<TRecord, TValue> lens) where TValue : notnull => None;
    }
}
