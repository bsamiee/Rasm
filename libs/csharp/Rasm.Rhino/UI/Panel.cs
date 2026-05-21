using Eto.Forms;
using DrawingIcon = System.Drawing.Icon;
using DrawingSize = System.Drawing.Size;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum PanelHostPhase { Shown, Hidden, Closed }

public abstract record PanelPlacement {
    private PanelPlacement() { }
    public static PanelPlacement Default { get; } = new DefaultPlacement();
    public static Fin<PanelPlacement> Dock(Guid dockBarId) => dockBarId switch { Guid id when id != Guid.Empty => Fin.Succ<PanelPlacement>(value: new DockPlacement(id)), _ => Fin.Fail<PanelPlacement>(error: Op.Of(name: nameof(Dock)).InvalidInput()) };
    public static Fin<PanelPlacement> Sibling(Guid panelId) => panelId switch { Guid id when id != Guid.Empty => Fin.Succ<PanelPlacement>(value: new SiblingPlacement(id)), _ => Fin.Fail<PanelPlacement>(error: Op.Of(name: nameof(Sibling)).InvalidInput()) };
    internal abstract Fin<Unit> Open(Type panelType, Guid panelId, bool selected);
    private sealed record DefaultPlacement : PanelPlacement {
        internal override Fin<Unit> Open(Type panelType, Guid panelId, bool selected) => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.OpenPanel(panelType: panelType, makeSelectedPanel: selected); return Fin.Succ(value: unit); });
    }
    private sealed record DockPlacement(Guid DockBarId) : PanelPlacement {
        internal override Fin<Unit> Open(Type panelType, Guid panelId, bool selected) => RhinoUi.Protect(valid: () => global::Rhino.UI.Panels.OpenPanel(dockBarId: DockBarId, panelType: panelType, makeSelectedPanel: selected) switch { Guid id when id != Guid.Empty || OperatingSystem.IsMacOS() => Fin.Succ(value: unit), _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()) });
    }
    private sealed record SiblingPlacement(Guid PanelId) : PanelPlacement {
        internal override Fin<Unit> Open(Type panelType, Guid panelId, bool selected) => RhinoUi.Protect(valid: () => global::Rhino.UI.Panels.OpenPanelAsSibling(panelId: panelId, siblingPanelId: PanelId, makeSelectedPanel: selected) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()) });
    }
}

public enum UiChromeFileMode { Open, Close, Save, SaveAs }

public abstract record UiChromeOp<T> {
    private UiChromeOp() { }

    internal virtual bool Interactive => true;
    internal abstract Fin<T> Run(RhinoDoc? document);

    public sealed record EtoToolbar(IEnumerable<UiAction> Actions) : UiChromeOp<ToolBar> {
        internal override Fin<ToolBar> Run(RhinoDoc? document) =>
            from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(EtoToolbar)).InvalidInput())
            from actions in Optional(Actions).ToFin(Fail: Op.Of(name: nameof(EtoToolbar)).InvalidInput()).Map(static values => toSeq(values))
            from commands in actions.TraverseM(action => action.ToCommand(document: validDocument)).As()
            select ((Func<ToolBar>)(() => {
                ToolBar toolbar = new();
                _ = commands.Iter(command => toolbar.Items.Add(command.CreateToolItem()));
                return toolbar;
            }))();
    }

    public sealed record EtoMenu(IEnumerable<UiAction> Actions) : UiChromeOp<MenuBar> {
        internal override Fin<MenuBar> Run(RhinoDoc? document) =>
            from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(EtoMenu)).InvalidInput())
            from actions in Optional(Actions).ToFin(Fail: Op.Of(name: nameof(EtoMenu)).InvalidInput()).Map(static values => toSeq(values))
            from commands in actions.TraverseM(action => action.ToCommand(document: validDocument)).As()
            select ((Func<MenuBar>)(() => {
                MenuBar menu = new();
                _ = commands.Iter(command => menu.Items.Add(command.CreateMenuItem()));
                return menu;
            }))();
    }

    public sealed record RuiSnapshot : UiChromeOp<PanelChromeSnapshot> {
        internal override bool Interactive => false;

        internal override Fin<PanelChromeSnapshot> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: Snapshot);
    }

    public sealed record RuiFile(UiChromeFileMode Mode, Option<Guid> FileId = default, Option<string> Path = default, Option<string> Name = default, bool IgnoreCase = true, bool Prompt = false, bool SaveAfterOpen = false) : UiChromeOp<PanelChromeSnapshot> {
        internal override bool Interactive => Mode == UiChromeFileMode.Close && Prompt;

        internal override Fin<PanelChromeSnapshot> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () =>
                from _ in Mode switch {
                    UiChromeFileMode.Open => from path in NonBlank(value: Path)
                                             from file in Optional(RhinoApp.ToolbarFiles.Open(path: path)).ToFin(Fail: Op.Of(name: nameof(RuiFile)).InvalidResult())
                                             from saved in SaveAfterOpen switch {
                                                 true => file.Save() switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiFile)).InvalidResult()) },
                                                 false => Fin.Succ(value: unit),
                                             }
                                             select saved,
                    UiChromeFileMode.Close => from file in FindFile(fileId: FileId, path: Path, name: Name, ignoreCase: IgnoreCase)
                                              from closed in file.Close(prompt: Prompt) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiFile)).InvalidResult()) }
                                              select closed,
                    UiChromeFileMode.Save => from file in FindFile(fileId: FileId, path: Path, name: Name, ignoreCase: IgnoreCase)
                                             from saved in file.Save() switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiFile)).InvalidResult()) }
                                             select saved,
                    UiChromeFileMode.SaveAs => from file in FindFile(fileId: FileId, path: Option<string>.None, name: Name, ignoreCase: IgnoreCase)
                                               from path in NonBlank(value: Path)
                                               from saved in file.SaveAs(path: path) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiFile)).InvalidResult()) }
                                               select saved,
                    _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiFile)).InvalidInput()),
                }
                from snapshot in Snapshot()
                select snapshot);
    }

    public sealed record RuiGroup(Guid FileId, Guid GroupId, bool Visible) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () =>
                from file in FindFile(fileId: Some(FileId), path: Option<string>.None, name: Option<string>.None, ignoreCase: true)
                from activeGroup in toSeq(Enumerable.Range(start: 0, count: file.GroupCount)).Choose(index => Optional(file.GetGroup(index))).Find(candidate => candidate.Id == GroupId).ToFin(Fail: Op.Of(name: nameof(RuiGroup)).InvalidInput())
                select ((Func<Unit>)(() => { activeGroup.Visible = Visible; return unit; }))());
    }

    public sealed record RuiSidebar(bool Visible, bool Mru = false) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () => {
                _ = Mru switch {
                    true => ((Func<Unit>)(() => { global::Rhino.UI.ToolbarFileCollection.MruSidebarIsVisible = Visible; return unit; }))(),
                    false => ((Func<Unit>)(() => { global::Rhino.UI.ToolbarFileCollection.SidebarIsVisible = Visible; return unit; }))(),
                };
                return Fin.Succ(value: unit);
            });
    }

    public sealed record RuiToolbarSize(Option<DrawingSize> Bitmap = default, Option<DrawingSize> Tab = default) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            RhinoUi.Protect(valid: () =>
                (Bitmap.Case, Tab.Case) switch {
                    (DrawingSize bitmap, DrawingSize tab) when bitmap.Width > 0 && bitmap.Height > 0 && tab.Width > 0 && tab.Height > 0 => Fin.Succ(value: ((Func<Unit>)(() => { global::Rhino.UI.Toolbar.BitmapSize = bitmap; global::Rhino.UI.Toolbar.TabSize = tab; return unit; }))()),
                    (DrawingSize bitmap, _) when bitmap.Width > 0 && bitmap.Height > 0 => Fin.Succ(value: ((Func<Unit>)(() => { global::Rhino.UI.Toolbar.BitmapSize = bitmap; return unit; }))()),
                    (_, DrawingSize tab) when tab.Width > 0 && tab.Height > 0 => Fin.Succ(value: ((Func<Unit>)(() => { global::Rhino.UI.Toolbar.TabSize = tab; return unit; }))()),
                    _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiToolbarSize)).InvalidInput()),
                });
    }

    private static Fin<global::Rhino.UI.ToolbarFile> FindFile(Option<Guid> fileId, Option<string> path, Option<string> name, bool ignoreCase) =>
        (fileId.Bind(id => toSeq(RhinoApp.ToolbarFiles).Choose(Optional).Find(file => file.Id == id)) |
         path.Bind(value => Optional(RhinoApp.ToolbarFiles.FindByPath(path: value))) |
         name.Bind(value => Optional(RhinoApp.ToolbarFiles.FindByName(name: value, ignoreCase: ignoreCase))))
        .ToFin(Fail: Op.Of(name: nameof(FindFile)).InvalidInput());

    private static Fin<string> NonBlank(Option<string> value) =>
        value.ToFin(Fail: Op.Of(name: nameof(NonBlank)).InvalidInput()).Bind(NonBlank);

    private static Fin<string> NonBlank(string value) =>
        Op.Of(name: nameof(NonBlank)).AcceptText(value: value).MapFail(_ => Op.Of(name: nameof(NonBlank)).InvalidInput());

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
public sealed record PanelOp<TPanel, T> where TPanel : RasmPanel {
    private readonly Func<RhinoDoc?, Fin<T>> run;

    internal PanelOp(Func<RhinoDoc?, Fin<T>> run, bool interactive) {
        this.run = run;
        Interactive = interactive;
    }

    internal bool Interactive { get; }

    internal Fin<T> Run(RhinoDoc? document) => run(arg: document);
}

public readonly record struct PanelChromeSnapshot(
    Seq<(Guid Id, string Name, string Path, int GroupCount, int ToolbarCount)> Files,
    Seq<(Guid FileId, Guid GroupId, string Name, bool Visible, bool IsDocked)> Groups,
    Seq<(Guid FileId, Guid ToolbarId, string Name)> Toolbars,
    bool SidebarVisible,
    bool MruSidebarVisible);

public sealed record UiAction(
    string Id,
    string Text,
    Option<string> ToolTip,
    Option<Eto.Drawing.Image> Image,
    Func<RhinoDoc, Fin<Unit>> Run) {
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
                from active in Optional(document).ToFin(Fail: Op.Of(name: nameof(UiAction)).InvalidInput())
                let command = typeof(TCommand).Name
                from _ in guard(global::Rhino.Commands.Command.LookupCommandId(name: command, searchForEnglishName: true) != Guid.Empty, Op.Of(name: nameof(UiAction)).InvalidInput())
                from __ in guard(RhinoApp.RunScript(documentSerialNumber: active.RuntimeSerialNumber, script: $"_{command}", echo: false), Op.Of(name: nameof(UiAction)).InvalidResult())
                select unit);

    internal Fin<Eto.Forms.Command> ToCommand(RhinoDoc document) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(ToCommand)).InvalidInput())
        from action in Optional(Run).ToFin(Fail: Op.Of(name: nameof(ToCommand)).InvalidInput())
        from id in string.IsNullOrWhiteSpace(value: Id) switch {
            false => Fin.Succ(value: Id),
            true => Fin.Fail<string>(error: Op.Of(name: nameof(ToCommand)).InvalidInput()),
        }
        from text in string.IsNullOrWhiteSpace(value: Text) switch {
            false => Fin.Succ(value: Text),
            true => Fin.Fail<string>(error: Op.Of(name: nameof(ToCommand)).InvalidInput()),
        }
        select ((Func<Eto.Forms.Command>)(() => {
            Eto.Forms.Command command = new() { ID = id, MenuText = text, ToolBarText = text, ToolTip = ToolTip.IfNone(text) };
            _ = Image.Iter(active => command.Image = active);
            command.Executed += (_, _) => _ = RhinoUi.Protect(valid: () => action(arg: validDocument));
            return command;
        }))();
}

public readonly record struct PanelSnapshot<TPanel>(
    Guid PanelId,
    bool Visible,
    bool Selected,
    Seq<TPanel> Instances,
    Seq<Guid> OpenPanelIds,
    Option<Guid> DockBarId,
    Seq<Guid> DockBarIds) where TPanel : RasmPanel;

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
    internal static Option<PanelMenuState> Of(global::Rhino.UI.RuiUpdateUi ui) => Optional(ui).Bind(static valid => (valid.FileId, valid.MenuId, valid.MenuItemId) switch { (Guid file, Guid menu, Guid item) when file != Guid.Empty && menu != Guid.Empty && item != Guid.Empty => Some(new PanelMenuState(Enabled: valid.Enabled, Checked: valid.Checked, RadioChecked: valid.RadioChecked, Text: Optional(valid.Text), FileId: valid.FileId, MenuId: valid.MenuId, ItemId: valid.MenuItemId, MenuHandle: valid.MenuHandle, MenuIndex: valid.MenuIndex, WindowsMenuItemId: valid.WindowsMenuItemId)), _ => Option<PanelMenuState>.None });

    internal Unit Apply(global::Rhino.UI.RuiUpdateUi ui) { ui.Enabled = Enabled; ui.Checked = Checked; ui.RadioChecked = RadioChecked; _ = Text.Iter(value => ui.Text = value); return unit; }
}

public readonly record struct PanelIcon(Option<DrawingIcon> Icon, Option<string> ResourceName = default, Option<ReflectionAssembly> ResourceAssembly = default) {
    public static PanelIcon Of(DrawingIcon value) => new(Icon: Some(value));
    public static PanelIcon Resource(string name, ReflectionAssembly? assembly = null) => new(Icon: Option<DrawingIcon>.None, ResourceName: Some(name), ResourceAssembly: Optional(assembly));

    internal Fin<Unit> Register(global::Rhino.PlugIns.PlugIn plugin, Type type, string caption, global::Rhino.UI.PanelType mode) {
        Option<DrawingIcon> icon = Icon;
        Option<string> resourceName = ResourceName;
        Option<ReflectionAssembly> resourceAssembly = ResourceAssembly;
        return icon.Case switch {
            DrawingIcon value => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, value, mode); return Fin.Succ(value: unit); }),
            _ => from resource in resourceName.ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
                 from registered in RhinoUi.Protect(valid: () => {
                     _ = resourceAssembly.Case switch {
                         ReflectionAssembly assembly => ((Func<Unit>)(() => { global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, assembly, resource, mode); return unit; }))(),
                         _ => ((Func<Unit>)(() => { global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, null!, resource, mode); return unit; }))(),
                     };
                     return Fin.Succ(value: unit);
                 })
                 select registered,
        };
    }

    internal Fin<Unit> Change(Type type) =>
        Icon.Case switch {
            DrawingIcon value => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, value); return Fin.Succ(value: unit); }),
            _ => from resource in ResourceName.ToFin(Fail: Op.Of(name: nameof(Change)).InvalidInput())
                 from changed in RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, resource); return Fin.Succ(value: unit); })
                 select changed,
        };
}

public readonly record struct PanelHostEvent(PanelHostPhase Phase, Option<global::Rhino.UI.ShowPanelEventArgs> Show, Option<global::Rhino.UI.PanelEventArgs> Closed);

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmPanel : Panel, global::Rhino.UI.IPanel {
    protected enum PanelPhase { Shown, Hidden, Closing }

    protected readonly record struct PanelContext(PanelPhase Phase, uint DocumentSerialNumber, global::Rhino.UI.ShowPanelReason Reason = default, bool OnCloseDocument = false);

    protected RasmPanel() => global::Rhino.UI.EtoExtensions.UseRhinoStyle(this);

    public void PanelShown(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) => Apply(phase: PanelPhase.Shown, documentSerialNumber: documentSerialNumber, reason: reason);
    public void PanelHidden(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) => Apply(phase: PanelPhase.Hidden, documentSerialNumber: documentSerialNumber, reason: reason);
    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => Apply(phase: PanelPhase.Closing, documentSerialNumber: documentSerialNumber, onCloseDocument: onCloseDocument);

    protected virtual Fin<Unit> Change(PanelContext context) =>
        Fin.Succ(value: unit);

    private Unit Apply(PanelPhase phase, uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason = default, bool onCloseDocument = false) {
        _ = RhinoUi.Protect(valid: () => Change(context: new PanelContext(Phase: phase, DocumentSerialNumber: documentSerialNumber, Reason: reason, OnCloseDocument: onCloseDocument)));
        return unit;
    }

    internal readonly record struct PanelIdentity(Type Type, Guid Id);

    internal static Fin<PanelIdentity> PanelIdentityOf<TPanel>() where TPanel : RasmPanel {
        Type type = typeof(TPanel);
        bool constructible = type.GetConstructor(types: [typeof(uint)]) is not null || type.GetConstructor(types: Type.EmptyTypes) is not null;
        return type.GetCustomAttributes(attributeType: typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false).FirstOrDefault() switch { System.Runtime.InteropServices.GuidAttribute attribute when constructible && Guid.TryParse(input: attribute.Value, result: out Guid id) && id != Guid.Empty => Fin.Succ(value: new PanelIdentity(Type: type, Id: id)), _ => Fin.Fail<PanelIdentity>(error: Op.Of(name: nameof(PanelIdentityOf)).InvalidInput()) };
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class PanelOp {
    public static PanelOp<TPanel, T> Chrome<TPanel, T>(UiChromeOp<T> operation) where TPanel : RasmPanel =>
        new(
            run: document =>
                from valid in Optional(operation).ToFin(Fail: Op.Of(name: nameof(Chrome)).InvalidInput())
                from result in valid.Run(document: document)
                select result,
            interactive: Optional(operation).Map(static valid => valid.Interactive).IfNone(true));

    public static PanelOp<TPanel, Unit> Register<TPanel>(
        global::Rhino.PlugIns.PlugIn plugin,
        string caption,
        PanelIcon icon,
        global::Rhino.UI.PanelType panelType = global::Rhino.UI.PanelType.PerDoc) where TPanel : RasmPanel =>
        new(
            run: _ =>
                from validPlugin in Optional(plugin).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
                from validCaption in Op.Of(name: nameof(Register)).AcceptText(value: caption).MapFail(_ => Op.Of(name: nameof(Register)).InvalidInput())
                from panel in RasmPanel.PanelIdentityOf<TPanel>()
                from registered in icon.Register(plugin: validPlugin, type: panel.Type, caption: validCaption, mode: panelType)
                select registered,
            interactive: false);

    public static PanelOp<TPanel, Unit> Open<TPanel>(PanelPlacement? placement = null, bool selected = true) where TPanel : RasmPanel =>
        new(run: _ =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from opened in (placement ?? PanelPlacement.Default).Open(panelType: panel.Type, panelId: panel.Id, selected: selected)
            select opened,
            interactive: true);

    public static PanelOp<TPanel, PanelSnapshot<TPanel>> Show<TPanel>(bool selected = true) where TPanel : RasmPanel =>
        new(
            run: document =>
                from _ in Open<TPanel>(selected: selected).Run(document: document)
                from snapshot in Snapshot<TPanel>().Run(document: document)
                select snapshot,
            interactive: true);

    public static PanelOp<TPanel, bool> Float<TPanel>(global::Rhino.UI.Panels.FloatPanelMode mode = global::Rhino.UI.Panels.FloatPanelMode.Show) where TPanel : RasmPanel =>
        new(run: _ => RasmPanel.PanelIdentityOf<TPanel>().Bind(panel => RhinoUi.Protect(valid: () => Fin.Succ(value: global::Rhino.UI.Panels.FloatPanel(panelType: panel.Type, mode: mode)))), interactive: true);

    public static PanelOp<TPanel, bool> DockBarInUse<TPanel>(Guid dockBarId) where TPanel : RasmPanel =>
        new(run: _ => dockBarId switch {
            Guid id when id != Guid.Empty => RhinoUi.Protect(valid: () => Fin.Succ(value: global::Rhino.UI.Panels.DockBarIdInUse(dockBarId: id))),
            _ => Fin.Fail<bool>(error: Op.Of(name: nameof(DockBarInUse)).InvalidInput()),
        }, interactive: false);

    public static PanelOp<TPanel, Unit> Icon<TPanel>(PanelIcon icon) where TPanel : RasmPanel =>
        new(run: _ =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from changed in icon.Change(type: panel.Type)
            select changed,
            interactive: false);

    public static PanelOp<TPanel, Unit> Close<TPanel>() where TPanel : RasmPanel =>
        new(run: document =>
            RasmPanel.PanelIdentityOf<TPanel>().Bind(panel => RhinoUi.Protect(valid: () => {
                _ = document switch {
                    RhinoDoc doc => ((Func<Unit>)(() => { global::Rhino.UI.Panels.ClosePanel(panelId: panel.Id, doc: doc); return unit; }))(),
                    _ => ((Func<Unit>)(() => { global::Rhino.UI.Panels.ClosePanel(panelType: panel.Type); return unit; }))(),
                };
                return Fin.Succ(value: unit);
            })),
            interactive: true);

    public static PanelOp<TPanel, PanelSnapshot<TPanel>> Snapshot<TPanel>() where TPanel : RasmPanel =>
        new(run: document =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from doc in Optional(document).ToFin(Fail: Op.Of(name: nameof(Snapshot)).InvalidInput())
            from snapshot in RhinoUi.Protect(valid: () => {
                Seq<TPanel> instances = toSeq(global::Rhino.UI.Panels.GetPanels<TPanel>(doc));
                Seq<Guid> open = toSeq(global::Rhino.UI.Panels.GetOpenPanelIds()).Distinct();
                Guid dock = global::Rhino.UI.Panels.PanelDockBar(panelId: panel.Id);
                return Fin.Succ(value: new PanelSnapshot<TPanel>(
                    PanelId: panel.Id,
                    Visible: global::Rhino.UI.Panels.IsPanelVisible(panelId: panel.Id, isSelectedTab: false),
                    Selected: global::Rhino.UI.Panels.IsPanelVisible(panelId: panel.Id, isSelectedTab: true),
                    Instances: instances,
                    OpenPanelIds: open,
                    DockBarId: dock == Guid.Empty ? Option<Guid>.None : Some(dock),
                    DockBarIds: toSeq(global::Rhino.UI.Panels.PanelDockBars(panelId: panel.Id)).Filter(static id => id != Guid.Empty).Distinct()));
            })
            select snapshot,
            interactive: false);

    public static PanelOp<TPanel, T> With<TPanel, T>(Func<Seq<TPanel>, Fin<T>> run) where TPanel : RasmPanel =>
        new(run: document => from validRun in Optional(run).ToFin(Fail: Op.Of(name: nameof(With)).InvalidInput()) from snapshot in Snapshot<TPanel>().Run(document: document) from panels in snapshot.Instances switch { Seq<TPanel> values when !values.IsEmpty => Fin.Succ(value: values), _ => Fin.Fail<Seq<TPanel>>(error: Op.Of(name: nameof(With)).InvalidResult()) } from result in validRun(arg: panels) select result, interactive: true);

    public static PanelOp<TPanel, Unit> MenuState<TPanel>(Guid fileId, Guid menuId, Guid itemId, Func<PanelMenuState, Fin<PanelMenuState>> update) where TPanel : RasmPanel =>
        new(run: _ => from validUpdate in Optional(update).ToFin(Fail: Op.Of(name: nameof(MenuState)).InvalidInput()) from validIds in guard(fileId != Guid.Empty && menuId != Guid.Empty && itemId != Guid.Empty, Op.Of(name: nameof(MenuState)).InvalidInput()) from registered in RhinoUi.Protect(valid: () => global::Rhino.UI.RuiUpdateUi.RegisterMenuItem(fileId, menuId, itemId, (_, ui) => _ = RhinoUi.Protect(valid: () => from source in PanelMenuState.Of(ui: ui).ToFin(Fail: Op.Of(name: nameof(MenuState)).InvalidInput()) from state in validUpdate(arg: source) select state.Apply(ui: ui))) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(MenuState)).InvalidResult()) }) select registered, interactive: false);

    public static PanelOp<TPanel, T> Watch<TPanel, T>(Func<PanelHostEvent, Fin<Unit>> change, Func<Fin<T>> run) where TPanel : RasmPanel =>
        new(run: _ =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from valid in Optional(change).ToFin(Fail: Op.Of(name: nameof(Watch)).InvalidInput())
            from body in Optional(run).ToFin(Fail: Op.Of(name: nameof(Watch)).InvalidInput())
            from subscription in RhinoUi.Protect(valid: () => {
                void Show(object? _, global::Rhino.UI.ShowPanelEventArgs args) =>
                    _ = (args.PanelId == panel.Id) switch {
                        true => RhinoUi.Protect(valid: () => valid(arg: new PanelHostEvent(Phase: args.Show ? PanelHostPhase.Shown : PanelHostPhase.Hidden, Show: Some(args), Closed: Option<global::Rhino.UI.PanelEventArgs>.None))),
                        false => Fin.Succ(value: unit),
                    };

                void Closed(object? _, global::Rhino.UI.PanelEventArgs args) =>
                    _ = (args.PanelId == panel.Id) switch {
                        true => RhinoUi.Protect(valid: () => valid(arg: new PanelHostEvent(Phase: PanelHostPhase.Closed, Show: Option<global::Rhino.UI.ShowPanelEventArgs>.None, Closed: Some(args)))),
                        false => Fin.Succ(value: unit),
                    };

                EventHandler<global::Rhino.UI.ShowPanelEventArgs> show = Show;
                EventHandler<global::Rhino.UI.PanelEventArgs> closed = Closed;
                global::Rhino.UI.Panels.Show += show;
                global::Rhino.UI.Panels.Closed += closed;
                return Fin.Succ(value: new PanelSubscription(show: show, closed: closed));
            })
            from result in RhinoUi.Protect(valid: () => {
                try {
                    return body();
                } finally {
                    subscription.Dispose();
                }
            })
            select result,
            interactive: false);

    private sealed class PanelSubscription(EventHandler<global::Rhino.UI.ShowPanelEventArgs> show, EventHandler<global::Rhino.UI.PanelEventArgs> closed) : IDisposable {
        private bool disposed;

        public void Dispose() {
            _ = disposed switch {
                true => unit,
                false => ((Func<Unit>)(() => {
                    global::Rhino.UI.Panels.Show -= show;
                    global::Rhino.UI.Panels.Closed -= closed;
                    return unit;
                }))(),
            };
            disposed = true;
            GC.SuppressFinalize(obj: this);
        }
    }
}
