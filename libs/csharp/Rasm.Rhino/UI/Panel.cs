using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmPanel : Panel, global::Rhino.UI.IPanel {
    protected RasmPanel() => global::Rhino.UI.EtoExtensions.UseRhinoStyle(this);

    public static Fin<Unit> Register<TPanel>(
        global::Rhino.PlugIns.PlugIn plugin,
        string caption,
        System.Reflection.Assembly? iconAssembly,
        string iconResourceId,
        global::Rhino.UI.PanelType panelType = global::Rhino.UI.PanelType.PerDoc) where TPanel : RasmPanel =>
        from validPlugin in Optional(plugin).ToFin(Fail: Op.Of(name: nameof(Register)).InvalidInput())
        from validCaption in NonBlank(value: caption)
        from validResource in NonBlank(value: iconResourceId)
        from panel in PanelType<TPanel>()
        from registered in Try.lift<Unit>(f: () => {
            global::Rhino.UI.Panels.RegisterPanel(validPlugin, panel, validCaption, iconAssembly, validResource, panelType);
            return unit;
        })
            .Run()
            .MapFail(_ => Op.Of(name: nameof(Register)).InvalidResult())
        select registered;

    public void PanelShown(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        _ = RhinoUi.Protect(valid: () => OnPanelShown(documentSerialNumber: documentSerialNumber, reason: reason));

    public void PanelHidden(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        _ = RhinoUi.Protect(valid: () => Hidden(documentSerialNumber: documentSerialNumber, reason: reason));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) =>
        _ = RhinoUi.Protect(valid: () => Closing(documentSerialNumber: documentSerialNumber, onCloseDocument: onCloseDocument));

    protected virtual Fin<Unit> OnPanelShown(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Hidden(uint documentSerialNumber, global::Rhino.UI.ShowPanelReason reason) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Closing(uint documentSerialNumber, bool onCloseDocument) =>
        Fin.Succ(value: unit);

    private static Fin<Type> PanelType<TPanel>() where TPanel : RasmPanel {
        Type type = typeof(TPanel);
        return (type.GetCustomAttributes(attributeType: typeof(System.Runtime.InteropServices.GuidAttribute), inherit: false).Length,
                type.GetConstructor(types: Type.EmptyTypes),
                type.GetConstructor(types: [typeof(uint)])) switch {
                    ( > 0, not null, _) => Fin.Succ(value: type),
                    ( > 0, _, not null) => Fin.Succ(value: type),
                    _ => Fin.Fail<Type>(error: Op.Of(name: nameof(Register)).InvalidInput()),
                };
    }

    private static Fin<string> NonBlank(string value) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => Fin.Succ(value: value),
            true => Fin.Fail<string>(error: Op.Of(name: nameof(Register)).InvalidInput()),
        };
}
