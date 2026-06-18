using System.Runtime.CompilerServices;
using Eto.Forms;
using StatusBar = Rhino.UI.StatusBar;

namespace Rasm.Rhino.UI;

// --- [MODELS] -----------------------------------------------------------------------------
// ShowToast returns the toast id; capture it as a receipt so a future dismiss/replace has a target. RhinoView exposes no native toast-removal member, so the handle is host evidence only — no Dismiss/Replace op exists yet.
public readonly record struct UiToastHandle(RhinoView View, uint Id);

public readonly record struct UiToast(RhinoView View, string Message, Option<int> TextHeight = default, Option<System.Drawing.PointF> Location = default) {
    internal Fin<UiToastHandle> Apply() {
        RhinoView? target = View;
        string text = Message;
        Option<int> textHeight = TextHeight;
        Option<System.Drawing.PointF> location = Location;
        return from view in Op.Of().Need(target)
               from message in guard(!string.IsNullOrWhiteSpace(value: text), Op.Of().InvalidInput()).ToFin().Map(_ => text.Trim())
               select new UiToastHandle(View: view, Id: (textHeight.Case, location.Case) switch {
                   (int height, System.Drawing.PointF point) when height > 0 => view.ShowToast(message, height, point),
                   (int height, _) when height > 0 => view.ShowToast(message, height),
                   _ => view.ShowToast(message),
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
    bool ClearMessage = false,
    bool HideProgress = false) {
    public static UiStatus Combine(Seq<UiStatus> statuses) =>
        statuses.Fold(new UiStatus(), static (acc, value) => new(
            Prompt: value.Prompt | acc.Prompt,
            PromptDefault: value.PromptDefault | acc.PromptDefault,
            CommandMessage: value.CommandMessage | acc.CommandMessage,
            Message: value.Message | (value.ClearMessage ? Option<string>.None : acc.Message),
            Distance: value.Distance | acc.Distance,
            Number: value.Number | acc.Number,
            Point: value.Point | acc.Point,
            Toasts: acc.Toasts + value.Toasts,
            ClearMessage: acc.ClearMessage || value.ClearMessage,
            HideProgress: acc.HideProgress || value.HideProgress));
    public static UiStatus operator +(UiStatus left, UiStatus right) => Combine(Seq(left, right));

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
            _ = Op.SideWhen(status.ClearMessage, static () => StatusBar.ClearMessagePane());
            _ = status.Message.Iter(static value => StatusBar.SetMessagePane(message: value));
            _ = status.Distance.Iter(static value => StatusBar.SetDistancePane(distance: value));
            _ = status.Number.Iter(static value => StatusBar.SetNumberPane(number: value));
            _ = status.Point.Iter(static value => StatusBar.SetPointPane(point: value));
            _ = Op.SideWhen(status.HideProgress, static () => StatusBar.HideProgressMeter());
            // best-effort toasts: each is recovered to success so one failed notification does not cross-cancel the rest; the captured handle is discarded at this batch boundary
            return status.Toasts
                .TraverseM(static value => value.Apply().Map(static _ => unit) | Fin.Succ(value: unit))
                .As().Map(static _ => unit);
        });
    }
}

// StatusBar.ShowProgressMeter returns a tri-state int (1=created, 0=refused, -1=another process owns the meter). A borrowed (Foreign) meter exists but is not ours: it must never be updated or hidden, only Created owns the IDisposable lifecycle.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MeterState {
    private MeterState() { }
    public sealed record Created(uint Serial) : MeterState;
    public sealed record Refused : MeterState;
    public sealed record Foreign : MeterState;

    internal static MeterState Admit(int code, uint serial) =>
        code switch {
            1 => new Created(Serial: serial),
            -1 => new Foreign(),
            _ => new Refused(),
        };
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
    internal readonly record struct Scope(RhinoDoc Document, RunMode Mode, Option<Window> Owner = default) {
        public Window? Parent =>
            Owner.Case switch {
                Window window => window,
                _ => global::Rhino.UI.RhinoEtoApp.MainWindowForDocument(Document),
            };
    }

    private readonly RhinoDoc document;
    private readonly RunMode mode;

    internal RhinoUi(RhinoDoc document, RunMode mode) {
        ArgumentNullException.ThrowIfNull(argument: document);
        this.document = document;
        this.mode = mode;
    }

    public Fin<T> Use<T>(UiIntent<T> intent) =>
        Op.Of().Need(intent).Bind(valid => {
            Scope scope = new(Document: document, Mode: mode);
            Func<Fin<T>> work = (valid.Interactive && mode == RunMode.Scripted, valid.Scripted.Case) switch {
                (true, Func<Scope, Fin<T>> scripted) => () => scripted(arg: scope),
                (true, _) => () => Fin.Fail<T>(error: Op.Of().InvalidInput()),
                _ => () => valid.Run(scope: scope),
            };
            return DispatchThread(uiBound: valid.Interactive, mode: mode, run: work, name: nameof(Use));
        });

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Op.Of(name: name).Need(valid).Bind(work => RhinoApp.IsOnMainThread ? Op.Of(name: name).Catch(work) : Invoke(valid: work, name: name));
    internal static Fin<T> DispatchThread<T>(
        bool uiBound,
        RunMode mode,
        Func<Fin<T>> run,
        [CallerMemberName] string name = "") =>
        Op.Of(name: name).Need(run)
            .Bind(work => (uiBound, mode, RhinoApp.IsOnMainThread) switch {
                (false, _, _) or (_, RunMode.Scripted, _) or (_, _, true) => Op.Of(name: name).Catch(work),   // direct-run arms now capsuled (no marshal); off-thread already proven false below
                (_, _, false) => Invoke(valid: work, name: name),
            });

    internal static Unit Deliver<TState, TResult>(
        Atom<TState> state,
        Func<TState, Fin<(TState State, TResult Result)>> run,
        Action<TResult> apply,
        Action<Error>? failed = null,
        [CallerMemberName] string name = "") =>
        Protect(valid: () => Op.Of(name: name).Need(run).Bind(validRun =>
            Op.Of(name: name).Need(apply).Bind(validApply =>
                validRun(arg: state.Value).Map(next => Op.Side(() => {
                    _ = state.Swap(_ => next.State);
                    validApply(next.Result);
                })))))
        .Match(
            Succ: static _ => unit,
            Fail: error => {
                _ = Optional(failed).IfSome(handler => handler(error));
                return unit;
            });

    internal static Fin<Unit> Enqueue(Action run, string name) =>
        Op.Of(name: name).Need(run)
            .Map(valid => {
                RhinoApp.InvokeOnUiThread(method: () => _ = Op.Of(name: name).Catch(() => {
                    valid();
                    return Fin.Succ(value: unit);
                }).IfFail(error => RhinoApp.WriteLine($"Rasm UI enqueue failed: {error.Message}")), args: []);
                return unit;
            });
    internal Seq<T> Windows<T>() where T : Window =>
        toSeq(global::Rhino.UI.EtoExtensions.WindowsFromDocument<T>(document));

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
}

public sealed class UiProgress : IDisposable {
    private readonly MeterState state;
    private readonly int lower;
    private readonly int upper;
    private int current;
    private bool disposed;

    private UiProgress(MeterState state, int lower, int upper) {
        this.state = state;
        this.lower = lower;
        this.upper = upper;
        current = lower;
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiProgressSpec spec, Func<UiProgress, Fin<T>> run) =>
        from validRun in Op.Of(name: nameof(UiProgress)).Need(run)
        from scope in Start(document: document, spec: spec)
        from result in RhinoUi.Protect(valid: () => {
            try { return validRun(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    public Fin<int> Update(UiProgressStep step) {
        UiProgress self = this;
        int Commit(int value) { self.current = value; return value; }
        Fin<int> Drive(uint serial, int target, int rawPosition, string? label) {
            _ = label switch {
                string text => Op.Side(() => StatusBar.UpdateProgressMeter(docSerialNumber: serial, label: text, position: rawPosition, absolute: step.Absolute)),
                _ => Op.Side(() => StatusBar.UpdateProgressMeter(docSerialNumber: serial, position: rawPosition, absolute: step.Absolute)),
            };
            return Fin.Succ(value: Commit(target));
        }
        Fin<int> Owned(uint serial) =>
            (step.Position.Map(position => step.Absolute ? position : self.current + position).Case, step.Position.Case, step.Label.Case) switch {
                (int position, _, _) when position < self.lower || position > self.upper => Fin.Fail<int>(error: Op.Of().InvalidInput()),
                (int target, int position, string label) => Drive(serial, target, position, label),
                (_, _, string label) => (Op.Side(() => StatusBar.UpdateProgressMeter(docSerialNumber: serial, label: label, position: RhinoMath.UnsetIntIndex, absolute: true)), Fin.Succ(value: self.current)).Item2,   // position = UnsetIntIndex → label-only update (native contract)
                (int target, int position, _) => Drive(serial, target, position, label: null),
                _ => Fin.Fail<int>(error: Op.Of().InvalidInput()),
            };
        return disposed switch {
            true => Fin.Fail<int>(error: Op.Of().InvalidInput()),
            false => state.Switch(
                state: Owned,
                created: static (owned, c) => owned(arg: c.Serial),
                foreign: static (_, _) => Fin.Succ(value: 0),   // borrowed meter — no-op, not failure
                refused: static (_, _) => Fin.Fail<int>(error: Op.Of().InvalidResult())),
        };
    }

    public Fin<TState> Fold<TItem, TState>(
        IEnumerable<TItem> items,
        TState initial,
        Func<TState, TItem, Fin<TState>> step,
        Func<TItem, UiProgressStep> progress) =>
        from source in Op.Of().Need(items).Map(static values => toSeq(values))
        from transition in Op.Of().Need(step)
        from project in Op.Of().Need(progress)
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
            false => state.Switch(   // only the owned (Created) meter hides; a borrowed Foreign meter belongs to another process and must not be torn down
                created: static c => Op.Side(() => StatusBar.HideProgressMeter(docSerialNumber: c.Serial)),
                foreign: static _ => unit,
                refused: static _ => unit),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    private static Fin<UiProgress> Start(RhinoDoc document, UiProgressSpec spec) =>
        from validDocument in Op.Of(name: nameof(UiProgress)).Need(document)
        from label in Op.Of(name: nameof(UiProgress)).Need(spec.Label)
        from _ in guard(spec.Upper >= spec.Lower, Op.Of(name: nameof(UiProgress)).InvalidInput())
        from created in MeterState.Admit(
            code: StatusBar.ShowProgressMeter(
                docSerialNumber: validDocument.RuntimeSerialNumber,
                lowerLimit: spec.Lower,
                upperLimit: spec.Upper,
                label: label,
                embedLabel: spec.EmbedLabel,
                showPercentComplete: spec.ShowPercentComplete),
            serial: validDocument.RuntimeSerialNumber) switch {
                MeterState.Refused => Fin.Fail<UiProgress>(error: Op.Of(name: nameof(UiProgress)).InvalidResult()),
                MeterState meter => Fin.Succ(value: new UiProgress(state: meter, lower: spec.Lower, upper: spec.Upper)),
            }
        select created;

}
