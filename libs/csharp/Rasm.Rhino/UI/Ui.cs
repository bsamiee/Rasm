using System.Runtime.CompilerServices;
using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public sealed partial record RhinoUi {
    private readonly RhinoDoc document;
    private readonly RunMode mode;

    internal RhinoUi(RhinoDoc document, RunMode mode) {
        ArgumentNullException.ThrowIfNull(argument: document);
        this.document = document;
        this.mode = mode;
    }

    public Fin<T> Show<T>(UiDialogIntent<T> dialog) =>
        mode switch {
            RunMode.Scripted => Fin.Fail<T>(error: Op.Of(name: nameof(Show)).InvalidInput()),
            _ => Optional(dialog)
                .ToFin(Fail: Op.Of(name: nameof(Show)).InvalidInput())
                .Bind(valid => OnUiThread(run: () => valid.Show(document: document))),
        };

    public Fin<Unit> Show(Form form) =>
        Show(dialog: UiDialogIntent.Modeless(form: form));

    public Fin<T> Wait<T>(Func<Fin<T>> run) =>
        mode switch {
            RunMode.Scripted => Fin.Fail<T>(error: Op.Of(name: nameof(Wait)).InvalidInput()),
            _ => Optional(run)
                .ToFin(Fail: Op.Of(name: nameof(Wait)).InvalidInput())
                .Bind(valid => OnUiThread(run: () => {
                    using global::Rhino.UI.WaitCursor cursor = new();
                    cursor.Set();
                    return Protect(valid: valid);
                })),
        };

    public Seq<T> Windows<T>() where T : Form =>
        toSeq(global::Rhino.UI.EtoExtensions.WindowsFromDocument<T>(document));

    public Fin<Unit> Status(string message) =>
        mode switch {
            RunMode.Scripted => Fin.Fail<Unit>(error: Op.Of(name: nameof(Status)).InvalidInput()),
            _ => OnUiThread(run: () => {
                global::Rhino.UI.StatusBar.SetMessagePane(message);
                return Fin.Succ(value: unit);
            }),
        };

    public Fin<Unit> Status(double number, bool distance = false) =>
        mode switch {
            RunMode.Scripted => Fin.Fail<Unit>(error: Op.Of(name: nameof(Status)).InvalidInput()),
            _ => OnUiThread(run: () => {
                Action<double> set = distance switch {
                    true => global::Rhino.UI.StatusBar.SetDistancePane,
                    false => global::Rhino.UI.StatusBar.SetNumberPane,
                };
                set(obj: number);
                return Fin.Succ(value: unit);
            }),
        };

    public Fin<Unit> Status(Point3d point) =>
        mode switch {
            RunMode.Scripted => Fin.Fail<Unit>(error: Op.Of(name: nameof(Status)).InvalidInput()),
            _ => OnUiThread(run: () => {
                global::Rhino.UI.StatusBar.SetPointPane(point);
                return Fin.Succ(value: unit);
            }),
        };

    public Fin<Unit> Progress(int lower, int upper, string label, bool embedLabel = true, bool showPercentComplete = true) =>
        OnUiThread(run: () => global::Rhino.UI.StatusBar.ShowProgressMeter(
            docSerialNumber: document.RuntimeSerialNumber,
            lowerLimit: lower,
            upperLimit: upper,
            label: label,
            embedLabel: embedLabel,
            showPercentComplete: showPercentComplete) switch {
                1 => Fin.Succ(value: unit),
                _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Progress)).InvalidResult()),
            });

    public Fin<int> Progress(int position, bool absolute = true) =>
        OnUiThread(run: () => global::Rhino.UI.StatusBar.UpdateProgressMeter(
            docSerialNumber: document.RuntimeSerialNumber,
            position: position,
            absolute: absolute) switch {
                int previous and >= 0 => Fin.Succ(value: previous),
                _ => Fin.Fail<int>(error: Op.Of(name: nameof(Progress)).InvalidResult()),
            });

    public Fin<int> Progress(string label, int position, bool absolute = true) =>
        OnUiThread(run: () => global::Rhino.UI.StatusBar.UpdateProgressMeter(
            docSerialNumber: document.RuntimeSerialNumber,
            label: label,
            position: position,
            absolute: absolute) switch {
                int previous and >= 0 => Fin.Succ(value: previous),
                _ => Fin.Fail<int>(error: Op.Of(name: nameof(Progress)).InvalidResult()),
            });

    public Fin<Unit> HideProgress() =>
        OnUiThread(run: () => {
            global::Rhino.UI.StatusBar.HideProgressMeter(docSerialNumber: document.RuntimeSerialNumber);
            return Fin.Succ(value: unit);
        });

    public Fin<Unit> OpenPanel<TPanel>(bool makeSelected = true) where TPanel : RasmPanel =>
        mode switch {
            RunMode.Scripted => Fin.Fail<Unit>(error: Op.Of(name: nameof(OpenPanel)).InvalidInput()),
            _ => OnUiThread(run: () =>
                RasmPanel.PanelType<TPanel>().Map(type => {
                    global::Rhino.UI.Panels.OpenPanel(panelType: type, makeSelectedPanel: makeSelected);
                    return unit;
                })),
        };

    public Fin<Unit> ClosePanel<TPanel>() where TPanel : RasmPanel =>
        OnUiThread(run: () => RasmPanel.PanelType<TPanel>().Map(type => {
            global::Rhino.UI.Panels.ClosePanel(panelId: type.GUID, doc: document);
            return unit;
        }));

    public Fin<bool> PanelVisible<TPanel>() where TPanel : RasmPanel =>
        mode switch {
            RunMode.Scripted => Fin.Fail<bool>(error: Op.Of(name: nameof(PanelVisible)).InvalidInput()),
            _ => OnUiThread(run: () => RasmPanel.PanelType<TPanel>().Map(static type => global::Rhino.UI.Panels.IsPanelVisible(panelType: type))),
        };

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
