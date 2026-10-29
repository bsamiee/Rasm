using System.Collections.Generic;
using System.ComponentModel;
using Eto.Drawing;
using Eto.Forms;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.UI.Numeric;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.UI;
using Rhino.UI.Controls;
using UnitsNet;

namespace Rasm.Rhino.UI.Inputs;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ChoiceItem<T>(T Value, string Caption) where T : notnull {
    public Option<IconResource> Icon { get; init; }

    public override string ToString() => LocalText.Of(Caption).Local;
}

public sealed record ValueRow<TControl, T>(Func<TControl> Make, Func<TControl, RowScope, IO<BindableBinding<TControl, Option<T>>>> Binding, IO<Option<T>> Current, Func<Edit<T>, IO<Unit>> Edited)
    : IRow<ValueRow<TControl, T>>
    where TControl : Control
    where T : notnull {
    public Func<TControl, bool> Previewing { get; init; } = static _ => false;
    public Option<Func<TControl, HostEvent<Unit>>> Settled { get; init; }
    public Func<TControl, Option<T>, RowScope, IO<Unit>> Shaped { get; init; } = static (_, _, _) => IO.pure(unit);
    public Func<TControl, IO<Unit>> Attached { get; init; } = static _ => IO.pure(unit);
    public Seq<HostEvent<Unit>> Changes { get; init; }
    public Func<IO<ValueRow<TControl, T>>, Seq<Seq<Command<Invoke>>>> Menu { get; init; } = static _ => [];

    public RowShape Shape { get; init; } = RowShape.Inline;

    public static IO<RowCells> Realize(IO<ValueRow<TControl, T>> latest, LocalizeStringPair label, RowScope scope) =>
        from first in latest
        from control in use(first.Make)
        from attached in first.Attached(control)
        from current in first.Current
        from shaped in first.Shaped(control, current, scope)
        from binding in first.Binding(control, scope)
        from show in Values.Bound(control, binding, first.Previewing, first.Settled.Map(settled => settled(control)), current, edit => latest.Bind(row => row.Edited(edit)))
        let shown =
            from row in latest
            from value in row.Current
            from reshaped in row.Shaped(control, value, scope)
            from written in show(value)
            select unit
        from changes in first.Changes.TraverseM(change => change.Attach(_ => shown.OnUiThread())).As()
        select new RowCells(control, shown) { Menu = first.Menu(latest) };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Values {
    // --- [BINDING]
    internal static IO<Func<Option<T>, IO<Unit>>> Bound<TControl, T>(
        TControl control, BindableBinding<TControl, Option<T>> binding, Func<TControl, bool> previewing, Option<HostEvent<Unit>> settled, Option<T> initial, Func<Edit<T>, IO<Unit>> edited)
        where TControl : Control
        where T : notnull =>
        from state in IO.lift(() => Ref((Shown: initial, Previewed: Option<T>.None)))
        let entered = fun((T value) =>
            from phase in IO.lift(() => previewing(control) ? EditPhase.Preview : EditPhase.Commit)
            from fresh in atomicIO(() => {
                (Option<T> Shown, Option<T> Previewed) held = state.Value;
                state.Value = phase.Applies ? (Shown: Some(value), Previewed: None) : held with { Previewed = Some(value) };
                return phase.Applies ? held.Previewed.IsSome || held.Shown != Some(value) : (held.Previewed | held.Shown) != Some(value);
            })
            from sent in when(fresh, edited(phase.Of(value))).As()
            select unit)
        from dual in use(() => binding.Bind(Binding.Delegate(() => state.Value.Shown, held => Session.Run(held.Traverse(entered).As()))), static dual => dual.Unbind())
        from settles in settled.Traverse(settle => settle.Attach(_ =>
            from value in IO.lift(() => dual.Destination.DataValue)
            from before in atomicIO(() => {
                (Option<T> Shown, Option<T> Previewed) held = state.Value;
                state.Value = (Shown: held.Previewed.IsSome ? value : held.Shown, Previewed: None);
                return held;
            })
            from sent in when(before.Previewed.IsSome, value == before.Shown
                ? edited(new Edit<T>.Cancel())
                : value.Traverse(held => edited(EditPhase.Commit.Of(held))).As().Map(unit)).As()
            select unit)).As()
        select (Func<Option<T>, IO<Unit>>)(value => atomicIO(() => state.Value = (Shown: value, Previewed: Option<T>.None)).Bind(_ => IO.lift(() => dual.Update())));

    internal static BindableBinding<NumericUpDownWithUnitParsing, Option<double>> Numeric(NumericUpDownWithUnitParsing field) =>
        new(field,
            static held => Some(held.Value),
            static (held, value) => value.Match(
                Some: number => (held.AlternateText, held.Value) = ("", number),
                None: () => held.AlternateText = LocalText.Of("Varies").Local),
            static (held, handler) => held.ValueChanged += handler,
            static (held, handler) => held.ValueChanged -= handler);

    internal static bool Stepping(NumericUpDownWithUnitParsing field) => field.IsStepping || field.IsTextChanging;

    internal static IO<Unit> Digits(NumericUpDownWithUnitParsing field) =>
        HostTheme.Applied(field, HostPlatform.Current.Digits(field));

    internal static TField Fielded<TField>(TField field) where TField : NumericUpDownWithUnitParsing {
        (field.ValueUpdateMode, field.HideAlternateTextWhenValueChanges) = (NumericUpDownWithUnitParsing.UpdateModeWhenInPanel | NumericUpDownWithUnitParsingUpdateMode.OnValueChange, true);
        return field;
    }

    // --- [TOGGLES]
    public static ValueRow<CheckBox, bool> Check(Option<bool> value, Func<Edit<bool>, IO<Unit>> edited) =>
        new(static () => new CheckBox(), static (box, _) => IO.pure(box.CheckedBinding.Convert(static held => Optional(held), static held => held.ToNullable())), IO.pure(value), edited) {
            Shaped = static (box, current, _) => IO.lift(() => { box.ThreeState = current.IsNone; }),
        };

    // --- [CHOICES]
    public static ValueRow<DropDown, T> Choice<T>(Option<T> value, Seq<ChoiceItem<T>> items, Func<Edit<T>, IO<Unit>> edited) where T : notnull =>
        Listed(value, _ => IO.pure(items), edited);

    public static ValueRow<SegmentedButton, Seq<T>> Flags<T>(Option<Seq<T>> value, Seq<ChoiceItem<T>> items, Func<Edit<Seq<T>>, IO<Unit>> edited) where T : notnull =>
        new(static () => new SegmentedButton { SelectionMode = SegmentedSelectionMode.Multiple },
            (strip, _) => IO.pure(new BindableBinding<SegmentedButton, Option<Seq<T>>>(strip,
                held => Some(toSeq(held.Items).Zip(items).Filter(static pair => pair.First.Selected).Map(static pair => pair.Second.Value).Strict()),
                (held, wanted) => toSeq(held.Items).Zip(items).Iter(pair => pair.First.Selected = wanted.Exists(chosen => chosen.Exists(value => EqualityComparer<T>.Default.Equals(value, pair.Second.Value)))),
                static (held, handler) => held.SelectedItemsChanged += handler,
                static (held, handler) => held.SelectedItemsChanged -= handler)),
            IO.pure(value), edited) {
            Attached = strip => items.TraverseM(item =>
                from segment in use(() => new ButtonSegmentedItem { Text = item.Icon.Map(static _ => "").IfNone(LocalText.Of(item.Caption).Local), ToolTip = LocalText.Of(item.Caption).Local })
                from placed in IO.lift(() => strip.Items.Add(segment))
                from themed in item.Icon.Traverse(icon => Icons.Themed(icon, IconSlot.PanelButton, strip, frames => segment.Image = frames)).As()
                select unit).As().Map(unit),
        };

    public static ValueRow<DropDown, Guid> Component<TComponent>(Option<Guid> value, Func<RhinoDoc, IEnumerable<TComponent>> table, Func<Edit<Guid>, IO<Unit>> edited)
        where TComponent : ModelComponent =>
        Listed(value, scope => scope.DocumentIO.Map(doc => toSeq(toSeq(table(doc))
            .Filter(static component => !component.IsDeleted && !component.IsSystemComponent)
            .Map(static component => new ChoiceItem<Guid>(component.Id, component.Name))
            .OrderBy(static item => item.Caption, Comparer<string>.Create(Localization.LogicalSort)))), edited) with {
            Changes = [HostTheme.Changed, Tables],
        };

    public static ValueRow<EnumDropDown<T?>, T> Enumerated<T>(Option<T> value, Func<Edit<T>, IO<Unit>> edited) where T : struct, Enum =>
        new(static () => new EnumDropDown<T?> { GetText = static held => LocalText.Of(held.ToString() ?? "").Local },
            static (drop, _) => IO.pure(drop.SelectedValueBinding.Convert(static held => Optional(held), static held => held.ToNullable())), IO.pure(value), edited);

    private static ValueRow<DropDown, T> Listed<T>(Option<T> value, Func<RowScope, IO<Seq<ChoiceItem<T>>>> items, Func<Edit<T>, IO<Unit>> edited) where T : notnull =>
        new(static () => new DropDown { ItemTextBinding = Binding.Delegate<ChoiceItem<T>, string>(static item => item.Caption) },
            static (drop, _) => IO.pure(new BindableBinding<DropDown, Option<T>>(drop,
                static held => Optional(held.SelectedValue as ChoiceItem<T>).Map(static item => item.Value),
                static (held, wanted) => held.SelectedValue = wanted.Bind(chosen => Stocked<T>(held).Find(item => EqualityComparer<T>.Default.Equals(item.Value, chosen))).ValueUnsafe(),
                static (held, handler) => held.SelectedValueChanged += handler,
                static (held, handler) => held.SelectedValueChanged -= handler)),
            IO.pure(value), edited) {
            Shaped = (drop, _, scope) =>
                from listed in items(scope)
                let localized = listed.Map(static item => item with { Caption = LocalText.Of(item.Caption).Local })
                from icons in localized.TraverseM(static item => item.Icon.Traverse(static icon => Icons.Frames(icon, IconSlot.DropDownItem)).As().Map(frames => (Item: item, Frames: frames))).As()
                let images = toHashMap(icons.Choose(static held => held.Frames.Map(frames => (held.Item, frames))))
                from stocked in unless(Stocked<T>(drop) == localized, IO.lift(() => {
                    drop.ItemImageBinding = Binding.Delegate<ChoiceItem<T>, Image?>(item => images.Find(item).ValueUnsafe());
                    drop.DataStore = [.. localized];
                })).As()
                select unit,
            Changes = [HostTheme.Changed],
        };

    private static Seq<ChoiceItem<T>> Stocked<T>(DropDown drop) where T : notnull => toSeq(drop.DataStore ?? []).Choose(static item => Optional(item as ChoiceItem<T>));

    private static readonly HostEvent<Unit> Tables =
        Subscriptions.Merged(
            Subscriptions.On<LayerTableEventArgs>(static h => RhinoDoc.LayerTableEvent += h, static h => RhinoDoc.LayerTableEvent -= h).Choose(static _ => Some(unit)),
            Subscriptions.On<LinetypeTableEventArgs>(static h => RhinoDoc.LinetypeTableEvent += h, static h => RhinoDoc.LinetypeTableEvent -= h).Choose(static _ => Some(unit)),
            Subscriptions.On<RhinoDoc.RenderContentTableEventArgs>(static h => RhinoDoc.RenderMaterialsTableEvent += h, static h => RhinoDoc.RenderMaterialsTableEvent -= h).Choose(static _ => Some(unit)),
            Subscriptions.On<DimStyleTableEventArgs>(static h => RhinoDoc.DimensionStyleTableEvent += h, static h => RhinoDoc.DimensionStyleTableEvent -= h).Choose(static _ => Some(unit)),
            Subscriptions.On<GroupTableEventArgs>(static h => RhinoDoc.GroupTableEvent += h, static h => RhinoDoc.GroupTableEvent -= h).Choose(static _ => Some(unit)));

    // --- [MOMENTS]
    public static ValueRow<DateTimePicker, LocalDateTime> Moment(Option<LocalDateTime> value, DateTimePickerMode part, DateInterval limits, Func<Edit<LocalDateTime>, IO<Unit>> edited) =>
        new(() => new DateTimePicker { Mode = part, MinDate = limits.Start.AtMidnight().ToDateTimeUnspecified(), MaxDate = limits.End.PlusDays(1).AtMidnight().ToDateTimeUnspecified() },
            static (picker, _) => IO.pure(picker.ValueBinding.Convert(static held => Conversions.Present(held), static held => held.Map(static wall => wall.ToDateTimeUnspecified()).ToNullable())),
            IO.pure(value), edited);

    // --- [COLORS]
    public static ValueRow<ColorPicker, Color4f> Swatch(Option<Color4f> value, Func<Edit<Color4f>, IO<Unit>> edited) =>
        new(static () => new ColorPicker { AllowAlpha = true },
            static (picker, _) => IO.pure(Colored(picker)),
            IO.pure(value), edited) {
            Previewing = static _ => true,
            Settled = Some(Closed),
            Menu = static latest => [[Named(latest.Bind(static row => row.Current), edit => latest.Bind(row => row.Edited(edit)))]],
        };

    internal static BindableBinding<ColorPicker, Option<Color4f>> Colored(ColorPicker picker) =>
        picker.ValueBinding.Convert(static held => Some(new Color4f(held.ToSystemDrawing())), held => held.Map(static color => color.AsSystemColor().ToEto()).IfNone(picker.Value));

    internal static HostEvent<Unit> Closed(ColorPicker picker) =>
        Subscriptions.Inline<MouseEventArgs>(h => picker.MouseUp += h, h => picker.MouseUp -= h).Choose(static _ => Some(unit));

    public static ValueRow<DisplayAndPrintColorPicker, ColorPair> DisplayAndPrint(
        Option<ColorPair> value, LanguageExt.HashSet<DisplayAndPrintColorSourceMode> offered, Option<ColorPair> resolved, Func<Edit<ColorPair>, IO<Unit>> edited) =>
        new(static () => new DisplayAndPrintColorPicker { PickerMode = DisplayAndPrintColorPickerMode.Dual }, static (picker, _) => IO.lift(() => Paired(picker)), IO.pure(value), edited) {
            Shaped = (picker, _, _) => IO.lift(() => {
                (picker.ShowByLayer, picker.ShowByObject, picker.ShowByParent, picker.ShowViewport, picker.ShowNone) = (
                    offered.Contains(DisplayAndPrintColorSourceMode.ByLayer), offered.Contains(DisplayAndPrintColorSourceMode.ByObject),
                    offered.Contains(DisplayAndPrintColorSourceMode.ByParent), offered.Contains(DisplayAndPrintColorSourceMode.Viewport),
                    offered.Contains(DisplayAndPrintColorSourceMode.None));
                _ = resolved.Iter(held => (picker.DisplayResolvedColor, picker.PrintResolvedColor) = (held.Display.Color.ToEto(), held.Print.Color.ToEto()));
            }),
        };

    private static BindableBinding<DisplayAndPrintColorPicker, Option<ColorPair>> Paired(DisplayAndPrintColorPicker picker) =>
        Ref(Option<PropertyChangedEventHandler>.None) switch {
            var adapter => new(picker,
                static held => Conversions.Present(!held.DisplayVaries && !held.PrintVaries,
                    new ColorPair(new ColorSource(held.DisplaySourceMode, held.DisplayColor.ToSystemDrawing()), new ColorSource(held.PrintSourceMode, held.PrintColor.ToSystemDrawing()))),
                static (held, value) => value.Match(
                    Some: pair => (held.DisplaySourceMode, held.DisplayColor, held.PrintSourceMode, held.PrintColor, held.LinkPrintToDisplay) =
                        (pair.Display.Source, pair.Display.Color.ToEto(), pair.Print.Source, pair.Print.Color.ToEto(), pair.Display == pair.Print),
                    None: () => (held.DisplayVaries, held.PrintVaries) = (true, true)),
                (held, handler) => held.PropertyChanged += atomic(() => (adapter.Value = Some<PropertyChangedEventHandler>((sender, _) => handler(sender, EventArgs.Empty))).ValueUnsafe()),
                (held, _) => atomic(() => adapter.Value).Iter(handler => held.PropertyChanged -= handler)),
        };

    internal static Command<Invoke> Named(IO<Option<Color4f>> current, Func<Edit<Color4f>, IO<Unit>> edited) =>
        new Command<Invoke>.Run(new CommandFace("color-named", "Named Colors…"), Some(new RowRule(_ => current.Map(static held => held.IsSome))), new Invoke.Effect(scope =>
            from shown in current.Bind(static held => IO.lift(() => Conversions.Required(held)))
            from document in scope.DocumentIO
            from edit in HostDialogs.ShowColorDialog(document, shown, allowAlpha: true, Some(NamedColorList.Default), Some<Func<Color4f, IO<Unit>>>(color => edited(EditPhase.Preview.Of(color))))
                .Map(static chosen => EditPhase.Commit.Of(chosen))
                .Catch(static error => error.Is(Errors.Cancelled), static _ => IO.pure<Edit<Color4f>>(new Edit<Color4f>.Cancel()))
            from sent in edited(edit)
            select unit));

    // --- [NUMBERS]
    public static ValueRow<NumericUpDownWithUnitParsing, double> Number(Option<double> value, NumberRange range, Func<Edit<double>, IO<Unit>> edited) =>
        new(() => Fielded(new ScaledField(range)), static (field, _) => IO.pure(Numeric(field)), IO.pure(value), edited) {
            Previewing = Stepping,
            Shaped = (field, _, _) => IO.lift(() => range.Applied(field)),
            Attached = Digits,
        };

    public static ValueRow<global::Rhino.UI.Controls.Slider, double> Slider(Option<double> value, double low, double high, Option<int> decimals, Func<Edit<double>, IO<Unit>> edited) =>
        new(static () => new global::Rhino.UI.Controls.Slider(new Panel(), usePercentageSign: false),
            static (slider, _) => IO.pure(new BindableBinding<global::Rhino.UI.Controls.Slider, double?>(slider, Binding.Property(static (global::Rhino.UI.Controls.Slider held) => held.Value1))
                .Convert(static held => Optional(held), static held => held.ToNullable())),
            IO.pure(value), edited) {
            Previewing = static _ => true,
            Settled = Some<Func<global::Rhino.UI.Controls.Slider, HostEvent<Unit>>>(static slider =>
                Subscriptions.Inline<PropertyChangedEventHandler, PropertyChangedEventArgs>(h => slider.PropertyChanged += h, h => slider.PropertyChanged -= h, static handler => handler.Invoke)
                    .Choose(static args => Conversions.Present(args.PropertyName is "OnEndChanged", unit))),
            Attached = slider => IO.lift(() => {
                slider.SetMinMax(low, high);
                _ = decimals.Iter(places => slider.Decimals = places);
            }),
        };

    public static ValueRow<NumericUpDownWithUnitParsing, Length> Distance(Option<Length> value, Length low, Length high, bool modelUnits, Func<Edit<Length>, IO<Unit>> edited) =>
        new(static () => Fielded(new NumericUpDownWithUnitParsing { UseDistanceDisplayMode = true }),
            (field, scope) =>
                from doc in scope.DocumentIO
                from measured in NumberFields.Measured(field, doc, modelUnits)
                select Numeric(field).Convert(
                    held => held.Map(number => Quantities.From(number, modelUnits ? doc.ModelUnits : doc.PageUnits)),
                    held => held.Map(length => Quantities.As(length, modelUnits ? doc.ModelUnits : doc.PageUnits))),
            IO.pure(value), edited) {
            Previewing = Stepping,
            Shaped = (field, _, scope) => scope.DocumentIO.Bind(doc => IO.lift(() => {
                (field.MinValue, field.MaxValue) = (Quantities.As(low, modelUnits ? doc.ModelUnits : doc.PageUnits), Quantities.As(high, modelUnits ? doc.ModelUnits : doc.PageUnits));
            })),
            Attached = Digits,
        };

    public static ValueRow<NumericUpDownWithUnitParsing, Angle> Turn(Option<Angle> value, Func<Edit<Angle>, IO<Unit>> edited) =>
        new(static () => Fielded(new NumericUpDownWithUnitParsing { Suffix = "°", MaximumDecimalPlaces = 3 }),
            static (field, _) => IO.pure(Numeric(field).Convert(
                static held => held.Map(static degrees => Angle.FromDegrees(Quantities.Exact(degrees))),
                static held => held.Map(static angle => angle.Degrees.ToDouble()))),
            IO.pure(value), edited) {
            Previewing = Stepping,
            Attached = Digits,
        };

    // --- [TEXT]
    public static ValueRow<TextBox, string> Line(Option<string> value, Func<Edit<string>, IO<Unit>> edited) =>
        new(static () => new TextBox(), static (box, _) => IO.pure(new BindableBinding<TextBox, Option<string>>(box,
                static held => Optional(held.Text),
                static (held, text) => held.Text = text.IfNone(""),
                static (held, handler) => held.TextChanged += handler,
                static (held, handler) => held.TextChanged -= handler)),
            IO.pure(value), edited) {
            Previewing = static box => box.HasFocus,
            Settled = Some<Func<TextBox, HostEvent<Unit>>>(static box => Subscriptions.Merged(
                Subscriptions.Inline<EventArgs>(h => box.LostFocus += h, h => box.LostFocus -= h).Choose(static _ => Some(unit)),
                Subscriptions.Inline<KeyEventArgs>(h => box.KeyDown += h, h => box.KeyDown -= h).Choose(static args => Conversions.Present(args.Key is Keys.Enter, unit)))),
            Shaped = static (box, current, _) => IO.lift(() => { box.PlaceholderText = current.IsNone ? LocalText.Of("Varies").Local : ""; }),
        };
}
