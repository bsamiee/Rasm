using Eto.Forms;
using DrawingIcon = System.Drawing.Icon;
using DrawingSize = System.Drawing.Size;
using GuidAttribute = System.Runtime.InteropServices.GuidAttribute;
using ReflectionAssembly = System.Reflection.Assembly;
using RhinoControls = Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "context")]
public abstract partial record PanelPlacement {
    private PanelPlacement() { }
    public sealed record Auto : PanelPlacement;
    public sealed record AtDockBar(Guid DockBarId) : PanelPlacement;
    public sealed record AsSibling(Guid SiblingPanelId) : PanelPlacement;

    public static PanelPlacement Default { get; } = new Auto();
    public static Fin<PanelPlacement> Dock(Guid dockBarId) =>
        dockBarId == Guid.Empty
            ? Fin.Fail<PanelPlacement>(error: Op.Of(name: nameof(Dock)).InvalidInput())
            : Fin.Succ<PanelPlacement>(value: new AtDockBar(dockBarId));
    public static Fin<PanelPlacement> Sibling(Guid panelId) =>
        panelId == Guid.Empty
            ? Fin.Fail<PanelPlacement>(error: Op.Of(name: nameof(Sibling)).InvalidInput())
            : Fin.Succ<PanelPlacement>(value: new AsSibling(panelId));

    internal Fin<Unit> Open(Type panelType, Guid panelId, bool selected) =>
        RhinoUi.Protect(valid: () => Switch<(Type Type, Guid Id, bool Selected), Fin<Unit>>(
            (panelType, panelId, selected),
            auto: static (ctx, _) => {
                global::Rhino.UI.Panels.OpenPanel(panelType: ctx.Type, makeSelectedPanel: ctx.Selected);
                return Fin.Succ(value: unit);
            },
            // BOUNDARY ADAPTER — OpenPanel(dockBarId) returns the host dock-bar id on Windows but Guid.Empty on macOS even on
            // success (macOS has no dock-bar identity), so the macOS branch accepts Empty rather than reading it as failure.
            atDockBar: static (ctx, dock) => global::Rhino.UI.Panels.OpenPanel(dockBarId: dock.DockBarId, panelType: ctx.Type, makeSelectedPanel: ctx.Selected) switch {
                Guid id when id != Guid.Empty || OperatingSystem.IsMacOS() => Fin.Succ(value: unit),
                _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()),
            },
            asSibling: static (ctx, sibling) => global::Rhino.UI.Panels.OpenPanelAsSibling(panelId: ctx.Id, siblingPanelId: sibling.SiblingPanelId, makeSelectedPanel: ctx.Selected) switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()),
            }));
}

public enum UiChromeFileMode { Open, Close, Save, SaveAs }

public abstract record UiChromeOp<T> {
    private UiChromeOp() { }

    internal virtual bool Interactive => true;
    internal abstract Fin<T> Run(RhinoDoc? document);

    public sealed record EtoToolbar(IEnumerable<UiAction> Actions) : UiChromeOp<ToolBar> {
        internal override Fin<ToolBar> Run(RhinoDoc? document) =>
            BuildBar(Actions, document, nameof(EtoToolbar), static () => new ToolBar(), static (bar, cmd) => bar.Items.Add(cmd.CreateToolItem()));
    }

    public sealed record EtoMenu(IEnumerable<UiAction> Actions) : UiChromeOp<MenuBar> {
        internal override Fin<MenuBar> Run(RhinoDoc? document) =>
            BuildBar(Actions, document, nameof(EtoMenu), static () => new MenuBar(), static (bar, cmd) => bar.Items.Add(cmd.CreateMenuItem()));
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
            RhinoUi.Protect(valid: () =>
                (Bitmap.Case, Tab.Case) switch {
                    (DrawingSize bitmap, DrawingSize tab) when bitmap.Width > 0 && bitmap.Height > 0 && tab.Width > 0 && tab.Height > 0 => Fin.Succ(value: Op.Side(() => { global::Rhino.UI.Toolbar.BitmapSize = bitmap; global::Rhino.UI.Toolbar.TabSize = tab; })),
                    (DrawingSize bitmap, _) when bitmap.Width > 0 && bitmap.Height > 0 => Fin.Succ(value: Op.Side(() => global::Rhino.UI.Toolbar.BitmapSize = bitmap)),
                    (_, DrawingSize tab) when tab.Width > 0 && tab.Height > 0 => Fin.Succ(value: Op.Side(() => global::Rhino.UI.Toolbar.TabSize = tab)),
                    _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(RuiToolbarSize)).InvalidInput()),
                });
    }

    // Atomic multi-op chrome: a distinct combinator over the existing ops (not a knob on any single op); threads
    // each child op's Run through the same document on the rail, short-circuiting on the first failure.
    public sealed record Batch(IEnumerable<UiChromeOp<Unit>> Ops) : UiChromeOp<Unit> {
        internal override Fin<Unit> Run(RhinoDoc? document) =>
            Op.Of(name: nameof(Batch)).Need(Ops).Bind(src => toSeq(src).TraverseM(op => op.Run(document: document)).As().Map(static _ => unit));
    }

    private static Fin<TBar> BuildBar<TBar>(IEnumerable<UiAction>? actions, RhinoDoc? document, string opName, Func<TBar> create, Action<TBar, Eto.Forms.Command> add) where TBar : class =>
        from validDocument in Op.Of(name: opName).Need(document)
        from validActions in Op.Of(name: opName).Need(actions).Map(static values => toSeq(values))
        from commands in validActions.TraverseM(action => action.ToCommand(document: validDocument)).As()
        select ((Func<TBar>)(() => { TBar bar = create(); _ = commands.Iter(c => add(bar, c)); return bar; }))();

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
                from active in Op.Of(name: nameof(UiAction)).Need(document)
                let command = typeof(TCommand).Name
                from _ in guard(global::Rhino.Commands.Command.LookupCommandId(name: command, searchForEnglishName: true) != Guid.Empty, Op.Of(name: nameof(UiAction)).InvalidInput())
                from __ in guard(RhinoApp.RunScript(documentSerialNumber: active.RuntimeSerialNumber, script: $"_{command}", echo: false), Op.Of(name: nameof(UiAction)).InvalidResult())
                select unit);

    internal Fin<Eto.Forms.Command> ToCommand(RhinoDoc document) =>
        from validDocument in Op.Of(name: nameof(ToCommand)).Need(document)
        from action in Op.Of(name: nameof(ToCommand)).Need(Run)
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
    private static readonly Atom<HashMap<(Guid File, Guid Menu, Guid Item), Func<PanelMenuState, Fin<PanelMenuState>>>> MenuUpdates =
        Atom(HashMap<(Guid File, Guid Menu, Guid Item), Func<PanelMenuState, Fin<PanelMenuState>>>());

    private static Option<PanelMenuState> Of(global::Rhino.UI.RuiUpdateUi ui) => Optional(ui).Bind(static valid => (valid.FileId, valid.MenuId, valid.MenuItemId) switch { (Guid file, Guid menu, Guid item) when file != Guid.Empty && menu != Guid.Empty && item != Guid.Empty => Some(new PanelMenuState(Enabled: valid.Enabled, Checked: valid.Checked, RadioChecked: valid.RadioChecked, Text: Optional(valid.Text), FileId: valid.FileId, MenuId: valid.MenuId, ItemId: valid.MenuItemId, MenuHandle: valid.MenuHandle, MenuIndex: valid.MenuIndex, WindowsMenuItemId: valid.WindowsMenuItemId)), _ => Option<PanelMenuState>.None });

    private Unit Apply(global::Rhino.UI.RuiUpdateUi ui) { ui.Enabled = Enabled; ui.Checked = Checked; ui.RadioChecked = RadioChecked; _ = Text.Iter(value => ui.Text = value); return unit; }

    // Read-modify-write codec: project the live RuiUpdateUi into PanelMenuState, transform, write back.
    internal static Fin<Unit> Bind(global::Rhino.UI.RuiUpdateUi ui, Func<PanelMenuState, Fin<PanelMenuState>> update) =>
        from source in Op.Of(name: nameof(Bind)).Need(Of(ui: ui))
        from state in Op.Of(name: nameof(Bind)).Need(update).Bind(valid => valid(arg: source))
        select state.Apply(ui: ui);

    // Single home for the RuiUpdateUi.RegisterMenuItem boundary: folds registration over every valid id-triple,
    // each callback threading the read-modify-write codec. Empty/partial triples are skipped, not failed.
    internal static Fin<Unit> BindMany(Seq<(Guid File, Guid Menu, Guid Item)> ids, Func<PanelMenuState, Fin<PanelMenuState>> update) =>
        Op.Of(name: nameof(BindMany)).Need(update).Bind(valid =>
            ids.Filter(static id => id.File != Guid.Empty && id.Menu != Guid.Empty && id.Item != Guid.Empty)
                .TraverseM(id => RhinoUi.Protect(valid: () => {
                    HashMap<(Guid File, Guid Menu, Guid Item), Func<PanelMenuState, Fin<PanelMenuState>>> before = MenuUpdates.Value;
                    _ = MenuUpdates.Swap(state => state.AddOrUpdate(id, valid));
                    return before.ContainsKey(id) switch {
                        true => Fin.Succ(value: unit),
                        false => global::Rhino.UI.RuiUpdateUi.RegisterMenuItem(
                            id.File,
                            id.Menu,
                            id.Item,
                            (_, ui) => _ = MenuUpdates.Value.Find(id).Iter(handler => _ = RhinoUi.Protect(valid: () => Bind(ui: ui, update: handler))))
                            ? Fin.Succ(value: unit)
                            : Fin.Fail<Unit>(error: Op.Of(name: nameof(BindMany)).InvalidResult()),
                    };
                }))
                .As().Map(static _ => unit));
}

public readonly record struct PanelIcon(Option<DrawingIcon> Icon, Option<string> ResourceName = default, Option<ReflectionAssembly> ResourceAssembly = default) {
    public static PanelIcon Of(DrawingIcon value) => new(Icon: Some(value));
    public static PanelIcon Resource(string name, ReflectionAssembly? assembly = null) => new(Icon: Option<DrawingIcon>.None, ResourceName: Some(name), ResourceAssembly: Optional(assembly));

    private Fin<Unit> Resolve(Op op, Func<DrawingIcon, Fin<Unit>> onIcon, Func<string, Fin<Unit>> onResource) =>
        Icon.Case switch {
            DrawingIcon value => onIcon(arg: value),
            _ => op.Need(ResourceName).Bind(onResource),
        };

    internal Fin<Unit> Register(global::Rhino.PlugIns.PlugIn plugin, Type type, string caption, global::Rhino.UI.PanelType mode) {
        Option<ReflectionAssembly> resourceAssembly = ResourceAssembly;
        return Resolve(
            op: Op.Of(name: nameof(Register)),
            onIcon: value => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, value, mode); return Fin.Succ(value: unit); }),
            onResource: resource => RhinoUi.Protect(valid: () => {
                _ = resourceAssembly.Case switch {
                    ReflectionAssembly assembly => Op.Side(() => global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, assembly, resource, mode)),
                    _ => Op.Side(() => global::Rhino.UI.Panels.RegisterPanel(plugin, type, caption, null!, resource, mode)),
                };
                return Fin.Succ(value: unit);
            }));
    }

    internal Fin<Unit> Change(Type type) =>
        Resolve(
            op: Op.Of(name: nameof(Change)),
            onIcon: value => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, value); return Fin.Succ(value: unit); }),
            onResource: resource => RhinoUi.Protect(valid: () => { global::Rhino.UI.Panels.ChangePanelIcon(type, resource); return Fin.Succ(value: unit); }));
}

[Union]
public abstract partial record PanelEvent {
    private PanelEvent() { }

    public sealed record Shown(Guid Id, Option<uint> DocumentSerial, global::Rhino.UI.ShowPanelReason Reason) : PanelEvent;
    public sealed record Hidden(Guid Id, Option<uint> DocumentSerial, global::Rhino.UI.ShowPanelReason Reason) : PanelEvent;
    public sealed record Closing(Guid Id, uint DocumentSerial, bool OnCloseDocument) : PanelEvent;
    public sealed record Closed(Guid Id) : PanelEvent;

    public Guid PanelId =>
        this switch {
            Shown value => value.Id,
            Hidden value => value.Id,
            Closing value => value.Id,
            Closed value => value.Id,
            _ => Guid.Empty,
        };
}

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
        Op op = Op.Of(name: nameof(PanelIdentityOf));
        return (type.GetConstructor(types: [typeof(uint)]) is not null || type.GetConstructor(types: Type.EmptyTypes) is not null, type.GetCustomAttributes(attributeType: typeof(GuidAttribute), inherit: false).FirstOrDefault()) switch { (false, _) => Fin.Fail<PanelIdentity>(error: op.Unsupported(geometryType: type, outputType: typeof(PanelIdentity))), (true, GuidAttribute attribute) when Guid.TryParse(input: attribute.Value, result: out Guid id) && id != Guid.Empty => Fin.Succ(value: new PanelIdentity(Type: type, Id: id)), _ => Fin.Fail<PanelIdentity>(error: op.InvalidInput()) };
    }
}

public sealed class RasmSection : RhinoControls.EtoCollapsibleSection {
    private readonly global::Rhino.UI.LocalizeStringPair caption;
    private readonly int height;
    private readonly bool expanded;

    private RasmSection(global::Rhino.UI.LocalizeStringPair caption, int height, Control content, bool expanded) {
        this.caption = caption;
        this.height = height;
        this.expanded = expanded;
        Content = content;
        global::Rhino.UI.EtoExtensions.UseRhinoStyle(content);
    }

    public override global::Rhino.UI.LocalizeStringPair Caption => caption;
    public override int SectionHeight => height;
    public override bool InitiallyExpanded => expanded;

    public static Fin<RasmSection> Of(string caption, int sectionHeight, Control content, bool expanded = true, Option<string> local = default) =>
        from text in Op.Of(name: nameof(RasmSection)).AcceptText(value: caption).MapFail(_ => Op.Of(name: nameof(RasmSection)).InvalidInput())
        from valid in Op.Of(name: nameof(RasmSection)).Need(content)
        from _ in guard(sectionHeight > 0, Op.Of(name: nameof(RasmSection)).InvalidInput())
        select new RasmSection(caption: new global::Rhino.UI.LocalizeStringPair(text, local.IfNone(text)), height: sectionHeight, content: valid, expanded: expanded);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class PanelOp {
    // Single home for the PanelIdentityOf<TPanel>() resolution shared across Open/Float/Icon/Close/Snapshot.
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
            Guid dock = global::Rhino.UI.Panels.PanelDockBar(panelId: panel.Id);
            return Fin.Succ(value: new PanelSnapshot<TPanel>(
                PanelId: panel.Id,
                Visible: global::Rhino.UI.Panels.IsPanelVisible(panelId: panel.Id, isSelectedTab: false),
                Selected: global::Rhino.UI.Panels.IsPanelVisible(panelId: panel.Id, isSelectedTab: true),
                Instances: instances,
                OpenPanelIds: open,
                DockBarId: dock == Guid.Empty ? Option<Guid>.None : Some(dock),
                DockBarIds: toSeq(global::Rhino.UI.Panels.PanelDockBars(panelId: panel.Id)).Filter(static id => id != Guid.Empty).Distinct()));
        }), interactive: false);

    public static UiIntent<T> With<TPanel, T>(Func<Seq<TPanel>, Fin<T>> run) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: scope => from validRun in Op.Of(name: nameof(With)).Need(run) from snapshot in Snapshot<TPanel>().Run(scope: scope) from panels in snapshot.Instances switch { Seq<TPanel> values when !values.IsEmpty => Fin.Succ(value: values), _ => Fin.Fail<Seq<TPanel>>(error: Op.Of(name: nameof(With)).InvalidResult()) } from result in validRun(arg: panels) select result, interactive: true);

    public static UiIntent<Unit> MenuState<TPanel>(Guid fileId, Guid menuId, Guid itemId, Func<PanelMenuState, Fin<PanelMenuState>> update) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: _ =>
            from validUpdate in Op.Of(name: nameof(MenuState)).Need(update)
            from gated in guard(fileId != Guid.Empty && menuId != Guid.Empty && itemId != Guid.Empty, Op.Of(name: nameof(MenuState)).InvalidInput())
            from registered in PanelMenuState.BindMany(ids: Seq((fileId, menuId, itemId)), update: validUpdate)
            select registered, interactive: false);

    public static UiIntent<T> Watch<TPanel, T>(Func<PanelEvent, Fin<Unit>> change, Func<Fin<T>> run, Func<PanelEvent, bool>? until = null) where TPanel : RasmPanel =>
        UiIntent.OfScope(run: _ =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from valid in Op.Of(name: nameof(Watch)).Need(change)
            from body in Op.Of(name: nameof(Watch)).Need(run)
            from subscription in RhinoUi.Protect(valid: () => {
                Subscription? box = null;   // forward ref so an `until` match can self-detach mid-run
                Fin<Unit> Fire(PanelEvent host) =>
                    valid(arg: host).Map(_ => Optional(until).Filter(predicate => predicate(arg: host)).Iter(_ => box?.Dispose()));
                Func<TArgs, Fin<Unit>> Gate<TArgs>(Func<TArgs, Guid> idOf, Func<TArgs, PanelEvent> make) =>
                    args => idOf(arg: args) == panel.Id ? Fire(host: make(arg: args)) : Fin.Succ(value: unit);
                box = new Subscription(detachers:
                    Subscriptions.Attach(active: true, h => global::Rhino.UI.Panels.Show += h, h => global::Rhino.UI.Panels.Show -= h, Gate<global::Rhino.UI.ShowPanelEventArgs>(idOf: static args => args.PanelId, make: static args => args.Show ? new PanelEvent.Shown(Id: args.PanelId, DocumentSerial: Some(args.DocumentSerialNumber), Reason: global::Rhino.UI.ShowPanelReason.Show) : new PanelEvent.Hidden(Id: args.PanelId, DocumentSerial: Some(args.DocumentSerialNumber), Reason: global::Rhino.UI.ShowPanelReason.Hide)))
                    + Subscriptions.Attach(active: true, h => global::Rhino.UI.Panels.Closed += h, h => global::Rhino.UI.Panels.Closed -= h, Gate<global::Rhino.UI.PanelEventArgs>(idOf: static args => args.PanelId, make: static args => new PanelEvent.Closed(Id: args.PanelId))));
                return Fin.Succ(value: box);
            })
            from result in RhinoUi.Protect(valid: () => {
                // BOUNDARY ADAPTER — native panel-event detach must close after the scoped body exits (including throw).
                try { return body(); } finally { subscription.Dispose(); }
            })
            select result,
            interactive: false);
}
