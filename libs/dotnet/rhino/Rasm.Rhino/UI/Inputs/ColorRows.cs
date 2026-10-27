using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rhino.Display;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ColorRamp(Seq<ColorStop> Stops, bool Stepped) {
    public System.Drawing.Color At(double position) =>
        Stops.Filter(stop => stop.Position <= position).Last.Match(
            Some: before => Stepped
                ? before.Color
                : Stops.Filter(stop => stop.Position > position).Head.Match(
                    Some: after => (after.Position - before.Position) switch {
                        var span when span > 0d => new Color4f(before.Color).BlendTo((float)((position - before.Position) / span), new Color4f(after.Color)).AsSystemColor(),
                        _ => before.Color,
                    },
                    None: () => before.Color),
            None: () => Stops.Head.Map(static stop => stop.Color).IfNone(System.Drawing.Color.Transparent));

    public ColorRamp WithStop(int index, ColorStop stop) =>
        this with { Stops = toSeq(Stops.Map((held, at) => at == index ? stop : held).OrderBy(static held => held.Position)) };

    public ColorRamp Added(double position) =>
        this with { Stops = toSeq(Stops.Add(new ColorStop(At(position), position)).OrderBy(static held => held.Position)) };

    public ColorRamp Removed(int index) => this with { Stops = Stops.Take(index) + Stops.Skip(index + 1) };
}

internal sealed record RampShown(ColorRamp Ramp) {
    private const float Marker = 6f;

    public Option<int> Picked { get; init; }

    public Option<ColorStop> Stop => Picked.Bind(at => Ramp.Stops.At(at));

    // --- [GEOMETRY]
    public static double Position(PointF at, float width) => double.Clamp(at.X / float.Max(width, 1f), 0d, 1d);

    public Option<int> Nearest(PointF at, float width) =>
        Ramp.Stops.Map((stop, index) => (Index: index, Distance: double.Abs(stop.Position - Position(at, width)) * width)).Filter(static held => held.Distance <= Marker * 2f) switch {
            var near => near.Head.Map(_ => near.MinBy(static held => held.Distance).Index),
        };

    // --- [PAINT]
    public void Paint(Graphics graphics, RectangleF bounds) {
        RectangleF band = new(0f, 0f, bounds.Width, float.Max(bounds.Height - (Marker * 2f), 1f));
        foreach ((ColorStop from, ColorStop to) in Ramp.Stops.Zip(Ramp.Stops.Skip(1), static (from, to) => (from, to))) {
            RectangleF segment = new((float)from.Position * band.Width, 0f, (float)(to.Position - from.Position) * band.Width, band.Height);
            if (Ramp.Stepped) {
                graphics.FillRectangle(from.Color.ToEto(), segment);
            } else {
                using LinearGradientBrush brush = new(from.Color.ToEto(), to.Color.ToEto(), segment.TopLeft, segment.TopRight);
                graphics.FillRectangle(brush, segment);
            }
        }
        _ = Ramp.Stops.Head.Iter(first => graphics.FillRectangle(first.Color.ToEto(), new RectangleF(0f, 0f, (float)first.Position * band.Width, band.Height)));
        _ = Ramp.Stops.Last.Iter(last => graphics.FillRectangle(last.Color.ToEto(), new RectangleF((float)last.Position * band.Width, 0f, (float)(1d - last.Position) * band.Width, band.Height)));
        foreach ((ColorStop stop, int index) in Ramp.Stops.Map(static (stop, index) => (stop, index))) {
            float x = (float)stop.Position * band.Width;
            PointF[] marker = [new(x, band.Bottom), new(x - Marker, band.Bottom + (Marker * 2f)), new(x + Marker, band.Bottom + (Marker * 2f))];
            graphics.FillPolygon(Picked == Some(index) ? SystemColors.Highlight : SystemColors.ControlText, marker);
        }
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ColorRows {
    // --- [SWATCHES]
    public static ControlRow Swatch(string label, Color4f color, Func<Edit<Color4f>, IO<Unit>> edited) =>
        new ControlRow.Bound<(Color4f Color, Func<Edit<Color4f>, IO<Unit>> Edited)>(label, RowShape.Inline, (color, edited), static (latest, scope) =>
            from picker in Picker(edit => latest.Bind(row => row.Row.Edited(edit)), scope.Owner)
            let well = new Well(latest.Map(static row => Some(row.Row.Color)), edit => latest.Bind(row => row.Row.Edited(edit)))
            select new RowCells(picker.Control, latest.Bind(row => picker.Show(row.Row.Color))) { Menu = Menu(IO.pure(Some(well))) });

    public static ControlRow Swatches(string label, Seq<Color4f> colors, Func<Edit<Seq<Color4f>>, IO<Unit>> edited) =>
        new ControlRow.Bound<(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited)>(label, RowShape.Block, (colors, edited), static (latest, scope) =>
            from picked in atomIO(Option<int>.None)
            from owned in use(static () => EnvIO.New(token: CancellationToken.None))
            from seated in IO.lift(static () => Ref(Option<Lease<Unit>>.None))
            from strip in Containers.Frame(static () => new StackLayout { Orientation = Orientation.Horizontal, Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table).Width })
            let selected = from row in latest from held in picked.ValueIO select held.Filter(at => at < row.Row.Colors.Count)
            let shown =
                from row in latest
                from reseated in Products.Cleared(seated).Bind(_ => Products.Swapped(seated,
                    from pickers in row.Row.Colors.Map(static (color, index) => (Color: color, Index: index)).TraverseM(held =>
                        from picker in Picker(edit => latest.Bind(current => current.Row.Edited(edit.Mapped(next => current.Row.Colors.Map((kept, at) => at == held.Index ? next : kept)))), scope.Owner)
                        from focused in Subscriptions.Host<EventArgs>(h => picker.Control.GotFocus += h, h => picker.Control.GotFocus -= h).Inline(_ => picked.SwapIO(_ => Some(held.Index)).Map(unit), scope.Owner)
                        from first in picker.Show(held.Color)
                        select picker.Control).As()
                    from released in Products.Released(strip.Items.Clear)
                    from added in IO.lift(() => pickers.Iter(control => strip.Items.Add(control)))
                    select unit)).Under(owned)
                select unit
            let add = new Command<Invoke>.Run(new CommandFace("color-add", "Add Color"), None, new Invoke.Effect(_ =>
                from row in latest
                from held in selected
                let next = (held.Bind(index => row.Row.Colors.At(index)) | row.Row.Colors.Last).Map(row.Row.Colors.Add).IfNone(row.Row.Colors.Add(new Color4f(1f, 1f, 1f, 1f)))
                from edited in row.Row.Edited(Edits.Commit(next))
                from chosen in picked.SwapIO(_ => Some(next.Count - 1))
                from redrawn in shown
                select unit))
            let remove = new Command<Invoke>.Run(new CommandFace("color-remove", "Remove Color"), Some(new RowRule(_ => selected.Map(static at => at.IsSome))), new Invoke.Effect(_ =>
                from row in latest
                from held in selected
                from removed in held.Traverse(index =>
                    from edited in row.Row.Edited(Edits.Commit(row.Row.Colors.Take(index) + row.Row.Colors.Skip(index + 1)))
                    from chosen in picked.SwapIO(_ => Some(int.Min(index, row.Row.Colors.Count - 2)).Filter(static kept => kept >= 0))
                    select unit).As()
                from redrawn in shown
                select unit))
            from pair in ButtonRows.AddRemove(add, remove, scope)
            from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(pair.Control, HorizontalAlignment.Left), new StackLayoutItem(strip, HorizontalAlignment.Stretch)) {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            })
            select new RowCells(body, shown.Bind(_ => pair.Refresh)) {
                Menu = Menu(selected.Map(at => at.Map(index => Indexed(latest, index)))),
            });

    private static Well Indexed(IO<ControlRow.Bound<(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited)>> latest, int index) =>
        new(latest.Map(row => row.Row.Colors.At(index)),
            edit => latest.Bind(row => row.Row.Edited(edit.Mapped(color => row.Row.Colors.Map((held, at) => at == index ? color : held)))));

    private static IO<(ColorPicker Control, Func<Color4f, IO<Unit>> Show)> Picker(Func<Edit<Color4f>, IO<Unit>> edited, IPlugInLifetime owner) =>
        from picker in use(static () => new ColorPicker { AllowAlpha = true })
        from change in ControlChange<EventArgs>.Attached(
            Subscriptions.Host<EventArgs>(h => picker.ValueChanged += h, h => picker.ValueChanged -= h),
            _ => edited(Edits.Commit(new Color4f(picker.Value.ToSystemDrawing()))),
            owner)
        select (picker, (Func<Color4f, IO<Unit>>)(color => change.Detached(IO.lift(() => { picker.Value = color.AsSystemColor().ToEto(); }))));

    // --- [GRADIENTS]
    public static ControlRow Gradient(string label, ColorRamp ramp, Func<Edit<ColorRamp>, IO<Unit>> edited) =>
        new ControlRow.Bound<(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited)>(label, RowShape.Block, (ramp, edited), static (latest, scope) =>
            from first in latest
            from strip in Surfaces.Of(new RampShown(first.Row.Ramp), static (graphics, bounds, shown) => shown.Paint(graphics, bounds), scope.Owner)
            from sized in IO.lift(() => { strip.Height = 28; })
            let well = Stopping(latest, strip)
            from swatch in Picker(edit => well.Edited(edit), scope.Owner)
            from position in NumberFields.Scalar(new NumberRange(0d, 1d) { Step = Some(0.01d), Decimals = Some(2) }, edit => Positioned(latest, strip, edit), scope.Owner)
            let shown =
                from row in latest
                from drawn in strip.Show(strip.State with { Ramp = row.Row.Ramp })
                let chosen = strip.State.Stop
                from enabled in IO.lift(() => { (swatch.Control.Enabled, position.Control.Enabled) = (chosen.IsSome, chosen.IsSome); })
                from colored in chosen.Traverse(stop => swatch.Show(new Color4f(stop.Color))).As()
                from placed in chosen.Traverse(stop => position.Show(stop.Position)).As()
                select unit
            from pressed in Subscriptions.Host<MouseEventArgs>(h => strip.MouseDown += h, h => strip.MouseDown -= h).Inline(args =>
                from row in latest
                let picked = strip.State.Nearest(args.Location, strip.Width)
                from chosen in strip.Show(strip.State with { Picked = picked })
                from added in when(picked.IsNone && args.Buttons is MouseButtons.Primary, row.Row.Edited(Edits.Commit(row.Row.Ramp.Added(RampShown.Position(args.Location, strip.Width))))).As()
                from redrawn in shown
                select unit, scope.Owner)
            from dragged in Subscriptions.Host<MouseEventArgs>(h => strip.MouseMove += h, h => strip.MouseMove -= h).Inline(args =>
                when(args.Buttons is MouseButtons.Primary, Positioned(latest, strip, Edits.Preview(RampShown.Position(args.Location, strip.Width))).Bind(_ => shown)).As(), scope.Owner)
            from released in Subscriptions.Host<MouseEventArgs>(h => strip.MouseUp += h, h => strip.MouseUp -= h).Inline(args =>
                when(args.Buttons is MouseButtons.Primary, Positioned(latest, strip, Edits.Commit(RampShown.Position(args.Location, strip.Width))).Bind(_ => shown)).As(), scope.Owner)
            from keyed in Subscriptions.During<KeyEventArgs>(h => strip.KeyDown += h, h => strip.KeyDown -= h).Inline(args =>
                when(args.Key is Keys.Delete or Keys.Backspace,
                    from row in latest
                    from removed in strip.State.Picked.Filter(_ => row.Row.Ramp.Stops.Count > 1).Traverse(index => row.Row.Edited(Edits.Commit(row.Row.Ramp.Removed(index)))).As()
                    from cleared in strip.Show(strip.State with { Picked = None })
                    from handled in IO.lift(() => { args.Handled = true; })
                    from redrawn in shown
                    select unit).As(), scope.Owner)
            from popup in ChoiceRows.Popup(
                Seq(new ChoiceItem<bool>(Value: false, RowText.Localize("Smooth", scope.Owner).Local), new ChoiceItem<bool>(Value: true, RowText.Localize("Stepped", scope.Owner).Local)),
                stepped => latest.Bind(row => row.Row.Edited(Edits.Commit(row.Row.Ramp with { Stepped = stepped }))), scope.Owner)
            from line in RowGrid.Lined([new TableCell(swatch.Control), new TableCell(position.Control, scaleWidth: true)])
            select new RowCells(popup.Control, latest.Bind(row => popup.Show(row.Row.Ramp.Stepped)).Bind(_ => shown)) {
                Lines = Seq(new RowLine(None, strip), new RowLine(Some("Stop"), line)),
                Menu = Menu(IO.pure(Some(well))),
            });

    private static Well Stopping(IO<ControlRow.Bound<(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited)>> latest, Surface<RampShown> strip) =>
        new(IO.lift(() => strip.State.Stop.Map(static stop => new Color4f(stop.Color))),
            edit => from row in latest
                    from edited in strip.State.Picked.Traverse(at => row.Row.Edited(edit.Mapped(color => row.Row.Ramp.WithStop(at, new ColorStop(color.AsSystemColor(), row.Row.Ramp.Stops[at].Position))))).As()
                    select unit);

    private static IO<Unit> Positioned(IO<ControlRow.Bound<(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited)>> latest, Surface<RampShown> strip, Edit<double> edit) =>
        from row in latest
        from moved in strip.State.Picked.Traverse(at =>
            from edited in row.Row.Edited(edit.Mapped(value => row.Row.Ramp.WithStop(at, new ColorStop(row.Row.Ramp.Stops[at].Color, value))))
            from reselected in edit.Held.Traverse(value => strip.Show(strip.State with {
                Picked = Some(row.Row.Ramp.WithStop(at, new ColorStop(row.Row.Ramp.Stops[at].Color, value)).Stops
                    .Map(static (stop, index) => (stop.Position, Index: index)).Find(stop => stop.Position == value).Map(static stop => stop.Index).IfNone(at)),
            })).As()
            select unit).As()
        select unit;

    // --- [WELLS]
    private sealed record Well(IO<Option<Color4f>> Stored, Func<Edit<Color4f>, IO<Unit>> Edited);

    private static IO<Unit> Dialog(Well well, RowScope scope) =>
        from stored in well.Stored
        from opened in stored.Traverse(current =>
            from document in scope.DocumentIO
            from edit in HostDialogs.ShowColorDialog(document, current, allowAlpha: true, None, Some<(Func<Color4f, IO<Unit>> Deliver, IPlugInLifetime Owner)>((color => well.Edited(Edits.Preview(color)), scope.Owner)))
                .Map(static chosen => Edits.Commit(chosen))
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(Edits.Cancel<Color4f>()))
            from edited in well.Edited(edit)
            select unit).As()
        select unit;

    private static Seq<Seq<Command<Invoke>>> Menu(IO<Option<Well>> held) =>
        new RowRule(_ => Stored(held).Map(static current => current.IsSome)) switch {
            var stored => [Seq<Command<Invoke>>(
                new Command<Invoke>.Run(new CommandFace("color-picker", "Color Picker…"), Some(stored),
                    new Invoke.Effect(scope => held.Bind(well => well.Traverse(current => Dialog(current, scope)).As()).Map(unit))),
                new Command<Invoke>.Run(new CommandFace("color-named", "Named Colors"), Some(stored),
                    new Invoke.Listed(_ => Listed(held))))],
        };

    private static IO<Seq<Seq<Command<Invoke>>>> Listed(IO<Option<Well>> held) =>
        from well in held
        from current in Stored(held)
        select Seq(well.Map(shown => Named(shown, current)).IfNone(Seq<Command<Invoke>>()));

    private static IO<Option<Color4f>> Stored(IO<Option<Well>> held) =>
        held.Bind(static well => well.Traverse(static shown => shown.Stored).As()).Map(static current => current.Flatten());

    private static Seq<Command<Invoke>> Named(Well well, Option<Color4f> stored) =>
        toSeq(NamedColorList.Default).Map<Command<Invoke>>(named => CommandRows.Picked(
            new CommandFace(named.Name, named.Name),
            stored.Exists(held => Conversions.Same(held.AsSystemColor(), named.Color)),
            _ => well.Edited(Edits.Commit(new Color4f(named.Color)))));

    // --- [PAIRS]
    public static ControlRow DisplayPrint(
        string label, ((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Display, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Print) value,
        HashSet<DisplayAndPrintColorSourceMode> offered, Option<(System.Drawing.Color Display, System.Drawing.Color Print)> resolved,
        Func<Edit<((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Display, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Print)>, IO<Unit>> edited) =>
        new ControlRow.Bound<(
            ((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Display, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Print) Value,
            HashSet<DisplayAndPrintColorSourceMode> Offered, Option<(System.Drawing.Color Display, System.Drawing.Color Print)> Resolved,
            Func<Edit<((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Display, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) Print)>, IO<Unit>> Edited)>(
            label, RowShape.Inline, (value, offered, resolved, edited), static (latest, scope) =>
                from picker in use(static () => new DisplayAndPrintColorPicker())
                from row in latest
                from configured in IO.lift(() => {
                    (picker.PickerMode, picker.ShowByLayer, picker.ShowByObject, picker.ShowByParent, picker.ShowViewport, picker.ShowNone) = (
                        DisplayAndPrintColorPickerMode.Dual,
                        row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.ByLayer), row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.ByObject),
                        row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.ByParent), row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.Viewport),
                        row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.None));
                })
                from change in ControlChange<PropertyChangedEventArgs>.Attached(
                    Subscriptions.Host<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => picker.PropertyChanged += h, h => picker.PropertyChanged -= h, static handler => handler.Invoke),
                    _ =>
                        from current in latest
                        from read in IO.lift(() => (Display: (picker.DisplaySourceMode, picker.DisplayColor.ToSystemDrawing()), Print: (picker.PrintSourceMode, picker.PrintColor.ToSystemDrawing())))
                        from committed in when(!Same(current.Row.Value.Display, read.Display) || !Same(current.Row.Value.Print, read.Print),
                            current.Row.Edited(Edits.Commit(read))).As()
                        select unit,
                    scope.Owner)
                select new RowCells(picker, latest.Bind(current => change.Detached(IO.lift(() => {
                    (picker.DisplaySourceMode, picker.DisplayColor, picker.PrintSourceMode, picker.PrintColor) =
                        (current.Row.Value.Display.Source, current.Row.Value.Display.Color.ToEto(), current.Row.Value.Print.Source, current.Row.Value.Print.Color.ToEto());
                    picker.LinkPrintToDisplay = Same(current.Row.Value.Display, current.Row.Value.Print);
                    _ = current.Row.Resolved.Iter(held => (picker.DisplayResolvedColor, picker.PrintResolvedColor) = (held.Display.ToEto(), held.Print.ToEto()));
                })))));

    private static bool Same((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) left, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) right) =>
        left.Source == right.Source && Conversions.Same(left.Color, right.Color);
}
