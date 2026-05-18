using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmPanel : Panel, global::Rhino.UI.IPanel {
    protected RasmPanel() => global::Rhino.UI.EtoExtensions.UseRhinoStyle(this);

    public void PanelShown(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        _ = OnPanelShown(documentSerialNumber: documentSerialNumber, reason: reason);

    public void PanelHidden(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        _ = Hidden(documentSerialNumber: documentSerialNumber, reason: reason);

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) =>
        _ = Closing(documentSerialNumber: documentSerialNumber, onCloseDocument: onCloseDocument);

    protected virtual Fin<Unit> OnPanelShown(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Hidden(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Closing(uint documentSerialNumber, bool onCloseDocument) =>
        Fin.Succ(value: unit);
}
