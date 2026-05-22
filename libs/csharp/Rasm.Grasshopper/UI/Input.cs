using Eto.Forms;
using Grasshopper2.Extensions;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
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
    public static readonly CursorKind WireIn = new(key: 9, cursor: static _ => Grasshopper2.UI.Canvas.Canvas.CursorWireIn ?? Cursors.Pointer);
    public static readonly CursorKind WireOut = new(key: 10, cursor: static _ => Grasshopper2.UI.Canvas.Canvas.CursorWireOut ?? Cursors.Pointer);
    public static readonly CursorKind WireQuestion = new(key: 11, cursor: static _ => Grasshopper2.UI.Canvas.Canvas.CursorQuestion ?? Cursors.Default);

    [UseDelegateFromConstructor]
    internal partial Cursor Cursor(Grasshopper2.UI.Canvas.Canvas canvas);
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
        controlCase: static c => c.Source.SelectionMode(),
        mouseCase: static m => m.Source.SelectionMode(),
        windowCase: static w => w.Source.SelectionMode());
}

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
public readonly record struct InputSelectionSnapshot(SelectionMode Mode, InputModifierSnapshot Modifiers);

[StructLayout(LayoutKind.Auto)]
public readonly record struct InputPanelSnapshot(int Count, string Category, bool Shown);

[StructLayout(LayoutKind.Auto)]
public readonly record struct ToolbarSnapshot(int Count, bool Enabled, float MinimumWidth, float MaximumWidth, float Height);

[StructLayout(LayoutKind.Auto)]
public readonly record struct MenuSnapshot(int Count);

public readonly record struct InputClipboardSnapshot(
    Option<string> Text,
    Option<string> Html,
    Option<Image> Image,
    Seq<Uri> Uris,
    Seq<string> Types) {
    public static InputClipboardSnapshot Empty => new(
        Text: Option<string>.None,
        Html: Option<string>.None,
        Image: Option<Image>.None,
        Uris: Seq<Uri>(),
        Types: Seq<string>());

    public static InputClipboardSnapshot Of(string text) => Empty with { Text = Optional(text) };
}

public readonly record struct FileFilter(string Name, Seq<string> Extensions);

public readonly record struct UiCommand(
    string Name,
    string Info,
    Func<Fin<Unit>> Run,
    Option<IIcon> Icon = default,
    Option<Image> Image = default,
    Option<BarShortcut> Shortcut = default,
    bool Enabled = true,
    Option<Func<Fin<bool>>> CanExecute = default) {
    internal Fin<UiCommand> Validated(Op op) {
        UiCommand command = this;
        return Optional(command.Run)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "command run delegate is required"))
            .Map(_ => command);
    }

    internal Command ToEtoCommand() {
        string name = Name;
        string info = Info;
        Func<Fin<Unit>> run = Run;
        Command command = new() { ID = name, MenuText = name, ToolBarText = name, ToolTip = info, Enabled = EffectiveEnabled() };
        _ = MenuImage.Iter(image => command.Image = image);
        _ = Shortcut.Iter(shortcut => command.Shortcut = shortcut.Keys);
        command.Executed += (_, _) => _ = GrasshopperUi.Protect(valid: run);
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

[Union]
internal partial record UiCommandSurface {
    private UiCommandSurface() { }
    public sealed record ToolbarCase(Bar Bar) : UiCommandSurface;
    public sealed record MenuCase(ContextMenu Target) : UiCommandSurface;
    public static UiCommandSurface Toolbar(Bar bar) => new ToolbarCase(Bar: bar);
    public static UiCommandSurface Menu(ContextMenu menu) => new MenuCase(Target: menu);
}

public abstract record ToolbarItem {
    public sealed record Button(UiCommand Command, bool CloseOnActivate = true) : ToolbarItem;
    public sealed record Toggle(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record SectionToggle(string Name, bool State, Seq<string> Sections = default) : ToolbarItem;
    public sealed record Radio(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record TextInput(string Name, string Value, Func<string, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record Number(string Name, UiNumber Value, Func<decimal, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record SwatchInput(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record ColourBars(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record Spacer(string Chapter, string Section) : ToolbarItem;

    internal Fin<Unit> Apply(UiCommandSurface surface) =>
        Optional(surface)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(UiCommandSurface)))
            .Bind(valid => (this, valid) switch {
                (Button button, UiCommandSurface.ToolbarCase toolbar) => button.Command.Validated(op: Op.Of(name: nameof(Button))).Map(command => {
                    PushButton pushed = toolbar.Bar.AddPushButton(
                        icon: command.Icon.IfNone(default(IIcon)!),
                        nomen: new Nomen(name: command.Name, info: command.Info),
                        callback: () => _ = GrasshopperUi.Protect(valid: command.Run),
                        keys: command.Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null));
                    pushed.Enabled = command.EffectiveEnabled();
                    pushed.CloseOnActivate = button.CloseOnActivate;
                    return unit;
                }),
                (Button button, UiCommandSurface.MenuCase menu) => button.Command.Validated(op: Op.Of(name: nameof(Button))).Map(command => {
                    menu.Target.Items.Add(item: command.ToMenuItem());
                    return unit;
                }),
                (Toggle toggle, UiCommandSurface.ToolbarCase toolbar) =>
                    from command in toggle.Command.Validated(op: Op.Of(name: nameof(Toggle)))
                    from changed in Optional(toggle.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Toggle)), detail: "toggle change delegate is required"))
                    select AddRadioToggle(bar: toolbar.Bar, command: command, state: toggle.State, optional: true, changed: changed),
                (Toggle toggle, UiCommandSurface.MenuCase menu) =>
                    from command in toggle.Command.Validated(op: Op.Of(name: nameof(Toggle)))
                    from changed in Optional(toggle.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Toggle)), detail: "toggle change delegate is required"))
                    from added in Try.lift(f: () => {
                        CheckCommand menuCommand = new() {
                            ID = command.Name,
                            MenuText = command.Name,
                            ToolTip = command.Info,
                            Enabled = command.EffectiveEnabled(),
                            Checked = toggle.State,
                        };
                        _ = command.MenuImage.Iter(image => menuCommand.Image = image);
                        _ = command.Shortcut.Iter(shortcut => menuCommand.Shortcut = shortcut.Keys);
                        menuCommand.Executed += (_, _) => _ = GrasshopperUi.Protect(valid: () => changed(arg: menuCommand.Checked));
                        MenuItem item = menuCommand.CreateMenuItem();
                        _ = command.CanExecute.Iter(can => item.Validate += (_, _) =>
                            item.Enabled = command.Enabled && GrasshopperUi.Protect(valid: can).IfFail(_ => false));
                        menu.Target.Items.Add(item: item);
                        return unit;
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Toggle)), detail: $"menu toggle threw: {error.Message}"))
                    select added,
                (SectionToggle toggle, UiCommandSurface.ToolbarCase toolbar) => Try.lift(f: () => {
                    _ = toolbar.Bar.AddToggle(nomen: new Nomen(name: toggle.Name, info: toggle.Name), initialState: toggle.State, additionalAffectedSections: [.. toggle.Sections]);
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(SectionToggle)), detail: $"AddToggle threw: {error.Message}")),
                (Radio radio, UiCommandSurface.ToolbarCase toolbar) =>
                    from command in radio.Command.Validated(op: Op.Of(name: nameof(Radio)))
                    from changed in Optional(radio.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Radio)), detail: "radio change delegate is required"))
                    select AddRadioToggle(bar: toolbar.Bar, command: command, state: radio.State, optional: false, changed: changed),
                (TextInput text, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(text.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(TextInput)), detail: "text change delegate is required"))
                    from added in Try.lift(f: () => {
                        TextField field = toolbar.Bar.AddTextField(icon: default!, nomen: new Nomen(name: text.Name, info: text.Name), initial: text.Value, placeholder: text.Name);
                        field.TextChanged += (_, value) => _ = GrasshopperUi.Protect(valid: () => changed(arg: value));
                        return unit;
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(TextInput)), detail: $"AddTextField threw: {error.Message}"))
                    select added,
                (Number number, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(number.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Number)), detail: "number change delegate is required"))
                    from value in Optional(number.Value).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Number)), detail: "UiNumber is required"))
                    from added in Try.lift(f: () => {
                        toolbar.Bar.Add(new NumberSlider(nomen: new Nomen(name: number.Name, info: number.Name), number: value, callback: current => _ = GrasshopperUi.Protect(valid: () => changed(arg: current))));
                        return unit;
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Number)), detail: $"NumberSlider threw: {error.Message}"))
                    select added,
                (SwatchInput colour, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(colour.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SwatchInput)), detail: "colour change delegate is required"))
                    from added in Try.lift(f: () => {
                        toolbar.Bar.AddLifeColours(nomen: new Nomen(name: colour.Name, info: colour.Name), initial: colour.Family, assignment: value => _ = GrasshopperUi.Protect(valid: () => changed(arg: value)));
                        return unit;
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(SwatchInput)), detail: $"AddLifeColours threw: {error.Message}"))
                    select added,
                (ColourBars colour, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(colour.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ColourBars)), detail: "colour change delegate is required"))
                    from added in Try.lift(f: () => {
                        void Assign(OpenColor.Family value) => _ = GrasshopperUi.Protect(valid: () => changed(arg: value));
                        Nomen nomen = new(name: $"{colour.Name} {{family}}", info: $"{colour.Name} {{family}}");
                        toolbar.Bar.AddLifeColours(nomen: nomen, initial: colour.Family, assignment: Assign);
                        toolbar.Bar.AddCoolColours(nomen: nomen, initial: colour.Family, assignment: Assign);
                        toolbar.Bar.AddWarmColours(nomen: nomen, initial: colour.Family, assignment: Assign);
                        return unit;
                    }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ColourBars)), detail: $"colour bars threw: {error.Message}"))
                    select added,
                (Spacer spacer, UiCommandSurface.ToolbarCase toolbar) => Try.lift(f: () => {
                    _ = toolbar.Bar.AddSpacer(chapterName: spacer.Chapter, sectionName: spacer.Section);
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Spacer)), detail: $"AddSpacer threw: {error.Message}")),
                (Spacer, UiCommandSurface.MenuCase menu) => Try.lift(f: () => { menu.Target.AddSeparator(); return unit; })
                            .Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Spacer)), detail: $"AddSeparator threw: {error.Message}")),
                (_, UiCommandSurface.MenuCase) => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: $"{GetType().Name} cannot be projected to a context menu")),
                _ => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: "unknown command surface")),
            });

    private static Unit AddRadioToggle(Bar bar, UiCommand command, bool state, bool optional, Func<bool, Fin<Unit>> changed) {
        RadioToggle toggled = bar.AddRadioToggle(
            icon: command.Icon.IfNone(default(IIcon)!),
            nomen: new Nomen(name: command.Name, info: command.Info),
            initial: state,
            callback: value => _ = GrasshopperUi.Protect(valid: () => changed(arg: value)),
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
    public sealed record Selection(InputSelectionSource Source) : InputRequest<InputSelectionSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputSelectionSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Selection(source: Source).Run(scope: scope);
    }
    public sealed record Modifiers(Keys Keys) : InputRequest<InputModifierSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputModifierSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Modifiers(keys: Keys).Run(scope: scope);
    }
    public sealed record Panel(Func<InputPanel, Fin<Unit>> Populate) : InputRequest<InputPanelSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputPanelSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Panel(populate: Populate).Run(scope: scope);
    }
    public sealed record Popup(Control Owner, PointF Location, RectangleF Screen, Func<InputPanel, Fin<Unit>> Populate) : InputRequest<InputPanelSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputPanelSnapshot> Apply(GrasshopperUi.Scope scope) => Input.PopupPanel(owner: Owner, location: Location, screen: Screen, populate: Populate).Run(scope: scope);
    }
    public sealed record CommandBar(CommandPlan Plan) : InputRequest<ToolbarSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<ToolbarSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Toolbar(plan: Plan).Run(scope: scope);
    }
    public sealed record CommandPanel(CommandPlan Plan) : InputRequest<InputPanelSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputPanelSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Panel(plan: Plan).Run(scope: scope);
    }
    public sealed record MenuShow(CommandPlan Plan, Control Owner, PointF Location) : InputRequest<MenuSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<MenuSnapshot> Apply(GrasshopperUi.Scope scope) => Input.ShowMenu(plan: Plan, owner: Owner, location: Location).Run(scope: scope);
    }
    public sealed record Cursor(CursorKind Kind) : InputRequest<CursorKind> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<CursorKind> Apply(GrasshopperUi.Scope scope) => Input.Cursor(kind: Kind).Run(scope: scope);
    }
    // RAII cursor; dispose the returned IDisposable to restore the canvas's prior Cursor.
    public sealed record CursorScope(CursorKind Kind) : InputRequest<IDisposable> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<IDisposable> Apply(GrasshopperUi.Scope scope) => Input.CursorScope(kind: Kind).Run(scope: scope);
    }
    public sealed record Message(string Title, string Body, MessageBoxType Kind = MessageBoxType.Information, MessageBoxButtons Buttons = MessageBoxButtons.OK, MessageBoxDefaultButton Default = MessageBoxDefaultButton.Default) : InputRequest<DialogResult> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<DialogResult> Apply(GrasshopperUi.Scope scope) => Input.MessageDialog(title: Title, message: Body, kind: Kind, buttons: Buttons, defaultButton: Default).Run(scope: scope);
    }
    public sealed record File(FileDialogMode Mode, Option<string> InitialPath, Seq<FileFilter> Filters, bool MultiSelect = false, Option<string> Title = default) : InputRequest<Seq<string>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Seq<string>> Apply(GrasshopperUi.Scope scope) => Input.FileDialog(mode: Mode, initialPath: InitialPath, multiSelect: MultiSelect, title: Title, filters: [.. Filters]).Run(scope: scope);
    }
    public sealed record Clipboard(InputClipboardOp Op) : InputRequest<InputClipboardSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputClipboardSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Clipboard(op: Op).Run(scope: scope);
    }
    // Typed modal dialog. The Configure delegate is responsible for installing widgets, wiring
    // a confirmation Button that invokes dialog.Close(result), and returning Fin.Succ once setup
    // is complete. macOS: DisplayMode.Attached presents as a sheet on the parent window.
    public sealed record Dialog<TResult>(Func<Eto.Forms.Dialog<TResult>, Fin<Unit>> Configure, string Title = "", DialogDisplayMode Mode = DialogDisplayMode.Default) : InputRequest<Option<TResult>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Option<TResult>> Apply(GrasshopperUi.Scope scope) => Input.Dialog(configure: Configure, title: Title, mode: Mode).Run(scope: scope);
    }
    public sealed record PickColor(Color Initial, bool AllowAlpha = true) : InputRequest<Option<Color>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Option<Color>> Apply(GrasshopperUi.Scope scope) => Input.PickColor(initial: Initial, allowAlpha: AllowAlpha).Run(scope: scope);
    }
    public sealed record PickFont(Option<Font> Initial = default) : InputRequest<Option<Font>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Option<Font>> Apply(GrasshopperUi.Scope scope) => Input.PickFont(initial: Initial).Run(scope: scope);
    }
    public sealed record Notify(string Title, string Body, Option<Image> ContentImage = default) : InputRequest<Unit> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Unit> Apply(GrasshopperUi.Scope scope) => Input.Notify(title: Title, message: Body, contentImage: ContentImage).Run(scope: scope);
    }
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
        GhUi.Canvas(run: scope => scope.NeedCanvas().Map(canvas => {
            canvas.Cursor = kind.Cursor(canvas: canvas);
            return kind;
        }));

    internal static GrasshopperUiIntent<IDisposable> CursorScope(CursorKind kind) =>
        GhUi.Canvas(run: scope => scope.NeedCanvas().Map(canvas => {
            Cursor previous = canvas.Cursor;
            canvas.Cursor = kind.Cursor(canvas: canvas);
            return (IDisposable)new ScopedCursor(target: canvas, previous: previous);
        }));

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
        GhUi.Read(run: _scope =>
            Try.lift(f: () => {
                Func<InputClipboardSnapshot> action = op switch {
                    InputClipboardOp.ReadCase => ClipboardSnapshotOf,
                    InputClipboardOp.WriteCase w => () => {
                        Clipboard clipboard = Eto.Forms.Clipboard.Instance;
                        _ = w.Payload.Text.Iter(text => clipboard.Text = text);
                        _ = w.Payload.Html.Iter(html => clipboard.Html = html);
                        _ = w.Payload.Image.Iter(image => clipboard.Image = image);
                        _ = w.Payload.Uris.IsEmpty ? unit : Optional(w.Payload.Uris.ToArray()).Iter(uris => clipboard.Uris = uris);
                        return ClipboardSnapshotOf();
                    }
                    ,
                    InputClipboardOp.ClearCase => static () => {
                        Eto.Forms.Clipboard.Instance.Clear();
                        return InputClipboardSnapshot.Empty;
                    }
                    ,
                    _ => static () => InputClipboardSnapshot.Empty,
                };
                return action();
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Clipboard)), detail: $"Clipboard op threw: {error.Message}")));

    internal static GrasshopperUiIntent<Option<TResult>> Dialog<TResult>(Func<Dialog<TResult>, Fin<Unit>> configure, string title, DialogDisplayMode mode) =>
        GhUi.Read(run: scope =>
            from validConfigure in Optional(configure).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Dialog)), detail: "null configure delegate"))
            from result in RunDialog(scope: scope, title: title, mode: mode, configure: validConfigure)
            select result);

    private static Fin<Option<TResult>> RunDialog<TResult>(GrasshopperUi.Scope scope, string title, DialogDisplayMode mode, Func<Dialog<TResult>, Fin<Unit>> configure) =>
        Try.lift<Fin<Option<TResult>>>(f: () => {
            using Dialog<TResult> dialog = new() { Title = title, DisplayMode = mode };
            return configure(arg: dialog).Map(_ => Optional(dialog.ShowModal(owner: DialogParent(scope: scope))));
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
                Unit assignImage = contentImage.Iter(image => notification.ContentImage = image);
                notification.Show(indicator: null!);
                return assignImage;
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
    private static InputModifierSnapshot ModifierOf(Keys keys) =>
        new(Shift: keys.HasShift(), Command: keys.HasCommand(), Option: keys.HasOption());

    private static Control? DialogParent(GrasshopperUi.Scope scope) =>
        scope.Editor.Map(static _ => (Control?)Editor.ThisOrRhino).IfNone((Control?)null);

    internal static Seq<string> RunFileDialog(FileDialog dialog, Control? parent, Option<string> initialPath, FileFilter[] filters, Option<string> title) {
        using FileDialog owned = dialog;
        _ = initialPath.Filter(static path => !string.IsNullOrWhiteSpace(path))
            .IfSome(path => owned.Directory = new Uri(uriString: Directory.Exists(path: path) ? path : System.IO.Path.GetDirectoryName(path: path) ?? string.Empty));
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
