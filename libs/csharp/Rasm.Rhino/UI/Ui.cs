using System.Runtime.CompilerServices;
using Eto.Forms;

namespace Rasm.Rhino.UI;

public sealed partial record RhinoUi {
    private readonly RhinoDoc document;
    private readonly RunMode mode;

    internal RhinoUi(RhinoDoc document, RunMode mode) {
        ArgumentNullException.ThrowIfNull(argument: document);
        this.document = document;
        this.mode = mode;
    }

    public Fin<T> Run<T>(Func<RhinoDoc, Fin<T>> operation, bool interactive = false) =>
        (interactive && mode == RunMode.Scripted) switch {
            true => Fin.Fail<T>(error: Op.Of(name: nameof(Run)).InvalidInput()),
            false => Optional(operation)
                .ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
                .Bind(valid => OnUiThread(run: () => valid(arg: document))),
        };

    public Fin<T> Show<T>(UiDialogIntent<T> dialog) =>
        Optional(dialog)
            .ToFin(Fail: Op.Of(name: nameof(Show)).InvalidInput())
            .Bind(valid => Run(operation: valid.Show, interactive: true));

    public Fin<Unit> Show(Form form) =>
        Show(dialog: UiDialogIntent.Modeless(form: form));

    public Fin<T> Wait<T>(Func<Fin<T>> run) =>
        Run(
            operation: _ => Optional(run)
                .ToFin(Fail: Op.Of(name: nameof(Wait)).InvalidInput())
                .Bind(valid => {
                    using global::Rhino.UI.WaitCursor cursor = new();
                    cursor.Set();
                    return Protect(valid: valid);
                }),
            interactive: true);

    public Seq<T> Windows<T>() where T : Form =>
        toSeq(global::Rhino.UI.EtoExtensions.WindowsFromDocument<T>(document));

    public Option<TPanel> Panel<TPanel>() where TPanel : RasmPanel =>
        Optional(global::Rhino.UI.Panels.GetPanel<TPanel>(document));

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
