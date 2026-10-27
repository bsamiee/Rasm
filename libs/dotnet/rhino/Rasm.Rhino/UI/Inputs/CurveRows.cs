using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CurveLane(string Name, string Caption, Seq<Vector2> Points, Func<float, float> Fit, RectangleF Domain, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public Option<IO<Seq<float>>> Underlay { get; init; }
    public Option<Seq<Vector2>> Rest { get; init; }

    public Vector2 Clamped(Vector2 point) => new(float.Clamp(point.X, Domain.Left, Domain.Right), float.Clamp(point.Y, Domain.Top, Domain.Bottom));

    public Seq<Vector2> Moved(int index, Vector2 entered) =>
        (Before: Points.At(index - 1), After: Points.At(index + 1)) switch {
            var near => Points.Map((point, at) => at == index
                ? new Vector2(
                    float.Clamp(entered.X, near.Before.Map(static held => float.BitIncrement(held.X)).IfNone(Domain.Left), near.After.Map(static held => float.BitDecrement(held.X)).IfNone(Domain.Right)),
                    float.Clamp(entered.Y, Domain.Top, Domain.Bottom))
                : point),
        };

    public Option<(Seq<Vector2> Points, int At)> Placed(float input) =>
        float.Clamp(input, Domain.Left, Domain.Right) switch {
            var x when Points.Exists(point => point.X == x) => None,
            var x => Points.Filter(point => point.X < x).Count switch {
                var at => Some((Points.Take(at).Add(new Vector2(x, float.Clamp(Fit(x), Domain.Top, Domain.Bottom))) + Points.Skip(at), at)),
            },
        };
}

internal sealed record CurvePlot(CurveLane Lane) {
    private const float Handle = 4f;
    private const int Samples = 128;

    public Seq<Func<float, float>> References { get; init; }
    public Option<Seq<float>> Underlay { get; init; }
    public Option<int> Picked { get; init; }

    public Option<Vector2> Anchor => Picked.Bind(index => Lane.Points.At(index));

    // --- [GEOMETRY]
    public Vector2 Plotted(PointF at, SizeF size) =>
        new(Lane.Domain.Left + (at.X / float.Max(size.Width - 1f, 1f) * Lane.Domain.Width), Lane.Domain.Bottom - (at.Y / float.Max(size.Height - 1f, 1f) * Lane.Domain.Height));

    public PointF Placed(Vector2 point, SizeF size) =>
        new((point.X - Lane.Domain.Left) / Lane.Domain.Width * (size.Width - 1f), (Lane.Domain.Bottom - point.Y) / Lane.Domain.Height * (size.Height - 1f));

    public Option<int> Nearest(PointF at, SizeF size) =>
        Lane.Points.Map((point, index) => (Index: index, Distance: PointF.Distance(Placed(point, size), at))).Filter(static held => held.Distance <= Handle * 2f) switch {
            var near => near.Head.Map(_ => near.MinBy(static held => held.Distance).Index),
        };

    // --- [PAINT]
    public void Paint(Graphics graphics, RectangleF bounds) {
        RectangleF frame = new(0f, 0f, bounds.Width - 1f, bounds.Height - 1f);
        graphics.FillRectangle(SystemColors.ControlBackground, frame);
        _ = Underlay.Iter(bins => {
            using SolidBrush brush = new(new Color(SystemColors.DisabledText, 0.35f));
            float width = frame.Width / bins.Count;
            float peak = float.Max(bins.Fold(0f, float.Max), float.Epsilon);
            _ = bins.Iter((bin, index) => graphics.FillRectangle(brush, new RectangleF(index * width, frame.Bottom - (bin / peak * frame.Height), width, bin / peak * frame.Height)));
        });
        graphics.DrawRectangle(SystemColors.DisabledText, frame);
        _ = References.Iter(reference => Traced(graphics, reference, SystemColors.DisabledText, bounds.Size));
        Traced(graphics, Lane.Fit, SystemColors.ControlText, bounds.Size);
        foreach ((Vector2 point, int index) in Lane.Points.Map(static (point, index) => (point, index))) {
            PointF at = Placed(point, bounds.Size);
            RectangleF handle = new(at.X - Handle, at.Y - Handle, Handle * 2f, Handle * 2f);
            graphics.FillEllipse(Picked == Some(index) ? SystemColors.Highlight : SystemColors.ControlBackground, handle);
            graphics.DrawEllipse(SystemColors.ControlText, handle);
        }
    }

    private void Traced(Graphics graphics, Func<float, float> fit, Color color, SizeF size) {
        PointF[] points = [.. toSeq(Range(0, Samples + 1)).Map(step => Lane.Domain.Left + (Lane.Domain.Width * step / Samples)).Map(x => Placed(Lane.Clamped(new Vector2(x, fit(x))), size))];
        using Pen pen = new(color);
        graphics.DrawLines(pen, points);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveRows {
    // --- [EDITORS]
    public static ControlRow Editor(string label, IterableNE<CurveLane> lanes, Option<Func<string, IO<Seq<float>>>> pick = default) =>
        new ControlRow.Bound<(IterableNE<CurveLane> Lanes, Option<Func<string, IO<Seq<float>>>> Pick)>(label, RowShape.Block, (lanes, pick), static (latest, scope) =>
            from row in latest
            let keyed = latest.Map(static current => toHashMap(current.Row.Lanes.Map(static lane => (lane.Name, lane))))
            from view in Surfaces.Of(new CurvePlot(row.Row.Lanes.Head), static (graphics, bounds, plot) => plot.Paint(graphics, bounds), scope.Owner)
            from sized in IO.lift(() => { view.Height = 160; })
            from input in Coordinates(row.Row.Lanes, static domain => (domain.Left, domain.Right), edit => Entered(keyed, view, static (point, x) => point with { X = x }, edit), scope.Owner)
            from output in Coordinates(row.Row.Lanes, static domain => (domain.Top, domain.Bottom), edit => Entered(keyed, view, static (point, y) => point with { Y = y }, edit), scope.Owner)
            let shown =
                from current in latest
                from lanes in keyed
                let lane = lanes[view.State.Lane.Name]
                from drawn in view.Show(view.State with {
                    Lane = lane,
                    References = toSeq(lanes.Values).Filter(other => !string.Equals(other.Name, lane.Name, StringComparison.Ordinal) && other.Domain == lane.Domain).Map(static other => other.Fit),
                })
                from faced in IO.lift(() => {
                    view.Enabled = current.Enabled;
                    view.ToolTip = $"{RowText.Localize(current.Label, scope.Owner).Local}: {RowText.Localize(lane.Caption, scope.Owner).Local}";
                })
                let anchor = view.State.Anchor
                from inputs in input.Show(lane.Name, anchor.Map(static point => point.X))
                from outputs in output.Show(lane.Name, anchor.Map(static point => point.Y))
                select unit
            from pressed in Subscriptions.Host<MouseEventArgs>(h => view.MouseDown += h, h => view.MouseDown -= h).Inline(args =>
                from focused in IO.lift(view.Focus)
                let picked = view.State.Nearest(args.Location, view.Size)
                from chosen in view.Show(view.State with { Picked = picked })
                from added in when(picked.IsNone && args.Buttons is MouseButtons.Primary, Inserted(keyed, view, Seq(view.State.Plotted(args.Location, view.Size).X))).As()
                from redrawn in shown
                select unit, scope.Owner)
            from dragged in Subscriptions.Host<MouseEventArgs>(h => view.MouseMove += h, h => view.MouseMove -= h).Inline(args =>
                when(args.Buttons is MouseButtons.Primary, Dragged(keyed, view, Edits.Preview(view.State.Plotted(args.Location, view.Size))).Bind(_ => shown)).As(), scope.Owner)
            from released in Subscriptions.Host<MouseEventArgs>(h => view.MouseUp += h, h => view.MouseUp -= h).Inline(args =>
                when(args.Buttons is MouseButtons.Primary, Dragged(keyed, view, Edits.Commit(view.State.Plotted(args.Location, view.Size))).Bind(_ => shown)).As(), scope.Owner)
            from keys in Subscriptions.During<KeyEventArgs>(h => view.KeyDown += h, h => view.KeyDown -= h).Inline(args =>
                when(args.Key is Keys.Delete or Keys.Backspace, Removed(keyed, view).Bind(_ => IO.lift(() => { args.Handled = true; })).Bind(_ => shown)).As(), scope.Owner)
            from strip in toSeq(row.Row.Lanes).Count is 1
                ? IO.pure(Option<Control>.None)
                : from chosen in ChoiceRows.Strip(toSeq(row.Row.Lanes).Map(lane => new ChoiceItem<string>(lane.Name, RowText.Localize(lane.Caption, scope.Owner).Local)),
                      name => Laned(keyed, view, name).Bind(_ => shown), scope.Owner)
                  from first in chosen.Show(row.Row.Lanes.Head.Name)
                  select Some(chosen.Control)
            from picked in row.Row.Pick.Traverse(read => CommandRows.Button(
                    new Command<Invoke>.Run(
                        new CommandFace("curve-pick", "Pick Point") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.SVGEditorEyeDropperSvg)) }, None,
                        new Invoke.Effect(_ => read(view.State.Lane.Name).Bind(xs => Inserted(keyed, view, xs)).Bind(_ => shown))),
                    IconSlot.Small, scope)).As()
            from body in strip.Match(
                Some: chooser => Containers.Frame(() => new StackLayout(new StackLayoutItem(chooser, HorizontalAlignment.Left), new StackLayoutItem(view, HorizontalAlignment.Stretch)) {
                    Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Table),
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                }).Map(static stack => (Control)stack),
                None: () => IO.pure<Control>(view))
            select new RowCells(body, shown.Bind(_ => picked.Traverse(static button => button.Refresh).As()).Map(unit)) {
                Pick = picked.Map(static button => button.Control),
                Lines = Seq(new RowLine(Some("Input"), input.Panel), new RowLine(Some("Output"), output.Panel)),
                Menu = [Commands(keyed, view, shown)],
            });

    private static IO<Unit> Laned(IO<HashMap<string, CurveLane>> keyed, Surface<CurvePlot> view, string name) =>
        from lanes in keyed
        from underlay in view.State.Underlay.IsSome ? lanes[name].Underlay.Traverse(static read => read).As() : IO.pure(Option<Seq<float>>.None)
        from changed in view.Show(new CurvePlot(lanes[name]) { Underlay = underlay })
        select unit;

    private static Seq<Command<Invoke>> Commands(IO<HashMap<string, CurveLane>> keyed, Surface<CurvePlot> view, IO<Unit> shown) =>
        keyed.Map(lanes => lanes[view.State.Lane.Name]) switch {
            var active => [
                new Command<Invoke>.Run(new CommandFace("curve-reset", "Reset Curve"),
                    Some(new RowRule(_ => active.Map(static lane => lane.Rest.Exists(rest => rest != lane.Points)))),
                    new Invoke.Effect(_ =>
                        from lane in active
                        from reset in lane.Rest.Traverse(rest => lane.Edited(Edits.Commit(rest))).As()
                        from cleared in view.Show(view.State with { Picked = None })
                        from redrawn in shown
                        select unit)),
                new Command<Invoke>.Run(new CommandFace("curve-underlay", "Underlay"),
                    Some(new RowRule(_ => active.Map(static lane => lane.Underlay.IsSome))),
                    new Invoke.Effect(_ =>
                        from lane in active
                        from underlay in view.State.Underlay.IsSome ? IO.pure(Option<Seq<float>>.None) : lane.Underlay.Traverse(static read => read).As()
                        from changed in view.Show(view.State with { Underlay = underlay })
                        from redrawn in shown
                        select unit)) {
                    Marked = new RowRule(_ => IO.lift(() => view.State.Underlay.IsSome)),
                },
            ],
        };

    // --- [POINTS]
    private static IO<Unit> Inserted(IO<HashMap<string, CurveLane>> keyed, Surface<CurvePlot> view, Seq<float> xs) =>
        from lanes in keyed
        let lane = lanes[view.State.Lane.Name]
        let placed = xs.Fold((lane.Points, At: Option<int>.None), (inserted, x) =>
            (lane with { Points = inserted.Points }).Placed(x).Match(Some: static next => (next.Points, Some(next.At)), None: () => inserted))
        from written in placed.At.Traverse(at =>
            from taken in lane.Edited(Edits.Commit(placed.Points))
            from chosen in view.Show(view.State with { Picked = Some(at) })
            select unit).As()
        select unit;

    private static IO<Unit> Removed(IO<HashMap<string, CurveLane>> keyed, Surface<CurvePlot> view) =>
        from lanes in keyed
        let lane = lanes[view.State.Lane.Name]
        from removed in view.State.Picked.Filter(_ => lane.Points.Count > 2).Traverse(at =>
            from taken in lane.Edited(Edits.Commit(lane.Points.Take(at) + lane.Points.Skip(at + 1)))
            from cleared in view.Show(view.State with { Picked = None })
            select unit).As()
        select unit;

    private static IO<Unit> Dragged(IO<HashMap<string, CurveLane>> keyed, Surface<CurvePlot> view, Edit<Vector2> edit) =>
        from lanes in keyed
        let lane = lanes[view.State.Lane.Name]
        from moved in view.State.Picked.Traverse(index => lane.Edited(edit.Mapped(point => lane.Moved(index, point)))).As()
        select unit;

    private static IO<Unit> Entered(IO<HashMap<string, CurveLane>> keyed, Surface<CurvePlot> view, Func<Vector2, float, Vector2> coordinate, Edit<double> edit) =>
        from lanes in keyed
        let lane = lanes[view.State.Lane.Name]
        from moved in view.State.Picked.Traverse(index => lane.Edited(edit.Mapped(value =>
            lane.Points.At(index).Map(point => lane.Moved(index, coordinate(point, (float)value))).IfNone(lane.Points)))).As()
        select unit;

    // --- [COORDINATES]
    private static IO<(Control Panel, Func<string, Option<float>, IO<Unit>> Show)> Coordinates(
        IterableNE<CurveLane> lanes, Func<RectangleF, (float Low, float High)> axis, Func<Edit<double>, IO<Unit>> edited, IPlugInLifetime owner) =>
        from fields in toSeq(lanes).TraverseM(lane => axis(lane.Domain) switch {
            var ends => NumberFields.Scalar(new NumberRange(ends.Low, ends.High), edited, owner).Map(field => (lane.Name, Field: field)),
        }).As()
        let keyed = toHashMap(fields)
        from panel in Containers.Frame(static () => new Panel())
        from placed in IO.lift(() => {
            panel.Content = keyed[lanes.Head.Name].Control;
            keyed[lanes.Head.Name].Control.Enabled = false;
        })
        select (
            (Control)panel,
            (Func<string, Option<float>, IO<Unit>>)((name, value) => keyed[name] switch {
                var field => IO.lift(() => {
                    panel.Content = field.Control;
                    field.Control.Enabled = value.IsSome;
                }).Bind(_ => value.Traverse(held => field.Show(held)).As().Map(unit)),
            }));
}
