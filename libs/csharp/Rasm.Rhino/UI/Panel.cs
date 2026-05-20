using Eto.Forms;
using DrawingIcon = System.Drawing.Icon;
using ReflectionAssembly = System.Reflection.Assembly;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
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

public sealed record PanelOp<TPanel, T> where TPanel : RasmPanel {
    private readonly Func<RhinoDoc?, Fin<T>> run;

    internal PanelOp(Func<RhinoDoc?, Fin<T>> run, bool interactive) {
        this.run = run;
        Interactive = interactive;
    }

    internal bool Interactive { get; }

    internal Fin<T> Run(RhinoDoc? document) => run(arg: document);
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

public enum PanelHostPhase { Shown, Hidden, Closed }

public readonly record struct PanelHostEvent(PanelHostPhase Phase, Option<global::Rhino.UI.ShowPanelEventArgs> Show, Option<global::Rhino.UI.PanelEventArgs> Closed);

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
        internal override Fin<Unit> Open(Type panelType, Guid panelId, bool selected) => RhinoUi.Protect(valid: () => global::Rhino.UI.Panels.OpenPanel(dockBarId: DockBarId, panelType: panelType, makeSelectedPanel: selected) switch { Guid id when id != Guid.Empty => Fin.Succ(value: unit), _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()) });
    }
    private sealed record SiblingPlacement(Guid PanelId) : PanelPlacement {
        internal override Fin<Unit> Open(Type panelType, Guid panelId, bool selected) => RhinoUi.Protect(valid: () => global::Rhino.UI.Panels.OpenPanelAsSibling(panelId: panelId, siblingPanelId: PanelId, makeSelectedPanel: selected) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()) });
    }
}

public static class PanelOp {
    public static PanelOp<TPanel, Unit> Register<TPanel>(
        global::Rhino.PlugIns.PlugIn plugin,
        string caption,
        object icon,
        ReflectionAssembly? iconAssembly = null,
        global::Rhino.UI.PanelType panelType = global::Rhino.UI.PanelType.PerDoc) where TPanel : RasmPanel =>
        new(
            run: _ =>
                from validPlugin in Optional(plugin).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
                from validCaption in NonBlank(value: caption)
                from validIcon in Optional(icon).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
                from panel in RasmPanel.PanelIdentityOf<TPanel>()
                from registered in RhinoUi.Protect(valid: () => IconDispatch(value: validIcon, native: iconValue => { global::Rhino.UI.Panels.RegisterPanel(validPlugin, panel.Type, validCaption, iconValue, panelType); return unit; }, resource: resourceName => from assembly in Optional(iconAssembly).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput()) select ((Func<Unit>)(() => { global::Rhino.UI.Panels.RegisterPanel(validPlugin, panel.Type, validCaption, assembly, resourceName, panelType); return unit; }))(), name: nameof(Register)))
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

    public static PanelOp<TPanel, Unit> Icon<TPanel>(object icon) where TPanel : RasmPanel =>
        new(run: _ =>
            from panel in RasmPanel.PanelIdentityOf<TPanel>()
            from value in Optional(icon).ToFin(Fail: Op.Of(name: nameof(Icon)).InvalidInput())
            from changed in RhinoUi.Protect(valid: () => IconDispatch(value: value, native: iconValue => { global::Rhino.UI.Panels.ChangePanelIcon(panel.Type, iconValue); return unit; }, resource: resourceName => Fin.Succ(value: ((Func<Unit>)(() => { global::Rhino.UI.Panels.ChangePanelIcon(panel.Type, resourceName); return unit; }))()), name: nameof(Icon)))
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

    private static Fin<string> NonBlank(string value) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => Fin.Succ(value: value),
            true => Fin.Fail<string>(error: Op.Of(name: nameof(PanelOp)).InvalidInput()),
        };

    private static Fin<Unit> IconDispatch(object value, Func<DrawingIcon, Unit> native, Func<string, Fin<Unit>> resource, string name) => (Optional(value), Optional(native), Optional(resource)) switch { (Option<object> source, Option<Func<DrawingIcon, Unit>> draw, _) when source.Case is DrawingIcon icon && draw.Case is Func<DrawingIcon, Unit> apply => Fin.Succ(value: apply(arg: icon)), (Option<object> source, _, Option<Func<string, Fin<Unit>>> res) when source.Case is string key && res.Case is Func<string, Fin<Unit>> apply => NonBlank(value: key).Bind(valid => apply(arg: valid)), _ => Fin.Fail<Unit>(error: Op.Of(name: name).InvalidInput()) };

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
