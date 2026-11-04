using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Theme;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CurveChannel(string Name, string Caption, Seq<Vector2> Points, Func<float, float> Fit, NumberRange Across, NumberRange Along, Func<Edit<Seq<Vector2>>, IO<Unit>> Edited) {
    public static readonly Lens<Vector2, float> X = Lens<Vector2, float>.New(static point => point.X, static x => point => point with { X = x });
    public static readonly Lens<Vector2, float> Y = Lens<Vector2, float>.New(static point => point.Y, static y => point => point with { Y = y });

    public Option<IO<Seq<float>>> Underlay { get; init; }
    public Option<Seq<Vector2>> Rest { get; init; }

    public Vector2 Clamped(Vector2 point) => new((float)Across.Clamped(point.X), (float)Along.Clamped(point.Y));

    public Seq<Vector2> Moved(int index, Vector2 entered) =>
        (Before: Points.At(index - 1), After: Points.At(index + 1)) switch {
            var near => Points.Map((point, at) => at == index
                ? new Vector2(
                    float.Clamp(entered.X, near.Before.Map(static held => float.BitIncrement(held.X)).IfNone((float)Across.Low), near.After.Map(static held => float.BitDecrement(held.X)).IfNone((float)Across.High)),
                    (float)Along.Clamped(entered.Y))
                : point),
        };

    public Option<(Seq<Vector2> Points, int At)> Placed(float input) =>
        (float)Across.Clamped(input) switch {
            var x when Points.Exists(point => point.X == x) => None,
            var x => Points.Filter(point => point.X < x).Count switch {
                var at => Some((Points.Take(at).Add(new Vector2(x, (float)Along.Clamped(Fit(x)))) + Points.Skip(at), at)),
            },
        };

    public Option<Seq<Vector2>> Removed(int index) => Conversions.Present(Points.Count > 2, Points.Take(index) + Points.Skip(index + 1));
}

internal sealed record CurvePlot(CurveChannel Channel) {
    public Seq<Func<float, float>> References { get; init; }
    public Option<Seq<float>> Underlay { get; init; }
    public Option<int> Picked { get; init; }

    public Option<Vector2> PickedPoint => Picked.Bind(index => Channel.Points.At(index));

    public static float Handle => SystemFonts.Label().LineHeight / 4f;

    // --- [GEOMETRY]
    public Vector2 Plotted(PointF at, SizeF size) =>
        new((float)(Channel.Across.Low + (at.X / float.Max(size.Width - 1f, 1f) * (Channel.Across.High - Channel.Across.Low))),
            (float)(Channel.Along.High - (at.Y / float.Max(size.Height - 1f, 1f) * (Channel.Along.High - Channel.Along.Low))));

    public PointF Placed(Vector2 point, SizeF size) =>
        new((float)((point.X - Channel.Across.Low) / (Channel.Across.High - Channel.Across.Low) * (size.Width - 1f)),
            (float)((Channel.Along.High - point.Y) / (Channel.Along.High - Channel.Along.Low) * (size.Height - 1f)));

    public Option<int> Nearest(PointF at, SizeF size) =>
        Channel.Points.Map((point, index) => (Index: index, Distance: PointF.Distance(Placed(point, size), at))).Filter(static held => held.Distance <= Handle * 2f) switch {
            var near => near.Head.Map(_ => near.MinBy(static held => held.Distance).Index),
        };

    // --- [PAINT]
    public void Paint(Graphics graphics, RectangleF bounds, ThemeZone zone) {
        RectangleF frame = new(0f, 0f, bounds.Width - 1f, bounds.Height - 1f);
        graphics.FillRectangle(zone.Entry.Enabled.Background, frame);
        _ = Underlay.Iter(bins => {
            using SolidBrush brush = new(new Color(zone.Text.Secondary, 0.35f));
            float width = frame.Width / bins.Count;
            float peak = float.Max(bins.Fold(0f, float.Max), float.Epsilon);
            _ = bins.Iter((bin, index) => graphics.FillRectangle(brush, new RectangleF(index * width, frame.Bottom - (bin / peak * frame.Height), width, bin / peak * frame.Height)));
        });
        graphics.DrawRectangle(zone.Entry.Enabled.Border, frame);
        _ = References.Iter(reference => Traced(graphics, reference, zone.Text.Disabled, bounds.Size));
        Traced(graphics, Channel.Fit, zone.Text.Enabled, bounds.Size);
        foreach ((Vector2 point, int index) in Channel.Points.Map(static (point, index) => (point, index))) {
            PointF at = Placed(point, bounds.Size);
            RectangleF handle = new(at.X - Handle, at.Y - Handle, Handle * 2f, Handle * 2f);
            graphics.FillEllipse(Picked == Some(index) ? zone.Highlight : zone.Entry.Enabled.Background, handle);
            graphics.DrawEllipse(zone.Text.Enabled, handle);
        }
    }

    private void Traced(Graphics graphics, Func<float, float> fit, Color color, SizeF size) {
        int samples = int.Max((int)MathF.Ceiling(size.Width), 2);
        PointF[] points = [.. toSeq(Range(0, samples + 1)).Map(step => (float)(Channel.Across.Low + ((Channel.Across.High - Channel.Across.Low) * step / samples))).Map(x => Placed(Channel.Clamped(new Vector2(x, fit(x))), size))];
        using Pen pen = new(color);
        graphics.DrawLines(pen, points);
    }
}

public sealed record CurveRow(IterableNE<CurveChannel> Channels) : IRow<CurveRow> {
    public Option<Func<string, IO<Seq<float>>>> Pick { get; init; }

    public RowShape Shape { get; init; } = RowShape.Block;

    public CurveChannel Channel(string name) => toSeq(Channels).Find(channel => string.Equals(channel.Name, name, StringComparison.Ordinal)).IfNone(Channels.Head);

    public static IO<RowCells> Realize(IO<CurveRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from view in Surfaces.Of(new CurvePlot(first.Channels.Head), static (graphics, bounds, plot, zone) => plot.Paint(graphics, bounds, zone))
        from sized in IO.lift(() => { (view.Height, view.CanFocus) = (RhinoLayout.FixedWidth(RhinoLayout.WidthControlType.Numeric) * 2, true); })
        let active = latest.Map(row => row.Channel(view.State.Channel.Name))
        from across in Axis(view, active, CurveChannel.X, static channel => channel.Across)
        from along in Axis(view, active, CurveChannel.Y, static channel => channel.Along)
        let shown =
            from channel in active
            from row in latest
            from drawn in view.Show(view.State with {
                Channel = channel,
                References = toSeq(row.Channels).Filter(other => !string.Equals(other.Name, channel.Name, StringComparison.Ordinal) && other.Across == channel.Across && other.Along == channel.Along).Map(static other => other.Fit),
                Picked = view.State.Picked.Filter(at => at < channel.Points.Count),
            })
            from tipped in IO.lift(() => { view.ToolTip = $"{label.Local}: {LocalText.Of(channel.Caption).Local}"; })
            from inputs in across.Show(view.State.PickedPoint.Map(static point => CurveChannel.X.Get(point)))
            from outputs in along.Show(view.State.PickedPoint.Map(static point => CurveChannel.Y.Get(point)))
            select unit
        from pressed in Subscriptions.On<MouseEventArgs>(h => view.MouseDown += h, h => view.MouseDown -= h).Attach(args =>
            from focused in IO.lift(view.Focus)
            from captured in IO.lift(() => { if (args.Buttons is MouseButtons.Primary) _ = view.CaptureMouse(); })
            let picked = args.Buttons is MouseButtons.Primary || !view.IsMouseCaptured ? view.State.Nearest(args.Location, view.Size) : view.State.Picked
            from chosen in view.Show(view.State with { Picked = picked })
            from added in when(picked.IsNone && args.Buttons is MouseButtons.Primary, Inserted(active, view, [view.State.Plotted(args.Location, view.Size).X])).As()
            from redrawn in shown
            select unit)
        from dragged in Subscriptions.On<MouseEventArgs>(h => view.MouseMove += h, h => view.MouseMove -= h).Attach(args =>
            IO.lift(() => view.IsMouseCaptured && Mouse.IsAnyButtonPressed(MouseButtons.Primary))
                .Bind(captured => when(captured, Dragged(active, view, new Edit<Vector2>.Changed(Applies: false, view.State.Plotted(args.Location, view.Size))).Bind(_ => shown)).As()))
        from released in Subscriptions.On<MouseEventArgs>(h => view.MouseUp += h, h => view.MouseUp -= h).Attach(args =>
            IO.lift(() => view.IsMouseCaptured).Bind(captured => when(captured,
                IO.lift(view.ReleaseMouseCapture)
                >> IO.lift<Edit<Vector2>>(() => Mouse.IsAnyButtonPressed(MouseButtons.Primary)
                    ? new Edit<Vector2>.Cancel()
                    : new Edit<Vector2>.Changed(Applies: true, view.State.Plotted(args.Location, view.Size)))
                    .Bind(edit => Dragged(active, view, edit)).Bind(_ => shown)).As()))
        from keys in Subscriptions.Inline<KeyEventArgs>(h => view.KeyDown += h, h => view.KeyDown -= h).Attach(args =>
            when(args.Key is Keys.Delete or Keys.Backspace, Removed(active, view).Bind(_ => IO.lift(() => { args.Handled = true; })).Bind(_ => shown)).As())
        from chooser in toSeq(first.Channels).Count is 1
            ? IO.pure(Option<(Control Control, IO<Unit> Refresh)>.None)
            : CommandRows.Button(new Command<Invoke>.Choice(new CommandFace("curve-channels", label.English), toSeq(first.Channels).Map(channel =>
                    new Command<Invoke>.Run(new CommandFace(channel.Name, channel.Caption), new Invoke.Effect(_ => Switched(latest, view, channel.Name).Bind(_ => shown))) {
                        Marked = new RowRule(_ => IO.lift(() => string.Equals(view.State.Channel.Name, channel.Name, StringComparison.Ordinal))),
                    })), IconSlot.Small, scope).Map(static held => Some(held))
        from picker in first.Pick.Traverse(read => CommandRows.Button(
                new Command<Invoke>.Run(
                    new CommandFace("curve-pick", "Pick Point") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.SVGEditorEyeDropperSvg)) },
                    new Invoke.Effect(_ => read(view.State.Channel.Name).Bind(xs => Inserted(active, view, xs)).Bind(_ => shown))),
                IconSlot.Small, scope)).As()
        from body in chooser.Match(
            Some: held => Containers.Frame(() => new StackLayout(new StackLayoutItem(held.Control, HorizontalAlignment.Left), new StackLayoutItem(view, HorizontalAlignment.Stretch)) {
                Spacing = RhinoLayout.StackedSpacing(Orientation.Vertical, RhinoLayout.SpacingType.Table),
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }).Map(static stack => (Control)stack),
            None: () => IO.pure<Control>(view))
        select new RowCells(body, shown.Bind(_ => Products.Each(chooser.Map(static held => held.Refresh).ToSeq() + picker.Map(static held => held.Refresh).ToSeq())).Map(unit)) {
            Pick = picker.Map(static held => held.Control),
            Lines = [new RowLine(Some("Input"), across.Control), new RowLine(Some("Output"), along.Control)],
            Menu = [Commands(active, view, shown)],
        };

    private static IO<(Control Control, Func<Option<float>, IO<Unit>> Show)> Axis(Surface<CurvePlot> view, IO<CurveChannel> active, Lens<Vector2, float> axis, Func<CurveChannel, NumberRange> range) =>
        from field in use(static () => Values.Fielded(new NumericUpDownWithUnitParsing()))
        from digits in Values.Digits(field)
        from show in Values.Bound(field, Values.Numeric(field), Values.Stepping, None, None, edit =>
            from channel in active
            from moved in (from index in view.State.Picked from point in channel.Points.At(index) select (Index: index, Point: point))
                .Traverse(held => channel.Edited(edit.Mapped(value => channel.Moved(held.Index, axis.Set((float)value, held.Point))))).As()
            select unit)
        select ((Control)field, (Func<Option<float>, IO<Unit>>)(value =>
            from channel in active
            from ranged in IO.lift(() => {
                _ = range(channel).Applied(field);
                field.Enabled = value.IsSome;
            })
            from written in value.Traverse(held => show(Some((double)held))).As()
            select unit));

    private static IO<Unit> Switched(IO<CurveRow> latest, Surface<CurvePlot> view, string name) =>
        from row in latest
        let channel = row.Channel(name)
        from underlay in view.State.Underlay.IsSome ? channel.Underlay.Traverse(static read => read).As() : IO.pure(Option<Seq<float>>.None)
        from changed in view.Show(new CurvePlot(channel) { Underlay = underlay })
        select unit;

    private static Seq<Command<Invoke>> Commands(IO<CurveChannel> active, Surface<CurvePlot> view, IO<Unit> shown) => [
        new Command<Invoke>.Run(new CommandFace("curve-reset", "Reset Curve"),
            new Invoke.Effect(_ =>
                from channel in active
                from reset in channel.Rest.Traverse(rest => channel.Edited(new Edit<Seq<Vector2>>.Changed(Applies: true, rest))).As()
                from cleared in view.Show(view.State with { Picked = None })
                from redrawn in shown
                select unit)) {
            Enabled = new RowRule(_ => active.Map(static channel => channel.Rest.Exists(rest => rest != channel.Points))),
        },
        new Command<Invoke>.Run(new CommandFace("curve-underlay", "Underlay"),
            new Invoke.Effect(_ =>
                from channel in active
                from underlay in view.State.Underlay.IsSome ? IO.pure(Option<Seq<float>>.None) : channel.Underlay.Traverse(static read => read).As()
                from changed in view.Show(view.State with { Underlay = underlay })
                from redrawn in shown
                select unit)) {
            Enabled = new RowRule(_ => active.Map(static channel => channel.Underlay.IsSome)),
            Marked = new RowRule(_ => IO.lift(() => view.State.Underlay.IsSome)),
        },
    ];

    // --- [POINTS]
    private static IO<Unit> Inserted(IO<CurveChannel> active, Surface<CurvePlot> view, Seq<float> xs) =>
        from channel in active
        let placed = xs.Fold((channel.Points, At: Option<int>.None), (inserted, x) =>
            (channel with { Points = inserted.Points }).Placed(x).Match(Some: static next => (next.Points, Some(next.At)), None: () => inserted))
        from written in placed.At.Traverse(at =>
            from taken in channel.Edited(new Edit<Seq<Vector2>>.Changed(Applies: true, placed.Points))
            from chosen in view.Show(view.State with { Picked = Some(at) })
            select unit).As()
        select unit;

    private static IO<Unit> Removed(IO<CurveChannel> active, Surface<CurvePlot> view) =>
        from channel in active
        from removed in view.State.Picked.Bind(channel.Removed).Traverse(points =>
            from taken in channel.Edited(new Edit<Seq<Vector2>>.Changed(Applies: true, points))
            from cleared in view.Show(view.State with { Picked = None })
            select unit).As()
        select unit;

    private static IO<Unit> Dragged(IO<CurveChannel> active, Surface<CurvePlot> view, Edit<Vector2> edit) =>
        from channel in active
        from moved in view.State.Picked.Traverse(index => channel.Edited(edit.Mapped(point => channel.Moved(index, point)))).As()
        select unit;
}
