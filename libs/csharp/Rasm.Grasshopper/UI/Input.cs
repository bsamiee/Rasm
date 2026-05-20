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
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
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
    public sealed record WriteCase(string Text) : InputClipboardOp;
    public sealed record ClearCase : InputClipboardOp;
    public static readonly InputClipboardOp Read = new ReadCase();
    public static InputClipboardOp Write(string text) => new WriteCase(Text: text);
    public static readonly InputClipboardOp Clear = new ClearCase();
}

// --- [MODELS] ----------------------------------------------------------------------------
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

public readonly record struct FileFilter(string Name, Seq<string> Extensions);

public readonly record struct UiCommand(
    string Name,
    string Info,
    Func<Fin<Unit>> Run,
    Option<IIcon> Icon = default,
    Option<BarShortcut> Shortcut = default,
    bool Enabled = true) {
    internal Fin<UiCommand> Validated(Op op) {
        UiCommand command = this;
        return Optional(command.Run)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "command run delegate is required"))
            .Map(_ => command);
    }

    internal Command ToEtoCommand() {
        string name = Name;
        string info = Info;
        bool enabled = Enabled;
        Func<Fin<Unit>> run = Run;
        Command command = new() { ID = name, MenuText = name, ToolBarText = name, ToolTip = info, Enabled = enabled };
        command.Executed += (_, _) => _ = GrasshopperUi.Protect(valid: run);
        return command;
    }
}

public abstract record ToolbarItem {
    public sealed record Button(UiCommand Command, bool CloseOnActivate = true) : ToolbarItem;
    public sealed record Toggle(UiCommand Command, bool State, Func<bool, Fin<Unit>> Changed) : ToolbarItem;
    public sealed record Spacer(string Chapter, string Section) : ToolbarItem;

    internal Fin<Unit> Apply(Bar bar) =>
        Optional(bar)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(Bar)))
            .Bind(valid => this switch {
                Button button => button.Command.Validated(op: Op.Of(name: nameof(Button))).Map(command => {
                    PushButton pushed = valid.AddPushButton(
                        icon: command.Icon.IfNone(default(IIcon)!),
                        nomen: new Nomen(name: command.Name, info: command.Info),
                        callback: () => _ = GrasshopperUi.Protect(valid: command.Run),
                        keys: command.Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null));
                    pushed.Enabled = command.Enabled;
                    pushed.CloseOnActivate = button.CloseOnActivate;
                    return unit;
                }),
                Toggle toggle =>
                    from command in toggle.Command.Validated(op: Op.Of(name: nameof(Toggle)))
                    from changed in Optional(toggle.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Toggle)), detail: "toggle change delegate is required"))
                    select ((Func<Unit>)(() => {
                        RadioToggle toggled = valid.AddRadioToggle(
                            icon: command.Icon.IfNone(default(IIcon)!),
                            nomen: new Nomen(name: command.Name, info: command.Info),
                            initial: toggle.State,
                            callback: state => _ = GrasshopperUi.Protect(valid: () => changed(arg: state)),
                            keys: command.Shortcut.Map(static shortcut => (BarShortcut?)shortcut).IfNone((BarShortcut?)null));
                        toggled.Enabled = command.Enabled;
                        return unit;
                    }))(),
                Spacer spacer => Try.lift<Unit>(f: () => {
                    _ = valid.AddSpacer(chapterName: spacer.Chapter, sectionName: spacer.Section);
                    return unit;
                }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Spacer)), detail: "AddSpacer threw")),
                _ => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: "unknown toolbar item")),
            });

    internal Fin<Unit> Apply(ContextMenu menu) =>
        Optional(menu)
            .ToFin(Fail: UiFault.MissingScope(field: nameof(ContextMenu)))
            .Bind(valid => this switch {
                Button button => button.Command.Validated(op: Op.Of(name: nameof(Button))).Map(command => {
                    _ = valid.AddItem(text: command.Name, description: command.Info, command: command.ToEtoCommand());
                    return unit;
                }),
                Toggle toggle =>
                    from command in toggle.Command.Validated(op: Op.Of(name: nameof(Toggle)))
                    from changed in Optional(toggle.Changed).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Toggle)), detail: "toggle change delegate is required"))
                    select ((Func<Unit>)(() => {
                        Command menuCommand = command.ToEtoCommand();
                        menuCommand.Executed += (_, _) => _ = GrasshopperUi.Protect(valid: () => changed(arg: !toggle.State));
                        _ = valid.AddItem(text: command.Name, description: command.Info, command: menuCommand);
                        return unit;
                    }))(),
                Spacer => Try.lift<Unit>(f: () => { valid.AddSeparator(); return unit; })
                    .Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Spacer)), detail: "AddSeparator threw")),
                _ => Fin.Fail<Unit>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(ToolbarItem)), detail: "unknown menu item")),
            });
}

public readonly record struct ToolbarPlan(Seq<ToolbarItem> Items);

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
    public sealed record CommandBar(ToolbarPlan Plan) : InputRequest<ToolbarSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<ToolbarSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Toolbar(plan: Plan).Run(scope: scope);
    }
    public sealed record Menu(ToolbarPlan Plan) : InputRequest<MenuSnapshot> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<MenuSnapshot> Apply(GrasshopperUi.Scope scope) => Input.Menu(plan: Plan).Run(scope: scope);
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
        internal override Fin<Option<string>> Apply(GrasshopperUi.Scope scope) => Input.FileDialog(mode: Mode, initialPath: InitialPath.IfNone(string.Empty), filters: [.. Filters]).Run(scope: scope);
    }
    public sealed record Clipboard(InputClipboardOp Op) : InputRequest<Option<string>> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Read;
        internal override Fin<Option<string>> Apply(GrasshopperUi.Scope scope) => Input.Clipboard(op: Op).Run(scope: scope);
    }
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class Input {
    internal static GrasshopperUiIntent<InputSelectionSnapshot> Selection(InputSelectionSource source) =>
        IntentFactory.Read<InputSelectionSnapshot>(run: _ =>
            Optional(source)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Selection)), detail: "null source"))
                .Map(valid => new InputSelectionSnapshot(Mode: valid.Mode(), Modifiers: ModifierOf(keys: Keyboard.Modifiers))));

    internal static GrasshopperUiIntent<InputModifierSnapshot> Modifiers(Keys keys) =>
        IntentFactory.Read<InputModifierSnapshot>(run: _ => Fin.Succ(value: ModifierOf(keys: keys)));

    internal static GrasshopperUiIntent<InputPanelSnapshot> Panel(Func<InputPanel, Fin<Unit>> populate) =>
        IntentFactory.Read<InputPanelSnapshot>(run: _ =>
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
        IntentFactory.Read<InputPanelSnapshot>(run: _ =>
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
        IntentFactory.Read<ToolbarSnapshot>(run: _ =>
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

    internal static GrasshopperUiIntent<ToolbarSnapshot> Toolbar(ToolbarPlan plan) =>
        Toolbar(populate: bar =>
            plan.Items.TraverseM(item => item.Apply(bar: bar)).Map(static _ => unit).As());

    internal static GrasshopperUiIntent<MenuSnapshot> Menu(ToolbarPlan plan) =>
        IntentFactory.Read<MenuSnapshot>(run: _ => {
            using ContextMenu menu = new();
            return plan.Items
                .TraverseM(item => item.Apply(menu: menu))
                .Map(_ => new MenuSnapshot(Count: menu.Items.Count))
                .As();
        });

    internal static GrasshopperUiIntent<CursorKind> Cursor(CursorKind kind) =>
        IntentFactory.Canvas<CursorKind>(run: scope => scope.NeedCanvas().Map(canvas => {
            canvas.Cursor = CursorOf(kind: kind, canvas: canvas);
            return kind;
        }));

    internal static GrasshopperUiIntent<InputDialogResponse> MessageDialog(string title, string message, MessageDialogKind kind = MessageDialogKind.Information, MessageDialogButtons buttons = MessageDialogButtons.Ok) =>
        IntentFactory.Read<InputDialogResponse>(run: _ =>
            Try.lift<InputDialogResponse>(f: () => {
                DialogResult result = MessageBox.Show(
                    text: message,
                    caption: title,
                    buttons: ButtonsOf(buttons),
                    type: TypeOf(kind));
                return ResponseOf(result: result);
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(MessageDialog)), detail: "MessageBox.Show threw")));

    internal static GrasshopperUiIntent<Option<string>> FileDialog(FileDialogMode mode, string? initialPath = null, params FileFilter[] filters) =>
        IntentFactory.Read<Option<string>>(run: _ =>
            Try.lift<Option<string>>(f: () => mode switch {
                FileDialogMode.Open => RunFileDialog(dialog: new OpenFileDialog(), initialPath: initialPath, filters: filters),
                FileDialogMode.Save => RunFileDialog(dialog: new SaveFileDialog(), initialPath: initialPath, filters: filters),
                FileDialogMode.Folder => RunFolderDialog(initialPath: initialPath),
                _ => Option<string>.None,
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(FileDialog)), detail: "FileDialog.ShowDialog threw")));

    internal static GrasshopperUiIntent<Option<string>> Clipboard(InputClipboardOp op) =>
        IntentFactory.Read<Option<string>>(run: _ =>
            Try.lift<Option<string>>(f: () => {
                Func<Option<string>> action = op switch {
                    InputClipboardOp.ReadCase => static () => Optional(Eto.Forms.Clipboard.Instance.Text),
                    InputClipboardOp.WriteCase w => () => Some(Eto.Forms.Clipboard.Instance.Text = w.Text),
                    InputClipboardOp.ClearCase => static () => {
                        Eto.Forms.Clipboard.Instance.Clear();
                        return Option<string>.None;
                    }
                    ,
                    _ => static () => Option<string>.None,
                };
                return action();
            }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Clipboard)), detail: "Clipboard op threw")));

    // --- [OPERATIONS] ----------------------------------------------------------------------
    private static InputModifierSnapshot ModifierOf(Keys keys) =>
        new(Shift: keys.HasShift(), Command: keys.HasCommand(), Option: keys.HasOption());

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
