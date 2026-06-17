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
                    DetailViewObject value when !value.CommitViewportChanges() =>
                        Fin.Fail<Unit>(error: Op.Of().InvalidResult()),
                    _ => Fin.Succ(value: Op.Side(() => ctx.View.Redraw())),
                },
            deferred: static (ctx, _) => Fin.Succ(value: Op.Side(() => ctx.Document.Views.Redraw(deferred: true))));
}

// --- [CONSTANTS] --------------------------------------------------------------------------
internal static class CameraDefaults { internal const double LensLength = 50.0, FramePadding = 1.1; internal const int DetailCacheDocuments = 8; }

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct CameraOutcome<T>(
    T Value,
    RedrawRequest Redraw,
    Seq<Commands.DocumentResourceChange> Resources) {
    internal static CameraOutcome<T> Create(T value, RedrawRequest? redraw = null, Seq<Commands.DocumentResourceChange>? resources = null) =>
        new(Value: value, Redraw: redraw ?? RedrawRequest.Empty, Resources: resources ?? Seq<Commands.DocumentResourceChange>());

    internal static CameraOutcome<T> WithResource(T value, Commands.DocumentResourceChange change, RedrawRequest? redraw = null) =>
        Create(value: value, redraw: redraw, resources: toSeq([change]));
}

public readonly record struct CameraScopeResult<T>(
    CameraScope Scope,
    Option<T> Value,
    Option<Error> Failure,
    RedrawRequest Redraw,
    Seq<Commands.DocumentResourceChange> Resources) {
    public bool Succeeded => Failure.IsNone;

    internal static CameraScopeResult<T> FromOutcome(CameraScope scope, Fin<CameraOutcome<T>> result) =>
        result.Match(
            Succ: value => new CameraScopeResult<T>(
                Scope: scope,
                Value: Some(value: value.Value),
                Failure: Option<Error>.None,
                Redraw: value.Redraw,
                Resources: value.Resources),
            Fail: error => new CameraScopeResult<T>(
                Scope: scope,
                Value: Option<T>.None,
                Failure: Some(value: error),
                Redraw: RedrawRequest.Empty,
                Resources: Seq<Commands.DocumentResourceChange>()));
}

[SmartEnum<int>]
public sealed partial class CameraSyncPolicy {
    // AbortOnFirst polarity is load-bearing: Independent viewports are each their own authority, so the first
    // failure short-circuits the whole rail (TraverseM monadic abort); Coordinated is a set that tolerates
    // partial success, so every member runs and the fold guards on at-least-one-success (accumulate-then-guard).
    public static readonly CameraSyncPolicy
        Independent = new(key: 0, abortOnFirst: true, mergeRedraw: false),
        Coordinated = new(key: 1, abortOnFirst: false, mergeRedraw: true);

    private bool AbortOnFirst { get; }
    public bool MergeRedraw { get; }

    internal Fin<Seq<CameraScopeResult<T>>> Traverse<T>(
        Seq<CameraScope> scopes,
        Func<CameraScope, Fin<CameraOutcome<T>>> run) =>
        Traverse(plan: scopes.Map(scope => (Scope: scope, Run: run)));

    // Correlated overload: each scope carries its own run (the CameraRig case where every viewport
    // gets a distinct CameraOp). The abort/accumulate algebra is the policy row's, identical to the
    // shared-run overload above — only the per-element run differs.
    internal Fin<Seq<CameraScopeResult<T>>> Traverse<T>(
        Seq<(CameraScope Scope, Func<CameraScope, Fin<CameraOutcome<T>>> Run)> plan) =>
        AbortOnFirst
            ? plan.TraverseM(pair => pair.Run(arg: pair.Scope)
                    .Map(outcome => CameraScopeResult<T>.FromOutcome(scope: pair.Scope, result: Fin.Succ(value: outcome))))
                .As()
            : plan.Traverse(pair => Fin.Succ(value: CameraScopeResult<T>.FromOutcome(
                    scope: pair.Scope,
                    result: pair.Run(arg: pair.Scope))))
                .As()
                .Bind(receipts => guard(
                        receipts.Exists(static receipt => receipt.Succeeded),
                        () => receipts.Find(static receipt => receipt.Failure.IsSome)
                            .Bind(static receipt => receipt.Failure)
                            .IfNone(() => Op.Of().InvalidResult())).ToFin()
                    .Map(_ => receipts));

    public CameraOutcome<Seq<CameraScopeResult<T>>> FoldReceipts<T>(Seq<CameraScopeResult<T>> receipts) {
        Seq<CameraScopeResult<T>> succeeded = receipts.Filter(static receipt => receipt.Succeeded);
        return CameraOutcome<Seq<CameraScopeResult<T>>>.Create(
            value: receipts,
            redraw: MergeRedraw
                ? receipts.Fold(RedrawRequest.Empty, static (left, right) => left | right.Redraw)
                : succeeded.IsEmpty
                    ? RedrawRequest.Empty
                    : succeeded.Last().Redraw,
            resources: receipts.Bind(static receipt => receipt.Resources));
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public sealed class RhinoCamera {
    private static readonly Op ScopeKey = Op.Of(name: nameof(Scope));
    private static readonly Op BroadcastKey = Op.Of(name: nameof(Broadcast));
    private static readonly Op RigKey = Op.Of(name: nameof(Rig));

    private RhinoCamera(RhinoDoc document, RunMode mode) {
        Document = document ?? throw new ArgumentNullException(paramName: nameof(document));
        Mode = mode;
    }

    public RhinoDoc Document { get; }
    public RunMode Mode { get; }

    public static RhinoCamera Live(RhinoDoc document, RunMode mode = RunMode.Interactive) =>
        new(document: document, mode: mode);

    public Fin<CameraScope> Scope(ViewportTarget target) =>
        from active in ActiveDocument
        from valid in Optional(target).ToFin(Fail: ScopeKey.InvalidInput())
        from scopes in valid.Resolve(document: active, op: ScopeKey)
        from scope in SingleScope(scopes: scopes, op: ScopeKey)
        select scope;

    public Fin<Seq<CameraScope>> Scopes(ViewportTarget target) =>
        from active in ActiveDocument
        from valid in Optional(target).ToFin(Fail: ScopeKey.InvalidInput())
        from scopes in valid.Resolve(document: active, op: ScopeKey)
        select scopes;

    public Fin<T> In<T>(ViewportTarget target, Func<CameraScope, Fin<T>> use) =>
        from valid in Optional(use).ToFin(Fail: Op.Of().InvalidInput())
        from scope in Scope(target: target)
        from result in valid(arg: scope)
        select result;

    public Fin<CameraOutcome<T>> Run<T>(CameraOp<T> operation, ViewportTarget target) =>
        from valid in Optional(operation).ToFin(Fail: Op.Of().InvalidInput())
        from outcome in RhinoUi.DispatchThread(
            uiBound: valid.UiBound,
            mode: Mode,
            run: () => ExecuteRun(operation: valid, target: target),
            name: nameof(Run))
        select outcome;

    public Fin<T> RunValue<T>(CameraOp<T> operation, ViewportTarget target) =>
        Run(operation: operation, target: target).Map(outcome => outcome.Value);

    public Fin<CameraOutcome<Seq<CameraScopeResult<T>>>> Broadcast<T>(
        CameraOp<T> operation,
        ViewportTarget target,
        CameraSyncPolicy policy) =>
        from valid in Optional(operation).ToFin(Fail: BroadcastKey.InvalidInput())
        from outcome in RhinoUi.DispatchThread(
            uiBound: valid.UiBound,
            mode: Mode,
            run: () =>
                from scopes in Scopes(target: target)
                from folded in ExecuteBroadcast(operation: valid, scopes: scopes, policy: policy)
                select folded,
            name: nameof(Broadcast))
        select outcome;

    public Fin<CameraOutcome<Seq<CameraScopeResult<CameraChangeReceipt>>>> Rig(
        Seq<(ViewportTarget Target, CameraOp<CameraChangeReceipt> Op)> assignments,
        CameraSyncPolicy policy) =>
        from valid in guard(!assignments.IsEmpty, RigKey.InvalidInput()).ToFin()
        from chosen in Optional(policy).ToFin(Fail: RigKey.InvalidInput())
        from outcome in RhinoUi.DispatchThread(
            uiBound: true,
            mode: Mode,
            run: () =>
                from plan in assignments.TraverseM(pair =>
                    from op in Optional(pair.Op).ToFin(Fail: RigKey.InvalidInput())
                    from scope in Scope(target: pair.Target)
                    select (Scope: scope, Run: (Func<CameraScope, Fin<CameraOutcome<CameraChangeReceipt>>>)(s => ExecuteRunOnScope(operation: op, scope: s)))).As()
                from folded in chosen.Traverse(plan: plan).Map(chosen.FoldReceipts)
                select folded,
            name: nameof(Rig))
        select outcome;

    public static UiIntent<CameraOutcome<T>> Intent<T>(CameraOp<T> operation, ViewportTarget target) =>
        UiIntent.Operation(run: (doc, mode) => Live(document: doc, mode: mode).Run(operation: operation, target: target));

    internal static Fin<CameraScope> SingleScope(Seq<CameraScope> scopes, Op op) =>
        guard(scopes.Count == 1, op.InvalidInput()).ToFin().Map(_ => scopes[0]);

    private Fin<RhinoDoc> ActiveDocument =>
        Document switch {
            { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false } doc => Fin.Succ(value: doc),
            _ => Fin.Fail<RhinoDoc>(error: ScopeKey.MissingContext()),
        };

    private static Fin<CameraOutcome<T>> ExecuteRunOnScope<T>(CameraOp<T> operation, CameraScope scope) =>
        from outcome in operation.Run(arg: scope)
        from _ in outcome.Redraw.ApplyTo(scope: scope)
        select outcome with { Redraw = RedrawRequest.Empty };

    private Fin<CameraOutcome<T>> ExecuteRun<T>(CameraOp<T> operation, ViewportTarget target) =>
        from scope in Scope(target: target)
        from outcome in ExecuteRunOnScope(operation: operation, scope: scope)
        select outcome;

    private static Fin<CameraOutcome<Seq<CameraScopeResult<T>>>> ExecuteBroadcast<T>(
        CameraOp<T> operation,
        Seq<CameraScope> scopes,
        CameraSyncPolicy policy) =>
        policy.Traverse(scopes: scopes, run: scope => ExecuteRunOnScope(operation: operation, scope: scope))
            .Map(policy.FoldReceipts);
}
