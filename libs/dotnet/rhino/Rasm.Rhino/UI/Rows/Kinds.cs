using Eto.Forms;
using Rasm.Rhino.UI.Styling;
using Rhino.UI;

namespace Rasm.Rhino.UI.Rows;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IRow<TSelf> where TSelf : IRow<TSelf> {
    public RowShape Shape { get; }

    public static abstract IO<RowCells> Realize(IO<TSelf> latest, LocalizeStringPair label, RowScope scope);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RowShape {
    public static readonly RowShape Inline = new(labeled: true, spans: false, fills: false);
    public static readonly RowShape Unlabeled = new(labeled: false, spans: false, fills: false);
    public static readonly RowShape Block = new(labeled: true, spans: true, fills: false);
    public static readonly RowShape Filled = new(labeled: true, spans: true, fills: true);
    public static readonly RowShape Nested = new(labeled: false, spans: true, fills: false);

    public bool Labeled { get; }
    public bool Spans { get; }
    public bool Fills { get; }
}

public sealed record RowLine(Option<string> Caption, Control Value);

public sealed record RowCells(Control Value, IO<Unit> Show) {
    public Option<Control> Pick { get; init; }
    public Seq<RowLine> Lines { get; init; }
    public Seq<Seq<Command<Invoke>>> Menu { get; init; }
}

public sealed record Row {
    private Row(string label, RowShape shape, object kind, Func<LocalizeStringPair, RowScope, IO<(RowCells Cells, Func<Row, IO<bool>> Restated)>> seated) =>
        (Label, Shape, Kind, Seated) = (label, shape, kind, seated);

    public string Label { get; }
    public RowShape Shape { get; }
    public Option<string> Help { get; init; }
    public Option<RowRule> Visible { get; init; }
    public Option<RowRule> Enabled { get; init; }
    public bool Modified { get; init; }

    internal object Kind { get; }
    internal Func<LocalizeStringPair, RowScope, IO<(RowCells Cells, Func<Row, IO<bool>> Restated)>> Seated { get; }

    public static Row Of<TKind>(string label, TKind kind) where TKind : IRow<TKind> =>
        new(label, kind.Shape, kind, (caption, scope) =>
            from latest in atomIO(kind)
            from cells in TKind.Realize(latest.ValueIO, caption, scope)
            from shown in cells.Show
            select (cells, (Func<Row, IO<bool>>)(next => next.Kind is TKind held
                ? writeIO(latest, held).Bind(_ => cells.Show).Map(static _ => true)
                : IO.pure(value: false))));
}

public sealed record GroupRow(Seq<Row> Rows) : IRow<GroupRow> {
    public bool Expanded { get; init; } = true;

    public RowShape Shape { get; init; } = RowShape.Nested;

    public static IO<RowCells> Realize(IO<GroupRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from stored in IO.lift(() => scope.Settings.Map(static node => node.Child("Groups")))
        from settings in stored.Traverse(static node => node.Created).As()
        from table in RowTable.Made(scope)
        from header in use(() => new Label { Text = label.Local, VerticalAlignment = VerticalAlignment.Center })
        from expander in use(
            () => new Expander {
                Header = header,
                Content = table.View,
                Expanded = settings.Bind(held => Conversions.Present(held.TryGetBool(label.English, out bool expanded), expanded)).IfNone(first.Expanded),
            },
            static held => {
                (held.Content, held.Header) = (null, null);
                held.Dispose();
            })
        from persisted in settings.Traverse(held => Subscriptions.Inline<EventArgs>(h => expander.ExpandedChanged += h, h => expander.ExpandedChanged -= h)
            .Attach(_ => IO.lift(() => held.SetBool(label.English, expander.Expanded)))).As()
        select new RowCells(expander, latest.Bind(current => table.Show(current.Rows)));
}

public sealed record CommandRow(Command<Invoke> Command) : IRow<CommandRow> {
    public RowShape Shape { get; init; } = RowShape.Unlabeled;

    public static IO<RowCells> Realize(IO<CommandRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from button in CommandRows.Button(first.Command, IconSlot.PanelButton, scope)
        select new RowCells(button.Control, button.Refresh);
}
