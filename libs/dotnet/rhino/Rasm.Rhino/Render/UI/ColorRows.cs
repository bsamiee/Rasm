using System.ComponentModel;
using System.Numerics;
using Eto.Forms;
using Rasm.Imaging.Color;
using Rasm.Imaging.Grade;
using Rasm.Imaging.Pixels;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Imaging;
using Rasm.UI.Styling;
using Rhino.Display;
using Rhino.UI;
using Rhino.UI.Controls;
using SkiaSharp;
using Wacton.Unicolour;

namespace Rasm.Rhino.Render.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SwatchEncoding {
    public abstract bool Alpha { get; }
    public abstract Gamut Gamut { get; }
    public abstract Color4f Shown(Vector4 stored);
    public abstract Vector4 Stored(Color4f shown);
    public abstract Vector4 Light(Vector4 stored);

    public sealed record DisplayReferred : SwatchEncoding {
        public override bool Alpha => false;
        public override Gamut Gamut => Gamut.StandardRgb;
        public override Color4f Shown(Vector4 stored) => new(stored.X, stored.Y, stored.Z, 1f);
        public override Vector4 Stored(Color4f shown) => new(shown.R, shown.G, shown.B, 1f);

        public override Vector4 Light(Vector4 stored) {
            Vector4[] light = [stored];
            TransferCurve.Srgb.Decode(light, Nits.ReferenceWhite);
            return light[0];
        }
    }

    public sealed record SceneLinear(Gamut Working) : SwatchEncoding {
        public override bool Alpha => true;
        public override Gamut Gamut => Working;
        public override Vector4 Light(Vector4 stored) => stored;

        public override Color4f Shown(Vector4 stored) =>
            new Unicolour(Working.Configuration, ColourSpace.RgbLinear, stored.X, stored.Y, stored.Z).ConvertToConfiguration(Configuration.Default).MapToRgbGamut().Rgb switch {
                var rgb => new Color4f((float)rgb.R, (float)rgb.G, (float)rgb.B, stored.W),
            };

        public override Vector4 Stored(Color4f shown) =>
            new Unicolour(Configuration.Default, ColourSpace.Rgb, shown.R, shown.G, shown.B, shown.A).ConvertToConfiguration(Working.Configuration) switch {
                var color => new((float)color.RgbLinear.R, (float)color.RgbLinear.G, (float)color.RgbLinear.B, (float)color.Alpha.A),
            };
    }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneRow<TRecord, TLocal> where TRecord : notnull where TLocal : notnull {
    // --- [STATE]
    private readonly RowSource<TRecord> source;
    private readonly Func<SceneRow<TRecord, TLocal>, TRecord, TLocal, Node> draw;
    private readonly Func<SceneRow<TRecord, TLocal>, Option<TRecord>, TLocal, IO<Unit>> beside;
    private readonly Atom<(Option<TRecord> Record, TLocal Local)> state;
    private readonly Atom<Option<Func<RowEdit<TRecord>>>> attached = Atom(Option<Func<RowEdit<TRecord>>>.None);

    private SceneRow(
        RowSource<TRecord> source, TLocal local, Func<SceneRow<TRecord, TLocal>, TRecord, TLocal, Node> draw,
        Func<SceneRow<TRecord, TLocal>, Option<TRecord>, TLocal, IO<Unit>> beside, RowScope scope) {
        (this.source, this.draw, this.beside) = (source, draw, beside);
        state = Atom((Record: Option<TRecord>.None, Local: local));
        View = new SceneView(scope, Drawn(None, local));
    }

    public SceneView View { get; }

    public TLocal Held => state.Value.Local;

    public IO<Option<TRecord>> Shown => state.ValueIO.Map(static held => held.Record);

    // --- [BINDING]
    public static IO<(SceneRow<TRecord, TLocal> Row, RowEdit<TRecord> Edit)> Bind(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> fields, TLocal local, Func<SceneRow<TRecord, TLocal>, TRecord, TLocal, Node> draw,
        Func<SceneRow<TRecord, TLocal>, Option<TRecord>, TLocal, IO<Unit>> beside, RowScope scope, CallbackSite site, Action<IDisposable> retain) =>
        from row in DisposalOps.Acquire<SceneRow<TRecord, TLocal>, SceneView>(IO.lift(() => new SceneRow<TRecord, TLocal>(source, local, draw, beside, scope)), static held => held.View, retain)
        from bound in RowEdit.Bind<TRecord, Func<RowEdit<TRecord>>>(
            source, fields, held => row.attached.Swap(_ => Some(held)), _ => row.attached.Swap(static _ => None), static edit => () => edit, row.Show, scope, site, retain)
        select (row, bound);

    // --- [SHOW]
    public IO<Unit> Show(Option<TRecord> record) => state.SwapIO(held => held with { Record = record }).Bind(Redrawn);

    public IO<Unit> Change(Func<TLocal, TLocal> change) => state.SwapIO(held => held with { Local = change(held.Local) }).Bind(Redrawn);

    private IO<Unit> Redrawn((Option<TRecord> Record, TLocal Local) held) => View.Show(Drawn(held.Record, held.Local)).Bind(_ => beside(this, held.Record, held.Local));

    private Node Drawn(Option<TRecord> record, TLocal local) =>
        record.Match(
            Some: held => draw(this, held, local),
            None: () => draw(this, source.Default, local) switch { var mixed => mixed with { Conditions = mixed.Conditions.TryAdd(Condition.Mixed) } });

    // --- [EDITS]
    public Func<Edit<TValue>, IO<Unit>> Edited<TValue>(Func<TValue, TRecord, Fin<TRecord>> written) where TValue : notnull =>
        change => Taken(Hosted(change), written);

    public IO<Unit> Taken<TValue>(Rhino.UI.Drawing.Edit<TValue> change, Func<TValue, TRecord, Fin<TRecord>> written) where TValue : notnull =>
        from held in state.ValueIO
        from shown in change.Carried.Bind(value => written(value, held.Record.IfNone(source.Default)).ToOption()).Traverse(next => Show(Some(next))).As()
        from taken in attached.Value.Traverse(edit => edit().Take(change, written)).As()
        from synced in change.Carried.IsSome ? IO.pure(unit) : attached.Value.Traverse(static edit => edit().Shown).As().Map(static _ => unit)
        select unit;

    private static Rhino.UI.Drawing.Edit<TValue> Hosted<TValue>(Edit<TValue> change) where TValue : notnull =>
        change.Switch<Rhino.UI.Drawing.Edit<TValue>>(
            preview: static held => new Rhino.UI.Drawing.Edit<TValue>.Preview(held.Value),
            step: static held => new Rhino.UI.Drawing.Edit<TValue>.Step(held.Value),
            commit: static held => new Rhino.UI.Drawing.Edit<TValue>.Commit(held.Value),
            cancel: static _ => new Rhino.UI.Drawing.Edit<TValue>.Cancel());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ColorRows {
    // --- [SWATCHES]
    private const string PickCaption = "Pick Color";
    private const string PickerCaption = "Color Picker…";

    private static readonly Sizing Grown = new Sizing.Grow(Factor.Create(1f));
    private static readonly SwatchEncoding Displayed = new SwatchEncoding.DisplayReferred();

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
        from box in gate.Traverse(_ => DisposalOps.Acquire(IO.lift(static () => new CheckBox()), retain)).As()
        let gated = from member in gate from shown in box select (Lens: member, Box: shown)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(SwatchColor))
        let caption = RowText.Localize(field.Caption, scope.Sink).Local
        let write = Written(swatch)
        from scened in SceneRow<TRecord, Unit>.Bind(
            source, IterableNE.create(field), unit,
            (row, record, _) => Face(Displayed, Some(swatch.Get(record).Display), gate.ForAll(held => held.Get(record)) ? [] : [Condition.Disabled], caption,
                _ => Dialog(Single(row, swatch, gate, write), scope, site)),
            (_, record, _) => Boxed(gated, record), scope, site, retain)
        let well = Single(scened.Row, swatch, gate, write)
        from gating in gated.Traverse(pair => Gating(source, scened.Edit, pair.Lens, pair.Box, scope, site, retain)).As()
        from pick in stage.Traverse(input => PreviewRows.Pick(input, PickCaption, mean => well.Edited(new Edit<Vector4>.Commit(mean)), scope, retain)).As()
        select new RowCells(
            scened.Row.View, box.Map(static shown => (Control)shown), pick, None, [], [],
            new RowHelp(None, Some(Conversions.Hex(swatch.Get(source.Default)))), scened.Edit.Shown, Some<RowEdit>(scened.Edit),
            Some(ChoiceRows.Context(scened.Row.View, Menu(IO.pure(well), site), [])));

    private static Func<Vector4, TRecord, Fin<TRecord>> Written<TRecord>(Lens<TRecord, Swatch> swatch) where TRecord : notnull =>
        (color, record) => swatch.Set(Swatch.From(color), record);

    private static Well Single<TRecord>(SceneRow<TRecord, Unit> row, Lens<TRecord, Swatch> swatch, Option<Lens<TRecord, bool>> gate, Func<Vector4, TRecord, Fin<TRecord>> write)
        where TRecord : notnull =>
        new(Displayed,
            row.Shown.Map(shown => shown.Map(record => swatch.Get(record).Display)),
            row.Shown.Map(shown => shown.ForAll(record => gate.ForAll(held => held.Get(record)))),
            row.Edited(write));

    private static IO<Unit> Boxed<TRecord>(Option<(Lens<TRecord, bool> Lens, CheckBox Box)> gated, Option<TRecord> value) where TRecord : notnull =>
        IO.lift(() => {
            _ = gated.Iter(pair => {
                pair.Box.ThreeState = value.IsNone;
                pair.Box.Checked = value.Map(pair.Lens.Get).ToNullable();
            });
        });

    private static IO<Unit> Gating<TRecord>(RowSource<TRecord> source, RowEdit<TRecord> edit, Lens<TRecord, bool> gate, CheckBox box, RowScope scope, CallbackSite site, Action<IDisposable> retain)
        where TRecord : notnull =>
        Subscriptions.Attach(h => box.CheckedChanged += h, h => box.CheckedChanged -= h, Callbacks.Handler<EventArgs>(_ => Toggled(source, edit, gate, box, scope), site), retain);

    private static IO<Unit> Toggled<TRecord>(RowSource<TRecord> source, RowEdit<TRecord> edit, Lens<TRecord, bool> gate, CheckBox box, RowScope scope) where TRecord : notnull =>
        from ticked in IO.lift(() => Optional(box.Checked))
        from held in scope.Read(source)
        from committed in ticked.Filter(state => state != gate.Get(held)).Traverse(state => edit.Commit(gate.Set(state, held))).As()
        select unit;

    private static IO<RowCells> Stripped<TRecord, TValue>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, TValue> lens, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>> {
        Func<TRecord, Seq<Swatch>> listed = record => lens.Get(record).ToValue();
        Func<TRecord, Func<Seq<Swatch>, Seq<Swatch>>, Fin<TRecord>> revise = (record, change) => Revised(lens, record, change);
        return
            from pair in DisposalOps.Acquire(IO.lift(() => new AddRemoveButton {
                AddToolTip = RowText.Localize("Add Color", scope.Sink).Local, RemoveToolTip = RowText.Localize("Remove Color", scope.Sink).Local, RemoveEnabled = false,
            }), retain)
            let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatches))
            let caption = RowText.Localize(field.Caption, scope.Sink).Local
            from scened in SceneRow<TRecord, Option<int>>.Bind(
                source, IterableNE.create(field), Option<int>.None,
                (row, record, selected) => new Node.Grid(new Tracks.AutoFill(Sizing.Fit.Content), [
                    listed(record).Map((entry, index) => Face(Displayed, Some(entry.Display), selected == Some(index) ? [Condition.Selected] : [], caption,
                        _ => row.Change(_ => Some(index)).Bind(_ => Dialog(Indexed(row, listed, revise, index), scope, site))) with { Key = new Key.Ordinal((uint)index), }),
                ]) { Width = Grown },
                (_, record, selected) => IO.lift(() => {
                    int count = record.Map(held => listed(held).Count).IfNone(0);
                    pair.AddEnabled = count > 0;
                    pair.RemoveEnabled = selected.Exists(at => at < count);
                }),
                scope, site, retain)
            let row = scened.Row
            from added in Subscriptions.Attach(h => pair.Added += h, h => pair.Added -= h,
                new EventHandler(Callbacks.Handler<EventArgs>(_ => Listed(row, listed, revise, list => Copied(list, row.Held), Last), site).Invoke), retain)
            from removed in Subscriptions.Attach(h => pair.Removed += h, h => pair.Removed -= h,
                new EventHandler(Callbacks.Handler<EventArgs>(_ => row.Held.Match(
                    Some: at => Listed(row, listed, revise, list => list.Take(at).Concat(list.Skip(at + 1)), _ => Some(int.Max(at - 1, 0))),
                    None: static () => IO.pure(unit)), site).Invoke), retain)
            from pick in stage.Traverse(input => PreviewRows.Pick(input, PickCaption, mean => Listed(row, listed, revise, list => list.Add(Swatch.From(mean)), Last), scope, retain)).As()
            from body in Containers.Frame(() => new StackLayout(
                    new StackLayoutItem(pair, HorizontalAlignment.Left), new StackLayoutItem(row.View, HorizontalAlignment.Stretch)) {
                Orientation = Eto.Forms.Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }, retain)
            select new RowCells(
                body, None, pick, None, [], [],
                new RowHelp(None, Some(string.Join(' ', lens.Get(source.Default).ToValue().Map(Conversions.Hex)))),
                scened.Edit.Shown, Some<RowEdit>(scened.Edit),
                Some(ChoiceRows.Context(row.View, Menu(IO.lift(() => row.Held.Map(index => Indexed(row, listed, revise, index)).IfNone(Unpicked)), site), [])));
    }

    private static Fin<TRecord> Revised<TRecord, TValue>(Lens<TRecord, TValue> lens, TRecord record, Func<Seq<Swatch>, Seq<Swatch>> change)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<Swatch>, ValidationError>, IConvertible<Seq<Swatch>> =>
        ValidationResults.Validated<TValue, Seq<Swatch>>(change(lens.Get(record).ToValue())).Map(next => lens.Set(next, record));

    private static IO<Unit> Listed<TRecord>(
        SceneRow<TRecord, Option<int>> row, Func<TRecord, Seq<Swatch>> listed, Func<TRecord, Func<Seq<Swatch>, Seq<Swatch>>, Fin<TRecord>> revise,
        Func<Seq<Swatch>, Seq<Swatch>> change, Func<Seq<Swatch>, Option<int>> picked) where TRecord : notnull =>
        from edited in row.Edited<Unit>((_, record) => revise(record, change))(new Edit<Unit>.Commit(unit))
        from shown in row.Shown
        from chosen in row.Change(_ => shown.Bind(record => picked(listed(record))))
        select unit;

    private static Seq<Swatch> Copied(Seq<Swatch> list, Option<int> selected) =>
        (selected.Bind(at => list.At(at)) | list.Last).Map(list.Add).IfNone(list);

    private static Option<int> Last(Seq<Swatch> list) => Some(list.Count - 1).Filter(static at => at >= 0);

    private static Well Indexed<TRecord>(
        SceneRow<TRecord, Option<int>> row, Func<TRecord, Seq<Swatch>> listed, Func<TRecord, Func<Seq<Swatch>, Seq<Swatch>>, Fin<TRecord>> revise, int index) where TRecord : notnull =>
        row.Shown.Map(shown => shown.Bind(record => listed(record).At(index)).Map(static entry => entry.Display)) switch {
            var stored => new Well(Displayed, stored, stored.Map(static held => held.IsSome),
                row.Edited<Vector4>((color, record) => revise(record, list => list.Map((held, at) => at == index ? Swatch.From(color) : held)))),
        };

    // --- [WELLS]
    private sealed record Well(SwatchEncoding Encoding, IO<Option<Vector4>> Stored, IO<bool> Enabled, Func<Edit<Vector4>, IO<Unit>> Edited);

    private static readonly Well Unpicked = new(Displayed, IO.pure(Option<Vector4>.None), IO.pure(value: false), static _ => IO.pure(unit));

    private static Node Face(SwatchEncoding encoding, Option<Vector4> stored, Seq<Condition> conditions, string caption, Func<SKRect, IO<Unit>> pressed) =>
        Grading.Swatch(encoding.Light(stored.IfNone(Vector4.Zero)), encoding.Gamut, pressed) switch {
            var face => face with {
                Conditions = face.Conditions.Union(conditions + (stored.IsNone ? Seq(Condition.Disabled) : [])),
                Accessible = face.Accessible.Combine(new Accessible {
                    Label = caption,
                    Value = stored.Map(static held => (AccessValue)new AccessValue.Text(Conversions.Hex(Swatch.From(held)))),
                }),
            },
        };

    private static IO<Unit> Dialog(Well well, RowScope scope, CallbackSite site) =>
        (from enabled in well.Enabled
         from opened in when(enabled,
             from document in scope.DocumentIO
             from stored in well.Stored
             from taken in DisposalOps.OnFailure(
                 HostDialogs.ShowColorDialog(
                         document, well.Encoding.Shown(stored.IfNone(Vector4.One)), well.Encoding.Alpha, None,
                         Some<(Func<Color4f, IO<Unit>> Deliver, CallbackSite Site)>((color => well.Edited(new Edit<Vector4>.Preview(well.Encoding.Stored(color))), site)))
                     .Bind(chosen => well.Edited(new Edit<Vector4>.Commit(well.Encoding.Stored(chosen)))),
                 well.Edited(new Edit<Vector4>.Cancel()))
             select unit).As()
         select unit)
        .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));

    private static Seq<Seq<Command<RowRule, Invoke>>> Menu(IO<Well> held, CallbackSite site) =>
        [[
            new Command<RowRule, Invoke>.Run(new CommandFace("color-picker", RowText.Localize(PickerCaption).Local, None, None),
                new RowRule([], [], _ => held.Bind(static well => well.Enabled)),
                new Invoke.Effect(scope => held.Bind(well => Dialog(well, scope, site)))),
            new Command<RowRule, Invoke>.Run(new CommandFace("color-named", "Named Colors", None, None),
                new RowRule([], [], _ => held.Bind(static well => well.Enabled)),
                new Invoke.Listed(_ => held.Bind(static well => well.Stored.Map(stored => Seq(Named(well, stored)))))),
        ]];

    private static Seq<Command<RowRule, Invoke>> Named(Well well, Option<Vector4> stored) =>
        toSeq(NamedColorList.Default).Map<Command<RowRule, Invoke>>(named => CommandRows.Picked(
            new CommandFace(named.Name, named.Name, None, None),
            stored.Exists(held => well.Encoding.Shown(held).AsSystemColor().ToArgb() == named.Color.ToArgb()),
            _ => well.Edited(new Edit<Vector4>.Commit(well.Encoding.Stored(new Color4f(named.Color))))));

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
            Some(ChoiceRows.Context(picker, [], [])));

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
        let named = fun((ToneWheel wheel) => RowText.Localize(wheel.Parameter.Text.Caption, scope.Sink).Local)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Wheels))
        let gamut = working.IfNone(Gamut.StandardRgb)
        from scened in SceneRow<WheelsState, ToneWheel>.Bind(
            source, members, Opening(record),
            (row, held, wheel) => Grading.Wheel(wheel.Lens.Get(held), gamut, row.Edited<WheelOffset>((offset, state) => wheel.Lens.Set(offset, state))) switch {
                var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = named(wheel) }) },
            },
            static (_, _, _) => IO.pure(unit), scope, site, retain)
        let row = scened.Row
        let bound = scened.Edit
        let current = fun(() => row.Held)
        from hue in NumericRows.Line(
            source, WheelOffsetParameter.Hue.Text.Caption, Followed(current, static tone => tone.Hue), WheelHue.Presentation, bound, None, scope.Sink, retain)
        from strength in NumericRows.Line(
            source, WheelOffsetParameter.Strength.Text.Caption, Followed(current, static tone => tone.Strength), AxisFraction.Presentation, bound, None, scope.Sink, retain)
        from luma in NumericRows.Line(
            source, WheelOffsetParameter.Luma.Text.Caption, Followed(current, static tone => tone.Luma), WheelLuma.Presentation, bound, None, scope.Sink, retain)
        from lanes in working.ToSeq().Bind(held => toSeq(TracePlane.Items).Choose(channel => channel.Lane.Map(lane => Laned(channel, lane, WheelOffset.ChannelBounds(held), row, held, scope.Sink, retain))))
            .TraverseM(static make => make).As()
        let lines = Lines(bound, scope.Read(source), Seq(hue.Show, strength.Show, luma.Show) + lanes.Map(static lane => lane.Show))
        from popup in ChoiceRows.Popup(
                toSeq(ToneWheel.Items).Map(item => new ChoiceItem<ToneWheel>(item, named(item), None)),
                picked => row.Change(_ => picked).Bind(_ => lines), scope.Sink, retain)
        select new RowCells(
            popup.Control, None, None, None, [],
            Seq(new RowLine(Some(""), row.View, None, None), hue.Line, strength.Line, luma.Line) + lanes.Map(static lane => lane.Line),
            new RowHelp(None, None), lines.Bind(_ => popup.Show(Some(row.Held))), Some<RowEdit>(bound),
            Some(ChoiceRows.Context(row.View, [], [])));

    private static ToneWheel Opening(WheelsState record) =>
        toSeq(ToneWheel.Items).Find(wheel => wheel.Lens.Get(record) != WheelOffset.Default).IfNone(ToneWheel.Global);

    private static Lens<WheelsState, T> Followed<T>(Func<ToneWheel> current, Func<ToneWheel, Lens<WheelsState, T>> member) =>
        Lens<WheelsState, T>.New(held => member(current()).Get(held), value => held => member(current()).Set(value, held));

    private static IO<(RowLine Line, Func<Option<WheelsState>, IO<Unit>> Show)> Laned(
        TracePlane channel, Lens<Vector3, float> lane, (Vector3 Low, Vector3 High) bounds, SceneRow<WheelsState, ToneWheel> row, Gamut working, IPlugInSink sink, Action<IDisposable> retain) {
        NumericDomain<SignedMagnitude, float> domain = NumericDomain.Bounded<SignedMagnitude, float>(new() { Soft = (lane.Get(bounds.Low), lane.Get(bounds.High)) });
        NumberText<float> text = NumberText.Scalar(domain, RowEdit.Varies);
        return from field in DisposalOps.Acquire(IO.lift(() => new NumberField<SignedMagnitude, float>(text, domain.Create, domain.Key, domain.Bounds, sink)), retain)
               from edited in Subscriptions.Attach(h => field.Edited += h, h => field.Edited -= h,
                   Callbacks.Handler<Rhino.UI.Drawing.EditEventArgs<SignedMagnitude>>(args => row.Taken(args.Edit, (value, held) => Crossed(row.Held, lane, working, value, held)),
                       new CallbackSite(sink, typeof(ColorRows), nameof(Wheels))), retain)
               select (
                   new RowLine(Some(channel.Curve.Parameter.Text.Caption), field, None, Some(field.Fit)),
                   (Func<Option<WheelsState>, IO<Unit>>)(value => field.Receive(Lane(row.Held, lane, working, value))));
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
            .Traverse(static pair =>
                from position in RampPosition.Validated(pair.First.Position)
                from stop in ValidationResults.ToFin(RampStop.Validate(position, pair.Second, out RampStop item), item)
                select stop)
            .As()
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
        SwatchEncoding encoding = new SwatchEncoding.SceneLinear(working);
        return from stop in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
               from position in DisposalOps.Acquire(IO.lift(() => new NumberField<RampPosition, double>(text, domain.Create, domain.Key, domain.Bounds, scope.Sink)), retain)
               let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Gradient))
               let caption = RowText.Localize(field.Caption, scope.Sink).Local
               from scened in SceneRow<TRecord, Selection>.Bind(
                   source, IterableNE.create(field), new Selection.One(None),
                   (row, record, picked) => Grading.Gradient(lens.Get(record), working, picked, Picking(row), row.Edited<Ramp>((ramp, held) => lens.Set(ramp, held))) switch {
                       var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = caption }) },
                   },
                   (row, record, picked) => Stopped(stop, position, Stopping(row, lens, encoding), Selected(lens, record, picked), caption, scope, site),
                   scope, site, retain)
               let row = scened.Row
               from popup in ChoiceRows.Popup(
                       ChoiceRows.Items<RampInterpolation>(None, scope.Sink),
                       picked => row.Edited<RampInterpolation>((interpolation, held) => Interpolated(lens, interpolation, held))(new Edit<RampInterpolation>.Commit(picked)), scope.Sink, retain)
               from line in Containers.Frame(() => new TableLayout(new TableRow(new TableCell(stop, scaleWidth: true), new TableCell(position))), retain)
               from spaced in IO.lift(() => { line.Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Table); })
               from positioned in Subscriptions.Attach(h => position.Edited += h, h => position.Edited -= h,
                   Callbacks.Handler<Rhino.UI.Drawing.EditEventArgs<RampPosition>>(args => Positioned(row, lens, args.Edit), site with { Member = nameof(NumberField<,>.Edited) }), retain)
               select new RowCells(
                   popup.Control, None, None, None, [],
                   Seq(new RowLine(Some(""), row.View, None, None), new RowLine(Some("Stop"), line, None, Some(position.Fit))),
                   new RowHelp(None, None), scened.Edit.Shown.Bind(_ => row.Shown).Bind(shown => popup.Show(shown.Map(record => lens.Get(record).Interpolation))), Some<RowEdit>(scened.Edit),
                   Some(ChoiceRows.Context(row.View, [], [])));
    }

    private static Func<Edit<Selection>, IO<Unit>> Picking<TRecord>(SceneRow<TRecord, Selection> row) where TRecord : notnull =>
        edit => row.Change(held => edit.Carried.IfNone(held));

    private static Option<(int At, RampStop Stop)> Selected<TRecord>(Lens<TRecord, Ramp> lens, Option<TRecord> record, Selection picked) where TRecord : notnull =>
        from held in record
        from at in Index(picked)
        from stop in lens.Get(held).Stops.At(at)
        select (at, stop);

    private static Option<int> Index(Selection picked) => picked.Lead.Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None);

    private static Selection.One Picked(int index) => new(Some<Key>(new Key.Ordinal((uint)index)));

    private static Well Stopping<TRecord>(SceneRow<TRecord, Selection> row, Lens<TRecord, Ramp> lens, SwatchEncoding encoding) where TRecord : notnull =>
        row.Shown.Map(shown => Selected(lens, shown, row.Held).Map(static held => held.Stop.Color)) switch {
            var stored => new Well(encoding, stored, stored.Map(static held => held.IsSome), row.Edited<Vector4>((color, record) => Recolored(lens, Index(row.Held), color, record))),
        };

    private static IO<Unit> Stopped(
        SceneView stop, NumberField<RampPosition, double> position, Well well, Option<(int At, RampStop Stop)> selected, string caption, RowScope scope, CallbackSite site) =>
        from swatch in stop.Show(Face(well.Encoding, selected.Map(static held => held.Stop.Color), [], caption, _ => Dialog(well, scope, site)))
        from enabled in IO.lift(() => { position.Enabled = selected.IsSome; })
        from shown in position.Receive(selected.Map(static held => held.Stop.Position))
        select unit;

    private static Fin<TRecord> Interpolated<TRecord>(Lens<TRecord, Ramp> lens, RampInterpolation interpolation, TRecord record) where TRecord : notnull =>
        ValidationResults.ToFin(Ramp.Validate(lens.Get(record).Stops, interpolation, out Ramp? ramp), ramp).Map(held => lens.Set(held, record));

    private static Fin<TRecord> Recolored<TRecord>(Lens<TRecord, Ramp> lens, Option<int> selected, Vector4 color, TRecord record) where TRecord : notnull =>
        from at in selected.ToFin(new Refused(nameof(Selection.Lead), Refused.Absent))
        let ramp = lens.Get(record)
        from current in Conversions.Within(at, ramp.Stops.Count, nameof(Ramp.Stops)).Map(_ => ramp.Stops[at])
        from item in ValidationResults.ToFin(RampStop.Validate(current.Position, color, out RampStop next), next)
        from written in ValidationResults.ToFin(Ramp.Validate(ramp.Stops.Map((kept, index) => index == at ? item : kept), ramp.Interpolation, out Ramp? replaced), replaced)
        select lens.Set(written, record);

    private static IO<Unit> Positioned<TRecord>(SceneRow<TRecord, Selection> row, Lens<TRecord, Ramp> lens, Rhino.UI.Drawing.Edit<RampPosition> change) where TRecord : notnull =>
        Index(row.Held).Match(
            Some: at =>
                from shown in row.Shown
                let moved = from value in change.Carried from record in shown select lens.Get(record).Moved(at, value).Index
                from taken in row.Taken(change, (value, record) => lens.Set(lens.Get(record).Moved(at, value).Ramp, record))
                from reselected in moved.Traverse(index => row.Change(_ => Picked(index))).As()
                select unit,
            None: static () => IO.pure(unit));

    // --- [QUALIFIERS]
    public static ControlRow Qualifier<TRecord>(
        RowSource<TRecord> source, string caption, string help, RangeAxis axis, Lens<TRecord, HueRange> range, RowField<TRecord> field,
        Option<Func<TRecord, Vector4, Vector4>> grade, Option<StageInput> stage, RowRules rules) where TRecord : notnull =>
        IterableNE.create(field) switch {
            var members => ControlRow.Of(source, members, caption, help, RowShape.Block, (scope, retain) => Qualified(source, members, caption, axis, range, grade, stage, scope, retain), rules),
        };

    private static IO<RowCells> Qualified<TRecord>(
        RowSource<TRecord> source, IterableNE<RowField<TRecord>> members, string caption, RangeAxis axis, Lens<TRecord, HueRange> range,
        Option<Func<TRecord, Vector4, Vector4>> grade, Option<StageInput> stage, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from shown in IO.pure(RowText.Localize(caption, scope.Sink).Local)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Qualifier))
        let lenses = axis.Members(range)
        let band = HueRangeParameter.Band(axis)
        from scened in SceneRow<TRecord, Unit>.Bind(
            source, members, unit,
            (row, record, _) => Grading.Qualified(range.Get(record), axis, grade.Map(held => Graded(range, held, record)), row.Edited<HueRange>((value, held) => range.Set(value, held))) switch {
                var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = shown }) },
            },
            static (_, _, _) => IO.pure(unit), scope, site, retain)
        let bound = scened.Edit
        from pick in stage.Traverse(input => PreviewRows.Pick(
            input, PickCaption, mean => scened.Row.Edited<Vector4>((color, held) => Centered(axis, lenses.Center, color, held))(new Edit<Vector4>.Commit(mean)), scope, retain)).As()
        from center in NumericRows.Line(source, band.Center.Text.Caption, lenses.Center, axis.Presentation, bound, pick, scope.Sink, retain)
        from width in NumericRows.Line(source, band.Width.Text.Caption, lenses.Width, axis.Presentation, bound, None, scope.Sink, retain)
        from softness in NumericRows.Line(source, band.Softness.Text.Caption, lenses.Softness, axis.Presentation, bound, None, scope.Sink, retain)
        select new RowCells(
            scened.Row.View, None, None, None, [], Seq(center.Line, width.Line, softness.Line), new RowHelp(None, None),
            Lines(bound, scope.Read(source), Seq(center.Show, width.Show, softness.Show)), Some<RowEdit>(bound),
            Some(ChoiceRows.Context(scened.Row.View, [], [])));

    private static Func<HueRange, Vector4, Vector4> Graded<TRecord>(Lens<TRecord, HueRange> range, Func<TRecord, Vector4, Vector4> grade, TRecord record) where TRecord : notnull =>
        (shown, color) => grade(range.Set(shown, record), color);

    private static Fin<TRecord> Centered<TRecord>(RangeAxis axis, Lens<TRecord, AxisFraction> center, Vector4 color, TRecord record) where TRecord : notnull =>
        AxisFraction.Validated(axis.Coordinate(Hsy.From(color))).Map(value => center.Set(value, record));
}
