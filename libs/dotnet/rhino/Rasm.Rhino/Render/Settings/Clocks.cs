using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.Render;
using Rhino.UI;

namespace Rasm.Rhino.Render.Settings;

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
    public static readonly ClockZone Local = new("Local: {0}", Preferences.Property(static () => ClockFormat.Get(SupportOptions.LabelFormatLoc()), static format => SupportOptions.SetLabelFormatLoc(format.Key)),
        static at => at.TimeOfDay);

    public static readonly ClockZone Utc = new("UTC: {0}", Preferences.Property(static () => ClockFormat.Get(SupportOptions.LabelFormatUtc()), static format => SupportOptions.SetLabelFormatUtc(format.Key)),
        static at => at.ToInstant().InUtc().TimeOfDay);

    public string Template { get; }
    public Preference<ClockFormat> Format { get; }

    [UseDelegateFromConstructor]
    public partial LocalTime TimeOf(OffsetDateTime at);
}

public sealed record ClockRow(ClockZone Zone, ReadoutSource<OffsetDateTime> Moment) : IRow<ClockRow> {
    public RowShape Shape { get; init; } = RowShape.Unlabeled;

    public static IO<RowCells> Realize(IO<ClockRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from live in first.Moment.Live(static _ => use(static () => new Button()), (button, at) => Shown(button, first.Zone, at), scope)
        from clicked in Subscriptions.On<EventArgs>(h => live.Click += h, h => live.Click -= h).Attach(_ => first.Zone.Format.Read.Bind(format => first.Zone.Format.Write(format.Next())))
        select new RowCells(live, IO.pure(unit));

    private static IO<Unit> Shown(Button button, ClockZone zone, OffsetDateTime at) =>
        from format in zone.Format.Read
        let text = LocalText.Of(zone.Template, LocalTimePattern.Create(format.Pattern, LocalText.Culture).Format(zone.TimeOf(at))).Local
        from written in when(!string.Equals(button.Text, text, StringComparison.Ordinal), IO.lift(() => { button.Text = text; })).As()
        select written;
}
