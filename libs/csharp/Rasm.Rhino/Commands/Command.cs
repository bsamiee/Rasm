using Rasm.Rhino.Exchange;
using Result = Rhino.Commands.Result;
using UiGumballSnapshot = Rasm.Rhino.UI.UiGumballSnapshot;
using UiViewportPreview = Rasm.Rhino.UI.UiViewportPreview;

namespace Rasm.Rhino.Commands;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record CommandGraph<TState>(
    TState Initial,
    Seq<CommandStage<TState>> Stages,
    Func<CommandCommitContext<TState>, Fin<Result>> Commit,
    CommandGraphEvents<TState> Events = default) {
    public Fin<CommandGraph<TState>> Append(CommandStage<TState> stage) =>
        from valid in Optional(stage).ToFin(Fail: Op.Of(name: nameof(Append)).InvalidInput())
        select this with { Stages = Stages + Seq(valid) };

    internal Fin<Result> Run(RhinoCommandContext context) =>
        from active in Optional(context).ToFin(Fail: Op.Of(name: nameof(CommandGraph<>)).InvalidInput())
        from commit in Optional(Commit).ToFin(Fail: Op.Of(name: nameof(CommandGraph<>)).InvalidInput())
        from stages in Stages switch {
            Seq<CommandStage<TState>> values when !values.IsEmpty => Fin.Succ(value: values),
            _ => Fin.Fail<Seq<CommandStage<TState>>>(error: Op.Of(name: nameof(CommandGraph<>)).InvalidInput()),
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
    Func<PromptGumballContext<TState>, Fin<Option<PromptTransition<TState>>>>? Gumball = null,
    Func<CommandStageContext<TState>, CommandOptionValue, Fin<TState>>? OptionLens = null,
    Func<CommandStageContext<TState>, Fin<PromptTransition<TState>>>? Enter = null,
    Func<CommandStageContext<TState>, CommandGet<TValue>, Fin<Unit>>? Rejected = null) : CommandStage<TState>(Name) {
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
                CommandInputEventKind.Nothing => (inputEvent.Result.Bind(static got => got.Option).Case, Optional(OptionLens).Case) switch {
                    (CommandOptionValue selected, Func<CommandStageContext<TState>, CommandOptionValue, Fin<TState>> lens) =>
                        from state in lens(arg1: context, arg2: selected)
                        select (PromptTransition<TState>)new PromptTransition<TState>.Stay(State: state),
                    _ => Optional(Enter)
                        .Map(run => run(arg: context))
                        .IfNone(Fin.Succ<PromptTransition<TState>>(value: new PromptTransition<TState>.Stay(State: context.State))),
                },
                CommandInputEventKind.Rejected => inputEvent.Result.ToFin(Fail: Op.Of(name: Name).InvalidResult()).Bind(got =>
                    Optional(Rejected)
                        .Map(run => run(arg1: context, arg2: got).Map(_ => (PromptTransition<TState>)new PromptTransition<TState>.Stay(State: context.State)))
                        .IfNone(Fin.Succ<PromptTransition<TState>>(value: new PromptTransition<TState>.Stay(State: context.State)))),
                CommandInputEventKind.NoResult or CommandInputEventKind.Miss or CommandInputEventKind.Timeout => Fin.Succ<PromptTransition<TState>>(value: new PromptTransition<TState>.Stay(State: context.State)),
                CommandInputEventKind.Option => Optional(OptionLens).Case switch {
                    Func<CommandStageContext<TState>, CommandOptionValue, Fin<TState>> lens =>
                        from got in inputEvent.Result.ToFin(Fail: Op.Of(name: Name).InvalidResult())
                        from selected in got.Option.ToFin(Fail: Op.Of(name: Name).InvalidResult())
                        from state in lens(arg1: context, arg2: selected)
                        select (PromptTransition<TState>)new PromptTransition<TState>.Stay(State: state),
                    _ => inputEvent.Result.ToFin(Fail: Op.Of(name: Name).InvalidResult()).Bind(got => receive(arg1: context, arg2: got)),
                },
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
        Optional(transition).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidInput()).Bind(valid => valid.Apply(context: this));

    public Fin<Seq<FileEndpoint>> Files(FilePrompt prompt) =>
        Context.Mode switch {
            RunMode.Scripted => Events.Files(context: this, prompt: prompt),
            _ => Context.Files.Run(operation: FileOp.Prompt(prompt: prompt)),
        };

    public Fin<Unit> Status(UI.UiStatus status) =>
        Context.Ui.Use(intent: UI.UiIntent.Status(status: status));

    public Fin<T> Use<T>(UI.UiIntent<T> intent, Func<CommandStageContext<TState>, Fin<T>> scripted) =>
        from validIntent in Optional(intent).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
        from run in Optional(scripted).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
        from result in Context.Ui.Use(intent: validIntent.WithScripted(fallback: (_, _) => run(arg: this)))
        select result;
}

public readonly record struct CommandCommitContext<TState>(RhinoCommandContext Context, TState State, Seq<TState> History) {
    public RhinoDoc Document => Context.Document;
    public DocumentEdit Edit => Context.Edit;
    public UI.RhinoUi Ui => Context.Ui;
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
    public Fin<bool> Pick(PickContext pick) => Event.PickGumball(pick: pick);
    public Fin<bool> Update(Line worldLine) => Event.UpdateGumball(worldLine: worldLine);
    public Fin<bool> Update(Plane frame) => Event.UpdateGumball(frame: frame);
}

public readonly record struct CommandGraphEvents<TState>(
    Func<CommandStageContext<TState>, Option<string>>? ScriptedToken = null,
    Func<CommandStageContext<TState>, FilePrompt, Fin<Seq<FileEndpoint>>>? ScriptedFilePrompt = null) {
    public Option<string> Token(CommandStageContext<TState> context) =>
        Optional(ScriptedToken).Bind(project => project(arg: context));

    public Fin<Seq<FileEndpoint>> Files(CommandStageContext<TState> context, FilePrompt prompt) =>
        Optional(ScriptedFilePrompt)
            .Map(project => project(arg1: context, arg2: prompt))
            .IfNone(() =>
                from valid in Optional(prompt).ToFin(Fail: Op.Of(name: nameof(Files)).InvalidInput())
                from defaults in valid.Defaults(op: Op.Of(name: nameof(Files)))
                select defaults);
}

// Not `[Union]`: generic-Union source-gen forces `where TState : allows ref struct`, incompatible with record-based `CommandStageContext<TState>`.
public abstract record PromptTransition<TState> {
    private PromptTransition() { }

    internal Fin<CommandStageContext<TState>> Apply(CommandStageContext<TState> context) =>
        Optional(context).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidInput()).Bind(active => this switch {
            Stay stay => (active with { State = stay.State }).Run(),
            Forward next => (active with { State = next.State, Index = active.Index + 1, History = Seq(active.State) + active.History }).Run(),
            Back => active.History.IsEmpty switch {
                false => (active with { State = active.History[0], Index = Math.Max(0, active.Index - 1), History = active.History.Tail }).Run(),
                true => active.Run(),
            },
            Commit commit => Fin.Succ(value: active with { State = commit.State, Index = active.Stages.Count }),
            Cancel => Fin.Fail<CommandStageContext<TState>>(error: new Fault.Cancelled()),
            _ => Fin.Fail<CommandStageContext<TState>>(error: Op.Of(name: nameof(Apply)).InvalidInput()),
        });

    public sealed record Stay(TState State) : PromptTransition<TState>;
    public sealed record Forward(TState State) : PromptTransition<TState>;
    public sealed record Back : PromptTransition<TState>;
    public sealed record Commit(TState State) : PromptTransition<TState>;
    public sealed record Cancel : PromptTransition<TState>;
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmCommand<TSelf> : Command where TSelf : RasmCommand<TSelf> {
    public override string EnglishName => typeof(TSelf).Name;

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) =>
        UI.RhinoUi.Protect(valid: () =>
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
            Error error => Failure(name: EnglishName, message: error.Message),
        };

    private static Result Failure(string name, string message) {
        RhinoApp.WriteLine(message: $"{name}: {message}");
        return Result.Failure;
    }
}
