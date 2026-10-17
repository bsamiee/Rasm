using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Rows;
using Rhino.Render;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ClockFormat {
    public static readonly ClockFormat HourMinute12 = new(0, "h:mm tt");
    public static readonly ClockFormat HourMinuteSecond12 = new(1, "h:mm:ss tt");
    public static readonly ClockFormat HourMinute24 = new(2, "HH:mm");
    public static readonly ClockFormat HourMinuteSecond24 = new(3, "HH:mm:ss");

    [IgnoreMember]
    public static readonly ClockFormat Default = HourMinute24;

    public string Pattern { get; }

    public ClockFormat Next() => Get((Key + 1) % Items.Count);
}

[SmartEnum]
public sealed partial class ClockZone {
    public static readonly ClockZone Local = new("Local: {0}", SupportOptions.LabelFormatLoc, SupportOptions.SetLabelFormatLoc, static at => at.TimeOfDay);
    public static readonly ClockZone Utc = new("UTC: {0}", SupportOptions.LabelFormatUtc, SupportOptions.SetLabelFormatUtc, static at => at.ToInstant().InUtc().TimeOfDay);

    public string Template { get; }

    private readonly Func<int> _read;
    private readonly Action<int> _write;

    public ValueStore<ClockFormat> Format =>
        ValueStore.Of(
            IO.lift(() => ClockFormat.Validated(_read()).Map(static held => Some(held).Filter(static format => format != ClockFormat.Default))),
            held => IO.lift(() => _write(held.IfNone(ClockFormat.Default).Key)),
            Applied.Live,
            None);

    [UseDelegateFromConstructor]
    public partial LocalTime TimeOf(OffsetDateTime at);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MomentRows {
    // --- [PICKERS]
    public static ControlRow Picker<TValue>(RowSource<Option<TValue>> source, RowField<Option<TValue>> field, DateTimePickerMode part, RowRules rules)
        where TValue : IObjectFactory<TValue, LocalDateTime, ValidationError>, IConvertible<LocalDateTime>, System.Numerics.IMinMaxValue<TValue> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Picked(source, field, part, scope, retain), rules);

    // --- [RANGES]
    public static ControlRow DateSpan<TRecord, TBound>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, DateInterval> read, Func<TRecord, DateInterval, Fin<TRecord>> write, RowRules rules)
        where TRecord : notnull
        where TBound : System.Numerics.IMinMaxValue<TBound>, IConvertible<LocalDateTime> =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Ranged(source, field, read, write, (Min: TBound.MinValue.ToValue().Date, Max: TBound.MaxValue.ToValue().Date), scope, retain), rules);

    // --- [CLOCKS]
    public static ControlRow Clock(string caption, string help, ClockZone zone, ReadoutSource<OffsetDateTime> moment, RowRules rules) =>
        new ControlRow.Readout(RowShape.Unlabeled, (scope, retain) =>
            DisposalOps.Acquire(owned =>
            from command in IO.lift(static () => new Command())
            from faced in IO.lift(() => {
                (command.ToolBarText, command.ToolTip) = (RowText.Localize("Time format").Local, RowText.Localize("Steps the time format Rhino's Sun panel shares").Local);
            })
            from live in TextRows.Live(moment, (_, keep) => ButtonRows.Push(command, keep), (held, at) => Shown(held, zone, at), scope, owned)
            from executed in Subscriptions.Attach(h => command.Executed += h, h => command.Executed -= h,
                Callbacks.Handler<EventArgs>(_ => Cycled(zone.Format), new CallbackSite(scope.Sink, typeof(Command), nameof(Command.Executed))), owned)
            select new RowCells(live, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None), retain),
            caption, help, rules);

    // --- [REALIZE]
    private static IO<RowCells> Picked<TValue>(RowSource<Option<TValue>> source, RowField<Option<TValue>> field, DateTimePickerMode part, RowScope scope, Action<IDisposable> retain)
        where TValue : IObjectFactory<TValue, LocalDateTime, ValidationError>, IConvertible<LocalDateTime>, System.Numerics.IMinMaxValue<TValue> =>
        DisposalOps.Acquire(owned =>
        from picker in DisposalOps.Acquire(IO.lift(static () => new DateTimePicker()), owned)
        from ranged in IO.lift(() => { (picker.Mode, picker.MinDate, picker.MaxDate) = (part, TValue.MinValue.ToValue().ToDateTimeUnspecified(), TValue.MaxValue.ToValue().ToDateTimeUnspecified()); })
        let site = new CallbackSite(scope.Sink, typeof(DateTimePicker), nameof(DateTimePicker.ValueChanged))
        let pattern = LocalDateTimePattern.Create("G", RowText.Culture)
        let merged = fun((Option<TValue> held, LocalDateTime wall) =>
            ValidationResults.Validated<TValue, LocalDateTime>(held.Map(value => part switch {
                DateTimePickerMode.Date => wall.Date.At(value.ToValue().TimeOfDay),
                DateTimePickerMode.Time => value.ToValue().Date.At(wall.TimeOfDay),
                DateTimePickerMode.DateTime => wall,
                _ => throw new UnreachableException(nameof(DateTimePickerMode)),
            }).IfNone(wall)).Map(static value => Some(value)))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => picker.ValueChanged += h, h => picker.ValueChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ => Answered(edit, source, picker, merged, scope), site),
            held => IO.lift(() => {
                picker.Value = held.Flatten().Map(static value => value.ToValue().ToDateTimeUnspecified()).ToNullable();
            }),
            scope, site, owned)
        select new RowCells(
            picker, None, None, None, [], [], new RowHelp(Some((pattern.Format(TValue.MinValue.ToValue()), pattern.Format(TValue.MaxValue.ToValue()))), None),
            bound.Shown, Some<RowEdit>(bound), Some(ChoiceRows.Context(picker, [], [], scope.Sink))), retain);

    private static IO<Unit> Answered<TValue>(
        RowEdit<Option<TValue>> edit, RowSource<Option<TValue>> source, DateTimePicker picker, Func<Option<TValue>, LocalDateTime, Fin<Option<TValue>>> merged, RowScope scope) =>
        from held in scope.Read(source)
        from answered in Optional(picker.Value).Match(
            Some: picked => edit.Preview(merged(held, LocalDateTime.FromDateTime(picked)), Some(Accessors.StepEventsDelay)),
            None: () => edit.Commit(Fin.Succ(Option<TValue>.None)).Map(unit))
        select answered;

    private static IO<RowCells> Ranged<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, DateInterval> read, Func<TRecord, DateInterval, Fin<TRecord>> write,
        (LocalDate Min, LocalDate Max) limits, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        DisposalOps.Acquire(owned =>
        from start in DisposalOps.Acquire(IO.lift(static () => new DateTimePicker()), owned)
        from end in DisposalOps.Acquire(IO.lift(static () => new DateTimePicker()), owned)
        from moded in IO.lift(() => { (start.Mode, end.Mode) = (DateTimePickerMode.Date, DateTimePickerMode.Date); })
        from table in DisposalOps.Acquire(IO.lift(static () => new TableLayout()), owned)
        from placed in IO.lift(() => {
            table.Rows.Add(new TableRow(new TableCell(start, scaleWidth: true), new TableCell(end, scaleWidth: true)));
            table.Spacing = new Size(RhinoLayout.StackedSpacing(Orientation.Horizontal, RhinoLayout.SpacingType.Table), 0);
        })
        let walls = (Min: limits.Min.AtMidnight().ToDateTimeUnspecified(), Max: limits.Max.AtMidnight().ToDateTimeUnspecified())
        let site = new CallbackSite(scope.Sink, typeof(DateTimePicker), nameof(DateTimePicker.ValueChanged))
        let pattern = LocalDatePattern.Create("d", RowText.Culture)
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => Callbacks.Each(Seq(IO.lift(() => start.ValueChanged += h), IO.lift(() => end.ValueChanged += h))).Run(),
            h => Callbacks.Each(Seq(IO.lift(() => start.ValueChanged -= h), IO.lift(() => end.ValueChanged -= h))).Run(),
            edit => Callbacks.Handler<EventArgs>(_ => Routed(edit, source, write, start, end, walls, scope), site),
            held => IO.lift(() => Spanned(start, end, walls, held.Map(read))),
            scope, site, owned)
        select new RowCells(
            table, None, None, None, [], [],
            new RowHelp(
                Some((pattern.Format(limits.Min), pattern.Format(limits.Max))),
                Some(read(source.Default)).Map(dates => RowText.Localize("{0} – {1}", arguments: [pattern.Format(dates.Start), pattern.Format(dates.End)]).Local)),
            bound.Shown, Some<RowEdit>(bound), Some(ChoiceRows.Labelled([], [], scope.Sink))), retain);

    private static IO<Unit> Routed<TRecord>(
        RowEdit<TRecord> edit, RowSource<TRecord> source, Func<TRecord, DateInterval, Fin<TRecord>> write, DateTimePicker start, DateTimePicker end,
        (DateTime Min, DateTime Max) walls, RowScope scope)
        where TRecord : notnull =>
        from narrowed in IO.lift(() => Narrowed(start, end, walls))
        from held in scope.Read(source)
        from routed in (Optional(start.Value), Optional(end.Value))
            .Apply(static (first, last) => new DateInterval(LocalDate.FromDateTime(first), LocalDate.FromDateTime(last))).As()
            .Match(Some: picked => edit.Preview(write(held, picked), Some(Accessors.StepEventsDelay)), None: () => edit.Cancel)
        select routed;

    private static void Narrowed(DateTimePicker start, DateTimePicker end, (DateTime Min, DateTime Max) walls) =>
        (start.MaxDate, end.MinDate) = (end.Value ?? walls.Max, start.Value ?? walls.Min);

    private static void Spanned(DateTimePicker start, DateTimePicker end, (DateTime Min, DateTime Max) walls, Option<DateInterval> held) {
        (start.MinDate, start.MaxDate, end.MinDate, end.MaxDate) = (walls.Min, walls.Max, walls.Min, walls.Max);
        (start.Value, end.Value) = (
            held.Map(static dates => dates.Start.AtMidnight().ToDateTimeUnspecified()).ToNullable(),
            held.Map(static dates => dates.End.AtMidnight().ToDateTimeUnspecified()).ToNullable());
        Narrowed(start, end, walls);
    }

    private static IO<Unit> Shown(Button button, ClockZone zone, OffsetDateTime at) =>
        from format in zone.Format.Read
        let text = RowText.Localize(zone.Template, arguments: [LocalTimePattern.Create(format.IfNone(ClockFormat.Default).Pattern, RowText.Culture).Format(zone.TimeOf(at))]).Local
        from written in when(!string.Equals(button.Text, text, StringComparison.Ordinal), IO.lift(() => { button.Text = text; })).As()
        select written;

    private static IO<Unit> Cycled(ValueStore<ClockFormat> format) =>
        format.Read.Bind(held => format.Write(Some(held.IfNone(ClockFormat.Default).Next()))).Map(unit);
}
