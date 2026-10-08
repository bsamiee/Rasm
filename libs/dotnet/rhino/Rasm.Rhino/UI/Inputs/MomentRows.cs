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
[SmartEnum<DateTimePickerMode>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class MomentPart {
    private static readonly Lens<LocalDateTime, LocalDate> DateOf =
        Lens<LocalDateTime, LocalDate>.New(static at => at.Date, static date => at => date.At(at.TimeOfDay));

    private static readonly Lens<LocalDateTime, LocalTime> TimeOf =
        Lens<LocalDateTime, LocalTime>.New(static at => at.TimeOfDay, static time => at => at.Date.At(time));

    public static readonly MomentPart Date = new(DateTimePickerMode.Date, static (held, picked) => DateOf.Set(DateOf.Get(picked), held));
    public static readonly MomentPart Time = new(DateTimePickerMode.Time, static (held, picked) => TimeOf.Set(TimeOf.Get(picked), held));
    public static readonly MomentPart Whole = new(DateTimePickerMode.DateTime, static (_, picked) => picked);

    [UseDelegateFromConstructor]
    public partial LocalDateTime Merge(LocalDateTime held, LocalDateTime picked);

    public static bool operator <(MomentPart left, MomentPart right) => Comparer<MomentPart>.Default.Compare(left, right) < 0;
    public static bool operator <=(MomentPart left, MomentPart right) => Comparer<MomentPart>.Default.Compare(left, right) <= 0;
    public static bool operator >(MomentPart left, MomentPart right) => Comparer<MomentPart>.Default.Compare(left, right) > 0;
    public static bool operator >=(MomentPart left, MomentPart right) => Comparer<MomentPart>.Default.Compare(left, right) >= 0;
}

[SmartEnum<int>]
[ValidationError<InvalidRhinoValue>]
public sealed partial class ClockFormat {
    public static readonly ClockFormat HourMinute12 = new(0, "h:mm tt", static () => HourMinuteSecond12);
    public static readonly ClockFormat HourMinuteSecond12 = new(1, "h:mm:ss tt", static () => HourMinute24);
    public static readonly ClockFormat HourMinute24 = new(2, "HH:mm", static () => HourMinuteSecond24);
    public static readonly ClockFormat HourMinuteSecond24 = new(3, "HH:mm:ss", static () => HourMinute12);

    [IgnoreMember]
    public static readonly ClockFormat Default = HourMinute24;

    public string Pattern { get; }

    [UseDelegateFromConstructor]
    public partial ClockFormat Next();
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
            IO.lift(() => Conversions.Validated<ClockFormat, int, InvalidRhinoValue>(_read()).Map(static held => Some(held).Filter(static format => format != ClockFormat.Default))),
            held => IO.lift(() => _write(held.IfNone(ClockFormat.Default).Key)),
            Applied.Live,
            None);

    [UseDelegateFromConstructor]
    public partial LocalTime TimeOf(OffsetDateTime at);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MomentRows {
    // --- [PICKERS]
    public static ControlRow Picker<TValue, TError>(RowSource<Option<TValue>> source, RowField<Option<TValue>> field, MomentPart part, RowRules rules)
        where TValue : IObjectFactory<TValue, LocalDateTime, TError>, IConvertible<LocalDateTime>, System.Numerics.IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Inline, scope => Picked<TValue, TError>(source, field, part, scope), rules);

    // --- [RANGES]
    public static ControlRow DateSpan<TRecord, TBound>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, DateInterval> read, Func<TRecord, DateInterval, Fin<TRecord>> write, RowRules rules)
        where TRecord : notnull
        where TBound : System.Numerics.IMinMaxValue<TBound>, IConvertible<LocalDateTime> =>
        ControlRow.Of(source, field, RowShape.Inline, scope => Ranged(source, field, read, write, (Min: TBound.MinValue.ToValue().Date, Max: TBound.MaxValue.ToValue().Date), scope), rules);

    // --- [CLOCKS]
    public static ControlRow Clock(string caption, string help, ClockZone zone, ReadoutSource<OffsetDateTime> moment, RowRules rules) =>
        new ControlRow.Readout(RowShape.Unlabeled, scope =>
            from command in IO.lift(static () => new Command {
                ToolBarText = RowText.Localize("Time format").Local,
                ToolTip = RowText.Localize("Steps the time format Rhino's Sun panel shares").Local,
            })
            let site = new CallbackSite(scope.Sink, typeof(Command), nameof(Command.Executed))
            from button in ButtonRows.Push(command)
            from live in TextRows.Live(moment, _ => IO.pure(button), (held, at) => Shown(held, zone, at), scope)
            from executed in DisposalOps.OnFailure(
                Subscriptions.Attach(h => command.Executed += h, h => command.Executed -= h, Callbacks.Handler<EventArgs>(_ => Cycled(zone.Format), site)),
                IO.lift(live.Release.Dispose))
            select new RowCells(live.Control, None, None, None, [], [], new RowHelp(None, None), IO.pure(unit), None, None, DisposalOps.Composite(Seq(live.Release, executed), site)),
            caption, help, rules);

    // --- [REALIZE]
    private static IO<RowCells> Picked<TValue, TError>(RowSource<Option<TValue>> source, RowField<Option<TValue>> field, MomentPart part, RowScope scope)
        where TValue : IObjectFactory<TValue, LocalDateTime, TError>, IConvertible<LocalDateTime>, System.Numerics.IMinMaxValue<TValue>
        where TError : Error, IValidationError<TError> =>
        from picker in IO.lift(() => new DateTimePicker {
            Mode = part.Key, MinDate = TValue.MinValue.ToValue().ToDateTimeUnspecified(), MaxDate = TValue.MaxValue.ToValue().ToDateTimeUnspecified(),
        })
        let site = new CallbackSite(scope.Sink, typeof(DateTimePicker), nameof(DateTimePicker.ValueChanged))
        let pattern = LocalDateTimePattern.Create("G", RowText.Culture)
        let merged = fun((Option<TValue> held, LocalDateTime wall) =>
            Conversions.Validated<TValue, LocalDateTime, TError>(held.Map(value => part.Merge(value.ToValue(), wall)).IfNone(wall)).Map(static value => Some(value)))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => picker.ValueChanged += h, h => picker.ValueChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ =>
                from held in scope.Read(source)
                from answered in Optional(picker.Value).Match(
                    Some: picked => edit.Preview(merged(held, LocalDateTime.FromDateTime(picked)), Some(Accessors.StepEventsDelay)),
                    None: () => edit.Commit(Fin.Succ(Option<TValue>.None)))
                select answered, site),
            held => IO.lift(() => {
                picker.Value = held.Bind(static value => value).Map(static value => value.ToValue().ToDateTimeUnspecified()).ToNullable();
            }),
            scope, site)
        select new RowCells(
            picker, None, None, None, [], [], new RowHelp(Some((pattern.Format(TValue.MinValue.ToValue()), pattern.Format(TValue.MaxValue.ToValue()))), None),
            bound.Edit.Shown, Some<RowEdit>(bound.Edit), Some(ChoiceRows.Context(picker, [], [], scope.Sink)), bound.Release);

    private static IO<RowCells> Ranged<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Func<TRecord, DateInterval> read, Func<TRecord, DateInterval, Fin<TRecord>> write,
        (LocalDate Min, LocalDate Max) limits, RowScope scope)
        where TRecord : notnull =>
        from start in IO.lift(static () => new DateTimePicker { Mode = DateTimePickerMode.Date })
        from end in IO.lift(static () => new DateTimePicker { Mode = DateTimePickerMode.Date })
        from table in IO.lift(() => new TableLayout(new TableRow(new TableCell(start, scaleWidth: true), new TableCell(end, scaleWidth: true))) {
            Spacing = new Size(RhinoLayout.StackedSpacing(Orientation.Horizontal, RhinoLayout.SpacingType.Table), 0),
        })
        let walls = (Min: limits.Min.AtMidnight().ToDateTimeUnspecified(), Max: limits.Max.AtMidnight().ToDateTimeUnspecified())
        let site = new CallbackSite(scope.Sink, typeof(DateTimePicker), nameof(DateTimePicker.ValueChanged))
        let pattern = LocalDatePattern.Create("d", RowText.Culture)
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => {
                start.ValueChanged += h;
                end.ValueChanged += h;
            },
            h => {
                start.ValueChanged -= h;
                end.ValueChanged -= h;
            },
            edit => Callbacks.Handler<EventArgs>(_ =>
                from narrowed in IO.lift(() => Narrowed(start, end, walls))
                from held in scope.Read(source)
                from routed in (Optional(start.Value), Optional(end.Value))
                    .Apply(static (first, last) => new DateInterval(LocalDate.FromDateTime(first), LocalDate.FromDateTime(last))).As()
                    .Match(Some: picked => edit.Preview(write(held, picked), Some(Accessors.StepEventsDelay)), None: () => edit.Cancel)
                select routed, site),
            held => IO.lift(() => Spanned(start, end, walls, held.Map(read))),
            scope, site)
        select new RowCells(
            table, None, None, None, [], [],
            new RowHelp(
                Some((pattern.Format(limits.Min), pattern.Format(limits.Max))),
                Some(read(source.Default)).Map(dates => RowText.Localize("{0} – {1}", arguments: [pattern.Format(dates.Start), pattern.Format(dates.End)]).Local)),
            bound.Edit.Shown, Some<RowEdit>(bound.Edit), Some(ChoiceRows.Labelled([], [], scope.Sink)), bound.Release);

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
        format.Read.Bind(held => format.Write(Some(held.IfNone(ClockFormat.Default).Next()))).Map(static _ => unit);
}
