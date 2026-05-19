using Eto.Forms;
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

    internal static Fin<Type> PanelType<TPanel>() where TPanel : RasmPanel {
        Type type = typeof(TPanel);
        return (type.GetCustomAttributes(attributeType: typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false).Length,
                type.GetConstructor(types: [typeof(uint)]),
                type.GetConstructor(types: Type.EmptyTypes)) switch {
                    ( > 0, not null, _) => Fin.Succ(value: type),
                    ( > 0, _, not null) => Fin.Succ(value: type),
                    _ => Fin.Fail<Type>(error: Op.Of(name: nameof(PanelType)).InvalidInput()),
                };
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

public static class PanelOp {
    public static PanelOp<TPanel, Unit> Register<TPanel>(
        global::Rhino.PlugIns.PlugIn plugin,
        string caption,
        System.Reflection.Assembly? iconAssembly,
        string iconResourceId,
        global::Rhino.UI.PanelType panelType = global::Rhino.UI.PanelType.PerDoc) where TPanel : RasmPanel =>
        new(
            run: _ =>
                from validPlugin in Optional(plugin).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
                from validCaption in NonBlank(value: caption)
                from validResource in NonBlank(value: iconResourceId)
                from panel in RasmPanel.PanelType<TPanel>()
                from registered in RhinoUi.Protect(valid: () => {
                    global::Rhino.UI.Panels.RegisterPanel(validPlugin, panel, validCaption, iconAssembly, validResource, panelType);
                    return Fin.Succ(value: unit);
                })
                select registered,
            interactive: false);

    public static PanelOp<TPanel, Unit> Open<TPanel>(bool selected = true, Option<Guid> dock = default, Option<Guid> sibling = default) where TPanel : RasmPanel =>
        new(run: _ =>
            RasmPanel.PanelType<TPanel>().Bind(panelType => (dock.Case, sibling.Case) switch {
                (Guid dockBar, _) => global::Rhino.UI.Panels.OpenPanel(dockBarId: dockBar, panelType: panelType, makeSelectedPanel: selected) switch {
                    Guid id when id != Guid.Empty => Fin.Succ(value: unit),
                    _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()),
                },
                (_, Guid siblingPanel) => global::Rhino.UI.Panels.OpenPanelAsSibling(panelId: PanelId(panelType: panelType), siblingPanelId: siblingPanel, makeSelectedPanel: selected) switch {
                    true => Fin.Succ(value: unit),
                    false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Open)).InvalidResult()),
                },
                _ => RhinoUi.Protect(valid: () => {
                    global::Rhino.UI.Panels.OpenPanel(panelType: panelType, makeSelectedPanel: selected);
                    return Fin.Succ(value: unit);
                }),
            }),
            interactive: true);

    public static PanelOp<TPanel, bool> Float<TPanel>(global::Rhino.UI.Panels.FloatPanelMode mode = global::Rhino.UI.Panels.FloatPanelMode.Show) where TPanel : RasmPanel =>
        new(run: _ => RasmPanel.PanelType<TPanel>().Map(panel => global::Rhino.UI.Panels.FloatPanel(panelType: panel, mode: mode)), interactive: true);

    public static PanelOp<TPanel, Unit> Close<TPanel>() where TPanel : RasmPanel =>
        new(run: document =>
            RasmPanel.PanelType<TPanel>().Map(panel => {
                _ = document switch {
                    RhinoDoc doc => ((Func<Unit>)(() => { global::Rhino.UI.Panels.ClosePanel(panelId: PanelId(panelType: panel), doc: doc); return unit; }))(),
                    _ => ((Func<Unit>)(() => { global::Rhino.UI.Panels.ClosePanel(panelType: panel); return unit; }))(),
                };
                return unit;
            }),
            interactive: true);

    public static PanelOp<TPanel, (Guid PanelId, bool Visible, Seq<TPanel> Instances, Seq<Guid> OpenPanelIds)> Snapshot<TPanel>(bool selectedTabOnWindows = false) where TPanel : RasmPanel =>
        new(run: document => from panel in RasmPanel.PanelType<TPanel>() from doc in Optional(document).ToFin(Fail: Op.Of(name: nameof(Snapshot)).InvalidInput()) let id = PanelId(panelType: panel) select (PanelId: id, Visible: global::Rhino.UI.Panels.IsPanelVisible(panelType: panel, isSelectedTab: selectedTabOnWindows), Instances: toSeq(global::Rhino.UI.Panels.GetPanels<TPanel>(doc)), OpenPanelIds: toSeq(global::Rhino.UI.Panels.GetOpenPanelIds())), interactive: false);

    private static Fin<string> NonBlank(string value) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => Fin.Succ(value: value),
            true => Fin.Fail<string>(error: Op.Of(name: nameof(PanelOp)).InvalidInput()),
        };

    private static Guid PanelId(Type panelType) =>
        ((System.Runtime.InteropServices.GuidAttribute)panelType.GetCustomAttributes(attributeType: typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false)[0]).Value switch {
            string value => Guid.Parse(input: value),
            _ => Guid.Empty,
        };
}
