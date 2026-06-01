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
// Library mirror of Grasshopper2.Extensions.SelectionMode — the single canonical selection-modifier
// vocabulary; each item carries its host value so the GH2<->library mapping is item-owned projection
// (Gh / FromGh), centralizing the Inverse fallback in FromGh. Public callers and
// scenarios never name the host enum.
[SmartEnum<int>]
public sealed partial class SelectionMode {
    public GhSelectionMode Gh { get; }
    public static readonly SelectionMode Promote = new(key: 0, gh: GhSelectionMode.Promote);
    public static readonly SelectionMode Include = new(key: 1, gh: GhSelectionMode.Include);
    public static readonly SelectionMode Exclude = new(key: 2, gh: GhSelectionMode.Exclude);
    public static readonly SelectionMode Inverse = new(key: 3, gh: GhSelectionMode.Inverse);
    internal static SelectionMode FromGh(GhSelectionMode gh) => toSeq(Items).Find(m => m.Gh == gh).IfNone(Inverse);
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
    // Wire cursors prefer the host's static override (Canvas.CursorWireIn/Out/Question) and fall back to
    // a stock cursor — folded into the item delegate so Resolve is a single uniform Cursor() call.
    public static readonly CursorKind WireIn = new(key: 9, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorWireIn).IfNone(Cursors.Pointer));
    public static readonly CursorKind WireOut = new(key: 10, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorWireOut).IfNone(Cursors.Pointer));
    public static readonly CursorKind WireQuestion = new(key: 11, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorQuestion).IfNone(Cursors.Default));
    // App-wide busy: the Eto cursor stays Default; CursorScope additionally engages a Rhino.UI.WaitCursor
    // (the platform spinner) for this kind, restored when the scope disposes.
    public static readonly CursorKind Wait = new(key: 20, cursor: static _ => Cursors.Default);

    [UseDelegateFromConstructor]
    internal partial Cursor Cursor(Grasshopper2.UI.Canvas.Canvas canvas);

    internal Fin<Cursor> Resolve(Grasshopper2.UI.Canvas.Canvas canvas) => Fin.Succ(Cursor(canvas: canvas));
}

[SmartEnum<int>]
public sealed partial class DialogPresentation {
    public static readonly DialogPresentation Modal = new(key: 0);
    public static readonly DialogPresentation AttachedSheet = new(key: 1);

    // Exactly two presentations: Modal shows directly, AttachedSheet switches DisplayMode first.
    internal Option<TResult> Show<TResult>(Dialog<Option<TResult>> dialog, Control? parent) =>
        Switch(
            (Dialog: dialog, Parent: parent),
            modal: static s => s.Dialog.ShowModal(owner: s.Parent),
            attachedSheet: static s => {
                s.Dialog.DisplayMode = DialogDisplayMode.Attached;
                return s.Dialog.ShowModal(owner: s.Parent);
            });
}

[SmartEnum<int>]
public sealed partial class PanelBehavior {
    public static readonly PanelBehavior Modal = new(key: 0);
    public static readonly PanelBehavior Attached = new(key: 1);
    public static readonly PanelBehavior Floating = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class MaterialPolicy {
    public static readonly MaterialPolicy Default = new(key: 0);
    public static readonly MaterialPolicy Content = new(key: 1);
    public static readonly MaterialPolicy Hud = new(key: 2);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct PresentationPolicy(Option<PanelBehavior> Behavior = default, Option<MaterialPolicy> Material = default) {
    internal DialogPresentation DialogPresentation =>
        Behavior.Filter(static behavior => behavior == PanelBehavior.Attached).Map(static _ => DialogPresentation.AttachedSheet).IfNone(DialogPresentation.Modal);
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

    // Coalescing merge: two Writes union their payload fields (Option choice / Seq concat), a Clear absorbs
    // any peer (the clipboard is wiped), two Reads fold their requested DataTypes, and a mixed Write/Read
    // keeps the Write (the mutating intent dominates a query).
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

    // Field-wise coalesce: scalar fields take the right operand's value when present (Option choice), the
    // collection fields concatenate. Powers the InputClipboardOp Write+Write merge.
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
    public sealed record DragSourceCase(TransferPayload Payload) : InputTransferOp;
    public sealed record DropPolicyCase(Seq<string> DataTypes, DragEffects Effects) : InputTransferOp;
    public static InputTransferOp Clipboard(InputClipboardOp op) => new ClipboardCase(Op: op);
    public static InputTransferOp DragSource(TransferPayload payload) => new DragSourceCase(Payload: payload);
    public static InputTransferOp DropPolicy(Seq<string> dataTypes, DragEffects effects = DragEffects.Copy) => new DropPolicyCase(DataTypes: dataTypes, Effects: effects);
}

public abstract record FormField(string Key, string Label) {
    public sealed record Text(string Name, string Caption, string Value = "") : FormField(Key: Name, Label: Caption);
    public sealed record TextArea(string Name, string Caption, string Value = "") : FormField(Key: Name, Label: Caption);
    public sealed record Toggle(string Name, string Caption, bool Value = false) : FormField(Key: Name, Label: Caption);
    public sealed record Number(string Name, string Caption, double Value = 0d) : FormField(Key: Name, Label: Caption);
    public sealed record Choice(string Name, string Caption, Seq<string> Options, int Selected = 0) : FormField(Key: Name, Label: Caption);

    public static FormField TextInput(string key, string label, string value = "") => new Text(Name: key, Caption: label, Value: value);
    public static FormField TextAreaInput(string key, string label, string value = "") => new TextArea(Name: key, Caption: label, Value: value);
    public static FormField Check(string key, string label, bool value = false) => new Toggle(Name: key, Caption: label, Value: value);
    public static FormField Numeric(string key, string label, double value = 0d) => new Number(Name: key, Caption: label, Value: value);
    public static FormField Select(string key, string label, Seq<string> options, int selected = 0) => new Choice(Name: key, Caption: label, Options: options, Selected: selected);
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

    // Bar.AddPushButton / AddRadioToggle take a nullable BarShortcut; project the Option to that boundary shape.
    internal BarShortcut? ShortcutKey() => Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null);

    // icon.DrawToBitmap rasterizes a GH2 IIcon whose embedded resource can be unresolved under programmatic plugin
    // load (the same null-icon class that crashed the editor StatusBar) — guard both the throw and a null bitmap so a
    // missing icon degrades to "no menu image" instead of propagating out of command construction.
    private const int MenuIconExtent = 16;
    internal Option<Image> MenuImage =>
        Image | Icon.Bind(static icon =>
            Op.Of(name: nameof(MenuImage))
                .Attempt(body: () => icon.DrawToBitmap(size: new Size(width: MenuIconExtent, height: MenuIconExtent), padding: 0, background: Colors.Transparent), what: "icon.DrawToBitmap")
                .Map(bitmap => Optional<Image>(bitmap))
                .IfFail(Option<Image>.None));
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
    public sealed partial record ToggleCase(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SectionToggleCase(string Name, bool State, Seq<string> Sections = default) : ToolbarItem;
    public sealed partial record RadioCase(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record TextInputCase(string Name, string Value, Func<string, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record NumberCase(string Name, UiNumber Value, Func<decimal, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SwatchInputCase(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record ColourBarsCase(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SpacerCase(string Chapter, string Section) : ToolbarItem;
    public sealed partial record LabelCase(string Caption) : ToolbarItem;
    public sealed partial record CheckCase(string Name, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record TextCase(string Name, string Value, Func<string, Fin<Unit>> Changed) : ToolbarItem;
    public sealed partial record SpectrumCase(string Name, Seq<OpenColor.Family> Palette, OpenColor.Family Initial, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;

    public static ToolbarItem Button(UiCommand command, bool closeOnActivate = true) => new ButtonCase(Command: command, CloseOnActivate: closeOnActivate);
    public static ToolbarItem Toggle(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new ToggleCase(Command: command, State: state, Changed: changed);
    public static ToolbarItem SectionToggle(string name, bool state, Seq<string> sections = default) => new SectionToggleCase(Name: name, State: state, Sections: sections);
    public static ToolbarItem Radio(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new RadioCase(Command: command, State: state, Changed: changed);
    public static ToolbarItem TextInput(string name, string value, Func<string, Fin<Unit>> changed) => new TextInputCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem Number(string name, UiNumber value, Func<decimal, Fin<Unit>> changed) => new NumberCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem SwatchInput(string name, OpenColor.Family family, Func<OpenColor.Family, Fin<Unit>> changed) => new SwatchInputCase(Name: name, Family: family, Changed: changed);
    public static ToolbarItem ColourBars(string name, OpenColor.Family family, Func<OpenColor.Family, Fin<Unit>> changed) => new ColourBarsCase(Name: name, Family: family, Changed: changed);
    public static ToolbarItem Spacer(string chapter, string section) => new SpacerCase(Chapter: chapter, Section: section);
    public static ToolbarItem Label(string text) => new LabelCase(Caption: text);
    public static ToolbarItem Check(string name, bool state, Func<bool, Fin<Unit>> changed) => new CheckCase(Name: name, State: state, Changed: changed);
    public static ToolbarItem Text(string name, string value, Func<string, Fin<Unit>> changed) => new TextCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem Spectrum(string name, OpenColor.Family[] spectrum, OpenColor.Family initial, Func<OpenColor.Family, Fin<Unit>> changed) => new SpectrumCase(Name: name, Palette: toSeq(spectrum), Initial: initial, Changed: changed);

    // Label/Check/Text are InputPanel-native widgets (no Bar/ContextMenu equivalent); they route to the
    // InputPanel surface. Bar widgets (incl. Spectrum) route to the embedded bar.
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
        toggleCase: static (bar, item) => ProjectRadioToggle(
            bar: bar,
            command: item.Command,
            state: item.State,
            optional: true,
            changed: item.Changed,
            op: ToggleCase.SelfOp,
            detail: "toggle change delegate is required"),
        sectionToggleCase: static (bar, item) => SectionToggleCase.SelfOp.Attempt(body: () => {
            _ = bar.AddToggle(nomen: new Nomen(name: item.Name, info: item.Name), initialState: item.State, additionalAffectedSections: [.. item.Sections]);
            return unit;
        }, what: "AddToggle"),
        radioCase: static (bar, item) => ProjectRadioToggle(
            bar: bar,
            command: item.Command,
            state: item.State,
            optional: false,
            changed: item.Changed,
            op: RadioCase.SelfOp,
            detail: "radio change delegate is required"),
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
        textCase: static (_, _) => Reject(item: nameof(TextCase), surface: "a toolbar"));

    // Restrictive surface: only Button/Toggle/Spacer project to a context menu; every other case Rejects via the
    // exhaustive generated Switch, the correct conservative default for a menu.
    private Fin<Unit> ProjectMenu(ContextMenu menu) => Switch(
        state: menu,
        buttonCase: static (menu, item) => ButtonCase.SelfOp.Attempt(body: () => PushMenuButton(menu: menu, command: item.Command), what: "menu button"),
        toggleCase: static (menu, item) =>
            from changed in ToggleCase.SelfOp.NeedChanged(item.Changed, noun: "toggle")
            from added in ToggleCase.SelfOp.Attempt(body: () => PushCheckMenuItem(menu: menu, command: item.Command, state: item.State, changed: changed), what: "menu toggle")
            select added,
        spacerCase: static (menu, _) => SpacerCase.SelfOp.Attempt(body: menu.AddSeparator, what: "AddSeparator"),
        sectionToggleCase: static (_, _) => Reject(item: nameof(SectionToggleCase), surface: "a context menu"),
        radioCase: static (_, _) => Reject(item: nameof(RadioCase), surface: "a context menu"),
        textInputCase: static (_, _) => Reject(item: nameof(TextInputCase), surface: "a context menu"),
        numberCase: static (_, _) => Reject(item: nameof(NumberCase), surface: "a context menu"),
        swatchInputCase: static (_, _) => Reject(item: nameof(SwatchInputCase), surface: "a context menu"),
        colourBarsCase: static (_, _) => Reject(item: nameof(ColourBarsCase), surface: "a context menu"),
        labelCase: static (_, _) => Reject(item: nameof(LabelCase), surface: "a context menu"),
        checkCase: static (_, _) => Reject(item: nameof(CheckCase), surface: "a context menu"),
        textCase: static (_, _) => Reject(item: nameof(TextCase), surface: "a context menu"),
        spectrumCase: static (_, _) => Reject(item: nameof(SpectrumCase), surface: "a context menu"));

    // Inlined former UiCommand.ToEtoCommand/ToMenuItem — the sole call site was this menu button; co-located with
    // PushCheckMenuItem so the menu surface owns both its button and toggle construction.
    private static Unit PushMenuButton(ContextMenu menu, UiCommand command) {
        Func<Fin<Unit>> run = command.Run;
        Command etoCommand = new() { ID = command.Name, MenuText = command.Name, ToolBarText = command.Name, ToolTip = command.Info, Enabled = command.EffectiveEnabled() };
        _ = command.MenuImage.Iter(image => etoCommand.Image = image);
        _ = command.Shortcut.Iter(shortcut => etoCommand.Shortcut = shortcut.Keys);
        etoCommand.Executed += (_, _) => _ = GrasshopperUi.Handler(valid: run);
        MenuItem menuItem = etoCommand.CreateMenuItem();
        _ = command.CanExecute.Iter(can => menuItem.Validate += (_, _) =>
            menuItem.Enabled = command.Enabled && GrasshopperUi.Protect(valid: can).IfFail(_ => false));
        menu.Items.Add(item: menuItem);
        return unit;
    }

    private static Unit PushCheckMenuItem(ContextMenu menu, UiCommand command, bool state, Func<bool, Fin<Unit>> changed) {
        CheckCommand menuCommand = new() {
            ID = command.Name,
            MenuText = command.Name,
            ToolTip = command.Info,
            Enabled = command.EffectiveEnabled(),
            Checked = state,
        };
        _ = command.MenuImage.Iter(image => menuCommand.Image = image);
        _ = command.Shortcut.Iter(shortcut => menuCommand.Shortcut = shortcut.Keys);
        menuCommand.Executed += (_, _) => _ = GrasshopperUi.Handler(valid: () => changed(arg: menuCommand.Checked));
        MenuItem menuItem = menuCommand.CreateMenuItem();
        _ = command.CanExecute.Iter(can => menuItem.Validate += (_, _) =>
            menuItem.Enabled = command.Enabled && GrasshopperUi.Protect(valid: can).IfFail(_ => false));
        menu.Items.Add(item: menuItem);
        return unit;
    }

    private static Fin<Unit> ProjectRadioToggle(Bar bar, UiCommand command, bool state, bool optional, Func<bool, Fin<Unit>> changed, Op op, string detail) =>
        from validChanged in Optional(changed).ToFin(Fail: UiFault.InvalidInput(op: op, detail: detail))
        select AddRadioToggle(bar: bar, command: command, state: state, optional: optional, changed: validChanged);

    // Unified projection rejection across all three surfaces; surface is the human phrase ("a toolbar" /
    // "a context menu" / "an input panel").
    private static Fin<Unit> Reject(string item, string surface) =>
        Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: $"{item} cannot be projected to {surface}"));

    // Restrictive surface: only the panel-native widgets (Label/Check/Text) project here; bar widgets route
    // through the embedded bar (Input.Panel(CommandPlan)) and every other case Rejects via the generated Switch.
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
        toggleCase: static (_, _) => Reject(item: nameof(ToggleCase), surface: "an input panel"),
        sectionToggleCase: static (_, _) => Reject(item: nameof(SectionToggleCase), surface: "an input panel"),
        radioCase: static (_, _) => Reject(item: nameof(RadioCase), surface: "an input panel"),
        textInputCase: static (_, _) => Reject(item: nameof(TextInputCase), surface: "an input panel"),
        numberCase: static (_, _) => Reject(item: nameof(NumberCase), surface: "an input panel"),
        swatchInputCase: static (_, _) => Reject(item: nameof(SwatchInputCase), surface: "an input panel"),
        colourBarsCase: static (_, _) => Reject(item: nameof(ColourBarsCase), surface: "an input panel"),
        spacerCase: static (_, _) => Reject(item: nameof(SpacerCase), surface: "an input panel"),
        spectrumCase: static (_, _) => Reject(item: nameof(SpectrumCase), surface: "an input panel"));

    private static Unit AddRadioToggle(Bar bar, UiCommand command, bool state, bool optional, Func<bool, Fin<Unit>> changed) {
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

public sealed record InputRequest<T> : GhUiRequest<T> { internal InputRequest(GrasshopperUiPolicy policy, Func<GrasshopperUi.Scope, Fin<T>> run) : base(policy: policy, run: run) { } }

public static class InputRequest {
    public static InputRequest<InputSelectionSnapshot> Selection(InputSelectionSource source) => Read(run: scope => Input.Selection(source: source).Run(scope: scope));
    public static InputRequest<InputModifierSnapshot> Modifiers(Keys keys) => Read(run: scope => Input.Modifiers(keys: keys).Run(scope: scope));
    public static InputRequest<InputModifierSnapshot> ModifierState() => Read(run: static scope => Input.ModifierState().Run(scope: scope));
    public static InputRequest<PointerSnapshot> PointerState() => Read(run: static scope => Input.PointerState().Run(scope: scope));
    public static InputRequest<InputPanelSnapshot> Panel(Func<InputPanel, Fin<Unit>> populate) => Read(run: scope => Input.Panel(populate: populate).Run(scope: scope));
    public static InputRequest<Subscription> Popup(Control owner, PointF location, RectangleF screen, Func<InputPanel, Fin<Unit>> populate) => Read(run: scope => Input.PopupPanel(owner: owner, location: location, screen: screen, populate: populate).Run(scope: scope));
    public static InputRequest<ToolbarSnapshot> CommandBar(CommandPlan plan) => Read(run: scope => Input.Toolbar(plan: plan).Run(scope: scope));
    public static InputRequest<InputPanelSnapshot> CommandPanel(CommandPlan plan) => Read(run: scope => Input.Panel(plan: plan).Run(scope: scope));
    public static InputRequest<MenuSnapshot> MenuShow(CommandPlan plan, Control owner, PointF location) => Read(run: scope => Input.ShowMenu(plan: plan, owner: owner, location: location).Run(scope: scope));
    public static InputRequest<CursorKind> Cursor(CursorKind kind) => Of(policy: GrasshopperUiPolicy.Canvas(), run: scope => Input.Cursor(kind: kind).Run(scope: scope));
    public static InputRequest<IDisposable> CursorScope(CursorKind kind) => Of(policy: GrasshopperUiPolicy.Canvas(), run: scope => Input.CursorScope(kind: kind).Run(scope: scope));
    public static InputRequest<DialogResult> Message(string title, string body, MessageBoxType kind = MessageBoxType.Information, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxDefaultButton @default = MessageBoxDefaultButton.Default) => Read(run: scope => Input.MessageDialog(title: title, message: body, kind: kind, buttons: buttons, defaultButton: @default).Run(scope: scope));
    public static InputRequest<PathDialogSnapshot> File(PathDialogSpec spec) => Read(run: scope => Input.FileDialog(spec: spec).Run(scope: scope));
    public static InputRequest<InputClipboardSnapshot> Clipboard(InputClipboardOp op) => Read(run: scope => Input.Clipboard(op: op).Run(scope: scope));
    public static InputRequest<Option<TResult>> Dialog<TResult>(Func<Eto.Forms.Dialog<Option<TResult>>, Fin<Unit>> configure, string title = "", Option<DialogPresentation> presentation = default) => Read(run: scope => Input.Dialog(configure: configure, title: title, presentation: presentation).Run(scope: scope));
    public static InputRequest<Option<Color>> PickColor(Color initial, bool allowAlpha = true) => Read(run: scope => Input.PickColor(initial: initial, allowAlpha: allowAlpha).Run(scope: scope));
    public static InputRequest<Option<Font>> PickFont(Option<Font> initial = default) => Read(run: scope => Input.PickFont(initial: initial).Run(scope: scope));
    public static InputRequest<Unit> Notify(string title, string body, Option<Image> contentImage = default, bool trayIndicator = false) => Read(run: scope => Input.Notify(title: title, message: body, contentImage: contentImage, trayIndicator: trayIndicator).Run(scope: scope));
    public static InputRequest<Option<string>> EditPrompt(string title, string body, string @default = "", bool multiline = false) => Read(run: scope => Input.EditPrompt(title: title, message: body, defaultText: @default, multiline: multiline).Run(scope: scope));
    public static InputRequest<Option<double>> NumberPrompt(string title, string body, double @default = 0d, UiNumberRange range = default) => Read(run: scope => Input.NumberPrompt(title: title, message: body, initial: @default, range: range).Run(scope: scope));
    public static InputRequest<Option<TResult>> Form<TResult>(FormPlan<TResult> plan) => Read(run: scope => Input.Form(plan: plan).Run(scope: scope));
    public static InputRequest<TransferPayload> Transfer(InputTransferOp op) => Read(run: scope => Input.Transfer(op: op).Run(scope: scope));

    private static InputRequest<T> Read<T>(Func<GrasshopperUi.Scope, Fin<T>> run) => Of(policy: GrasshopperUiPolicy.Read, run: run);
    private static InputRequest<T> Of<T>(GrasshopperUiPolicy policy, Func<GrasshopperUi.Scope, Fin<T>> run) => new(policy: policy, run: run);
}

// Scope-restoring cursor capsule; disposing reinstates the canvas's prior Cursor. Use via
// `using IDisposable _ = await GhUi.Input(InputRequest.CursorScope(...)).Use(...);`
// or directly through `Input.CursorScope(kind)` when the surrounding scope is already typed.
internal sealed class ScopedCursor(Grasshopper2.UI.Canvas.Canvas target, Cursor previous, IDisposable? busy = null) : IDisposable {
    // BOUNDARY ADAPTER — restore the canvas cursor and release any engaged busy-spinner exactly once.
    public void Dispose() {
        busy?.Dispose();
        target.Cursor = previous;
    }
}

file static class WaitCursorLease {
    private static readonly Atom<(int Count, Rhino.UI.WaitCursor? Cursor)> State = Atom<(int Count, Rhino.UI.WaitCursor? Cursor)>(value: (Count: 0, Cursor: null));

    internal static IDisposable Enter() {
        // BOUNDARY ADAPTER — engage the platform spinner exactly once for the first concurrent lease; a candidate
        // cursor is allocated up front (Swap may retry under CAS contention) and disposed when an incumbent wins.
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
            // BOUNDARY ADAPTER — one-shot guard so a doubled Dispose cannot underflow the lease count.
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
internal static partial class Input {
    internal static GrasshopperUiIntent<InputSelectionSnapshot> Selection(InputSelectionSource source) =>
        GhUi.Read(run: _ =>
            Optional(source)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "null source"))
                .Bind(valid =>
                    from mode in valid.Mode()
                    select new InputSelectionSnapshot(Mode: mode, Modifiers: ModifierOf(keys: Keyboard.Modifiers))));

    internal static GrasshopperUiIntent<InputModifierSnapshot> Modifiers(Keys keys) =>
        GhUi.Read(run: _ => Fin.Succ(value: ModifierOf(keys: keys)));

    internal static GrasshopperUiIntent<InputModifierSnapshot> ModifierState() =>
        GhUi.Read(run: _ => Fin.Succ(value: ModifierOf(keys: Keyboard.Modifiers)));

    internal static GrasshopperUiIntent<PointerSnapshot> PointerState() =>
        GhUi.Read(run: _ =>
            Mouse.IsSupported
                ? Fin.Succ(value: new PointerSnapshot(ScreenPosition: Mouse.Position, Buttons: Mouse.Buttons))
                : Fin.Fail<PointerSnapshot>(error: UiFault.MutationRejected(op: Op.Of(name: nameof(PointerState)), detail: "mouse position is not supported on this platform")));

    internal static GrasshopperUiIntent<InputPanelSnapshot> Panel(Func<InputPanel, Fin<Unit>> populate) =>
        GhUi.Read(run: _ =>
            Optional(populate)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Panel)), detail: "null populate"))
                .Bind(valid => {
                    InputPanel panel = new();
                    return valid(arg: panel).Map(_ => new InputPanelSnapshot(
                        Count: panel.Count,
                        Category: panel.Category ?? string.Empty));
                }));

    internal static GrasshopperUiIntent<InputPanelSnapshot> Panel(CommandPlan plan) =>
        Panel(populate: panel => Try.lift(f: () => panel.AddBar(drawCategoryLabels: false)).Run()
            .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Panel)), detail: $"InputPanel.AddBar threw: {error.Message}"))
            .Bind(bar =>
                from native in plan.Items.Filter(static item => item.IsPanelNative).TraverseM(item => item.Apply(surface: UiCommandSurface.InputPanel(panel: panel))).As()
                from barred in plan.Items.Filter(static item => !item.IsPanelNative).TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).As()
                select unit));

    // Returns a dismissable Subscription whose detach closes the popup form (form captured in a closure cell,
    // mirroring WireShapeInstall.Push) so the popup shares every other chrome op's Subscription lifetime.
    internal static GrasshopperUiIntent<Subscription> PopupPanel(Control owner, PointF location, RectangleF screen, Func<InputPanel, Fin<Unit>> populate) =>
        GhUi.Read(run: _ =>
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null owner"))
            from validPopulate in Optional(populate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null populate"))
            from validLocation in Op.Of(name: nameof(PopupPanel)).AcceptPoint(value: location, detail: "non-finite location")
            from validScreen in Op.Of(name: nameof(PopupPanel)).AcceptRect(value: screen, detail: "invalid screen bounds", requirePositive: true)
            let clamped = new PointF(
                x: Math.Clamp(value: validLocation.X, min: validScreen.Left, max: validScreen.Right),
                y: Math.Clamp(value: validLocation.Y, min: validScreen.Top, max: validScreen.Bottom))
            let panel = new InputPanel()
            from populated in validPopulate(arg: panel)
            from sub in PopupSubscription(panel: panel, owner: validOwner, location: clamped, screen: validScreen)
            select sub);

    private static Fin<Subscription> PopupSubscription(InputPanel panel, Control owner, PointF location, RectangleF screen) {
        Form? form = null;
        return Subscription.Bind(
            attach: () => form = panel.ShowAsForm(owner, location, screen),
            // BOUNDARY ADAPTER — Form.Close alone does not release the macOS NSWindow; Dispose it after closing.
            detach: () => {
                form?.Close();
                form?.Dispose();
            },
            marshalToUi: true,
            detachOnce: true);
    }

    internal static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(Func<Bar, Fin<Unit>> populate) =>
        GhUi.Read(run: _ =>
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

    internal static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(CommandPlan plan) =>
        Toolbar(populate: bar =>
            plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).Map(static _ => unit).As());

    internal static GrasshopperUiIntent<MenuSnapshot> ShowMenu(CommandPlan plan, Control owner, PointF location) =>
        GhUi.Read(run: _ =>
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ShowMenu)), detail: "null owner"))
            from validLocation in Op.Of(name: nameof(ShowMenu)).AcceptPoint(value: location, detail: "non-finite location")
            let menu = new ContextMenu()
            from populated in plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Menu(menu: menu))).As()
            from shown in Try.lift(f: () => {
                menu.Show(validOwner, validLocation);
                return new MenuSnapshot(Count: menu.Items.Count);
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ShowMenu)), detail: error.Message))
            select shown);

    internal static GrasshopperUiIntent<CursorKind> Cursor(CursorKind kind) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(canvas =>
            kind.Resolve(canvas: canvas).Map(cursor => {
                canvas.Cursor = cursor;
                return kind;
            })));

    internal static GrasshopperUiIntent<IDisposable> CursorScope(CursorKind kind) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Bind(canvas =>
            kind.Resolve(canvas: canvas).Map(cursor => {
                Cursor previous = canvas.Cursor;
                canvas.Cursor = cursor;
                IDisposable? busy = kind == CursorKind.Wait ? WaitCursorLease.Enter() : null;
                return (IDisposable)new ScopedCursor(target: canvas, previous: previous, busy: busy);
            })));

    internal static GrasshopperUiIntent<DialogResult> MessageDialog(string title, string message, MessageBoxType kind = MessageBoxType.Information, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Default) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => MessageBox.Show(
                parent: DialogParent(scope: scope),
                text: message,
                caption: title,
                buttons: buttons,
                type: kind,
                defaultButton: defaultButton))
            .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(MessageDialog)), detail: $"MessageBox.Show threw: {error.Message}")));

    internal static GrasshopperUiIntent<PathDialogSnapshot> FileDialog(PathDialogSpec spec) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => spec.Mode.Run(parent: DialogParent(scope: scope), spec: spec))
                .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(FileDialog)), detail: $"FileDialog.ShowDialog threw: {error.Message}")));

    internal static GrasshopperUiIntent<InputClipboardSnapshot> Clipboard(InputClipboardOp op) =>
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

    internal static GrasshopperUiIntent<TransferPayload> Transfer(InputTransferOp op) =>
        GhUi.Read(run: scope =>
            Optional(op)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Transfer)), detail: "transfer op is required"))
                .Bind(valid => valid.Switch(
                    clipboardCase: c => Clipboard(op: c.Op).Run(scope: scope).Map(TransferPayload.Of),
                    dragSourceCase: static d => Fin.Succ(d.Payload),
                    dropPolicyCase: d => Clipboard(op: new InputClipboardOp.ReadCase(DataTypes: d.DataTypes)).Run(scope: scope)
                        .Map(clipboard => new TransferPayload(Clipboard: clipboard, Effects: Some(d.Effects), AllowedEffects: Some(d.Effects))))));

    internal static GrasshopperUiIntent<Option<TResult>> Dialog<TResult>(Func<Dialog<Option<TResult>>, Fin<Unit>> configure, string title, Option<DialogPresentation> presentation) =>
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

    internal static GrasshopperUiIntent<Option<TResult>> Form<TResult>(FormPlan<TResult> plan) =>
        GhUi.Read(run: scope => RunForm(scope: scope, plan: plan));

    private static Fin<Option<TResult>> RunForm<TResult>(GrasshopperUi.Scope scope, FormPlan<TResult> plan) =>
        Optional(plan.Submit)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Form)), detail: "form submit delegate is required"))
            .Bind(submit => Try.lift<Fin<Option<TResult>>>(f: () => {
                using Dialog<Option<TResult>> dialog = new() { Title = plan.Title };
                DynamicLayout layout = new() { Padding = new Padding(all: 12), Spacing = new Size(width: 8, height: 8) };
                Seq<(FormField Field, Control Control)> controls = plan.Fields.Map(field => (Field: field, Control: FormControl(field: field))).ToSeq();
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

    private static Control FormControl(FormField field) =>
        field switch {
            FormField.Text text => new TextBox { Text = text.Value },
            FormField.TextArea text => new TextArea { Text = text.Value, Size = new Size(width: 360, height: 120) },
            FormField.Toggle boolean => new CheckBox { Checked = boolean.Value },
            FormField.Number number => new TextBox { Text = string.Create(CultureInfo.InvariantCulture, $"{number.Value}") },
            FormField.Choice choice => ChoiceControl(choice: choice),
            _ => new TextBox(),
        };

    private static DropDown ChoiceControl(FormField.Choice choice) {
        DropDown control = new();
        _ = choice.Options.Iter(control.Items.Add);
        control.SelectedIndex = Math.Clamp(value: choice.Selected, min: 0, max: Math.Max(val1: control.Items.Count - 1, val2: 0));
        return control;
    }

    private static FormSnapshot CaptureForm(Seq<(FormField Field, Control Control)> controls) =>
        new(Values: controls.ToDictionary(
            keySelector: static entry => entry.Field.Key,
            elementSelector: static entry => FormValue(field: entry.Field, control: entry.Control),
            comparer: StringComparer.Ordinal));

    private static object FormValue(FormField field, Control control) =>
        (field, control) switch {
            (FormField.Text, TextBox value) => value.Text,
            (FormField.TextArea, TextArea value) => value.Text,
            (FormField.Toggle, CheckBox value) => value.Checked ?? false,
            (FormField.Number, TextBox value) => double.Parse(s: value.Text, provider: CultureInfo.InvariantCulture),
            (FormField.Choice choice, DropDown value) => choice.Options[value.SelectedIndex],
            _ => string.Empty,
        };

    internal static GrasshopperUiIntent<Option<Color>> PickColor(Color initial, bool allowAlpha) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => {
                using ColorDialog dialog = new() { Color = initial, AllowAlpha = allowAlpha };
                return dialog.ShowDialog(parent: DialogParent(scope: scope)) == DialogResult.Ok
                    ? Some(dialog.Color)
                    : Option<Color>.None;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(PickColor)), detail: $"ColorDialog threw: {error.Message}")));

    internal static GrasshopperUiIntent<Option<Font>> PickFont(Option<Font> initial) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => {
                using FontDialog dialog = new();
                _ = initial.Iter(f => dialog.Font = f);
                return dialog.ShowDialog(parent: DialogParent(scope: scope)) == DialogResult.Ok
                    ? Some(dialog.Font)
                    : Option<Font>.None;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(PickFont)), detail: $"FontDialog threw: {error.Message}")));

    internal static GrasshopperUiIntent<Option<string>> EditPrompt(string title, string message, string defaultText, bool multiline) =>
        GhUi.Read(run: _ =>
            Op.Of(name: nameof(EditPrompt)).Attempt(
                body: () => Rhino.UI.Dialogs.ShowEditBox(title: title, message: message, defaultText: defaultText, multiline: multiline, text: out string text)
                    ? Optional(text)
                    : Option<string>.None,
                what: "Dialogs.ShowEditBox"));

    internal static GrasshopperUiIntent<Option<double>> NumberPrompt(string title, string message, double initial, UiNumberRange range = default) =>
        GhUi.Read(run: _ =>
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

    internal static GrasshopperUiIntent<Unit> Notify(string title, string message, Option<Image> contentImage, bool trayIndicator) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => {
                using TrayIndicator? indicator = trayIndicator ? new TrayIndicator { Title = title, Visible = true } : null;
                using Notification notification = new() { Title = title, Message = message };
                _ = contentImage.Iter(image => notification.ContentImage = image);
                notification.Show(indicator: indicator);
                return unit;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Notify)), detail: $"Notification threw: {error.Message}")));

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

    // SelectFolderDialog : CommonDialog (NOT FileDialog) — separate dispatch (per Eto verification).
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
