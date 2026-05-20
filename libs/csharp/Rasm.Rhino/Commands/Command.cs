using Result = Rhino.Commands.Result;
using UiGumballSnapshot = Rasm.Rhino.UI.UiGumballSnapshot;
using UiViewportPreview = Rasm.Rhino.UI.UiViewportPreview;

namespace Rasm.Rhino.Commands;

public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        Rasm.Rhino.UI.RhinoUi.Protect(valid: () =>
            RhinoCommandContext.Of(doc: doc, mode: mode)
            .Bind(context =>
                Ready(context: context)
                    .Bind(_ => Run(context: context))
                    .Bind(result => Complete(context: context, result: result))))
            .Match(Succ: static result => result, Fail: FailureResult);

    protected abstract Fin<Result> Run(RhinoCommandContext context);

    protected Fin<Result> Run<TState>(RhinoCommandContext context, CommandGraph<TState> graph) =>
        from active in Optional(context).ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
        from valid in Optional(graph).ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
        from result in valid.Run(context: active)
        select result;

    protected virtual Fin<Unit> Ready(RhinoCommandContext context) =>
        from active in Optional(context).ToFin(Fail: Op.Of(name: nameof(Ready)).InvalidInput())
        from _ in active.Scope.Context.Map(static _ => unit)
        select unit;

    protected virtual Fin<Result> Complete(RhinoCommandContext context, Result result) =>
        Optional(context).ToFin(Fail: Op.Of(name: nameof(Complete)).InvalidInput()).Bind(active => result switch {
            Result.Success => active.Edit.Redraw().Map(_ => result),
            _ => Fin.Succ(value: result),
        });

    protected virtual Result FailureResult(Error fault) =>
        fault switch {
            Fault.Cancelled => Result.Cancel,
            Error error => ((Func<Result>)(() => { RhinoApp.WriteLine($"{EnglishName}: {error.Message}"); return Result.Failure; }))(),
        };
}

public sealed record CommandGraph<TState>(
    TState Initial,
    Seq<CommandStage<TState>> Stages,
    Func<CommandCommitContext<TState>, Fin<Result>> Commit,
    CommandGraphEvents<TState> Events = default) {
    internal Fin<Result> Run(RhinoCommandContext context) =>
        from active in Optional(context).ToFin(Fail: Op.Of(name: nameof(CommandGraph<TState>)).InvalidInput())
        from commit in Optional(Commit).ToFin(Fail: Op.Of(name: nameof(CommandGraph<TState>)).InvalidInput())
        from stages in Stages switch {
            Seq<CommandStage<TState>> values when !values.IsEmpty => Fin.Succ(value: values),
            _ => Fin.Fail<Seq<CommandStage<TState>>>(error: Op.Of(name: nameof(CommandGraph<TState>)).InvalidInput()),
        }
        from final in new CommandStageContext<TState>(Context: active, State: Initial, Stages: stages, Index: 0, History: Seq<TState>(), Events: Events).Run()
        from result in commit(arg: new CommandCommitContext<TState>(Context: active, State: final.State, History: final.History))
        select result;
}

public abstract record CommandStage<TState>(string Name) {
    internal abstract Fin<PromptTransition<TState>> Run(CommandStageContext<TState> context);
}

public sealed record PromptStage<TState, TValue>(
    string Name,
    Func<TState, CommandInputRequest<TValue>> Input,
    Func<TState, Seq<CommandInputPolicy>> Policies,
    Func<CommandStageContext<TState>, CommandGet<TValue>, Fin<PromptTransition<TState>>> Receive,
    Func<PromptPreviewContext<TState>, Option<UiViewportPreview>>? Preview = null,
    Func<PromptGumballContext<TState>, Fin<Option<PromptTransition<TState>>>>? Gumball = null) : CommandStage<TState>(Name) {
    internal override Fin<PromptTransition<TState>> Run(CommandStageContext<TState> context) =>
        from input in Optional(Input).ToFin(Fail: Op.Of(name: Name).InvalidInput())
        from policies in Optional(Policies).ToFin(Fail: Op.Of(name: Name).InvalidInput())
        from receive in Optional(Receive).ToFin(Fail: Op.Of(name: Name).InvalidInput())
        from request in Optional(input(arg: context.State)).ToFin(Fail: Op.Of(name: Name).InvalidInput())
        from activePolicies in Optional(policies(arg: context.State)).ToFin(Fail: Op.Of(name: Name).InvalidInput())
        let transition = Atom(Option<PromptTransition<TState>>.None)
        let stagePolicies = activePolicies + PreviewPolicy(context: context) + GumballPolicy(context: context, transition: transition)
        let staged = request.With(policies: stagePolicies)
        from inputEvent in context.Context.Mode switch {
            RunMode.Scripted => context.Events.Token(context: context).ToFin(Fail: Op.Of(name: Name).InvalidInput()).Bind(token => context.Context.Input.Script(request: staged, token: token)),
            _ => context.Context.Input.GetEvent(request: staged),
        }
        from next in transition.Value.Case switch {
            PromptTransition<TState> value => Fin.Succ(value: value),
            _ => inputEvent.Kind switch {
                CommandInputEventKind.Cancel or CommandInputEventKind.Exit => Fin.Succ<PromptTransition<TState>>(value: new PromptTransition<TState>.Cancel()),
                CommandInputEventKind.Undo => Fin.Succ<PromptTransition<TState>>(value: new PromptTransition<TState>.Back()),
                _ => inputEvent.Result.ToFin(Fail: Op.Of(name: Name).InvalidResult()).Bind(got => receive(arg1: context, arg2: got)),
            },
        }
        select next;

    private Seq<CommandInputPolicy> PreviewPolicy(CommandStageContext<TState> context) =>
        Optional(Preview)
            .Map(preview => Seq(CommandInputPolicy.PointEvents(pointEvent => (pointEvent.Display.IsSome, preview(arg: new PromptPreviewContext<TState>(Stage: context, Event: pointEvent)).Case) switch {
                (true, UiViewportPreview active) => pointEvent.Preview(preview: active),
                _ => Fin.Succ(value: unit),
            })))
            .IfNone(Seq<CommandInputPolicy>());

    private Seq<CommandInputPolicy> GumballPolicy(CommandStageContext<TState> context, Atom<Option<PromptTransition<TState>>> transition) =>
        Optional(Gumball)
            .Map(project => Seq(CommandInputPolicy.PointEvents(pointEvent => project(arg: new PromptGumballContext<TState>(Stage: context, Event: pointEvent)).Map(next => next.Iter(value => {
                _ = transition.Swap(_ => Some(value));
                _ = pointEvent.Getter.InterruptMouseMove();
            })))))
            .IfNone(Seq<CommandInputPolicy>());
}

public sealed record CommandStageContext<TState>(
    RhinoCommandContext Context,
    TState State,
    Seq<CommandStage<TState>> Stages,
    int Index,
    Seq<TState> History,
    CommandGraphEvents<TState> Events) {
    internal Fin<CommandStageContext<TState>> Run() =>
        (Index >= Stages.Count) switch {
            true => Fin.Succ(value: this),
            false => Stages[Index].Run(context: this).Bind(Apply),
        };

    private Fin<CommandStageContext<TState>> Apply(PromptTransition<TState> transition) =>
        Optional(transition).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidInput()).Bind(valid => valid switch {
            PromptTransition<TState>.Stay stay => (this with { State = stay.State }).Run(),
            PromptTransition<TState>.Forward next => (this with { State = next.State, Index = Index + 1, History = Seq(State) + History }).Run(),
            PromptTransition<TState>.Back => History.IsEmpty switch {
                false => (this with { State = History[0], Index = Math.Max(0, Index - 1), History = History.Tail }).Run(),
                _ => Run(),
            },
            PromptTransition<TState>.Commit commit => Fin.Succ(value: this with { State = commit.State, Index = Stages.Count }),
            PromptTransition<TState>.Cancel => Fin.Fail<CommandStageContext<TState>>(error: new Fault.Cancelled()),
            _ => Fin.Fail<CommandStageContext<TState>>(error: Op.Of(name: nameof(Apply)).InvalidInput()),
        });

    public Fin<Seq<string>> Files(Rasm.Rhino.UI.UiFileSpec spec) =>
        Context.Mode switch {
            RunMode.Scripted => Events.Files(context: this, spec: spec),
            _ => Context.Ui.Use(intent: Rasm.Rhino.UI.UiIntent.File(spec: spec)),
        };
}

public readonly record struct CommandCommitContext<TState>(RhinoCommandContext Context, TState State, Seq<TState> History) {
    public RhinoDoc Document => Context.Document;
    public DocumentEdit Edit => Context.Edit;
    public Rasm.Rhino.UI.RhinoUi Ui => Context.Ui;
}

public readonly record struct PromptPreviewContext<TState>(CommandStageContext<TState> Stage, CommandPointEvent Event) {
    public TState State => Stage.State;
    public RhinoCommandContext Context => Stage.Context;
    public Option<Point3d> Point => Event.Point;
    public Option<RhinoViewport> Viewport => Event.Viewport;
    public Option<DisplayPipeline> Display => Event.Display;
    public Option<CommandSelection.Reference> Reference => Event.Getter switch {
        GetPoint getter => CommandSelection.Reference.Of(getter: getter),
        _ => Option<CommandSelection.Reference>.None,
    };
}

public readonly record struct PromptGumballContext<TState>(CommandStageContext<TState> Stage, CommandPointEvent Event) {
    public TState State => Stage.State;
    public RhinoCommandContext Context => Stage.Context;
    public Option<Point3d> Point => Event.Point;
    public Option<UiGumballSnapshot> Snapshot => Event.GumballSnapshot;
    public Fin<bool> Pick(global::Rhino.Input.Custom.PickContext pick) => Event.PickGumball(pick: pick);
    public Fin<bool> Update(Line worldLine) => Event.UpdateGumball(worldLine: worldLine);
    public Fin<bool> Update(Plane frame) => Event.UpdateGumball(frame: frame);
}

public readonly record struct CommandGraphEvents<TState>(
    Func<CommandStageContext<TState>, Option<string>>? ScriptedToken = null,
    Func<CommandStageContext<TState>, Rasm.Rhino.UI.UiFileSpec, Fin<Seq<string>>>? ScriptedFiles = null) {
    public Option<string> Token(CommandStageContext<TState> context) =>
        Optional(ScriptedToken).Bind(project => project(arg: context));

    public Fin<Seq<string>> Files(CommandStageContext<TState> context, Rasm.Rhino.UI.UiFileSpec spec) =>
        Optional(ScriptedFiles).Map(project => project(arg1: context, arg2: spec)).IfNone(() =>
            spec.FileName.Map(value => Seq(value)).ToFin(Fail: Op.Of(name: nameof(Files)).InvalidInput()));
}

public abstract record PromptTransition<TState> {
    private PromptTransition() { }

    public sealed record Stay(TState State) : PromptTransition<TState>;
    public sealed record Forward(TState State) : PromptTransition<TState>;
    public sealed record Back : PromptTransition<TState>;
    public sealed record Commit(TState State) : PromptTransition<TState>;
    public sealed record Cancel : PromptTransition<TState>;
}
