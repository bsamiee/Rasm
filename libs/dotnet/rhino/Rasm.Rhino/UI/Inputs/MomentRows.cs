using Eto.Drawing;
using Eto.Forms;
using NodaTime.Text;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.UI;
using Rhino.Render;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class ClockFormat {
    public static readonly ClockFormat HourMinute12 = new(0, "h:mm tt");
    public static readonly ClockFormat HourMinuteSecond12 = new(1, "h:mm:ss tt");
    public static readonly ClockFormat HourMinute24 = new(2, "HH:mm");
    public static readonly ClockFormat HourMinuteSecond24 = new(3, "HH:mm:ss");

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
        ValueStores.Of(
            IO.lift(() => ClockFormat.TryGet(_read(), out ClockFormat? held) ? Some(held).Filter(static format => format != ClockFormat.HourMinute24) : None),
            held => IO.lift(() => _write(held.IfNone(ClockFormat.HourMinute24).Key)),
            Applied.Live,
            None);

    [UseDelegateFromConstructor]
    public partial LocalTime TimeOf(OffsetDateTime at);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MomentRows {
    // --- [PICKERS]
    public static ControlRow Picker(string label, LocalDateTime value, DateTimePickerMode part, (LocalDateTime Min, LocalDateTime Max) limits, Func<Edit<LocalDateTime>, IO<Unit>> edited) =>
        new ControlRow.Bound<(LocalDateTime Value, DateTimePickerMode Part, (LocalDateTime Min, LocalDateTime Max) Limits, Func<Edit<LocalDateTime>, IO<Unit>> Edited)>(
            label, RowShape.Inline, (value, part, limits, edited), static (latest, scope) =>
                from picker in use(static () => new DateTimePicker())
                from change in ControlChange<EventArgs>.Attached(
                    Subscriptions.Host<EventArgs>(h => picker.ValueChanged += h, h => picker.ValueChanged -= h),
                    _ =>
                        from current in latest
                        from answered in Optional(picker.Value).Map(static picked => LocalDateTime.FromDateTime(picked)).Traverse(wall => current.Row.Edited(Edit.Commit(current.Row.Part switch {
                            DateTimePickerMode.Date => wall.Date.At(current.Row.Value.TimeOfDay),
                            DateTimePickerMode.Time => current.Row.Value.Date.At(wall.TimeOfDay),
                            _ => wall,
                        }))).As()
                        select unit,
                    scope.Owner)
                select new RowCells(picker, latest.Bind(current => change.Detached(IO.lift(() => {
                    (picker.Mode, picker.MinDate, picker.MaxDate) = (current.Row.Part, current.Row.Limits.Min.ToDateTimeUnspecified(), current.Row.Limits.Max.ToDateTimeUnspecified());
                    picker.Value = current.Row.Value.ToDateTimeUnspecified();
                })))));

    // --- [RANGES]
    public static ControlRow DateSpan(string label, DateInterval value, (LocalDate Min, LocalDate Max) limits, Func<Edit<DateInterval>, IO<Unit>> edited) =>
        new ControlRow.Bound<(DateInterval Value, (LocalDate Min, LocalDate Max) Limits, Func<Edit<DateInterval>, IO<Unit>> Edited)>(
            label, RowShape.Inline, (value, limits, edited), static (latest, scope) =>
                from start in use(static () => new DateTimePicker { Mode = DateTimePickerMode.Date })
                from end in use(static () => new DateTimePicker { Mode = DateTimePickerMode.Date })
                from table in Containers.Frame(static () => new TableLayout())
                from placed in IO.lift(() => {
                    table.Rows.Add(new TableRow(new TableCell(start, scaleWidth: true), new TableCell(end, scaleWidth: true)));
                    table.Spacing = new Size(RhinoLayout.StackedSpacing(Orientation.Horizontal, RhinoLayout.SpacingType.Table), 0);
                })
                let walls = latest.Map(static current => (Min: current.Row.Limits.Min.AtMidnight().ToDateTimeUnspecified(), Max: current.Row.Limits.Max.AtMidnight().ToDateTimeUnspecified()))
                from change in ControlChange<EventArgs>.Attached(
                    Subscriptions.Merged(
                        Subscriptions.Host<EventArgs>(h => start.ValueChanged += h, h => start.ValueChanged -= h),
                        Subscriptions.Host<EventArgs>(h => end.ValueChanged += h, h => end.ValueChanged -= h)),
                    _ =>
                        from bounds in walls
                        from narrowed in IO.lift(() => Narrowed(start, end, bounds))
                        from current in latest
                        from routed in (Optional(start.Value), Optional(end.Value))
                            .Apply(static (first, last) => new DateInterval(LocalDate.FromDateTime(first), LocalDate.FromDateTime(last))).As()
                            .Traverse(picked => current.Row.Edited(Edit.Commit(picked))).As()
                        select unit,
                    scope.Owner)
                select new RowCells(table, from current in latest
                                           from bounds in walls
                                           from shown in change.Detached(IO.lift(() => Spanned(start, end, bounds, current.Row.Value)))
                                           select shown));

    // --- [CLOCKS]
    public static ControlRow Clock(string label, ClockZone zone, ReadoutSource<OffsetDateTime> moment) =>
        new ControlRow.Bound<(ClockZone Zone, ReadoutSource<OffsetDateTime> Moment)>(label, RowShape.Unlabeled, (zone, moment), static (latest, scope) =>
            from row in latest
            from live in TextRows.Live(row.Row.Moment, static _ => use(static () => new Button()), (held, at) => Shown(held, row.Row.Zone, at), scope)
            from clicked in Subscriptions.Host<EventArgs>(h => live.Click += h, h => live.Click -= h).Inline(_ => Cycled(row.Row.Zone.Format), scope.Owner)
            select new RowCells(live, IO.pure(unit)));

    // --- [REALIZE]
    private static Unit Narrowed(DateTimePicker start, DateTimePicker end, (DateTime Min, DateTime Max) walls) {
        (start.MaxDate, end.MinDate) = (end.Value ?? walls.Max, start.Value ?? walls.Min);
        return unit;
    }

    private static Unit Spanned(DateTimePicker start, DateTimePicker end, (DateTime Min, DateTime Max) walls, DateInterval held) {
        (start.MinDate, start.MaxDate, end.MinDate, end.MaxDate) = (walls.Min, walls.Max, walls.Min, walls.Max);
        (start.Value, end.Value) = (held.Start.AtMidnight().ToDateTimeUnspecified(), held.End.AtMidnight().ToDateTimeUnspecified());
        return Narrowed(start, end, walls);
    }

    private static IO<Unit> Shown(Button button, ClockZone zone, OffsetDateTime at) =>
        from format in zone.Format.Read
        let text = RowText.Localize(zone.Template, arguments: [LocalTimePattern.Create(format.IfNone(ClockFormat.HourMinute24).Pattern, RowText.Culture).Format(zone.TimeOf(at))]).Local
        from written in when(!string.Equals(button.Text, text, StringComparison.Ordinal), IO.lift(() => { button.Text = text; })).As()
        select written;

    private static IO<Unit> Cycled(ValueStore<ClockFormat> format) =>
        format.Read.Bind(held => format.Write(Some(held.IfNone(ClockFormat.HourMinute24).Next()))).Map(unit);
}
