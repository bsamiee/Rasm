using System.ComponentModel;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Drawing;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Display;
using Rhino.UI;
using Rhino.UI.Controls;
using Wacton.Unicolour;

namespace Rasm.Rhino.Render.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SwatchEncoding {
    public abstract bool Alpha { get; }
    public abstract Color4f Shown(Vector4 stored);
    public abstract Vector4 Stored(Color4f shown);

    public sealed record DisplayReferred : SwatchEncoding {
        public override bool Alpha => false;
        public override Color4f Shown(Vector4 stored) => new(stored.X, stored.Y, stored.Z, 1f);
        public override Vector4 Stored(Color4f shown) => new(shown.R, shown.G, shown.B, 1f);
    }

    public sealed record SceneLinear(Gamut Working) : SwatchEncoding {
        public override bool Alpha => true;

        public override Color4f Shown(Vector4 stored) =>
            Plots.Encoded(Working, stored.AsVector3()) switch {
                var color => new Color4f(color.R, color.G, color.B, stored.W),
            };

        public override Vector4 Stored(Color4f shown) =>
            new Unicolour(Configuration.Default, ColourSpace.Rgb, shown.R, shown.G, shown.B, shown.A).ConvertToConfiguration(Working.Configuration) switch {
                var color => new((float)color.RgbLinear.R, (float)color.RgbLinear.G, (float)color.RgbLinear.B, (float)color.Alpha.A),
            };
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ColorRows {
    // --- [SWATCHES]
    private const int SwatchSide = 24;
    private const float FrameWidth = 1f;
    private const float WellWidth = 3f;
    private const float RimWidth = 1f;
    private const string PickCaption = "Pick Color";

    public static ControlRow SwatchColor<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Swatch> lens, Option<StageInput> stage, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Swatched(source, field, lens, Option<Lens<TRecord, bool>>.None, stage, scope, retain), rules);

    public static ControlRow SwatchColor<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<Swatch>> lens, Option<StageInput> stage, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline,
            (scope, retain) => Swatched(source, field, Prelude.lens(lens, Gated<Swatch>.ValueEntry.Lens), Some(Prelude.lens(lens, Gated<Swatch>.EnabledEntry.Lens)), stage, scope, retain), rules);

    public static ControlRow Swatches<TRecord, TValue>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Option<StageInput> stage, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>> =>
        ControlRow.Of(source, field, RowShape.Block, (scope, retain) => Stripped(source, field, lens, stage, scope, retain), rules);

    private static IO<RowCells> Swatched<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Swatch> swatch, Option<Lens<TRecord, bool>> gate, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from well in Welled(new SwatchEncoding.DisplayReferred(), new Size(-1, SwatchSide), IO.pure(unit), scope.Sink, retain)
        from box in gate.Traverse(_ => DisposalOps.Acquire(IO.lift(static () => new CheckBox()), retain)).As()
        let gated = from member in gate from shown in box select (Lens: member, Box: shown)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(SwatchColor))
        let write = Written(swatch)
        from bound in RowEdit.Bind(source, IterableNE.create(field), h => well.Dialog.Executed += h, h => well.Dialog.Executed -= h,
                edit => Opened(well, edit, write, scope, site), value => Shown(well, swatch, gated, value), scope, site, retain)
        from gating in gated.Traverse(pair => Gating(source, bound, pair.Lens, pair.Box, scope, site, retain)).As()
        from pick in stage.Traverse(input => Pick(input, bound, write, scope, retain)).As()
        select new RowCells(
            well.Button, box.Map(static shown => (Control)shown), pick.Map(static button => (Control)button), None, [], [],
            new RowHelp(None, Some(Conversions.Hex(swatch.Get(source.Default)))), bound.Shown, Some<RowEdit>(bound),
            Some(ChoiceRows.Context(well.Button, Menu(well, bound, write), [], scope.Sink)));

    private static Func<Vector4, TRecord, Fin<TRecord>> Written<TRecord>(Lens<TRecord, Swatch> swatch) where TRecord : notnull =>
        (color, record) => Fin.Succ(swatch.Set(Swatch.From(color), record));

    private static IO<Unit> Shown<TRecord>(Well well, Lens<TRecord, Swatch> swatch, Option<(Lens<TRecord, bool> Lens, CheckBox Box)> gated, Option<TRecord> value)
        where TRecord : notnull =>
        (from record in value from pair in gated select pair.Lens.Get(record)) switch {
            var on => IO.lift(() => {
                well.Dialog.Enabled = on.ForAll(static held => held);
                _ = gated.Iter(pair => {
                    pair.Box.ThreeState = value.IsNone;
                    pair.Box.Checked = on.ToNullable();
                });
            }).Bind(_ => Restyled(well, value.Map(record => swatch.Get(record).Display), PaintSlot.FrameEdge)),
        };

    private static IO<Unit> Gating<TRecord>(RowSource<TRecord> source, RowEdit<TRecord> edit, Lens<TRecord, bool> gate, CheckBox box, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull =>
        Subscriptions.Attach(h => box.CheckedChanged += h, h => box.CheckedChanged -= h, Callbacks.Handler<EventArgs>(_ => Toggled(source, edit, gate, box, scope), site), retain);

    private static IO<Unit> Toggled<TRecord>(RowSource<TRecord> source, RowEdit<TRecord> edit, Lens<TRecord, bool> gate, CheckBox box, RowScope scope) where TRecord : notnull =>
        from ticked in IO.lift(() => Optional(box.Checked))
        from held in scope.Read(source)
        from committed in ticked.Filter(state => state != gate.Get(held)).Traverse(state => edit.Commit(gate.Set(state, held))).As()
        select unit;

    private static IO<Button> Pick<TRecord>(StageInput stage, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        PreviewRows.Pick(stage, PickCaption, mean => edit.Take(new Edit<Vector4>.Commit(mean), write).Map(unit), scope, retain);

    private static IO<RowCells> Stripped<TRecord, TValue>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>> =>
        from add in IO.lift(static () => new Command())
        from remove in IO.lift(static () => new Command())
        from named in IO.lift(() => {
            add.ToolTip = RowText.Localize("Add Color").Local;
            remove.ToolTip = RowText.Localize("Remove Color").Local;
            remove.Enabled = false;
        })
        from pair in ButtonRows.AddRemove(add, remove, retain)
        from state in atomIO(new StripState<TRecord>(None, None, 0))
        from wells in IO.lift(static () => Ref(Seq<Well>()))
        from owned in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from panel in DisposalOps.Acquire(IO.lift(() => new Panel { Tag = owned }), value =>
            IO.lift(() => { value.Content = null; value.Dispose(); })
                .Bind(_ => atomicIO(() => wells.Value = []))
                .Bind(_ => owned.Close), retain)
        from body in DisposalOps.Acquire(IO.lift(() => new StackLayout(
                new StackLayoutItem(pair, HorizontalAlignment.Left), new StackLayoutItem(panel)) {
            Orientation = Orientation.Vertical,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
        }), retain)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatches))
        let made = fun((RowEdit<TRecord> edit) => new Strip<TRecord>(
            panel, add, remove, state, wells, owned,
            record => lens.Get(record).ToValue(), (record, change) => Revised(lens, record, change), edit, scope, site))
        from bound in RowEdit.Bind(source, IterableNE.create(field), h => add.Executed += h, h => add.Executed -= h,
                edit => Callbacks.Handler<EventArgs>(_ => Added(made(edit)), site), value => state.SwapIO(held => held with { Shown = value }).Map(unit), scope, site, retain)
        let strip = made(bound)
        from removed in Subscriptions.Attach(h => remove.Executed += h, h => remove.Executed -= h, Callbacks.Handler<EventArgs>(_ => Removed(strip), site), retain)
        from resized in Subscriptions.Attach(h => panel.SizeChanged += h, h => panel.SizeChanged -= h, Callbacks.Handler<EventArgs>(_ => Resized(strip), site), retain)
        from pick in stage.Traverse(input => PreviewRows.Pick(input, PickCaption, mean => Picked(strip, mean), scope, retain)).As()
        select new RowCells(
            body, None, pick.Map(static button => (Control)button), None, [], [],
            new RowHelp(None, Some(string.Join(' ', lens.Get(source.Default).ToValue().Map(Conversions.Hex)))),
            bound.Shown.Bind(_ => Laid(strip)), Some<RowEdit>(bound),
            Some(ChoiceRows.Context(panel, [], [], scope.Sink)));

    private static Fin<TRecord> Revised<TRecord, TValue>(Lens<TRecord, TValue> lens, TRecord record, Func<Seq<Swatch>, Seq<Swatch>> change)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>> =>
        ValidationResults.Validated<TValue, Seq<Swatch>>(change(lens.Get(record).ToValue())).Map(next => lens.Set(next, record));

    private static IO<Unit> Added<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        from held in strip.State.ValueIO
        from taken in strip.Edit.Take(new Edit<Unit>.Commit(unit), (_, record) => strip.Revise(record, list => Copied(list, held.Selected)))
        from last in Last(strip, taken)
        select last;

    private static Seq<Swatch> Copied(Seq<Swatch> list, Option<int> selected) =>
        (selected.Bind(at => list.At(at)) | list.Last).Map(list.Add).IfNone(list);

    private static IO<Unit> Picked<TRecord>(Strip<TRecord> strip, Vector4 mean) where TRecord : notnull =>
        strip.Edit.Take(new Edit<Vector4>.Commit(mean), (color, record) => strip.Revise(record, list => list.Add(Swatch.From(color)))).Bind(records => Last(strip, records));

    private static IO<Unit> Removed<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        strip.State.ValueIO.Bind(held => held.Selected.Traverse(at => Cut(strip, at)).As()).Map(unit);

    private static IO<Unit> Cut<TRecord>(Strip<TRecord> strip, int at) where TRecord : notnull =>
        strip.Edit.Take(new Edit<Unit>.Commit(unit), (_, record) => strip.Revise(record, list => list.Take(at).Concat(list.Skip(at + 1))))
            .Bind(_ => Selected(strip, Some(int.Max(at - 1, 0))));

    private static IO<Unit> Last<TRecord>(Strip<TRecord> strip, Seq<TRecord> records) where TRecord : notnull =>
        records.Head.Traverse(record => Selected(strip, Some(strip.Listed(record).Count - 1))).As().Map(unit);

    private static IO<Unit> Selected<TRecord>(Strip<TRecord> strip, Option<int> at) where TRecord : notnull =>
        strip.State.SwapIO(held => held with { Selected = at }).Bind(held => Framed(strip, held));

    private static IO<Unit> Resized<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        from width in IO.lift(() => strip.Panel.Width)
        from held in strip.State.ValueIO
        from laid in when(int.Max(width / SwatchSide, 1) != held.Columns, Laid(strip)).As()
        select laid;

    private static Seq<Option<Swatch>> Entries<TRecord>(Strip<TRecord> strip, StripState<TRecord> held) where TRecord : notnull =>
        held.Shown.Match(Some: record => strip.Listed(record).Map(static swatch => Some(swatch)), None: static () => Seq(Option<Swatch>.None));

    private static IO<Unit> Laid<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        unless(strip.Edit.Editing,
        from columns in IO.lift(() => int.Max(strip.Panel.Width / SwatchSide, 1))
        from held in strip.State.ValueIO
        from made in DisposalOps.Replace(keep => Built(strip, held, columns, keep),
            (value, lease) =>
                from published in DisposalOps.Published(strip.Owned, (Control?)value.Layout, lease, () => strip.Panel.Content, content => strip.Panel.Content = content)
                from shown in atomicIO(() => strip.Wells.Value = value.Wells)
                from laid in strip.State.SwapIO(current => current with { Columns = columns })
                select unit)
        from framed in Framed(strip, held)
        select unit).As();

    private static IO<(Seq<Well> Wells, StackLayout Layout)> Built<TRecord>(Strip<TRecord> strip, StripState<TRecord> held, int columns, Action<IDisposable> keep) where TRecord : notnull =>
        from wells in Entries(strip, held).Map((entry, index) => Placed(strip, entry, index, keep)).TraverseM(static make => make).As()
        from lines in toSeq(wells.Chunk(columns)).TraverseM(line => DisposalOps.Acquire(IO.lift(() => new StackLayout([.. toSeq(line).Map(static well => new StackLayoutItem(well.Button))]) { Orientation = Orientation.Horizontal }), keep)).As()
        from layout in DisposalOps.Acquire(IO.lift(() => new StackLayout([.. lines.Map(static line => new StackLayoutItem(line, HorizontalAlignment.Left))]) { Orientation = Orientation.Vertical }), keep)
        select (Wells: wells, Layout: layout);

    private static IO<Unit> Framed<TRecord>(Strip<TRecord> strip, StripState<TRecord> held) where TRecord : notnull =>
        from wells in strip.Wells.ValueIO
        let count = held.Shown.Map(record => strip.Listed(record).Count).IfNone(0)
        from enabled in IO.lift(() => {
            strip.Add.Enabled = count > 0;
            strip.Remove.Enabled = held.Selected.Exists(at => at < count);
        })
        from painted in wells.Zip(Entries(strip, held))
            .Map((pair, index) => Restyled(pair.First, pair.Second.Map(static swatch => swatch.Display), Frame(held.Selected, index)))
            .TraverseM(static paint => paint)
            .As()
        select unit;

    private static PaintSlot Frame(Option<int> selected, int index) => selected.Exists(at => at == index) ? PaintSlot.Selection : PaintSlot.FrameEdge;

    private static IO<Well> Placed<TRecord>(Strip<TRecord> strip, Option<Swatch> entry, int index, Action<IDisposable> retain) where TRecord : notnull =>
        from well in Welled(new SwatchEncoding.DisplayReferred(), new Size(SwatchSide, SwatchSide), Selected(strip, Some(index)), strip.Scope.Sink, retain)
        from sessions in entry.Traverse(_ => Session(well, strip.Edit, Indexed(strip, index), strip.Scope, strip.Site, retain)).As()
        from enabled in IO.lift(() => { well.Dialog.Enabled = entry.IsSome; })
        select well;

    private static Func<Vector4, TRecord, Fin<TRecord>> Indexed<TRecord>(Strip<TRecord> strip, int index) where TRecord : notnull =>
        (color, record) => strip.Revise(record, list => Replaced(list, index, Swatch.From(color)));

    private static Seq<Swatch> Replaced(Seq<Swatch> list, int index, Swatch swatch) =>
        list.Map((held, at) => at == index ? swatch : held);

    private static IO<Unit> Session<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull =>
        from attached in Subscriptions.Attach(h => well.Dialog.Executed += h, h => well.Dialog.Executed -= h, Opened(well, edit, write, scope, site), retain)
        from context in ChoiceRows.Context(well.Button, Menu(well, edit, write), [], scope.Sink)([], IO.pure(unit), retain)
        select unit;

    private static IO<Well> Welled(SwatchEncoding encoding, Size size, IO<Unit> touched, IPlugInSink sink, Action<IDisposable> retain) =>
        from dialog in IO.lift(static () => RowText.Localize("Color Picker…").Local switch {
            var shown => new Command { MenuText = shown, ToolTip = shown },
        })
        from button in ButtonRows.Pressed(dialog, sink, retain)
        from sized in IO.lift(() => { button.Size = size; })
        from look in atomIO((Stored: Option<Vector4>.None, Frame: PaintSlot.FrameEdge))
        let well = new Well(button, dialog, encoding, touched, look)
        from painted in Painted(well)
        select well;

    private static EventHandler<EventArgs> Opened<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, CallbackSite site)
        where TRecord : notnull =>
        Callbacks.Handler<EventArgs>(_ => Dialog(well, edit, write, scope, site), site);

    private static IO<Unit> Dialog<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, CallbackSite site) where TRecord : notnull =>
        (from touched in well.Touched
         from document in scope.DocumentIO
         from look in well.Look.ValueIO
         from opened in edit.Opened
         from taken in DisposalOps.OnFailure(
             HostDialogs.ShowColorDialog(
                 document, well.Encoding.Shown(look.Stored.IfNone(Vector4.One)), well.Encoding.Alpha, None, Some((Previewed(well, edit, write), site)))
                 .Bind(chosen => edit.Take(new Edit<Vector4>.Commit(well.Encoding.Stored(chosen)), write)),
             edit.Cancel)
         select unit)
        .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));

    private static Func<Color4f, IO<Unit>> Previewed<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write) where TRecord : notnull =>
        color => edit.Take(new Edit<Vector4>.Preview(well.Encoding.Stored(color)), write).Map(unit);

    private static Seq<Seq<MenuEntry>> Menu<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write) where TRecord : notnull =>
        Seq(Seq<MenuEntry>(well.Dialog, new MenuEntry.Listed(RowText.Localize("Named Colors").Local, well.Look.ValueIO.Map(look => Named(well, edit, write, look.Stored)))));

    private static Seq<MenuPick> Named<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, Option<Vector4> stored) where TRecord : notnull =>
        toSeq(NamedColorList.Default).Map<MenuPick>(named => new MenuPick.Item(
            named.Name,
            stored.Exists(held => well.Encoding.Shown(held).AsSystemColor().ToArgb() == named.Color.ToArgb()),
            well.Touched.Bind(_ => edit.Take(new Edit<Vector4>.Commit(well.Encoding.Stored(new Color4f(named.Color))), write)).Map(unit)));

    private static IO<Unit> Restyled(Well well, Option<Vector4> stored, PaintSlot frame) =>
        well.Look.SwapIO(_ => (stored, frame)).Bind(_ => Painted(well));

    private static IO<Unit> Painted(Well well) =>
        well.Look.ValueIO.Bind(look => well.Button.Show((canvas, bounds) => Drawn(canvas, bounds, well.Encoding, look.Stored, look.Frame)));

    private static IO<Unit> Drawn(PlotCanvas canvas, RectangleF bounds, SwatchEncoding encoding, Option<Vector4> stored, PaintSlot frame) =>
        from swatch in IO.pure(RectangleF.Inset(bounds, new PaddingF(FrameWidth + WellWidth)))
        from well in Plots.Paint(canvas, Seq(
            new PlotMark<Unit>(new MarkShape.Band(RectangleF.Inset(bounds, new PaddingF(FrameWidth))), new MarkStyle.Fill(PaintSlot.WindowBackground, 1f), None),
            new PlotMark<Unit>(new MarkShape.Band(RectangleF.Inset(bounds, new PaddingF(FrameWidth / 2f))), new MarkStyle.Stroke(frame, 1f, FrameWidth), None)))
        from color in when(canvas.Enabled, stored.Match(Some: held => Colored(canvas, swatch, encoding.Shown(held)), None: () => Varied(canvas, swatch))).As()
        from rim in Plots.Paint(canvas, Seq(
            new PlotMark<Unit>(new MarkShape.Band(RectangleF.Inset(swatch, new PaddingF(RimWidth / 2f))), new MarkStyle.Stroke(frame, 1f, RimWidth), None)))
        select unit;

    private static IO<Unit> Colored(PlotCanvas canvas, RectangleF swatch, Color4f shown) =>
        from board in when(shown.A < 1f, Thumbnails.Checker(canvas, swatch, Checkerboard.Content)).As()
        from fill in Plots.Paint(canvas, Seq(new PlotMark<Unit>(new MarkShape.Band(swatch), new MarkStyle.Fill(new MarkColor.Fixed(shown.ToEto()), shown.A), None)))
        select unit;

    private static IO<Unit> Varied(PlotCanvas canvas, RectangleF swatch) =>
        from white in Plots.Paint(canvas, Seq(new PlotMark<Unit>(new MarkShape.Band(swatch), new MarkStyle.Fill(new MarkColor.Fixed(Colors.White), 1f), None)))
        from text in IO.lift(() => canvas.Graphics.MeasureString(canvas.Font, RowEdit.Varies))
        from marked in text.Width <= swatch.Width && text.Height <= swatch.Height
            ? IO.lift(() => canvas.Graphics.DrawText(canvas.Font, Colors.Black, swatch.Center - (text / 2f), RowEdit.Varies))
            : Plots.Paint(canvas, Seq(new PlotMark<Unit>(
                new MarkShape.Polygon([swatch.TopRight, swatch.BottomRight, swatch.BottomLeft]), new MarkStyle.Fill(new MarkColor.Fixed(Colors.Black), 1f), None)))
        select unit;

    private sealed record Well(
        DrawnButton Button, Command Dialog, SwatchEncoding Encoding, IO<Unit> Touched, Atom<(Option<Vector4> Stored, PaintSlot Frame)> Look);

    private sealed record Strip<TRecord>(
        Panel Panel, Command Add, Command Remove, Atom<StripState<TRecord>> State,
        Ref<Seq<Well>> Wells, Retained<IDisposable> Owned,
        Func<TRecord, Seq<Swatch>> Listed, Func<TRecord, Func<Seq<Swatch>, Seq<Swatch>>, Fin<TRecord>> Revise, RowEdit<TRecord> Edit, RowScope Scope, CallbackSite Site)
        where TRecord : notnull;

    private sealed record StripState<TRecord>(Option<TRecord> Shown, Option<int> Selected, int Columns) where TRecord : notnull;

    // --- [PAIRS]
    public static ControlRow DisplayPrint<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, string caption, string help,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> display,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> print,
        LanguageExt.HashSet<DisplayAndPrintColorSourceMode> offered, Option<Func<TRecord, IO<(System.Drawing.Color Display, System.Drawing.Color Print)>>> resolved, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, fields, caption, help, RowShape.Inline, (scope, retain) => Paired(source, fields, display, print, offered, resolved, scope, retain), rules);

    private static IO<RowCells> Paired<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> display,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> print,
        LanguageExt.HashSet<DisplayAndPrintColorSourceMode> offered, Option<Func<TRecord, IO<(System.Drawing.Color Display, System.Drawing.Color Print)>>> resolved, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from picker in DisposalOps.Acquire(IO.lift(static () => new DisplayAndPrintColorPicker()), retain)
        from configured in IO.lift(() => {
            picker.PickerMode = DisplayAndPrintColorPickerMode.Dual;
            picker.ShowByLayer = offered.Contains(DisplayAndPrintColorSourceMode.ByLayer);
            picker.ShowByObject = offered.Contains(DisplayAndPrintColorSourceMode.ByObject);
            picker.ShowByParent = offered.Contains(DisplayAndPrintColorSourceMode.ByParent);
            picker.ShowViewport = offered.Contains(DisplayAndPrintColorSourceMode.Viewport);
            picker.ShowNone = offered.Contains(DisplayAndPrintColorSourceMode.None);
        })
        let site = new CallbackSite(scope.Sink, typeof(DisplayAndPrintColorPicker), nameof(DisplayAndPrintColorPicker.PropertyChanged))
        from bound in RowEdit.Bind<TRecord, PropertyChangedEventHandler>(source, fields, h => picker.PropertyChanged += h, h => picker.PropertyChanged -= h,
            edit => Callbacks.Handler<PropertyChangedEventArgs>(_ => Changed(picker, display, print, edit, source, scope), site).Invoke,
            value => value.Match(Some: record => Matched(picker, display.Get(record), print.Get(record), resolved.Map(read => read(record))), None: () => Mixed(picker)),
            scope, site, retain)
        select new RowCells(
            picker, None, None, None, [], [], new RowHelp(None, None), bound.Shown, Some<RowEdit>(bound),
            Some(ChoiceRows.Context(picker, [], [], scope.Sink)));

    private static IO<Unit> Changed<TRecord>(
        DisplayAndPrintColorPicker picker, Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> display,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> print, RowEdit<TRecord> edit, RowSource<TRecord> source, RowScope scope)
        where TRecord : notnull =>
        from held in scope.Read(source)
        from read in IO.lift(() => (
            Display: (picker.DisplaySourceMode, picker.DisplayColor.ToSystemDrawing()),
            Print: (picker.PrintSourceMode, picker.PrintColor.ToSystemDrawing())))
        from committed in when(!Same(display.Get(held), read.Display) || !Same(print.Get(held), read.Print), edit.Commit(print.Set(read.Print, display.Set(read.Display, held))).Map(unit)).As()
        select unit;

    private static IO<Unit> Matched(
        DisplayAndPrintColorPicker picker, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) display, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) print,
        Option<IO<(System.Drawing.Color Display, System.Drawing.Color Print)>> resolved) =>
        from sides in IO.lift(() => {
            picker.DisplayVaries = false;
            picker.PrintVaries = false;
            picker.DisplaySourceMode = display.Source;
            picker.DisplayColor = display.Color.ToEto();
            picker.PrintSourceMode = print.Source;
            picker.PrintColor = print.Color.ToEto();
            picker.LinkPrintToDisplay = Same(display, print);
        })
        from colors in resolved.Traverse(static read => read).As()
        from shown in IO.lift(() => colors.Iter(held => {
            picker.DisplayResolvedColor = held.Display.ToEto();
            picker.PrintResolvedColor = held.Print.ToEto();
        }))
        select unit;

    private static IO<Unit> Mixed(DisplayAndPrintColorPicker picker) =>
        IO.lift(() => {
            picker.DisplayVaries = true;
            picker.PrintVaries = true;
        });

    private static bool Same((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) left, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) right) =>
        left.Source == right.Source && Conversions.Same(left.Color, right.Color);

    // --- [WHEELS]
    public static ControlRow Wheels(RowSource<WheelsState> source, string caption, string help, Option<Gamut> working, RowRules rules) =>
        IterableNE.create(ToneWheel.Items[0], ToneWheel.Items.Skip(1)).Map(static wheel => new RowField<WheelsState>(wheel.Parameter)) switch {
            var members => ControlRow.Of(source, members, caption, help, RowShape.Block, (scope, retain) => Wheeled(source, members, working, scope, retain), rules),
        };

    private static IO<RowCells> Wheeled(
        RowSource<WheelsState> source, IterableNE<RowField<WheelsState>> members, Option<Gamut> working, RowScope scope, Action<IDisposable> retain) =>
        from record in scope.Read(source)
        from shown in atomIO(Opening(record))
        let texts = (Hue: NumberText.Scalar(NumericDomain.Bounded<WheelHue, float>(WheelHue.Presentation), RowEdit.Varies), Strength: NumberText.Scalar(NumericDomain.Bounded<AxisFraction, float>(AxisFraction.Presentation), RowEdit.Varies))
        let faced = fun((ToneWheel wheel) => new ShownWheel(Wording.English.Shown(wheel.Parameter.Text.Caption, scope.Sink), texts.Hue, texts.Strength))
        let current = fun(() => shown.Value)
        from wheel in DisposalOps.Acquire(IO.lift(() => new GradingWheel(scope.Sink, faced(current()))), retain)
        let site = new CallbackSite(scope.Sink, typeof(GradingWheel), nameof(GradingWheel.Edited))
        from bound in RowEdit.Bind(source, members, h => wheel.Edited += h, h => wheel.Edited -= h,
            edit => Callbacks.Handler<EditEventArgs<WheelOffset>>(args => edit.Take(args.Edit, (offset, held) => Fin.Succ(current().Lens.Set(offset, held))).Map(unit), site),
            value => IO.lift(current).Bind(chosen => wheel.Receive(value.Map(chosen.Lens.Get))), scope, site, retain)
        from hue in NumericRows.Line(
            source, WheelOffsetParameter.Hue.Text.Caption, Followed(current, static tone => tone.Hue), WheelHue.Presentation, bound, None, scope.Sink, retain)
        from strength in NumericRows.Line(
            source, WheelOffsetParameter.Strength.Text.Caption, Followed(current, static tone => tone.Strength), AxisFraction.Presentation, bound, None, scope.Sink, retain)
        from luma in NumericRows.Line(
            source, WheelOffsetParameter.Luma.Text.Caption, Followed(current, static tone => tone.Luma), WheelLuma.Presentation, bound, None, scope.Sink, retain)
        from lanes in working.ToSeq().Bind(gamut => toSeq(TraceChannel.Items).Choose(channel => channel.Lane.Map(lane => Laned(channel, lane, WheelOffset.LaneBounds(gamut), current, bound, gamut, scope.Sink, retain))))
            .TraverseM(static make => make).As()
        let lines = Lines(bound, scope.Read(source), Seq(hue.Show, strength.Show, luma.Show) + lanes.Map(static lane => lane.Show))
        from popup in ChoiceRows.Popup(
                toSeq(ToneWheel.Items).Map(item => new ChoiceItem<ToneWheel>(item, Wording.English.Shown(item.Parameter.Text.Caption, scope.Sink), None)),
                picked => Switched(shown, wheel, faced(picked), picked).Bind(_ => lines), scope.Sink, retain)
        select new RowCells(
            popup.Control, None, None, None, [],
            Seq(new RowLine(Some(""), wheel, None, None), hue.Line, strength.Line, luma.Line) + lanes.Map(static lane => lane.Line),
            new RowHelp(None, None), lines.Bind(_ => IO.lift(current)).Bind(chosen => popup.Show(Some(chosen))), Some<RowEdit>(bound),
            Some(ChoiceRows.Context(wheel, [], [], scope.Sink)));

    private static IO<Unit> Switched(Atom<ToneWheel> shown, GradingWheel wheel, ShownWheel faced, ToneWheel picked) =>
        writeIO(shown, picked).Bind(_ => wheel.Show(faced));

    private static ToneWheel Opening(WheelsState record) =>
        toSeq(ToneWheel.Items).Find(wheel => wheel.Lens.Get(record) != WheelOffset.Default).IfNone(ToneWheel.Global);

    private static Lens<WheelsState, T> Followed<T>(Func<ToneWheel> current, Func<ToneWheel, Lens<WheelsState, T>> member) =>
        Lens<WheelsState, T>.New(held => member(current()).Get(held), value => held => member(current()).Set(value, held));

    private static IO<(RowLine Line, Func<Option<WheelsState>, IO<Unit>> Show)> Laned(
        TraceChannel channel, Lens<Vector3, float> lane, (Vector3 Low, Vector3 High) bounds, Func<ToneWheel> current, RowEdit<WheelsState> edit, Gamut working, IPlugInSink sink, Action<IDisposable> retain) {
        NumericDomain<SignedMagnitude, float> domain = NumericDomain.Bounded<SignedMagnitude, float>(new() { Soft = (lane.Get(bounds.Low), lane.Get(bounds.High)) });
        NumberText<float> text = NumberText.Scalar(domain, RowEdit.Varies);
        return from field in DisposalOps.Acquire(IO.lift(() => new NumberField<SignedMagnitude, float>(text, domain.Create, domain.Key, domain.Bounds, sink)), retain)
               from edited in Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h,
                   Callbacks.Handler<EditEventArgs<SignedMagnitude>>(args => edit.Take(args.Edit, (value, held) => Crossed(current(), lane, working, value, held)).Map(unit), new CallbackSite(sink, typeof(ColorRows), nameof(Wheels))), retain)
               select (
                   new RowLine(Some(channel.Map(luma: "Luma", red: "Red", green: "Green", blue: "Blue")), field, None,
                       Some(field.Fit)),
                   (Func<Option<WheelsState>, IO<Unit>>)(value => IO.lift(current).Bind(wheel => field.Receive(Lane(wheel, lane, working, value)))));
    }

    private static Fin<WheelsState> Crossed(ToneWheel wheel, Lens<Vector3, float> lane, Gamut working, float value, WheelsState held) =>
        wheel.Lens.Get(held) switch {
            var offset => WheelOffset.FromMultiplier(lane.Set(value, offset.Multiplier(working)), working, offset.Hue).Map(next => wheel.Lens.Set(next, held)),
        };

    private static Option<SignedMagnitude> Lane(ToneWheel wheel, Lens<Vector3, float> lane, Gamut working, Option<WheelsState> value) =>
        from record in value
        from weight in SignedMagnitude.Validated(lane.Get(wheel.Lens.Get(record).Multiplier(working))).ToOption()
        select weight;

    private static IO<Unit> Lines<TRecord>(RowEdit<TRecord> edit, IO<TRecord> read, Seq<Func<Option<TRecord>, IO<Unit>>> shows) where TRecord : notnull =>
        unless(edit.Editing,
        from bracketed in edit.Shown
        from record in read
        from shown in shows.TraverseM(show => show(Some(record))).As()
        select unit).As();

    // --- [GRADIENTS]
    public static ControlRow Gradient<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Ramp> lens, Gamut working, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Block, (scope, retain) => Ramped(source, field, lens, working, scope, retain), rules);

    public static Fin<Ramp> RampOf(ColorGradient gradient) {
        Seq<ColorStop> stops = toSeq(gradient.GetColorStops());
        Vector4[] light = [.. stops.Map(static stop => new Vector4(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A) / byte.MaxValue)];
        TransferCurve.Srgb.Decode(light, Nits.ReferenceWhite);
        return stops.Zip(toSeq(light))
            .Traverse(static pair => (
                from position in RampPosition.Validated(pair.First.Position)
                from stop in ValidationResults.ToFin(RampStop.Validate(position, pair.Second, out RampStop item), item)
                select stop).ToValidation())
            .As()
            .ToFin()
            .Bind(static held => ValidationResults.ToFin(Ramp.Validate(held, RampInterpolation.Linear, out Ramp? ramp), ramp));
    }

    public static ColorGradient GradientOf(ColorGradient held, Ramp ramp) {
        Vector4[] encoded = [.. ramp.Stops.Map(static stop => stop.Color)];
        TransferCurve.Srgb.Encode(encoded, Nits.ReferenceWhite);
        ColorGradient written = held.Duplicate();
        written.SetColorStops(ramp.Stops.Zip(toSeq(encoded), static (stop, color) => new ColorStop(
            System.Drawing.Color.FromArgb((int)Swatch.From(new Vector4(color.W)) & byte.MaxValue, System.Drawing.Color.FromArgb(Swatch.From(color))), stop.Position)));
        return written;
    }

    private static IO<RowCells> Ramped<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Ramp> lens, Gamut working, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull {
        NumericDomain<RampPosition, double> domain = NumericDomain.Bounded<RampPosition, double>(new());
        NumberText<double> text = NumberText.Scalar(domain, RowEdit.Varies);
        return from bar in DisposalOps.Acquire(IO.lift(() => new GradientRamp(scope.Sink, Wording.English.Shown(field.Caption, scope.Sink), new GradientRampState.Editor(working, text, None, None, None))), retain)
               from stop in atomIO((Ramp: Option<Ramp>.None, Selected: Option<int>.None))
               let site = new CallbackSite(scope.Sink, typeof(GradientRamp), nameof(GradientRamp.Edited))
               from bound in RowEdit.Bind(source, IterableNE.create(field), h => bar.Edited += h, h => bar.Edited -= h,
                   edit => Callbacks.Handler<EditEventArgs<Ramp>>(args => Dragged(edit, stop, lens, args.Edit), site), value => bar.Receive(value.Map(lens.Get)), scope, site, retain)
               from popup in ChoiceRows.Popup(
                       ChoiceRows.Items<RampInterpolation>(None, scope.Sink),
                       picked => bound.Take(new Edit<RampInterpolation>.Commit(picked), (interpolation, held) => Interpolated(lens, interpolation, held)).Map(unit), scope.Sink, retain)
               from well in Welled(new SwatchEncoding.SceneLinear(working), new Size(-1, SwatchSide), IO.pure(unit), scope.Sink, retain)
               from position in DisposalOps.Acquire(IO.lift(() => new NumberField<RampPosition, double>(text, domain.Create, domain.Key, domain.Bounds, scope.Sink)), retain)
               from line in DisposalOps.Acquire(IO.lift(() => new TableLayout(new TableRow(new TableCell(well.Button, scaleWidth: true), new TableCell(position)))), retain)
               from spaced in IO.lift(() => { line.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table); })
               let stopped = Stopped(stop.ValueIO, well, position)
               from session in Session(well, bound, (color, record) => Recolored(lens, stop.Value.Selected, color, record), scope, site, retain)
               from selected in Subscriptions.Attach(h => bar.Selected += h, h => bar.Selected -= h,
                   Callbacks.Handler<StopSelectedEventArgs>(selected => Chosen(stop, selected.Index).Bind(_ => stopped), site with { Member = nameof(GradientRamp.Selected) }), retain)
               from positioned in Subscriptions.Attach(h => position.Edited += h, h => position.Edited -= h,
                   Callbacks.Handler<EditEventArgs<RampPosition>>(args => Positioned(bound, stop, bar, lens, args.Edit), site with { Member = nameof(NumberField<,>.Edited) }), retain)
               select new RowCells(
                   popup.Control, None, None, None, [],
                   Seq(new RowLine(Some(""), bar, None, None), new RowLine(Some("Stop"), line, None, Some(position.Fit))),
                   new RowHelp(None, None), Restopped(bound, scope.Read(source), lens, popup.Show, stop, stopped), Some<RowEdit>(bound),
                   Some(ChoiceRows.Context(bar, [], [], scope.Sink))) { Plot = Some<ComponentControl>(bar) };
    }

    private static IO<Unit> Dragged<TRecord>(RowEdit<TRecord> edit, Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, Lens<TRecord, Ramp> lens, Edit<Ramp> change)
        where TRecord : notnull =>
        stop.SwapIO(held => held with { Ramp = change.Carried | held.Ramp }).Bind(_ => edit.Take(change, (ramp, record) => Fin.Succ(lens.Set(ramp, record)))).Map(unit);

    private static IO<Unit> Chosen(Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, Option<int> at) =>
        stop.SwapIO(held => held with { Selected = at }).Map(unit);

    private static Fin<TRecord> Interpolated<TRecord>(Lens<TRecord, Ramp> lens, RampInterpolation interpolation, TRecord record) where TRecord : notnull =>
        ValidationResults.ToFin(Ramp.Validate(lens.Get(record).Stops, interpolation, out Ramp? ramp), ramp).Map(held => lens.Set(held, record));

    private static Fin<TRecord> Recolored<TRecord>(Lens<TRecord, Ramp> lens, Option<int> selected, Vector4 color, TRecord record) where TRecord : notnull =>
        from at in selected.ToFin(new Refused(nameof(GradientRamp.Selected), Refused.Absent))
        let ramp = lens.Get(record)
        from current in Conversions.Within(at, ramp.Stops.Count, nameof(Ramp.Stops)).Map(_ => ramp.Stops[at])
        from item in ValidationResults.ToFin(RampStop.Validate(current.Position, color, out RampStop next), next)
        from written in ValidationResults.ToFin(Ramp.Validate(ramp.Stops.Map((kept, index) => index == at ? item : kept), ramp.Interpolation, out Ramp? replaced), replaced)
        select lens.Set(written, record);

    private static IO<Unit> Stopped(IO<(Option<Ramp> Ramp, Option<int> Selected)> stop, Well well, NumberField<RampPosition, double> position) =>
        from state in stop
        let selected = from ramp in state.Ramp from at in state.Selected from held in ramp.Stops.At(at) select held
        from enabled in IO.lift(() => {
            well.Dialog.Enabled = selected.IsSome;
            position.Enabled = selected.IsSome;
        })
        from swatch in Restyled(well, selected.Map(static held => held.Color), PaintSlot.FrameEdge)
        from shown in position.Receive(selected.Map(static held => held.Position))
        select unit;

    private static IO<Unit> Restopped<TRecord>(
        RowEdit<TRecord> edit, IO<TRecord> read, Lens<TRecord, Ramp> lens, Func<Option<RampInterpolation>, IO<Unit>> interpolation,
        Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, IO<Unit> stopped) where TRecord : notnull =>
        unless(edit.Editing,
        from bracketed in edit.Shown
        from record in read
        from chosen in interpolation(Some(lens.Get(record).Interpolation))
        from moved in stop.SwapIO(state => state with { Ramp = Some(lens.Get(record)) })
        from shown in stopped
        select unit).As();

    private static IO<Unit> Positioned<TRecord>(
        RowEdit<TRecord> edit, Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, GradientRamp bar, Lens<TRecord, Ramp> lens, Edit<RampPosition> change) where TRecord : notnull =>
        from state in stop.ValueIO
        from taken in edit.Take(change, (value, record) => state.Selected.ToFin(new Refused(nameof(GradientRamp.Selected), Refused.Absent)).Map(at => lens.Set(lens.Get(record).Moved(at, value).Ramp, record)))
        from reselected in (from value in change.Settled from ramp in state.Ramp from at in state.Selected select ramp.Moved(at, value)).Traverse(moved => Reselected(stop, bar, moved)).As()
        select unit;

    private static IO<Unit> Reselected(Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, GradientRamp bar, (Ramp Ramp, int Index) moved) =>
        stop.SwapIO(held => held with { Ramp = Some(moved.Ramp) }).Bind(_ => bar.Select(Some(moved.Index)));

    // --- [QUALIFIERS]
    public static ControlRow Qualifier<TRecord>(
        RowSource<TRecord> source, string caption, string help, RangeAxis axis, Lens<TRecord, HueRange> range, RowField<TRecord> field,
        Option<Func<TRecord, Vector4, Vector4>> grade, Option<StageInput> stage, RowRules rules) where TRecord : notnull =>
        ControlRow.Of(source, IterableNE.create(field), caption, help, RowShape.Block, (scope, retain) =>
            Qualified(source, IterableNE.create(field), caption, axis, range, HueRangeParameter.Band(axis) switch {
                var (center, width, softness) => (center.Text.Caption, width.Text.Caption, softness.Text.Caption),
            }, grade, stage, scope, retain), rules);

    private static IO<RowCells> Qualified<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> members, string caption, RangeAxis axis, Lens<TRecord, HueRange> range,
        (string Center, string Width, string Softness) band, Option<Func<TRecord, Vector4, Vector4>> grade, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from text in IO.pure(NumberText.Scalar(NumericDomain.Bounded<AxisFraction, float>(axis.Presentation), RowEdit.Varies))
        from bar in DisposalOps.Acquire(IO.lift(() => new HueRangeBar(scope.Sink, axis, Wording.English.Shown(caption, scope.Sink), new BandTexts(text, text, text))), retain)
        let site = new CallbackSite(scope.Sink, typeof(HueRangeBar), nameof(HueRangeBar.Edited))
        let lenses = axis.Members(range)
        from bound in RowEdit.Bind(source, members, h => bar.Edited += h, h => bar.Edited -= h,
            edit => Callbacks.Handler<EditEventArgs<HueRange>>(args => edit.Take(args.Edit, (value, held) => Fin.Succ(range.Set(value, held))).Map(unit), site),
            value => Banded(bar, range, grade, value), scope, site, retain)
        from pick in stage.Traverse(input => Pick(input, bound, (color, held) => Centered(axis, lenses.Center, color, held), scope, retain)).As()
        from center in NumericRows.Line(source, band.Center, lenses.Center, axis.Presentation, bound, pick.Map(static button => (Control)button), scope.Sink, retain)
        from width in NumericRows.Line(source, band.Width, lenses.Width, axis.Presentation, bound, None, scope.Sink, retain)
        from softness in NumericRows.Line(source, band.Softness, lenses.Softness, axis.Presentation, bound, None, scope.Sink, retain)
        select new RowCells(
            bar, None, None, None, [], Seq(center.Line, width.Line, softness.Line), new RowHelp(None, None),
            Lines(bound, scope.Read(source), Seq(center.Show, width.Show, softness.Show)), Some<RowEdit>(bound),
            Some(ChoiceRows.Context(bar, [], [], scope.Sink)));

    private static IO<Unit> Banded<TRecord>(HueRangeBar bar, Lens<TRecord, HueRange> range, Option<Func<TRecord, Vector4, Vector4>> grade, Option<TRecord> value) where TRecord : notnull =>
        bar.Graded(from record in value from held in grade select Graded(range, held, record)).Bind(_ => bar.Receive(value.Map(range.Get)));

    private static Func<HueRange, Vector4, Vector4> Graded<TRecord>(Lens<TRecord, HueRange> range, Func<TRecord, Vector4, Vector4> grade, TRecord record) where TRecord : notnull =>
        (shown, color) => grade(range.Set(shown, record), color);

    private static Fin<TRecord> Centered<TRecord>(RangeAxis axis, Lens<TRecord, AxisFraction> center, Vector4 color, TRecord record) where TRecord : notnull =>
        AxisFraction.Validated(axis.Coordinate(Hsy.From(color))).Map(value => center.Set(value, record));
}
