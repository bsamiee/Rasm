using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rhino;
using UnitsNet;

namespace Rasm.Rhino.UI.Numeric;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Sliders {
    // --- [ROWS]
    public static ControlRow Numeric(string label, Slider track) =>
        new ControlRow.Bound<Slider>(label, RowShape.Inline, track, static (latest, scope, retain) =>
            Scened(latest, static row => (row.Row.Drawn, new ScalarText(row.Row.Track(row.Row.Soft.IfNone(row.Row.Domain)), RowText.Culture) { Step = row.Row.Step, Decimals = row.Row.Decimals, Suffix = row.Row.Unit },
                (row.Row.Domain.Low, row.Row.Domain.High), row.Row.Value, row.Row.Edited), square: false, scope, retain));

    public static ControlRow Turned(string label, Dial dial) =>
        new ControlRow.Bound<Dial>(label, RowShape.Inline, dial, static (latest, scope, retain) =>
            Scened(latest, static row => (row.Row.Drawn, new ScalarText(row.Row.Axis, RowText.Culture) { Step = row.Row.Axis.Step(0f) },
                (row.Row.Axis.Domain.Low, row.Row.Axis.Domain.High), row.Row.Value, row.Row.Edited), square: true, scope, retain));

    public static ControlRow Distance(string label, Length value, (Length Low, Length High) domain, bool modelUnits, Func<Edit<Length>, IO<Unit>> edited) =>
        new ControlRow.Bound<(Length Value, (Length Low, Length High) Domain, bool ModelUnits, Func<Edit<Length>, IO<Unit>> Edited)>(
            label, RowShape.Inline, (value, domain, modelUnits, edited), static (latest, scope, retain) =>
                from row in latest
                from doc in scope.DocumentIO
                from text in Measure(doc, row.Row.Domain, row.Row.ModelUnits)
                from field in DisposalOps.Acquire(IO.lift(() => new NumberField<Length>(text, row.Row.Domain, edit => latest.Bind(current => current.Row.Edited(edit)), scope.Sink)), retain)
                from retexted in EventKind.DocumentPropertiesChanged.In(doc.RuntimeSerialNumber).Inline(_ =>
                    from current in latest
                    from next in Measure(doc, current.Row.Domain, current.Row.ModelUnits)
                    from retexted in field.Retext(next, current.Row.Domain)
                    select retexted, scope.Sink, retain)
                select new RowCells(field, latest.Bind(current => field.Receive(current.Row.Value))));

    // --- [REALIZE]
    private static IO<RowCells> Scened<TRow>(
        IO<ControlRow.Bound<TRow>> latest, Func<ControlRow.Bound<TRow>, (Node Drawn, NumberText<float> Text, (float Low, float High) Bounds, float Value, Func<Edit<float>, IO<Unit>> Edited)> read,
        bool square, RowScope scope, Action<IDisposable> retain)
        where TRow : notnull =>
        from first in latest.Map(read)
        let site = new CallbackSite(scope.Sink, typeof(Sliders), nameof(Scened))
        from field in DisposalOps.Acquire(IO.lift(() => new NumberField<float>(first.Text, first.Bounds, edit => latest.Bind(current => read(current).Edited(edit)), scope.Sink)), retain)
        from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
        from squared in when(square, Subscriptions.Attach(h => field.SizeChanged += h, h => field.SizeChanged -= h,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => { view.Width = field.Height; }), site), retain)).As()
        from line in RowGrid.Lined([new(view, scaleWidth: !square), new(field)], retain)
        select new RowCells(line, latest.Bind(current => read(current) switch {
            var shown =>
                from drawn in view.Show(shown.Drawn with { Accessible = shown.Drawn.Accessible.Combine(new Accessible { Label = RowText.Localize(current.Label, scope.Sink).Local }) })
                from retexted in field.Retext(shown.Text, shown.Bounds)
                from received in field.Receive(shown.Value)
                select unit,
        }));

    private static IO<NumberText<Length>> Measure(RhinoDoc doc, (Length Low, Length High) ends, bool modelUnits) =>
        DistanceDisplay.Read(doc, modelUnits).Map<NumberText<Length>>(display => new DistanceText(modelUnits ? doc.ModelUnits : doc.PageUnits, display, ends));
}
