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

    internal readonly record struct Scope(RhinoDoc Document, RunMode Mode);

    public Fin<T> Use<T>(UiIntent<T> intent) =>
        Optional(intent)
            .ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
            .Bind(valid => (valid.Interactive && mode == RunMode.Scripted) switch {
                true => Fin.Fail<T>(error: Op.Of(name: nameof(Use)).InvalidInput()),
                false => OnUiThread(run: () => valid.Run(scope: new Scope(Document: document, Mode: mode))),
            });

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

public readonly record struct UiStatus(
    Option<string> Prompt = default,
    Option<string> PromptDefault = default,
    Option<string> Message = default,
    Option<double> Distance = default,
    Option<double> Number = default,
    Option<Point3d> Point = default,
    bool ClearMessage = false) {
    public static UiStatus operator +(UiStatus left, UiStatus right) => Add(left: left, right: right);
    public static UiStatus Add(UiStatus left, UiStatus right) =>
        new(Prompt: right.Prompt.IsSome ? right.Prompt : left.Prompt, PromptDefault: right.PromptDefault.IsSome ? right.PromptDefault : left.PromptDefault, Message: right.Message.IsSome ? right.Message : left.Message, Distance: right.Distance.IsSome ? right.Distance : left.Distance, Number: right.Number.IsSome ? right.Number : left.Number, Point: right.Point.IsSome ? right.Point : left.Point, ClearMessage: left.ClearMessage || right.ClearMessage);

    internal Fin<Unit> Apply() {
        UiStatus status = this;
        return RhinoUi.Protect(valid: () => {
            _ = (status.Prompt.Case, status.PromptDefault.Case) switch {
                (string prompt, string fallback) => ((Func<Unit>)(() => { RhinoApp.SetCommandPrompt(prompt: prompt, promptDefault: fallback); return unit; }))(),
                (string prompt, _) => ((Func<Unit>)(() => { RhinoApp.SetCommandPrompt(prompt: prompt); return unit; }))(),
                _ => unit,
            };
            _ = status.ClearMessage ? ((Func<Unit>)(() => { global::Rhino.UI.StatusBar.ClearMessagePane(); return unit; }))() : unit;
            _ = status.Message.Iter(static value => global::Rhino.UI.StatusBar.SetMessagePane(message: value));
            _ = status.Distance.Iter(static value => global::Rhino.UI.StatusBar.SetDistancePane(distance: value));
            _ = status.Number.Iter(static value => global::Rhino.UI.StatusBar.SetNumberPane(number: value));
            _ = status.Point.Iter(static value => global::Rhino.UI.StatusBar.SetPointPane(point: value));
            return Fin.Succ(value: unit);
        });
    }

}

public readonly record struct UiProgressSpec(
    int Lower,
    int Upper,
    string Label,
    bool EmbedLabel = true,
    bool ShowPercentComplete = true);

public readonly record struct UiProgressStep(
    Option<int> Position = default,
    Option<string> Label = default,
    bool Absolute = true);

public sealed class UiProgress : IDisposable {
    private readonly uint documentSerialNumber;
    private bool disposed;

    private UiProgress(uint documentSerialNumber) => this.documentSerialNumber = documentSerialNumber;

    public Fin<int> Update(UiProgressStep step) =>
        (step.Position.Case, step.Label.Case) switch {
            (int position, string label) => Previous(value: global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, label: label, position: position, absolute: step.Absolute)),
            (_, string label) => Previous(value: global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, label: label, position: RhinoMath.UnsetIntIndex, absolute: true)),
            (int position, _) => Previous(value: global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, position: position, absolute: step.Absolute)),
            _ => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
        };

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => Hide(),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiProgressSpec spec, Func<UiProgress, Fin<T>> run) =>
        from scope in Start(document: document, spec: spec)
        from result in RhinoUi.Protect(valid: () => {
            // BOUNDARY ADAPTER - native status meter must hide even when caller rail fails.
            try { return run(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    private static Fin<UiProgress> Start(RhinoDoc document, UiProgressSpec spec) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(UiProgress)).InvalidInput())
        from label in Optional(spec.Label).ToFin(Fail: Op.Of(name: nameof(UiProgress)).InvalidInput())
        from _ in guard(spec.Upper >= spec.Lower, Op.Of(name: nameof(UiProgress)).InvalidInput())
        from created in global::Rhino.UI.StatusBar.ShowProgressMeter(
            docSerialNumber: validDocument.RuntimeSerialNumber,
            lowerLimit: spec.Lower,
            upperLimit: spec.Upper,
            label: label,
            embedLabel: spec.EmbedLabel,
            showPercentComplete: spec.ShowPercentComplete) switch {
                1 => Fin.Succ(value: new UiProgress(documentSerialNumber: validDocument.RuntimeSerialNumber)),
                -1 => Fin.Fail<UiProgress>(error: Op.Of(name: nameof(UiProgress)).InvalidInput()),
                _ => Fin.Fail<UiProgress>(error: Op.Of(name: nameof(UiProgress)).InvalidResult()),
            }
        select created;

    private static Fin<int> Previous(int value) =>
        value switch {
            RhinoMath.UnsetIntIndex => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidResult()),
            _ => Fin.Succ(value: value),
        };

    private Unit Hide() {
        global::Rhino.UI.StatusBar.HideProgressMeter(docSerialNumber: documentSerialNumber);
        return unit;
    }
}
