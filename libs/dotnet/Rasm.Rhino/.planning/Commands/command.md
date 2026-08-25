# [RASM_RHINO_COMMAND]

`CommandFlow<TState>.Drive` interprets one admitted, bounded command program over immutable state. Host lifecycle enters once through `RasmCommand<TSelf,TState>`, acquisition and mutation remain calls into their owning rails, and every command lifecycle callback fans out through the kernel hook rail on the folder's own `RhinoPoint` roster — the observer plumbing, the per-observer reject sink, and the hand-bounded fault ledgers this page once carried are the kernel's now.

## [01]-[INDEX]

- [02]-[TERMINAL_POLICY]: `CommandVerdict`, `ScriptEcho`, `ReplayHook`, `HistoryOwner`, `FaultNotice`, and `CommandPolicy`.
- [03]-[PROGRAM]: `StageKey`, `Stage<TState>`, `FlowStep<TState>`, `CommandTurn<TState>`, and `CommandFlow<TState>` under a QuikGraph-proved topology.
- [04]-[HOST_ADAPTER]: `CommandFaults` on the kernel ring and the one `RasmCommand<TSelf,TState>` derivation.
- [05]-[REGISTRY_AND_EVENTS]: the self-typed `CommandQuery<TAnswer>`, `CommandActivity`, the `CommandFact` family, `[Mapper] CommandMap`, `CommandPulse` firing the kernel `HookRail`, and `CommandRegistry`.
- [06]-[SCRIPT]: `ScriptOp` and the `Scripting` run/proxy pair.
- [07]-[RESEARCH]: open verification rows.

## [02]-[TERMINAL_POLICY]

`CommandVerdict` preserves every native terminal in both directions. `CommandPolicy` admits the session demand, replay behavior, and stage budget as one value before a flow starts, each defect reported beside the others. `ReplayHook` and `HistoryOwner` are the two seam values `Objects/history.md` composes upward — the regrowth body and the command identity a history record keys on.

- Law: `OfNative`'s parameter is `result` — `Objects/materials.md` calls `OfNative(result:, key:)` by name, and the prior `native` spelling was a live CS1739 at that composing site; the coupling is recorded at both ends.
- Law: the two-row presentation and language vocabularies key on the HOST bool itself — `[SmartEnum<bool>]` deletes the mirror column, so the row name is the read and no `.Echo`/`.IsEnglish` getter restates the key.
- Law: `CommandPolicy` accumulates — an empty need set, an absent notice, and an out-of-band budget are three independent defects one `Validation` reports together, each as its own `DraftFault` clause, where the prior single-message hook collapsed all three onto "policy is incomplete".

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Linq;
using Rasm.Domain;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;

namespace Rasm.Rhino.Commands;

// --- [TYPES] ---------------------------------------------------------------------------
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

    public static Fin<CommandVerdict> OfNative(Result result, Op? key = null) =>
        Items.AsIterable().Find(verdict => verdict.Native == result)
            .ToFin(Fail: key.OrDefault().InvalidResult(detail: result.ToString()));
}

[SmartEnum<bool>]
public sealed partial class ScriptEcho {
    public static readonly ScriptEcho Silent = new(key: false);
    public static readonly ScriptEcho Visible = new(key: true);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class ReplayHook {
    public Func<ReplayHistoryData, bool> Regrow { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Func<ReplayHistoryData, bool> regrow) =>
        validationError = regrow is null
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(Regrow) }))
            : validationError;
}

public sealed class HistoryOwner {
    private readonly Command owner;

    private HistoryOwner(Command owner, Guid id) => (this.owner, Id) = (owner, id);

    public Guid Id { get; }

    public static Fin<HistoryOwner> Of(Command owner, Op? key = null) {
        Op op = key.OrDefault(name: nameof(HistoryOwner));
        return from admitted in op.Need(owner)
               from _ in guard(admitted.Id != Guid.Empty, op.InvalidInput(axis: nameof(Id))).ToFin()
               select new HistoryOwner(owner: admitted, id: admitted.Id);
    }

    internal Fin<Rhino.DocObjects.HistoryRecord> Mint(int version, Op key) =>
        key.Catch(() => Optional(new Rhino.DocObjects.HistoryRecord(command: owner, version: version))
            .ToFin(Fail: key.InvalidResult()));
}

[SmartEnum<int>]
public sealed partial class FaultNotice {
    public static readonly FaultNotice Announce = new(key: 0, report: static message => RhinoApp.WriteLine(message: message));
    public static readonly FaultNotice Silent = new(key: 1, report: static _ => { });

    [UseDelegateFromConstructor]
    internal partial void Report(string message);
}

[ComplexValueObject]
[ValidationError]
public sealed partial class CommandPolicy {
    public Seq<SessionNeed> Needs { get; }
    public Option<ReplayHook> Replay { get; }
    public FaultNotice Notice { get; }
    public Rasm.Numerics.Dimension StageBudget { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Seq<SessionNeed> needs,
        ref Option<ReplayHook> replay,
        ref FaultNotice notice,
        ref Rasm.Numerics.Dimension stageBudget) =>
        validationError = FactoryValidation.Of(FactoryValidation.Violated(
            (needs.IsEmpty, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Needs) }))),
            (notice is null, () => new ValidationClause(string.Join(" | ", new object?[] { Op.Of(), nameof(Notice) }))),
            (stageBudget.Value > 65536,
                () => new ValidationClause(string.Join(" | ", new object?[] {
                    Op.Of(), nameof(StageBudget), stageBudget.Value, "a stage budget at or under 65536" }))));
}
```

## [03]-[PROGRAM]

`Stage<TState>` is the closed transition family. Its manual generic hierarchy keeps `TState` free of the source generator's `allows ref struct` propagation. Closure is private constructors, which the compiler cannot prove across a record hierarchy, so every switch over `Stage<TState>` or `FlowStep<TState>` carries a terminal refusal arm — the discard is the exhaustiveness proof CS8509 demands, never a swallowed case.

- Law: the successor roster is a BASE POSITIONAL column — every case hands its outgoing keys to the root at construction, so no static ladder re-derives per case what the case already knows, and a new case cannot land without stating where it goes.
- Law: `CommandFlow<TState>.Of` proves topology on the graph, not on a guard ladder. The row set builds ONE QuikGraph `AdjacencyGraph` (vertices = admitted keys, edges = declared successors); distinctness, entry membership, successor resolution, terminal presence, and REACHABILITY from the entry are five independent clauses accumulated through `Validation` — the prior four guards reported first-defect-only and never asked whether a stage was reachable at all, so an orphaned stage rode every program silently.
- Law: `Drive` folds the kernel `foldUntil` over the budget range — the fold carries the rail as its state, stops on a settled verdict or a failed rail, and exhausting the range without a verdict is a typed budget refusal naming `StageBudget`; a success-shaped fall-through past the bound would certify an unconverged program as converged.
- Law: `Commit.Fold` is railed and rides `Tables.Commit` as its receipt projection inside `DocumentCommit.Sealed`, so a fold refusal fails the commit with the operation faults instead of surviving a sealed record.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>]
[ValidationError]
public readonly partial struct StageKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value?.Trim() ?? string.Empty;
        validationError = value.Length is 0
            ? new ValidationError(string.Join(" | ", new object?[] { Op.Of(), nameof(StageKey) }))
            : validationError;
    }
}

public abstract record Stage<TState>(Seq<StageKey> Successors) {
    public sealed record Effect(Func<CommandTurn<TState>, Fin<TState>> Run, StageKey Next) : Stage<TState>(Seq(Next));
    public sealed record Prompt(Func<TState, Fin<Acquire>> Request, Func<TState, AcquiredReceipt, Fin<TState>> Fold, StageKey Next) : Stage<TState>(Seq(Next));
    public sealed record Branch(Func<TState, StageKey> Route, Seq<StageKey> Targets) : Stage<TState>(Targets);
    public sealed record Commit(Func<CommandTurn<TState>, Fin<TableTransaction>> Plan, Func<TState, TableReceipt, Fin<TState>> Fold, StageKey Next) : Stage<TState>(Seq(Next));
    public sealed record Halt(CommandVerdict Verdict) : Stage<TState>(Seq<StageKey>());

    internal Fin<Unit> Admit(Op key) => this switch {
        Effect row => guard(row.Run is not null, key.InvalidInput(axis: nameof(Effect.Run))).ToFin(),
        Prompt row => guard(row.Request is not null && row.Fold is not null, key.InvalidInput(axis: nameof(Prompt))).ToFin(),
        Branch row => guard(
            row.Route is not null && !row.Targets.IsEmpty && row.Targets.Distinct().Count == row.Targets.Count,
            key.InvalidInput(axis: nameof(Branch))).ToFin(),
        Commit row => guard(row.Plan is not null && row.Fold is not null, key.InvalidInput(axis: nameof(Commit))).ToFin(),
        Halt row => guard(row.Verdict is not null, key.InvalidInput(axis: nameof(Halt))).ToFin(),
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
            : Fin.Fail<FlowStep<TState>>(error: key.InvalidInput(axis: nameof(Branch.Targets)))),
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

// --- [MODELS] --------------------------------------------------------------------------
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
        return from _ in guard(!candidates.IsEmpty, op.InvalidInput()).ToFin()
               from admittedEntry in AdmitKey(entry, op)
               from admitted in candidates
                   .Traverse(row => AdmitRow(row, op).ToValidation())
                   .As()
                   .ToFin()
               let table = admitted.Strict()
               from _____ in Topology(table: table, entry: admittedEntry, op: op)
               select new CommandFlow<TState>(rows: toHashMap(table), entry: admittedEntry);
    }

    private static Fin<Unit> Topology(Seq<(StageKey Key, Stage<TState> Stage)> table, StageKey entry, Op op) {
        QuikGraph.AdjacencyGraph<StageKey, QuikGraph.SEdge<StageKey>> graph =
            QuikGraph.GraphExtensions.ToAdjacencyGraph<StageKey, QuikGraph.SEdge<StageKey>>(
                table.Bind(static row => row.Stage.Successors.Map(next => new QuikGraph.SEdge<StageKey>(row.Key, next)))
                    .AsIterable().AsEnumerable(),
                allowParallelEdges: false);
        table.Iter(row => graph.AddVertex(v: row.Key));
        HashSet<StageKey> keys = toHashSet(table.Map(static row => row.Key));
        System.Collections.Generic.HashSet<StageKey> reached = [];
        var search = new QuikGraph.Algorithms.Search.BreadthFirstSearchAlgorithm<StageKey, QuikGraph.SEdge<StageKey>>(graph);
        search.DiscoverVertex += vertex => reached.Add(item: vertex);
        search.Compute(root: entry);
        return (
                guard(table.Map(static row => row.Key).Distinct().Count == table.Count,
                    (Error)new KernelFault.InvalidValue(nameof(Rows), string.Join(" | ", new object?[] { op, "distinct stage keys" }))).ToValidation(),
                guard(keys.Contains(entry),
                    (Error)new KernelFault.InvalidValue(nameof(Entry), string.Join(" | ", new object?[] { op, "an entry stage" }))).ToValidation(),
                guard(table.Bind(static row => row.Stage.Successors).ForAll(keys.Contains),
                    (Error)new KernelFault.InvalidValue(nameof(Stage<TState>.Successors), string.Join(" | ", new object?[] { op, "every successor resolves to a row" }))).ToValidation(),
                guard(table.Exists(static row => row.Stage is Stage<TState>.Halt),
                    (Error)new KernelFault.InvalidValue(nameof(Stage<TState>.Halt), string.Join(" | ", new object?[] { op, "at least one halt stage" }))).ToValidation(),
                guard(table.ForAll(row => reached.Contains(row.Key)),
                    (Error)new KernelFault.InvalidValue(nameof(Rows), string.Join(" | ", new object?[] { op, "every stage reachable from the entry" }))).ToValidation())
            .Apply(static (_, _, _, _, _) => unit)
            .As()
            .ToFin();
    }

    public Fin<(CommandVerdict Verdict, TState State)> Drive(DocumentSession session, TState seed, CommandPolicy policy) {
        Op op = Op.Of();
        return op.Catch(() =>
            from active in op.Need(policy)
            from cursor in Range(0, active.StageBudget.Value).AsIterable().ToSeq()
                .foldUntil(
                    Fin.Succ(value: new FlowCursor<TState>(Key: Entry, State: seed, Trail: [], Verdict: None)),
                    (held, _) => held.Bind(cursor => Step(session: session, held: cursor, op: op)),
                    valueIs: static held => held.Match(
                        Succ: static cursor => cursor.Verdict.IsSome,
                        Fail: static _ => true))
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
        from __ in stage.Successors.TraverseM(next => AdmitKey(next, op)).As()
        select (Key: key, Stage: stage);
}
```

## [04]-[HOST_ADAPTER]

`RasmCommand<TSelf,TState>` owns the only `Command` derivation. Session admission, deterministic release, flow execution, and native projection occur in the sealed callback; replay never escapes its host-owned callback window.

Both host overrides collapse a typed rail into a bare native verdict, so both persist the `Error` into `CommandFaults` before the scalar returns — the console line is a presentation leg, never the sink. The site is recoverable from the error itself, because each override mints its `Op` with its own name, so the ledger needs no parallel site column. `Commands` is S1 and the `ObjectsTelemetry` egress is S2, so publishing there is the forbidden upward edge; the process-local cell is the S1 evidence surface, and a consumer above the boundary reads it.

- Law: `CommandFaults` is a LEDGER declaration, not a fault family and not a factory — it holds one `Ring<Error>` under a declared retention row and one `Refused` sink, mints no case, takes no message string, and classifies nothing. It belongs to this package's `<Surface>Faults` ledger family beside `PluginFaults`, `ShellFaults`, and `DisplayFaults`, so renaming it alone forks four consistent declarations into three plus one. The near-collision with `Rasm.AppHost`'s `CommandFault` union is not one: that family sits at a stratum this package cannot reference, and the branch row rules a referencing package's own EVIDENCE and REFUSAL types, which is what this static holder is not. A suffix sweep converting it destroys a real retention surface and reaches no untyped producer.
- Law: the process ledger IS the kernel ring. A cap, oldest-first eviction, and a shed counter were this page's hand `FaultLedger` — the kernel `Ring<Error>` is that shape once for the estate, its `Park` verdict is COUNTED (`Lost`) rather than discarded, and the capacity is a named policy row instead of a buried literal. The per-observer twin deletes with the observer plumbing (`[05]`).

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class CommandFaults {
    internal static readonly Rasm.Numerics.Dimension Retention = Rasm.Numerics.Dimension.Create(value: 256);

    private static readonly Ring<Error> Refusals = new(cap: Retention);

    public static Seq<Error> Faults => Refusals.Parked;
    public static long Shed => Refusals.Shed;

    internal static TNative Refused<TNative>(Error error, FaultNotice notice, TNative native) {
        _ = Refusals.Park(item: error);
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

`CommandRegistry.Ask` re-admits each query payload before any live read, and the query is SELF-TYPED — the request case fixes its answer shape, so the nine-case answer union and every consumer cast delete. `CommandPulse` detaches lifecycle, prompt, and escape callbacks into the evidence-bearing `CommandFact` family and FIRES the kernel hook rail at the folder's own `RhinoPoint` command rows, the seat gated by the fact's own `Seats` fan; a consumer subscribes a `HookTap` on the rail and the observer type, its reject sink, and its hand-bounded ledger delete.

- Law: `CommandFact` realizes the kernel `IHookFact<RhinoPoint>` floor, so the FACT declares where it seats and the rail refuses a foreign pair at fire entry and again on the veto fold's product. Five cases are 1:1 with the `CommandPulse` row that mints them, and `Rejected` — the `CommandMap` failure fact — fans to exactly the mapped pulses' points, derived off the pulse roster's own `Point` column so a new pulse row moves the fan with it. This is what keeps a command fact off the document, display, and host rows the shared roster also carries.
- Law: the rail is the ONE fan-out. `HookRail<RhinoPoint, CommandFact, PluginKey>` carries gates, taps, isolation, and the bounded `FaultCell` — a subscriber raise parks on the rail's cell and the pulse settles, so instrumentation is never a liveness dependency and the per-observer `Reject` callback has no seam left to exist on. NAMED LOSS: the observer-local reject arm; bought back as `rail.Faults.Parked` beside `Shed`, a number where a `void` callback was a discard.
- Law: escape has ONE arming seat and it is a `CommandPulse` row. Escape answers on EVERY cancellable command, not only on a metered one; the pulse row arms once per mount and every command lane reads the same fact.
- Law: the escape handler is a per-mount closure, never a static method group — the host keys `+=`/`-=` on delegate IDENTITY, so one shared instance makes the second mount's attach a no-op and its detach a theft of the first mount's hook.
- Law: the three host-args projections are ONE `[Mapper] CommandMap` — each source type's field copy generates, `[UserMapping]` rows carry the `UndoMoment` and `OptionKind` resolutions, and the Started/Ended CASE stays on the pulse row because the same host args type serves both events: the discriminant is the pulse's own, unrecoverable from the argument value, so the mapper owns the columns and the row owns the case.
- Law: `Activity` is a `CapabilitySet<CommandActivity>` — `InCommand` and `InScript` were two bools whose product is genuinely open (a script runs inside a command), so the set prints its own wire and a third activity axis is one row.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CommandActivity : ICapability<CommandActivity> {
    public static readonly CommandActivity InCommand = new(key: "in-command");
    public static readonly CommandActivity InScript = new(key: "in-script");
}

public sealed record CommandQuery<TAnswer> {
    internal CommandQuery(Func<Op, Fin<CommandQuery<TAnswer>>> admit, Func<Fin<TAnswer>> read) => (Admit, Read) = (admit, read);

    internal Func<Op, Fin<CommandQuery<TAnswer>>> Admit { get; }
    internal Func<Fin<TAnswer>> Read { get; }
}

public static class CommandQuery {
    public static CommandQuery<bool> Exists(string name) => Text(
        name: name,
        read: admitted => Fin.Succ(value: Command.IsCommand(name: admitted)));

    public static CommandQuery<bool> Valid(string name) => Text(
        name: name,
        read: admitted => Fin.Succ(value: Command.IsValidCommandName(name: admitted)));

    public static CommandQuery<Option<Guid>> Resolve(string name, CommandLanguage language) => Text(
        name: name,
        read: admitted => Fin.Succ(value: Optional(Command.LookupCommandId(
            name: admitted, searchForEnglishName: language.Key)).Filter(static id => id != Guid.Empty)));

    public static CommandQuery<Option<string>> Name(Guid id, CommandLanguage language) => new(
        admit: op => guard(id != Guid.Empty, op.InvalidInput(axis: nameof(id))).ToFin()
            .Map(_ => CommandQuery.Name(id: id, language: language)),
        read: () => Fin.Succ(value: Optional(Command.LookupCommandName(
            commandId: id, englishName: language.Key)).Filter(static value => value.Length > 0)));

    public static CommandQuery<Seq<string>> Names(CommandLanguage language, CommandRoster roster) => new(
        admit: op => from _ in op.Need(language) from __ in op.Need(roster) select Names(language, roster),
        read: () => Fin.Succ(value: toSeq(Command.GetCommandNames(english: language.Key, loaded: roster.Key))));

    public static CommandQuery<Seq<RecentCommand>> Recent { get; } = Free(
        read: static () => Fin.Succ(value: toSeq(Command.GetMostRecentCommands())
            .Map(static row => new RecentCommand(Display: row.DisplayString, Macro: row.Macro))));

    public static CommandQuery<Seq<Guid>> Stack { get; } = Free(
        read: static () => Fin.Succ(value: toSeq(Command.GetCommandStack())));

    public static CommandQuery<CapabilitySet<CommandActivity>> State { get; } = Free(
        read: static () => Fin.Succ(value: CapabilitySet<CommandActivity>.Of(
            toSeq(new[] {
                Command.InCommand() ? Some(CommandActivity.InCommand) : None,
                Command.InScriptRunnerCommand() ? Some(CommandActivity.InScript) : None,
            }).Somes().ToArray().AsSpan())));

    public static CommandQuery<string> Prompt { get; } = Free(
        read: static () => Fin.Succ(value: RhinoApp.CommandPrompt));

    private static CommandQuery<TAnswer> Text<TAnswer>(string name, Func<string, Fin<TAnswer>> read) => new(
        admit: op => op.AcceptText(value: name).Map(admitted => new CommandQuery<TAnswer>(
            admit: static key => Fin.Fail<CommandQuery<TAnswer>>(error: key.InvalidResult()),
            read: () => read(admitted))),
        read: () => read(name));

    private static CommandQuery<TAnswer> Free<TAnswer>(Func<Fin<TAnswer>> read) =>
        new(admit: op => Fin.Succ(value: Free(read)), read: read);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CommandFact : IHookFact<RhinoPoint> {
    private CommandFact() { }
    public sealed record Started(CommandEvent Value) : CommandFact;
    public sealed record Ended(CommandEvent Value) : CommandFact;
    public sealed record Undo(UndoEvent Value) : CommandFact;
    public sealed record PromptChanged(PromptEvent Value) : CommandFact;
    public sealed record Escaped(EscapeEvent Value) : CommandFact;
    public sealed record Rejected(Error Value) : CommandFact;

    public bool Seats(RhinoPoint at) => Switch(
        state: at,
        started: static (row, _) => row == RhinoPoint.CommandBegin,
        ended: static (row, _) => row == RhinoPoint.CommandEnd,
        undo: static (row, _) => row == RhinoPoint.CommandUndo,
        promptChanged: static (row, _) => row == RhinoPoint.CommandPrompt,
        escaped: static (row, _) => row == RhinoPoint.CommandEscape,
        rejected: static (row, _) => toSeq(CommandPulse.Items)
            .Exists(pulse => pulse != CommandPulse.Escape && pulse.Point() == row));
}

public sealed record CommandEvent(Guid Id, string English, string Local, string Help, string Plugin, CommandVerdict Verdict, uint DocumentSerial);
public sealed record UndoEvent(uint DocumentSerial, Guid CommandId, uint UndoSerial, UndoMoment Moment);

[Generator.Equals.Equatable]
public sealed partial record PromptEvent(
    string Prompt,
    string Default,
    [property: Generator.Equals.OrderedEquality] Seq<CommandOptionEvent> Options);

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

[SmartEnum<bool>]
public sealed partial class CommandLanguage {
    public static readonly CommandLanguage English = new(key: true);
    public static readonly CommandLanguage Local = new(key: false);
}

[SmartEnum<bool>]
public sealed partial class CommandRoster {
    public static readonly CommandRoster Loaded = new(key: true);
    public static readonly CommandRoster Installed = new(key: false);
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

// --- [OPERATIONS] ----------------------------------------------------------------------
[Riok.Mapperly.Abstractions.Mapper(
    RequiredMappingStrategy = Riok.Mapperly.Abstractions.RequiredMappingStrategy.Both,
    EnabledConversions = Riok.Mapperly.Abstractions.MappingConversionType.All & ~Riok.Mapperly.Abstractions.MappingConversionType.ExplicitCast)]
internal static partial class CommandMap {
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.CommandId), nameof(CommandEvent.Id))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.CommandEnglishName), nameof(CommandEvent.English))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.CommandLocalName), nameof(CommandEvent.Local))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.CommandHelpURL), nameof(CommandEvent.Help))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.CommandPluginName), nameof(CommandEvent.Plugin))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.CommandResult), nameof(CommandEvent.Verdict))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(CommandEventArgs.DocumentRuntimeSerialNumber), nameof(CommandEvent.DocumentSerial))]
    internal static partial Fin<CommandEvent> ToEvent(CommandEventArgs args);

    [Riok.Mapperly.Abstractions.MapProperty(nameof(UndoRedoEventArgs.DocumentSerialNumber), nameof(UndoEvent.DocumentSerial))]
    [Riok.Mapperly.Abstractions.MapProperty(nameof(UndoRedoEventArgs.UndoSerialNumber), nameof(UndoEvent.UndoSerial))]
    internal static partial Fin<UndoEvent> ToEvent(UndoRedoEventArgs args);

    internal static partial Fin<PromptEvent> ToEvent(CommandPromptChangedEventArgs args);

    [Riok.Mapperly.Abstractions.UserMapping]
    private static Fin<CommandVerdict> Verdict(Result result) =>
        CommandVerdict.OfNative(result: result, key: Op.Of(name: nameof(CommandMap)));

    [Riok.Mapperly.Abstractions.UserMapping]
    private static Fin<UndoMoment> Moment(UndoRedoEventArgs args) =>
        UndoMoment.Items.AsIterable().Find(moment => moment.Matches(args))
            .ToFin(Fail: Op.Of(name: nameof(CommandMap)).InvalidResult());

    [Riok.Mapperly.Abstractions.UserMapping]
    private static Seq<CommandOptionEvent> Options(CommandPromptChangedEventArgs args) =>
        toSeq(args.Options ?? []).Choose(option => OptionKind
            .Of(native: option.OptionType, key: Op.Of(name: nameof(CommandOptionEvent)))
            .ToOption()
            .Map(kind => new CommandOptionEvent(
                Index: option.Index,
                Kind: kind,
                English: option.EnglishName,
                Local: option.LocalName,
                Value: option.StringOptionValue,
                ListIndex: option.CurrentListOptionIndex,
                Toggle: Optional(option.CurrentToggleValue))));
}

[SmartEnum<int>]
public sealed partial class CommandPulse {
    public static readonly CommandPulse Begin = new(key: 0, point: static () => RhinoPoint.CommandBegin,
        attach: static (rail, op) => Subscription.Attach(
            subscribe: static (EventHandler<CommandEventArgs> handler) => Command.BeginCommand += handler,
            unsubscribe: static handler => Command.BeginCommand -= handler,
            handler: (_, args) => ignore(rail.Fire(
                at: RhinoPoint.CommandBegin,
                fact: CommandMap.ToEvent(args: args).Match<CommandFact>(
                    Succ: static value => new CommandFact.Started(Value: value),
                    Fail: static error => new CommandFact.Rejected(Value: error)),
                key: op))));
    public static readonly CommandPulse End = new(key: 1, point: static () => RhinoPoint.CommandEnd,
        attach: static (rail, op) => Subscription.Attach(
            subscribe: static (EventHandler<CommandEventArgs> handler) => Command.EndCommand += handler,
            unsubscribe: static handler => Command.EndCommand -= handler,
            handler: (_, args) => ignore(rail.Fire(
                at: RhinoPoint.CommandEnd,
                fact: CommandMap.ToEvent(args: args).Match<CommandFact>(
                    Succ: static value => new CommandFact.Ended(Value: value),
                    Fail: static error => new CommandFact.Rejected(Value: error)),
                key: op))));
    public static readonly CommandPulse UndoRedo = new(key: 2, point: static () => RhinoPoint.CommandUndo,
        attach: static (rail, op) => Subscription.Attach(
            subscribe: static (EventHandler<UndoRedoEventArgs> handler) => Command.UndoRedo += handler,
            unsubscribe: static handler => Command.UndoRedo -= handler,
            handler: (_, args) => ignore(rail.Fire(
                at: RhinoPoint.CommandUndo,
                fact: CommandMap.ToEvent(args: args).Match<CommandFact>(
                    Succ: static value => new CommandFact.Undo(Value: value),
                    Fail: static error => new CommandFact.Rejected(Value: error)),
                key: op))));
    public static readonly CommandPulse Prompt = new(key: 3, point: static () => RhinoPoint.CommandPrompt,
        attach: static (rail, op) => Subscription.Attach(
            subscribe: static (EventHandler<CommandPromptChangedEventArgs> handler) => RhinoApp.CommandPromptChanged += handler,
            unsubscribe: static handler => RhinoApp.CommandPromptChanged -= handler,
            handler: (_, args) => ignore(rail.Fire(
                at: RhinoPoint.CommandPrompt,
                fact: CommandMap.ToEvent(args: args).Match<CommandFact>(
                    Succ: static value => new CommandFact.PromptChanged(Value: value),
                    Fail: static error => new CommandFact.Rejected(Value: error)),
                key: op))));
    public static readonly CommandPulse Escape = new(key: 4, point: static () => RhinoPoint.CommandEscape,
        attach: static (rail, op) => Subscription.Attach(
            subscribe: static (EventHandler handler) => RhinoApp.EscapeKeyPressed += handler,
            unsubscribe: static handler => RhinoApp.EscapeKeyPressed -= handler,
            handler: (_, _) => ignore(rail.Fire(
                at: RhinoPoint.CommandEscape,
                fact: new CommandFact.Escaped(Value: EscapeEvent.Signalled),
                key: op))));

    [UseDelegateFromConstructor]
    internal partial RhinoPoint Point();

    [UseDelegateFromConstructor]
    internal partial Fin<Subscription> Attach(HookRail<RhinoPoint, CommandFact, PluginKey> rail, Op op);

    public static Fin<Subscription> Mount(
        HookRail<RhinoPoint, CommandFact, PluginKey> rail,
        Op? key = null,
        params ReadOnlySpan<CommandPulse> pulses) {
        Op op = key.OrDefault(name: nameof(CommandPulse));
        Seq<CommandPulse> candidates = toSeq(pulses.ToArray());
        return from active in op.Need(rail)
               from _ in guard(!candidates.IsEmpty && candidates.ForAll(static pulse => pulse is not null), op.InvalidInput())
               from attached in Subscription.AttachAll(candidates.Distinct().Map(pulse =>
                   (Func<Fin<Subscription>>)(() => pulse.Attach(active, op))))
               select attached;
    }
}

public static class CommandRegistry {
    public static Fin<TAnswer> Ask<TAnswer>(CommandQuery<TAnswer> query, Op? key = null) {
        Op op = key.OrDefault();
        return op.Need(query)
            .Bind(request => request.Admit(op))
            .Bind(request => op.Catch(request.Read));
    }
}
```

## [06]-[SCRIPT]

`Scripting.Run` targets the admitted session document and preserves the native terminal. `Scripting.Proxy` dispatches one proxy inside the same document and thread grant: the caller supplies a typed body over the admitted `DocumentSession`, the re-closed `SessionMode`, and its own payload type, and the host `RunCommandDelegate`, `RhinoDoc`, `RunMode`, and `object data` stay inside the adapter. Script text, echo, and MRU display are case evidence; named dispatch validates registry membership before execution.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScriptOp {
    private ScriptOp() { }
    public sealed record Macro(string Text, ScriptEcho Echo, Option<string> Display = default) : ScriptOp;
    public sealed record Named(string CommandName) : ScriptOp;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Scripting {
    public static Fin<CommandVerdict> Run(DocumentSession session, ScriptOp script, Op? key = null) {
        Op op = key.OrDefault();
        return from request in op.Need(script)
               from target in op.Need(session)
               from _ in guard(RhinoApp.IsOnMainThread, op.InvalidContext())
               from verdict in target.Demand(
                   use: document => request.Switch(
                       state: (Serial: (uint)target.Key, Document: document, Op: op),
                       macro: static (held, run) =>
                           from text in held.Op.AcceptText(value: run.Text)
                           from _ in guard(run.Echo is not null, held.Op.InvalidInput(axis: nameof(Macro.Echo)))
                           from ok in held.Op.Catch(() => Fin.Succ(value: run.Display.Case switch {
                               string display => RhinoApp.RunScript(documentSerialNumber: held.Serial, script: text, mruDisplayString: display, echo: run.Echo.Key),
                               _ => RhinoApp.RunScript(documentSerialNumber: held.Serial, script: text, echo: run.Echo.Key),
                           }))
                           select ok ? CommandVerdict.Completed : CommandVerdict.Failed,
                       named: static (held, run) =>
                           from name in held.Op.AcceptText(value: run.CommandName)
                           from _ in guard(Command.IsCommand(name: name), held.Op.InvalidInput(axis: nameof(Named.CommandName)))
                           from native in held.Op.Catch(() => Fin.Succ(value: RhinoApp.ExecuteCommand(document: held.Document, commandName: name)))
                           from result in CommandVerdict.OfNative(result: native, key: held.Op)
                           select result),
                   key: op,
                   needs: [SessionNeed.Acquire])
               select verdict;
    }

    public static Fin<Unit> Proxy<TPayload>(
        DocumentSession session,
        Func<DocumentSession, SessionMode, TPayload, Fin<CommandVerdict>> body,
        TPayload payload,
        Op? key = null)
        where TPayload : notnull {
        Op op = key.OrDefault();
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

- Packages: `RhinoCommon` (`Rasm.Rhino/.api/api-rhinocommon-commands.md` — `Command` subclassing, `RunCommand`, command-context enums); `Thinktecture.Runtime.Extensions` (`libs/dotnet/.api/api-thinktecture-runtime-extensions.md` — `[SmartEnum]` census rows, `[ComplexValueObject]`/`[ValueObject]` carriers); `Riok.Mapperly` (`libs/dotnet/.api/api-mapperly.md` — the `CommandMap` `[Mapper]`); kernel `Domain/rails` (`Op`, `Fin`) and `Domain/hooks` (`HookRail`, `IHookFact`).

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
