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
public enum CursorKind {
    Default, Crosshair, Pointer, IBeam, Move,
    VerticalSplit, HorizontalSplit, SizeAll, NotAllowed,
    WireIn, WireOut, WireQuestion,
}

public enum FileDialogMode { Open, Save, Folder }

public enum MessageDialogKind { Information, Warning, Error, Question }

[Flags]
public enum MessageDialogButtons {
    None = 0,
    Ok = 1, Cancel = 2, Yes = 4, No = 8,
    OkCancel = Ok | Cancel,
    YesNo = Yes | No,
    YesNoCancel = Yes | No | Cancel,
}

public enum InputDialogResponse { None, Ok, Cancel, Yes, No, Abort, Ignore, Retry }

[Union]
public partial record InputSelectionSource {
    private InputSelectionSource() { }
    public sealed record ControlCase(Control Source) : InputSelectionSource;
    public sealed record MouseCase(MouseEventArgs Source) : InputSelectionSource;
    public sealed record WindowCase(WindowSelectionEventArgs Source) : InputSelectionSource;

    public static InputSelectionSource From(Control control) => new ControlCase(Source: control);
    public static InputSelectionSource From(MouseEventArgs mouse) => new MouseCase(Source: mouse);
    public static InputSelectionSource From(WindowSelectionEventArgs window) => new WindowCase(Source: window);

    internal SelectionMode Mode() => this switch {
        ControlCase c => c.Source.SelectionMode(),
        MouseCase m => m.Source.SelectionMode(),
        WindowCase w => w.Source.SelectionMode(),
        _ => SelectionMode.Include,
    };
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
    public sealed record Message(string Title, string Body, MessageDialogKind Kind = MessageDialogKind.Information, MessageDialogButtons Buttons = MessageDialogButtons.Ok) : InputRequest<InputDialogResponse> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<InputDialogResponse> Apply(GrasshopperUi.Scope scope) => Input.MessageDialog(title: Title, message: Body, kind: Kind, buttons: Buttons).Run(scope: scope);
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
            canvas.Cursor = CursorOf(kind: kind, canvas: canvas);
            return kind;
        }));

    internal static GrasshopperUiIntent<InputDialogResponse> MessageDialog(string title, string message, MessageDialogKind kind = MessageDialogKind.Information, MessageDialogButtons buttons = MessageDialogButtons.Ok) =>
        GhUi.Read<InputDialogResponse>(run: _ =>
            Try.lift<InputDialogResponse>(f: () => {
                DialogResult result = MessageBox.Show(
                    text: message,
                    caption: title,
                    buttons: ButtonsOf(buttons),
                    type: TypeOf(kind));
                return ResponseOf(result: result);
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(MessageDialog)), detail: "MessageBox.Show threw")));

    internal static GrasshopperUiIntent<Option<string>> FileDialog(FileDialogMode mode, Option<string> initialPath = default, params FileFilter[] filters) =>
        GhUi.Read<Option<string>>(run: _ =>
            Try.lift<Option<string>>(f: () => mode switch {
                FileDialogMode.Open => RunFileDialog(dialog: new OpenFileDialog(), initialPath: PathOrEmpty(path: initialPath), filters: filters),
                FileDialogMode.Save => RunFileDialog(dialog: new SaveFileDialog(), initialPath: PathOrEmpty(path: initialPath), filters: filters),
                FileDialogMode.Folder => RunFolderDialog(initialPath: PathOrEmpty(path: initialPath)),
                _ => Option<string>.None,
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(FileDialog)), detail: "FileDialog.ShowDialog threw")));

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

    private static string PathOrEmpty(Option<string> path) =>
        path.Filter(static value => !string.IsNullOrWhiteSpace(value)).IfNone(string.Empty);

    private static Eto.Forms.Cursor CursorOf(CursorKind kind, Grasshopper2.UI.Canvas.Canvas canvas) {
        _ = canvas;
        return kind switch {
            CursorKind.Default => Cursors.Default,
            CursorKind.Crosshair => Cursors.Crosshair,
            CursorKind.Pointer => Cursors.Pointer,
            CursorKind.IBeam => Cursors.IBeam,
            CursorKind.Move => Cursors.Move,
            CursorKind.VerticalSplit => Cursors.VerticalSplit,
            CursorKind.HorizontalSplit => Cursors.HorizontalSplit,
            CursorKind.SizeAll => Cursors.SizeAll,
            CursorKind.NotAllowed => Cursors.NotAllowed,
            CursorKind.WireIn => Grasshopper2.UI.Canvas.Canvas.CursorWireIn ?? Cursors.Pointer,
            CursorKind.WireOut => Grasshopper2.UI.Canvas.Canvas.CursorWireOut ?? Cursors.Pointer,
            CursorKind.WireQuestion => Grasshopper2.UI.Canvas.Canvas.CursorQuestion ?? Cursors.Default,
            _ => Cursors.Default,
        };
    }

    private static MessageBoxType TypeOf(MessageDialogKind k) => k switch {
        MessageDialogKind.Information => MessageBoxType.Information,
        MessageDialogKind.Warning => MessageBoxType.Warning,
        MessageDialogKind.Error => MessageBoxType.Error,
        MessageDialogKind.Question => MessageBoxType.Question,
        _ => MessageBoxType.Information,
    };

    private static MessageBoxButtons ButtonsOf(MessageDialogButtons b) => b switch {
        MessageDialogButtons.Ok => MessageBoxButtons.OK,
        MessageDialogButtons.OkCancel => MessageBoxButtons.OKCancel,
        MessageDialogButtons.YesNo => MessageBoxButtons.YesNo,
        MessageDialogButtons.YesNoCancel => MessageBoxButtons.YesNoCancel,
        MessageDialogButtons value when (value & MessageDialogButtons.Ok) == MessageDialogButtons.Ok && (value & MessageDialogButtons.Cancel) == MessageDialogButtons.Cancel => MessageBoxButtons.OKCancel,
        MessageDialogButtons value when (value & MessageDialogButtons.Yes) == MessageDialogButtons.Yes && (value & MessageDialogButtons.No) == MessageDialogButtons.No => MessageBoxButtons.YesNo,
        _ => MessageBoxButtons.OK,
    };

    private static InputDialogResponse ResponseOf(DialogResult result) => result switch {
        DialogResult.None => InputDialogResponse.None,
        DialogResult.Ok => InputDialogResponse.Ok,
        DialogResult.Cancel => InputDialogResponse.Cancel,
        DialogResult.Yes => InputDialogResponse.Yes,
        DialogResult.No => InputDialogResponse.No,
        DialogResult.Abort => InputDialogResponse.Abort,
        DialogResult.Ignore => InputDialogResponse.Ignore,
        DialogResult.Retry => InputDialogResponse.Retry,
        _ => InputDialogResponse.None,
    };

    private static Option<string> RunFileDialog(Eto.Forms.FileDialog dialog, string? initialPath, FileFilter[] filters) {
        _ = Optional(initialPath).Filter(static path => !string.IsNullOrWhiteSpace(path)).IfSome(path => dialog.FileName = path);
        _ = toSeq(filters).Iter(f => dialog.Filters.Add(item: new Eto.Forms.FileFilter(name: f.Name, extensions: [.. f.Extensions])));
        DialogResult result = dialog.ShowDialog(parent: (Control?)null);
        return result switch {
            DialogResult.Ok => Some(dialog.FileName),
            _ => Option<string>.None,
        };
    }

    // SelectFolderDialog : CommonDialog (NOT FileDialog) — separate dispatch (per Eto verification).
    private static Option<string> RunFolderDialog(string? initialPath) {
        using SelectFolderDialog dialog = new();
        _ = Optional(initialPath).Filter(static path => !string.IsNullOrWhiteSpace(path)).IfSome(path => dialog.Directory = path);
        DialogResult result = dialog.ShowDialog(parent: (Control?)null);
        return result switch {
            DialogResult.Ok => Some(dialog.Directory),
            _ => Option<string>.None,
        };
    }
}
