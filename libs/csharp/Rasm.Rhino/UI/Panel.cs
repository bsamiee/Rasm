using Eto.Forms;
using Rasm.Rhino.Events;
using DrawingIcon = System.Drawing.Icon;
using DrawingSize = System.Drawing.Size;
using GuidAttribute = System.Runtime.InteropServices.GuidAttribute;
using ReflectionAssembly = System.Reflection.Assembly;
using RhinoControls = Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public readonly record struct PanelEvent(PanelEventKind Kind, Guid Id, Option<uint> DocumentSerial = default);

[SmartEnum<int>]
public sealed partial class PanelEventKind {
    public static readonly PanelEventKind None = new(key: 0, emit: static (_, _) => Fin.Fail<Unit>(error: Op.Of(name: nameof(PanelEventKind)).InvalidInput()));
    public static readonly PanelEventKind Shown = new(key: 1, emit: static (panelId, serial) => Fin.Succ(value: Op.Side(() => global::Rhino.UI.Panels.OnShowPanel(panelId: panelId, documentSerialNumber: serial.IfNone(0u), show: true))));
    public static readonly PanelEventKind Hidden = new(key: 2, emit: static (panelId, serial) => Fin.Succ(value: Op.Side(() => global::Rhino.UI.Panels.OnShowPanel(panelId: panelId, documentSerialNumber: serial.IfNone(0u), show: false))));
    public static readonly PanelEventKind Closed = new(key: 3, emit: static (panelId, serial) => Fin.Succ(value: Op.Side(() => global::Rhino.UI.Panels.OnClosePanel(panelId: panelId, documentSerialNumber: serial.IfNone(0u)))));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Emit(Guid panelId, Option<uint> documentSerial);
}

public abstract record PanelIcon {
    private PanelIcon() { }
    public sealed record Native(DrawingIcon Value) : PanelIcon;
    public sealed record StandardResource(string Name) : PanelIcon;
    public sealed record AssemblyResource(string Name, ReflectionAssembly Assembly) : PanelIcon;
    public static PanelIcon Of(DrawingIcon value) => new Native(Value: value);
    public static PanelIcon Resource(string name, ReflectionAssembly? assembly = null) =>
        assembly is ReflectionAssembly active ? new AssemblyResource(Name: name, Assembly: active) : new StandardResource(Name: name);
    public static PanelIcon Embedded<TAnchor>(string name) => new AssemblyResource(Name: name, Assembly: typeof(TAnchor).Assembly);
    public static PanelIcon CallingAssembly(string name) => new AssemblyResource(Name: name, Assembly: ReflectionAssembly.GetCallingAssembly());
    internal Fin<Unit> Register(global::Rhino.PlugIns.PlugIn plugin, Type type, string caption, global::Rhino.UI.PanelType mode) {
        Op op = Op.Of(name: nameof(Register));
        return this switch {
            Native icon => op.Need(icon.Value).Bind(value => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, value, mode); return Fin.Succ(value: unit); })),
            AssemblyResource resource => op.AcceptText(value: resource.Name).MapFail(_ => op.InvalidInput()).Bind(name => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, resource.Assembly, name, mode); return Fin.Succ(value: unit); })),
            StandardResource resource => op.AcceptText(value: resource.Name).MapFail(_ => op.InvalidInput()).Bind(name => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.RegisterPanel(plugIn: plugin, type: type, caption: caption, iconAssembly: null, iconResourceId: name, panelType: mode); return Fin.Succ(value: unit); })),
            _ => Fin.Fail<Unit>(error: op.InvalidInput()),
        };
    }

    internal Fin<Unit> Change(Type type) {
        Op op = Op.Of(name: nameof(Change));
        return this switch {
            Native icon => op.Need(icon.Value).Bind(value => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, value); return Fin.Succ(value: unit); })),
            AssemblyResource resource => op.AcceptText(value: resource.Name).MapFail(_ => op.InvalidInput()).Bind(name => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, name); return Fin.Succ(value: unit); })),
            StandardResource resource => op.AcceptText(value: resource.Name).MapFail(_ => op.InvalidInput()).Bind(name => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, name); return Fin.Succ(value: unit); })),
            _ => Fin.Fail<Unit>(error: op.InvalidInput()),
        };
    }
}

[Union(SwitchMapStateParameterName = "context")]
public abstract partial record PanelPlacement {
    private PanelPlacement() { }
    public sealed record Auto : PanelPlacement;
    public sealed record AtDockBar(Guid DockBarId) : PanelPlacement;
    public sealed record AsSibling(Guid SiblingPanelId) : PanelPlacement;

    public static PanelPlacement Default { get; } = new Auto();
    public static Fin<PanelPlacement> Dock(Guid dockBarId) =>
        guard(dockBarId != Guid.Empty, Op.Of(name: nameof(Dock)).InvalidInput()).ToFin().Map(_ => (PanelPlacement)new AtDockBar(dockBarId));
    public static Fin<PanelPlacement> Sibling(Guid panelId) =>
        guard(panelId != Guid.Empty, Op.Of(name: nameof(Sibling)).InvalidInput()).ToFin().Map(_ => (PanelPlacement)new AsSibling(panelId));

    internal Fin<Unit> Open(Type panelType, Guid panelId, bool selected) =>
        RhinoUi.Protect(valid: () => Switch<(Type Type, Guid Id, bool Selected), Fin<Unit>>(
            (panelType, panelId, selected),
            auto: static (ctx, _) => {
                global::Rhino.UI.Panels.OpenPanel(panelType: ctx.Type, makeSelectedPanel: ctx.Selected);
                return Fin.Succ(value: unit);
            },
            atDockBar: static (ctx, dock) => global::Rhino.UI.Panels.OpenPanel(dockBarId: dock.DockBarId, panelType: ctx.Type, makeSelectedPanel: ctx.Selected) switch {
                Guid id when id != Guid.Empty || OperatingSystem.IsMacOS() => Fin.Succ(value: unit),
                _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()),
            },
            asSibling: static (ctx, sibling) => Op.Of(name: nameof(Open)).Confirm(success: global::Rhino.UI.Panels.OpenPanelAsSibling(panelId: ctx.Id, siblingPanelId: sibling.SiblingPanelId, makeSelectedPanel: ctx.Selected))));
}

public enum UiActionKind { Callback, Command, Separator, Submenu, Toggle, Radio }

public enum UiChromeFileMode { Open, Close, Save, SaveAs }

public abstract record UiChromeOp<T> {
    private UiChromeOp() { }

    internal virtual bool Interactive => true;
    internal abstract Fin<T> Run(RhinoDoc? document);

    public sealed record EtoToolbar(IEnumerable<UiAction> Actions) : UiChromeOp<ToolBar> {
        internal override Fin<ToolBar> Run(RhinoDoc? document) =>
            BuildBar(Actions, document, nameof(EtoToolbar), static () => new ToolBar(), static (action, doc) => action.ToToolItems(document: doc), static (bar, item) => bar.Items.Add(item));
    }

    public sealed record EtoMenu(IEnumerable<UiAction> Actions) : UiChromeOp<MenuBar> {
        internal override Fin<MenuBar> Run(RhinoDoc? document) =>
            BuildBar(Actions, document, nameof(EtoMenu), static () => new MenuBar(), static (action, doc) => action.ToMenuItems(document: doc), static (bar, item) => bar.Items.Add(item));
    }

    public sealed record RuiSnapshot : UiChromeOp<PanelChromeSnapshot> {
        internal override bool Interactive => false;

        internal override Fin<PanelChromeSnapshot> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: Snapshot);
    }

    public sealed record RuiFile(UiChromeFileMode Mode, Option<Guid> FileId = default, Option<string> Path = default, Option<string> Name = default, bool IgnoreCase = true, bool Prompt = false, bool SaveAfterOpen = false) : UiChromeOp<PanelChromeSnapshot> {
        internal override bool Interactive => Mode == UiChromeFileMode.Close && Prompt;

        internal override Fin<PanelChromeSnapshot> Run(RhinoDoc? document) {
            Op op = Op.Of(name: nameof(RuiFile));
            return RhinoUi.Protect(valid: () =>
                from _ in Mode switch {
                    UiChromeFileMode.Open => from path in NonBlank(value: Path)
                                             from file in Optional(RhinoApp.ToolbarFiles.Open(path: path)).ToFin(Fail: op.InvalidResult())
                                             from saved in SaveAfterOpen ? op.Confirm(file.Save()) : Fin.Succ(value: unit)
                                             select saved,
                    UiChromeFileMode.Close => from file in FindFile(fileId: FileId, path: Path, name: Name, ignoreCase: IgnoreCase)
                                              from closed in op.Confirm(file.Close(prompt: Prompt))
                                              select closed,
                    UiChromeFileMode.Save => from file in FindFile(fileId: FileId, path: Path, name: Name, ignoreCase: IgnoreCase)
                                             from saved in op.Confirm(file.Save())
                                             select saved,
                    UiChromeFileMode.SaveAs => from file in FindFile(fileId: FileId, path: Option<string>.None, name: Name, ignoreCase: IgnoreCase)
                                               from path in NonBlank(value: Path)
                                               from saved in op.Confirm(file.SaveAs(path: path))
                                               select saved,
                    _ => Fin.Fail<Unit>(error: op.InvalidInput()),
                }
                from snapshot in Snapshot()
                select snapshot);
        }
    }

    public sealed record RuiGroup(Guid FileId, Guid GroupId, bool Visible) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () =>
                from file in FindFile(fileId: Some(FileId), path: Option<string>.None, name: Option<string>.None, ignoreCase: true)
                from activeGroup in Op.Of(name: nameof(RuiGroup)).Need(toSeq(Enumerable.Range(start: 0, count: file.GroupCount)).Choose(index => Optional(file.GetGroup(index))).Find(candidate => candidate.Id == GroupId))
                select Op.Side(() => activeGroup.Visible = Visible));
    }

    public sealed record RuiSidebar(bool Visible, bool Mru = false) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () => Fin.Succ(value: Mru switch {
                true => Op.Side(() => global::Rhino.UI.ToolbarFileCollection.MruSidebarIsVisible = Visible),
                false => Op.Side(() => global::Rhino.UI.ToolbarFileCollection.SidebarIsVisible = Visible),
            }));
    }

    public sealed record RuiToolbarSize(Option<DrawingSize> Bitmap = default, Option<DrawingSize> Tab = default) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () => Seq<(Option<DrawingSize> Size, Func<DrawingSize, Unit> Set)>(
                (Size: Bitmap, Set: v => Op.Side(() => global::Rhino.UI.Toolbar.BitmapSize = v)),
                (Size: Tab, Set: v => Op.Side(() => global::Rhino.UI.Toolbar.TabSize = v)))
                .Choose(row => row.Size.Filter(static size => size is { Width: > 0, Height: > 0 }).Map(size => (Size: size, row.Set))) switch {
                    Seq<(DrawingSize Size, Func<DrawingSize, Unit> Set)> rows when !rows.IsEmpty => Fin.Succ(value: Op.Side(() => rows.Iter(row => row.Set(row.Size)))),
                    _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiToolbarSize)).InvalidInput()),
                });
    }
    public sealed record Batch(IEnumerable<UiChromeOp<Unit>> Ops) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            Op.Of(name: nameof(Batch)).Need(Ops).Bind(src => toSeq(src).TraverseM(op => op.Run(document: document)).As().Map(static _ => unit));
    }

    private static Fin<TBar> BuildBar<TBar, TItem>(IEnumerable<UiAction>? actions, RhinoDoc? document, string opName, Func<TBar> create, Func<UiAction, RhinoDoc, Fin<Seq<TItem>>> render, Action<TBar, TItem> add) where TBar : class =>
        from validDocument in Op.Of(name: opName).Need(document)
        from validActions in Op.Of(name: opName).Need(actions).Map(static values => toSeq(values))
        from validRender in Op.Of(name: opName).Need(render)
        from items in validActions.TraverseM(action => validRender(arg1: action, arg2: validDocument)).As().Map(static groups => groups.Bind(static item => item))
        select ((Func<TBar>)(() => { TBar bar = create(); _ = items.Iter(item => add(arg1: bar, arg2: item)); return bar; }))();

    private static Fin<global::Rhino.UI.ToolbarFile> FindFile(Option<Guid> fileId, Option<string> path, Option<string> name, bool ignoreCase) =>
        Op.Of(name: nameof(FindFile)).Need(
            fileId.Bind(id => toSeq(RhinoApp.ToolbarFiles).Choose(Optional).Find(file => file.Id == id)) |
            path.Bind(value => Optional(RhinoApp.ToolbarFiles.FindByPath(path: value))) |
            name.Bind(value => Optional(RhinoApp.ToolbarFiles.FindByName(name: value, ignoreCase: ignoreCase))));

    private static Fin<string> NonBlank(Option<string> value) =>
        Op.Of(name: nameof(NonBlank)).Need(value)
          .Bind(text => Op.Of(name: nameof(NonBlank)).AcceptText(value: text)
              .MapFail(_ => Op.Of(name: nameof(NonBlank)).InvalidInput()));

    private static Fin<PanelChromeSnapshot> Snapshot() {
        Seq<global::Rhino.UI.ToolbarFile> files = toSeq(RhinoApp.ToolbarFiles).Choose(Optional);
        Seq<(Guid Id, string Name, string Path, int GroupCount, int ToolbarCount)> fileRows =
            files.Map(static file => (file.Id, file.Name, file.Path, file.GroupCount, file.ToolbarCount));
        Seq<(Guid FileId, Guid GroupId, string Name, bool Visible, bool IsDocked)> groupRows =
            files.Bind(file => toSeq(Enumerable.Range(start: 0, count: file.GroupCount))
                .Choose(index => Optional(file.GetGroup(index)).Map(group => (file.Id, group.Id, group.Name, group.Visible, group.IsDocked))));
        Seq<(Guid FileId, Guid ToolbarId, string Name)> toolbarRows =
            files.Bind(file => toSeq(Enumerable.Range(start: 0, count: file.ToolbarCount))
                .Choose(index => Optional(file.GetToolbar(index)).Map(toolbar => (file.Id, toolbar.Id, toolbar.Name))));
        return Fin.Succ(value: new PanelChromeSnapshot(
            Files: fileRows,
            Groups: groupRows,
            Toolbars: toolbarRows,
            SidebarVisible: global::Rhino.UI.ToolbarFileCollection.SidebarIsVisible,
            MruSidebarVisible: global::Rhino.UI.ToolbarFileCollection.MruSidebarIsVisible));
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct PanelChromeSnapshot(
    Seq<(Guid Id, string Name, string Path, int GroupCount, int ToolbarCount)> Files,
    Seq<(Guid FileId, Guid GroupId, string Name, bool Visible, bool IsDocked)> Groups,
    Seq<(Guid FileId, Guid ToolbarId, string Name)> Toolbars,
    bool SidebarVisible,
    bool MruSidebarVisible);

public readonly record struct PanelMenuState(
    bool Enabled = true,
    bool Checked = false,
    bool RadioChecked = false,
    Option<string> Text = default,
    Guid FileId = default,
    Guid MenuId = default,
    Guid ItemId = default,
    nint MenuHandle = default,
    int MenuIndex = -1,
    uint WindowsMenuItemId = 0) {
    private static readonly Atom<HashMap<(Guid File, Guid Menu, Guid Item), Func<PanelMenuState, Fin<PanelMenuState>>>> MenuUpdates =
        Atom(HashMap<(Guid File, Guid Menu, Guid Item), Func<PanelMenuState, Fin<PanelMenuState>>>());
    private static readonly Lock MenuGate = new();

    private static Option<PanelMenuState> Of(global::Rhino.UI.RuiUpdateUi ui) => Optional(ui).Bind(static valid => (valid.FileId, valid.MenuId, valid.MenuItemId) switch { (Guid file, Guid menu, Guid item) when file != Guid.Empty && menu != Guid.Empty && item != Guid.Empty => Some(new PanelMenuState(Enabled: valid.Enabled, Checked: valid.Checked, RadioChecked: valid.RadioChecked, Text: Optional(valid.Text), FileId: valid.FileId, MenuId: valid.MenuId, ItemId: valid.MenuItemId, MenuHandle: valid.MenuHandle, MenuIndex: valid.MenuIndex, WindowsMenuItemId: valid.WindowsMenuItemId)), _ => Option<PanelMenuState>.None });

    private Unit Apply(global::Rhino.UI.RuiUpdateUi ui) { ui.Enabled = Enabled; ui.Checked = Checked; ui.RadioChecked = RadioChecked; _ = Text.Iter(value => ui.Text = value); return unit; }
    internal static Fin<Unit> Bind(global::Rhino.UI.RuiUpdateUi ui, Func<PanelMenuState, Fin<PanelMenuState>> update) =>
        from source in Op.Of(name: nameof(Bind)).Need(Of(ui: ui))
        from state in Op.Of(name: nameof(Bind)).Need(update).Bind(valid => valid(arg: source))
        select state.Apply(ui: ui);
    internal static Fin<Unit> BindMany(Seq<(Guid File, Guid Menu, Guid Item)> ids, Func<PanelMenuState, Fin<PanelMenuState>> update) =>
        Op.Of(name: nameof(BindMany)).Need(update).Bind(valid =>
            ids.Filter(static id => id.File != Guid.Empty && id.Menu != Guid.Empty && id.Item != Guid.Empty)
                .TraverseM(id => RhinoUi.Protect(valid: () => BindOne(id: id, update: valid))).As().Map(static _ => unit));

    private static Fin<Unit> BindOne((Guid File, Guid Menu, Guid Item) id, Func<PanelMenuState, Fin<PanelMenuState>> update) {
        using Lock.Scope scope = MenuGate.EnterScope();
        bool first = MenuUpdates.Value.Find(id).IsNone;
        _ = MenuUpdates.Swap(state => state.AddOrUpdate(key: id, value: update));
        return first switch {
            false => Fin.Succ(value: unit),
            true => global::Rhino.UI.RuiUpdateUi.RegisterMenuItem(id.File, id.Menu, id.Item,
                (_, ui) => _ = MenuUpdates.Value.Find(id).Iter(handler => _ = RhinoUi.Protect(valid: () => Bind(ui: ui, update: handler)))) switch {
                    true => Fin.Succ(value: unit),
                    false => Fin.Fail<Unit>(error: Op.Of(name: nameof(BindMany)).InvalidResult()).MapFail(error => { _ = MenuUpdates.Swap(state => state.Remove(id)); return error; }),
                },
        };
    }
}

public readonly record struct PanelSnapshot<TPanel>(
    Guid PanelId,
    bool Visible,
    bool Selected,
    Seq<TPanel> Instances,
    Seq<Guid> OpenPanelIds,
    Option<Guid> DockBarId,
    Seq<Guid> DockBarIds) where TPanel : RasmPanel;

public sealed record UiAction(
    string Id,
    string Text,
    Option<string> ToolTip,
    Option<Eto.Drawing.Image> Image,
    Func<RhinoDoc, Fin<Unit>> Run,
    UiActionKind Kind = UiActionKind.Callback,
    Seq<UiAction> Children = default,
    Option<Func<RhinoDoc, Fin<PanelMenuState>>> State = default) {
    public static UiAction Callback(string id, string text, Func<RhinoDoc, Fin<Unit>> run, Option<string> tooltip = default, Option<Eto.Drawing.Image> image = default) =>
        new(Id: id, Text: text, ToolTip: tooltip, Image: image, Run: run);

    public static UiAction Command<TCommand>(
        string text,
        Option<string> tooltip = default,
        Option<Eto.Drawing.Image> image = default) where TCommand : global::Rhino.Commands.Command =>
        new(
            Id: typeof(TCommand).Name,
            Text: text,
            ToolTip: tooltip,
            Image: image,
            Run: document =>
                from active in Op.Of(name: nameof(UiAction)).Need(document)
                let command = typeof(TCommand).Name
                from _ in guard(global::Rhino.Commands.Command.LookupCommandId(name: command, searchForEnglishName: true) != Guid.Empty, Op.Of(name: nameof(UiAction)).InvalidInput())
                from __ in guard(RhinoApp.RunScript(documentSerialNumber: active.RuntimeSerialNumber, script: $"_{command}", echo: false), Op.Of(name: nameof(UiAction)).InvalidResult())
                select unit,
            Kind: UiActionKind.Command);

    public static UiAction Separator(string id = "separator") =>
        new(Id: id, Text: "-", ToolTip: Option<string>.None, Image: Option<Eto.Drawing.Image>.None, Run: _ => Fin.Succ(value: unit), Kind: UiActionKind.Separator);

    public static UiAction Submenu(string id, string text, Seq<UiAction> children, Option<string> tooltip = default, Option<Eto.Drawing.Image> image = default) =>
        new(Id: id, Text: text, ToolTip: tooltip, Image: image, Run: _ => Fin.Succ(value: unit), Kind: UiActionKind.Submenu, Children: children);

    public UiAction Toggle(Func<RhinoDoc, Fin<PanelMenuState>> state) =>
        this with { Kind = UiActionKind.Toggle, State = Some(state) };

    public UiAction Radio(Func<RhinoDoc, Fin<PanelMenuState>> state) =>
        this with { Kind = UiActionKind.Radio, State = Some(state) };

    internal Fin<Seq<MenuItem>> ToMenuItems(RhinoDoc document) =>
        Kind switch {
            UiActionKind.Separator => Fin.Succ(value: MenuSeparator()),
            UiActionKind.Submenu => ToSubMenu(document: document).Map(OneMenu),
            UiActionKind.Toggle => ToCheckMenu(document: document, radio: false).Map(OneMenu),
            UiActionKind.Radio => ToCheckMenu(document: document, radio: true).Map(OneMenu),
            _ => ToCommand(document: document).Map(command => Seq(command.CreateMenuItem())),
        };

    internal Fin<Seq<ToolItem>> ToToolItems(RhinoDoc document) =>
        Kind switch {
            UiActionKind.Separator => Fin.Succ(value: ToolSeparator()),
            UiActionKind.Submenu => Children.TraverseM(child => child.ToToolItems(document: document)).As().Map(static groups => groups.Bind(static item => item)),
            UiActionKind.Toggle or UiActionKind.Radio => ToCheckTool(document: document).Map(OneTool),
            _ => ToCommand(document: document).Map(command => Seq(command.CreateToolItem())),
        };

    internal Fin<Eto.Forms.Command> ToCommand(RhinoDoc document) =>
        from validDocument in Op.Of(name: nameof(ToCommand)).Need(document)
        from action in Op.Of(name: nameof(ToCommand)).Need(Run)
        from id in Op.Of(name: nameof(ToCommand)).AcceptText(value: Id).MapFail(_ => Op.Of(name: nameof(ToCommand)).InvalidInput())
        from text in Op.Of(name: nameof(ToCommand)).AcceptText(value: Text).MapFail(_ => Op.Of(name: nameof(ToCommand)).InvalidInput())
        select ((Func<Eto.Forms.Command>)(() => {
            Eto.Forms.Command command = new() { ID = id, MenuText = text, ToolBarText = text, ToolTip = ToolTip.IfNone(text), Enabled = Kind != UiActionKind.Separator };
            _ = Image.Iter(active => command.Image = active);
            command.Executed += (_, _) => _ = RhinoUi.Protect(valid: () => action(arg: validDocument));
            return command;
        }))();

    private Fin<ButtonMenuItem> ToSubMenu(RhinoDoc document) =>
        from id in Op.Of(name: nameof(ToSubMenu)).AcceptText(value: Id).MapFail(_ => Op.Of(name: nameof(ToSubMenu)).InvalidInput())
        from text in Op.Of(name: nameof(ToSubMenu)).AcceptText(value: Text).MapFail(_ => Op.Of(name: nameof(ToSubMenu)).InvalidInput())
        from children in Children.TraverseM(child => child.ToMenuItems(document: document)).As().Map(static groups => groups.Bind(static item => item))
        select ((Func<ButtonMenuItem>)(() => {
            ButtonMenuItem item = new() { ID = id, Text = text, ToolTip = ToolTip.IfNone(text) };
            _ = Image.Iter(active => item.Image = active);
            _ = children.Iter(item.Items.Add);
            BindState(document: document, item: item);
            return item;
        }))();

    private Fin<MenuItem> ToCheckMenu(RhinoDoc document, bool radio) =>
        from command in ToCommand(document: document)
        select ((Func<MenuItem>)(() => {
            MenuItem item = radio ? new RadioMenuItem() : new CheckMenuItem();
            item.ID = command.ID;
            item.Text = command.MenuText;
            item.ToolTip = command.ToolTip;
            item.Enabled = command.Enabled;
            if (item is ButtonMenuItem button) button.Image = command.Image;
            item.Click += (_, _) => command.Execute();
            BindState(document: document, item: item);
            return item;
        }))();

    private Fin<ToolItem> ToCheckTool(RhinoDoc document) =>
        from command in ToCommand(document: document)
        select ((Func<ToolItem>)(() => {
            CheckToolItem item = new() { Text = command.ToolBarText, ToolTip = command.ToolTip, Image = command.Image, Enabled = command.Enabled };
            item.Click += (_, _) => command.Execute();
            ApplyState(document: document, apply: state => {
                item.Enabled = state.Enabled;
                item.Checked = state.Checked || state.RadioChecked;
                _ = state.Text.Iter(value => item.Text = value);
            });
            return item;
        }))();

    private void BindState(RhinoDoc document, MenuItem item) {
        ApplyState(document: document, apply: state => ApplyState(item: item, state: state));
        item.Validate += (_, _) => ApplyState(document: document, apply: state => ApplyState(item: item, state: state));
    }

    private void ApplyState(RhinoDoc document, Action<PanelMenuState> apply) =>
        _ = State.Iter(project => _ = RhinoUi.Protect(valid: () => project(arg: document).Map(state => Op.Side(() => apply(state)))));

    private static Unit ApplyState(MenuItem item, PanelMenuState state) {
        item.Enabled = state.Enabled;
        _ = state.Text.Iter(value => item.Text = value);
        if (item is CheckMenuItem check) check.Checked = state.Checked;
        if (item is RadioMenuItem radio) radio.Checked = state.RadioChecked || state.Checked;
        return unit;
    }

    private static Seq<MenuItem> OneMenu(MenuItem item) => Seq(item);

    private static Seq<ToolItem> OneTool(ToolItem item) => Seq(item);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Eto menu owns item disposal after attachment.")]
    private static Seq<MenuItem> MenuSeparator() => Seq<MenuItem>(new SeparatorMenuItem());

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Eto toolbar owns item disposal after attachment.")]
    private static Seq<ToolItem> ToolSeparator() => Seq<ToolItem>(new SeparatorToolItem());
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmPanel : Panel, global::Rhino.UI.IPanel {
    protected enum PanelPhase { Shown, Hidden, Closing }

    protected readonly record struct PanelContext(PanelPhase Phase, uint DocumentSerialNumber, global::Rhino.UI.ShowPanelReason Reason = default, bool OnCloseDocument = false);

    protected RasmPanel() => global::Rhino.UI.EtoExtensions.UseRhinoStyle(this);

    public void PanelShown(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) => Apply(phase: PanelPhase.Shown, documentSerialNumber: documentSerialNumber, reason: reason);
    public void PanelHidden(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) => Apply(phase: PanelPhase.Hidden, documentSerialNumber: documentSerialNumber, reason: reason);
    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => Apply(phase: PanelPhase.Closing, documentSerialNumber: documentSerialNumber, onCloseDocument: onCloseDocument);
    protected virtual Fin<Unit> Change(PanelContext context) => context switch {
        { Phase: PanelPhase.Shown } when global::Rhino.UI.Panels.IsShowing(reason: context.Reason) => OnReveal(context: context),
        { Phase: PanelPhase.Hidden } when global::Rhino.UI.Panels.IsHiding(reason: context.Reason) => OnConceal(context: context),
        _ => Fin.Succ(value: unit),
    };
    protected virtual Fin<Unit> OnReveal(PanelContext context) => Fin.Succ(value: unit);
    protected virtual Fin<Unit> OnConceal(PanelContext context) => Fin.Succ(value: unit);

    private Unit Apply(PanelPhase phase, uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason = default, bool onCloseDocument = false) {
        _ = RhinoUi.Protect(valid: () => Change(context: new PanelContext(Phase: phase, DocumentSerialNumber: documentSerialNumber, Reason: reason, OnCloseDocument: onCloseDocument)));
        return unit;
    }

    internal readonly record struct PanelIdentity(Type Type, Guid Id);

    internal static Fin<PanelIdentity> PanelIdentityOf<TPanel>() where TPanel : RasmPanel {
        Type type = typeof(TPanel);
        Op op = Op.Of(name: nameof(PanelIdentityOf));
        return (type.GetConstructor(types: [typeof(uint)]) is not null || type.GetConstructor(types: Type.EmptyTypes) is not null, type.GetCustomAttributes(attributeType: typeof(GuidAttribute), inherit: false).FirstOrDefault()) switch { (false, _) => Fin.Fail<PanelIdentity>(error: op.Unsupported(geometryType: type, outputType: typeof(PanelIdentity))), (true, GuidAttribute attribute) when Guid.TryParse(input: attribute.Value, result: out Guid id) && id != Guid.Empty => Fin.Succ(value: new PanelIdentity(Type: type, Id: id)), _ => Fin.Fail<PanelIdentity>(error: op.InvalidInput()) };
    }
}

public sealed class RasmSection : RhinoControls.EtoCollapsibleSection {
    private readonly global::Rhino.UI.LocalizeStringPair caption;
    private readonly int height;
    private readonly bool expanded;
    private readonly bool collapsible;
    private readonly Func<bool> hidden;
    private readonly global::Rhino.UI.LocalizeStringPair commandOption;

    private RasmSection(global::Rhino.UI.LocalizeStringPair caption, int height, Control content, bool expanded, bool collapsible, Func<bool> hidden, global::Rhino.UI.LocalizeStringPair commandOption) {
        this.caption = caption;
        this.height = height;
        this.expanded = expanded;
        this.collapsible = collapsible;
        this.hidden = hidden;
        this.commandOption = commandOption;
        Content = content;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(content);
    }

    public override global::Rhino.UI.LocalizeStringPair Caption => caption;
    public override int SectionHeight => height;
    public override bool InitiallyExpanded => expanded;
    public override bool Collapsible => collapsible;
    public override bool Hidden => hidden();   // Func<bool> backs the per-paint visibility poll
    public override global::Rhino.UI.LocalizeStringPair CommandOptionName => commandOption;

    public static Fin<RasmSection> Of(string caption, int sectionHeight, Control content, bool expanded = true, Option<string> local = default, bool collapsible = true, Func<bool>? hidden = null, Option<string> commandOption = default) =>
        from text in Op.Of(name: nameof(RasmSection)).AcceptText(value: caption).MapFail(_ => Op.Of(name: nameof(RasmSection)).InvalidInput())
        from valid in Op.Of(name: nameof(RasmSection)).Need(content)
        from _ in guard(sectionHeight > 0, Op.Of(name: nameof(RasmSection)).InvalidInput())
        select new RasmSection(
            caption: new global::Rhino.UI.LocalizeStringPair(text, local.IfNone(text)),
            height: sectionHeight, content: valid, expanded: expanded,
            collapsible: collapsible,
            hidden: hidden ?? (static () => false),
            commandOption: commandOption.Map(static value => new global::Rhino.UI.LocalizeStringPair(value, value)).IfNone(() => new global::Rhino.UI.LocalizeStringPair("", "")));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class PanelOp {
    private static UiIntent<T> Bind<TPanel, T>(Func<RhinoUi.Scope, RasmPanel.PanelIdentity, Fin<T>> run, bool interactive) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: scope => RasmPanel.PanelIdentityOf<TPanel>().Bind(panel => run(arg1: scope, arg2: panel)), interactive: interactive);

    public static UiIntent<T> Chrome<TPanel, T>(UiChromeOp<T> operation) where TPanel : RasmPanel =>
        UiIntent.OfScope(
            run: scope =>
                from valid in Op.Of(name: nameof(Chrome)).Need(operation)
                from result in valid.Run(document: scope.Document)
                select result,
            interactive: Optional(operation).Map(static valid => valid.Interactive).IfNone(noneValue: true));

    public static Fin<RhinoControls.EtoCollapsibleSectionHolder> Sections(IEnumerable<RasmSection> sections, bool scrollbars = true) =>
        Op.Of(name: nameof(Sections)).Need(sections).Bind(src => toSeq(src) switch {
            Seq<RasmSection> items when !items.IsEmpty => RhinoUi.Protect(valid: () => {
                RhinoControls.EtoCollapsibleSectionHolder holder = new() { UseScrollbars = scrollbars };
                _ = items.Iter(section => holder.Add(section: section));
                return Fin.Succ(value: holder);
            }),
            _ => Fin.Fail<RhinoControls.EtoCollapsibleSectionHolder>(error: Op.Of(name: nameof(Sections)).InvalidResult()),
        });

    public static UiIntent<Unit> Register<TPanel>(
        global::Rhino.PlugIns.PlugIn plugin,
        string caption,
        PanelIcon icon,
        global::Rhino.UI.PanelType panelType = global::Rhino.UI.PanelType.PerDoc) where TPanel : RasmPanel =>
        UiIntent.OfScope(
            run: _ =>
                from validPlugin in Op.Of(name: nameof(Register)).Need(plugin)
                from validCaption in Op.Of(name: nameof(Register)).AcceptText(value: caption).MapFail(_ => Op.Of(name: nameof(Register)).InvalidInput())
                from panel in RasmPanel.PanelIdentityOf<TPanel>()
                from registered in icon.Register(plugin: validPlugin, type: panel.Type, caption: validCaption, mode: panelType)
                select registered,
            interactive: false);

    public static UiIntent<Unit> Open<TPanel>(PanelPlacement? placement = null, bool selected = true) where TPanel : RasmPanel =>
        Bind<TPanel, Unit>(run: (_, panel) => (placement ?? PanelPlacement.Default).Open(panelType: panel.Type, panelId: panel.Id, selected: selected), interactive: true);

    public static UiIntent<PanelSnapshot<TPanel>> Show<TPanel>(bool selected = true) where TPanel : RasmPanel =>
        UiIntent.OfScope(
            run: scope =>
                from _ in Open<TPanel>(selected: selected).Run(scope: scope)
                from snapshot in Snapshot<TPanel>().Run(scope: scope)
                select snapshot,
            interactive: true);

    public static UiIntent<bool> Float<TPanel>(global::Rhino.UI.Panels.FloatPanelMode mode = global::Rhino.UI.Panels.FloatPanelMode.Show) where TPanel : RasmPanel =>
        Bind<TPanel, bool>(run: (_, panel) => RhinoUi.Protect(valid: () => Fin.Succ(value: global::Rhino.UI.Panels.FloatPanel(panelType: panel.Type, mode: mode))), interactive: true);

    public static UiIntent<bool> DockBarInUse<TPanel>(Guid dockBarId) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: _ => dockBarId switch {
            Guid id when id != Guid.Empty => RhinoUi.Protect(valid: () => Fin.Succ(value: global::Rhino.UI.Panels.DockBarIdInUse(dockBarId: id))),
            _ => Fin.Fail<bool>(error: Op.Of(name: nameof(DockBarInUse)).InvalidInput()),
        }, interactive: false);

    public static UiIntent<Unit> Icon<TPanel>(PanelIcon icon) where TPanel : RasmPanel =>
        Bind<TPanel, Unit>(run: (_, panel) => icon.Change(type: panel.Type), interactive: false);

    public static UiIntent<Unit> Close<TPanel>() where TPanel : RasmPanel =>
        Bind<TPanel, Unit>(run: (scope, panel) => RhinoUi.Protect(valid: () => {
            global::Rhino.UI.Panels.ClosePanel(panelId: panel.Id, doc: scope.Document);
            return Fin.Succ(value: unit);
        }), interactive: true);

    public static UiIntent<PanelSnapshot<TPanel>> Snapshot<TPanel>() where TPanel : RasmPanel =>
        Bind<TPanel, PanelSnapshot<TPanel>>(run: (scope, panel) => RhinoUi.Protect(valid: () => {
            Seq<TPanel> instances = toSeq(global::Rhino.UI.Panels.GetPanels<TPanel>(scope.Document));
            Seq<Guid> open = toSeq(global::Rhino.UI.Panels.GetOpenPanelIds()).Distinct();
            bool activeScope = ReferenceEquals(RhinoDoc.ActiveDoc, scope.Document);
            Guid dock = activeScope ? global::Rhino.UI.Panels.PanelDockBar(panelId: panel.Id) : Guid.Empty;
            return Fin.Succ(value: new PanelSnapshot<TPanel>(
                PanelId: panel.Id,
                Visible: activeScope && global::Rhino.UI.Panels.IsPanelVisible(panelId: panel.Id, isSelectedTab: false),
                Selected: activeScope && global::Rhino.UI.Panels.IsPanelVisible(panelId: panel.Id, isSelectedTab: true),
                Instances: instances,
                OpenPanelIds: open,
                DockBarId: dock == Guid.Empty ? Option<Guid>.None : Some(dock),
                DockBarIds: activeScope ? toSeq(global::Rhino.UI.Panels.PanelDockBars(panelId: panel.Id)).Filter(static id => id != Guid.Empty).Distinct() : Seq<Guid>()));
        }), interactive: false);

    public static UiIntent<T> With<TPanel, T>(Func<Seq<TPanel>, Fin<T>> run) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: scope => from validRun in Op.Of(name: nameof(With)).Need(run) from snapshot in Snapshot<TPanel>().Run(scope: scope) from panels in snapshot.Instances switch { Seq<TPanel> values when !values.IsEmpty => Fin.Succ(value: values), _ => Fin.Fail<Seq<TPanel>>(error: Op.Of(name: nameof(With)).InvalidResult()) } from result in validRun(arg: panels) select result, interactive: true);
    public static UiIntent<(bool Visible, bool Selected)> Visible<TPanel>() where TPanel : RasmPanel =>
        Bind<TPanel, (bool Visible, bool Selected)>(run: (scope, panel) => RhinoUi.Protect(valid: () => {
            bool activeScope = ReferenceEquals(RhinoDoc.ActiveDoc, scope.Document);
            return Fin.Succ(value: (Visible: activeScope && global::Rhino.UI.Panels.IsPanelVisible(panelType: panel.Type, isSelectedTab: false), Selected: activeScope && global::Rhino.UI.Panels.IsPanelVisible(panelType: panel.Type, isSelectedTab: true)));
        }), interactive: false);
    public static UiIntent<Unit> Emit<TPanel>(PanelEvent panelEvent) where TPanel : RasmPanel =>
        Bind<TPanel, Unit>(run: (_, panel) => from kind in Op.Of(name: nameof(Emit)).Need(panelEvent.Kind)
                                              let active = panelEvent with { Kind = kind }
                                              from _same in guard(active.Id == panel.Id, Op.Of(name: nameof(Emit)).InvalidInput())
                                              from emitted in RhinoUi.Protect(valid: () => active.Kind.Emit(panelId: panel.Id, documentSerial: active.DocumentSerial))
                                              select emitted, interactive: false);

    public static UiIntent<Unit> MenuState<TPanel>(Guid fileId, Guid menuId, Guid itemId, Func<PanelMenuState, Fin<PanelMenuState>> update) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: _ =>
            from validUpdate in Op.Of(name: nameof(MenuState)).Need(update)
            from gated in guard(fileId != Guid.Empty && menuId != Guid.Empty && itemId != Guid.Empty, Op.Of(name: nameof(MenuState)).InvalidInput())
            from registered in PanelMenuState.BindMany(ids: Seq((fileId, menuId, itemId)), update: validUpdate)
            select registered, interactive: false);

    public static UiIntent<T> Watch<TPanel, T>(Func<PanelEvent, Fin<Unit>> change, Func<Fin<T>> run, Func<PanelEvent, bool>? until = null) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: scope =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from valid in Op.Of(name: nameof(Watch)).Need(change)
            from body in Op.Of(name: nameof(Watch)).Need(run)
            let serial = scope.Document.RuntimeSerialNumber
            from subscription in RhinoUi.Protect(valid: () => {
                Subscription? box = null;   // forward ref so an `until` match can self-detach mid-run
                Fin<Unit> Fire(WatchPayload.Panel host, uint documentSerial) {
                    PanelEvent local = host.State switch {
                        WatchPanelState.Shown => new PanelEvent(Kind: PanelEventKind.Shown, Id: host.Id, DocumentSerial: Some(documentSerial)),
                        WatchPanelState.Hidden => new PanelEvent(Kind: PanelEventKind.Hidden, Id: host.Id, DocumentSerial: Some(documentSerial)),
                        WatchPanelState.Closed => new PanelEvent(Kind: PanelEventKind.Closed, Id: host.Id, DocumentSerial: Some(documentSerial)),
                        _ => new PanelEvent(Kind: PanelEventKind.None, Id: host.Id, DocumentSerial: Some(documentSerial)),
                    };
                    return valid(arg: local).Map(_ => Optional(until).Filter(predicate => predicate(arg: local)).Iter(_ => box?.Dispose()));
                }
                Fin<Unit> Deliver(WatchEvent native) =>
                    native.Panel.Case switch {
                        WatchPayload.Panel host when host.Id == panel.Id && native.DocumentSerialNumber == serial => Fire(host: host, documentSerial: native.DocumentSerialNumber),
                        _ => Fin.Succ(value: unit),
                    };
                return WatchBus.Subscribe(
                    target: new WatchTarget.Document(Value: scope.Document),
                    spec: new WatchSpec(Sink: Deliver, Phases: Seq(WatchPhase.PanelShown, WatchPhase.PanelHidden, WatchPhase.PanelClosed)))
                    .Map(active => { box = active; return active; });
            })
            from result in RhinoUi.Protect(valid: () => {
                try { return body(); } finally { subscription.Dispose(); }
            })
            select result,
            interactive: false);
}
