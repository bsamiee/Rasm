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

    internal readonly record struct Scope(RhinoDoc Document, RunMode Mode, Option<Window> Owner = default) {
        public Window? Parent =>
            Owner.Case switch {
                Window window => window,
                _ => global::Rhino.UI.RhinoEtoApp.MainWindowForDocument(Document),
            };
    }

    public Fin<T> Use<T>(UiIntent<T> intent) =>
        Optional(intent)
            .ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
            .Bind(valid => (valid.Interactive && mode == RunMode.Scripted, valid.Scripted.Case) switch {
                (true, Func<Scope, Fin<T>> scripted) => OnUiThread(run: () => scripted(arg: new Scope(Document: document, Mode: mode))),
                (true, _) => Fin.Fail<T>(error: Op.Of(name: nameof(Use)).InvalidInput()),
                _ => OnUiThread(run: () => valid.Run(scope: new Scope(Document: document, Mode: mode))),
            });

    internal Seq<T> Windows<T>() where T : Window =>
        toSeq(global::Rhino.UI.EtoExtensions.WindowsFromDocument<T>(document));

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run, [CallerMemberName] string name = "") =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Bind(valid => RhinoApp.IsOnMainThread switch {
                true => Catch(valid: valid, name: name),
                false => Invoke(valid: valid, name: name),
            });

    private static Fin<T> Invoke<T>(Func<Fin<T>> valid, string name) =>
        Try.lift<Fin<T>>(f: () => {
            Fin<T> result = Fin.Fail<T>(error: Op.Of(name: name).InvalidResult());
            RhinoApp.InvokeAndWait(action: () => result = Catch(valid: valid, name: name));
            return result;
        })
            .Run()
            .MapFail(_ => Op.Of(name: name).InvalidResult())
            .Bind(static result => result);

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        OnUiThread(run: valid, name: name);

    internal static Fin<Unit> Enqueue(Action run, string name) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Map(valid => {
                RhinoApp.InvokeOnUiThread(method: valid, args: []);
                return unit;
            });

    private static Fin<T> Catch<T>(Func<Fin<T>> valid, string name) =>
        Optional(valid)
            .ToFin(Fail: Op.Of(name: name).InvalidInput())
            .Bind(callback => Try.lift<Fin<T>>(f: callback)
                .Run()
                .MapFail(_ => Op.Of(name: name).InvalidResult())
                .Bind(static result => result));
}

public readonly record struct UiStatus(
    Option<string> Prompt = default,
    Option<string> PromptDefault = default,
    Option<string> CommandMessage = default,
    Option<string> Message = default,
    Option<double> Distance = default,
    Option<double> Number = default,
    Option<Point3d> Point = default,
    bool ClearMessage = false) {
    public static UiStatus operator +(UiStatus left, UiStatus right) => Add(left: left, right: right);
    public static UiStatus Add(UiStatus left, UiStatus right) =>
        new(Prompt: right.Prompt | left.Prompt, PromptDefault: right.PromptDefault | left.PromptDefault, CommandMessage: right.CommandMessage | left.CommandMessage, Message: right.Message | (right.ClearMessage ? Option<string>.None : left.Message), Distance: right.Distance | left.Distance, Number: right.Number | left.Number, Point: right.Point | left.Point, ClearMessage: left.ClearMessage || right.ClearMessage);
    public static UiStatus Add(params UiStatus[] statuses) =>
        Optional(statuses)
            .Map(static values => toSeq(values).Fold(initialState: new UiStatus(), f: static (state, value) => state + value))
            .IfNone(new UiStatus());

    public static UiStatus Script(string message) =>
        string.IsNullOrWhiteSpace(value: message) switch {
            false => new UiStatus(CommandMessage: Some(message.Trim()), Message: Option<string>.None, ClearMessage: false),
            true => new UiStatus(),
        };

    internal Fin<Unit> Apply() {
        UiStatus status = this;
        return RhinoUi.Protect(valid: () => {
            _ = (status.Prompt.Case, status.PromptDefault.Case) switch {
                (string prompt, string fallback) => ((Func<Unit>)(() => { RhinoApp.SetCommandPrompt(prompt: prompt, promptDefault: fallback); return unit; }))(),
                (string prompt, _) => ((Func<Unit>)(() => { RhinoApp.SetCommandPrompt(prompt: prompt); return unit; }))(),
                _ => unit,
            };
            _ = status.CommandMessage.Iter(static value => RhinoApp.SetCommandPromptMessage(prompt: value));
            _ = status.ClearMessage switch {
                true => ((Func<Unit>)(static () => { global::Rhino.UI.StatusBar.ClearMessagePane(); return unit; }))(),
                false => unit,
            };
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
    bool Absolute = true) {
    public static UiProgressStep Relative(int delta = 1, string? label = null) =>
        new(Position: Some(delta), Label: Optional(label), Absolute: false);
}

public sealed class UiProgress : IDisposable {
    private readonly uint documentSerialNumber;
    private readonly int lower;
    private readonly int upper;
    private int current;
    private bool disposed;

    private UiProgress(uint documentSerialNumber, int lower, int upper) {
        this.documentSerialNumber = documentSerialNumber;
        this.lower = lower;
        this.upper = upper;
        current = lower;
    }

    public Fin<int> Update(UiProgressStep step) =>
        disposed switch {
            true => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
            false => (step.Position.Map(position => step.Absolute ? position : current + position).Case, step.Position.Case, step.Label.Case) switch {
                (int position, _, _) when position < lower || position > upper => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
                (int target, int position, string label) => global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, label: label, position: position, absolute: step.Absolute) switch { RhinoMath.UnsetIntIndex => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidResult()), _ => Fin.Succ(value: ((Func<int>)(() => { current = target; return target; }))()) },
                (_, _, string label) => Fin.Succ(value: ((Func<int>)(() => { _ = global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, label: label, position: RhinoMath.UnsetIntIndex, absolute: true); return current; }))()),
                (int target, int position, _) => global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, position: position, absolute: step.Absolute) switch { RhinoMath.UnsetIntIndex => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidResult()), _ => Fin.Succ(value: ((Func<int>)(() => { current = target; return target; }))()) },
                _ => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
            },
        };

    public Fin<TState> Fold<TItem, TState>(
        IEnumerable<TItem> items,
        TState initial,
        Func<TState, TItem, Fin<TState>> step,
        Func<TItem, UiProgressStep> progress) =>
        from source in Optional(items).ToFin(Fail: Op.Of(name: nameof(Fold)).InvalidInput()).Map(static values => toSeq(values))
        from transition in Optional(step).ToFin(Fail: Op.Of(name: nameof(Fold)).InvalidInput())
        from project in Optional(progress).ToFin(Fail: Op.Of(name: nameof(Fold)).InvalidInput())
        from result in source.Fold(
            Fin.Succ(value: initial),
            (state, item) =>
                from current in state
                from next in transition(arg1: current, arg2: item)
                from _ in Update(step: project(arg: item))
                select next)
        select result;

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => ((Func<Unit>)(() => { global::Rhino.UI.StatusBar.HideProgressMeter(docSerialNumber: documentSerialNumber); return unit; }))(),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiProgressSpec spec, Func<UiProgress, Fin<T>> run) =>
        from validRun in Optional(run).ToFin(Fail: Op.Of(name: nameof(UiProgress)).InvalidInput())
        from scope in Start(document: document, spec: spec)
        from result in RhinoUi.Protect(valid: () => {
            // BOUNDARY ADAPTER - native status meter must hide even when caller rail fails.
            try { return validRun(arg: scope); } finally { scope.Dispose(); }
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
                1 => Fin.Succ(value: new UiProgress(documentSerialNumber: validDocument.RuntimeSerialNumber, lower: spec.Lower, upper: spec.Upper)),
                -1 => Fin.Fail<UiProgress>(error: Op.Of(name: nameof(UiProgress)).InvalidResult()),
                _ => Fin.Fail<UiProgress>(error: Op.Of(name: nameof(UiProgress)).InvalidResult()),
            }
        select created;

}
