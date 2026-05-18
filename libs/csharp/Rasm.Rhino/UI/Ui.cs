using System.Runtime.CompilerServices;
using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public sealed record RhinoUi {
    private readonly RhinoDoc document;
    private readonly RunMode mode;

    internal RhinoUi(RhinoDoc document, RunMode mode) {
        ArgumentNullException.ThrowIfNull(argument: document);
        this.document = document;
        this.mode = mode;
    }

    public Fin<T> Show<T>(UiDialog<T> dialog) =>
        mode switch {
            RunMode.Scripted => Fin.Fail<T>(error: Op.Of(name: nameof(Show)).InvalidInput()),
            _ => Optional(dialog)
                .ToFin(Fail: Op.Of(name: nameof(Show)).InvalidInput())
                .Bind(valid => OnUiThread(run: () => valid.Show(document: document))),
        };

    public Fin<Unit> Show(Form form) =>
        Show(dialog: UiDialog.Modeless(form: form));

    public Seq<T> Windows<T>() where T : Form =>
        toSeq(global::Rhino.UI.EtoExtensions.WindowsFromDocument<T>(document));

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

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Try.lift<Fin<T>>(f: valid)
            .Run()
            .MapFail(_ => Op.Of(name: name).InvalidResult())
            .Bind(static result => result);
}
