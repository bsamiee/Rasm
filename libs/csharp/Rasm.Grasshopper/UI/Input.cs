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
// vocabulary, boundary-mapped at InputSelectionSource (GH2 -> library) and CanvasOp.WindowSelect
// (library -> GH2) so public callers and scenarios never name the host enum.
public enum SelectionMode { Promote, Include, Exclude, Inverse }
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
    public static readonly CursorKind NotAllowed = new(key: 8, cursor: static _ => Cursors.NotAllowed);
    // Wire cursors prefer the host's static override (Canvas.CursorWireIn/Out/Question) and fall back to
    // a stock cursor — folded into the item delegate so Resolve is a single uniform Cursor() call.
    public static readonly CursorKind WireIn = new(key: 9, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorWireIn).IfNone(Cursors.Pointer));
    public static readonly CursorKind WireOut = new(key: 10, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorWireOut).IfNone(Cursors.Pointer));
    public static readonly CursorKind WireQuestion = new(key: 11, cursor: static _ => Optional(Grasshopper2.UI.Canvas.Canvas.CursorQuestion).IfNone(Cursors.Default));

    [UseDelegateFromConstructor]
    internal partial Cursor Cursor(Grasshopper2.UI.Canvas.Canvas canvas);

    internal Fin<Cursor> Resolve(Grasshopper2.UI.Canvas.Canvas canvas) => Fin.Succ(Cursor(canvas: canvas));
}

[SmartEnum<int>]
public sealed partial class DialogPresentation {
    public static readonly DialogPresentation Modal = new(key: 0);
    public static readonly DialogPresentation AttachedSheet = new(key: 1);

    internal Option<TResult> Show<TResult>(Dialog<TResult> dialog, Control? parent) =>
        Key switch {
            var key when key == Modal.Key => Optional(dialog.ShowModal(owner: parent)),
            var key when key == AttachedSheet.Key => Optional(Attached(dialog: dialog, parent: parent)),
            _ => Option<TResult>.None,
        };

    private static TResult Attached<TResult>(Dialog<TResult> dialog, Control? parent) {
        dialog.DisplayMode = DialogDisplayMode.Attached;
        return dialog.ShowModal(owner: parent);
    }
}

[SmartEnum<int>]
public sealed partial class FileDialogMode {
    private delegate Seq<string> DialogRun(Control? parent, Option<string> initialPath, FileFilter[] filters, bool multiSelect, Option<string> title);

    public static readonly FileDialogMode Open = new(
        key: 0,
        run: static (parent, initialPath, filters, multi, title) =>
            Input.RunFileDialog(dialog: new OpenFileDialog { MultiSelect = multi }, parent: parent, initialPath: initialPath, filters: filters, title: title));
    public static readonly FileDialogMode Save = new(
        key: 1,
        run: static (parent, initialPath, filters, _, title) =>
            Input.RunFileDialog(dialog: new SaveFileDialog(), parent: parent, initialPath: initialPath, filters: filters, title: title));
    public static readonly FileDialogMode Folder = new(
        key: 2,
        run: static (parent, initialPath, _, _, title) =>
            Input.RunFolderDialog(parent: parent, initialPath: initialPath, title: title));

    [UseDelegateFromConstructor]
    internal partial Seq<string> Run(Control? parent, Option<string> initialPath, FileFilter[] filters, bool multiSelect, Option<string> title);
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

    internal SelectionMode Mode() => Switch(
        controlCase: static c => Map(c.Source.SelectionMode()),
        mouseCase: static m => Map(m.Source.SelectionMode()),
        windowCase: static w => Map(w.Source.SelectionMode()));

    private static SelectionMode Map(GhSelectionMode mode) =>
        mode switch {
            GhSelectionMode.Promote => SelectionMode.Promote,
            GhSelectionMode.Include => SelectionMode.Include,
            GhSelectionMode.Exclude => SelectionMode.Exclude,
            _ => SelectionMode.Inverse,
        };
}

[SkipUnionOps]
[Union]
public partial record InputClipboardOp {
    private InputClipboardOp() { }
    public sealed record ReadCase : InputClipboardOp;
    public sealed record WriteCase(InputClipboardSnapshot Payload) : InputClipboardOp;
    public sealed record ClearCase : InputClipboardOp;
    public static readonly InputClipboardOp Read = new ReadCase();
    public static InputClipboardOp Write(string text) => new WriteCase(Payload: InputClipboardSnapshot.Of(text: text));
    public static InputClipboardOp Write(InputClipboardSnapshot payload) => new WriteCase(Payload: payload);
    public static readonly InputClipboardOp Clear = new ClearCase();
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct InputModifierSnapshot(bool Shift, bool Command, bool Option);

[StructLayout(LayoutKind.Auto)]
public readonly record struct PointerSnapshot(PointF ScreenPosition, MouseButtons Buttons);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputSelectionSnapshot(SelectionMode Mode, InputModifierSnapshot Modifiers);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputPanelSnapshot(int Count, string Category, bool Shown);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ToolbarSnapshot(int Count, bool Enabled, float MinimumWidth, float MaximumWidth, float Height);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MenuSnapshot(int Count);

public readonly record struct InputClipboardSnapshot(Option<string> Text, Option<string> Html, Option<Image> Image, Seq<Uri> Uris, Seq<string> Types) {
    public static InputClipboardSnapshot Empty => new(
        Text: Option<string>.None,
        Html: Option<string>.None,
        Image: Option<Image>.None,
        Uris: Seq<Uri>(),
        Types: Seq<string>());

    public static InputClipboardSnapshot Of(string text) => Empty with { Text = Optional(text) };
}

public readonly record struct FileFilter(string Name, Seq<string> Extensions);

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

    internal Command ToEtoCommand() {
        string name = Name;
        string info = Info;
        Func<Fin<Unit>> run = Run;
        Command command = new() { ID = name, MenuText = name, ToolBarText = name, ToolTip = info, Enabled = EffectiveEnabled() };
        _ = MenuImage.Iter(image => command.Image = image);
        _ = Shortcut.Iter(shortcut => command.Shortcut = shortcut.Keys);
        command.Executed += (_, _) => _ = GrasshopperUi.Handler(valid: run);
        return command;
    }

    internal MenuItem ToMenuItem() {
        bool enabled = Enabled;
        Option<Func<Fin<bool>>> canExecute = CanExecute;
        Command command = ToEtoCommand();
        MenuItem item = command.CreateMenuItem();
        _ = canExecute.Iter(can => item.Validate += (_, _) =>
            item.Enabled = enabled && GrasshopperUi.Protect(valid: can).IfFail(_ => false));
        return item;
    }

    internal bool EffectiveEnabled() =>
        Enabled && CanExecute.Map(can => GrasshopperUi.Protect(valid: can).IfFail(_ => false)).IfNone(noneValue: true);

    internal Option<Image> MenuImage =>
        Image | Icon.Map(static icon => (Image)icon.DrawToBitmap(size: new Size(width: 16, height: 16), padding: 0, background: Colors.Transparent));
}

[SkipUnionOps]
[Union]
internal partial record UiCommandSurface {
    private UiCommandSurface() { }
    public sealed record ToolbarCase(Bar Bar) : UiCommandSurface;
    public sealed record MenuCase(ContextMenu Target) : UiCommandSurface;
    public static UiCommandSurface Toolbar(Bar bar) => new ToolbarCase(Bar: bar);
    public static UiCommandSurface Menu(ContextMenu menu) => new MenuCase(Target: menu);
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

    public static ToolbarItem Button(UiCommand command, bool closeOnActivate = true) => new ButtonCase(Command: command, CloseOnActivate: closeOnActivate);
    public static ToolbarItem Toggle(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new ToggleCase(Command: command, State: state, Changed: changed);
    public static ToolbarItem SectionToggle(string name, bool state, Seq<string> sections = default) => new SectionToggleCase(Name: name, State: state, Sections: sections);
    public static ToolbarItem Radio(UiCommand command, bool state, Func<bool, Fin<Unit>> changed) => new RadioCase(Command: command, State: state, Changed: changed);
    public static ToolbarItem TextInput(string name, string value, Func<string, Fin<Unit>> changed) => new TextInputCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem Number(string name, UiNumber value, Func<decimal, Fin<Unit>> changed) => new NumberCase(Name: name, Value: value, Changed: changed);
    public static ToolbarItem SwatchInput(string name, OpenColor.Family family, Func<OpenColor.Family, Fin<Unit>> changed) => new SwatchInputCase(Name: name, Family: family, Changed: changed);
    public static ToolbarItem ColourBars(string name, OpenColor.Family family, Func<OpenColor.Family, Fin<Unit>> changed) => new ColourBarsCase(Name: name, Family: family, Changed: changed);
    public static ToolbarItem Spacer(string chapter, string section) => new SpacerCase(Chapter: chapter, Section: section);

    internal Fin<Unit> Apply(UiCommandSurface surface) =>
        Optional(surface)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(UiCommandSurface)))
            .Bind(valid => valid.Switch(
                toolbarCase: toolbar => ProjectToolbar(bar: toolbar.Bar),
                menuCase: menu => ProjectMenu(menu: menu.Target)));

    private Fin<Unit> ProjectToolbar(Bar bar) => Switch(
        state: bar,
        buttonCase: static (bar, item) => Fin.Succ(item.Command).Map(command => {
            PushButton pushed = bar.AddPushButton(
                icon: command.Icon.IfNone(default(IIcon)!),
                nomen: new Nomen(name: command.Name, info: command.Info),
                callback: () => _ = GrasshopperUi.Handler(valid: command.Run),
                keys: command.Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null));
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
            from changed in Optional(item.Changed).ToFin(Fail: UiFault.InvalidInput(op: TextInputCase.SelfOp, detail: "text change delegate is required"))
            from added in TextInputCase.SelfOp.Attempt(body: () => {
                TextField field = bar.AddTextField(icon: default!, nomen: new Nomen(name: item.Name, info: item.Name), initial: item.Value, placeholder: item.Name);
                field.TextChanged += (_, value) => _ = GrasshopperUi.Handler(valid: () => changed(arg: value));
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
            from changed in Optional(item.Changed).ToFin(Fail: UiFault.InvalidInput(op: SwatchInputCase.SelfOp, detail: "colour change delegate is required"))
            from added in SwatchInputCase.SelfOp.Attempt(
                body: () => bar.AddLifeColours(nomen: new Nomen(name: item.Name, info: item.Name), initial: item.Family, assignment: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value))),
                what: "AddLifeColours")
            select added,
        colourBarsCase: static (bar, item) =>
            from changed in Optional(item.Changed).ToFin(Fail: UiFault.InvalidInput(op: ColourBarsCase.SelfOp, detail: "colour change delegate is required"))
            from added in ColourBarsCase.SelfOp.Attempt(body: () => {
                void Assign(OpenColor.Family value) => _ = GrasshopperUi.Handler(valid: () => changed(value));
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
        }, what: "AddSpacer"));

    private Fin<Unit> ProjectMenu(ContextMenu menu) => Switch(
        state: menu,
        buttonCase: static (menu, item) => Fin.Succ(item.Command).Map(command => {
            menu.Items.Add(item: command.ToMenuItem());
            return unit;
        }),
        toggleCase: static (menu, item) =>
            from command in Fin.Succ(item.Command)
            from changed in Optional(item.Changed).ToFin(Fail: UiFault.InvalidInput(op: ToggleCase.SelfOp, detail: "toggle change delegate is required"))
            from added in ToggleCase.SelfOp.Attempt(body: () => PushCheckMenuItem(menu: menu, command: command, state: item.State, changed: changed), what: "menu toggle")
            select added,
        spacerCase: static (menu, _) => SpacerCase.SelfOp.Attempt(body: menu.AddSeparator, what: "AddSeparator"),
        sectionToggleCase: static (_, _) => MenuUnsupported(name: nameof(SectionToggleCase)),
        radioCase: static (_, _) => MenuUnsupported(name: nameof(RadioCase)),
        textInputCase: static (_, _) => MenuUnsupported(name: nameof(TextInputCase)),
        numberCase: static (_, _) => MenuUnsupported(name: nameof(NumberCase)),
        swatchInputCase: static (_, _) => MenuUnsupported(name: nameof(SwatchInputCase)),
        colourBarsCase: static (_, _) => MenuUnsupported(name: nameof(ColourBarsCase)));

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

    private static Fin<Unit> MenuUnsupported(string name) =>
        Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: $"{name} cannot be projected to a context menu"));

    private static Unit AddRadioToggle(Bar bar, UiCommand command, bool state, bool optional, Func<bool, Fin<Unit>> changed) {
        RadioToggle toggled = bar.AddRadioToggle(
            icon: command.Icon.IfNone(default(IIcon)!),
            nomen: new Nomen(name: command.Name, info: command.Info),
            initial: state,
            callback: value => _ = GrasshopperUi.Handler(valid: () => changed(arg: value)),
            keys: command.Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null));
        toggled.Optional = optional;
        toggled.Enabled = command.EffectiveEnabled();
        return unit;
    }
}

public readonly record struct CommandPlan(Seq<ToolbarItem> Items) {
    public static CommandPlan Empty => new(Items: Seq<ToolbarItem>());
    public static CommandPlan operator +(CommandPlan left, CommandPlan right) => new(Items: left.Items + right.Items);
    public static CommandPlan Add(CommandPlan left, CommandPlan right) => left + right;
}

public abstract record InputRequest<T> : GhUiRequest<T> {
    internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;

    public sealed record Selection(InputSelectionSource Source) : InputRequest<InputSelectionSnapshot> { internal override Fin<InputSelectionSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Selection(source: Source).Run(scope: scope); }
    public sealed record Modifiers(Keys Keys) : InputRequest<InputModifierSnapshot> { internal override Fin<InputModifierSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Modifiers(keys: Keys).Run(scope: scope); }
    public sealed record ModifierStateCase : InputRequest<InputModifierSnapshot> { internal override Fin<InputModifierSnapshot> Apply(GrasshopperUi.Scope scope) => Input.ModifierState().Run(scope: scope); }
    public sealed record PointerStateCase : InputRequest<PointerSnapshot> { internal override Fin<PointerSnapshot> Apply(GrasshopperUi.Scope scope) => Input.PointerState().Run(scope: scope); }
    public sealed record Panel(Func<InputPanel, Fin<Unit>> Populate) : InputRequest<InputPanelSnapshot> { internal override Fin<InputPanelSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Panel(populate: Populate).Run(scope: scope); }
    public sealed record Popup(Control Owner, PointF Location, RectangleF Screen, Func<InputPanel, Fin<Unit>> Populate) : InputRequest<InputPanelSnapshot> { internal override Fin<InputPanelSnapshot> Apply(GrasshopperUi.Scope scope) => Input.PopupPanel(owner: Owner, location: Location, screen: Screen, populate: Populate).Run(scope: scope); }
    public sealed record CommandBar(CommandPlan Plan) : InputRequest<ToolbarSnapshot> { internal override Fin<ToolbarSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Toolbar(plan: Plan).Run(scope: scope); }
    public sealed record CommandPanel(CommandPlan Plan) : InputRequest<InputPanelSnapshot> { internal override Fin<InputPanelSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Panel(plan: Plan).Run(scope: scope); }
    public sealed record MenuShow(CommandPlan Plan, Control Owner, PointF Location) : InputRequest<MenuSnapshot> { internal override Fin<MenuSnapshot> Apply(GrasshopperUi.Scope scope) => Input.ShowMenu(plan: Plan, owner: Owner, location: Location).Run(scope: scope); }
    public sealed record Cursor(CursorKind Kind) : InputRequest<CursorKind> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<CursorKind> Apply(GrasshopperUi.Scope scope) => Input.Cursor(kind: Kind).Run(scope: scope);
    }
    // RAII cursor; dispose the returned IDisposable to restore the canvas's prior Cursor.
    public sealed record CursorScope(CursorKind Kind) : InputRequest<IDisposable> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<IDisposable> Apply(GrasshopperUi.Scope scope) => Input.CursorScope(kind: Kind).Run(scope: scope);
    }
    public sealed record Message(string Title, string Body, MessageBoxType Kind = MessageBoxType.Information, MessageBoxButtons Buttons = MessageBoxButtons.OK, MessageBoxDefaultButton Default = MessageBoxDefaultButton.Default) : InputRequest<DialogResult> { internal override Fin<DialogResult> Apply(GrasshopperUi.Scope scope) => Input.MessageDialog(title: Title, message: Body, kind: Kind, buttons: Buttons, defaultButton: Default).Run(scope: scope); }
    public sealed record File(FileDialogMode Mode, Option<string> InitialPath, Seq<FileFilter> Filters, bool MultiSelect = false, Option<string> Title = default) : InputRequest<Seq<string>> { internal override Fin<Seq<string>> Apply(GrasshopperUi.Scope scope) => Input.FileDialog(mode: Mode, initialPath: InitialPath, multiSelect: MultiSelect, title: Title, filters: [.. Filters]).Run(scope: scope); }
    public sealed record Clipboard(InputClipboardOp Op) : InputRequest<InputClipboardSnapshot> { internal override Fin<InputClipboardSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Clipboard(op: Op).Run(scope: scope); }
    // Typed modal dialog. The Configure delegate is responsible for installing widgets, wiring
    // a confirmation Button that invokes dialog.Close(result), and returning Fin.Succ once setup
    // is complete. macOS: DisplayMode.Attached presents as a sheet on the parent window.
    public sealed record Dialog<TResult>(Func<Eto.Forms.Dialog<TResult>, Fin<Unit>> Configure, string Title = "", Option<DialogPresentation> Presentation = default) : InputRequest<Option<TResult>> {
        internal override Fin<Option<TResult>> Apply(GrasshopperUi.Scope scope) => Input.Dialog(configure: Configure, title: Title, presentation: Presentation).Run(scope: scope);
    }
    public sealed record PickColor(Color Initial, bool AllowAlpha = true) : InputRequest<Option<Color>> { internal override Fin<Option<Color>> Apply(GrasshopperUi.Scope scope) => Input.PickColor(initial: Initial, allowAlpha: AllowAlpha).Run(scope: scope); }
    public sealed record PickFont(Option<Font> Initial = default) : InputRequest<Option<Font>> { internal override Fin<Option<Font>> Apply(GrasshopperUi.Scope scope) => Input.PickFont(initial: Initial).Run(scope: scope); }
    public sealed record Notify(string Title, string Body, Option<Image> ContentImage = default) : InputRequest<Unit> { internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Input.Notify(title: Title, message: Body, contentImage: ContentImage).Run(scope: scope); }
}

// Scope-restoring cursor capsule; disposing reinstates the canvas's prior Cursor. Use via
// `using IDisposable _ = await GhUi.Input(new InputRequest<...>.CursorScope(...)).Use(...);`
// or directly through `Input.CursorScope(kind)` when the surrounding scope is already typed.
internal sealed class ScopedCursor(Grasshopper2.UI.Canvas.Canvas target, Cursor previous) : IDisposable {
    public void Dispose() => target.Cursor = previous;
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Input {
    internal static GrasshopperUiIntent<InputSelectionSnapshot> Selection(InputSelectionSource source) =>
        GhUi.Read(run: _ =>
            Optional(source)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "null source"))
                .Map(valid => new InputSelectionSnapshot(Mode: valid.Mode(), Modifiers: ModifierOf(keys: Keyboard.Modifiers))));

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
                        Category: panel.Category ?? string.Empty,
                        Shown: false));
                }));

    internal static GrasshopperUiIntent<InputPanelSnapshot> PopupPanel(Control owner, PointF location, RectangleF screen, Func<InputPanel, Fin<Unit>> populate) =>
        GhUi.Read(run: _ =>
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null owner"))
            from validPopulate in Optional(populate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null populate"))
            from validLocation in Op.Of(name: nameof(PopupPanel)).AcceptPoint(value: location, detail: "non-finite location")
            from validScreen in Op.Of(name: nameof(PopupPanel)).AcceptRect(value: screen, detail: "invalid screen bounds", requirePositive: true)
            let panel = new InputPanel()
            from populated in validPopulate(arg: panel)
            let form = panel.ShowAsForm(validOwner, validLocation, validScreen)
            select new InputPanelSnapshot(
                Count: panel.Count,
                Category: panel.Category ?? string.Empty,
                Shown: form?.Visible ?? false));

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

    internal static GrasshopperUiIntent<InputPanelSnapshot> Panel(CommandPlan plan) =>
        Panel(populate: panel => Try.lift(f: () => panel.AddBar(drawCategoryLabels: false)).Run()
            .MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Panel)), detail: $"InputPanel.AddBar threw: {error.Message}"))
            .Bind(bar => plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).Map(static _ => unit).As()));

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
                return (IDisposable)new ScopedCursor(target: canvas, previous: previous);
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

    internal static GrasshopperUiIntent<Seq<string>> FileDialog(FileDialogMode mode, Option<string> initialPath = default, bool multiSelect = false, Option<string> title = default, params FileFilter[] filters) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => mode.Run(parent: DialogParent(scope: scope), initialPath: initialPath, filters: filters, multiSelect: multiSelect, title: title))
                .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(FileDialog)), detail: $"FileDialog.ShowDialog threw: {error.Message}")));

    internal static GrasshopperUiIntent<InputClipboardSnapshot> Clipboard(InputClipboardOp op) =>
        GhUi.Read(run: _scope => Op.Of(name: nameof(Clipboard)).Attempt(
            body: () => op.Switch(
                readCase: static _ => ClipboardSnapshotOf(),
                writeCase: static w => {
                    Clipboard clipboard = Eto.Forms.Clipboard.Instance;
                    clipboard.Clear();
                    _ = w.Payload.Text.Iter(text => clipboard.Text = text);
                    _ = w.Payload.Html.Iter(html => clipboard.Html = html);
                    _ = w.Payload.Image.Iter(image => clipboard.Image = image);
                    _ = w.Payload.Uris.IsEmpty ? unit : Optional(w.Payload.Uris.ToArray()).Iter(uris => clipboard.Uris = uris);
                    return ClipboardSnapshotOf();
                },
                clearCase: static _ => {
                    Eto.Forms.Clipboard.Instance.Clear();
                    return InputClipboardSnapshot.Empty;
                }),
            what: "Clipboard op"));

    internal static GrasshopperUiIntent<Option<TResult>> Dialog<TResult>(Func<Dialog<TResult>, Fin<Unit>> configure, string title, Option<DialogPresentation> presentation) =>
        GhUi.Read(run: scope =>
            from validConfigure in Optional(configure).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Dialog)), detail: "null configure delegate"))
            from result in RunDialog(scope: scope, title: title, presentation: presentation.IfNone(DialogPresentation.Modal), configure: validConfigure)
            select result);

    private static Fin<Option<TResult>> RunDialog<TResult>(GrasshopperUi.Scope scope, string title, DialogPresentation presentation, Func<Dialog<TResult>, Fin<Unit>> configure) =>
        Try.lift<Fin<Option<TResult>>>(f: () => {
            using Dialog<TResult> dialog = new() { Title = title };
            Rhino.UI.EtoExtensions.UseRhinoStyle(dialog);
            return configure(arg: dialog).Map(_ => presentation.Show(dialog: dialog, parent: DialogParent(scope: scope)));
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Dialog)), detail: $"Dialog<T> threw: {error.Message}")).Bind(static r => r);

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

    internal static GrasshopperUiIntent<Unit> Notify(string title, string message, Option<Image> contentImage) =>
        GhUi.Read(run: scope =>
            Try.lift(f: () => {
                Notification notification = new() { Title = title, Message = message };
                _ = contentImage.Iter(image => notification.ContentImage = image);
                notification.Show();
                return unit;
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Notify)), detail: $"Notification threw: {error.Message}")));

    private static InputClipboardSnapshot ClipboardSnapshotOf() {
        Clipboard clipboard = Eto.Forms.Clipboard.Instance;
        return new(
            Text: Optional(clipboard.Text),
            Html: Optional(clipboard.Html),
            Image: Optional(clipboard.Image),
            Uris: toSeq(clipboard.Uris ?? []),
            Types: toSeq(clipboard.Types ?? []));
    }

    // --- [OPERATIONS] -------------------------------------------------------------------------
    internal static InputModifierSnapshot ModifierOf(Keys keys) =>
        new(Shift: keys.HasShift(), Command: keys.HasCommand(), Option: keys.HasOption());

    private static Control? DialogParent(GrasshopperUi.Scope scope) =>
        scope.Editor.Map(static _ => (Control?)Grasshopper2.UI.Editor.ThisOrRhino).IfNone(() =>
            scope.Canvas.Map(static c => (Control?)c.ControlObject)
                .IfNone(() => (Control?)(Rhino.UI.RhinoEtoApp.MainWindowForOwner ?? Rhino.UI.RhinoEtoApp.MainWindow)));

    internal static Seq<string> RunFileDialog(FileDialog dialog, Control? parent, Option<string> initialPath, FileFilter[] filters, Option<string> title) {
        using FileDialog owned = dialog;
        // Empty-string Uri throws — fail closed before `new Uri(...)` when the resolved path is empty
        // (Path.GetDirectoryName can return null/empty for relative paths or root volumes).
        _ = initialPath.Filter(static path => !string.IsNullOrWhiteSpace(path))
            .Bind(path => Optional(Directory.Exists(path: path) ? path : System.IO.Path.GetDirectoryName(path: path))
                .Filter(static resolved => !string.IsNullOrWhiteSpace(resolved)))
            .IfSome(resolved => owned.Directory = new Uri(uriString: resolved));
        _ = title.IfSome(value => owned.Title = value);
        _ = toSeq(filters).Iter(f => owned.Filters.Add(item: new Eto.Forms.FileFilter(name: f.Name, extensions: [.. f.Extensions])));
        DialogResult result = owned.ShowDialog(parent: parent);
        return result switch {
            DialogResult.Ok when owned is OpenFileDialog { MultiSelect: true } open => toSeq(open.Filenames),
            DialogResult.Ok => Seq(owned.FileName),
            _ => Seq<string>(),
        };
    }

    // SelectFolderDialog : CommonDialog (NOT FileDialog) — separate dispatch (per Eto verification).
    internal static Seq<string> RunFolderDialog(Control? parent, Option<string> initialPath, Option<string> title) {
        using SelectFolderDialog dialog = new();
        _ = initialPath.Filter(static path => !string.IsNullOrWhiteSpace(path)).IfSome(path => dialog.Directory = path);
        _ = title.IfSome(value => dialog.Title = value);
        DialogResult result = dialog.ShowDialog(parent: parent);
        return result switch {
            DialogResult.Ok => Seq(dialog.Directory),
            _ => Seq<string>(),
        };
    }
}
