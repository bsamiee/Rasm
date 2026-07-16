# [RASM_RHINO_COMMAND]

Native command owner (`Rasm.Rhino.Commands`). One generic stage algebra carries every command shape — effect, acquisition, branch, commit, terminal — as cases of one closed `Stage<TModel>` union driven by one bounded continue-or-done fold, so a command is a row table of keyed stages over one immutable state record, never a subclass ladder or a virtual escape hatch. `Rhino.Commands.Command` derivation is a thin adapter: host lifecycle enters at `RunCommand`, admits a `DocumentSession` from the live handle and run mode, drives the flow, and leaves as a `CommandVerdict` mapped onto the native `Result` — no host member is named anywhere else on the page's flow path. Replay and scripted execution are policy rows: `ReplayHistory` routes to the one replay delegate, and `RhinoApp` script dispatch is one sanctioned `ScriptOp` union — every other `RunScript` spelling in the package is dead. Command registry resolution, the nested command stack, and the begin/end/undo pulse events are typed projections over the static host registry.

## [01]-[INDEX]

- [02]-[VERDICT_AND_POLICY]: `CommandVerdict` the terminal vocabulary with both native directions, `CommandPolicy` the per-command policy row.
- [03]-[STAGE_ALGEBRA]: `StageKey`, `Stage<TModel>`, `CommandFlow<TState>`, and the bounded drive fold.
- [04]-[NATIVE_ADAPTER]: `RasmCommand<TSelf,TState>` — the one host lifecycle ingress/egress — plus the registry surface and the `CommandPulse` event rows.
- [05]-[SCRIPT_ROW]: `ScriptOp` — the one sanctioned script execution row.
- [06]-[SURFACE_LEDGER]: the page's owner table.

## [02]-[VERDICT_AND_POLICY]

- Owner: `CommandVerdict` `[SmartEnum<int>]` — the terminal command vocabulary with the `Native` column carrying the host `Result` projection; `OfNative` is the inverse on the same owner, so the correspondence is declared once in the rows and both directions read it. `CommandPolicy` — the per-command policy record: the session needs the command demands at admission, the replay delegate, and the stage budget bounding the drive fold.
- Law: cancel and nothing are verdicts, never faults — a getter cancel or an empty-enter terminal folds to `Cancelled`/`Empty` on the verdict rail, and only a genuine operation failure lands `Failed`; the host-shutdown terminal is `Exit`, which `RunCommand` must return as `Result.ExitRhino` or host shutdown is left undefined.
- Law: the policy row is the whole per-command variation — needs, replay, budget — so two commands never differ by adapter code; a command that mutates names `SessionNeed.Mutate` and `SessionNeed.Undo` here, and the admission failure surfaces as `Failed` before any stage runs.
- Growth: a new terminal is one row with its `Native` column; the drive fold, the adapter, and every consumer read it with zero signature change.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CommandVerdict : IDetachedDocumentResult {
    public static readonly CommandVerdict Completed = new(key: 0, native: Result.Success);
    public static readonly CommandVerdict Cancelled = new(key: 1, native: Result.Cancel);
    public static readonly CommandVerdict Empty = new(key: 2, native: Result.Nothing);
    public static readonly CommandVerdict Failed = new(key: 3, native: Result.Failure);
    public static readonly CommandVerdict Exit = new(key: 4, native: Result.ExitRhino);

    public Result Native { get; }

    public static CommandVerdict OfNative(Result result) =>
        Items.AsIterable().Find(row => row.Native == result).IfNone(Failed);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record CommandPolicy(
    Seq<SessionNeed> Needs,
    Option<Func<ReplayHistoryData, bool>> Replay = default,
    int StageBudget = 64) {
    public static CommandPolicy Reading { get; } = new(Needs: Seq(SessionNeed.Read));
    public static CommandPolicy Mutating { get; } =
        new(Needs: Seq(SessionNeed.Read, SessionNeed.Mutate, SessionNeed.Undo, SessionNeed.Redraw));
    public static CommandPolicy Prompting { get; } =
        new(Needs: Seq(SessionNeed.Read, SessionNeed.Acquire, SessionNeed.Mutate, SessionNeed.Undo, SessionNeed.Redraw));
}
```

## [03]-[STAGE_ALGEBRA]

- Owner: `StageKey` `[ValueObject<string>]` — the stage identity a branch routes on; `Stage<TModel>` `[Union]` — the ONE transition algebra: `Effect` runs typed work over the turn, `Prompt` composes the acquisition matrix and folds the receipt into state, `Branch` routes to the next key from state alone, `Commit` plans a `TableTransaction` and folds its `TableReceipt`, `Halt` carries the terminal verdict as payload — cancel, exit, success, and nothing are one case discriminated by the verdict value, never sibling cases. Its type parameter is `TModel` because the generated state-threaded `Switch` reserves `TState` for its own state type parameter; consumers instantiate with their command state as the argument. `CommandFlow<TState>` — the keyed row table with its entry key; `CommandTurn<TState>` — the state the stages read: session plus domain state.
- Entry: `CommandFlow<TState>.Drive(DocumentSession, TState, CommandPolicy) : Fin<(CommandVerdict Verdict, TState State)>` — the one bounded fold: resolve the current key, dispatch the stage, advance or finish, and refuse a flow that exhausts the stage budget with a typed fault instead of looping the UI thread.
- Law: the census-era `EffectCase`/`BranchCase`/`CommitCase` class roster and the `PromptStage` virtual escape hatch are dead — a prompt is the `Prompt` case whose `Acquire` request and fold are ordinary case payload, so every stage kind participates in the one generated `Switch` and a new stage kind is one case that breaks every drive site at compile time.
- Law: stage succession is data — `Effect`, `Prompt`, and `Commit` carry their successor key as payload and `Branch` computes one, so the flow topology is recoverable from the row table alone; an acquisition cancel short-circuits the fold to `Cancelled` without running the successor, and an empty-accept terminal folds to `Empty` by the same rail.
- Law: undo is the journal, never a stage case — the drive fold journals every stage entry as one `(key, state)` frame, an `Undone` acquisition terminal pops the journal and re-enters the previous stage with its entry state, and an undo on the empty journal folds to `Cancelled`; the census-era per-stage `Back` transition subtype is dead because rewinding is fold state, not stage vocabulary.
- Law: a `Commit` stage is the only mutation site — its plan routes through `Tables.Commit` so undo bracketing, redraw suppression, and receipt evidence arrive from the document rail; a stage mutating the document outside `Commit` has no undo story and is the deleted form.
- Boundary: the drive fold never touches a host member — the session is its whole document reach, and acquisition, options, and selection arrive through their owning pages composed inside the case payloads.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct StageKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        validationError = string.IsNullOrWhiteSpace(value) ? new ValidationError("stage key is blank") : validationError;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Stage<TModel> {
    private Stage() { }
    public sealed record Effect(Func<CommandTurn<TModel>, Fin<TModel>> Run, StageKey Next) : Stage<TModel>;
    public sealed record Prompt(
        Func<TModel, Acquire> Request,
        Func<TModel, AcquiredReceipt, Fin<TModel>> Fold,
        StageKey Next) : Stage<TModel>;
    public sealed record Branch(Func<TModel, StageKey> Route) : Stage<TModel>;
    public sealed record Commit(
        Func<CommandTurn<TModel>, Fin<TableTransaction>> Plan,
        Func<TModel, TableReceipt, TModel> Fold,
        StageKey Next) : Stage<TModel>;
    public sealed record Halt(CommandVerdict Verdict) : Stage<TModel>;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record FlowStep<TModel> {
    private FlowStep() { }
    public sealed record Advance(StageKey Key, TModel State) : FlowStep<TModel>;
    public sealed record Rewind(TModel State) : FlowStep<TModel>;
    public sealed record Finished(CommandVerdict Verdict, TModel State) : FlowStep<TModel>;
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct CommandTurn<TState>(DocumentSession Session, TState State);

public sealed record CommandFlow<TState>(Seq<(StageKey Key, Stage<TState> Stage)> Rows, StageKey Entry) {
    public static Fin<CommandFlow<TState>> Of(StageKey entry, params ReadOnlySpan<(StageKey Key, Stage<TState> Stage)> rows) {
        Op op = Op.Of(name: nameof(CommandFlow<TState>));
        Seq<(StageKey Key, Stage<TState> Stage)> table = toSeq(rows.ToArray());
        return from _ in guard(!table.IsEmpty, op.InvalidInput())
               from __ in guard(table.Map(static row => row.Key).Distinct().Count == table.Count, op.InvalidInput())
               from ___ in guard(table.Exists(row => row.Key == entry), op.InvalidInput())
               select new CommandFlow<TState>(Rows: table, Entry: entry);
    }

    public Fin<(CommandVerdict Verdict, TState State)> Drive(DocumentSession session, TState seed, CommandPolicy policy) {
        Op op = Op.Of();
        CommandFlow<TState> flow = this;
        return from active in Optional(policy).ToFin(Fail: op.InvalidInput())
               from outcome in Loop(
                   flow: flow, session: session, key: flow.Entry, state: seed,
                   trail: Seq<(StageKey Key, TState State)>(), remaining: active.StageBudget, op: op)
               select outcome;
    }

    private static Fin<(CommandVerdict, TState)> Loop(
        CommandFlow<TState> flow, DocumentSession session, StageKey key, TState state,
        Seq<(StageKey Key, TState State)> trail, int remaining, Op op) =>
        remaining <= 0
            ? Fin.Fail<(CommandVerdict, TState)>(error: op.InvalidResult(detail: nameof(CommandPolicy.StageBudget)))
            : flow.Rows.Find(row => row.Key == key)
                .ToFin(Fail: op.MissingContext())
                .Bind(row => row.Stage.Switch(
                    state: new CommandTurn<TState>(Session: session, State: state),
                    effect: static (turn, stage) => stage.Run(arg: turn)
                        .Map(next => (FlowStep<TState>)new FlowStep<TState>.Advance(Key: stage.Next, State: next)),
                    prompt: static (turn, stage) => Acquisition.Get(session: turn.Session, request: stage.Request(arg: turn.State))
                        .Bind(receipt => receipt.Terminal.Switch(
                            state: (Turn: turn, Stage: stage, Receipt: receipt),
                            value: static ctx => ctx.Stage.Fold(arg1: ctx.Turn.State, arg2: ctx.Receipt)
                                .Map(next => (FlowStep<TState>)new FlowStep<TState>.Advance(Key: ctx.Stage.Next, State: next)),
                            cancelled: static ctx => Fin.Succ<FlowStep<TState>>(
                                value: new FlowStep<TState>.Finished(Verdict: CommandVerdict.Cancelled, State: ctx.Turn.State)),
                            nothing: static ctx => Fin.Succ<FlowStep<TState>>(
                                value: new FlowStep<TState>.Finished(Verdict: CommandVerdict.Empty, State: ctx.Turn.State)),
                            undone: static ctx => Fin.Succ<FlowStep<TState>>(
                                value: new FlowStep<TState>.Rewind(State: ctx.Turn.State)),
                            exit: static ctx => Fin.Succ<FlowStep<TState>>(
                                value: new FlowStep<TState>.Finished(Verdict: CommandVerdict.Exit, State: ctx.Turn.State)))),
                    branch: static (turn, stage) => Fin.Succ<FlowStep<TState>>(
                        value: new FlowStep<TState>.Advance(Key: stage.Route(arg: turn.State), State: turn.State)),
                    commit: static (turn, stage) =>
                        from plan in stage.Plan(arg: turn)
                        from receipt in Tables.Commit(session: turn.Session, transaction: plan)
                        select (FlowStep<TState>)new FlowStep<TState>.Advance(
                            Key: stage.Next, State: stage.Fold(arg1: turn.State, arg2: receipt)),
                    halt: static (turn, stage) => Fin.Succ<FlowStep<TState>>(
                        value: new FlowStep<TState>.Finished(Verdict: stage.Verdict, State: turn.State))))
                .Bind(step => step.Switch(
                    state: (Flow: flow, Session: session, Key: key, Entry: state, Trail: trail, Remaining: remaining, Op: op),
                    advance: static (held, next) => Loop(
                        flow: held.Flow, session: held.Session, key: next.Key, state: next.State,
                        trail: Seq((held.Key, held.Entry)) + held.Trail, remaining: held.Remaining - 1, op: held.Op),
                    rewind: static (held, _) => held.Trail.Head.Match(
                        Some: frame => Loop(
                            flow: held.Flow, session: held.Session, key: frame.Key, state: frame.State,
                            trail: held.Trail.Tail, remaining: held.Remaining - 1, op: held.Op),
                        None: () => Fin.Succ(value: (CommandVerdict.Cancelled, held.Entry))),
                    finished: static (_, terminal) => Fin.Succ(value: (terminal.Verdict, terminal.State))));
}
```

## [04]-[NATIVE_ADAPTER]

- Owner: `RasmCommand<TSelf,TState>` — the abstract `Rhino.Commands.Command` derivation: identity, the flow declaration, the seed, and the policy row are the whole subclass surface. `CommandRegistry` — the typed projection over the static host registry. `CommandPulse` `[SmartEnum<int>]` — the begin/end/undo event rows binding the static host events through the document `Subscription` algebra.
- Entry: `RunCommand(RhinoDoc, RunMode)` is sealed — it admits `DocumentSession.Of(new SessionSource.Live(doc), SessionMode.OfRunMode(mode), policy needs)`, drives the flow, disposes the session, and maps the verdict through `Native`; `ReplayHistory(ReplayHistoryData)` is sealed and routes to the policy's replay delegate, answering `false` where no row exists so a replay-free command never re-solves.
- Law: the adapter is the package's only `Rhino.Commands.Command` derivation site and the only site naming `RunCommand`, `ReplayHistory`, or `Result` — the flow, the stages, and every consumer hold the session and the verdict vocabulary; a second host-derived base class owning state algebra is the deleted form.
- Law: an admission failure is a verdict, not a throw — a session the policy cannot prove (wrong lane, read-only document, engaged getter) folds to `Failed` and writes its fault through `RhinoApp.WriteLine` so the operator sees the refusal cause on the command line.
- Law: registry answers are read live per call — the host mutates the command roster on plug-in load, so no roster is cached; `Stack` and `Active` answer the nested-command question the transparent-command policy on the acquisition page reads.
- Law: command style is declaration material — transparency, hidden, script-runner, do-not-repeat, and not-undoable ride the `[CommandStyle]` attribute on the concrete command class (`Style` rows `None`/`Hidden`/`ScriptRunner`/`Transparent`/`DoNotRepeat`/`NotUndoable`/`System`), and history participation is implicit through the `ReplayHistory` override plus an add-time `HistoryRecord`; no runtime flag re-derives either.
- Law: prompt observation is the HostUi shell's `PromptWatch` — its `PromptFact` stream carries the typed prompt, default-as-absence, and the detached command-line option roster — so no prompt row exists here; a signal-only `RhinoApp.CommandPromptChanged` row beside that owner is the split-brain the vocabulary deletes.
- RESEARCH: the `CommandEventArgs` and `UndoRedoEventArgs` member surfaces and the `MostRecentCommandDescription` member spellings are unverified — the pulse rows deliver signal-only payloads and `Recent` answers a tally until the members verify, at which point the payloads become typed projections with zero new surface.
- Growth: a new lifecycle signal is one `CommandPulse` row; a new registry question is one member on `CommandRegistry` over the verified host surface.

```csharp
// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmCommand<TSelf, TState> : Command
    where TSelf : RasmCommand<TSelf, TState> {
    protected abstract CommandPolicy Policy { get; }
    protected abstract TState Seed { get; }
    protected abstract Fin<CommandFlow<TState>> Flow { get; }

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) {
        Op op = Op.Of(name: EnglishName);
        Fin<CommandVerdict> outcome =
            from flow in Flow
            from lane in SessionMode.OfRunMode(mode: mode, key: op)
            from session in DocumentSession.Of(
                source: new SessionSource.Live(Document: doc),
                mode: lane,
                needs: Policy.Needs.ToArray())
            from verdict in op.Catch(() => {
                using DocumentSession active = session;
                return flow.Drive(session: active, seed: Seed, policy: Policy).Map(static driven => driven.Verdict);
            })
            select verdict;
        return outcome.Match(
            Succ: static verdict => verdict.Native,
            Fail: error => {
                RhinoApp.WriteLine(message: error.Message);
                return CommandVerdict.Failed.Native;
            });
    }

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) =>
        Policy.Replay.Map(replay => replay(arg: replayData)).IfNone(noneValue: false);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CommandRegistry {
    public static bool Valid(string name) => Command.IsValidCommandName(name: name);

    public static Option<Guid> Resolve(string name, bool searchEnglish = true) =>
        Optional(Command.LookupCommandId(name: name, searchForEnglishName: searchEnglish))
            .Filter(static id => id != Guid.Empty);

    public static Option<string> NameOf(Guid commandId, bool english = true) =>
        Optional(Command.LookupCommandName(commandId: commandId, englishName: english))
            .Filter(static name => name.Length > 0);

    public static Seq<string> Names(bool english = true, bool loaded = true) =>
        toSeq(Command.GetCommandNames(english: english, loaded: loaded));

    public static Seq<Guid> Stack => toSeq(Command.GetCommandStack());

    public static bool Active => Command.InCommand();

    public static bool ScriptRunning => Command.InScriptRunnerCommand();

    public static Fin<int> Recent(Op key) =>
        key.Catch(() => Fin.Succ(value: Command.GetMostRecentCommands().Length));

    public static Fin<Unit> Proxy(RunCommandDelegate callback, DocumentSession session, Option<object> data, Op key) =>
        from active in Optional(callback).ToFin(Fail: key.InvalidInput())
        from _ in session.Demand(
            use: document => key.Catch(() => {
                Command.RunProxyCommand(commandCallback: active, doc: document, data: data.IfNoneUnsafe((object?)null));
                return Fin.Succ(value: session.Key);
            }),
            key: key,
            needs: [SessionNeed.Read])
        select unit;

    public static string Prompt => RhinoApp.CommandPrompt;
}

[SmartEnum<int>]
public sealed partial class CommandPulse {
    public static readonly CommandPulse Begin = new(key: 0, observe: static deliver => Subscription.Attach(
        subscribe: static (EventHandler<CommandEventArgs> handler) => Command.BeginCommand += handler,
        unsubscribe: static handler => Command.BeginCommand -= handler,
        handler: (_, _) => ignore(deliver(arg: unit))));
    public static readonly CommandPulse End = new(key: 1, observe: static deliver => Subscription.Attach(
        subscribe: static (EventHandler<CommandEventArgs> handler) => Command.EndCommand += handler,
        unsubscribe: static handler => Command.EndCommand -= handler,
        handler: (_, _) => ignore(deliver(arg: unit))));
    public static readonly CommandPulse UndoRedo = new(key: 2, observe: static deliver => Subscription.Attach(
        subscribe: static (EventHandler<UndoRedoEventArgs> handler) => Command.UndoRedo += handler,
        unsubscribe: static handler => Command.UndoRedo -= handler,
        handler: (_, _) => ignore(deliver(arg: unit))));

    [UseDelegateFromConstructor]
    public partial Fin<Subscription> Observe(Func<Unit, Fin<Unit>> deliver);
}
```

## [05]-[SCRIPT_ROW]

- Owner: `ScriptOp` `[Union]` — the ONE sanctioned script execution row: `Macro` runs command-line text against the session's document through the serial-targeted `RhinoApp.RunScript` overloads, `Named` executes one registered command through `RhinoApp.ExecuteCommand` and returns its verdict. Every other `RunScript` spelling in the package — the census-era block-operation script fallback included — is dead.
- Entry: `Scripting.Run(DocumentSession, ScriptOp) : Fin<CommandVerdict>` — one `Demand` window under `SessionNeed.Acquire`, whose mode admission already refuses the headless lane (script dispatch requires the windowed runtime), targeting the session's `DocKey` so a macro never runs against whichever document happens to be active.
- Law: a macro answers a boolean, a named command answers a `Result` — both project onto the verdict vocabulary at this seam, so a scripted cancel is `Cancelled` on the same rail an interactive cancel rides.
- Law: the MRU display string is case payload, never a second overload family — `Macro` carries `Display` as `Option<string>` and the dispatch selects the three-argument or four-argument host overload from the option.
- Boundary: script text grammar, token feeds for scripted acquisition, and option-token decode belong to the acquisition and options pages; this row only executes.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptOp {
    private ScriptOp() { }
    public sealed record Macro(string Text, bool Echo = true, Option<string> Display = default) : ScriptOp;
    public sealed record Named(string CommandName) : ScriptOp;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Scripting {
    public static Fin<CommandVerdict> Run(DocumentSession session, ScriptOp script) {
        Op op = Op.Of();
        return from request in Optional(script).ToFin(Fail: op.InvalidInput())
               from verdict in session.Demand(
                   use: document => request.Switch(
                       state: (Key: session.Key, Document: document, Op: op),
                       macro: static (ctx, run) =>
                           from text in ctx.Op.AcceptText(value: run.Text)
                           from ok in ctx.Op.Catch(() => Fin.Succ(value: run.Display.Case switch {
                               string display => RhinoApp.RunScript(
                                   documentSerialNumber: (uint)ctx.Key, script: text, mruDisplayString: display, echo: run.Echo),
                               _ => RhinoApp.RunScript(documentSerialNumber: (uint)ctx.Key, script: text, echo: run.Echo),
                           }))
                           select ok ? CommandVerdict.Completed : CommandVerdict.Failed,
                       named: static (ctx, run) =>
                           from name in ctx.Op.AcceptText(value: run.CommandName)
                           from _ in guard(CommandRegistry.Valid(name: name), ctx.Op.InvalidInput())
                           from result in ctx.Op.Catch(() => Fin.Succ(value: RhinoApp.ExecuteCommand(document: ctx.Document, commandName: name)))
                           select CommandVerdict.OfNative(result: result)),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select verdict;
    }
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [CONCERN]           | [OWNER]                     | [FORM]                                      | [ENTRY]                                |
| :-----: | :------------------ | :-------------------------- | :------------------------------------------ | :------------------------------------- |
|  [01]   | terminal vocabulary | `CommandVerdict`            | rows with `Native` column, both directions  | `Native` / `OfNative`                  |
|  [02]   | per-command policy  | `CommandPolicy`             | needs + replay + budget record              | `RasmCommand.Policy`                   |
|  [03]   | transition algebra  | `Stage<TModel>`             | one `[Union]`, prompt and commit as cases   | `CommandFlow.Drive`                    |
|  [04]   | host adaptation     | `RasmCommand<TSelf,TState>` | sealed `RunCommand`/`ReplayHistory` adapter | host command registration              |
|  [05]   | registry projection | `CommandRegistry`           | live typed reads over host registry         | `Resolve` / `Names` / `Stack`          |
|  [06]   | lifecycle pulse     | `CommandPulse`              | host-event rows via `Subscription`          | `Observe(deliver) : Fin<Subscription>` |
|  [07]   | script execution    | `ScriptOp`                  | one union, serial-targeted dispatch         | `Scripting.Run(session, script)`       |
