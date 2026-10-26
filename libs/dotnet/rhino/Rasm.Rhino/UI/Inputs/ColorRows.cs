using System.ComponentModel;
using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.Display;
using Rhino.UI;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ColorRows {
    // --- [SWATCHES]
    public static ControlRow Swatch(string label, Color4f color, Func<Edit<Color4f>, IO<Unit>> edited) =>
        new ControlRow.Bound<(Color4f Color, Func<Edit<Color4f>, IO<Unit>> Edited)>(label, RowShape.Inline, (color, edited), static (latest, scope, retain) =>
            from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatch))
            let well = new Well(latest.Map(static row => Some(row.Row.Color)), edit => latest.Bind(row => row.Row.Edited(edit)))
            select new RowCells(view, latest.Bind(row => view.Show(Faced(row.Row.Color, RowText.Localize(row.Label, scope.Sink).Local, _ => Dialog(well, scope, site))))) {
                Menu = Menu(IO.pure(well), site),
            });

    public static ControlRow Swatches(string label, Seq<Color4f> colors, Func<Edit<Seq<Color4f>>, IO<Unit>> edited) =>
        new ControlRow.Bound<(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited)>(label, RowShape.Block, (colors, edited), static (latest, scope, retain) =>
            from picked in atomIO<Selection>(new Selection.One(None))
            from view in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatches))
            let selected = from row in latest from held in picked.ValueIO select Index(held).Filter(at => at < row.Row.Colors.Count)
            let shown =
                from row in latest
                from held in picked.ValueIO
                from drew in view.Show(new SwatchList(row.Row.Colors.Map(static color => color.ToSKColorF()), held, edit => Chosen(latest, picked, edit, scope, site)).Drawn switch {
                    var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = RowText.Localize(row.Label, scope.Sink).Local }) },
                })
                select unit
            let add = new Command<RowRule, Invoke>.Run(new CommandFace("color-add", "Add Color", None, None), None, new Invoke.Effect(_ =>
                from held in picked.ValueIO
                from revised in Revised(latest, picked, held, colors => (Index(held).Bind(index => colors.At(index)) | colors.Last).Map(colors.Add).IfNone(colors.Add(new Color4f(1f, 1f, 1f, 1f))),
                    static colors => Some(colors.Count - 1))
                from redrawn in shown
                select unit))
            let remove = new Command<RowRule, Invoke>.Run(new CommandFace("color-remove", "Remove Color", None, None), Some(new RowRule(_ => selected.Map(static at => at.IsSome))),
                new Invoke.Effect(_ =>
                    from held in picked.ValueIO
                    from at in selected
                    from revised in at.Traverse(index => Revised(latest, picked, held, colors => colors.Take(index) + colors.Skip(index + 1),
                        colors => Some(int.Min(index, colors.Count - 1)).Filter(static kept => kept >= 0))).As()
                    from redrawn in shown
                    select unit))
            from pair in ButtonRows.AddRemove(add, remove, scope, retain)
            from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(pair.Control, HorizontalAlignment.Left), new StackLayoutItem(view, HorizontalAlignment.Stretch)) {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }, retain)
            select new RowCells(body, shown.Bind(_ => pair.Refresh)) {
                Menu = Menu(selected.Map(at => at.Map(index => Indexed(latest, index)).IfNone(Unpicked)), site),
            });

    private static IO<Unit> Chosen(IO<ControlRow.Bound<(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited)>> latest, Atom<Selection> picked, Edit<Selection> edit, RowScope scope, CallbackSite site) =>
        from moved in edit.Shown.Traverse(next => picked.SwapIO(_ => next)).As()
        from opened in (from next in edit.Applied from index in Index(next) select Dialog(Indexed(latest, index), scope, site)).Traverse(static open => open).As()
        select unit;

    private static IO<Unit> Revised(
        IO<ControlRow.Bound<(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited)>> latest, Atom<Selection> picked, Selection held,
        Func<Seq<Color4f>, Seq<Color4f>> change, Func<Seq<Color4f>, Option<int>> pick) =>
        from row in latest
        let next = change(row.Row.Colors)
        from edited in row.Row.Edited(new Edit<Seq<Color4f>>.Commit(next))
        from chosen in picked.SwapIO(_ => pick(next).Map(static index => (Selection)Picked(index)).IfNone(held))
        select unit;

    private static Well Indexed(IO<ControlRow.Bound<(Seq<Color4f> Colors, Func<Edit<Seq<Color4f>>, IO<Unit>> Edited)>> latest, int index) =>
        new(latest.Map(row => row.Row.Colors.At(index)),
            edit => latest.Bind(row => row.Row.Edited(edit.Map(color => row.Row.Colors.Map((held, at) => at == index ? color : held)).As())));

    // --- [GRADIENTS]
    public static ControlRow Gradient(string label, ColorRamp ramp, Func<Edit<ColorRamp>, IO<Unit>> edited) =>
        new ControlRow.Bound<(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited)>(label, RowShape.Block, (ramp, edited), static (latest, scope, retain) =>
            from picked in atomIO<Selection>(new Selection.One(None))
            let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Gradient))
            from editor in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            from stop in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
            from position in DisposalOps.Acquire(IO.lift(() => new NumberField<float>(new ScalarText(new Axis.Linear(AxisRange.Unit), Some(0.01f), None, None, RowText.Culture), (0f, 1f),
                edit => Positioned(latest, picked, edit), scope.Sink)), retain)
            let well = Stopping(latest, picked)
            let shown =
                from row in latest
                from held in picked.ValueIO
                let caption = RowText.Localize(row.Label, scope.Sink).Local
                let chosen = Index(held).Bind(at => row.Row.Ramp.Stops.At(at))
                from drew in editor.Show(new GradientEditor(row.Row.Ramp, row.Row.Edited) {
                    Picked = held,
                    Pick = Some<Func<Edit<Selection>, IO<Unit>>>(edit => edit.Shown.Traverse(next => picked.SwapIO(_ => next)).As().Map(unit)),
                }.Drawn switch {
                    var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = caption }) },
                })
                from swatch in stop.Show(Faced(chosen.Map(static held => Hosted(held.Color)).IfNone(new Color4f(0f, 0f, 0f, 0f)), caption, _ => Dialog(well, scope, site)))
                from enabled in IO.lift(() => { position.Enabled = chosen.IsSome; })
                from received in chosen.Traverse(held => position.Receive((float)held.At)).As()
                select unit
            from popup in ChoiceRows.Popup(
                Seq(new ChoiceItem<bool>(Value: false, RowText.Localize("Smooth", scope.Sink).Local, None), new ChoiceItem<bool>(Value: true, RowText.Localize("Stepped", scope.Sink).Local, None)),
                stepped => latest.Bind(row => row.Row.Edited(new Edit<ColorRamp>.Commit(Cast(row.Row.Ramp, stepped)))), scope.Sink, retain)
            from line in RowGrid.Lined([new TableCell(stop, scaleWidth: true), new TableCell(position)], retain)
            select new RowCells(popup.Control, latest.Bind(row => popup.Show(row.Row.Ramp is ColorRamp.Stepped)).Bind(_ => shown)) {
                Lines = Seq(new RowLine(None, editor), new RowLine(Some("Stop"), line)),
                Menu = Menu(IO.pure(well), site),
            });

    private static ColorRamp Cast(ColorRamp ramp, bool stepped) =>
        ramp.Switch(stepped, smooth: static (step, smooth) => step ? new ColorRamp.Stepped(smooth.Stops) : (ColorRamp)smooth, stepped: static (step, held) => step ? held : new ColorRamp.Smooth(held.Stops));

    private static Well Stopping(IO<ControlRow.Bound<(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited)>> latest, Atom<Selection> picked) =>
        new(from row in latest from held in picked.ValueIO select Index(held).Bind(at => row.Row.Ramp.Stops.At(at)).Map(static stop => Hosted(stop.Color)),
            edit => from row in latest
                    from held in picked.ValueIO
                    from edited in Index(held).Traverse(at => row.Row.Edited(edit.Map(color => row.Row.Ramp.WithStops(
                        row.Row.Ramp.Stops.Map((kept, index) => index == at ? kept with { Color = color.ToSKColorF() } : kept))).As())).As()
                    select unit);

    private static IO<Unit> Positioned(IO<ControlRow.Bound<(ColorRamp Ramp, Func<Edit<ColorRamp>, IO<Unit>> Edited)>> latest, Atom<Selection> picked, Edit<float> edit) =>
        from row in latest
        from held in picked.ValueIO
        from moved in Index(held).Traverse(at =>
            from edited in row.Row.Edited(edit.Map(value => Moved(row.Row.Ramp, at, value).Ramp).As())
            from reselected in edit.Shown.Traverse(value => picked.SwapIO(_ => Picked(Moved(row.Row.Ramp, at, value).At))).As()
            select unit).As()
        select unit;

    private static (ColorRamp Ramp, int At) Moved(ColorRamp ramp, int at, float value) =>
        ramp.Stops.Map((stop, index) => (Stop: index == at && Fraction.TryCreate(value, out Fraction? moved) ? stop with { At = moved } : stop, Moved: index == at)).Strict() switch {
            var stops => toSeq(stops.OrderBy(static held => (float)held.Stop.At)) switch {
                var ordered => (ramp.WithStops(ordered.Map(static held => held.Stop)), ordered.Map(static (held, index) => (held.Moved, index)).Find(static held => held.Moved).Map(static held => held.index).IfNone(at)),
            },
        };

    // --- [WELLS]
    private sealed record Well(IO<Option<Color4f>> Stored, Func<Edit<Color4f>, IO<Unit>> Edited);

    private static readonly Well Unpicked = new(IO.pure(Option<Color4f>.None), static _ => IO.pure(unit));

    private static Option<int> Index(Selection picked) => picked.Lead.Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None);

    private static Selection.One Picked(int index) => new(Some<Key>(new Key.Ordinal((uint)index)));

    private static Color4f Hosted(SKColorF color) => new(color.Red, color.Green, color.Blue, color.Alpha);

    private static Node Faced(Color4f color, string caption, Func<SKRect, IO<Unit>> pressed) =>
        new Swatch(color.ToSKColorF(), pressed).Drawn switch {
            var face => face with { Accessible = face.Accessible.Combine(new Accessible { Label = caption }) },
        };

    private static IO<Unit> Dialog(Well well, RowScope scope, CallbackSite site) =>
        (from stored in well.Stored
         from opened in stored.Traverse(current =>
             from document in scope.DocumentIO
             from taken in DisposalOps.OnFailure(
                 HostDialogs.ShowColorDialog(document, current, allowAlpha: true, None,
                         Some<(Func<Color4f, IO<Unit>> Deliver, CallbackSite Site)>((color => well.Edited(new Edit<Color4f>.Preview(color)), site)))
                     .Bind(chosen => well.Edited(new Edit<Color4f>.Commit(chosen))),
                 well.Edited(new Edit<Color4f>.Cancel()))
             select unit).As()
         select unit)
        .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));

    private static Seq<Seq<Command<RowRule, Invoke>>> Menu(IO<Well> held, CallbackSite site) =>
        new RowRule(_ => held.Bind(static well => well.Stored.Map(static current => current.IsSome))) switch {
            var stored => [Seq<Command<RowRule, Invoke>>(
                new Command<RowRule, Invoke>.Run(new CommandFace("color-picker", "Color Picker…", None, None), Some(stored),
                    new Invoke.Effect(scope => held.Bind(well => Dialog(well, scope, site)))),
                new Command<RowRule, Invoke>.Run(new CommandFace("color-named", "Named Colors", None, None), Some(stored),
                    new Invoke.Listed(_ => held.Bind(static well => well.Stored.Map(current => Seq(Named(well, current)))))))],
        };

    private static Seq<Command<RowRule, Invoke>> Named(Well well, Option<Color4f> stored) =>
        toSeq(NamedColorList.Default).Map<Command<RowRule, Invoke>>(named => CommandRows.Picked(
            new CommandFace(named.Name, named.Name, None, None),
            stored.Exists(held => Conversions.Same(held.AsSystemColor(), named.Color)),
            _ => well.Edited(new Edit<Color4f>.Commit(new Color4f(named.Color)))));

    // --- [PAIRS]
    public static ControlRow DisplayPrint(
        string label, ((DisplayAndPrintColorSourceMode Source, Color Color) Display, (DisplayAndPrintColorSourceMode Source, Color Color) Print) value,
        LanguageExt.HashSet<DisplayAndPrintColorSourceMode> offered, Option<(Color Display, Color Print)> resolved,
        Func<Edit<((DisplayAndPrintColorSourceMode Source, Color Color) Display, (DisplayAndPrintColorSourceMode Source, Color Color) Print)>, IO<Unit>> edited) =>
        new ControlRow.Bound<(
            ((DisplayAndPrintColorSourceMode Source, Color Color) Display, (DisplayAndPrintColorSourceMode Source, Color Color) Print) Value,
            LanguageExt.HashSet<DisplayAndPrintColorSourceMode> Offered, Option<(Color Display, Color Print)> Resolved,
            Func<Edit<((DisplayAndPrintColorSourceMode Source, Color Color) Display, (DisplayAndPrintColorSourceMode Source, Color Color) Print)>, IO<Unit>> Edited)>(
            label, RowShape.Inline, (value, offered, resolved, edited), static (latest, scope, retain) =>
                from picker in DisposalOps.Acquire(IO.lift(static () => new DisplayAndPrintColorPicker()), retain)
                from row in latest
                from configured in IO.lift(() => {
                    (picker.PickerMode, picker.ShowByLayer, picker.ShowByObject, picker.ShowByParent, picker.ShowViewport, picker.ShowNone) = (
                        DisplayAndPrintColorPickerMode.Dual,
                        row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.ByLayer), row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.ByObject),
                        row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.ByParent), row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.Viewport),
                        row.Row.Offered.Contains(DisplayAndPrintColorSourceMode.None));
                })
                let handler = new PropertyChangedEventHandler(Callbacks.Handler<PropertyChangedEventArgs>(_ =>
                    from current in latest
                    from read in IO.lift(() => (Display: (picker.DisplaySourceMode, picker.DisplayColor.ToSystemDrawing()), Print: (picker.PrintSourceMode, picker.PrintColor.ToSystemDrawing())))
                    from committed in when(!Same(current.Row.Value.Display, read.Display) || !Same(current.Row.Value.Print, read.Print),
                        current.Row.Edited(new Edit<((DisplayAndPrintColorSourceMode, Color), (DisplayAndPrintColorSourceMode, Color))>.Commit(read))).As()
                    select unit, new CallbackSite(scope.Sink, typeof(DisplayAndPrintColorPicker), nameof(DisplayAndPrintColorPicker.PropertyChanged))).Invoke)
                from attached in Subscriptions.Attach<PropertyChangedEventHandler>(h => picker.PropertyChanged += h, h => picker.PropertyChanged -= h, handler, retain)
                select new RowCells(picker, latest.Bind(current => Subscriptions.Detached<PropertyChangedEventHandler, Unit>(
                    h => picker.PropertyChanged += h, h => picker.PropertyChanged -= h, handler, IO.lift(() => {
                        (picker.DisplaySourceMode, picker.DisplayColor, picker.PrintSourceMode, picker.PrintColor) =
                            (current.Row.Value.Display.Source, current.Row.Value.Display.Color.ToEto(), current.Row.Value.Print.Source, current.Row.Value.Print.Color.ToEto());
                        picker.LinkPrintToDisplay = Same(current.Row.Value.Display, current.Row.Value.Print);
                        _ = current.Row.Resolved.Iter(held => (picker.DisplayResolvedColor, picker.PrintResolvedColor) = (held.Display.ToEto(), held.Print.ToEto()));
                    })))));

    private static bool Same((DisplayAndPrintColorSourceMode Source, Color Color) left, (DisplayAndPrintColorSourceMode Source, Color Color) right) =>
        left.Source == right.Source && Conversions.Same(left.Color, right.Color);
}
