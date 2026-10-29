using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Display;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Theme;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ColorRamp {
    private ColorRamp(Seq<ColorStop> stops, bool stepped) => (Stops, Stepped) = (toSeq(stops.OrderBy(static stop => stop.Position)), stepped);

    public Seq<ColorStop> Stops { get; }
    public bool Stepped { get; init; }

    public static ColorRamp Of(ColorStop first, params Seq<ColorStop> rest) => new(first.Cons(rest), stepped: false);

    public static Fin<ColorRamp> Of(ColorGradient gradient) =>
        toSeq(gradient.GetColorStops()) switch {
            var stops => RhinoFault.Unless(!stops.IsEmpty, RhinoReason.Missing, stops.Count).Map(_ => new ColorRamp(stops, stepped: false)),
        };

    public IO<Unit> Write(ColorGradient gradient) => IO.lift(() => gradient.SetColorStops(Stops));

    public System.Drawing.Color At(double position) =>
        (Before: Stops.Filter(stop => stop.Position <= position).Last, After: Stops.Filter(stop => stop.Position > position).Head) switch {
            var near => (Stepped
                    ? Option<System.Drawing.Color>.None
                    : from before in near.Before
                      from after in near.After
                      select new Color4f(before.Color).BlendTo((float)((position - before.Position) / (after.Position - before.Position)), new Color4f(after.Color)).AsSystemColor())
                .IfNone(() => (near.Before | near.After).Map(static stop => stop.Color).IfNone(Stops[0].Color)),
        };

    public ColorRamp WithStop(int index, ColorStop stop) => new(Stops.Map((held, at) => at == index ? stop : held), Stepped);

    public ColorRamp Added(double position) => new(Stops.Add(new ColorStop(At(position), position)), Stepped);

    public Option<ColorRamp> Removed(int index) => Conversions.Present(Stops.Count > 1, new ColorRamp(Stops.Take(index) + Stops.Skip(index + 1), Stepped));
}

public sealed record ColorSource(DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color);

public sealed record ColorPair(ColorSource Display, ColorSource Print);

internal sealed record RampShown(ColorRamp Ramp) {
    public Option<int> Picked { get; init; }

    public Option<ColorStop> Stop => Picked.Bind(at => Ramp.Stops.At(at));

    // --- [GEOMETRY]
    public static double Position(PointF at, float width) => double.Clamp(at.X / float.Max(width, 1f), 0d, 1d);

    public Option<int> Nearest(PointF at, float width, float marker) =>
        Ramp.Stops.Map((stop, index) => (Index: index, Distance: double.Abs(stop.Position - Position(at, width)) * width)).Filter(held => held.Distance <= marker * 2f) switch {
            var near => near.Head.Map(_ => near.MinBy(static held => held.Distance).Index),
        };

    // --- [PAINT]
    public void Paint(Graphics graphics, RectangleF bounds, ThemeZone zone) {
        float marker = bounds.Height / 4f;
        RectangleF band = new(0f, 0f, bounds.Width, float.Max(bounds.Height - (marker * 2f), 1f));
        foreach ((ColorStop from, ColorStop to) in Ramp.Stops.Zip(Ramp.Stops.Skip(1), static (from, to) => (from, to))) {
            RectangleF segment = new((float)from.Position * band.Width, 0f, (float)(to.Position - from.Position) * band.Width, band.Height);
            if (Ramp.Stepped) {
                graphics.FillRectangle(from.Color.ToEto(), segment);
            } else {
                using LinearGradientBrush brush = new(from.Color.ToEto(), to.Color.ToEto(), segment.TopLeft, segment.TopRight);
                graphics.FillRectangle(brush, segment);
            }
        }
        graphics.FillRectangle(Ramp.Stops[0].Color.ToEto(), new RectangleF(0f, 0f, (float)Ramp.Stops[0].Position * band.Width, band.Height));
        _ = Ramp.Stops.Last.Iter(last => graphics.FillRectangle(last.Color.ToEto(), new RectangleF((float)last.Position * band.Width, 0f, (float)(1d - last.Position) * band.Width, band.Height)));
        graphics.DrawRectangle(zone.Entry.Enabled.Border, band);
        foreach ((ColorStop stop, int index) in Ramp.Stops.Map(static (stop, index) => (stop, index))) {
            float x = (float)stop.Position * band.Width;
            PointF[] tip = [new(x, band.Bottom), new(x - marker, band.Bottom + (marker * 2f)), new(x + marker, band.Bottom + (marker * 2f))];
            graphics.FillPolygon(Picked == Some(index) ? zone.Highlight : zone.Text.Enabled, tip);
        }
    }
}

public sealed record SwatchesRow(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited) : IRow<SwatchesRow> {
    public RowShape Shape { get; init; } = RowShape.Block;

    public static IO<RowCells> Realize(IO<SwatchesRow> latest, LocalizeStringPair label, RowScope scope) =>
        from picked in atomIO(Option<int>.None)
        from seated in IO.lift(static () => Ref(HashMap<int, Owned<(ColorPicker Picker, Func<Option<Color4f>, IO<Unit>> Show)>>()))
        from owned in use(static () => EnvIO.New(token: CancellationToken.None))
        from strip in Containers.Frame(static () => new StackLayout { Orientation = Orientation.Horizontal, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Width })
        let selected = from row in latest from held in picked.ValueIO select held.Filter(at => at < row.Colors.Count)
        let shown =
            from row in latest
            from held in atomicIO(() => seated.Value)
            from made in toSeq(Range(0, row.Colors.Count)).Filter(index => !held.ContainsKey(index))
                .TraverseM(index => Products.Replace(seated, index, Swatched(latest, picked, strip, index)).Scoped(owned)).As()
            from dropped in toSeq(held.Keys).Filter(index => index >= row.Colors.Count).TraverseM(index =>
                held.Find(index).Traverse(cut => IO.lift(() => strip.Items.Remove(cut.Value.Picker))).As() >> Products.Clear(seated, index).Scoped(owned)).As()
            from kept in atomicIO(() => seated.Value)
            from colored in row.Colors.Map(static (color, index) => (Color: color, Index: index))
                .TraverseM(entry => kept.Find(entry.Index).Traverse(seat => seat.Value.Show(Some(entry.Color))).As()).As()
            select unit
        let add = new Command<Invoke>.Run(new CommandFace("color-add", "Add Color"), Some(new RowRule(_ => latest.Map(static row => !row.Colors.IsEmpty))), new Invoke.Effect(_ =>
            from row in latest
            from held in selected
            from next in IO.lift(() => Conversions.Required((held.Bind(index => row.Colors.At(index)) | row.Colors.Last).Map(row.Colors.Add)))
            from edited in row.Edited(EditPhase.Commit.Of(next))
            from chosen in picked.SwapIO(_ => Some(next.Count - 1))
            select unit))
        let remove = new Command<Invoke>.Run(new CommandFace("color-remove", "Remove Color"), Some(new RowRule(_ => selected.Map(static at => at.IsSome))), new Invoke.Effect(_ =>
            from row in latest
            from held in selected
            from removed in held.Traverse(index =>
                from edited in row.Edited(EditPhase.Commit.Of(row.Colors.Take(index) + row.Colors.Skip(index + 1)))
                from chosen in picked.SwapIO(_ => Some(int.Min(index, row.Colors.Count - 2)).Filter(static kept => kept >= 0))
                select unit).As()
            select unit))
        from pair in CommandRows.AddRemove(add, remove, scope)
        from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(pair.Control, HorizontalAlignment.Left), new StackLayoutItem(strip, HorizontalAlignment.Stretch)) {
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        })
        select new RowCells(body, shown.Bind(_ => pair.Refresh)) {
            Menu = [[Values.Named(
                from row in latest from at in selected select at.Bind(index => row.Colors.At(index)),
                edit => from row in latest
                        from at in selected
                        from sent in at.Traverse(index => row.Edited(edit.Mapped(color => row.Colors.Map((held, place) => place == index ? color : held)))).As()
                        select unit)]],
        };

    private static IO<(ColorPicker Picker, Func<Option<Color4f>, IO<Unit>> Show)> Swatched(IO<SwatchesRow> latest, Atom<Option<int>> picked, StackLayout strip, int index) =>
        from picker in use(static () => new ColorPicker { AllowAlpha = true })
        from show in Values.Bound(picker, Values.Colored(picker), static _ => true, Some(Values.Closed(picker)), None,
            edit => latest.Bind(row => row.Edited(edit.Mapped(next => row.Colors.Map((kept, at) => at == index ? next : kept)))))
        from focused in Subscriptions.On<EventArgs>(h => picker.GotFocus += h, h => picker.GotFocus -= h).Attach(_ => picked.SwapIO(_ => Some(index)).Map(unit))
        from placed in IO.lift(() => strip.Items.Add(picker))
        select (Picker: picker, Show: show);
}

public sealed record GradientRow(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited) : IRow<GradientRow> {
    public RowShape Shape { get; init; } = RowShape.Block;

    public static IO<RowCells> Realize(IO<GradientRow> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from strip in Surfaces.Of(new RampShown(first.Ramp), static (graphics, bounds, shown, zone) => shown.Paint(graphics, bounds, zone))
        from sized in IO.lift(() => { strip.Height = (int)MathF.Ceiling(SystemFonts.Label().LineHeight * 2f); })
        let stop = Stopped(latest, strip)
        from swatch in use(static () => new ColorPicker { AllowAlpha = true })
        from colored in Values.Bound(swatch, Values.Colored(swatch), static _ => true, Some(Values.Closed(swatch)), None, stop)
        from position in NumberFields.Scalar(new NumberRange(0d, 1d) { Step = Some(0.01d), Decimals = Some(2) }, edit => Positioned(latest, strip, edit))
        let shown =
            from row in latest
            from drawn in strip.Show(strip.State with { Ramp = row.Ramp })
            let chosen = strip.State.Stop
            from enabled in IO.lift(() => { (swatch.Enabled, position.Control.Enabled) = (chosen.IsSome, chosen.IsSome); })
            from painted in colored(chosen.Map(static held => new Color4f(held.Color)))
            from placed in chosen.Traverse(held => position.Show(held.Position)).As()
            select unit
        from pressed in Subscriptions.On<MouseEventArgs>(h => strip.MouseDown += h, h => strip.MouseDown -= h).Attach(args =>
            from row in latest
            let picked = strip.State.Nearest(args.Location, strip.Width, strip.Height / 4f)
            from chosen in strip.Show(strip.State with { Picked = picked })
            from added in when(picked.IsNone && args.Buttons is MouseButtons.Primary, row.Edited(EditPhase.Commit.Of(row.Ramp.Added(RampShown.Position(args.Location, strip.Width))))).As()
            from redrawn in shown
            select unit)
        from dragged in Subscriptions.On<MouseEventArgs>(h => strip.MouseMove += h, h => strip.MouseMove -= h).Attach(args =>
            when(args.Buttons is MouseButtons.Primary, Positioned(latest, strip, EditPhase.Preview.Of(RampShown.Position(args.Location, strip.Width))).Bind(_ => shown)).As())
        from released in Subscriptions.On<MouseEventArgs>(h => strip.MouseUp += h, h => strip.MouseUp -= h).Attach(args =>
            when(args.Buttons is MouseButtons.Primary, Positioned(latest, strip, EditPhase.Commit.Of(RampShown.Position(args.Location, strip.Width))).Bind(_ => shown)).As())
        from keyed in Subscriptions.Inline<KeyEventArgs>(h => strip.KeyDown += h, h => strip.KeyDown -= h).Attach(args =>
            when(args.Key is Keys.Delete or Keys.Backspace,
                from row in latest
                from removed in strip.State.Picked.Bind(row.Ramp.Removed).Traverse(next => row.Edited(EditPhase.Commit.Of(next))).As()
                from cleared in strip.Show(strip.State with { Picked = None })
                from handled in IO.lift(() => { args.Handled = true; })
                from redrawn in shown
                select unit).As())
        from mode in CommandRows.Button(
            new Command<Invoke>.Choice(new CommandFace("gradient-mode", "Interpolation"), None, [
                Moded(latest, "gradient-smooth", "Smooth", stepped: false),
                Moded(latest, "gradient-stepped", "Stepped", stepped: true),
            ]),
            IconSlot.Small, scope)
        from line in RowTable.Lined([new TableCell(swatch), new TableCell(position.Control, scaleWidth: true)])
        select new RowCells(mode.Control, shown.Bind(_ => mode.Refresh)) {
            Lines = [new RowLine(None, strip), new RowLine(Some("Stop"), line)],
            Menu = [[Values.Named(IO.lift(() => strip.State.Stop.Map(static held => new Color4f(held.Color))), stop)]],
        };

    private static Command<Invoke>.Run Moded(IO<GradientRow> latest, string name, string caption, bool stepped) =>
        new(new CommandFace(name, caption), None, new Invoke.Effect(_ => latest.Bind(row => row.Edited(EditPhase.Commit.Of(row.Ramp with { Stepped = stepped }))))) {
            Marked = new RowRule(_ => latest.Map(row => row.Ramp.Stepped == stepped)),
        };

    private static Func<Edit<Color4f>, IO<Unit>> Stopped(IO<GradientRow> latest, Surface<RampShown> strip) =>
        edit => from row in latest
                from edited in (from at in strip.State.Picked from held in row.Ramp.Stops.At(at) select (At: at, Held: held))
                    .Traverse(stop => row.Edited(edit.Mapped(color => row.Ramp.WithStop(stop.At, new ColorStop(color.AsSystemColor(), stop.Held.Position))))).As()
                select unit;

    private static IO<Unit> Positioned(IO<GradientRow> latest, Surface<RampShown> strip, Edit<double> edit) =>
        from row in latest
        from moved in (from at in strip.State.Picked from held in row.Ramp.Stops.At(at) select (At: at, Held: held)).Traverse(stop =>
            from edited in row.Edited(edit.Mapped(value => row.Ramp.WithStop(stop.At, new ColorStop(stop.Held.Color, value))))
            from reselected in edit.Held.Traverse(value => strip.Show(strip.State with {
                Picked = row.Ramp.WithStop(stop.At, new ColorStop(stop.Held.Color, value)).Stops
                    .Map(static (held, index) => (held.Position, Index: index)).Find(held => held.Position == value).Map(static held => held.Index) | Some(stop.At),
            })).As()
            select unit).As()
        select unit;
}
