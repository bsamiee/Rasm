using System.Runtime.CompilerServices;
using Eto.Forms;

namespace Rasm.Rhino.UI;

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct UiToast(RhinoView View, string Message, Option<int> TextHeight = default, Option<System.Drawing.PointF> Location = default) {
    internal Unit Apply() {
        UiToast toast = this;
        return Op.Side(() => _ = (toast.TextHeight.Case, toast.Location.Case) switch {
            (int height, System.Drawing.PointF point) when height > 0 => toast.View.ShowToast(toast.Message, height, point),
            (int height, _) when height > 0 => toast.View.ShowToast(toast.Message, height),
            _ => toast.View.ShowToast(toast.Message),
        });
    }
}

public readonly record struct UiStatus(
    Option<string> Prompt = default,
    Option<string> PromptDefault = default,
    Option<string> CommandMessage = default,
    Option<string> Message = default,
    Option<double> Distance = default,
    Option<double> Number = default,
    Option<Point3d> Point = default,
    Seq<UiToast> Toasts = default,
    bool ClearMessage = false) {
    public static UiStatus operator +(UiStatus left, UiStatus right) => Add(left: left, right: right);
    public static UiStatus Add(UiStatus left, UiStatus right) =>
        new(Prompt: right.Prompt | left.Prompt, PromptDefault: right.PromptDefault | left.PromptDefault, CommandMessage: right.CommandMessage | left.CommandMessage, Message: right.Message | (right.ClearMessage ? Option<string>.None : left.Message), Distance: right.Distance | left.Distance, Number: right.Number | left.Number, Point: right.Point | left.Point, Toasts: left.Toasts + right.Toasts, ClearMessage: left.ClearMessage || right.ClearMessage);
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
                (string prompt, string fallback) => Op.Side(() => RhinoApp.SetCommandPrompt(prompt: prompt, promptDefault: fallback)),
                (string prompt, _) => Op.Side(() => RhinoApp.SetCommandPrompt(prompt: prompt)),
                _ => unit,
            };
            _ = status.CommandMessage.Iter(static value => RhinoApp.SetCommandPromptMessage(prompt: value));
            _ = Op.SideWhen(status.ClearMessage, static () => global::Rhino.UI.StatusBar.ClearMessagePane());
            _ = status.Message.Iter(static value => global::Rhino.UI.StatusBar.SetMessagePane(message: value));
            _ = status.Distance.Iter(static value => global::Rhino.UI.StatusBar.SetDistancePane(distance: value));
            _ = status.Number.Iter(static value => global::Rhino.UI.StatusBar.SetNumberPane(number: value));
            _ = status.Point.Iter(static value => global::Rhino.UI.StatusBar.SetPointPane(point: value));
            _ = status.Toasts.Iter(static value => value.Apply());
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


// --- [SERVICES] ---------------------------------------------------------------------------
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
        Op.Of(name: nameof(Use)).Need(intent)
            .Bind(valid => (valid.Interactive && mode == RunMode.Scripted, valid.Scripted.Case) switch {
                (true, Func<Scope, Fin<T>> scripted) => Protect(valid: () => scripted(arg: new Scope(Document: document, Mode: mode))),
                (true, _) => Fin.Fail<T>(error: Op.Of(name: nameof(Use)).InvalidInput()),
                _ => Protect(valid: () => valid.Run(scope: new Scope(Document: document, Mode: mode))),
            });

    internal Seq<T> Windows<T>() where T : Window =>
        toSeq(global::Rhino.UI.EtoExtensions.WindowsFromDocument<T>(document));

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Op.Of(name: name).Need(valid)
            .Bind(work => RhinoApp.IsOnMainThread switch {
                true => Catch(valid: work, name: name),
                false => Invoke(valid: work, name: name),
            });

    /// Domain rails (Blocks, Camera, …) call this once at the service edge.
    /// Scripted RhinoCode runs inside a blocked main-thread idle handler; InvokeAndWait deadlocks there.
    /// Native table/object mutations succeed on the script thread — marshal only for interactive off-thread callers.
    internal static Fin<T> DispatchThread<T>(
        bool uiBound,
        RunMode mode,
        Func<Fin<T>> run,
        [CallerMemberName] string name = "") =>
        Op.Of(name: name).Need(run)
            .Bind(work => (uiBound, mode, RhinoApp.IsOnMainThread) switch {
                (false, _, _) => work(),
                (_, RunMode.Scripted, _) => work(),
                (_, _, true) => work(),
                (_, _, false) => Protect(valid: work, name: name),
            });

    private static Fin<T> Invoke<T>(Func<Fin<T>> valid, string name) {
        Op op = Op.Of(name: name);
        return op.Catch(() => {
            Fin<T>? captured = null;
            bool ran = false;
            RhinoApp.InvokeAndWait(action: () => {
                ran = true;
                captured = op.Catch(valid);
            });
            return (ran, captured) switch {
                (true, { } result) => result,
                (true, null) => Fin.Fail<T>(error: op.Caution(concern: "RhinoApp.InvokeAndWait executed but captured no result.")),
                (false, _) => Fin.Fail<T>(error: op.Caution(concern: "RhinoApp.InvokeAndWait did not execute the delegate.")),
            };
        });
    }

    internal static Fin<Unit> Enqueue(Action run, string name) =>
        Op.Of(name: name).Need(run)
            .Map(valid => {
                RhinoApp.InvokeOnUiThread(method: valid, args: []);
                return unit;
            });

    private static Fin<T> Catch<T>(Func<Fin<T>> valid, string name) =>
        Op.Of(name: name).Catch(valid);
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

    public Fin<int> Update(UiProgressStep step) {
        int Commit(int value) { current = value; return value; }
        Fin<int> Drive(int target, int rawPosition, string? label) =>
            label switch {
                string text => global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, label: text, position: rawPosition, absolute: step.Absolute),
                _ => global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, position: rawPosition, absolute: step.Absolute),
            } switch {
                RhinoMath.UnsetIntIndex => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidResult()),
                _ => Fin.Succ(value: Commit(target)),
            };
        return disposed switch {
            true => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
            false => (step.Position.Map(position => step.Absolute ? position : current + position).Case, step.Position.Case, step.Label.Case) switch {
                (int position, _, _) when position < lower || position > upper => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
                (int target, int position, string label) => Drive(target, position, label),
                (_, _, string label) => Op.Side(() => global::Rhino.UI.StatusBar.UpdateProgressMeter(docSerialNumber: documentSerialNumber, label: label, position: RhinoMath.UnsetIntIndex, absolute: true)) switch { _ => Fin.Succ(value: current) },
                (int target, int position, _) => Drive(target, position, null),
                _ => Fin.Fail<int>(error: Op.Of(name: nameof(Update)).InvalidInput()),
            },
        };
    }

    public Fin<TState> Fold<TItem, TState>(
        IEnumerable<TItem> items,
        TState initial,
        Func<TState, TItem, Fin<TState>> step,
        Func<TItem, UiProgressStep> progress) =>
        from source in Op.Of(name: nameof(Fold)).Need(items).Map(static values => toSeq(values))
        from transition in Op.Of(name: nameof(Fold)).Need(step)
        from project in Op.Of(name: nameof(Fold)).Need(progress)
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
            false => Op.Side(() => global::Rhino.UI.StatusBar.HideProgressMeter(docSerialNumber: documentSerialNumber)),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiProgressSpec spec, Func<UiProgress, Fin<T>> run) =>
        from validRun in Op.Of(name: nameof(UiProgress)).Need(run)
        from scope in Start(document: document, spec: spec)
        from result in RhinoUi.Protect(valid: () => {
            // BOUNDARY ADAPTER - native status meter must hide even when caller rail fails.
            try { return validRun(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    private static Fin<UiProgress> Start(RhinoDoc document, UiProgressSpec spec) =>
        from validDocument in Op.Of(name: nameof(UiProgress)).Need(document)
        from label in Op.Of(name: nameof(UiProgress)).Need(spec.Label)
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
