using System.ComponentModel;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.Rhino.UI.Surfaces;
using Rasm.UI;
using Rasm.UI.Components;
using Rasm.UI.Styling;
using Rhino.Display;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;
using SkiaSharp;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ColorRows {
    // --- [SWATCHES]
    public static ControlRow Swatch<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, SKColorF> lens, Option<IO<Option<SKColorF>>> pick, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, parameter, RowShape.Inline, (scope, retain) => Swatched(source, parameter, lens, static _ => true, pick, scope, retain), rules);

    public static ControlRow Swatch<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, Gated<SKColorF>> lens, Option<IO<Option<SKColorF>>> pick, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, parameter, RowShape.Inline, (scope, retain) =>
            from gate in ToggleRows.Boxed(source, parameter, Prelude.lens(lens, Gated<SKColorF>.EnabledEntry.Lens), scope, retain)
            from cells in Swatched(source, parameter, Prelude.lens(lens, Gated<SKColorF>.ValueEntry.Lens), record => lens.Get(record).Enabled, pick, scope, retain)
            select cells with { Gate = Some(gate.Value), Show = gate.Show.Bind(_ => cells.Show) }, rules);

    public static ControlRow Swatches<TRecord, TValue>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, Option<IO<Option<SKColorF>>> pick, RowRules rules)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
        ControlRow.Of(source, parameter, RowShape.Block, (scope, retain) => Listed(source, parameter, lens, pick, scope, retain), rules);

    private static IO<RowCells> Swatched<TRecord>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, SKColorF> lens, Func<TRecord, bool> enabled, Option<IO<Option<SKColorF>>> pick,
        RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from caption in IO.pure(RowText.Localize(parameter.Text.Caption, scope.Sink).Local)
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatch))
        from scened in SceneRow<TRecord, Unit>.Bind(
            source, IterableNE.create(parameter), unit,
            (row, record, _) => Faced(record.Map(lens.Get), record.ForAll(enabled) ? [] : [Condition.Disabled], caption, _ => Dialog(Single(row, lens, enabled), scope, site)),
            static (_, _, _) => IO.pure(unit), scope, site, retain)
        let well = Single(scened.Row, lens, enabled)
        from picked in pick.Traverse(read => Picker(read, well, scope, retain)).As()
        select new RowCells(
            scened.Row.View, None, picked.Map(static button => button.Control), None, [], [], new RowHelp(None, Some(new Ink.Fixed(lens.Get(source.Default)).ToValue())),
            scened.Edit.Shown.Bind(_ => picked.Traverse(static button => button.Refresh).As()).Map(unit), Some<RowEdit>(scened.Edit),
            Some(ChoiceRows.Context(scened.Row.View, Menu(IO.pure(well), site), [])));

    private static Well Single<TRecord>(SceneRow<TRecord, Unit> row, Lens<TRecord, SKColorF> lens, Func<TRecord, bool> enabled) where TRecord : notnull =>
        new(row.Shown.Map(shown => shown.Map(lens.Get)), row.Shown.Map(shown => shown.ForAll(enabled)), row.Edited<SKColorF>((color, record) => lens.Set(color, record)));

    private static IO<RowCells> Listed<TRecord, TValue>(
        RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, TValue> lens, Option<IO<Option<SKColorF>>> pick, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
        DisposalOps.Acquire(owned =>
            from caption in IO.pure(RowText.Localize(parameter.Text.Caption, scope.Sink).Local)
            let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Swatches))
            from scened in SceneRow<TRecord, Selection>.Bind(
                source, IterableNE.create(parameter), new Selection.One(None),
                (row, record, picked) => new SwatchList(record.Map(held => lens.Get(held).ToValue()).IfNone([]), picked, edit => Chosen(row, lens, edit, scope, site)).Drawn switch {
                    var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = caption }) },
                },
                static (_, _, _) => IO.pure(unit), scope, site, owned)
            let row = scened.Row
            let selected = row.Shown.Map(shown => from record in shown from at in Index(row.Held) where at < lens.Get(record).ToValue().Count select at)
            let add = new Command<RowRule, Invoke>.Run(new CommandFace("color-add", "Add Color", None, None), None,
                new Invoke.Effect(_ => Revised(row, lens, colors => (Index(row.Held).Bind(index => colors.At(index)) | colors.Last | lens.Get(source.Default).ToValue().Head).Map(colors.Add).IfNone(colors),
                    static colors => Some(colors.Count - 1))))
            let remove = new Command<RowRule, Invoke>.Run(new CommandFace("color-remove", "Remove Color", None, None),
                Some(new RowRule([], [], _ => selected.Map(static at => at.IsSome))),
                new Invoke.Effect(_ => selected.Bind(at => at.Traverse(index =>
                    Revised(row, lens, colors => colors.Take(index) + colors.Skip(index + 1), colors => Some(int.Min(index, colors.Count - 1)).Filter(static held => held >= 0))).As().Map(unit))))
            from pair in ButtonRows.AddRemove(add, remove, scope, owned)
            from picked in pick.Traverse(read => Picker(read, new Well(IO.pure(Option<SKColorF>.None), IO.pure(value: true),
                edit => edit.Settled.Traverse(color => Revised(row, lens, colors => colors.Add(color), static colors => Some(colors.Count - 1))).As().Map(unit)), scope, owned)).As()
            from body in Containers.Frame(() => new StackLayout(new StackLayoutItem(pair.Control, HorizontalAlignment.Left), new StackLayoutItem(row.View, HorizontalAlignment.Stretch)) {
                Orientation = Eto.Forms.Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
            }, owned)
            select new RowCells(
                body, None, picked.Map(static button => button.Control), None, [], [],
                new RowHelp(None, Some(string.Join(' ', lens.Get(source.Default).ToValue().Map(static color => new Ink.Fixed(color).ToValue())))),
                scened.Edit.Shown.Bind(_ => pair.Refresh).Bind(_ => picked.Traverse(static button => button.Refresh).As()).Map(unit), Some<RowEdit>(scened.Edit),
                Some(ChoiceRows.Context(row.View, Menu(selected.Map(at => at.Map(index => Indexed(row, lens, index)).IfNone(Unpicked)), site), [pair.Refresh]))), retain);

    private static IO<Unit> Chosen<TRecord, TValue>(SceneRow<TRecord, Selection> row, Lens<TRecord, TValue> lens, Edit<Selection> edit, RowScope scope, CallbackSite site)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
        from moved in edit.Carried.Traverse(picked => row.Change(_ => picked)).As()
        from opened in (from picked in edit.Settled from index in Index(picked) select Dialog(Indexed(row, lens, index), scope, site)).Traverse(static open => open).As()
        select unit;

    private static IO<Unit> Revised<TRecord, TValue>(SceneRow<TRecord, Selection> row, Lens<TRecord, TValue> lens, Func<Seq<SKColorF>, Seq<SKColorF>> change, Func<Seq<SKColorF>, Option<int>> picked)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
        from edited in row.Edited<Unit>((_, record) => ValidationResults.Validated<TValue, Seq<SKColorF>>(change(lens.Get(record).ToValue())).Map(next => lens.Set(next, record)))(new Edit<Unit>.Commit(unit))
        from shown in row.Shown
        from chosen in row.Change(held => shown.Map(record => (Selection)picked(lens.Get(record).ToValue()).Map(Picked).IfNone(new Selection.One(None))).IfNone(held))
        select unit;

    private static Well Indexed<TRecord, TValue>(SceneRow<TRecord, Selection> row, Lens<TRecord, TValue> lens, int index)
        where TRecord : notnull
        where TValue : IObjectFactory<TValue, Seq<SKColorF>, ValidationError>, IConvertible<Seq<SKColorF>> =>
        row.Shown.Map(shown => shown.Bind(record => lens.Get(record).ToValue().At(index))) switch {
            var stored => new Well(stored, stored.Map(static held => held.IsSome),
                row.Edited<SKColorF>((color, record) => ValidationResults.Validated<TValue, Seq<SKColorF>>(lens.Get(record).ToValue().Map((held, at) => at == index ? color : held)).Map(next => lens.Set(next, record)))),
        };

    // --- [GRADIENTS]
    public static ControlRow Gradient<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, ColorRamp> lens, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, parameter, RowShape.Block, (scope, retain) => Ramped(source, parameter, lens, scope, retain), rules);

    private static IO<RowCells> Ramped<TRecord>(RowSource<TRecord> source, IStateParameter<TRecord> parameter, Lens<TRecord, ColorRamp> lens, RowScope scope, Action<IDisposable> retain)
        where TRecord : notnull =>
        from text in IO.pure(new ScalarText<Fraction, float>(NumericDomain.Bounded<Fraction, float>(Fraction.Presentation), RowEdit.Varies))
        let caption = RowText.Localize(parameter.Text.Caption, scope.Sink).Local
        let site = new CallbackSite(scope.Sink, typeof(ColorRows), nameof(Gradient))
        from stop in DisposalOps.Acquire(IO.lift(() => new SceneView(scope, new Node.Box())), retain)
        from position in DisposalOps.Acquire(IO.lift(() => new NumberField<Fraction, float>(text, text.Domain.Create, text.Domain.Key, text.Domain.Bounds, scope.Sink)), retain)
        from scened in SceneRow<TRecord, Selection>.Bind(
            source, IterableNE.create(parameter), new Selection.One(None),
            (row, record, picked) => record.Map(lens.Get).Match(
                Some: ramp => new GradientEditor(ramp, row.Edited<ColorRamp>((next, held) => lens.Set(next, held))) {
                    Picked = picked,
                    Pick = Some<Func<Edit<Selection>, IO<Unit>>>(edit => row.Change(held => edit.Carried.IfNone(held))),
                }.Drawn,
                None: () => new GradientKey(lens.Get(source.Default), []).Drawn with { Conditions = [Condition.Mixed] }) switch {
                    var drawn => drawn with { Accessible = drawn.Accessible.Combine(new Accessible { Label = caption }) },
                },
            (row, record, picked) => Stopped(stop, position, Stopping(row, lens), Selected(lens, record, picked), caption, scope, site),
            scope, site, retain)
        let row = scened.Row
        from popup in ChoiceRows.Popup(
            Seq(typeof(ColorRamp.Smooth), typeof(ColorRamp.Stepped)).Map(kind => new ChoiceItem<Type>(kind, RowText.Localize(Captions.Of(kind), scope.Sink).Local, None)),
            kind => row.Edited<Type>((picked, record) => lens.Set(Cast(lens.Get(record), picked), record))(new Edit<Type>.Commit(kind)), scope.Sink, retain)
        from line in RowGrid.Lined([new TableCell(stop, scaleWidth: true), new TableCell(position)], retain)
        from positioned in Subscriptions.Attach<EventHandler<Edit<Fraction>>>(h => position.Edited += h, h => position.Edited -= h,
            Callbacks.Handler<Edit<Fraction>>(edit => Positioned(row, lens, edit), site with { Member = nameof(NumberField<,>.Edited) }), retain)
        select new RowCells(
            popup.Control, None, None, None, [],
            Seq(new RowLine(None, row.View, None, None), new RowLine(Some(Captions.Of(nameof(Stop<>))), line, None, Some(position.Fit))),
            new RowHelp(None, Some(lens.Get(source.Default).ToValue())),
            scened.Edit.Shown.Bind(_ => row.Shown).Bind(shown => popup.Show(shown.Map(record => lens.Get(record).GetType()))), Some<RowEdit>(scened.Edit),
            Some(ChoiceRows.Context(row.View, Menu(IO.pure(Stopping(row, lens)), site), [])));

    private static ColorRamp Cast(ColorRamp ramp, Type kind) =>
        ramp.GetType() == kind ? ramp : ramp.Switch(smooth: static smooth => (ColorRamp)new ColorRamp.Stepped(smooth.Stops), stepped: static stepped => new ColorRamp.Smooth(stepped.Stops));

    private static Option<(int At, Stop<SKColorF> Stop)> Selected<TRecord>(Lens<TRecord, ColorRamp> lens, Option<TRecord> record, Selection picked) where TRecord : notnull =>
        from held in record
        from at in Index(picked)
        from stop in lens.Get(held).Stops.At(at)
        select (at, stop);

    private static Well Stopping<TRecord>(SceneRow<TRecord, Selection> row, Lens<TRecord, ColorRamp> lens) where TRecord : notnull =>
        row.Shown.Map(shown => Selected(lens, shown, row.Held).Map(static held => held.Stop.Color)) switch {
            var stored => new Well(stored, stored.Map(static held => held.IsSome), row.Edited<SKColorF>((color, record) =>
                (from at in Index(row.Held)
                 let ramp = lens.Get(record)
                 from stop in ramp.Stops.At(at)
                 select lens.Set(ramp.WithStops(ramp.Stops.Map((kept, index) => index == at ? stop with { Color = color } : kept)), record)).IfNone(record))),
        };

    private static IO<Unit> Stopped(SceneView stop, NumberField<Fraction, float> position, Well well, Option<(int At, Stop<SKColorF> Stop)> selected, string caption, RowScope scope, CallbackSite site) =>
        from swatch in stop.Show(Faced(selected.Map(static held => held.Stop.Color), [], caption, _ => Dialog(well, scope, site)))
        from enabled in IO.lift(() => { position.Enabled = selected.IsSome; })
        from shown in position.Receive(selected.Map(static held => held.Stop.At))
        select unit;

    private static IO<Unit> Positioned<TRecord>(SceneRow<TRecord, Selection> row, Lens<TRecord, ColorRamp> lens, Edit<Fraction> edit) where TRecord : notnull =>
        Index(row.Held).Traverse(at =>
            from taken in row.Edited<Fraction>((value, record) => lens.Set(Moved(lens.Get(record), at, value).Ramp, record))(edit)
            from shown in row.Shown
            from reselected in (from value in edit.Carried from record in shown select Moved(lens.Get(record), at, value).At).Traverse(index => row.Change(_ => Picked(index))).As()
            select unit).As().Map(unit);

    private static (ColorRamp Ramp, int At) Moved(ColorRamp ramp, int at, Fraction value) =>
        ramp.Stops.Map((stop, index) => (Stop: index == at ? stop with { At = value } : stop, Moved: index == at)).Strict() switch {
            var moved => toSeq(moved.OrderBy(static held => (float)held.Stop.At)) switch {
                var ordered => (ramp.WithStops(ordered.Map(static held => held.Stop)), ordered.Map(static (held, index) => (held.Moved, index)).Find(static held => held.Moved).Map(static held => held.index).IfNone(at)),
            },
        };

    // --- [WELLS]
    private sealed record Well(IO<Option<SKColorF>> Stored, IO<bool> Enabled, Func<Edit<SKColorF>, IO<Unit>> Edited);

    private static readonly Well Unpicked = new(IO.pure(Option<SKColorF>.None), IO.pure(value: false), static _ => IO.pure(unit));

    private static Option<int> Index(Selection picked) => picked.Lead.Bind(static key => key is Key.Ordinal ordinal ? Some((int)ordinal.Index) : None);

    private static Selection.One Picked(int index) => new(Some<Key>(new Key.Ordinal((uint)index)));

    private static Node Faced(Option<SKColorF> stored, LanguageExt.HashSet<Condition> conditions, string caption, Func<SKRect, IO<Unit>> pressed) =>
        new Swatch(stored.IfNone(SKColorF.Empty), pressed).Drawn switch {
            var face => face with {
                Conditions = face.Conditions.Union(stored.IsNone ? conditions.TryAdd(Condition.Mixed) : conditions),
                Accessible = face.Accessible.Combine(new Accessible { Label = caption }),
            },
        };

    private static IO<(Control Control, IO<Unit> Refresh)> Picker(IO<Option<SKColorF>> read, Well well, RowScope scope, Action<IDisposable> retain) =>
        CommandRows.Button(
                new Command<RowRule, Invoke>.Run(
                    new CommandFace("color-pick", "Pick Color", None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.SVGEditorEyeDropperSvg))),
                    Some(new RowRule([], [], _ => well.Enabled)),
                    new Invoke.Effect(_ => read.Bind(picked => picked.Traverse(color => well.Edited(new Edit<SKColorF>.Commit(color))).As()).Map(unit))),
                IconSlot.Small, scope, retain);

    private static IO<Unit> Dialog(Well well, RowScope scope, CallbackSite site) =>
        (from enabled in well.Enabled
         from opened in when(enabled,
             from document in scope.DocumentIO
             from stored in well.Stored
             from taken in DisposalOps.OnFailure(
                 HostDialogs.ShowColorDialog(
                         document, Conversions.ToColor4f(stored.IfNone(SKColors.White)), allowAlpha: true, None,
                         Some<(Func<Color4f, IO<Unit>> Deliver, CallbackSite Site)>((color => well.Edited(new Edit<SKColorF>.Preview(Conversions.ToSKColorF(color))), site)))
                     .Bind(chosen => well.Edited(new Edit<SKColorF>.Commit(Conversions.ToSKColorF(chosen)))),
                 well.Edited(new Edit<SKColorF>.Cancel()))
             select unit).As()
         select unit)
        .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure(unit));

    private static Seq<Seq<Command<RowRule, Invoke>>> Menu(IO<Well> held, CallbackSite site) =>
        [Seq<Command<RowRule, Invoke>>(
            new Command<RowRule, Invoke>.Run(new CommandFace("color-picker", "Color Picker…", None, None),
                Some(new RowRule([], [], _ => held.Bind(static well => well.Enabled))),
                new Invoke.Effect(scope => held.Bind(well => Dialog(well, scope, site)))),
            new Command<RowRule, Invoke>.Run(new CommandFace("color-named", "Named Colors", None, None),
                Some(new RowRule([], [], _ => held.Bind(static well => well.Enabled))),
                new Invoke.Listed(_ => held.Bind(static well => well.Stored.Map(stored => Seq(Named(well, stored))))))
        )];

    private static Seq<Command<RowRule, Invoke>> Named(Well well, Option<SKColorF> stored) =>
        toSeq(NamedColorList.Default).Map<Command<RowRule, Invoke>>(named => CommandRows.Picked(
            new CommandFace(named.Name, named.Name, None, None),
            stored.Exists(held => Conversions.Same(Conversions.ToColor(held), named.Color)),
            _ => well.Edited(new Edit<SKColorF>.Commit(Conversions.ToSKColorF(named.Color)))));

    // --- [PAIRS]
    public static ControlRow DisplayPrint<TRecord>(
        RowSource<TRecord> source, IterableNE<IStateParameter<TRecord>> parameters, ParameterText text,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> display,
        Lens<TRecord, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color)> print,
        LanguageExt.HashSet<DisplayAndPrintColorSourceMode> offered, Option<Func<TRecord, IO<(System.Drawing.Color Display, System.Drawing.Color Print)>>> resolved, RowRules rules)
        where TRecord : notnull =>
        ControlRow.Of(source, parameters, text, RowShape.Inline, (scope, retain) =>
            from picker in DisposalOps.Acquire(IO.lift(static () => new DisplayAndPrintColorPicker()), retain)
            from configured in IO.lift(() => {
                (picker.PickerMode, picker.ShowByLayer, picker.ShowByObject, picker.ShowByParent, picker.ShowViewport, picker.ShowNone) = (
                    DisplayAndPrintColorPickerMode.Dual,
                    offered.Contains(DisplayAndPrintColorSourceMode.ByLayer), offered.Contains(DisplayAndPrintColorSourceMode.ByObject), offered.Contains(DisplayAndPrintColorSourceMode.ByParent),
                    offered.Contains(DisplayAndPrintColorSourceMode.Viewport), offered.Contains(DisplayAndPrintColorSourceMode.None));
            })
            let site = new CallbackSite(scope.Sink, typeof(DisplayAndPrintColorPicker), nameof(DisplayAndPrintColorPicker.PropertyChanged))
            from bound in RowEdit.Bind<TRecord, PropertyChangedEventHandler>(source, parameters, h => picker.PropertyChanged += h, h => picker.PropertyChanged -= h,
                edit => Callbacks.Handler<PropertyChangedEventArgs>(_ => Changed(picker, display, print, edit, source, scope), site).Invoke,
                value => value.Match(Some: record => Matched(picker, display.Get(record), print.Get(record), resolved.Map(read => read(record))), None: () => Mixed(picker)),
                scope, site, retain)
            select new RowCells(picker, None, None, None, [], [], new RowHelp(None, None), bound.Shown, Some<RowEdit>(bound), Some(ChoiceRows.Context(picker, [], []))), rules);

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
            (picker.DisplayVaries, picker.PrintVaries) = (false, false);
            (picker.DisplaySourceMode, picker.DisplayColor, picker.PrintSourceMode, picker.PrintColor) = (display.Source, display.Color.ToEto(), print.Source, print.Color.ToEto());
            picker.LinkPrintToDisplay = Same(display, print);
        })
        from colors in resolved.Traverse(static read => read).As()
        from shown in IO.lift(() => colors.Iter(held => (picker.DisplayResolvedColor, picker.PrintResolvedColor) = (held.Display.ToEto(), held.Print.ToEto())))
        select unit;

    private static IO<Unit> Mixed(DisplayAndPrintColorPicker picker) =>
        IO.lift(() => { (picker.DisplayVaries, picker.PrintVaries) = (true, true); });

    private static bool Same((DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) left, (DisplayAndPrintColorSourceMode Source, System.Drawing.Color Color) right) =>
        left.Source == right.Source && Conversions.Same(left.Color, right.Color);
}
