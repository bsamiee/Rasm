using System.Collections.Frozen;
using System.Globalization;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;
using GhSelectionMode = Grasshopper2.Extensions.SelectionMode;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SelectionMode {
    public GhSelectionMode Gh { get; }
    public static readonly SelectionMode Promote = new(key: 0, gh: GhSelectionMode.Promote);
    public static readonly SelectionMode Include = new(key: 1, gh: GhSelectionMode.Include);
    public static readonly SelectionMode Exclude = new(key: 2, gh: GhSelectionMode.Exclude);
    public static readonly SelectionMode Inverse = new(key: 3, gh: GhSelectionMode.Inverse);
    private static readonly Lazy<FrozenDictionary<GhSelectionMode, SelectionMode>> ByGh =
        new(static () => Items.ToFrozenDictionary(static m => m.Gh));
    internal static SelectionMode FromGh(GhSelectionMode gh) => ByGh.Value.GetValueOrDefault(key: gh) ?? Inverse;
}
[SmartEnum<int>]
[ValidationError<UiFault>]
public sealed partial class CursorKind {
    private delegate Cursor CursorSource(Grasshopper2.UI.Canvas.Canvas canvas);

    public static readonly CursorKind Default = new(key: 0, cursor: static _ => Cursors.Default);
    public static readonly CursorKind Crosshair = new(key: 1, cursor: static _ => Cursors.Crosshair);
    public static readonly CursorKind Pointer = new(key: 2, cursor: static _ => Cursors.Pointer);
    public static readonly CursorKind IBeam = new(key: 3, cursor: static _ => Cursors.IBeam);
    public static readonly CursorKind Move = new(key: 4, cursor: static _ => Cursors.Move);
    public static readonly CursorKind VerticalSplit = new(key: 5, cursor: static _ => Cursors.VerticalSplit);
    public static readonly CursorKind HorizontalSplit = new(key: 6, cursor: static _ => Cursors.HorizontalSplit);
    public static readonly CursorKind SizeAll = new(key: 7, cursor: static _ => Cursors.SizeAll);
    public static readonly CursorKind SizeLeft = new(key: 12, cursor: static _ => Cursors.SizeLeft);
    public static readonly CursorKind SizeRight = new(key: 13, cursor: static _ => Cursors.SizeRight);
    public static readonly CursorKind SizeTop = new(key: 14, cursor: static _ => Cursors.SizeTop);
    public static readonly CursorKind SizeBottom = new(key: 15, cursor: static _ => Cursors.SizeBottom);
    public static readonly CursorKind SizeTopLeft = new(key: 16, cursor: static _ => Cursors.SizeTopLeft);
    public static readonly CursorKind SizeTopRight = new(key: 17, cursor: static _ => Cursors.SizeTopRight);
    public static readonly CursorKind SizeBottomLeft = new(key: 18, cursor: static _ => Cursors.SizeBottomLeft);
    public static readonly CursorKind SizeBottomRight = new(key: 19, cursor: static _ => Cursors.SizeBottomRight);
    public static readonly CursorKind NotAllowed = new(key: 8, cursor: static _ => Cursors.NotAllowed);
    // GH2 wire cursor properties can be absent; item delegates fall back to stock Eto cursors.
    public static readonly CursorKind WireIn = new(key: 9, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorWireIn).IfNone(Cursors.Pointer));
    public static readonly CursorKind WireOut = new(key: 10, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorWireOut).IfNone(Cursors.Pointer));
    public static readonly CursorKind WireQuestion = new(key: 11, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorQuestion).IfNone(Cursors.Default));
    public static readonly CursorKind Wait = new(key: 20, cursor: static _ => Cursors.Default);

    [UseDelegateFromConstructor]
    internal partial Cursor Cursor(Grasshopper2.UI.Canvas.Canvas canvas);

    internal Fin<Cursor> Resolve(Grasshopper2.UI.Canvas.Canvas canvas) => Fin.Succ(Cursor(canvas: canvas));
}

[SmartEnum<int>]
public sealed partial class DialogPresentation {
    public static readonly DialogPresentation Modal = new(key: 0);
    public static readonly DialogPresentation AttachedSheet = new(key: 1);

    internal Option<TResult> Show<TResult>(Dialog<Option<TResult>> dialog, Control? parent) =>
        Switch(
            (Dialog: dialog, Parent: parent),
            modal: static s => s.Dialog.ShowModal(owner: s.Parent),
            attachedSheet: static s => {
                s.Dialog.DisplayMode = DialogDisplayMode.Attached;
                return s.Dialog.ShowModal(owner: s.Parent);
            });
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PresentationPolicy(Option<DialogPresentation> Presentation = default) {
    internal DialogPresentation DialogPresentation => Presentation.IfNone(DialogPresentation.Modal);
}

[SmartEnum<int>]
public sealed partial class FileDialogMode {
    private delegate PathDialogSnapshot DialogRun(Control? parent, PathDialogSpec spec);

    public static readonly FileDialogMode Open = new(
        key: 0,
        run: static (parent, spec) =>
            Input.RunFileDialog(dialog: new OpenFileDialog { MultiSelect = spec.MultiSelect }, parent: parent, spec: spec));
    public static readonly FileDialogMode Save = new(
        key: 1,
        run: static (parent, spec) =>
            Input.RunFileDialog(dialog: new SaveFileDialog(), parent: parent, spec: spec));
    public static readonly FileDialogMode Folder = new(
        key: 2,
        run: static (parent, spec) =>
            Input.RunFolderDialog(parent: parent, spec: spec));

    [UseDelegateFromConstructor]
    internal partial PathDialogSnapshot Run(Control? parent, PathDialogSpec spec);
}

[SmartEnum<int>]
public sealed partial class MenuCommandKind {
    private delegate Command MenuBuild(ContextMenu menu, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange);

    public static readonly MenuCommandKind Button = new(key: 0, build: static (_, command, _, _) => {
        Func<Fin<Unit>> run = command.Run;
        Command etoCommand = new() { ID = command.Name, MenuText = command.Name, ToolBarText = command.Name, ToolTip = command.Info, Enabled = command.EffectiveEnabled() };
        etoCommand.Executed += (_, _) => _ = GrasshopperUi.Handler(valid: run);
        return etoCommand;
    });
    public static readonly MenuCommandKind Check = new(key: 1, build: static (_menu, command, checkedState, onChange) => {
        CheckCommand menuCommand = new() { ID = command.Name, MenuText = command.Name, ToolTip = command.Info, Enabled = command.EffectiveEnabled(), Checked = checkedState };
        menuCommand.Executed += (_, _) => _ = GrasshopperUi.Handler(valid: () => onChange(arg: menuCommand.Checked));
        return menuCommand;
    });
    public static readonly MenuCommandKind Radio = new(key: 2, build: static (menu, command, checkedState, onChange) => {
        RadioCommand radio = new() {
            ID = command.Name, MenuText = command.Name, ToolTip = command.Info,
            Enabled = command.EffectiveEnabled(), Checked = checkedState,
            Controller = ToolbarItem.RadioControllers.GetValue(key: menu, createValueCallback: static _ => new RadioCommand()),
        };
        radio.Executed += (_, _) => _ = GrasshopperUi.Handler(valid: () => onChange(arg: radio.Checked));
        return radio;
    });

    [UseDelegateFromConstructor]
    internal partial Command Build(ContextMenu menu, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange);
}

[SmartEnum<int>]
public sealed partial class ToggleKind {
    private delegate Fin<Unit> BarProjector(Bar bar, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange);
    private delegate Fin<Unit> MenuProjector(ContextMenu menu, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange);

    public static readonly ToggleKind Toggle = new(
        key: 0,
        barProject: static (bar, command, checkedState, onChange) => ProjectBarToggle(op: Op.Of(name: nameof(Toggle)), bar: bar, command: command, checkedState: checkedState, optional: true, onChange: onChange, noun: "toggle"),
        menuProject: static (menu, command, checkedState, onChange) => ProjectMenuToggle(op: Op.Of(name: nameof(Toggle)), kind: MenuCommandKind.Check, menu: menu, command: command, checkedState: checkedState, onChange: onChange, noun: "toggle"));
    public static readonly ToggleKind Radio = new(
        key: 1,
        barProject: static (bar, command, checkedState, onChange) => ProjectBarToggle(op: Op.Of(name: nameof(Radio)), bar: bar, command: command, checkedState: checkedState, optional: false, onChange: onChange, noun: "radio"),
        menuProject: static (_, _, _, _) => RejectSurface(item: nameof(Radio), surface: "a context menu"));
    public static readonly ToggleKind MenuRadio = new(
        key: 2,
        barProject: static (_, _, _, _) => RejectSurface(item: nameof(MenuRadio), surface: "a toolbar"),
        menuProject: static (menu, command, checkedState, onChange) => ProjectMenuToggle(op: Op.Of(name: nameof(MenuRadio)), kind: MenuCommandKind.Radio, menu: menu, command: command, checkedState: checkedState, onChange: onChange, noun: "radio"));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> BarProject(Bar bar, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange);
    [UseDelegateFromConstructor]
    internal partial Fin<Unit> MenuProject(ContextMenu menu, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange);

    private static Fin<Unit> ProjectBarToggle(Op op, Bar bar, UiCommand command, bool checkedState, bool optional, Func<bool, Fin<Unit>> onChange, string noun) =>
        from changed in op.NeedChanged(onChange, noun: noun)
        from added in op.Attempt(body: () => ToolbarItem.AddRadioToggle(bar: bar, command: command, state: checkedState, optional: optional, changed: changed), what: $"{noun} toggle")
        select added;

    private static Fin<Unit> ProjectMenuToggle(Op op, MenuCommandKind kind, ContextMenu menu, UiCommand command, bool checkedState, Func<bool, Fin<Unit>> onChange, string noun) =>
        from changed in op.NeedChanged(onChange, noun: noun)
        from added in op.Attempt(body: () => UiCommand.BindMenu(kind: kind, menu: menu, command: command, checkedState: checkedState, onChange: changed), what: $"menu {noun}")
        select added;

    private static Fin<Unit> RejectSurface(string item, string surface) =>
        Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: $"{item} cannot be projected to {surface}"));
}

[SkipUnionOps]
[Union]
public partial record InputSelectionSource {
    private InputSelectionSource() { }
    public sealed record ControlCase(Control Source) : InputSelectionSource;
    public sealed record MouseCase(MouseEventArgs Source) : InputSelectionSource;
    public sealed record WindowCase(WindowSelectionEventArgs Source) : InputSelectionSource;

    public static InputSelectionSource From(Control control) => new ControlCase(Source: control);
    public static InputSelectionSource From(MouseEventArgs mouse) => new MouseCase(Source: mouse);
    public static InputSelectionSource From(WindowSelectionEventArgs window) => new WindowCase(Source: window);

    internal Fin<SelectionMode> Mode() => Switch(
        state: Op.Of(name: nameof(InputSelectionSource)),
        controlCase: static (op, c) => Optional(c.Source)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null control"))
            .Map(static source => SelectionMode.FromGh(gh: source.SelectionMode())),
        mouseCase: static (op, m) => Optional(m.Source)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null mouse event"))
            .Map(static source => SelectionMode.FromGh(gh: source.SelectionMode())),
        windowCase: static (op, w) => Optional(w.Source)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null window selection event"))
            .Map(static source => SelectionMode.FromGh(gh: source.SelectionMode())));
}

[SkipUnionOps]
[Union]
public partial record InputClipboardOp {
    private InputClipboardOp() { }
    public sealed record ReadCase(Seq<string> DataTypes = default) : InputClipboardOp;
    public sealed record WriteCase(InputClipboardSnapshot Payload) : InputClipboardOp;
    public sealed record ClearCase : InputClipboardOp;
    public static readonly InputClipboardOp Read = new ReadCase();
    public static InputClipboardOp Write(string text) => new WriteCase(Payload: InputClipboardSnapshot.Of(text: text));
    public static InputClipboardOp Write(InputClipboardSnapshot payload) => new WriteCase(Payload: payload);
    public static readonly InputClipboardOp Clear = new ClearCase();
    public static InputClipboardOp WriteData(byte[]? data, string type) =>
        Write(payload: InputClipboardSnapshot.Empty with { Data = Seq(new InputClipboardDataEntry(Type: type, Bytes: Optional(data).Map(static bytes => bytes.ToArray()))) });
    public static InputClipboardOp ReadData(string type) => new ReadCase(DataTypes: Seq(type));

    public static InputClipboardOp operator |(InputClipboardOp left, InputClipboardOp right) =>
        (left, right) switch {
            (_, ClearCase) or (ClearCase, _) => Clear,
            (WriteCase l, WriteCase r) => new WriteCase(Payload: l.Payload | r.Payload),
            (WriteCase, _) => left,
            (_, WriteCase) => right,
            (ReadCase l, ReadCase r) => new ReadCase(DataTypes: toSeq(l.DataTypes) + toSeq(r.DataTypes)),
            _ => right,
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct InputModifierSnapshot(bool Shift, bool Command, bool Option);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerSnapshot(PointF ScreenPosition, MouseButtons Buttons);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputSelectionSnapshot(SelectionMode Mode, InputModifierSnapshot Modifiers);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputPanelSnapshot(int Count, string Category);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ToolbarSnapshot(int Count, bool Enabled, int MinimumWidth, int MaximumWidth, int Height);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MenuSnapshot(int Count);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputClipboardDataEntry(string Type, Option<byte[]> Bytes = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputClipboardSnapshot(Option<string> Text, Option<string> Html, Option<Image> Image, Seq<Uri> Uris, Seq<string> Types, Seq<InputClipboardDataEntry> Data = default) {
    public static InputClipboardSnapshot Empty => new(
        Text: Option<string>.None,
        Html: Option<string>.None,
        Image: Option<Image>.None,
        Uris: Seq<Uri>(),
        Types: Seq<string>(),
        Data: Seq<InputClipboardDataEntry>());

    public static InputClipboardSnapshot Of(string text) => Empty with { Text = Optional(text) };

    public static InputClipboardSnapshot operator |(InputClipboardSnapshot left, InputClipboardSnapshot right) =>
        new(
            Text: right.Text | left.Text,
            Html: right.Html | left.Html,
            Image: right.Image | left.Image,
            Uris: left.Uris + right.Uris,
            Types: left.Types + right.Types,
            Data: toSeq(left.Data) + toSeq(right.Data));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct TransferPayload(InputClipboardSnapshot Clipboard, Option<DragEffects> Effects = default, Option<DragEffects> AllowedEffects = default) {
    public static TransferPayload Empty => new(Clipboard: InputClipboardSnapshot.Empty);
    public static TransferPayload Of(InputClipboardSnapshot clipboard) => new(Clipboard: clipboard);
}

[SkipUnionOps]
[Union]
public partial record InputTransferOp {
    private InputTransferOp() { }
    public sealed record ClipboardCase(InputClipboardOp Op) : InputTransferOp;
    public static InputTransferOp Clipboard(InputClipboardOp op) => new ClipboardCase(Op: op);
}

[SkipUnionOps]
[Union]
public partial record InputPanelContent {
    private InputPanelContent() { }
    public sealed record PanelCase(Func<InputPanel, Fin<Unit>> Populate) : InputPanelContent;
    public sealed record ControlContentCase(Control Control) : InputPanelContent;
    public static InputPanelContent OfPanel(Func<InputPanel, Fin<Unit>> populate) => new PanelCase(Populate: populate);
    public static InputPanelContent OfControl(Control control) => new ControlContentCase(Control: control);
}

[SkipUnionOps]
[Union]
public abstract partial record FormField {
    private FormField(string key, string label) => (Key, Label) = (key, label);
    public string Key { get; }
    public string Label { get; }
    internal abstract Control Render();
    internal abstract object Capture(Control control);

    public sealed record Text(string Key, string Label, string Value = "") : FormField(Key, Label) {
        internal override Control Render() => new TextBox { Text = Value };
        internal override object Capture(Control control) => control is TextBox box ? box.Text : string.Empty;
    }
    public sealed record TextArea(string Key, string Label, string Value = "") : FormField(Key, Label) {
        internal override Control Render() => new Eto.Forms.TextArea { Text = Value, Size = new Size(width: 360, height: 120) };
        internal override object Capture(Control control) => control is Eto.Forms.TextArea box ? box.Text : string.Empty;
    }
    public sealed record Toggle(string Key, string Label, bool Value = false) : FormField(Key, Label) {
        internal override Control Render() => new CheckBox { Checked = Value };
        internal override object Capture(Control control) => control is CheckBox box && (box.Checked ?? false);
    }
    public sealed record Number(string Key, string Label, double Value = 0d, UiNumberRange Range = default) : FormField(Key, Label) {
        internal override Control Render() =>
            Range.Bounds.Match(
                Some: bounds => (Control)new NumericStepper { MinValue = bounds.Minimum, MaxValue = bounds.Maximum, Value = Math.Clamp(value: Value, min: bounds.Minimum, max: bounds.Maximum) },
                None: () => new TextBox { Text = string.Create(provider: CultureInfo.InvariantCulture, $"{Value}") });
        internal override object Capture(Control control) =>
            control switch {
                NumericStepper stepper => stepper.Value,
                TextBox box => ParseText(text: box.Text),
                _ => Value,
            };
        // BOUNDARY ADAPTER — free-text NumericStepper fallback must not throw; reject invalid input back to Value.
        private double ParseText(string text) =>
            double.TryParse(s: text, style: NumberStyles.Float | NumberStyles.AllowLeadingSign, provider: CultureInfo.InvariantCulture, result: out double parsed)
                ? parsed
                : Value;
    }
    public sealed record Choice(string Key, string Label, Seq<string> Options, int Selected = 0) : FormField(Key, Label) {
        internal override Control Render() {
            DropDown control = new();
            _ = Options.Iter(control.Items.Add);
            control.SelectedIndex = Math.Clamp(value: Selected, min: 0, max: Math.Max(val1: control.Items.Count - 1, val2: 0));
            return control;
        }
        internal override object Capture(Control control) => control is DropDown drop && drop.SelectedIndex >= 0 ? Options[drop.SelectedIndex] : string.Empty;
    }
    public sealed record Color(string Key, string Label, Eto.Drawing.Color Value = default, bool AllowAlpha = false) : FormField(Key, Label) {
        internal override Control Render() => new ColorPicker { Value = Value, AllowAlpha = AllowAlpha };
        internal override object Capture(Control control) => control is ColorPicker picker ? picker.Value : Value;
    }
    public sealed record Slider(string Key, string Label, int Value = 0, int Minimum = 0, int Maximum = 100) : FormField(Key, Label) {
        internal override Control Render() => new Eto.Forms.Slider { MinValue = Minimum, MaxValue = Maximum, Value = Math.Clamp(value: Value, min: Minimum, max: Maximum) };
        internal override object Capture(Control control) => control is Eto.Forms.Slider slider ? slider.Value : Value;
    }
    public sealed record DateTime(string Key, string Label, Option<System.DateTime> Value = default) : FormField(Key, Label) {
        internal override Control Render() => new DateTimePicker { Value = Value.Map(static value => (System.DateTime?)value).IfNone((System.DateTime?)null) };
        internal override object Capture(Control control) => control is DateTimePicker picker ? Optional(picker.Value) : Value;
    }
    public sealed record File(string Key, string Label, Option<string> Value = default) : FormField(Key, Label) {
        internal override Control Render() {
            FilePicker picker = new();
            _ = Value.Iter(path => picker.FilePath = path);
            return picker;
        }
        internal override object Capture(Control control) => control is FilePicker picker ? Optional(picker.FilePath) : Value;
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FormSnapshot(IReadOnlyDictionary<string, object> Values) {
    public Option<T> Value<T>(string key) =>
        Values.TryGetValue(key: key, value: out object? value) && value is T typed ? Some(typed) : Option<T>.None;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct FormPlan<TResult>(
    string Title,
    Seq<FormField> Fields,
    Func<FormSnapshot, Fin<TResult>> Submit,
    PresentationPolicy Presentation = default,
    CommandPlan Commands = default,
    string AcceptText = "OK",
    string CancelText = "Cancel");

public readonly record struct FileFilter(string Name, Seq<string> Extensions);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PathDialogSpec(
    FileDialogMode Mode,
    Option<string> InitialPath = default,
    Seq<FileFilter> Filters = default,
    bool MultiSelect = false,
    Option<string> Title = default,
    Option<int> FilterIndex = default,
    Option<bool> CheckFileExists = default);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PathDialogSnapshot(Seq<string> Paths, int FilterIndex, Option<string> FilterName, bool Accepted) {
    public static PathDialogSnapshot Cancelled => new(Paths: Seq<string>(), FilterIndex: -1, FilterName: Option<string>.None, Accepted: false);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiNumberRange(Option<double> Minimum = default, Option<double> Maximum = default) {
    internal Option<(double Minimum, double Maximum)> Bounds {
        get {
            Option<double> minimum = Minimum;
            Option<double> maximum = Maximum;
            return minimum.Bind(min => maximum.Map(max => (Minimum: min, Maximum: max)))
                .Filter(static bounds => double.IsFinite(bounds.Minimum) && double.IsFinite(bounds.Maximum) && bounds.Minimum <= bounds.Maximum);
        }
    }
}

[ComplexValueObject(DefaultStringComparison = StringComparison.Ordinal)]
[ValidationError<UiFault>]
public readonly partial struct UiCommand {
    public string Name { get; }
    public string Info { get; }
    public Func<Fin<Unit>> Run { get; }
    public Option<IIcon> Icon { get; }
    public Option<Image> Image { get; }
    public Option<BarShortcut> Shortcut { get; }
    public bool Enabled { get; }
    public Option<Func<Fin<bool>>> CanExecute { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref UiFault? validationError,
        ref string name,
        ref string info,
        ref Func<Fin<Unit>> run,
        ref Option<IIcon> icon,
        ref Option<Image> image,
        ref Option<BarShortcut> shortcut,
        ref bool enabled,
        ref Option<Func<Fin<bool>>> canExecute) {
        Op op = Op.Of(name: nameof(UiCommand));
        string commandName = name;
        bool missingRun = run is null;
        bool missingName = string.IsNullOrWhiteSpace(commandName);
        validationError = (missingRun, missingName) switch {
            (true, _) => UiFault.Create(op: op, message: "Run delegate is required."),
            (false, true) => UiFault.Create(op: op, message: "Name is required."),
            _ => null,
        };
        _ = info;
        _ = icon;
        _ = image;
        _ = shortcut;
        _ = enabled;
        _ = canExecute;
    }

    internal bool EffectiveEnabled() =>
        Enabled && CanExecute.Map(can => GrasshopperUi.Protect(valid: can).IfFail(_ => false)).IfNone(noneValue: true);

    // GH2 toolbar shortcuts are nullable BarShortcut values, not Options.
    internal BarShortcut? ShortcutKey() => Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null);

    // GH2 IIcon rasterization can fail during programmatic plugin load; missing icons degrade to no menu image.
    private const int MenuIconExtent = 16;
    internal Option<Image> MenuImage =>
        Image | Icon.Bind(static icon =>
            Op.Of(name: nameof(MenuImage))
                .Attempt(body: () => icon.DrawToBitmap(size: new Size(width: MenuIconExtent, height: MenuIconExtent), padding: 0, background: Colors.Transparent), what: "icon.DrawToBitmap")
                .Map(bitmap => Optional<Image>(bitmap))
                .IfFail(Option<Image>.None));

    // BOUNDARY ADAPTER — append an Eto menu command of the requested kind and wire its shared visual/validity surface.
    internal static Unit BindMenu(MenuCommandKind kind, ContextMenu menu, UiCommand command, bool checkedState = false, Func<bool, Fin<Unit>>? onChange = null) {
        Command etoCommand = kind.Build(menu: menu, command: command, checkedState: checkedState, onChange: onChange ?? (static _ => Fin.Succ(value: unit)));
        _ = command.MenuImage.Iter(image => etoCommand.Image = image);
        _ = command.Shortcut.Iter(shortcut => etoCommand.Shortcut = shortcut.Keys);
        MenuItem menuItem = etoCommand.CreateMenuItem();
        _ = command.CanExecute.Iter(can => menuItem.Validate += (_, _) =>
            menuItem.Enabled = command.Enabled && GrasshopperUi.Protect(valid: can).IfFail(_ => false));
        menu.Items.Add(item: menuItem);
        return unit;
    }
}

[SkipUnionOps]
[Union]
internal partial record UiCommandSurface {
    private UiCommandSurface() { }
    public sealed record ToolbarCase(Bar Bar) : UiCommandSurface;
    public sealed record MenuCase(ContextMenu Target) : UiCommandSurface;
    public sealed record InputPanelCase(InputPanel Panel) : UiCommandSurface;
    public static UiCommandSurface Toolbar(Bar bar) => new ToolbarCase(Bar: bar);
    public static UiCommandSurface Menu(ContextMenu menu) => new MenuCase(Target: menu);
    public static UiCommandSurface InputPanel(InputPanel panel) => new InputPanelCase(Panel: panel);
}

[GenerateUnionOps]
[Union]
public partial record ToolbarItem {
    private ToolbarItem() { }
    public sealed partial record ButtonCase(UiCommand Command, bool CloseOnActivate = true) : ToolbarItem;
    public sealed partial record ToggleItem(UiCommand Command, bool Checked, Func<bool, Fin<Unit>> OnChange, ToggleKind Kind) : ToolbarItem;
    public sealed partial record SectionToggleCase(string Name, bool State, Seq<string> Sections = default) : ToolbarItem;
    public sealed partial record TextInputCase(string Name, string Value, Func<string, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record NumberCase(string Name, UiNumber Value, Func<decimal, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SwatchInputCase(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record ColourBarsCase(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SpacerCase(string Chapter, string Section) : ToolbarItem;
    public sealed partial record LabelCase(string Caption) : ToolbarItem;
    public sealed partial record CheckCase(string Name, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record TextCase(string Name, string Value, Func<string, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SpectrumCase(string Name, Seq<OpenColor.Family> Palette, OpenColor.Family Initial, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SubmenuCase(string Name, CommandPlan Plan) : ToolbarItem;

    public static ToolbarItem Button(UiCommand command, bool closeOnActivate = true) => new ButtonCase(Command: command, CloseOnActivate: closeOnActivate);
    public static ToolbarItem Toggle(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new ToggleItem(Command: command, Checked: state, OnChange: changed, Kind: ToggleKind.Toggle);
    public static ToolbarItem SectionToggle(string name, bool state, Seq<string> sections = default) => new SectionToggleCase(Name: name, State: state, Sections: sections);
    public static ToolbarItem Radio(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new ToggleItem(Command: command, Checked: state, OnChange: changed, Kind: ToggleKind.Radio);
    public static ToolbarItem TextInput(string name, string value, Func<string, Fin<Unit>> changed) => new TextInputCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem Number(string name, UiNumber value, Func<decimal, Fin<Unit>> changed) => new NumberCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem SwatchInput(string name, OpenColor.Family family, Func<OpenColor.Family, Fin<Unit>> changed) => new SwatchInputCase(Name: name, Family: family, Changed: changed);
    public static ToolbarItem ColourBars(string name, OpenColor.Family family, Func<OpenColor.Family, Fin<Unit>> changed) => new ColourBarsCase(Name: name, Family: family, Changed: changed);
    public static ToolbarItem Spacer(string chapter, string section) => new SpacerCase(Chapter: chapter, Section: section);
    public static ToolbarItem Label(string text) => new LabelCase(Caption: text);
    public static ToolbarItem Check(string name, bool state, Func<bool, Fin<Unit>> changed) => new CheckCase(Name: name, State: state, Changed: changed);
    public static ToolbarItem Text(string name, string value, Func<string, Fin<Unit>> changed) => new TextCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem Spectrum(string name, OpenColor.Family[] spectrum, OpenColor.Family initial, Func<OpenColor.Family, Fin<Unit>> changed) => new SpectrumCase(Name: name, Palette: toSeq(spectrum), Initial: initial, Changed: changed);
    public static ToolbarItem Submenu(string name, CommandPlan plan) => new SubmenuCase(Name: name, Plan: plan);
    public static ToolbarItem MenuRadio(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new ToggleItem(Command: command, Checked: state, OnChange: changed, Kind: ToggleKind.MenuRadio);

    internal bool IsPanelNative => this is LabelCase or CheckCase or TextCase;

    internal Fin<Unit> Apply(UiCommandSurface surface) =>
        Optional(surface)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(UiCommandSurface)))
            .Bind(valid => valid.Switch(
                toolbarCase: toolbar => ProjectToolbar(bar: toolbar.Bar),
                menuCase: menu => ProjectMenu(menu: menu.Target),
                inputPanelCase: panel => ProjectPanel(panel: panel.Panel)));

    private Fin<Unit> ProjectToolbar(Bar bar) => Switch(
        state: bar,
        buttonCase: static (bar, item) => Fin.Succ(item.Command).Map(command => {
            PushButton pushed = bar.AddPushButton(
                icon: command.Icon.IfNone(noneValue: StandardIcons.Parameters.Unknown),
                nomen: new Nomen(name: command.Name, info: command.Info),
                callback: () => _ = GrasshopperUi.Handler(valid: command.Run),
                keys: command.ShortcutKey());
            pushed.Enabled = command.EffectiveEnabled();
            pushed.CloseOnActivate = item.CloseOnActivate;
            return unit;
        }),
        toggleItem: static (bar, item) => item.Kind.BarProject(bar: bar, command: item.Command, checkedState: item.Checked, onChange: item.OnChange),
        sectionToggleCase: static (bar, item) => SectionToggleCase.SelfOp.Attempt(body: () => {
            _ = bar.AddToggle(nomen: new Nomen(name: item.Name, info: item.Name), initialState: item.State, additionalAffectedSections: [.. item.Sections]);
            return unit;
        }, what: "AddToggle"),
        textInputCase: static (bar, item) =>
            from changed in TextInputCase.SelfOp.NeedChanged(item.Changed, noun: "text")
            from added in TextInputCase.SelfOp.Attempt(body: () => {
                TextField field = bar.AddTextField(icon: StandardIcons.Parameters.Text, nomen: new Nomen(name: item.Name, info: item.Name), initial: item.Value, placeholder: item.Name);
                field.TextChanged += (_, value) => _ = GrasshopperUi.Handler(valid: () => changed(arg: value));
                return unit;
            }, what: "AddTextField")
            select added,
        numberCase: static (bar, item) =>
            from changed in Optional(item.Changed).ToFin(Fail: UiFault.InvalidInput(op: NumberCase.SelfOp, detail: "number change delegate is required"))
            from value in Optional(item.Value).ToFin(Fail: UiFault.InvalidInput(op: NumberCase.SelfOp, detail: "UiNumber is required"))
            from added in NumberCase.SelfOp.Attempt(
                body: () => bar.Add(new NumberSlider(nomen: new Nomen(name: item.Name, info: item.Name), number: value, callback: current => _ = GrasshopperUi.Handler(valid: () => changed(arg: current)))),
                what: "NumberSlider")
            select added,
        swatchInputCase: static (bar, item) =>
            from changed in SwatchInputCase.SelfOp.NeedChanged(item.Changed, noun: "colour")
            from added in SwatchInputCase.SelfOp.Attempt(
                body: () => bar.AddLifeColours(nomen: new Nomen(name: item.Name, info: item.Name), initial: item.Family, assignment: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value))),
                what: "AddLifeColours")
            select added,
        colourBarsCase: static (bar, item) =>
            from changed in ColourBarsCase.SelfOp.NeedChanged(item.Changed, noun: "colour")
            from added in ColourBarsCase.SelfOp.Attempt(body: () => {
                void Assign(OpenColor.Family value) => _ = GrasshopperUi.Handler(valid: () => changed(arg: value));
                Nomen nomen = new(name: $"{item.Name} {{family}}", info: $"{item.Name} {{family}}");
                Bar.CreateStandardColourBars(nomen: nomen, initial: item.Family, assignment: Assign, out Bar life, out Bar cool, out Bar warm);
                Seq<Seq<RadioToggle>> groups = Seq(life, cool, warm).Map(static toolbar => toSeq(toolbar.ActiveElements.OfType<RadioToggle>())).ToSeq();
                _ = groups.Bind(static toggles => toggles).Iter(bar.Add);
                _ = toSeq(Enumerable.Range(start: 0, count: groups.Count)).Iter(index =>
                    groups[index].Iter(toggle => toggle.StateChanged += (_, active) =>
                        _ = Optional(active).Filter(static selected => selected).Iter(_ => toSeq(groups.Where((_, i) => i != index)).Bind(static toggles => toggles).Iter(static t => Op.Side(() => t.SetState(state: false))))));
                return unit;
            }, what: "Bar.CreateStandardColourBars")
            select added,
        spacerCase: static (bar, item) => SpacerCase.SelfOp.Attempt(body: () => {
            _ = bar.AddSpacer(chapterName: item.Chapter, sectionName: item.Section);
            return unit;
        }, what: "AddSpacer"),
        spectrumCase: static (bar, item) =>
            from changed in SpectrumCase.SelfOp.NeedChanged(item.Changed, noun: "colour")
            from added in SpectrumCase.SelfOp.Attempt(
                body: () => bar.AddColours(nomen: new Nomen(name: item.Name, info: item.Name), spectrum: [.. item.Palette], initial: item.Initial, assignment: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value))),
                what: "AddColours")
            select added,
        labelCase: static (_, _) => Reject(item: nameof(LabelCase), surface: "a toolbar"),
        checkCase: static (_, _) => Reject(item: nameof(CheckCase), surface: "a toolbar"),
        textCase: static (_, _) => Reject(item: nameof(TextCase), surface: "a toolbar"),
        submenuCase: static (_, _) => Reject(item: nameof(SubmenuCase), surface: "a toolbar"));

    // Context menus accept only menu-capable items; generated Switch rejects the rest exhaustively.
    private Fin<Unit> ProjectMenu(ContextMenu menu) => Switch(
        state: menu,
        buttonCase: static (menu, item) => ButtonCase.SelfOp.Attempt(body: () => UiCommand.BindMenu(kind: MenuCommandKind.Button, menu: menu, command: item.Command), what: "menu button"),
        toggleItem: static (menu, item) => item.Kind.MenuProject(menu: menu, command: item.Command, checkedState: item.Checked, onChange: item.OnChange),
        spacerCase: static (menu, _) => SpacerCase.SelfOp.Attempt(body: menu.AddSeparator, what: "AddSeparator"),
        submenuCase: static (menu, item) => PushSubmenu(menu: menu, name: item.Name, plan: item.Plan),
        sectionToggleCase: static (_, _) => Reject(item: nameof(SectionToggleCase), surface: "a context menu"),
        textInputCase: static (_, _) => Reject(item: nameof(TextInputCase), surface: "a context menu"),
        numberCase: static (_, _) => Reject(item: nameof(NumberCase), surface: "a context menu"),
        swatchInputCase: static (_, _) => Reject(item: nameof(SwatchInputCase), surface: "a context menu"),
        colourBarsCase: static (_, _) => Reject(item: nameof(ColourBarsCase), surface: "a context menu"),
        labelCase: static (_, _) => Reject(item: nameof(LabelCase), surface: "a context menu"),
        checkCase: static (_, _) => Reject(item: nameof(CheckCase), surface: "a context menu"),
        textCase: static (_, _) => Reject(item: nameof(TextCase), surface: "a context menu"),
        spectrumCase: static (_, _) => Reject(item: nameof(SpectrumCase), surface: "a context menu"));

    private static Fin<Unit> PushSubmenu(ContextMenu menu, string name, CommandPlan plan) {
#pragma warning disable CA2000 // scratch render target: child items reparent into the submenu on Add; the empty husk is transient and host-collected
        ContextMenu child = new();
#pragma warning restore CA2000
        return plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Menu(menu: child))).As().Map(_seq => {
            // BOUNDARY ADAPTER - ContextMenu.Items reparent into the submenu on Add.
            SubMenuItem submenu = new() { Text = name };
            _ = toSeq(child.Items.ToArray()).Iter(item => submenu.Items.Add(item: item));
            menu.Items.Add(item: submenu);
            return unit;
        });
    }

    internal static readonly System.Runtime.CompilerServices.ConditionalWeakTable<ContextMenu, RadioCommand> RadioControllers = [];

    private static Fin<Unit> Reject(string item, string surface) =>
        Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: $"{item} cannot be projected to {surface}"));

    private Fin<Unit> ProjectPanel(InputPanel panel) => Switch(
        state: panel,
        labelCase: static (panel, item) => LabelCase.SelfOp.Attempt(body: () => {
            _ = panel.AddLabel(text: item.Caption);
            return unit;
        }, what: "AddLabel"),
        checkCase: static (panel, item) =>
            from changed in CheckCase.SelfOp.NeedChanged(item.Changed, noun: "check")
            from added in CheckCase.SelfOp.Attempt(body: () => {
                _ = panel.AddCheck(text: item.Name, @checked: item.State, checkedChanged: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value)));
                return unit;
            }, what: "AddCheck")
            select added,
        textCase: static (panel, item) =>
            from changed in TextCase.SelfOp.NeedChanged(item.Changed, noun: "text")
            from added in TextCase.SelfOp.Attempt(body: () => {
                _ = panel.AddText(text: item.Value, textChanged: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value)));
                return unit;
            }, what: "AddText")
            select added,
        buttonCase: static (_, _) => Reject(item: nameof(ButtonCase), surface: "an input panel"),
        toggleItem: static (_, _) => Reject(item: nameof(ToggleItem), surface: "an input panel"),
        sectionToggleCase: static (_, _) => Reject(item: nameof(SectionToggleCase), surface: "an input panel"),
        textInputCase: static (_, _) => Reject(item: nameof(TextInputCase), surface: "an input panel"),
        numberCase: static (_, _) => Reject(item: nameof(NumberCase), surface: "an input panel"),
        swatchInputCase: static (_, _) => Reject(item: nameof(SwatchInputCase), surface: "an input panel"),
        colourBarsCase: static (_, _) => Reject(item: nameof(ColourBarsCase), surface: "an input panel"),
        spacerCase: static (_, _) => Reject(item: nameof(SpacerCase), surface: "an input panel"),
        spectrumCase: static (_, _) => Reject(item: nameof(SpectrumCase), surface: "an input panel"),
        submenuCase: static (_, _) => Reject(item: nameof(SubmenuCase), surface: "an input panel"));

    internal static Unit AddRadioToggle(Bar bar, UiCommand command, bool state, bool optional, Func<bool, Fin<Unit>> changed) {
        RadioToggle toggled = bar.AddRadioToggle(
            icon: command.Icon.IfNone(noneValue: StandardIcons.Parameters.Unknown),
            nomen: new Nomen(name: command.Name, info: command.Info),
            initial: state,
            callback: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value)),
            keys: command.ShortcutKey());
        toggled.Optional = optional;
        toggled.Enabled = command.EffectiveEnabled();
        return unit;
    }
}

public readonly record struct CommandPlan(Seq<ToolbarItem> Items) {
    public static CommandPlan Empty => new(Items: Seq<ToolbarItem>());
    public static CommandPlan operator +(CommandPlan left, CommandPlan right) => new(Items: left.Items + right.Items);
    public static CommandPlan OfButtons(Seq<UiCommand> commands, bool closeOnActivate = true) =>
        new(Items: commands.Map(command => ToolbarItem.Button(command: command, closeOnActivate: closeOnActivate)));
    public static CommandPlan OfToggles(Seq<(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed)> toggles) =>
        new(Items: toggles.Map(static toggle => ToolbarItem.Toggle(command: toggle.Command, state: toggle.State, changed: toggle.Changed)));
}

internal sealed class ScopedCursor(Grasshopper2.UI.Canvas.Canvas target, Cursor previous, IDisposable? busy = null) : IDisposable {
    // BOUNDARY ADAPTER - restore the canvas cursor and release any WaitCursor lease exactly once.
    public void Dispose() {
        busy?.Dispose();
        target.Cursor = previous;
    }
}

file static class WaitCursorLease {
    private static readonly Atom<(int Count, Rhino.UI.WaitCursor? Cursor)> State = Atom<(int Count, Rhino.UI.WaitCursor? Cursor)>(value: (Count: 0, Cursor: null));

    internal static IDisposable Enter() {
        // BOUNDARY ADAPTER - WaitCursor sets the app cursor; the Atom lease prevents duplicate clears.
        Rhino.UI.WaitCursor? candidate = new();
        try {
            (int Count, Rhino.UI.WaitCursor? Cursor) swapped = State.Swap(current =>
                (Count: current.Count + 1, Cursor: current.Cursor ?? candidate));
            candidate = ReferenceEquals(swapped.Cursor, candidate) ? null : candidate;
            return new Token();
        } finally {
            candidate?.Dispose();
        }
    }

    private sealed class Token : IDisposable {
        private int disposed;

        public void Dispose() {
            // BOUNDARY ADAPTER - one-shot guard prevents doubled Dispose from underflowing the lease.
            Rhino.UI.WaitCursor? release = Interlocked.Exchange(ref disposed, 1) == 1
                ? null
                : Released();
            release?.Dispose();
        }

        private static Rhino.UI.WaitCursor? Released() {
            Rhino.UI.WaitCursor? captured = null;
            _ = State.Swap(state => {
                int next = Math.Max(0, state.Count - 1);
                captured = next == 0 ? state.Cursor : null;
                return (Count: next, Cursor: next == 0 ? null : state.Cursor);
            });
            return captured;
        }
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static partial class Input {
    public static GrasshopperUiIntent<InputSelectionSnapshot> Selection(InputSelectionSource source, Option<Keys> modifierOverride = default) =>
        GhUi.Read(run: _scope =>
            Optional(source)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "null source"))
                .Bind(valid =>
                    from mode in valid.Mode()
                    select new InputSelectionSnapshot(Mode: mode, Modifiers: ModifierOf(keys: modifierOverride.IfNone(() => Keyboard.Modifiers)))));

    public static GrasshopperUiIntent<InputModifierSnapshot> Modifiers(Keys keys) =>
        GhUi.Read(run: _scope => Fin.Succ(value: ModifierOf(keys: keys)));

    public static GrasshopperUiIntent<InputModifierSnapshot> ModifierState() =>
        GhUi.Read(run: _scope => Fin.Succ(value: ModifierOf(keys: Keyboard.Modifiers)));

    public static GrasshopperUiIntent<PointerSnapshot> PointerState() =>
        GhUi.Read(run: _scope =>
            Mouse.IsSupported
                ? Fin.Succ(value: new PointerSnapshot(ScreenPosition: Mouse.Position, Buttons: Mouse.Buttons))
                : Fin.Fail<PointerSnapshot>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(PointerState)), detail: "mouse position is not supported on this platform")));

    public static GrasshopperUiIntent<InputPanelSnapshot> Panel(Func<InputPanel, Fin<Unit>> populate) =>
        GhUi.Read(run: _scope =>
            Optional(populate)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Panel)), detail: "null populate"))
                .Bind(valid => {
                    InputPanel panel = new();
                    return valid(arg: panel).Map(_ => new InputPanelSnapshot(
                        Count: panel.Count,
                        Category: panel.Category ?? string.Empty));
                }));

    public static GrasshopperUiIntent<InputPanelSnapshot> Panel(CommandPlan plan) =>
        Panel(populate: panel => Try.lift(f: () => panel.AddBar(drawCategoryLabels: false)).Run()
            .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Panel)), detail: $"InputPanel.AddBar threw: {error.Message}"))
            .Bind(bar =>
                from native in plan.Items.Filter(static item => item.IsPanelNative).TraverseM(item => item.Apply(surface: UiCommandSurface.InputPanel(panel: panel))).As()
                from barred in plan.Items.Filter(static item => !item.IsPanelNative).TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).As()
                select unit));

    public static GrasshopperUiIntent<Subscription> PopupPanel(Control owner, PointF location, RectangleF screen, Func<InputPanel, Fin<Unit>> populate) =>
        PopupPanel(owner: owner, location: location, screen: screen, content: InputPanelContent.OfPanel(populate: populate));

    public static GrasshopperUiIntent<Subscription> PopupPanel(Control owner, PointF location, RectangleF screen, InputPanelContent content) =>
        GhUi.Read(run: _scope =>
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null owner"))
            from validContent in Optional(content).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null content"))
            from validLocation in Op.Of(name: nameof(PopupPanel)).AcceptPoint(value: location, detail: "non-finite location")
            from validScreen in Op.Of(name: nameof(PopupPanel)).AcceptRect(value: screen, detail: "invalid screen bounds", requirePositive: true)
            let clamped = new PointF(
                x: Math.Clamp(value: validLocation.X, min: validScreen.Left, max: validScreen.Right),
                y: Math.Clamp(value: validLocation.Y, min: validScreen.Top, max: validScreen.Bottom))
            from sub in validContent.Switch(
                panelCase: c => Optional(c.Populate)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null populate"))
                    .Bind(valid => {
                        InputPanel panel = new();
                        return valid(arg: panel).Bind(_ => PopupSubscription(panel: panel, owner: validOwner, location: clamped, screen: validScreen));
                    }),
                controlContentCase: c => Optional(c.Control)
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null control"))
                    .Bind(control => from app in GrasshopperUi.NeedApplication(op: Op.Of(name: nameof(PopupPanel)))
                                     from hosted in ControlPopupSubscription(app: app, control: control, owner: validOwner, location: clamped)
                                     select hosted))
            select sub);

    private static Fin<Subscription> PopupSubscription(InputPanel panel, Control owner, PointF location, RectangleF screen) {
        Form? form = null;
        return Subscription.Bind(
            attach: () => form = panel.ShowAsForm(owner, location, screen),
            // BOUNDARY ADAPTER - Form.Close alone does not release the macOS NSWindow; dispose after closing.
            detach: () => {
                form?.Close();
                form?.Dispose();
            },
            marshalToUi: true,
            detachOnce: true);
    }

    private static Fin<Subscription> ControlPopupSubscription(Application app, Control control, Control owner, PointF location) {
        Option<Window> ownerWindow = Optional(owner.ParentWindow);
        FloatingForm? form = null;
        return Subscription.Bind(
            // BOUNDARY ADAPTER - borderless, non-focusing FloatingForm at the clamped canvas point.
            attach: () => {
                form = new FloatingForm(app: app, owner: ownerWindow) {
                    Content = control,
                    WindowStyle = WindowStyle.None,
                    CanFocus = false,
                    Location = Eto.Drawing.Point.Round(point: location),
                };
                form.Show();
            },
            detach: () => {
                form?.Close();
                form?.Dispose();
            },
            marshalToUi: true,
            detachOnce: true);
    }

    internal static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(Func<Bar, Fin<Unit>> populate) =>
        GhUi.Read(run: _scope =>
            Optional(populate)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Toolbar)), detail: "null populate"))
                .Bind(valid => {
                    Bar bar = new();
                    return valid(arg: bar).Map(_ => {
                        bar.Layout();
                        return new ToolbarSnapshot(
                            Count: bar.Count, Enabled: bar.Enabled,
                            MinimumWidth: bar.MinimumWidth, MaximumWidth: bar.MaximumWidth,
                            Height: bar.Height);
                    });
                }));

    public static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(CommandPlan plan) =>
        Toolbar(populate: bar =>
            plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).Map(static _ => unit).As());

    public static GrasshopperUiIntent<MenuSnapshot> ShowMenu(CommandPlan plan, Control owner, PointF location) =>
        GhUi.Read(run: _scope =>
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ShowMenu)), detail: "null owner"))
            from validLocation in Op.Of(name: nameof(ShowMenu)).AcceptPoint(value: location, detail: "non-finite location")
            let menu = new ContextMenu()
            from populated in plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Menu(menu: menu))).As()
            from shown in Try.lift(f: () => {
                menu.Show(validOwner, validLocation);
                return new MenuSnapshot(Count: menu.Items.Count);
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ShowMenu)), detail: error.Message))
            select shown);

    public static GrasshopperUiIntent<CursorKind> Cursor(CursorKind kind) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(canvas =>
            kind.Resolve(canvas: canvas).Map(cursor => {
                canvas.Cursor = cursor;
                return kind;
            })));

    public static GrasshopperUiIntent<IDisposable> CursorScope(CursorKind kind) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(canvas =>
            kind.Resolve(canvas: canvas).Map(cursor => {
                Cursor previous = canvas.Cursor;
                canvas.Cursor = cursor;
                IDisposable? busy = kind == CursorKind.Wait ? WaitCursorLease.Enter() : null;
                return (IDisposable)new ScopedCursor(target: canvas, previous: previous, busy: busy);
            })));

    public static GrasshopperUiIntent<DialogResult> MessageDialog(string title, string message, MessageBoxType kind = MessageBoxType.Information, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Default) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => MessageBox.Show(
                parent: DialogParent(scope: scope),
                text: message,
                caption: title,
                buttons: buttons,
                type: kind,
                defaultButton: defaultButton))
            .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MessageDialog)), detail: $"MessageBox.Show threw: {error.Message}")));

    public static GrasshopperUiIntent<PathDialogSnapshot> FileDialog(PathDialogSpec spec) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => spec.Mode.Run(parent: DialogParent(scope: scope), spec: spec))
                .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(FileDialog)), detail: $"FileDialog.ShowDialog threw: {error.Message}")));

    public static GrasshopperUiIntent<InputClipboardSnapshot> Clipboard(InputClipboardOp op) =>
        GhUi.Read(run: _scope =>
            Optional(op)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Clipboard)), detail: "clipboard op is required"))
                .Bind(valid => valid.Switch(
                    readCase: static r =>
                        from types in ClipboardDataTypes(op: Op.Of(name: nameof(InputClipboardOp.Read)), types: r.DataTypes)
                        from clipboard in NeedClipboard(op: Op.Of(name: nameof(InputClipboardOp.Read)))
                        from snapshot in Op.Of(name: nameof(InputClipboardOp.Read)).Attempt(body: () => ClipboardSnapshotOf(clipboard: clipboard, dataTypes: types), what: "Clipboard snapshot")
                        select snapshot,
                    writeCase: static w =>
                        from data in ClipboardPayloadData(op: Op.Of(name: nameof(InputClipboardOp.Write)), data: w.Payload.Data)
                        from clipboard in NeedClipboard(op: Op.Of(name: nameof(InputClipboardOp.Write)))
                        from snapshot in Op.Of(name: nameof(InputClipboardOp.Write)).Attempt(body: () => {
                            clipboard.Clear();
                            _ = w.Payload.Text.Iter(text => clipboard.Text = text);
                            _ = w.Payload.Html.Iter(html => clipboard.Html = html);
                            _ = w.Payload.Image.Iter(image => clipboard.Image = image);
                            _ = w.Payload.Uris.IsEmpty ? unit : Optional(w.Payload.Uris.ToArray()).Iter(uris => clipboard.Uris = uris);
                            _ = data.Iter(entry => entry.Bytes.Iter(bytes => clipboard.SetData(value: bytes, type: entry.Type)));
                            return ClipboardSnapshotOf(clipboard: clipboard, dataTypes: data.Map(static entry => entry.Type));
                        }, what: "Clipboard write")
                        select snapshot,
                    clearCase: static _ =>
                        from clipboard in NeedClipboard(op: Op.Of(name: nameof(InputClipboardOp.Clear)))
                        from snapshot in Op.Of(name: nameof(InputClipboardOp.Clear)).Attempt(body: () => {
                            clipboard.Clear();
                            return InputClipboardSnapshot.Empty;
                        }, what: "Clipboard.Clear")
                        select snapshot)));

    public static GrasshopperUiIntent<TransferPayload> Transfer(InputTransferOp op) =>
        GhUi.Read(run: scope =>
            Optional(op)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Transfer)), detail: "transfer op is required"))
                .Bind(valid => valid.Switch(
                    clipboardCase: c => Clipboard(op: c.Op).Run(scope: scope).Map(TransferPayload.Of))));

    public static GrasshopperUiIntent<DragEffects> Drag(Control source, TransferPayload payload, Option<(Image Image, PointF Offset)> dragImage = default) =>
        GhUi.Read(run: _scope =>
            from validSource in Optional(source).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Drag)), detail: "null drag source"))
            from started in Op.Of(name: nameof(Drag)).Attempt(body: () => {
                // BOUNDARY ADAPTER - populate an Eto DataObject and run the platform drag loop.
                DataObject data = new();
                _ = payload.Clipboard.Text.Iter(text => data.Text = text);
                _ = payload.Clipboard.Html.Iter(html => data.Html = html);
                _ = payload.Clipboard.Image.Iter(image => data.Image = image);
                _ = payload.Clipboard.Uris.IsEmpty ? unit : Optional(payload.Clipboard.Uris.ToArray()).Iter(uris => data.Uris = uris);
                _ = payload.Clipboard.Data.Iter(entry => entry.Bytes.Iter(bytes => data.SetData(value: bytes, type: entry.Type)));
                DragEffects allowed = (payload.AllowedEffects | payload.Effects).IfNone(DragEffects.Copy);
                _ = dragImage is { IsSome: true, Case: ValueTuple<Image, PointF> di }
                    ? Op.Side(() => validSource.DoDragDrop(data: data, allowedEffects: allowed, image: di.Item1, cursorOffset: di.Item2))
                    : Op.Side(() => validSource.DoDragDrop(data: data, allowedEffects: allowed));
                return allowed;
            }, what: "Control.DoDragDrop")
            select started);

    public static GrasshopperUiIntent<Option<TResult>> Dialog<TResult>(Func<Dialog<Option<TResult>>, Fin<Unit>> configure, string title = "", Option<DialogPresentation> presentation = default) =>
        GhUi.Read(run: scope =>
            from validConfigure in Optional(configure).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Dialog)), detail: "null configure delegate"))
            from result in RunDialog(scope: scope, title: title, presentation: presentation.IfNone(DialogPresentation.Modal), configure: validConfigure)
            select result);

    private static Fin<Option<TResult>> RunDialog<TResult>(GrasshopperUi.Scope scope, string title, DialogPresentation presentation, Func<Dialog<Option<TResult>>, Fin<Unit>> configure) =>
        Try.lift<Fin<Option<TResult>>>(f: () => {
            using Dialog<Option<TResult>> dialog = new() { Title = title };
            Rhino.UI.EtoExtensions.UseRhinoStyle(dialog);
            return configure(arg: dialog).Map(_ => presentation.Show(dialog: dialog, parent: DialogParent(scope: scope)));
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Dialog)), detail: $"Dialog<T> threw: {error.Message}")).Bind(static r => r);

    public static GrasshopperUiIntent<Option<TResult>> Form<TResult>(FormPlan<TResult> plan) =>
        GhUi.Read(run: scope => RunForm(scope: scope, plan: plan));

    private static Fin<Option<TResult>> RunForm<TResult>(GrasshopperUi.Scope scope, FormPlan<TResult> plan) =>
        Optional(plan.Submit)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Form)), detail: "form submit delegate is required"))
            .Bind(submit => Try.lift<Fin<Option<TResult>>>(f: () => {
                using Dialog<Option<TResult>> dialog = new() { Title = plan.Title };
                DynamicLayout layout = new() { Padding = new Padding(all: 12), Spacing = new Size(width: 8, height: 8) };
                Seq<(FormField Field, Control Control)> controls = plan.Fields.Map(field => (Field: field, Control: field.Render())).ToSeq();
                _ = controls.Iter(entry => layout.AddRow(new Label { Text = entry.Field.Label }, entry.Control));
                Button accept = new() { Text = string.IsNullOrWhiteSpace(value: plan.AcceptText) ? "OK" : plan.AcceptText };
                Button cancel = new() { Text = string.IsNullOrWhiteSpace(value: plan.CancelText) ? "Cancel" : plan.CancelText };
                accept.Click += (_, _) => {
                    Fin<TResult> result = Try.lift(f: () => CaptureForm(controls: controls)).Run()
                        .MapFail(error => UiFault.InvalidInput(op: Op.Of(name: nameof(Form)), detail: error.Message))
                        .Bind(snapshot => submit(arg: snapshot));
                    _ = result.Map(value => { dialog.Close(Some(value)); return unit; })
                        .IfFail(error => {
                            _ = MessageBox.Show(parent: dialog, text: error.Message, caption: plan.Title, buttons: MessageBoxButtons.OK, type: MessageBoxType.Warning);
                            return unit;
                        });
                };
                cancel.Click += (_, _) => dialog.Close(Option<TResult>.None);
                _ = layout.AddRow(null, cancel, accept);
                dialog.DefaultButton = accept;
                dialog.AbortButton = cancel;
                dialog.Content = layout;
                Rhino.UI.EtoExtensions.UseRhinoStyle(dialog);
                return Fin.Succ(plan.Presentation.DialogPresentation.Show(dialog: dialog, parent: DialogParent(scope: scope)));
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Form)), detail: $"Form<T> threw: {error.Message}")).Bind(static value => value));

    private static FormSnapshot CaptureForm(Seq<(FormField Field, Control Control)> controls) =>
        new(Values: controls.ToDictionary(
            keySelector: static entry => entry.Field.Key,
            elementSelector: static entry => entry.Field.Capture(control: entry.Control),
            comparer: StringComparer.Ordinal));

    public static GrasshopperUiIntent<Option<Color>> PickColor(Color initial, bool allowAlpha = true) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => {
                using ColorDialog dialog = new() { Color = initial, AllowAlpha = allowAlpha };
                return dialog.ShowDialog(parent: DialogParent(scope: scope)) == DialogResult.Ok
                    ? Some(dialog.Color)
                    : Option<Color>.None;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(PickColor)), detail: $"ColorDialog threw: {error.Message}")));

    public static GrasshopperUiIntent<Option<Font>> PickFont(Option<Font> initial = default) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => {
                using FontDialog dialog = new();
                _ = initial.Iter(f => dialog.Font = f);
                return dialog.ShowDialog(parent: DialogParent(scope: scope)) == DialogResult.Ok
                    ? Some(dialog.Font)
                    : Option<Font>.None;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(PickFont)), detail: $"FontDialog threw: {error.Message}")));

    public static GrasshopperUiIntent<Option<string>> EditPrompt(string title, string message, string defaultText = "", bool multiline = false) =>
        GhUi.Read(run: _scope =>
            Op.Of(name: nameof(EditPrompt)).Attempt(
                body: () => Rhino.UI.Dialogs.ShowEditBox(title: title, message: message, defaultText: defaultText, multiline: multiline, text: out string text)
                    ? Optional(text)
                    : Option<string>.None,
                what: "Dialogs.ShowEditBox"));

    public static GrasshopperUiIntent<Option<double>> NumberPrompt(string title, string message, double initial = 0d, UiNumberRange range = default) =>
        GhUi.Read(run: _scope =>
            Op.Of(name: nameof(NumberPrompt)).Attempt(
                body: () => {
                    double number = initial;
                    bool accepted = range.Bounds
                        .Map(bounds => Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref number, minimum: bounds.Minimum, maximum: bounds.Maximum))
                        .IfNone(() => Rhino.UI.Dialogs.ShowNumberBox(title: title, message: message, number: ref number));
                    return accepted
                        ? Some(number)
                        : Option<double>.None;
                },
                what: "Dialogs.ShowNumberBox"));

    public static GrasshopperUiIntent<Subscription> Notify(string title, string message, Option<Image> contentImage = default, bool trayIndicator = false, Option<CommandPlan> menu = default, Option<Func<Fin<Unit>>> onActivated = default) =>
        GhUi.Read(run: _scope =>
            from indicator in Op.Of(name: nameof(Notify)).Attempt(
                body: () => trayIndicator || menu.IsSome ? Optional(new TrayIndicator { Title = title, Visible = true }) : Option<TrayIndicator>.None,
                what: "new TrayIndicator")
            from notification in Op.Of(name: nameof(Notify)).Attempt(body: () => {
                Notification built = new() { Title = title, Message = message };
                _ = contentImage.Iter(image => built.ContentImage = image);
                return built;
            }, what: "new Notification")
            from prepared in menu.Match(
                Some: plan => indicator
                    .ToFin(Fail: UiFault.MutationRejected(op: Op.Of(name: nameof(Notify)), detail: "tray menu requires a tray indicator"))
                    .Bind(tray => {
                        ContextMenu trayMenu = new();
                        return plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Menu(menu: trayMenu))).As()
                            .Map(_ => { tray.Menu = trayMenu; return unit; });
                    }),
                None: () => Fin.Succ(value: unit))
            from sub in NotifySubscription(indicator: indicator, notification: notification, onActivated: onActivated)
            select sub);

    private static Fin<Subscription> NotifySubscription(Option<TrayIndicator> indicator, Notification notification, Option<Func<Fin<Unit>>> onActivated) {
        EventHandler<NotificationEventArgs>? handler = null;
        return Subscription.Bind(
            // BOUNDARY ADAPTER - register the activation handler only after Show() so a failed Show leaks neither handler nor native objects.
            attach: () => {
                notification.Show(indicator: indicator.Map(static tray => (TrayIndicator?)tray).IfNone((TrayIndicator?)null));
                handler = onActivated is { IsSome: true, Case: Func<Fin<Unit>> activated }
                    ? ((_, _) => _ = GrasshopperUi.Handler(valid: activated))
                    : null;
                _ = Optional(handler).Iter(h => Application.Instance.NotificationActivated += h);
            },
            // BOUNDARY ADAPTER - detach the activation handler, then dispose the native notification and tray indicator.
            detach: () => {
                _ = Optional(handler).Iter(h => Application.Instance.NotificationActivated -= h);
                notification.Dispose();
                _ = indicator.Iter(tray => tray.Dispose());
            },
            marshalToUi: true,
            detachOnce: true);
    }

    public static GrasshopperUiIntent<Control> Scrollable(Control content, Size virtualSize = default) =>
        GhUi.Read(run: _scope =>
            from valid in Optional(content).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Scrollable)), detail: "null content"))
            from built in Op.Of(name: nameof(Scrollable)).Attempt(body: () => {
                // BOUNDARY ADAPTER - explicit virtual size disables Eto content expansion.
                Scrollable scrollable = new() { Content = valid };
                _ = Optional(virtualSize).Filter(static size => size.Width > 0 && size.Height > 0)
                    .Iter(size => { scrollable.ExpandContentWidth = false; scrollable.ExpandContentHeight = false; });
                return (Control)scrollable;
            }, what: "new Scrollable")
            select built);

    public static GrasshopperUiIntent<Control> Split(Control p1, Control p2, Orientation orientation = Orientation.Horizontal, int position = 0) =>
        GhUi.Read(run: _scope =>
            from validFirst in Optional(p1).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: "null first panel"))
            from validSecond in Optional(p2).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Split)), detail: "null second panel"))
            from built in Op.Of(name: nameof(Split)).Attempt(body: () => {
                // BOUNDARY ADAPTER - positive position seeds Eto Splitter.Position.
                Splitter splitter = new() { Panel1 = validFirst, Panel2 = validSecond, Orientation = orientation };
                _ = Optional(position).Filter(static p => p > 0).Iter(p => splitter.Position = p);
                return (Control)splitter;
            }, what: "new Splitter")
            select built);

    private static Fin<Clipboard> NeedClipboard(Op op) =>
        Optional(Eto.Forms.Clipboard.Instance).ToFin(Fail: UiFault.MutationRejected(op: op, detail: "Eto Clipboard.Instance is not initialized"));

    private static Fin<Seq<string>> ClipboardDataTypes(Op op, Seq<string> types) =>
        types.TraverseM(op.AcceptText).As();

    private static Fin<Seq<InputClipboardDataEntry>> ClipboardPayloadData(Op op, Seq<InputClipboardDataEntry> data) =>
        data.TraverseM(entry =>
            from type in op.AcceptText(value: entry.Type)
            from bytes in entry.Bytes.ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"clipboard data payload for {type} is required"))
            select new InputClipboardDataEntry(Type: type, Bytes: Some(bytes))).As();

    private static InputClipboardSnapshot ClipboardSnapshotOf(Clipboard clipboard, Seq<string> dataTypes = default) =>
        new(
            Text: Optional(clipboard.Text).Filter(_ => clipboard.ContainsText),
            Html: Optional(clipboard.Html).Filter(_ => clipboard.ContainsHtml),
            Image: Optional(clipboard.Image).Filter(_ => clipboard.ContainsImage),
            Uris: clipboard.ContainsUris ? toSeq(clipboard.Uris ?? []) : Seq<Uri>(),
            Types: toSeq(clipboard.Types ?? []),
            Data: dataTypes.Bind(type => clipboard.Contains(type: type)
                ? Seq(new InputClipboardDataEntry(Type: type, Bytes: Optional(clipboard.GetData(type: type))))
                : Seq<InputClipboardDataEntry>()));

    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static InputModifierSnapshot ModifierOf(Keys keys) =>
        new(Shift: keys.HasShift(), Command: keys.HasCommand(), Option: keys.HasOption());

    private static Control? DialogParent(GrasshopperUi.Scope scope) =>
        scope.Editor.Map(static _ => (Control?)Grasshopper2.UI.Editor.ThisOrRhino).IfNone(() =>
            scope.Canvas.Map(static c => (Control?)c.ControlObject)
                .IfNone(() => (Control?)(Rhino.UI.RhinoEtoApp.MainWindowForOwner ?? Rhino.UI.RhinoEtoApp.MainWindow)));

    internal static PathDialogSnapshot RunFileDialog(FileDialog dialog, Control? parent, PathDialogSpec spec) {
        using FileDialog owned = dialog;
        _ = spec.InitialPath.Filter(static path => !string.IsNullOrWhiteSpace(path))
            .IfSome(path => {
                string fullPath = System.IO.Path.GetFullPath(path: path);
                string directory = Directory.Exists(path: fullPath) ? fullPath : System.IO.Path.GetDirectoryName(path: fullPath) ?? string.Empty;
                _ = Optional(directory)
                    .Filter(static resolved => !string.IsNullOrWhiteSpace(resolved))
                    .IfSome(resolved => owned.Directory = new Uri(uriString: resolved));
                _ = Optional(System.IO.Path.GetFileName(path: fullPath))
                    .Filter(file => !string.IsNullOrWhiteSpace(value: file) && !Directory.Exists(path: fullPath))
                    .IfSome(file => owned.FileName = file);
            });
        _ = spec.Title.IfSome(value => owned.Title = value);
        _ = spec.CheckFileExists.IfSome(value => owned.CheckFileExists = value);
        _ = toSeq(spec.Filters).Iter(f => owned.Filters.Add(item: new Eto.Forms.FileFilter(name: f.Name, extensions: [.. f.Extensions])));
        _ = spec.FilterIndex.IfSome(index => owned.CurrentFilterIndex = index);
        DialogResult result = owned.ShowDialog(parent: parent);
        Seq<string> paths = result switch {
            DialogResult.Ok when owned is OpenFileDialog { MultiSelect: true } open => toSeq(open.Filenames),
            DialogResult.Ok => Seq(owned.FileName),
            _ => Seq<string>(),
        };
        return new PathDialogSnapshot(
            Paths: paths,
            FilterIndex: owned.CurrentFilterIndex,
            FilterName: owned.CurrentFilterIndex >= 0
                ? toSeq(spec.Filters).Skip(owned.CurrentFilterIndex).Head.Map(static f => f.Name)
                : Option<string>.None,
            Accepted: result == DialogResult.Ok);
    }

    // SelectFolderDialog derives from CommonDialog, not FileDialog, so folder mode dispatches separately.
    internal static PathDialogSnapshot RunFolderDialog(Control? parent, PathDialogSpec spec) {
        using SelectFolderDialog dialog = new();
        _ = spec.InitialPath.Filter(static path => !string.IsNullOrWhiteSpace(path)).IfSome(path => {
            string fullPath = System.IO.Path.GetFullPath(path: path);
            string directory = Directory.Exists(path: fullPath) ? fullPath : System.IO.Path.GetDirectoryName(path: fullPath) ?? fullPath;
            dialog.Directory = directory;
        });
        _ = spec.Title.IfSome(value => dialog.Title = value);
        DialogResult result = dialog.ShowDialog(parent: parent);
        return result switch {
            DialogResult.Ok => new PathDialogSnapshot(Paths: Seq(dialog.Directory), FilterIndex: -1, FilterName: Option<string>.None, Accepted: true),
            _ => PathDialogSnapshot.Cancelled,
        };
    }
}
