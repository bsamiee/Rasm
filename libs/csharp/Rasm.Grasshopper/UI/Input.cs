using System.IO;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Extensions;
using Grasshopper2.UI;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Icon;
using Grasshopper2.UI.InputPanel;
using Grasshopper2.UI.Toolbar;
using Rhino.Geometry;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CursorKind {
    private delegate Eto.Forms.Cursor CursorSource(Grasshopper2.UI.Canvas.Canvas canvas);

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
    internal partial Eto.Forms.Cursor Cursor(Grasshopper2.UI.Canvas.Canvas canvas);
}

[SmartEnum<int>]
public sealed partial class FileDialogMode {
    private delegate Option<string> DialogRun(Control? parent, Option<string> initialPath, FileFilter[] filters);

    public static readonly FileDialogMode Open = new(
        key: 0,
        run: static (parent, initialPath, filters) => Input.RunFileDialog(dialog: new OpenFileDialog(), parent: parent, initialPath: initialPath, filters: filters));
    public static readonly FileDialogMode Save = new(
        key: 1,
        run: static (parent, initialPath, filters) => Input.RunFileDialog(dialog: new SaveFileDialog(), parent: parent, initialPath: initialPath, filters: filters));
    public static readonly FileDialogMode Folder = new(
        key: 2,
        run: static (parent, initialPath, _) => Input.RunFolderDialog(parent: parent, initialPath: initialPath));

    [UseDelegateFromConstructor]
    internal partial Option<string> Run(Control? parent, Option<string> initialPath, FileFilter[] filters);
}

[SmartEnum<int>]
public sealed partial class MessageDialogKind {
    public static readonly MessageDialogKind Information = new(key: 0, type: MessageBoxType.Information);
    public static readonly MessageDialogKind Warning = new(key: 1, type: MessageBoxType.Warning);
    public static readonly MessageDialogKind Error = new(key: 2, type: MessageBoxType.Error);
    public static readonly MessageDialogKind Question = new(key: 3, type: MessageBoxType.Question);

    public MessageBoxType Type { get; }
}

[Flags]
public enum MessageDialogButtons {
    None = 0,
    Ok = 1, Cancel = 2, Yes = 4, No = 8,
    OkCancel = Ok | Cancel,
    YesNo = Yes | No,
    YesNoCancel = Yes | No | Cancel,
}

[SmartEnum<int>]
public sealed partial class InputDialogResponse {
    public static readonly InputDialogResponse None = new(key: 0, native: DialogResult.None);
    public static readonly InputDialogResponse Ok = new(key: 1, native: DialogResult.Ok);
    public static readonly InputDialogResponse Cancel = new(key: 2, native: DialogResult.Cancel);
    public static readonly InputDialogResponse Yes = new(key: 3, native: DialogResult.Yes);
    public static readonly InputDialogResponse No = new(key: 4, native: DialogResult.No);
    public static readonly InputDialogResponse Abort = new(key: 5, native: DialogResult.Abort);
    public static readonly InputDialogResponse Ignore = new(key: 6, native: DialogResult.Ignore);
    public static readonly InputDialogResponse Retry = new(key: 7, native: DialogResult.Retry);

    public DialogResult Native { get; }

    internal static InputDialogResponse Of(DialogResult result) =>
        toSeq(Items).Find(item => item.Native == result).IfNone(None);
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
    Option<Eto.Drawing.Image> Image,
    Seq<Uri> Uris,
    Seq<string> Types) {
    public static InputClipboardSnapshot Empty => new(
        Text: Option<string>.None,
        Html: Option<string>.None,
        Image: Option<Eto.Drawing.Image>.None,
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
    Option<Eto.Drawing.Image> Image = default,
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
        _ = Image.Iter(image => command.Image = image);
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
        Enabled && CanExecute.Map(can => GrasshopperUi.Protect(valid: can).IfFail(_ => false)).IfNone(true);
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
    public sealed record Radio(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record TextInput(string Name, string Value, Func<string, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record Number(string Name, double Value, Interval Range, Func<double, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record SwatchInput(string Name, OpenColor.Family Family, Func<OpenColor.Family, Fin<Unit>> Changed) : ToolbarItem;
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
                    select ((Func<Unit>)(() => {
                        CheckCommand menuCommand = new() {
                            ID = command.Name,
                            MenuText = command.Name,
                            ToolTip = command.Info,
                            Enabled = command.EffectiveEnabled(),
                            Checked = toggle.State,
                        };
                        _ = command.Image.Iter(image => menuCommand.Image = image);
                        _ = command.Shortcut.Iter(shortcut => menuCommand.Shortcut = shortcut.Keys);
                        menuCommand.Executed += (_, _) => _ = GrasshopperUi.Protect(valid: () => changed(arg: menuCommand.Checked));
                        MenuItem item = menuCommand.CreateMenuItem();
                        _ = command.CanExecute.Iter(can => item.Validate += (_, _) =>
                            item.Enabled = command.Enabled && GrasshopperUi.Protect(valid: can).IfFail(_ => false));
                        menu.Target.Items.Add(item: item);
                        return unit;
                    }))(),
                (Radio radio, UiCommandSurface.ToolbarCase toolbar) =>
                    from command in radio.Command.Validated(op: Op.Of(name: nameof(Radio)))
                    from changed in Optional(radio.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Radio)), detail: "radio change delegate is required"))
                    select AddRadioToggle(bar: toolbar.Bar, command: command, state: radio.State, optional: false, changed: changed),
                (TextInput text, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(text.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(TextInput)), detail: "text change delegate is required"))
                    select ((Func<Unit>)(() => {
                        TextField field = toolbar.Bar.AddTextField(icon: default!, nomen: new Nomen(name: text.Name, info: text.Name), initial: text.Value, placeholder: text.Name);
                        field.TextChanged += (_, value) => _ = GrasshopperUi.Protect(valid: () => changed(arg: value));
                        return unit;
                    }))(),
                (Number number, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(number.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Number)), detail: "number change delegate is required"))
                    select ((Func<Unit>)(() => {
                        UiNumber uiNumber = UiNumber.Standard(
                            lower: (decimal)number.Range.T0,
                            upper: (decimal)number.Range.T1,
                            value: (decimal)number.Value,
                            decimals: 3);
                        toolbar.Bar.Add(new NumberSlider(nomen: new Nomen(name: number.Name, info: number.Name), number: uiNumber, callback: value => _ = GrasshopperUi.Protect(valid: () => changed(arg: (double)value))));
                        return unit;
                    }))(),
                (SwatchInput colour, UiCommandSurface.ToolbarCase toolbar) =>
                    from changed in Optional(colour.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SwatchInput)), detail: "colour change delegate is required"))
                    select ((Func<Unit>)(() => {
                        toolbar.Bar.AddLifeColours(nomen: new Nomen(name: colour.Name, info: colour.Name), initial: colour.Family, assignment: value => _ = GrasshopperUi.Protect(valid: () => changed(arg: value)));
                        return unit;
                    }))(),
                (Spacer spacer, UiCommandSurface.ToolbarCase toolbar) => Try.lift<Unit>(f: () => {
                    _ = toolbar.Bar.AddSpacer(chapterName: spacer.Chapter, sectionName: spacer.Section);
                    return unit;
                }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Spacer)), detail: "AddSpacer threw")),
                (Spacer, UiCommandSurface.MenuCase menu) => Try.lift<Unit>(f: () => { menu.Target.AddSeparator(); return unit; })
                            .Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Spacer)), detail: "AddSeparator threw")),
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
    public sealed record Menu(CommandPlan Plan) : InputRequest<MenuSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<MenuSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Menu(plan: Plan).Run(scope: scope);
    }
    public sealed record MenuShow(ContextMenu Target, Control Owner, PointF Location) : InputRequest<MenuSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<MenuSnapshot> Apply(GrasshopperUi.Scope scope) => Input.ShowMenu(menu: Target, owner: Owner, location: Location).Run(scope: scope);
    }
    public sealed record Cursor(CursorKind Kind) : InputRequest<CursorKind> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Canvas();
        internal override Fin<CursorKind> Apply(GrasshopperUi.Scope scope) => Input.Cursor(kind: Kind).Run(scope: scope);
    }
    public sealed record Message(string Title, string Body, MessageDialogKind? Kind = null, MessageDialogButtons Buttons = MessageDialogButtons.Ok) : InputRequest<InputDialogResponse> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputDialogResponse> Apply(GrasshopperUi.Scope scope) => Input.MessageDialog(title: Title, message: Body, kind: Kind ?? MessageDialogKind.Information, buttons: Buttons).Run(scope: scope);
    }
    public sealed record File(FileDialogMode Mode, Option<string> InitialPath, Seq<FileFilter> Filters) : InputRequest<Option<string>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Option<string>> Apply(GrasshopperUi.Scope scope) => Input.FileDialog(mode: Mode, initialPath: InitialPath, filters: [.. Filters]).Run(scope: scope);
    }
    public sealed record Clipboard(InputClipboardOp Op) : InputRequest<InputClipboardSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputClipboardSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Clipboard(op: Op).Run(scope: scope);
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
internal static partial class Input {
    internal static GrasshopperUiIntent<InputSelectionSnapshot> Selection(InputSelectionSource source) =>
        GhUi.Read<InputSelectionSnapshot>(run: _ =>
            Optional(source)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "null source"))
                .Map(valid => new InputSelectionSnapshot(Mode: valid.Mode(), Modifiers: ModifierOf(keys: Keyboard.Modifiers))));

    internal static GrasshopperUiIntent<InputModifierSnapshot> Modifiers(Keys keys) =>
        GhUi.Read<InputModifierSnapshot>(run: _ => Fin.Succ(value: ModifierOf(keys: keys)));

    internal static GrasshopperUiIntent<InputPanelSnapshot> Panel(Func<InputPanel, Fin<Unit>> populate) =>
        GhUi.Read<InputPanelSnapshot>(run: _ =>
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
        GhUi.Read<InputPanelSnapshot>(run: _ =>
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null owner"))
            from validPopulate in Optional(populate).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "null populate"))
            from validLocation in Optional(location)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "non-finite location"))
            from validScreen in Optional(screen)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Width) && float.IsFinite(value.Height) && value.Width > 0 && value.Height > 0)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(PopupPanel)), detail: "invalid screen bounds"))
            let panel = new InputPanel()
            from populated in validPopulate(arg: panel)
            let form = panel.ShowAsForm(validOwner, validLocation, validScreen)
            select new InputPanelSnapshot(
                Count: panel.Count,
                Category: panel.Category ?? string.Empty,
                Shown: form?.Visible ?? false));

    internal static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(Func<Bar, Fin<Unit>> populate) =>
        GhUi.Read<ToolbarSnapshot>(run: _ =>
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
        Panel(populate: panel => Try.lift<Bar>(f: () => panel.AddBar(drawCategoryLabels: false)).Run()
            .MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Panel)), detail: "InputPanel.AddBar threw"))
            .Bind(bar => plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).Map(static _ => unit).As()));

    internal static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(CommandPlan plan) =>
        Toolbar(populate: bar =>
            plan.Items.TraverseM(item => item.Apply(surface: UiCommandSurface.Toolbar(bar: bar))).Map(static _ => unit).As());

    internal static GrasshopperUiIntent<MenuSnapshot> Menu(CommandPlan plan) =>
        GhUi.Read<MenuSnapshot>(run: _ => {
            using ContextMenu menu = new();
            return plan.Items
            .TraverseM(item => item.Apply(surface: UiCommandSurface.Menu(menu: menu)))
                .Map(_ => new MenuSnapshot(Count: menu.Items.Count))
                .As();
        });

    internal static GrasshopperUiIntent<MenuSnapshot> ShowMenu(ContextMenu menu, Control owner, PointF location) =>
        GhUi.Read<MenuSnapshot>(run: _ =>
            from validMenu in Optional(menu).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ShowMenu)), detail: "null menu"))
            from validOwner in Optional(owner).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ShowMenu)), detail: "null owner"))
            from validLocation in Optional(location)
                .Filter(static value => float.IsFinite(value.X) && float.IsFinite(value.Y))
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(ShowMenu)), detail: "non-finite location"))
            from shown in Try.lift<MenuSnapshot>(f: () => {
                validMenu.Show(validOwner, validLocation);
                return new MenuSnapshot(Count: validMenu.Items.Count);
            }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(ShowMenu)), detail: error.Message))
            select shown);

    internal static GrasshopperUiIntent<CursorKind> Cursor(CursorKind kind) =>
        GhUi.Canvas<CursorKind>(run: scope => scope.NeedCanvas().Map(canvas => {
            canvas.Cursor = kind.Cursor(canvas: canvas);
            return kind;
        }));

    internal static GrasshopperUiIntent<InputDialogResponse> MessageDialog(string title, string message, MessageDialogKind? kind = null, MessageDialogButtons buttons = MessageDialogButtons.Ok) =>
        GhUi.Read<InputDialogResponse>(run: scope =>
            Try.lift<InputDialogResponse>(f: () => {
                DialogResult result = MessageBox.Show(
                    parent: DialogParent(scope: scope),
                    text: message,
                    caption: title,
                    buttons: ButtonsOf(buttons),
                    type: (kind ?? MessageDialogKind.Information).Type);
                return InputDialogResponse.Of(result: result);
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(MessageDialog)), detail: "MessageBox.Show threw")));

    internal static GrasshopperUiIntent<Option<string>> FileDialog(FileDialogMode mode, Option<string> initialPath = default, params FileFilter[] filters) =>
        GhUi.Read<Option<string>>(run: scope =>
            Try.lift<Option<string>>(f: () => mode.Run(parent: DialogParent(scope: scope), initialPath: initialPath, filters: filters))
                .Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(FileDialog)), detail: "FileDialog.ShowDialog threw")));

    internal static GrasshopperUiIntent<InputClipboardSnapshot> Clipboard(InputClipboardOp op) =>
        GhUi.Read<InputClipboardSnapshot>(run: _scope =>
            Try.lift<InputClipboardSnapshot>(f: () => {
                Func<InputClipboardSnapshot> action = op switch {
                    InputClipboardOp.ReadCase => ClipboardSnapshotOf,
                    InputClipboardOp.WriteCase w => () => {
                        Eto.Forms.Clipboard clipboard = Eto.Forms.Clipboard.Instance;
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
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Clipboard)), detail: "Clipboard op threw")));

    private static InputClipboardSnapshot ClipboardSnapshotOf() {
        Eto.Forms.Clipboard clipboard = Eto.Forms.Clipboard.Instance;
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

    private static MessageBoxButtons ButtonsOf(MessageDialogButtons b) => b switch {
        MessageDialogButtons.Ok => MessageBoxButtons.OK,
        MessageDialogButtons.OkCancel => MessageBoxButtons.OKCancel,
        MessageDialogButtons.YesNo => MessageBoxButtons.YesNo,
        MessageDialogButtons.YesNoCancel => MessageBoxButtons.YesNoCancel,
        MessageDialogButtons value when (value & MessageDialogButtons.Ok) == MessageDialogButtons.Ok && (value & MessageDialogButtons.Cancel) == MessageDialogButtons.Cancel => MessageBoxButtons.OKCancel,
        MessageDialogButtons value when (value & MessageDialogButtons.Yes) == MessageDialogButtons.Yes && (value & MessageDialogButtons.No) == MessageDialogButtons.No => MessageBoxButtons.YesNo,
        _ => MessageBoxButtons.OK,
    };

    private static Control? DialogParent(GrasshopperUi.Scope scope) =>
        scope.Editor.Map(static _ => (Control?)Grasshopper2.UI.Editor.ThisOrRhino).IfNone((Control?)null);

    internal static Option<string> RunFileDialog(Eto.Forms.FileDialog dialog, Control? parent, Option<string> initialPath, FileFilter[] filters) {
        _ = initialPath.Filter(static path => !string.IsNullOrWhiteSpace(path))
            .IfSome(path => dialog.Directory = new Uri(uriString: Directory.Exists(path: path) ? path : System.IO.Path.GetDirectoryName(path: path) ?? string.Empty));
        _ = toSeq(filters).Iter(f => dialog.Filters.Add(item: new Eto.Forms.FileFilter(name: f.Name, extensions: [.. f.Extensions])));
        DialogResult result = dialog.ShowDialog(parent: parent);
        return result switch {
            DialogResult.Ok => Some(dialog.FileName),
            _ => Option<string>.None,
        };
    }

    // SelectFolderDialog : CommonDialog (NOT FileDialog) — separate dispatch (per Eto verification).
    internal static Option<string> RunFolderDialog(Control? parent, Option<string> initialPath) {
        using SelectFolderDialog dialog = new();
        _ = initialPath.Filter(static path => !string.IsNullOrWhiteSpace(path)).IfSome(path => dialog.Directory = path);
        DialogResult result = dialog.ShowDialog(parent: parent);
        return result switch {
            DialogResult.Ok => Some(dialog.Directory),
            _ => Option<string>.None,
        };
    }
}
