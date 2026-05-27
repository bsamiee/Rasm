using Rasm.Rhino.UI;

namespace Rasm.Rhino.Camera;

// --- [TYPES] ------------------------------------------------------------------------------
[Union(SwitchMapStateParameterName = "scope")]
public abstract partial record RedrawRequest {
    private RedrawRequest() { }

    public sealed record None : RedrawRequest;
    public sealed record View : RedrawRequest;
    public sealed record DetailCommit : RedrawRequest;
    public sealed record Deferred : RedrawRequest;

    public static RedrawRequest Empty { get; } = new None();

    public static RedrawRequest operator |(RedrawRequest left, RedrawRequest right) =>
        (left, right) switch {
            (None, RedrawRequest value) => value,
            (RedrawRequest value, None) => value,
            (DetailCommit, _) or (_, DetailCommit) => new DetailCommit(),
            (Deferred, _) or (_, Deferred) => new Deferred(),
            (View, _) or (_, View) => new View(),
            _ => left,
        };

    internal Fin<Unit> ApplyTo(CameraScope scope) =>
        Switch(
            scope,
            none: static (_, __) => Fin.Succ(value: unit),
            view: static (ctx, _) => Fin.Succ(value: Op.Side(() => ctx.View.Redraw())),
            detailCommit: static (ctx, _) =>
                ctx.Detail.Case switch {
                    DetailViewObject value when value.CommitViewportChanges() => Fin.Succ(value: Op.Side(() => ctx.View.Redraw())),
                    DetailViewObject => Fin.Fail<Unit>(error: Op.Of(name: nameof(ApplyTo)).InvalidResult()),
                    _ => Fin.Succ(value: Op.Side(() => ctx.View.Redraw())),
                },
            deferred: static (_, __) => Fin.Succ(value: unit));
}

public readonly record struct CameraScopeReceipt<T>(
    CameraScope Scope,
    Option<T> Value,
    Option<Error> Failure,
    RedrawRequest Redraw,
    Seq<Commands.DocumentResourceChange> Resources) {
    public bool Succeeded => Failure.IsNone;

    internal static CameraScopeReceipt<T> FromOutcome(CameraScope scope, Fin<CameraOutcome<T>> result) =>
        result.Match(
            Succ: value => new CameraScopeReceipt<T>(
                Scope: scope,
                Value: Some(value: value.Value),
                Failure: Option<Error>.None,
                Redraw: value.Redraw,
                Resources: value.Resources),
            Fail: error => new CameraScopeReceipt<T>(
                Scope: scope,
                Value: Option<T>.None,
                Failure: Some(value: error),
                Redraw: RedrawRequest.Empty,
                Resources: Seq<Commands.DocumentResourceChange>()));
}

public readonly record struct CameraOutcome<T>(
    T Value,
    RedrawRequest Redraw,
    Seq<Commands.DocumentResourceChange> Resources);

internal static class CameraOutcomeCreate {
    internal static CameraOutcome<T> Value<T>(T value, RedrawRequest? redraw = null, Seq<Commands.DocumentResourceChange>? resources = null) =>
        new(Value: value, Redraw: redraw ?? RedrawRequest.Empty, Resources: resources ?? Seq<Commands.DocumentResourceChange>());

    internal static CameraOutcome<T> Resource<T>(T value, Commands.DocumentResourceChange change, RedrawRequest? redraw = null) =>
        Value(value: value, redraw: redraw, resources: toSeq([change]));
}

public readonly record struct CameraSyncPolicy(
    bool StopOnFirstFailure = true,
    bool MergeRedraw = true) {
    public static CameraSyncPolicy Independent { get; } = new(StopOnFirstFailure: true, MergeRedraw: false);
    public static CameraSyncPolicy Rig { get; } = new(StopOnFirstFailure: false, MergeRedraw: true);

    public CameraOutcome<T> Fold<T>(Seq<CameraOutcome<T>> outcomes) {
        CameraSyncPolicy policy = this;
        return outcomes.Fold(
            initialState: Option<CameraOutcome<T>>.None,
            f: (state, next) => state.Case switch {
                CameraOutcome<T> current => Some(value: CameraOutcomeCreate.Value(
                    value: next.Value,
                    redraw: policy.MergeRedraw ? current.Redraw | next.Redraw : next.Redraw,
                    resources: current.Resources + next.Resources)),
                _ => Some(value: next),
            })
            .IfNone(() => CameraOutcomeCreate.Value(value: default(T)!));
    }

    public CameraOutcome<Seq<CameraScopeReceipt<T>>> FoldReceipts<T>(Seq<CameraScopeReceipt<T>> receipts) {
        CameraSyncPolicy policy = this;
        Seq<CameraScopeReceipt<T>> succeeded = receipts.Filter(static receipt => receipt.Succeeded);
        return CameraOutcomeCreate.Value(
            value: receipts,
            redraw: policy.MergeRedraw
                ? receipts.Fold(RedrawRequest.Empty, static (left, right) => left | right.Redraw)
                : succeeded.IsEmpty
                    ? RedrawRequest.Empty
                    : succeeded.Last().Redraw,
            resources: receipts.Fold(Seq<Commands.DocumentResourceChange>(), static (left, right) => left + right.Resources));
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoCamera {
    private static readonly Op ScopeKey = Op.Of(name: nameof(Scope));

    private RhinoCamera(RhinoDoc document, RunMode mode) {
        Document = document ?? throw new ArgumentNullException(paramName: nameof(document));
        Mode = mode;
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }

    public static RhinoCamera Live(RhinoDoc document, RunMode mode = RunMode.Interactive) =>
        new(document: document, mode: mode);

    private Fin<RhinoDoc> ActiveDocument =>
        Document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } doc => Fin.Succ(value: doc),
            _ => Fin.Fail<RhinoDoc>(error: ScopeKey.MissingContext()),
        };

    public Fin<CameraScope> Scope(ViewportTarget target) =>
        from active in ActiveDocument
        from valid in Optional(target).ToFin(Fail: ScopeKey.InvalidInput())
        from scope in valid.Resolve(document: active, op: ScopeKey)
        select scope;

    public Fin<Seq<CameraScope>> Scopes(ViewportTarget target) =>
        from active in ActiveDocument
        from valid in Optional(target).ToFin(Fail: ScopeKey.InvalidInput())
        from scopes in valid.ResolveMany(document: active, op: ScopeKey)
        select scopes;

    public Fin<T> In<T>(ViewportTarget target, Func<CameraScope, Fin<T>> use) =>
        from valid in Optional(use).ToFin(Fail: Op.Of(name: nameof(In)).InvalidInput())
        from scope in Scope(target: target)
        from result in valid(arg: scope)
        select result;

    public Fin<CameraOutcome<T>> Run<T>(CameraOp<T> operation, ViewportTarget target) =>
        Optional(operation).ToFin(Fail: Op.Of(name: nameof(Run)).InvalidInput())
            .Bind(valid => RhinoUi.DispatchThread(
                uiBound: valid.RequiresUiThread(),
                mode: Mode,
                run: () => ExecuteRun(operation: valid, target: target),
                name: nameof(Run)));

    public Fin<T> RunValue<T>(CameraOp<T> operation, ViewportTarget target) =>
        Run(operation: operation, target: target).Map(outcome => outcome.Value);

    public Fin<CameraOutcome<Seq<CameraScopeReceipt<T>>>> Broadcast<T>(
        CameraOp<T> operation,
        ViewportTarget target,
        CameraSyncPolicy policy) =>
        Optional(operation).ToFin(Fail: Op.Of(name: nameof(Broadcast)).InvalidInput())
            .Bind(valid => RhinoUi.DispatchThread(
                uiBound: valid.RequiresUiThread(),
                mode: Mode,
                run: () =>
                    from scopes in Scopes(target: target)
                    from folded in ExecuteBroadcast(operation: valid, scopes: scopes, policy: policy)
                    select folded,
                name: nameof(Broadcast)));

    public static UiIntent<CameraOutcome<T>> Intent<T>(CameraOp<T> operation, ViewportTarget target) =>
        UiIntent.Operation(run: (doc, mode) => Live(document: doc, mode: mode).Run(operation: operation, target: target));

    private Fin<CameraOutcome<T>> ExecuteRun<T>(CameraOp<T> operation, ViewportTarget target) =>
        from scope in Scope(target: target)
        from outcome in ExecuteRunOnScope(operation: operation, scope: scope)
        select outcome;

    private static Fin<CameraOutcome<Seq<CameraScopeReceipt<T>>>> ExecuteBroadcast<T>(
        CameraOp<T> operation,
        Seq<CameraScope> scopes,
        CameraSyncPolicy policy) =>
        policy.StopOnFirstFailure switch {
            true =>
                from receipts in scopes.TraverseM(scope =>
                        ExecuteRunOnScope(operation: operation, scope: scope)
                            .Map(outcome => CameraScopeReceipt<T>.FromOutcome(scope: scope, result: Fin.Succ(value: outcome))))
                    .As()
                select policy.FoldReceipts(receipts: receipts),
            false =>
                from receipts in scopes
                    .Traverse(scope =>
                        ExecuteRunOnScope(operation: operation, scope: scope)
                            .Match(
                                Succ: outcome => Fin.Succ(value: CameraScopeReceipt<T>.FromOutcome(scope: scope, result: Fin.Succ(value: outcome))),
                                Fail: error => Fin.Succ(value: CameraScopeReceipt<T>.FromOutcome(scope: scope, result: Fin.Fail<CameraOutcome<T>>(error: error)))))
                    .As()
                from folded in ResolveBroadcastFin(receipts: receipts, policy: policy)
                select folded,
        };

    private static Fin<CameraOutcome<Seq<CameraScopeReceipt<T>>>> ResolveBroadcastFin<T>(
        Seq<CameraScopeReceipt<T>> receipts,
        CameraSyncPolicy policy) =>
        receipts.Exists(static receipt => receipt.Succeeded) switch {
            true => Fin.Succ(value: policy.FoldReceipts(receipts: receipts)),
            false => Fin.Fail<CameraOutcome<Seq<CameraScopeReceipt<T>>>>(
                error: receipts.Find(static receipt => receipt.Failure.IsSome)
                    .Bind(static receipt => receipt.Failure)
                    .IfNone(() => Op.Of(name: "Broadcast").InvalidResult())),
        };

    private static Fin<CameraOutcome<T>> ExecuteRunOnScope<T>(CameraOp<T> operation, CameraScope scope) =>
        from outcome in operation.Run(arg: scope)
        from _ in outcome.Redraw.ApplyTo(scope: scope)
        select outcome with { Redraw = RedrawRequest.Empty };
}
