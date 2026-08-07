# [RASM_RHINO_COMMAND]

`CommandFlow<TState>.Drive` interprets one admitted, bounded command program over immutable state. Host lifecycle enters once through `RasmCommand<TSelf,TState>`, while acquisition and mutation remain calls into their owning rails.

## [01]-[INDEX]

- [02]-[TERMINAL_POLICY]: `CommandVerdict`, `ScriptEcho`, `ReplayHook`, `HistoryOwner`, `FaultNotice`, and `CommandPolicy`.
- [03]-[PROGRAM]: `StageKey`, `Stage<TState>`, `FlowStep<TState>`, `CommandTurn<TState>`, and `CommandFlow<TState>`.
- [04]-[HOST_ADAPTER]: `CommandFaults` and the one `RasmCommand<TSelf,TState>` derivation.
- [05]-[REGISTRY_AND_EVENTS]: `CommandQuery`/`CommandAnswer`, the `CommandFact` family, `CommandObserver`, `CommandPulse`, and `CommandRegistry`.
- [06]-[SCRIPT]: `ScriptOp` and the `Scripting` run/proxy pair.
- [07]-[RESEARCH]: open verification rows.

## [02]-[TERMINAL_POLICY]

`CommandVerdict` preserves every native terminal in both directions. `CommandPolicy` admits the session demand, replay behavior, and stage budget as one value before a flow starts. `ReplayHook` and `HistoryOwner` are the two seam values `Objects/history.md` composes upward — the regrowth body and the command identity a history record keys on.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Linq;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;

namespace Rasm.Rhino.Commands;

// --- [TYPES] -----------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CommandVerdict : IDetachedDocumentResult {
    public static readonly CommandVerdict Completed = new(key: 0, native: Result.Success);
    public static readonly CommandVerdict Cancelled = new(key: 1, native: Result.Cancel);
    public static readonly CommandVerdict Empty = new(key: 2, native: Result.Nothing);
    public static readonly CommandVerdict Failed = new(key: 3, native: Result.Failure);
    public static readonly CommandVerdict Unknown = new(key: 4, native: Result.UnknownCommand);
    public static readonly CommandVerdict Dismissed = new(key: 5, native: Result.CancelModelessDialog);
    public static readonly CommandVerdict Exit = new(key: 6, native: Result.ExitRhino);

    public Result Native { get; }

    public static Fin<CommandVerdict> OfNative(Result native, Op key) =>
        Items.AsIterable().Find(verdict => verdict.Native == native).ToFin(Fail: key.InvalidResult(detail: native.ToString()));
}

[SmartEnum]
public sealed partial class ScriptEcho {
    public static readonly ScriptEcho Silent = new(echo: false);
    public static readonly ScriptEcho Visible = new(echo: true);

    public bool Echo { get; }
}

// The replay seam is a VALUE at this stratum, not a bare delegate slot: the host's `ReplayHistoryData` is the only
// shape the override can take, so the admission that a hook exists and is non-null belongs to the type. The one
// producer is `Objects/history.md`'s `ReplayProgram`, which composes this owner downward — `Commands` is S1 and
// `Objects` is S2, so the seam type seats HERE and the compiler cannot admit the upward reference.
[ComplexValueObject]
public sealed partial class ReplayHook {
    public Func<ReplayHistoryData, bool> Regrow { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Func<ReplayHistoryData, bool> regrow) =>
        validationError = regrow is null
            ? new ValidationError(message: "replay hook carries no regrowth body")
            : validationError;
}

// Command identity as a VALUE at this stratum, seated beside `ReplayHook` for the same reason: `HistoryRecord`
// keys on `Command.Id` and reads nothing else, so history authoring needs the OWNER, not the host subclass, and
// `Objects` is S2 while `Commands` is S1 — the seam type seats HERE and `Objects/history.md`'s `HistoryScript`
// composes it upward, so no Objects signature names `Rhino.Commands.Command`. A command is a process-lifetime
// plug-in singleton with no document affinity, so one seat admitted at `RunCommand` outlives every grant it
// mints into; `RasmCommand` supplies `HistoryOwner.Of(this)` and nothing else can construct one.
public sealed class HistoryOwner {
    private readonly Command owner;

    private HistoryOwner(Command owner, Guid id) => (this.owner, Id) = (owner, id);

    public Guid Id { get; }

    public static Fin<HistoryOwner> Of(Command owner, Op? key = null) {
        Op op = key.OrDefault(name: nameof(HistoryOwner));
        return from admitted in op.Need(owner)
               from _ in guard(admitted.Id != Guid.Empty, op.InvalidInput()).ToFin()
               select new HistoryOwner(owner: admitted, id: admitted.Id);
    }

    internal Fin<Rhino.DocObjects.HistoryRecord> Mint(int version, Op key) =>
        key.Catch(() => Optional(new Rhino.DocObjects.HistoryRecord(command: owner, version: version))
            .ToFin(Fail: key.InvalidResult()));
}

// The console line is a PRESENTATION leg the caller chooses, never a fixed effect the ledger performs: a headless
// run, a test host, and a scripted lane each want a different one, and a hardcoded `RhinoApp.WriteLine` inside the
// sink writes to a console that may not exist and cannot be silenced without deleting the ledger entry too.
[SmartEnum<int>]
public sealed partial class FaultNotice {
    public static readonly FaultNotice Announce = new(key: 0, report: static message => RhinoApp.WriteLine(message: message));
    public static readonly FaultNotice Silent = new(key: 1, report: static _ => { });

    [UseDelegateFromConstructor]
    internal partial void Report(string message);
}

[ComplexValueObject]
public sealed partial class CommandPolicy {
    public Seq<SessionNeed> Needs { get; }
    public Option<ReplayHook> Replay { get; }
    public FaultNotice Notice { get; }
    public int StageBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<SessionNeed> needs,
        ref Option<ReplayHook> replay,
        ref FaultNotice notice,
        ref int stageBudget) {
        validationError = needs.IsEmpty
            || notice is null
            || stageBudget <= 0
            || stageBudget > 65536
            ? new ValidationError(message: "command policy is incomplete")
            : validationError;
    }
}
```

## [03]-[PROGRAM]

`Stage<TState>` is the closed transition family. Its manual generic hierarchy keeps `TState` free of the source generator's `allows ref struct` propagation. Closure is private constructors, which the compiler cannot prove across a record hierarchy, so every switch over `Stage<TState>` or `FlowStep<TState>` carries a terminal refusal arm — the discard is the exhaustiveness proof CS8509 demands, never a swallowed case, because an unreachable arm on a privately-closed family can only be reached by a case the family does not admit.

`CommandFlow<TState>.Of` re-admits every struct-backed key at the outer storage seam and accumulates independent row defects before admitting topology: keys are distinct, the entry exists, every successor resolves, and the table carries a terminal. `Drive` folds a fixed budget monadically, so continuation carries state without recursion or a mutable loop. `Commit.Fold` is railed and rides `Tables.Commit` as its receipt projection inside the Document commit envelope, so a fold refusal fails the commit with the operation faults instead of surviving a sealed record.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct StageKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError(message: "stage key is blank")
            : validationError;
    }
}

public abstract record Stage<TState> {
    private Stage() { }

    public sealed record Effect(Func<CommandTurn<TState>, Fin<TState>> Run, StageKey Next) : Stage<TState>;
    public sealed record Prompt(Func<TState, Fin<Acquire>> Request, Func<TState, AcquiredReceipt, Fin<TState>> Fold, StageKey Next) : Stage<TState>;
    public sealed record Branch(Func<TState, StageKey> Route, Seq<StageKey> Targets) : Stage<TState>;
    public sealed record Commit(Func<CommandTurn<TState>, Fin<TableTransaction>> Plan, Func<TState, TableReceipt, Fin<TState>> Fold, StageKey Next) : Stage<TState>;
    public sealed record Halt(CommandVerdict Verdict) : Stage<TState>;

    internal Fin<Unit> Admit(Op key) => this switch {
        Effect row => guard(row.Run is not null, key.InvalidInput()).ToFin(),
        Prompt row => guard(row.Request is not null && row.Fold is not null, key.InvalidInput()).ToFin(),
        Branch row => guard(
            row.Route is not null && !row.Targets.IsEmpty && row.Targets.Distinct().Count == row.Targets.Count,
            key.InvalidInput()).ToFin(),
        Commit row => guard(row.Plan is not null && row.Fold is not null, key.InvalidInput()).ToFin(),
        Halt row => guard(row.Verdict is not null, key.InvalidInput()).ToFin(),
        _ => Fin.Fail<Unit>(error: key.InvalidInput()),
    };

    internal Fin<FlowStep<TState>> Apply(CommandTurn<TState> turn, Op key) => this switch {
        Effect effect => effect.Run(arg: turn)
            .Map(state => (FlowStep<TState>)new FlowStep<TState>.Advance(Key: effect.Next, State: state)),
        Prompt prompt => prompt.Request(arg: turn.State)
            .Bind(request => Acquisition.Get(session: turn.Session, request: request))
            .Bind(receipt => receipt.Terminal.Switch(
                state: (Turn: turn, Stage: prompt, Receipt: receipt),
                value: static (held, _) => held.Stage.Fold(arg1: held.Turn.State, arg2: held.Receipt)
                    .Map(state => (FlowStep<TState>)new FlowStep<TState>.Advance(Key: held.Stage.Next, State: state)),
                cancelled: static (held, _) => Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Done(CommandVerdict.Cancelled, held.Turn.State)),
                nothing: static (held, _) => Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Done(CommandVerdict.Empty, held.Turn.State)),
                undone: static (held, _) => Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Back(State: held.Turn.State)),
                timedOut: static (held, _) => Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Done(CommandVerdict.Cancelled, held.Turn.State)),
                exit: static (held, _) => Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Done(CommandVerdict.Exit, held.Turn.State)))),
        Branch branch => key.Catch(() => branch.Route(arg: turn.State) is var routed && branch.Targets.Contains(routed)
            ? Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Advance(Key: routed, State: turn.State))
            : Fin.Fail<FlowStep<TState>>(error: key.InvalidInput())),
        Commit commit =>
            from plan in commit.Plan(arg: turn)
            from state in Tables.Commit(
                session: turn.Session,
                transaction: plan,
                project: receipt => commit.Fold(arg1: turn.State, arg2: receipt))
            select (FlowStep<TState>)new FlowStep<TState>.Advance(Key: commit.Next, State: state),
        Halt halt => Fin.Succ<FlowStep<TState>>(value: new FlowStep<TState>.Done(Verdict: halt.Verdict, State: turn.State)),
        _ => Fin.Fail<FlowStep<TState>>(error: key.InvalidInput()),
    };
}

internal abstract record FlowStep<TState> {
    private FlowStep() { }
    public sealed record Advance(StageKey Key, TState State) : FlowStep<TState>;
    public sealed record Back(TState State) : FlowStep<TState>;
    public sealed record Done(CommandVerdict Verdict, TState State) : FlowStep<TState>;
}

// --- [MODELS] ----------------------------------------------------------------------------
public readonly record struct CommandTurn<TState>(DocumentSession Session, TState State);

internal sealed record FlowCursor<TState>(StageKey Key, TState State, Seq<(StageKey Key, TState State)> Trail, Option<CommandVerdict> Verdict);

public sealed record CommandFlow<TState> {
    private CommandFlow(HashMap<StageKey, Stage<TState>> rows, StageKey entry) {
        Rows = rows;
        Entry = entry;
    }

    public HashMap<StageKey, Stage<TState>> Rows { get; }
    public StageKey Entry { get; }

    public static Fin<CommandFlow<TState>> Of(StageKey entry, params ReadOnlySpan<(StageKey Key, Stage<TState> Stage)> rows) {
        Op op = Op.Of(name: nameof(CommandFlow<>));
        Seq<(StageKey Key, Stage<TState> Stage)> candidates = toSeq(rows.ToArray());
        return from _ in guard(!candidates.IsEmpty, op.InvalidInput())
               from admittedEntry in AdmitKey(entry, op)
               from admitted in candidates
                   .Traverse(row => AdmitRow(row, op).ToValidation())
                   .As()
                   .ToFin()
               let table = admitted.Strict()
               let successors = table.Bind(static row => Successors(row.Stage))
               from ____ in guard(table.Map(static row => row.Key).Distinct().Count == table.Count, op.InvalidInput())
               from _____ in guard(table.Exists(row => row.Key == admittedEntry), op.InvalidInput())
               from ______ in guard(successors.ForAll(next => table.Exists(row => row.Key == next)), op.InvalidInput())
               from _______ in guard(table.Exists(static row => row.Stage is Stage<TState>.Halt), op.InvalidInput())
               select new CommandFlow<TState>(rows: toHashMap(table), entry: admittedEntry);
    }

    // The budget bounds the walk; the PREDICATE ends it. A monadic fold over the whole range answers on the first
    // terminal and then re-enters its own no-op arm for every remaining budget unit — up to 65535 closure calls
    // after the command is already decided — so the fold carries the rail as its state and stops on a settled
    // verdict or a failed rail, and exhausting the range without a verdict is the budget refusal.
    public Fin<(CommandVerdict Verdict, TState State)> Drive(DocumentSession session, TState seed, CommandPolicy policy) {
        Op op = Op.Of();
        return op.Catch(() =>
            from active in op.Need(policy)
            from cursor in toSeq(Enumerable.Range(start: 0, count: active.StageBudget))
                .FoldWhile(
                    Fin.Succ(value: new FlowCursor<TState>(Key: Entry, State: seed, Trail: [], Verdict: None)),
                    (held, _) => held.Bind(cursor => Step(session: session, held: cursor, op: op)),
                    static pair => pair.State.Match(
                        Succ: static cursor => cursor.Verdict.IsNone,
                        Fail: static _ => false))
            from verdict in cursor.Verdict.ToFin(Fail: op.InvalidResult(detail: nameof(CommandPolicy.StageBudget)))
            select (verdict, cursor.State));
    }

    private Fin<FlowCursor<TState>> Step(DocumentSession session, FlowCursor<TState> held, Op op) =>
        op.Catch(() => Rows.Find(held.Key).ToFin(Fail: op.MissingContext()).Bind(stage =>
            stage.Apply(turn: new CommandTurn<TState>(Session: session, State: held.State), key: op).Bind(next => next switch {
                FlowStep<TState>.Advance move => Fin.Succ(held with {
                    Key = move.Key,
                    State = move.State,
                    Trail = Seq((held.Key, held.State)) + held.Trail,
                }),
                FlowStep<TState>.Back _ => held.Trail.Head.Match(
                    Some: frame => Fin.Succ(held with { Key = frame.Key, State = frame.State, Trail = held.Trail.Tail }),
                    None: () => Fin.Succ(held with { Verdict = Some(CommandVerdict.Cancelled) })),
                FlowStep<TState>.Done terminal => Fin.Succ(held with { State = terminal.State, Verdict = Some(terminal.Verdict) }),
                _ => Fin.Fail<FlowCursor<TState>>(error: op.InvalidResult()),
            })));

    private static Seq<StageKey> Successors(Stage<TState> stage) => stage switch {
        Stage<TState>.Effect effect => Seq(effect.Next),
        Stage<TState>.Prompt prompt => Seq(prompt.Next),
        Stage<TState>.Branch branch => branch.Targets,
        Stage<TState>.Commit commit => Seq(commit.Next),
        _ => Seq<StageKey>(),
    };

    private static Fin<StageKey> AdmitKey(StageKey candidate, Op op) => op.Catch(() =>
        StageKey.Validate(value: candidate.ToValue(), provider: null, out StageKey? admitted) is null && admitted is { } value
            ? Fin.Succ(value: value)
            : Fin.Fail<StageKey>(error: op.InvalidInput()));

    private static Fin<(StageKey Key, Stage<TState> Stage)> AdmitRow(
        (StageKey Key, Stage<TState> Stage) row,
        Op op) =>
        from stage in op.Need(row.Stage)
        from key in AdmitKey(row.Key, op)
        from _ in stage.Admit(op)
        from __ in Successors(stage).TraverseM(next => AdmitKey(next, op)).As()
        select (Key: key, Stage: stage);
}
```

## [04]-[HOST_ADAPTER]

`RasmCommand<TSelf,TState>` owns the only `Command` derivation. Session admission, deterministic release, flow execution, and native projection occur in the sealed callback; replay never escapes its host-owned callback window.

Both host overrides collapse a typed rail into a bare native verdict, so both persist the `Error` into `CommandFaults` before the scalar returns — the console line is a presentation leg, never the sink. The site is recoverable from the error itself, because each override mints its `Op` with its own name, so the ledger needs no parallel site column. `Commands` is S1 and the `ObjectsTelemetry` egress is S2, so publishing there is the forbidden upward edge; the process-local cell is the S1 evidence surface, and a consumer above the boundary reads it.

```csharp signature
// --- [BOUNDARIES] -------------------------------------------------------------------------
// A process-lifetime ledger with no cap is a leak with a getter: every refused command in a long session accretes
// an `Error` with its whole exception graph and nothing ever drops one. The bound is the events page's
// `ReceiptJournal` shape verbatim — a capacity, oldest-first eviction, and a retained loss count that surfaces as
// its own entry, so a reader learns that entries were dropped rather than silently reading a truncated ledger.
public sealed record FaultLedger(Seq<Error> Entries, long Lost) {
    internal const int Capacity = 256;

    internal static readonly FaultLedger Empty = new(Entries: Seq<Error>(), Lost: 0L);

    internal FaultLedger Post(Error error) => Entries.Count < Capacity
        ? this with { Entries = Entries.Add(value: error) }
        : new FaultLedger(Entries: Entries.Tail.Add(value: error), Lost: checked(Lost + 1L));
}

public static class CommandFaults {
    private static readonly Atom<FaultLedger> Refusals = Atom(value: FaultLedger.Empty);

    public static FaultLedger Ledger => Refusals.Value;

    public static Seq<Error> Faults => Ledger.Entries;

    internal static TNative Refused<TNative>(Error error, FaultNotice notice, TNative native) {
        _ = Refusals.Swap(f: held => held.Post(error: error));
        notice.Report(message: error.Message);
        return native;
    }
}

public abstract class RasmCommand<TSelf, TState> : Command
    where TSelf : RasmCommand<TSelf, TState> {
    protected abstract CommandPolicy Policy { get; }
    protected abstract TState Seed { get; }
    protected abstract Fin<CommandFlow<TState>> Flow { get; }

    protected sealed override Result RunCommand(RhinoDoc doc, RunMode mode) {
        Op op = Op.Of(name: typeof(TSelf).Name);
        Fin<CommandVerdict> outcome = op.Catch(() =>
            from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
            from policy in op.Need(Policy)
            from flow in Flow
            from lane in SessionMode.OfRunMode(mode: mode, key: op)
            from session in DocumentSession.Of(source: new SessionSource.Live(Document: doc), mode: lane, needs: policy.Needs.ToArray())
            from verdict in op.Catch(() => {
                using DocumentSession active = session;
                return flow.Drive(session: active, seed: Seed, policy: policy).Map(static receipt => receipt.Verdict);
            })
            select verdict);
        FaultNotice notice = Optional(Policy).Map(static policy => policy.Notice).IfNone(FaultNotice.Announce);
        return outcome.Match(
            Succ: static verdict => verdict.Native,
            Fail: error => CommandFaults.Refused(error: error, notice: notice, native: Result.Failure));
    }

    protected sealed override bool ReplayHistory(ReplayHistoryData replayData) {
        Op op = Op.Of(name: nameof(ReplayHistory));
        Option<CommandPolicy> policy = Optional(Policy);
        FaultNotice notice = policy.Map(static row => row.Notice).IfNone(FaultNotice.Announce);
        return op.Catch(() => Fin.Succ(policy.Bind(static row => row.Replay).Match(
                Some: hook => hook.Regrow(arg: replayData),
                None: static () => false)))
            .Match(
                Succ: static accepted => accepted,
                Fail: error => CommandFaults.Refused(error: error, notice: notice, native: false));
    }
}
```

## [05]-[REGISTRY_AND_EVENTS]

`CommandRegistry.Ask` re-admits each query payload before any live existence, identity, roster, stack, state, or prompt read. `CommandPulse` detaches lifecycle, prompt, and escape callbacks into the evidence-bearing `CommandFact` family, while one observation entry composes any distinct pulse set under one subscription.

- Law: escape has ONE arming seat and it is a `CommandPulse` row. Escape answers on EVERY cancellable command, not only on a metered one, so arming it inside a progress lease or a pacing carrier reaches exactly the commands that happen to report progress and silently drops the rest; the pulse row arms once per observer and every command lane reads the same fact.
- Law: the escape handler is a per-lease closure, never a static method group — the host keys `+=`/`-=` on delegate IDENTITY, so one shared instance makes the second lease's attach a no-op and its detach a theft of the first lease's hook. Every other pulse row already closes over its observer; escape is the row where the mistake is invisible, because a bare `EventHandler` signature admits a static group the generic rows do not.
- Law: both fault ledgers are BOUNDED — the process-wide `CommandFaults` and the per-observer `CommandObserver` share `FaultLedger`, whose capacity, oldest-first eviction, and retained loss count mirror the events page's `ReceiptJournal`. An unbounded process-lifetime `Seq<Error>` retains every refused command's whole exception graph for the session and has no reader that can drain it.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandQuery {
    private CommandQuery() { }
    public sealed record Exists(string Name) : CommandQuery;
    public sealed record Valid(string Name) : CommandQuery;
    public sealed record Resolve(string Name, CommandLanguage Language) : CommandQuery;
    public sealed record Name(Guid Id, CommandLanguage Language) : CommandQuery;
    public sealed record Names(CommandLanguage Language, CommandRoster Roster) : CommandQuery;
    public sealed record Recent : CommandQuery;
    public sealed record Stack : CommandQuery;
    public sealed record State : CommandQuery;
    public sealed record Prompt : CommandQuery;

    internal Fin<CommandQuery> Admit(Op op) => Switch(
        state: op,
        exists: static (key, ask) => key.AcceptText(value: ask.Name).Map(_ => (CommandQuery)ask),
        valid: static (key, ask) => key.AcceptText(value: ask.Name).Map(_ => (CommandQuery)ask),
        resolve: static (key, ask) => from _ in key.AcceptText(value: ask.Name)
                                      from __ in key.Need(ask.Language)
                                      select (CommandQuery)ask,
        name: static (key, ask) => from _ in guard(ask.Id != Guid.Empty, key.InvalidInput()).ToFin()
                                   from __ in key.Need(ask.Language)
                                   select (CommandQuery)ask,
        names: static (key, ask) => from _ in key.Need(ask.Language)
                                    from __ in key.Need(ask.Roster)
                                    select (CommandQuery)ask,
        // The four parameterless asks carry nothing to admit and each says so; a catch-all here silently admits
        // the next case that DOES carry a payload.
        recent: static (_, ask) => Fin.Succ<CommandQuery>(value: ask),
        stack: static (_, ask) => Fin.Succ<CommandQuery>(value: ask),
        state: static (_, ask) => Fin.Succ<CommandQuery>(value: ask),
        prompt: static (_, ask) => Fin.Succ<CommandQuery>(value: ask));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandAnswer {
    private CommandAnswer() { }
    public sealed record Flag(bool Value) : CommandAnswer;
    public sealed record Id(Option<Guid> Value) : CommandAnswer;
    public sealed record NameValue(Option<string> Value) : CommandAnswer;
    public sealed record NameSet(Seq<string> Value) : CommandAnswer;
    public sealed record RecentSet(Seq<RecentCommand> Value) : CommandAnswer;
    public sealed record StackSet(Seq<Guid> Value) : CommandAnswer;
    public sealed record Activity(bool InCommand, bool InScript) : CommandAnswer;
    public sealed record PromptValue(string Value) : CommandAnswer;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandFact {
    private CommandFact() { }
    public sealed record Started(CommandEvent Value) : CommandFact;
    public sealed record Ended(CommandEvent Value) : CommandFact;
    public sealed record Undo(UndoEvent Value) : CommandFact;
    public sealed record PromptChanged(PromptEvent Value) : CommandFact;
    public sealed record Escaped(EscapeEvent Value) : CommandFact;
    public sealed record Rejected(Error Value) : CommandFact;
}

public sealed record CommandEvent(Guid Id, string English, string Local, string Help, string Plugin, CommandVerdict Verdict, uint DocumentSerial);
public sealed record UndoEvent(uint DocumentSerial, Guid CommandId, uint UndoSerial, UndoMoment Moment);
public sealed record PromptEvent(string Prompt, string Default, Seq<CommandOptionEvent> Options);

// Escape is a bare host signal — no args, no document, no command identity — so the fact carries the two things
// the signal MEANS on this rail rather than an empty marker: the verdict a cancellable command answers with, and
// the capability the interruption spends. A consumer folds it onto its own terminal without re-deriving either.
public sealed record EscapeEvent(CommandVerdict Verdict, SessionNeed Need) {
    internal static EscapeEvent Signalled { get; } =
        new(Verdict: CommandVerdict.Cancelled, Need: SessionNeed.Interrupt);
}
public sealed record RecentCommand(string Display, string Macro);
public sealed record CommandOptionEvent(
    int Index,
    OptionKind Kind,
    string English,
    string Local,
    string Value,
    int ListIndex,
    Option<bool> Toggle);

[ComplexValueObject]
public sealed partial class CommandObserver {
    // Bounded for the same reason the process ledger is: an observer outlives every command it watches, and a
    // misbehaving sink faults once per pulse. Same capacity, same oldest-first eviction, same retained loss count.
    private readonly Atom<FaultLedger> faults = Atom(value: FaultLedger.Empty);

    public Func<CommandFact, Fin<Unit>> Deliver { get; }
    public Func<Error, Unit> Reject { get; }
    public FaultLedger Ledger => faults.Value;
    public Seq<Error> Faults => Ledger.Entries;

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Func<CommandFact, Fin<Unit>> deliver,
        ref Func<Error, Unit> reject) {
        validationError = deliver is null || reject is null
            ? new ValidationError(message: "command observer is incomplete")
            : validationError;
    }

    internal Unit Guard(Func<CommandFact> project) {
        Op op = Op.Of(name: nameof(CommandPulse));
        return op.Catch(() => Deliver(arg: project())).Match(
            Succ: static _ => unit,
            Fail: primary => {
                Error retained = op.Catch(() => Fin.Succ(value: Reject(primary))).Match(
                    Succ: _ => primary,
                    Fail: secondary => primary + secondary);
                _ = faults.Swap(f: held => held.Post(error: retained));
                return unit;
            });
    }
}

[SmartEnum]
public sealed partial class CommandLanguage {
    public static readonly CommandLanguage English = new(isEnglish: true);
    public static readonly CommandLanguage Local = new(isEnglish: false);

    public bool IsEnglish { get; }
}

[SmartEnum]
public sealed partial class CommandRoster {
    public static readonly CommandRoster Loaded = new(isLoaded: true);
    public static readonly CommandRoster Installed = new(isLoaded: false);

    public bool IsLoaded { get; }
}

[SmartEnum<int>]
public sealed partial class UndoMoment {
    public static readonly UndoMoment BeforeRecording = new(key: 0, match: static e => e.IsBeforeBeginRecording);
    public static readonly UndoMoment Recording = new(key: 1, match: static e => e.IsBeginRecording);
    public static readonly UndoMoment BeforeRecorded = new(key: 2, match: static e => e.IsBeforeEndRecording);
    public static readonly UndoMoment Recorded = new(key: 3, match: static e => e.IsEndRecording);
    public static readonly UndoMoment Undoing = new(key: 4, match: static e => e.IsBeginUndo);
    public static readonly UndoMoment Undone = new(key: 5, match: static e => e.IsEndUndo);
    public static readonly UndoMoment Redoing = new(key: 6, match: static e => e.IsBeginRedo);
    public static readonly UndoMoment Redone = new(key: 7, match: static e => e.IsEndRedo);
    public static readonly UndoMoment Purged = new(key: 8, match: static e => e.IsPurgeRecord);

    [UseDelegateFromConstructor]
    public partial bool Matches(UndoRedoEventArgs value);
}

[SmartEnum<int>]
public sealed partial class CommandPulse {
    public static readonly CommandPulse Begin = new(key: 0, attach: static observer => Subscription.Attach(
        subscribe: static (EventHandler<CommandEventArgs> handler) => Command.BeginCommand += handler,
        unsubscribe: static handler => Command.BeginCommand -= handler,
        handler: (_, args) => observer.Guard(
            project: () => Project(args, static value => new CommandFact.Started(Value: value)))));
    public static readonly CommandPulse End = new(key: 1, attach: static observer => Subscription.Attach(
        subscribe: static (EventHandler<CommandEventArgs> handler) => Command.EndCommand += handler,
        unsubscribe: static handler => Command.EndCommand -= handler,
        handler: (_, args) => observer.Guard(
            project: () => Project(args, static value => new CommandFact.Ended(Value: value)))));
    public static readonly CommandPulse UndoRedo = new(key: 2, attach: static observer => Subscription.Attach(
        subscribe: static (EventHandler<UndoRedoEventArgs> handler) => Command.UndoRedo += handler,
        unsubscribe: static handler => Command.UndoRedo -= handler,
        handler: (_, args) => observer.Guard(project: () => Project(args))));
    public static readonly CommandPulse Prompt = new(key: 3, attach: static observer => Subscription.Attach(
        subscribe: static (EventHandler<CommandPromptChangedEventArgs> handler) => RhinoApp.CommandPromptChanged += handler,
        unsubscribe: static handler => RhinoApp.CommandPromptChanged -= handler,
        handler: (_, args) => observer.Guard(project: () => Project(args))));
    // ONE arming seat for escape, and the handler is a per-lease CLOSURE over its observer. `EscapeKeyPressed` is a
    // bare `EventHandler`, so a static method group would produce the SAME delegate instance for every lease: the
    // host's `-=` removes by delegate identity, so the second lease's detach would silently unhook the first, and
    // its `+=` would be a no-op the first lease already satisfied. Closing over `observer` makes each lease's
    // delegate distinct, which is what makes attach and detach pair one-to-one.
    public static readonly CommandPulse Escape = new(key: 4, attach: static observer => Subscription.Attach(
        subscribe: static (EventHandler handler) => RhinoApp.EscapeKeyPressed += handler,
        unsubscribe: static handler => RhinoApp.EscapeKeyPressed -= handler,
        handler: (_, _) => observer.Guard(project: static () => new CommandFact.Escaped(Value: EscapeEvent.Signalled))));

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Attach(CommandObserver observer);

    public Fin<Subscription> Observe(CommandObserver observer) =>
        Optional(observer).ToFin(Fail: Op.Of(name: nameof(CommandPulse)).InvalidInput()).Bind(Attach);

    public static Fin<Subscription> Observe(CommandObserver observer, params ReadOnlySpan<CommandPulse> pulses) {
        Op op = Op.Of(name: nameof(CommandPulse));
        Seq<CommandPulse> candidates = toSeq(pulses.ToArray());
        return from active in op.Need(observer)
               from _ in guard(!candidates.IsEmpty && candidates.ForAll(static pulse => pulse is not null), op.InvalidInput())
               from attached in Subscription.AttachAll(candidates.Distinct().Map(pulse =>
                   (Func<Fin<Subscription>>)(() => pulse.Attach(active))))
               select attached;
    }

    private static CommandFact Project(CommandEventArgs args, Func<CommandEvent, CommandFact> accept) =>
        CommandVerdict.OfNative(args.CommandResult, Op.Of(name: nameof(CommandPulse))).Match(
            Succ: verdict => accept(new CommandEvent(
                Id: args.CommandId,
                English: args.CommandEnglishName,
                Local: args.CommandLocalName,
                Help: args.CommandHelpURL,
                Plugin: args.CommandPluginName,
                Verdict: verdict,
                DocumentSerial: args.DocumentRuntimeSerialNumber)),
            Fail: static error => new CommandFact.Rejected(Value: error));

    private static CommandFact Project(UndoRedoEventArgs args) =>
        UndoMoment.Items.AsIterable().Find(moment => moment.Matches(args))
            .ToFin(Fail: Op.Of(name: nameof(CommandPulse)).InvalidResult())
            .Match<CommandFact>(
                Succ: moment => new CommandFact.Undo(Value: new UndoEvent(
                    DocumentSerial: args.DocumentSerialNumber,
                    CommandId: args.CommandId,
                    UndoSerial: args.UndoSerialNumber,
                    Moment: moment)),
                Fail: static error => new CommandFact.Rejected(Value: error));

    private static CommandFact Project(CommandPromptChangedEventArgs args) =>
        new CommandFact.PromptChanged(Value: new PromptEvent(
            Prompt: args.Prompt,
            Default: args.PromptDefault,
            Options: toSeq(args.Options ?? []).Choose(option => OptionKind
                .Of(native: option.OptionType, key: Op.Of(name: nameof(CommandOptionEvent)))
                .ToOption()
                .Map(kind => new CommandOptionEvent(
                    Index: option.Index,
                    Kind: kind,
                    English: option.EnglishName,
                    Local: option.LocalName,
                    Value: option.StringOptionValue,
                    ListIndex: option.CurrentListOptionIndex,
                    Toggle: Optional(option.CurrentToggleValue))))));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class CommandRegistry {
    public static Fin<CommandAnswer> Ask(CommandQuery query) {
        Op op = Op.Of();
        return op.Need(query).Bind(request => request.Admit(op)).Bind(request => op.Catch(() => Fin.Succ(request.Switch(
            exists: static ask => (CommandAnswer)new CommandAnswer.Flag(Value: Command.IsCommand(name: ask.Name)),
            valid: static ask => new CommandAnswer.Flag(Value: Command.IsValidCommandName(name: ask.Name)),
            resolve: static ask => new CommandAnswer.Id(Value: Optional(Command.LookupCommandId(
                name: ask.Name, searchForEnglishName: ask.Language.IsEnglish)).Filter(static id => id != Guid.Empty)),
            name: static ask => new CommandAnswer.NameValue(Value: Optional(Command.LookupCommandName(
                commandId: ask.Id, englishName: ask.Language.IsEnglish)).Filter(static value => value.Length > 0)),
            names: static ask => new CommandAnswer.NameSet(Value: toSeq(Command.GetCommandNames(
                english: ask.Language.IsEnglish, loaded: ask.Roster.IsLoaded))),
            recent: static _ => new CommandAnswer.RecentSet(Value: toSeq(Command.GetMostRecentCommands())
                .Map(static row => new RecentCommand(Display: row.DisplayString, Macro: row.Macro))),
            stack: static _ => new CommandAnswer.StackSet(Value: toSeq(Command.GetCommandStack())),
            state: static _ => new CommandAnswer.Activity(InCommand: Command.InCommand(), InScript: Command.InScriptRunnerCommand()),
            prompt: static _ => new CommandAnswer.PromptValue(Value: RhinoApp.CommandPrompt)))));
    }
}
```

## [06]-[SCRIPT]

`Scripting.Run` targets the admitted session document and preserves the native terminal. `Scripting.Proxy` dispatches one proxy inside the same document and thread grant: the caller supplies a typed body over the admitted `DocumentSession`, the re-closed `SessionMode`, and its own payload type, and the host `RunCommandDelegate`, `RhinoDoc`, `RunMode`, and `object data` stay inside the adapter. Script text, echo, and MRU display are case evidence; named dispatch validates registry membership before execution.

```csharp signature
// --- [TYPES] -----------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptOp {
    private ScriptOp() { }
    public sealed record Macro(string Text, ScriptEcho Echo, Option<string> Display = default) : ScriptOp;
    public sealed record Named(string CommandName) : ScriptOp;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Scripting {
    public static Fin<CommandVerdict> Run(DocumentSession session, ScriptOp script) {
        Op op = Op.Of();
        return from request in op.Need(script)
               from target in op.Need(session)
               from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from verdict in target.Demand(
                   use: document => request.Switch(
                       state: (Serial: (uint)target.Key, Document: document, Op: op),
                       macro: static (held, run) =>
                           from text in held.Op.AcceptText(value: run.Text)
                           from _ in guard(run.Echo is not null, held.Op.InvalidInput())
                           from ok in held.Op.Catch(() => Fin.Succ(value: run.Display.Case switch {
                               string display => RhinoApp.RunScript(documentSerialNumber: held.Serial, script: text, mruDisplayString: display, echo: run.Echo.Echo),
                               _ => RhinoApp.RunScript(documentSerialNumber: held.Serial, script: text, echo: run.Echo.Echo),
                           }))
                           select ok ? CommandVerdict.Completed : CommandVerdict.Failed,
                       named: static (held, run) =>
                           from name in held.Op.AcceptText(value: run.CommandName)
                           from _ in guard(Command.IsCommand(name: name), held.Op.InvalidInput())
                           from native in held.Op.Catch(() => Fin.Succ(value: RhinoApp.ExecuteCommand(document: held.Document, commandName: name)))
                           from result in CommandVerdict.OfNative(native: native, key: held.Op)
                           select result),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select verdict;
    }

    public static Fin<Unit> Proxy<TPayload>(
        DocumentSession session,
        Func<DocumentSession, SessionMode, TPayload, Fin<CommandVerdict>> body,
        TPayload payload)
        where TPayload : notnull {
        Op op = Op.Of();
        return from target in op.Need(session)
               from run in op.Need(body)
               from data in op.Need(payload)
               from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from dispatched in target.Demand(
                   use: document => op.Catch(() => {
                       Command.RunProxyCommand(
                           commandCallback: (_, mode, _) => SessionMode.OfRunMode(mode: mode, key: op)
                               .Bind(lane => run(arg1: target, arg2: lane, arg3: data))
                               .Match(
                                   Succ: static verdict => verdict.Native,
                                   // The proxy callback is host-invoked and carries no policy of its own, so it
                                   // announces: a refused proxy that reported nowhere is a silent host no-op.
                                   Fail: error => CommandFaults.Refused(
                                       error: error,
                                       notice: FaultNotice.Announce,
                                       native: Result.Failure)),
                           doc: document,
                           data: data);
                       return Fin.Succ(unit);
                   }),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select dispatched;
    }
}
```

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
