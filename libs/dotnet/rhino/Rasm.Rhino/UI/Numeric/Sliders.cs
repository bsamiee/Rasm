using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Rhino.UI.Numeric;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sliders {
    // --- [ROWS]
    public static ControlRow Numeric(string label, double value, NumberRange range, Func<Edit<double>, IO<Unit>> edited) =>
        new ControlRow.Bound<(double Value, NumberRange Range, Func<Edit<double>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, range, edited), static (latest, scope) =>
            from first in latest
            from field in NumberFields.Scalar(first.Row.Range, edit => latest.Bind(current => current.Row.Edited(edit)), scope.Owner)
            from track in use(static () => new Slider { MinValue = 0, SnapToTick = false })
            let step = first.Row.Range.Step.IfNone(() => field.Control.Increment)
            let ticks = (int)double.Ceiling((first.Row.Range.High - first.Row.Range.Low) / step)
            let placed = fun((double held) => (int)double.Round((first.Row.Range.Clamped(held) - first.Row.Range.Low) / step, MidpointRounding.AwayFromZero))
            let valued = fun((int tick) => first.Row.Range.Clamped(first.Row.Range.Low + (tick * step)))
            from sized in IO.lift(() => { track.MaxValue = ticks; })
            from change in ControlChange<EventArgs>.Attached(
                Subscriptions.Host<EventArgs>(h => track.ValueChanged += h, h => track.ValueChanged -= h),
                _ =>
                    from current in latest
                    from shown in field.Show(valued(track.Value))
                    from previewed in current.Row.Edited(Edits.Preview(valued(track.Value)))
                    select unit,
                scope.Owner)
            from committed in Subscriptions.Merged(
                    Subscriptions.Host<MouseEventArgs>(h => track.MouseUp += h, h => track.MouseUp -= h).Choose(static _ => Some(EventArgs.Empty)),
                    Subscriptions.Host<KeyEventArgs>(h => track.KeyUp += h, h => track.KeyUp -= h).Choose(static _ => Some(EventArgs.Empty)))
                .Inline(_ => latest.Bind(current => current.Row.Edited(Edits.Commit(valued(track.Value)))), scope.Owner)
            from line in RowGrid.Lined([new TableCell(track, scaleWidth: true), new TableCell(field.Control)])
            select new RowCells(line,
                from current in latest
                from shown in field.Show(current.Row.Value)
                from moved in change.Detached(IO.lift(() => { track.Value = placed(current.Row.Value); }))
                select unit));

    public static ControlRow Distance(string label, Length value, (Length Low, Length High) domain, bool modelUnits, Func<Edit<Length>, IO<Unit>> edited) =>
        new ControlRow.Bound<(Length Value, (Length Low, Length High) Domain, bool ModelUnits, Func<Edit<Length>, IO<Unit>> Edited)>(
            label, RowShape.Inline, (value, domain, modelUnits, edited), static (latest, scope) =>
                from row in latest
                from doc in scope.DocumentIO
                from field in NumberFields.Distance(doc, row.Row.Domain, row.Row.ModelUnits, edit => latest.Bind(current => current.Row.Edited(edit)), scope.Owner)
                select new RowCells(field.Control, latest.Bind(current => field.Show(current.Row.Value))));
}
