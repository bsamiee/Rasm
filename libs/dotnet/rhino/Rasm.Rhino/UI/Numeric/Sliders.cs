using Eto.Forms;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.UI;

namespace Rasm.Rhino.UI.Numeric;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SliderRow(double Value, NumberRange Range, Func<Edit<double>, IO<Unit>> Edited) : IRow<SliderRow> {
    private const int Ticks = 1000;

    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<SliderRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from following in IO.lift(static () => Ref(Option<Func<Option<double>, IO<Unit>>>.None))
        from field in NumberFields.Scalar(first.Range, edit =>
            from sent in latest.Bind(current => current.Edited(edit))
            from track in following.ValueIO
            from followed in track.Traverse(show => show(edit.Held)).As()
            select unit)
        from track in use(static () => new Slider { MinValue = 0, MaxValue = Ticks, SnapToTick = false })
        from moved in Values.Bound(track, Ticked(track, first.Range), static _ => true, Some(Released(track)), Some(first.Value), edit =>
            from shown in edit.Held.Traverse(field.Show).As()
            from sent in latest.Bind(current => current.Edited(edit))
            select unit)
        from kept in atomicIO(() => following.Value = Some(moved))
        from line in RowTable.Lined([new TableCell(track, scaleWidth: true), new TableCell(field.Control)])
        select new RowCells(line,
            from current in latest
            from shown in field.Show(current.Value)
            from placed in moved(Some(current.Value))
            select unit);

    private static BindableBinding<Slider, Option<double>> Ticked(Slider track, NumberRange range) =>
        new(track,
            held => Some(range.Scale.Value(range, (double)held.Value / Ticks) switch { var raw => range.Step.IsSome ? range.Snapped(raw) : range.Clamped(raw) }),
            (held, value) => value.Iter(number => held.Value = (int)double.Round(range.Scale.Fraction(range, range.Clamped(number)) * Ticks, MidpointRounding.AwayFromZero)),
            static (held, handler) => held.ValueChanged += handler,
            static (held, handler) => held.ValueChanged -= handler);

    private static HostEvent<Unit> Released(Slider track) =>
        Subscriptions.Merged(
            Subscriptions.Inline<MouseEventArgs>(h => track.MouseUp += h, h => track.MouseUp -= h).Choose(static _ => Some(unit)),
            Subscriptions.Inline<KeyEventArgs>(h => track.KeyUp += h, h => track.KeyUp -= h).Choose(static _ => Some(unit)));
}
