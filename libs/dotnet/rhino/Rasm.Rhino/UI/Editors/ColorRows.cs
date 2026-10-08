using System.ComponentModel;
using System.Numerics;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Grade.Balance;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Components;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Viewers;
using Rasm.Rhino.UI.Views;
using Rhino.Display;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;
using Wacton.Unicolour;

namespace Rasm.Rhino.UI.Editors;

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
        ControlRow.Of(source, field, RowShape.Inline, scope => Swatched(source, field, lens, Option<Lens<TRecord, bool>>.None, stage, scope), rules);

    public static ControlRow SwatchColor<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Gated<Swatch>> lens, Option<StageInput> stage, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline,
            scope => Swatched(source, field, Prelude.lens(lens, Gated<Swatch>.ValueEntry.Lens), Some(Prelude.lens(lens, Gated<Swatch>.EnabledEntry.Lens)), stage, scope), rules);

    public static ControlRow Swatches<TRecord, TValue, TError>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Option<StageInput> stage, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>
        where TError : Error, IValidationError<TError> =>
        ControlRow.Of(source, field, RowShape.Block, scope => Stripped<TRecord, TValue, TError>(source, field, lens, stage, scope), rules);

    private static IO<RowCells> Swatched<TRecord>(
        RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Swatch> swatch, Option<Lens<TRecord, bool>> gate, Option<StageInput> stage, RowScope scope)
        where TRecord : notnull =>
        from welled in Welled(new SwatchEncoding.DisplayReferred(), new Size(-1, SwatchSide), IO.pure(unit), scope.Sink)
        from box in DisposalOps.OnFailure(gate.Traverse(static _ => IO.lift(static () => new CheckBox())).As(), IO.lift(welled.Release.Dispose))
        let gated = from member in gate from shown in box select (Lens: member, Box: shown)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(SwatchColor))
        let write = Written(swatch)
        from bound in DisposalOps.OnFailure(
            RowEdit.Bind(source, IterableNE.create(field), h => welled.Well.Dialog.Executed += h, h => welled.Well.Dialog.Executed -= h,
                edit => Opened(welled.Well, edit, write, scope, site), value => Shown(welled.Well, swatch, gated, value), scope, site),
            IO.lift(welled.Release.Dispose))
        let held = Seq(welled.Release, bound.Release)
        from gating in DisposalOps.OnFailure(gated.Traverse(pair => Gating(source, bound.Edit, pair.Lens, pair.Box, scope, site)).As(), DisposalOps.Release(held))
        let kept = held + gating.ToSeq()
        from pick in DisposalOps.OnFailure(stage.Traverse(input => Pick(input, bound.Edit, write, scope)).As(), DisposalOps.Release(kept))
        select new RowCells(
            welled.Well.Button, box.Map(static shown => (Control)shown), pick.Map(static button => (Control)button.Control), None, [], [],
            new RowHelp(None, Some(Hex(swatch.Get(source.Default)))), bound.Edit.Shown, Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(welled.Well.Button, Menu(welled.Well, bound.Edit, write), [], scope.Sink)),
            DisposalOps.Composite(kept + pick.Map(static button => button.Release).ToSeq(), site));

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

    private static IO<IDisposable> Gating<TRecord>(RowSource<TRecord> source, RowEdit<TRecord> edit, Lens<TRecord, bool> gate, CheckBox box, RowScope scope, CallbackSite site)
        where TRecord : notnull =>
        Subscriptions.Attach(h => box.CheckedChanged += h, h => box.CheckedChanged -= h, Callbacks.Handler<EventArgs>(_ => Toggled(source, edit, gate, box, scope), site));

    private static IO<Unit> Toggled<TRecord>(RowSource<TRecord> source, RowEdit<TRecord> edit, Lens<TRecord, bool> gate, CheckBox box, RowScope scope) where TRecord : notnull =>
        from ticked in IO.lift(() => Optional(box.Checked))
        from held in scope.Read(source)
        from committed in ticked.Filter(state => state != gate.Get(held)).Traverse(state => edit.Commit(gate.Set(state, held))).As()
        select unit;

    private static IO<(Button Control, IDisposable Release)> Pick<TRecord>(StageInput stage, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope)
        where TRecord : notnull =>
        ButtonRows.Pick(stage, PickCaption, mean => edit.Take(new Edit<Vector4>.Commit(mean), write), scope);

    private static IO<RowCells> Stripped<TRecord, TValue, TError>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Option<StageInput> stage, RowScope scope)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>
        where TError : Error, IValidationError<TError> =>
        from add in IO.lift(static () => new Command { ToolTip = RowText.Localize("Add Color").Local })
        from remove in IO.lift(static () => new Command { ToolTip = RowText.Localize("Remove Color").Local, Enabled = false })
        from pair in ButtonRows.AddRemove(add, remove)
        from panel in IO.lift(static () => new Panel())
        from state in IO.lift(static () => Atom(new StripState<TRecord>(None, None, 0)))
        from wells in IO.lift(static () => Atom((Before: Seq<(Well Well, IDisposable Release)>(), After: Seq<(Well Well, IDisposable Release)>())))
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatches))
        let made = fun((RowEdit<TRecord> edit) => new Strip<TRecord>(
            panel, pair.Control, add, remove, state, wells, scope.Read(source),
            record => lens.Get(record).ToValue(), (record, change) => Revised<TRecord, TValue, TError>(lens, record, change), edit, scope, site))
        from bound in DisposalOps.OnFailure(
            RowEdit.Bind(source, IterableNE.create(field), h => add.Executed += h, h => add.Executed -= h,
                edit => Callbacks.Handler<EventArgs>(_ => Added(made(edit)), site), value => state.SwapIO(held => held with { Shown = value }).Map(static _ => unit), scope, site),
            IO.lift(pair.Release.Dispose))
        let strip = made(bound.Edit)
        let taken = Seq(pair.Release, bound.Release)
        from held in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                Seq(IO.pure<IDisposable>(new Disposal<Atom<(Seq<(Well Well, IDisposable Release)> Before, Seq<(Well Well, IDisposable Release)> After)>>(
                        wells, cell => _ = Callbacks.Answer(DisposalOps.Release(cell.Value.After.Map(static well => well.Release)), static () => unit, site))),
                    Subscriptions.Attach(h => remove.Executed += h, h => remove.Executed -= h, Callbacks.Handler<EventArgs>(_ => Removed(strip), site)),
                    Subscriptions.Attach(h => panel.SizeChanged += h, h => panel.SizeChanged -= h, Callbacks.Handler<EventArgs>(_ => Resized(strip), site))),
                DisposalOps.Release),
            DisposalOps.Release(taken))
        from pick in DisposalOps.OnFailure(stage.Traverse(input => ButtonRows.Pick(input, PickCaption, mean => Picked(strip, mean), scope)).As(), DisposalOps.Release(taken + held))
        select new RowCells(
            panel, None, pick.Map(static button => (Control)button.Control), None, [], [],
            new RowHelp(None, Some(string.Join(' ', lens.Get(source.Default).ToValue().Map(Hex)))), bound.Edit.Shown.Bind(_ => Laid(strip)), Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(panel, [], [], scope.Sink)),
            DisposalOps.Composite(taken + held + pick.Map(static button => button.Release).ToSeq(), site));

    private static Fin<TRecord> Revised<TRecord, TValue, TError>(Lens<TRecord, TValue> lens, TRecord record, Func<Seq<Swatch>, Seq<Swatch>> change)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, TError>, IConvertible<Seq<Swatch>>
        where TError : Error, IValidationError<TError> =>
        Conversions.Validated<TValue, Seq<Swatch>, TError>(change(lens.Get(record).ToValue())).Map(next => lens.Set(next, record));

    private static IO<Unit> Added<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        from held in strip.State.ValueIO
        from taken in strip.Edit.Take(new Edit<Unit>.Commit(unit), (_, record) => strip.Revise(record, list => Copied(list, held.Selected)))
        from last in Last(strip)
        select last;

    private static Seq<Swatch> Copied(Seq<Swatch> list, Option<int> selected) =>
        (selected.Bind(at => list.At(at)) | list.Last).Map(list.Add).IfNone(list);

    private static IO<Unit> Picked<TRecord>(Strip<TRecord> strip, Vector4 mean) where TRecord : notnull =>
        strip.Edit.Take(new Edit<Vector4>.Commit(mean), (color, record) => strip.Revise(record, list => list.Add(Swatch.From(color)))).Bind(_ => Last(strip));

    private static IO<Unit> Removed<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        strip.State.ValueIO.Bind(held => held.Selected.Traverse(at => Cut(strip, at)).As()).Map(static _ => unit);

    private static IO<Unit> Cut<TRecord>(Strip<TRecord> strip, int at) where TRecord : notnull =>
        strip.Edit.Take(new Edit<Unit>.Commit(unit), (_, record) => strip.Revise(record, list => list.Take(at).Concat(list.Skip(at + 1))))
            .Bind(_ => Selected(strip, Some(int.Max(at - 1, 0))));

    private static IO<Unit> Last<TRecord>(Strip<TRecord> strip) where TRecord : notnull =>
        strip.Read.Bind(record => Selected(strip, Some(strip.Listed(record).Count - 1)));

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
        from columns in IO.lift(() => int.Max(strip.Panel.Width / SwatchSide, 1))
        from held in strip.State.SwapIO(current => current with { Columns = columns })
        from made in DisposalOps.AcquireAll(
            Entries(strip, held).Map((entry, index) => Placed(strip, entry, index)), static placed => DisposalOps.Release(placed.Map(static well => well.Release)))
        from old in IO.lift(() => Optional(strip.Panel.Content))
        from laid in DisposalOps.OnFailure(IO.lift(() => Arranged(strip, made, columns)), DisposalOps.Release(made.Map(static well => well.Release)))
        from swapped in strip.Wells.SwapIO(prior => (prior.After, made))
        from freed in DisposalOps.Release(swapped.Before.Map(static well => well.Release))
        from dropped in DisposalOps.Release(old.ToSeq())
        from framed in Framed(strip, held)
        select unit;

    private static Unit Arranged<TRecord>(Strip<TRecord> strip, Seq<(Well Well, IDisposable Release)> wells, int columns) where TRecord : notnull {
        strip.Pair.Detach();
        strip.Panel.Content = new StackLayout([
            new StackLayoutItem(strip.Pair, HorizontalAlignment.Left),
            .. toSeq(wells.Chunk(columns)).Map(static line => new StackLayoutItem(Lined(line), HorizontalAlignment.Left)),
        ]) { Orientation = Orientation.Vertical };
        return unit;
    }

    private static StackLayout Lined((Well Well, IDisposable Release)[] line) =>
        new([.. toSeq(line).Map(static well => new StackLayoutItem(well.Well.Button))]) { Orientation = Orientation.Horizontal };

    private static IO<Unit> Framed<TRecord>(Strip<TRecord> strip, StripState<TRecord> held) where TRecord : notnull =>
        from wells in strip.Wells.ValueIO
        let count = held.Shown.Map(record => strip.Listed(record).Count).IfNone(0)
        from enabled in IO.lift(() => {
            strip.Add.Enabled = count > 0;
            strip.Remove.Enabled = held.Selected.Exists(at => at < count);
        })
        from painted in wells.After.Zip(Entries(strip, held))
            .Map((pair, index) => Restyled(pair.First.Well, pair.Second.Map(static swatch => swatch.Display), Frame(held.Selected, index)))
            .TraverseM(static paint => paint)
            .As()
        select unit;

    private static PaintSlot Frame(Option<int> selected, int index) => selected.Exists(at => at == index) ? PaintSlot.Selection : PaintSlot.FrameEdge;

    private static IO<(Well Well, IDisposable Release)> Placed<TRecord>(Strip<TRecord> strip, Option<Swatch> entry, int index) where TRecord : notnull =>
        from welled in Welled(new SwatchEncoding.DisplayReferred(), new Size(SwatchSide, SwatchSide), Selected(strip, Some(index)), strip.Scope.Sink)
        from sessions in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(entry.ToSeq().Bind(_ => Session(welled.Well, strip.Edit, Indexed(strip, index), strip.Scope, strip.Site)), DisposalOps.Release),
            IO.lift(welled.Release.Dispose))
        from enabled in IO.lift(() => { welled.Well.Dialog.Enabled = entry.IsSome; })
        select (welled.Well, DisposalOps.Composite(welled.Release.Cons(sessions), strip.Site));

    private static Func<Vector4, TRecord, Fin<TRecord>> Indexed<TRecord>(Strip<TRecord> strip, int index) where TRecord : notnull =>
        (color, record) => strip.Revise(record, list => Replaced(list, index, Swatch.From(color)));

    private static Seq<Swatch> Replaced(Seq<Swatch> list, int index, Swatch swatch) =>
        list.Map((held, at) => at == index ? swatch : held);

    private static Seq<IO<IDisposable>> Session<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, CallbackSite site)
        where TRecord : notnull =>
        Seq(Subscriptions.Attach(h => well.Dialog.Executed += h, h => well.Dialog.Executed -= h, Opened(well, edit, write, scope, site)),
            ChoiceRows.Context(well.Button, Menu(well, edit, write), [], scope.Sink)(None, [], IO.pure(unit)));

    private static IO<(Well Well, IDisposable Release)> Welled(SwatchEncoding encoding, Size size, IO<Unit> touched, IPlugInSink sink) =>
        from dialog in IO.lift(static () => RowText.Localize("Color Picker…").Local switch {
            var shown => new Command { MenuText = shown, ToolTip = shown },
        })
        from button in IO.lift(() => new ImageButton { Command = dialog, Size = size, ToolTip = dialog.ToolTip })
        from look in IO.lift(static () => Atom((Stored: Option<Vector4>.None, Frame: PaintSlot.FrameEdge)))
        from image in IO.lift(static () => Atom((Before: Option<Raster>.None, After: Option<Raster>.None)))
        let well = new Well(button, dialog, encoding, touched, look, image)
        let site = new CallbackSite(sink, typeof(ImageButton), nameof(ImageButton.Image))
        from held in DisposalOps.AcquireAll(
            Seq(IO.pure<IDisposable>(new Disposal<Well>(well, static owned => Released(owned))),
                Subscriptions.Attach(h => button.SizeChanged += h, h => button.SizeChanged -= h, Callbacks.Handler<EventArgs>(_ => Painted(well), site)),
                HostTheme.Changed.Inline(_ => Painted(well), sink)),
            DisposalOps.Release)
        select (well, DisposalOps.Composite(held, site));

    private static Unit Released(Well well) {
        well.Button.Command = null;
        return Freed(well.Image.Value.After);
    }

    private static Unit Freed(Option<Raster> raster) =>
        raster.Iter(static held => {
            held.Image.Dispose();
            held.Pixels.Dispose();
        });

    private static EventHandler<EventArgs> Opened<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, CallbackSite site)
        where TRecord : notnull =>
        Callbacks.Handler<EventArgs>(_ => Dialog(well, edit, write, scope, site), site);

    private static IO<Unit> Dialog<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, RowScope scope, CallbackSite site) where TRecord : notnull =>
        (from touched in well.Touched
         from document in IO.lift(scope.Document.ToFin(new Missing(nameof(RowScope.Document))))
         from look in well.Look.ValueIO
         from chosen in HostDialogs.ShowColorDialog(
             document, well.Encoding.Shown(look.Stored.IfNone(Vector4.One)), well.Encoding.Alpha, None, Some((Previewed(well, edit, write), site)))
         from taken in edit.Take(new Edit<Vector4>.Commit(well.Encoding.Stored(chosen)), write)
         select taken)
        .Catch(static error => error.Is(Errors.Cancelled), _ => edit.Take(new Edit<Vector4>.Cancel(), write));

    private static Func<Color4f, IO<Unit>> Previewed<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write) where TRecord : notnull =>
        color => edit.Take(new Edit<Vector4>.Preview(well.Encoding.Stored(color)), write);

    private static Seq<Seq<MenuEntry>> Menu<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write) where TRecord : notnull =>
        Seq(Seq<MenuEntry>(well.Dialog, new MenuEntry.Listed(RowText.Localize("Named Colors").Local, well.Look.ValueIO.Map(look => Named(well, edit, write, look.Stored)))));

    private static Seq<MenuPick> Named<TRecord>(Well well, RowEdit<TRecord> edit, Func<Vector4, TRecord, Fin<TRecord>> write, Option<Vector4> stored) where TRecord : notnull =>
        toSeq(NamedColorList.Default).Map<MenuPick>(named => new MenuPick.Item(
            named.Name,
            stored.Exists(held => well.Encoding.Shown(held).AsSystemColor().ToArgb() == named.Color.ToArgb()),
            well.Touched.Bind(_ => edit.Take(new Edit<Vector4>.Commit(well.Encoding.Stored(new Color4f(named.Color))), write))));

    private static IO<Unit> Restyled(Well well, Option<Vector4> stored, PaintSlot frame) =>
        well.Look.SwapIO(_ => (stored, frame)).Bind(_ => Painted(well));

    private static IO<Unit> Painted(Well well) =>
        from look in well.Look.ValueIO
        from scale in HostTheme.Scale
        from size in IO.lift(() => well.Button.Size)
        from painted in when(size.Width > 0 && size.Height > 0, Imaged(well, look, new RectangleF(size), scale)).As()
        select painted;

    private static IO<Unit> Imaged(Well well, (Option<Vector4> Stored, PaintSlot Frame) look, RectangleF bounds, float scale) =>
        from pixels in IO.lift(() => new Bitmap(Size.Ceiling(bounds.Size * scale), PixelFormat.Format32bppRgba))
        from image in DisposalOps.OnFailure(Rendered(well, look, bounds, scale, pixels), IO.lift(pixels.Dispose))
        from shown in IO.lift(() => { well.Button.Image = image; })
        from swapped in well.Image.SwapIO(prior => (prior.After, Some(new Raster(pixels, image))))
        select Freed(swapped.Before);

    private static IO<Icon> Rendered(Well well, (Option<Vector4> Stored, PaintSlot Frame) look, RectangleF bounds, float scale, Bitmap pixels) =>
        (from graphics in use(() => new Graphics(pixels))
         from scaled in IO.lift(() => graphics.ScaleTransform(scale, scale))
         from canvas in PlotCanvas.Of(graphics, scale, EtoFonts.SmallFont, well.Button.Enabled, focused: false)
         from drawn in Drawn(canvas, bounds, well.Encoding, look.Stored, look.Frame)
         select drawn).Bracket().Map(_ => new Icon(scale, pixels));

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

    private static string Hex(Swatch swatch) => $"#{(int)swatch:X6}";

    private sealed record Raster(Bitmap Pixels, Icon Image);

    private sealed record Well(
        ImageButton Button, Command Dialog, SwatchEncoding Encoding, IO<Unit> Touched,
        Atom<(Option<Vector4> Stored, PaintSlot Frame)> Look, Atom<(Option<Raster> Before, Option<Raster> After)> Image);

    private sealed record Strip<TRecord>(
        Panel Panel, AddRemoveButton Pair, Command Add, Command Remove, Atom<StripState<TRecord>> State,
        Atom<(Seq<(Well Well, IDisposable Release)> Before, Seq<(Well Well, IDisposable Release)> After)> Wells, IO<TRecord> Read,
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
        ControlRow.Of(source, fields, caption, help, RowShape.Inline, scope => Paired(source, fields, display, print, offered, resolved, scope), rules);

    private static IO<RowCells> Paired<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> display,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> print,
        LanguageExt.HashSet<DisplayAndPrintColorSourceMode> offered, Option<Func<TRecord, IO<(System.Drawing.Color Display, System.Drawing.Color Print)>>> resolved, RowScope scope)
        where TRecord : notnull =>
        from picker in IO.lift(() => new DisplayAndPrintColorPicker {
            PickerMode = DisplayAndPrintColorPickerMode.Dual,
            ShowByLayer = offered.Contains(DisplayAndPrintColorSourceMode.ByLayer),
            ShowByObject = offered.Contains(DisplayAndPrintColorSourceMode.ByObject),
            ShowByParent = offered.Contains(DisplayAndPrintColorSourceMode.ByParent),
            ShowViewport = offered.Contains(DisplayAndPrintColorSourceMode.Viewport),
            ShowNone = offered.Contains(DisplayAndPrintColorSourceMode.None),
        })
        let site = new CallbackSite(scope.Sink, typeof(DisplayAndPrintColorPicker), nameof(DisplayAndPrintColorPicker.PropertyChanged))
        from bound in RowEdit.Bind<TRecord, PropertyChangedEventHandler>(source, fields, h => picker.PropertyChanged += h, h => picker.PropertyChanged -= h,
            edit => Callbacks.Handler<PropertyChangedEventArgs>(_ => Changed(picker, display, print, edit, source, scope), site).Invoke,
            value => value.Match(Some: record => Matched(picker, display.Get(record), print.Get(record), resolved.Map(read => read(record))), None: () => Mixed(picker)),
            scope, site)
        select new RowCells(
            picker, None, None, None, [], [], new RowHelp(None, None), bound.Edit.Shown, Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(picker, [], [], scope.Sink)), bound.Release);

    private static IO<Unit> Changed<TRecord>(
        DisplayAndPrintColorPicker picker, Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> display,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> print, RowEdit<TRecord> edit, RowSource<TRecord> source, RowScope scope)
        where TRecord : notnull =>
        from held in scope.Read(source)
        from read in IO.lift(() => (
            Display: (picker.DisplaySourceMode, picker.DisplayColor.ToSystemDrawing()),
            Print: (picker.PrintSourceMode, picker.PrintColor.ToSystemDrawing())))
        from committed in when(!Same(display.Get(held), read.Display) || !Same(print.Get(held), read.Print), edit.Commit(print.Set(read.Print, display.Set(read.Display, held)))).As()
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
    public static ControlRow Wheels(
        RowSource<WheelsState> source, Func<ToneWheel, (RowField<WheelsState> Hue, RowField<WheelsState> Strength, RowField<WheelsState> Luma)> fields,
        string caption, string help, Func<ToneWheel, string> range, Gamut working, RowRules rules) =>
        IterableNE.create(ToneWheel.Items[0], ToneWheel.Items.Skip(1)).Bind(wheel => fields(wheel) switch { var (hue, strength, luma) => IterableNE.create(hue, strength, luma) }) switch {
            var members => ControlRow.Of(source, members, caption, help, RowShape.Block, scope => Wheeled(source, members, fields, range, working, scope), rules),
        };

    private static IO<RowCells> Wheeled(
        RowSource<WheelsState> source, IterableNE<RowField<WheelsState>> members,
        Func<ToneWheel, (RowField<WheelsState> Hue, RowField<WheelsState> Strength, RowField<WheelsState> Luma)> fields, Func<ToneWheel, string> range, Gamut working, RowScope scope) =>
        from record in scope.Read(source)
        from shown in IO.lift(() => Atom(Opening(record)))
        let texts = (Hue: NumberText.Scalar(WheelsParameter.Hue, RowEdit.Varies), Strength: NumberText.Scalar(WheelsParameter.Strength, RowEdit.Varies))
        let faced = fun((ToneWheel wheel) => new ShownWheel(Wording.English.Shown(range(wheel), scope.Sink), texts.Hue, texts.Strength))
        let current = fun(() => shown.Value)
        from wheel in IO.lift(() => new GradingWheel(scope.Sink, faced(current())))
        let site = new CallbackSite(scope.Sink, typeof(GradingWheel), nameof(GradingWheel.Edited))
        from bound in RowEdit.Bind(source, members, h => wheel.Edited += h, h => wheel.Edited -= h,
            edit => Callbacks.Handler<EditEventArgs<WheelOffset>>(args => edit.Take(args.Edit, (offset, held) => Fin.Succ(current().Lens.Set(offset, held))), site),
            value => IO.lift(current).Bind(chosen => wheel.Receive(value.Map(chosen.Lens.Get))), scope, site)
        from hue in DisposalOps.OnFailure(
            NumericRows.Line<WheelsState, WheelHue, float, InvalidGrade>(source, fields(current()).Hue, Followed(current, static tone => tone.Hue), WheelsParameter.Hue, bound.Edit, None, scope.Sink),
            IO.lift(bound.Release.Dispose))
        from strength in DisposalOps.OnFailure(
            NumericRows.Line<WheelsState, WheelStrength, float, InvalidGrade>(
                source, fields(current()).Strength, Followed(current, static tone => tone.Strength), WheelsParameter.Strength, bound.Edit, None, scope.Sink),
            DisposalOps.Release(Seq(bound.Release, hue.Release)))
        from luma in DisposalOps.OnFailure(
            NumericRows.Line<WheelsState, WheelLuma, float, InvalidGrade>(source, fields(current()).Luma, Followed(current, static tone => tone.Luma), WheelsParameter.Luma, bound.Edit, None, scope.Sink),
            DisposalOps.Release(Seq(bound.Release, hue.Release, strength.Release)))
        let lined = Seq(bound.Release, hue.Release, strength.Release, luma.Release)
        from lanes in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                toSeq(TraceChannel.Items).Choose(channel => channel.Lane.Map(lane => Laned(channel, lane, WheelOffset.LaneBounds(working), current, bound.Edit, working, scope.Sink))),
                static made => DisposalOps.Release(made.Map(static lane => lane.Release))),
            DisposalOps.Release(lined))
        let lines = Lines(bound.Edit, scope.Read(source), Seq(hue.Show, strength.Show, luma.Show) + lanes.Map(static lane => lane.Show))
        let held = lined + lanes.Map(static lane => lane.Release)
        from popup in DisposalOps.OnFailure(
            ChoiceRows.Popup(
                toSeq(ToneWheel.Items).Map(item => new ChoiceItem<ToneWheel>(item, Wording.English.Shown(range(item), scope.Sink), None)),
                picked => Switched(shown, wheel, faced(picked), picked).Bind(_ => lines), scope.Sink),
            DisposalOps.Release(held))
        select new RowCells(
            popup.Control, None, None, None, [],
            Seq(new RowLine(Some(""), wheel, None, None), hue.Line, strength.Line, luma.Line) + lanes.Map(static lane => lane.Line),
            new RowHelp(None, None), lines.Bind(_ => IO.lift(current)).Bind(chosen => popup.Show(Some(chosen))), Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(wheel, [], [], scope.Sink)), DisposalOps.Composite(held.Add(popup.Release), site));

    private static IO<Unit> Switched(Atom<ToneWheel> shown, GradingWheel wheel, ShownWheel faced, ToneWheel picked) =>
        shown.SwapIO(_ => picked).Bind(_ => wheel.Show(faced));

    private static ToneWheel Opening(WheelsState record) =>
        toSeq(ToneWheel.Items).Find(wheel => wheel.Lens.Get(record) != WheelOffset.Neutral).IfNone(ToneWheel.Global);

    private static Lens<WheelsState, T> Followed<T>(Func<ToneWheel> current, Func<ToneWheel, Lens<WheelsState, T>> member) =>
        Lens<WheelsState, T>.New(held => member(current()).Get(held), value => held => member(current()).Set(value, held));

    private static IO<(RowLine Line, Func<Option<WheelsState>, IO<Unit>> Show, IDisposable Release)> Laned(
        TraceChannel channel, Lens<Vector3, float> lane, (Vector3 Low, Vector3 High) bounds, Func<ToneWheel> current, RowEdit<WheelsState> edit, Gamut working, IPlugInSink sink) =>
        from text in IO.pure(NumberText.Scalar(new Presentation<MixerWeight, float> { Soft = (lane.Get(bounds.Low), lane.Get(bounds.High)) }, RowEdit.Varies))
        from field in IO.lift(() => new NumberField<MixerWeight, float, InvalidGrade>(text, sink))
        from edited in Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h,
            Callbacks.Handler<EditEventArgs<MixerWeight>>(args => edit.Take(args.Edit, (value, held) => Crossed(current(), lane, working, value, held)), new CallbackSite(sink, typeof(ColorRows), nameof(Wheels))))
        select (
            new RowLine(Some(channel.Map(luma: "Luma", red: "Red", green: "Green", blue: "Blue")), field, None,
                Some(new FieldFit(field, text.TextWidth(field.Font) + (2f * NumericRows.FieldInset)))),
            (Func<Option<WheelsState>, IO<Unit>>)(value => IO.lift(current).Bind(wheel => field.Receive(Lane(wheel, lane, working, value)))),
            edited);

    private static Fin<WheelsState> Crossed(ToneWheel wheel, Lens<Vector3, float> lane, Gamut working, float value, WheelsState held) =>
        wheel.Lens.Get(held) switch {
            var offset => WheelOffset.FromMultiplier(lane.Set(value, offset.Multiplier(working)), working, offset.Hue).Map(next => wheel.Lens.Set(next, held)),
        };

    private static Option<MixerWeight> Lane(ToneWheel wheel, Lens<Vector3, float> lane, Gamut working, Option<WheelsState> value) =>
        from record in value
        from weight in Conversions.Validated<MixerWeight, float, InvalidGrade>(lane.Get(wheel.Lens.Get(record).Multiplier(working))).ToOption()
        select weight;

    private static IO<Unit> Lines<TRecord>(RowEdit<TRecord> edit, IO<TRecord> read, Seq<Func<Option<TRecord>, IO<Unit>>> shows) where TRecord : notnull =>
        from bracketed in edit.Shown
        from record in read
        from shown in shows.TraverseM(show => show(Some(record))).As()
        select unit;

    // --- [GRADIENTS]
    public static ControlRow Gradient<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Ramp> lens, Func<RampInterpolation, string> caption, Gamut working, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Block, scope => Ramped(source, field, lens, caption, working, scope), rules);

    public static Fin<Ramp> RampOf(ColorGradient gradient) {
        Seq<ColorStop> stops = toSeq(gradient.GetColorStops());
        Vector4[] light = [.. stops.Map(static stop => new Vector4(stop.Color.R, stop.Color.G, stop.Color.B, stop.Color.A) / byte.MaxValue)];
        TransferCurve.Srgb.Decode(light, Nits.ReferenceWhite);
        return stops.Zip(toSeq(light))
            .Traverse(static pair => (
                from position in Conversions.Validated<RampPosition, double, InvalidPixelValue>(pair.First.Position)
                from stop in RampStop.Validate(position, pair.Second, out RampStop item) is { } error ? error : (Fin<RampStop>)item
                select stop).ToValidation())
            .As()
            .ToFin()
            .Bind(static held => Ramp.Validate(held, RampInterpolation.Linear, out Ramp? ramp) is { } error ? error : (Fin<Ramp>)ramp!);
    }

    public static ColorGradient GradientOf(ColorGradient held, Ramp ramp) {
        Vector4[] encoded = [.. ramp.Stops.Map(static stop => stop.Color)];
        TransferCurve.Srgb.Encode(encoded, Nits.ReferenceWhite);
        ColorGradient written = held.Duplicate();
        written.SetColorStops(ramp.Stops.Zip(toSeq(encoded), static (stop, color) => new ColorStop(
            System.Drawing.Color.FromArgb((int)Swatch.From(new Vector4(color.W)) & byte.MaxValue, System.Drawing.Color.FromArgb(Swatch.From(color))), stop.Position)));
        return written;
    }

    private static IO<RowCells> Ramped<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, Ramp> lens, Func<RampInterpolation, string> caption, Gamut working, RowScope scope)
        where TRecord : notnull =>
        from text in IO.pure(NumberText.Scalar(new Presentation<RampPosition, double>(), RowEdit.Varies))
        from bar in IO.lift(() => new GradientRamp(scope.Sink, Wording.English.Shown(field.Caption, scope.Sink), new GradientRampState.Editor(working, text, None, None, None)))
        from stop in IO.lift(static () => Atom((Ramp: Option<Ramp>.None, Selected: Option<int>.None)))
        let site = new CallbackSite(scope.Sink, typeof(GradientRamp), nameof(GradientRamp.Edited))
        from bound in RowEdit.Bind(source, IterableNE.create(field), h => bar.Edited += h, h => bar.Edited -= h,
            edit => Callbacks.Handler<EditEventArgs<Ramp>>(args => Dragged(edit, stop, lens, args.Edit), site), value => bar.Receive(value.Map(lens.Get)), scope, site)
        from popup in DisposalOps.OnFailure(
            ChoiceRows.Popup(
                ChoiceRows.Items<RampInterpolation, InvalidPixelValue>(caption, None, scope.Sink),
                picked => bound.Edit.Take(new Edit<RampInterpolation>.Commit(picked), (interpolation, held) => Interpolated(lens, interpolation, held)), scope.Sink),
            IO.lift(bound.Release.Dispose))
        let opened = Seq(bound.Release, popup.Release)
        from welled in DisposalOps.OnFailure(Welled(new SwatchEncoding.SceneLinear(working), new Size(-1, SwatchSide), IO.pure(unit), scope.Sink), DisposalOps.Release(opened))
        let held = opened.Add(welled.Release)
        from position in IO.lift(() => new NumberField<RampPosition, double, InvalidPixelValue>(text, scope.Sink))
        from line in IO.lift(() => new TableLayout(new TableRow(new TableCell(welled.Well.Button, scaleWidth: true), new TableCell(position))) {
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table),
        })
        let stopped = Stopped(stop.ValueIO, welled.Well, position)
        from attached in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                Session(welled.Well, bound.Edit, (color, record) => Recolored(lens, stop.Value.Selected, color, record), scope, site)
                + Seq(Subscriptions.Attach(h => bar.Selected += h, h => bar.Selected -= h,
                          Callbacks.Handler<StopSelectedEventArgs>(selected => Chosen(stop, selected.Index).Bind(_ => stopped), site with { Member = nameof(GradientRamp.Selected) })),
                      Subscriptions.Attach(h => position.Edited += h, h => position.Edited -= h,
                          Callbacks.Handler<EditEventArgs<RampPosition>>(args => Positioned(bound.Edit, stop, bar, lens, args.Edit), site with { Member = nameof(NumberField<,,>.Edited) }))),
                DisposalOps.Release),
            DisposalOps.Release(held))
        select new RowCells(
            popup.Control, None, None, None, [],
            Seq(new RowLine(Some(""), bar, None, None), new RowLine(Some("Stop"), line, None, Some(new FieldFit(position, text.TextWidth(position.Font) + (2f * NumericRows.FieldInset))))),
            new RowHelp(None, None), Restopped(bound.Edit, scope.Read(source), lens, popup.Show, stop, stopped), Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(bar, [], [], scope.Sink)), DisposalOps.Composite(held + attached, site));

    private static IO<Unit> Dragged<TRecord>(RowEdit<TRecord> edit, Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, Lens<TRecord, Ramp> lens, Edit<Ramp> change)
        where TRecord : notnull =>
        stop.SwapIO(held => held with { Ramp = Carried(change) | held.Ramp }).Bind(_ => edit.Take(change, (ramp, record) => Fin.Succ(lens.Set(ramp, record))));

    private static IO<Unit> Chosen(Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, Option<int> at) =>
        stop.SwapIO(held => held with { Selected = at }).Map(static _ => unit);

    private static Option<Ramp> Carried(Edit<Ramp> change) =>
        change.Switch<Option<Ramp>>(preview: static held => held.Value, step: static held => held.Value, commit: static held => held.Value, cancel: static _ => None);

    private static Fin<TRecord> Interpolated<TRecord>(Lens<TRecord, Ramp> lens, RampInterpolation interpolation, TRecord record) where TRecord : notnull =>
        Ramp.Validate(lens.Get(record).Stops, interpolation, out Ramp? ramp) is { } error ? error : (Fin<TRecord>)lens.Set(ramp!, record);

    private static Fin<TRecord> Recolored<TRecord>(Lens<TRecord, Ramp> lens, Option<int> selected, Vector4 color, TRecord record) where TRecord : notnull =>
        from at in selected.ToFin(new Missing(nameof(GradientRamp.Selected)))
        let ramp = lens.Get(record)
        from current in ramp.Stops.At(at).ToFin(new IndexOutOfRange(nameof(Ramp.Stops), at, ramp.Stops.Count))
        from item in RampStop.Validate(current.Position, color, out RampStop next) is { } refused ? refused : (Fin<RampStop>)next
        from written in Ramp.Validate(ramp.Stops.Map((kept, index) => index == at ? item : kept), ramp.Interpolation, out Ramp? replaced) is { } error ? error : (Fin<Ramp>)replaced!
        select lens.Set(written, record);

    private static IO<Unit> Stopped(IO<(Option<Ramp> Ramp, Option<int> Selected)> stop, Well well, NumberField<RampPosition, double, InvalidPixelValue> position) =>
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
        from bracketed in edit.Shown
        from record in read
        from chosen in interpolation(Some(lens.Get(record).Interpolation))
        from moved in stop.SwapIO(state => state with { Ramp = Some(lens.Get(record)) })
        from shown in stopped
        select unit;

    private static IO<Unit> Positioned<TRecord>(
        RowEdit<TRecord> edit, Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, GradientRamp bar, Lens<TRecord, Ramp> lens, Edit<RampPosition> change) where TRecord : notnull =>
        from state in stop.ValueIO
        from taken in edit.Take(change, (value, record) => state.Selected.ToFin(new Missing(nameof(GradientRamp.Selected))).Map(at => lens.Set(lens.Get(record).Moved(at, value).Ramp, record)))
        from reselected in (from value in StepOrCommit(change) from ramp in state.Ramp from at in state.Selected select ramp.Moved(at, value)).Traverse(moved => Reselected(stop, bar, moved)).As()
        select unit;

    private static Option<RampPosition> StepOrCommit(Edit<RampPosition> change) =>
        change.Switch<Option<RampPosition>>(preview: static _ => None, step: static held => held.Value, commit: static held => held.Value, cancel: static _ => None);

    private static IO<Unit> Reselected(Atom<(Option<Ramp> Ramp, Option<int> Selected)> stop, GradientRamp bar, (Ramp Ramp, int Index) moved) =>
        stop.SwapIO(held => held with { Ramp = Some(moved.Ramp) }).Bind(_ => bar.Select(Some(moved.Index)));

    // --- [QUALIFIERS]
    public static ControlRow Qualifier<TRecord>(
        RowSource<TRecord> source, string caption, string help, RangeAxis axis, Lens<TRecord, HueRange> range,
        (RowField<TRecord> Center, RowField<TRecord> Width, RowField<TRecord> Softness) band,
        Option<Func<TRecord, Vector4, Vector4>> grade, StageInput stage, RowRules rules) where TRecord : notnull =>
        IterableNE.create(band.Center, band.Width, band.Softness) switch {
            var members => ControlRow.Of(source, members, caption, help, RowShape.Block, scope => Qualified(source, members, caption, axis, range, band, grade, stage, scope), rules),
        };

    private static IO<RowCells> Qualified<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> members, string caption, RangeAxis axis, Lens<TRecord, HueRange> range,
        (RowField<TRecord> Center, RowField<TRecord> Width, RowField<TRecord> Softness) band, Option<Func<TRecord, Vector4, Vector4>> grade, StageInput stage, RowScope scope)
        where TRecord : notnull =>
        from text in IO.pure(NumberText.Scalar(axis.Presentation, RowEdit.Varies))
        from bar in IO.lift(() => new HueRangeBar(scope.Sink, axis, Wording.English.Shown(caption, scope.Sink), new BandTexts(text, text, text)))
        let site = new CallbackSite(scope.Sink, typeof(HueRangeBar), nameof(HueRangeBar.Edited))
        let lenses = axis.Members(range)
        from bound in RowEdit.Bind(source, members, h => bar.Edited += h, h => bar.Edited -= h,
            edit => Callbacks.Handler<EditEventArgs<HueRange>>(args => edit.Take(args.Edit, (value, held) => Fin.Succ(range.Set(value, held))), site),
            value => Banded(bar, range, grade, value), scope, site)
        from pick in DisposalOps.OnFailure(Pick(stage, bound.Edit, (color, held) => Centered(axis, lenses.Center, color, held), scope), IO.lift(bound.Release.Dispose))
        let opened = Seq(bound.Release, pick.Release)
        from center in DisposalOps.OnFailure(
            NumericRows.Line<TRecord, AxisFraction, float, InvalidGrade>(source, band.Center, lenses.Center, axis.Presentation, bound.Edit, Some<Control>(pick.Control), scope.Sink),
            DisposalOps.Release(opened))
        from width in DisposalOps.OnFailure(
            NumericRows.Line<TRecord, AxisFraction, float, InvalidGrade>(source, band.Width, lenses.Width, axis.Presentation, bound.Edit, None, scope.Sink),
            DisposalOps.Release(opened.Add(center.Release)))
        from softness in DisposalOps.OnFailure(
            NumericRows.Line<TRecord, AxisFraction, float, InvalidGrade>(source, band.Softness, lenses.Softness, axis.Presentation, bound.Edit, None, scope.Sink),
            DisposalOps.Release(opened + Seq(center.Release, width.Release)))
        select new RowCells(
            bar, None, None, None, [], Seq(center.Line, width.Line, softness.Line), new RowHelp(None, None),
            Lines(bound.Edit, scope.Read(source), Seq(center.Show, width.Show, softness.Show)), Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(bar, [], [], scope.Sink)), DisposalOps.Composite(opened + Seq(center.Release, width.Release, softness.Release), site));

    private static IO<Unit> Banded<TRecord>(HueRangeBar bar, Lens<TRecord, HueRange> range, Option<Func<TRecord, Vector4, Vector4>> grade, Option<TRecord> value) where TRecord : notnull =>
        bar.Graded(from record in value from held in grade select Graded(range, held, record)).Bind(_ => bar.Receive(value.Map(range.Get)));

    private static Func<HueRange, Vector4, Vector4> Graded<TRecord>(Lens<TRecord, HueRange> range, Func<TRecord, Vector4, Vector4> grade, TRecord record) where TRecord : notnull =>
        (shown, color) => grade(range.Set(shown, record), color);

    private static Fin<TRecord> Centered<TRecord>(RangeAxis axis, Lens<TRecord, AxisFraction> center, Vector4 color, TRecord record) where TRecord : notnull =>
        Conversions.Validated<AxisFraction, float, InvalidGrade>(axis.Coordinate(Hsy.From(color))).Map(value => center.Set(value, record));
}
