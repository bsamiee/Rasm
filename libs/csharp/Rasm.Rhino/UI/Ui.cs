using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public sealed record RhinoUi {
    private readonly RhinoDoc document;

    internal RhinoUi(RhinoDoc document) {
        ArgumentNullException.ThrowIfNull(argument: document);
        this.document = document;
    }

    public Fin<T> Show<T>(UiDialog<T> dialog) =>
        Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Show)).InvalidInput())
            .Bind(valid => OnUiThread(run: () => valid.Show(document: document)));

    internal static Window? Parent(RhinoDoc document) =>
        global::Rhino.UI.RhinoEtoApp.MainWindowForDocument(document);

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(OnUiThread)).InvalidInput())
            .Bind(valid => RhinoApp.IsOnMainThread switch {
                true => Protect(valid: valid),
                false => Invoke(valid: valid),
            });

    private static Fin<T> Invoke<T>(Func<Fin<T>> valid) =>
        Try.lift<Fin<T>>(f: () => {
            Fin<T> result = Fin.Fail<T>(error: Op.Of(name: nameof(Invoke)).InvalidResult());
            RhinoApp.InvokeAndWait(action: () => result = Protect(valid: valid));
            return result;
        })
            .Run()
            .MapFail(static _ => Op.Of(name: nameof(Invoke)).InvalidResult())
            .Bind(static result => result);

    private static Fin<T> Protect<T>(Func<Fin<T>> valid) =>
        Try.lift<Fin<T>>(f: valid)
            .Run()
            .MapFail(static _ => Op.Of(name: nameof(Protect)).InvalidResult())
            .Bind(static result => result);
}
