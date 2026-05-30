using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Rhino;
using Expected = Rasm.Domain.Expected;
using GhCanvas = Grasshopper2.UI.Canvas.Canvas;
using GhDocument = Grasshopper2.Doc.Document;
using GhDocumentMethods = Grasshopper2.Doc.DocumentMethods;
using GhEditor = Grasshopper2.UI.Editor;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;
using SolutionMode = Grasshopper2.Doc.SolutionMode;
using UndoAction = Grasshopper2.Undo.Action;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public partial record RepaintRequest {
    private RepaintRequest() { }
    public sealed record NoneCase : RepaintRequest;
    public sealed record ObjectCase(Guid Id) : RepaintRequest;
    public sealed record RegionCase(RectangleF Bounds) : RepaintRequest;
    public sealed record CanvasCase : RepaintRequest;
    public sealed record ScheduledCase : RepaintRequest;
    public sealed record SolutionCase : RepaintRequest;
    public sealed record DisplayCase : RepaintRequest;
    public sealed record SolutionAndDisplayCase : RepaintRequest;

    internal Unit ApplyTo(GrasshopperUi.Scope scope) =>
        Switch(state: scope,
            noneCase: static (_, _) => unit,
            objectCase: static (s, o) => s.Canvas.Bind(canvas => s.Objects.Map(objects =>
                Optional(objects.Find(instanceId: o.Id))
                    .Map(target => { canvas.Invalidate(rect: GrasshopperUi.ControlSpace(canvas: canvas, bounds: target.Attributes.AggregateBounds)); return unit; })
                    .IfNone(() => { canvas.Invalidate(); return unit; }))).IfNone(unit),
            regionCase: static (s, r) => s.Canvas.IfSome(canvas => canvas.Invalidate(rect: GrasshopperUi.ControlSpace(canvas: canvas, bounds: r.Bounds))),
            canvasCase: static (s, _) => s.Canvas.IfSome(static canvas => canvas.Invalidate()),
            scheduledCase: static (s, _) => s.Canvas.IfSome(static canvas => canvas.ScheduleRedraw()),
            solutionCase: static (s, op) => s.Document.IfSome(static doc => { _ = doc.Solution.Start(mode: SolutionMode.Regular); return unit; }),
            displayCase: static (s, _) => s.Document.IfSome(static doc => { doc.Display.UpdateDisplay(); return unit; }),
            solutionAndDisplayCase: static (s, op) => s.Document.IfSome(static doc => {
                _ = doc.Solution.Start(mode: SolutionMode.Regular);
                doc.Display.UpdateDisplay();
                return unit;
            }));

    public static readonly RepaintRequest None = new NoneCase();
    public static readonly RepaintRequest Canvas = new CanvasCase();
    public static readonly RepaintRequest Scheduled = new ScheduledCase();
    public static readonly RepaintRequest Solution = new SolutionCase();
    public static readonly RepaintRequest Display = new DisplayCase();
    public static readonly RepaintRequest SolutionAndDisplay = new SolutionAndDisplayCase();
    public static RepaintRequest Object(Guid id) => new ObjectCase(Id: id);
    public static RepaintRequest Region(RectangleF bounds) => new RegionCase(Bounds: bounds);

    // Absorption priority (None identity → SolutionAndDisplay → Solution → Display → Canvas → Scheduled
    // → Region union → same-Object idempotent → else Canvas). ApplyTo runs once at GrasshopperUi.Use exit;
    // CanvasOp.Invalidate dispatch is policy-only and never calls native invalidate directly.
    public static RepaintRequest operator |(RepaintRequest left, RepaintRequest right) =>
        (left, right) switch {
            (NoneCase, _) => right,
            (_, NoneCase) => left,
            (SolutionAndDisplayCase, _) or (_, SolutionAndDisplayCase) => SolutionAndDisplay,
            (SolutionCase, DisplayCase) or (DisplayCase, SolutionCase) => SolutionAndDisplay,
            (SolutionCase, _) or (_, SolutionCase) => Solution,
            (DisplayCase, _) or (_, DisplayCase) => Display,
            (CanvasCase, _) or (_, CanvasCase) => Canvas,
            (ScheduledCase, _) or (_, ScheduledCase) => Scheduled,
            (RegionCase l, RegionCase r) => Region(bounds: RectangleF.Union(l.Bounds, r.Bounds)),
            (ObjectCase l, ObjectCase r) when l.Id == r.Id => left,
            _ => Canvas,
        };
}

// Auto: caller commits via UiRail.CommitActions OR native path records no undo (wire selection mirrors ObjectList.SelectWire).
// Manual: caller-supplied record delegate captures the action list.
[SkipUnionOps]
[Union]
internal partial record UndoStrategy {
    private UndoStrategy() { }
    public sealed record AutoCase : UndoStrategy;
    public sealed record ManualCase(Func<GrasshopperUi.Scope, UndoEntry> Record) : UndoStrategy;

    public static readonly UndoStrategy Auto = new AutoCase();
    public static UndoStrategy Manual(Func<GrasshopperUi.Scope, UndoEntry> record) => new ManualCase(Record: record);
}

[SmartEnum<int>]
public sealed partial class SubscriptionTeardown {
    public static readonly SubscriptionTeardown RunAlways = new(key: 0);
    public static readonly SubscriptionTeardown DetachOnce = new(key: 1);
    public static readonly SubscriptionTeardown TokenGated = new(key: 2);
}

[SkipUnionOps]
[Union]
public partial record Subscription : IDisposable {
    private Subscription() { }
    // Teardown: RunAlways (LIFO composite, repeat detach), DetachOnce (pacer/timer), TokenGated (OwnedSubscription).
    public sealed record AtomCase(System.Action Detach, bool MarshalToUi, SubscriptionTeardown Teardown) : Subscription;
    public sealed record CompositeCase(Seq<Subscription> Members) : Subscription;
    public sealed record EmptyCase : Subscription;

    public static readonly Subscription Empty = new EmptyCase();
    public static Subscription Atom(System.Action detach, bool marshalToUi = false, bool detachOnce = false) =>
        new AtomCase(
            Detach: GuardDetach(detach: detach, detachOnce: detachOnce),
            MarshalToUi: marshalToUi,
            Teardown: detachOnce ? SubscriptionTeardown.DetachOnce : SubscriptionTeardown.RunAlways);
    [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "Composite owns the pacer atom; caller disposes the returned subscription.")]
    internal static Subscription PaintPacer(Subscription paintHook, System.Action pacerRelease) =>
        paintHook | Atom(detach: pacerRelease, detachOnce: true, marshalToUi: true);
    internal static Subscription DisposeOnce(Subscription inner) =>
        inner switch {
            EmptyCase => Empty,
            AtomCase atom when atom.Teardown != SubscriptionTeardown.RunAlways => inner,
            _ => Atom(detach: inner.Dispose, detachOnce: true),
        };
    // Members stored LIFO so Dispose iterates in detach order without a reversed view allocation.
    public static Subscription Composite(Seq<Subscription> members) =>
        members.Count switch {
            0 => Empty,
            1 => members.Head.IfNone(Empty),
            _ => new CompositeCase(Members: members.Rev()),
        };

    public static Subscription operator |(Subscription left, Subscription right) =>
        (left, right) switch {
            (EmptyCase, _) => right,
            (_, EmptyCase) => left,
            (CompositeCase l, CompositeCase r) => new CompositeCase(Members: r.Members + l.Members),
            (_, CompositeCase r) => new CompositeCase(Members: r.Members.Add(left)),
            (CompositeCase l, _) => new CompositeCase(Members: Seq(right) + l.Members),
            _ => new CompositeCase(Members: Seq(right, left)),
        };

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(obj: this);
    }

    protected virtual void Dispose(bool disposing) =>
        _ = disposing
            ? Switch(
                atomCase: static a => a.MarshalToUi
                    ? GrasshopperUi.DetachOnUiThread(run: a.Detach)
                    : GrasshopperUi.Protect(valid: () => { a.Detach(); return Fin.Succ(value: unit); }),
                compositeCase: static c => { _ = c.Members.Iter(static s => s.Dispose()); return Fin.Succ(value: unit); },
                emptyCase: static _ => Fin.Succ(value: unit))
            : Fin.Succ(value: unit);

    internal static System.Action GuardDetach(System.Action detach, bool detachOnce) {
        if (!detachOnce) {
            return detach;
        }
        int gate = 0;
        return () => _ = Interlocked.Exchange(ref gate, 1) == 0 ? Op.Side(detach) : unit;
    }

    internal static Fin<Subscription> Bind(
        System.Action attach,
        System.Action detach,
        bool marshalToUi = false,
        bool detachOnce = false,
        SubscriptionTeardown? teardown = null) =>
        Try.lift(f: () => { attach(); return unit; }).Run()
            .Map(_ => (Subscription)new AtomCase(
                Detach: detachOnce ? GuardDetach(detach: detach, detachOnce: true) : detach,
                MarshalToUi: marshalToUi,
                Teardown: teardown ?? (detachOnce ? SubscriptionTeardown.DetachOnce : SubscriptionTeardown.RunAlways)))
            .MapFail(attachError => {
                Error primary = UiFault.MutationRejected(op: Op.Of(name: nameof(Bind)), detail: $"attach failed: {attachError.Message}");
                return Try.lift(f: () => { detach(); return unit; }).Run().Match(
                    Succ: _ => primary,
                    Fail: rollbackError => primary + UiFault.MutationRejected(op: Op.Of(name: nameof(Bind)), detail: $"rollback failed: {rollbackError.Message}"));
            });
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Snapshot<T>(Option<Guid> OwnerId, T Payload) {
    public Snapshot<TOut> Map<TOut>(Func<T, TOut> project) {
        ArgumentNullException.ThrowIfNull(argument: project);
        return new(OwnerId: OwnerId, Payload: project(arg: Payload));
    }
}

public static class Snapshot {
    public static Snapshot<T> Of<T>(T payload, Option<Guid> ownerId = default) =>
        new(OwnerId: ownerId, Payload: payload);
}

internal readonly record struct DocumentMutationReceipt(int Changed, Seq<Guid> Created) {
    public static DocumentMutationReceipt None => new(Changed: 0, Created: Seq<Guid>());
    public static DocumentMutationReceipt Of(int changed, Seq<Guid> created) =>
        new(Changed: changed, Created: created.Filter(static id => id != Guid.Empty));
    public static DocumentMutationReceipt Count(int changed) => new(Changed: changed, Created: Seq<Guid>());
    public static DocumentMutationReceipt From(bool changed) => changed ? Count(changed: 1) : None;
    public static DocumentMutationReceipt CreatedObject(Guid id) =>
        id switch {
            Guid value when value != Guid.Empty => new(Changed: 1, Created: Seq(value)),
            _ => None,
        };
    public static DocumentMutationReceipt operator +(DocumentMutationReceipt left, DocumentMutationReceipt right) =>
        new(Changed: left.Changed + right.Changed, Created: left.Created + right.Created);
}
public readonly record struct DocumentMutationDelta(int Changed, DocumentSnapshot After, Seq<Guid> Created = default);
public readonly record struct LayoutMoveDelta(Guid ObjectId, float Dx, float Dy, LayoutSnapshot After, Option<SnappingSnapshot> Snap);
public readonly record struct LayoutArrangeDelta(Seq<LayoutMoveDelta> Moves) {
    public int Count => Moves.Count;
}

public readonly record struct UndoEntry(VerbNoun Name, Seq<UndoAction> Actions) {
    internal ActionList AsList() => new([.. Actions]);
}

internal sealed class UndoGroup {
    private readonly List<UndoAction> actions = [];
    public VerbNoun Name { get; private set; }
    internal UndoGroup(string verb, string noun) => Name = (verb, noun);
    internal Unit Add(UndoAction action) {
        actions.Add(item: action);
        return unit;
    }
    // Nested labels concatenate via VerbNoun so History preserves grouped provenance.
    internal Unit Annotate(string verb, string noun) {
        Name += (verb, noun);
        return unit;
    }
    internal UndoEntry ToEntry() => new(Name: Name, Actions: toSeq(actions));
    internal Fin<Unit> Commit(GhDocument document) {
        UndoEntry entry = ToEntry();
        return UiRail.CommitActions(document: document, name: entry.Name, actions: entry.AsList());
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct GrasshopperUiPolicy(
    bool OpenEditor = false,
    bool RequireCanvas = false,
    bool RequireDocument = false,
    Option<RepaintRequest> Repaint = default) {
    public static GrasshopperUiPolicy Read => new(Repaint: Some(RepaintRequest.None));

    internal RepaintRequest RepaintOrNone => Repaint.IfNone(RepaintRequest.None);

    internal static GrasshopperUiPolicy Canvas(bool openEditor = false, RepaintRequest? repaint = null) =>
        new(OpenEditor: openEditor, RequireCanvas: true, Repaint: Some(repaint ?? RepaintRequest.None));
    internal static GrasshopperUiPolicy Document(RepaintRequest? repaint = null) =>
        new(RequireCanvas: true, RequireDocument: true, Repaint: Some(repaint ?? RepaintRequest.None));

    public static GrasshopperUiPolicy operator |(GrasshopperUiPolicy left, GrasshopperUiPolicy right) =>
        new(
            OpenEditor: left.OpenEditor || right.OpenEditor,
            RequireCanvas: left.RequireCanvas || right.RequireCanvas,
            RequireDocument: left.RequireDocument || right.RequireDocument,
            Repaint: left.RepaintOrNone | right.RepaintOrNone);
}

internal readonly record struct IntentOutcome<T>(T Value, RepaintRequest Repaint);

public sealed record GrasshopperUiIntent<T> {
    private readonly Func<GrasshopperUi.Scope, Fin<IntentOutcome<T>>> execute;

    internal GrasshopperUiIntent(Func<GrasshopperUi.Scope, Fin<T>> run, GrasshopperUiPolicy policy)
        : this(execute: scope => run(arg: scope).Map(value => new IntentOutcome<T>(Value: value, Repaint: policy.RepaintOrNone)), policy: policy) { }

    private GrasshopperUiIntent(Func<GrasshopperUi.Scope, Fin<IntentOutcome<T>>> execute, GrasshopperUiPolicy policy) {
        this.execute = execute;
        Policy = policy;
    }

    internal GrasshopperUiPolicy Policy { get; }
    internal Fin<T> Run(GrasshopperUi.Scope scope) => execute(arg: scope).Map(outcome => outcome.Value);
    internal Fin<IntentOutcome<T>> RunWithRepaint(GrasshopperUi.Scope scope) => execute(arg: scope);

    public GrasshopperUiIntent<TOut> Map<TOut>(Func<T, TOut> project) =>
        new(
            execute: scope => execute(arg: scope).Map(outcome => new IntentOutcome<TOut>(
                Value: project(arg: outcome.Value),
                Repaint: outcome.Repaint)),
            policy: Policy);

    public GrasshopperUiIntent<TOut> Bind<TOut>(Func<T, GrasshopperUiIntent<TOut>> bind) =>
        new(
            execute: scope =>
                from left in execute(arg: scope)
                from next in Optional(bind(arg: left.Value))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Bind)), detail: "bind returned null"))
                let merged = Policy | next.Policy
                from resolved in merged == Policy
                    ? Fin.Succ(value: scope)
                    : GrasshopperUi.Scope.Resolve(policy: merged, cancellation: scope.Cancellation, undo: scope.UndoGroup)
                from right in next.RunWithRepaint(scope: resolved)
                select new IntentOutcome<TOut>(Value: right.Value, Repaint: left.Repaint | right.Repaint),
            policy: Policy);

    public GrasshopperUiIntent<TOut> BindFin<TOut>(Func<T, Fin<TOut>> bind) =>
        new(
            execute: scope => execute(arg: scope).Bind(outcome => bind(arg: outcome.Value).Map(value =>
                new IntentOutcome<TOut>(Value: value, Repaint: outcome.Repaint))),
            policy: Policy);
}

public abstract record GhUiRequest<T> {
    internal abstract GrasshopperUiPolicy Policy { get; }
    internal abstract Fin<T> Apply(GrasshopperUi.Scope scope);
}

// One generic request collapses the per-file CanvasRequest/DocumentRequest/... `Run` wrappers:
// PolicyOf reads the op's policy, DispatchOf routes to the owning service's dispatch.
internal sealed record OpRequest<TOp, TResult>(TOp Op, Func<TOp, GrasshopperUiPolicy> PolicyOf, Func<GrasshopperUi.Scope, TOp, Fin<TResult>> DispatchOf) : GhUiRequest<TResult> {
    internal override GrasshopperUiPolicy Policy => PolicyOf(arg: Op);
    internal override Fin<TResult> Apply(GrasshopperUi.Scope scope) => DispatchOf(arg1: scope, arg2: Op);
}

// --- [ERRORS] -----------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public abstract partial record UiFault : Expected, IValidationError<UiFault> {
    private UiFault() : base() { }

    public sealed record MissingScopeCase(string Field) : UiFault;
    public sealed record InvalidInputCase(Op Op, string Detail) : UiFault;
    public sealed record MutationRejectedCase(Op Op, string Detail) : UiFault;
    public sealed record GhEditorCase(string Detail) : UiFault;
    public sealed record ThreadMarshalCase(string Detail) : UiFault;
    public sealed record CancelledCase(Op Op) : UiFault;

    public override string Message => Switch(
        missingScopeCase: static m => $"GrasshopperUi scope field '{m.Field}' required but absent.",
        invalidInputCase: static i => $"Op '{i.Op}' rejected input: {i.Detail}.",
        mutationRejectedCase: static r => $"Op '{r.Op}' rejected by Grasshopper2: {r.Detail}.",
        ghEditorCase: static e => $"Grasshopper editor operation failed: {e.Detail}.",
        threadMarshalCase: static t => $"UI-thread marshal failed: {t.Detail}.",
        cancelledCase: static c => $"Op '{c.Op}' cancelled.");

    public override string Category => Switch(
        missingScopeCase: static _ => "Scope",
        invalidInputCase: static _ => "Input",
        mutationRejectedCase: static _ => "Mutation",
        ghEditorCase: static _ => "Editor",
        threadMarshalCase: static _ => "Thread",
        cancelledCase: static _ => "Cancelled");

    public static UiFault MissingScope(string field) => new MissingScopeCase(Field: field);
    public static UiFault InvalidInput(Op op, string detail) => new InvalidInputCase(Op: op, Detail: detail);
    public static UiFault MutationRejected(Op op, string detail) => new MutationRejectedCase(Op: op, Detail: detail);
    public static UiFault GhEditor(string detail) => new GhEditorCase(Detail: detail);
    public static UiFault ThreadMarshal(string detail) => new ThreadMarshalCase(Detail: detail);
    public static UiFault Cancelled(Op op) => new CancelledCase(Op: op);

    // IValidationError<UiFault> — required by [ValidationError<UiFault>] on validated VOs. The
    // Op-bearing overload preserves call-site provenance (nameof(SpringConfig)/CornerRadii/ZoomFactor);
    // the unary form is kept for the interface contract and defaults Op to "Validation".
    public static UiFault Create(string message) => Create(op: Op.Of(name: "Validation"), message: message);
    public static UiFault Create(Op op, string message) => InvalidInput(op: op, detail: message);
}

public static partial class OpUiExtensions {
    [BoundaryAdapter, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fin<PointF> AcceptPoint(this Op op, PointF value, string detail = "non-finite point") =>
        float.IsFinite(value.X) && float.IsFinite(value.Y)
            ? Fin.Succ(value)
            : Fin.Fail<PointF>(error: UiFault.InvalidInput(op: op, detail: detail));

    [BoundaryAdapter, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fin<RectangleF> AcceptRect(this Op op, RectangleF value, string detail = "non-finite rect", bool requirePositive = false) =>
        float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Width) && float.IsFinite(value.Height)
        && (!requirePositive || (value.Width > 0f && value.Height > 0f))
            ? Fin.Succ(value)
            : Fin.Fail<RectangleF>(error: UiFault.InvalidInput(op: op, detail: detail));

    [BoundaryAdapter, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fin<float> AcceptFinite(this Op op, float value, string detail = "non-finite value", bool requirePositive = false, bool nonNegative = false, float min = float.NegativeInfinity) =>
        float.IsFinite(value)
        && (!requirePositive || value > 0f)
        && (!nonNegative || value >= 0f)
        && value >= min
            ? Fin.Succ(value)
            : Fin.Fail<float>(error: UiFault.InvalidInput(op: op, detail: detail));

    [BoundaryAdapter, MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Fin<T> AcceptFinite<T>(this Op op, T value, IMotionVector<T> vector, string detail) =>
        Optional(vector)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "vector is required"))
            .Bind(valid => valid.IsFinite(value)
                ? Fin.Succ(value)
                : Fin.Fail<T>(error: UiFault.InvalidInput(op: op, detail: detail)));

    // Single-delegate admission gate shared by every toolbar/menu/panel arm that requires a `Changed`
    // callback; the noun forms the per-arm detail without a hand-rolled Optional(...).ToFin at each site.
    [BoundaryAdapter]
    internal static Fin<TDelegate> NeedChanged<TDelegate>(this Op op, TDelegate? changed, string noun) where TDelegate : class =>
        Optional(changed).ToFin(Fail: UiFault.InvalidInput(op: op, detail: $"{noun} change delegate is required"));

    // BOUNDARY ADAPTER — native GH2/Eto/Rhino throws → UiFault.MutationRejected.
    [BoundaryAdapter]
    internal static Fin<T> Attempt<T>(this Op op, Func<T> body, string what = "") =>
        Optional(body)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null delegate"))
            .Bind(valid => Try.lift(f: valid).Run().MapFail(error => UiFault.MutationRejected(
                op: op,
                detail: string.IsNullOrEmpty(what)
                    ? $"native call threw: {error.Message}"
                    : $"{what} threw: {error.Message}")));

    [BoundaryAdapter]
    internal static Fin<Unit> Attempt(this Op op, System.Action body, string what = "") =>
        op.Attempt(body: () => { body(); return unit; }, what: what);

    // Parallel AcceptX checks folded into one Validation before ToFin.
    [BoundaryAdapter]
    internal static Validation<Seq<UiFault>, T> ValidateParallel<T>(this Op op, T value, params Func<Op, Fin<Unit>>[] checks) {
        Seq<UiFault> faults = toSeq(checks).Choose(check =>
            check(arg: op).Match(
                Succ: static _ => None,
                Fail: static error => Some((UiFault)error)));
        return faults.IsEmpty
            ? Success<Seq<UiFault>, T>(value)
            : Fail<Seq<UiFault>, T>(faults);
    }

    [BoundaryAdapter]
    internal static Fin<T> AcceptAll<T>(this Op op, T value, params Func<Op, Fin<Unit>>[] checks) =>
        op.ValidateParallel(value: value, checks: checks).ToFin();

    // Validation<Seq<UiFault>, T> → Fin<T>; preserves accumulated fault provenance.
    [BoundaryAdapter]
    internal static Fin<T> ToFin<T>(this Validation<Seq<UiFault>, T> validation) =>
        validation.Match(
            Succ: Fin.Succ,
            Fail: static faults => Fin.Fail<T>(error: faults.IsEmpty
                ? UiFault.InvalidInput(op: Op.Of(name: "Validation"), detail: "empty failure")
                : faults.Skip(1).Fold((Error)faults[0], static (acc, fault) => acc + fault)));
}

// --- [SERVICES] ---------------------------------------------------------------------------
public static class GhUi {
    public static GrasshopperUiIntent<T> Apply<T>(GhUiRequest<T> request) =>
        Optional(request).Match(
            Some: valid => new GrasshopperUiIntent<T>(
                run: scope => valid.Apply(scope: scope),
                policy: valid.Policy),
            None: () => new GrasshopperUiIntent<T>(
                run: _ => Fin.Fail<T>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Apply)), detail: "request is required")),
                policy: GrasshopperUiPolicy.Read));

    public static GrasshopperUiIntent<CanvasResult> Canvas(CanvasOp op) => Apply(request: new OpRequest<CanvasOp, CanvasResult>(Op: op, PolicyOf: static o => o.UiPolicy, DispatchOf: static (scope, o) => UiRail.CanvasDispatch(scope: scope, op: o)));
    public static GrasshopperUiIntent<DocumentResult> Document(DocumentOp op) => Apply(request: new OpRequest<DocumentOp, DocumentResult>(Op: op, PolicyOf: static o => o.UiPolicy, DispatchOf: static (scope, o) => UI.Document.Dispatch(scope: scope, op: o)));
    public static GrasshopperUiIntent<EditorResult> Editor(EditorOp op) => Apply(request: new OpRequest<EditorOp, EditorResult>(Op: op, PolicyOf: static o => o.UiPolicy, DispatchOf: static (_, o) => UI.Editor.Dispatch(op: o)));
    public static GrasshopperUiIntent<LayoutResult> Layout(LayoutOp op) => Apply(request: new OpRequest<LayoutOp, LayoutResult>(Op: op, PolicyOf: static o => o.UiPolicy, DispatchOf: static (scope, o) => UI.Layout.Dispatch(scope: scope, op: o)));
    public static GrasshopperUiIntent<WireResult> Wire(WireOp op) => Apply(request: new OpRequest<WireOp, WireResult>(Op: op, PolicyOf: static o => o.UiPolicy, DispatchOf: static (scope, o) => UI.Wire.Dispatch(op: o).Run(scope: scope)));
    public static GrasshopperUiIntent<T> Input<T>(InputRequest<T> request) => Apply(request: request);
    public static GrasshopperUiIntent<T> Paint<T>(PaintRequest<T> request) => Apply(request: request);
    public static GrasshopperUiIntent<T> Motion<T>(MotionRequest<T> request) => Apply(request: request);
    public static GrasshopperUiIntent<CanvasChromeResult> CanvasChrome(CanvasChromeOp op) => Apply(request: new OpRequest<CanvasChromeOp, CanvasChromeResult>(Op: op, PolicyOf: static o => o.UiPolicy, DispatchOf: static (scope, o) => UI.CanvasChrome.Dispatch(op: o).Run(scope: scope)));
    public static GrasshopperUiIntent<CanvasChromeResult> Tooltip(TooltipOp op) => CanvasChrome(CanvasChromeOp.Tooltip(op: op));
    public static GrasshopperUiIntent<CanvasChromeResult> FloatingButton(FloatingButtonOp op) => CanvasChrome(CanvasChromeOp.FloatingButton(op: op));
    public static GrasshopperUiIntent<CanvasChromeResult> Interaction(InteractionOp op) => CanvasChrome(CanvasChromeOp.Interaction(op: op));
    public static GrasshopperUiIntent<Subscription> Event(UiEvent uiEvent) => Apply(request: new OpRequest<UiEvent, Subscription>(Op: uiEvent, PolicyOf: static e => e.UiPolicy, DispatchOf: static (scope, e) => Events.Subscribe(uiEvent: e).Run(scope: scope)));

    public static GrasshopperUiIntent<T> Group<T>(string verb, string noun, GrasshopperUiIntent<T> body) =>
        Optional(body).Match(
            Some: valid => Document(
                repaint: valid.Policy.RepaintOrNone,
                run: scope => scope.UndoGroup.Match(
                    Some: bag => { _ = bag.Annotate(verb: verb, noun: noun); return valid.Run(scope: scope); },
                    None: () => {
                        UndoGroup bag = new(verb: verb, noun: noun);
                        GrasshopperUi.Scope grouped = scope with { UndoGroup = Some(bag) };
                        return from value in valid.Run(scope: grouped)
                               from document in scope.NeedDocument()
                               from committed in bag.Commit(document: document)
                               select value;
                    })),
            None: () => Document(
                run: _ => Fin.Fail<T>(
                    error: UiFault.InvalidInput(op: Op.Of(name: nameof(Group)), detail: "body is required"))));

    internal static GrasshopperUiIntent<T> Read<T>(Func<GrasshopperUi.Scope, Fin<T>> run) =>
        new(run: run, policy: GrasshopperUiPolicy.Read);

    internal static GrasshopperUiIntent<T> Canvas<T>(Func<GrasshopperUi.Scope, Fin<T>> run, bool openEditor = false, RepaintRequest? repaint = null) =>
        new(run: run, policy: GrasshopperUiPolicy.Canvas(openEditor: openEditor, repaint: repaint));

    internal static GrasshopperUiIntent<T> Document<T>(Func<GrasshopperUi.Scope, Fin<T>> run, RepaintRequest? repaint = null) =>
        new(run: run, policy: GrasshopperUiPolicy.Document(repaint: repaint));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasSkin(Skin Effective, Skin Lit, Skin Dim) {
    internal static Option<CanvasSkin> Of(GhCanvas? canvas) =>
        Optional(canvas).Map(static c => new CanvasSkin(Effective: c.Skin, Lit: c.SkinLit, Dim: c.SkinDim));
}

[BoundaryAdapter]
public sealed partial record GrasshopperUi {
    private static readonly Atom<Seq<Error>> HandlerFaultSink = Atom(value: Seq<Error>());

    internal static int HandlerFaultCount => HandlerFaultSink.Value.Count;

    internal static Seq<Error> HandlerFaults => HandlerFaultSink.Value;

    // Host-callback boundary: a faulted UI handler is swallowed because native callbacks have no return rail;
    // keep the fault observable for bridge/spec validation and diagnostics.
    internal static Fin<Unit> Handler(Func<Fin<Unit>> valid) =>
        Protect(valid: valid).Match(
            Succ: static _ => Fin.Succ(value: unit),
            Fail: static error => HandlerFaultSink.Swap(faults => faults.Add(value: error)) switch {
                _ => Fin.Succ(value: unit),
            });

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Scope(Option<GhEditor> Editor, Option<GhCanvas> Canvas, Option<GhDocument> Document, Option<GhDocumentMethods> Methods, Option<GhObjectList> Objects, Option<CanvasSkin> Skin, Option<UndoGroup> UndoGroup, CancellationToken Cancellation) {
        [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "AcquireEditor yields GH2's process-static Editor singleton (Editor.Instance); it is owned for the host lifetime and must never be disposed here — disposing would tear down the live editor and its canvas.")]
        internal static Fin<Scope> Resolve(GrasshopperUiPolicy policy, CancellationToken cancellation, Option<UndoGroup> undo = default) =>
            cancellation.IsCancellationRequested
                ? Fin.Fail<Scope>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Resolve))))
                : Fin.Succ(value: AcquireEditor(openEditor: policy.OpenEditor)).Map(editor => {
                    GhCanvas? canvas = editor?.Canvas;
                    GhDocument? document = editor?.Documents.Current ?? canvas?.Document;
                    return new Scope(
                        Editor: Optional(editor),
                        Canvas: Optional(canvas),
                        Document: Optional(document),
                        Methods: Optional(document?.Methods),
                        Objects: Optional(document?.Objects),
                        Skin: CanvasSkin.Of(canvas: canvas),
                        UndoGroup: undo,
                        Cancellation: cancellation);
                }).Bind(scope => (policy.RequireCanvas && scope.Canvas.IsNone, policy.RequireDocument && scope.Document.IsNone) switch {
                    (true, _) => Fin.Fail<Scope>(error: UiFault.MissingScope(field: nameof(Canvas))),
                    (_, true) => Fin.Fail<Scope>(error: UiFault.MissingScope(field: nameof(Document))),
                    _ => Fin.Succ(value: scope),
                });

        // Never force GhEditor.ShowEditor: it always Form.Show()s, which paints the StatusBar and SIGABRTs the host
        // under programmatic plugin load (see Editor.OpenEditor). When an op requires the editor, construct the
        // singleton HEADLESS — the ctor builds the Canvas, no window is realized; ownership is GH2's process-static
        // _instance (returned here, never disposed). Ops that want a visible window Show it in their dispatch body.
        private static GhEditor? AcquireEditor(bool openEditor) =>
            (GhEditor.Instance, openEditor) switch {
                (GhEditor current, _) => current,
                (null, true) => new GhEditor(),
                _ => null,
            };

        internal Fin<GhCanvas> NeedCanvas([CallerMemberName] string name = "") =>
            Canvas.ToFin(Fail: UiFault.MissingScope(field: nameof(Canvas)));
        internal Fin<GhDocument> NeedDocument([CallerMemberName] string name = "") =>
            Document.ToFin(Fail: UiFault.MissingScope(field: nameof(Document)));
        internal Fin<GhDocumentMethods> NeedMethods([CallerMemberName] string name = "") =>
            Methods.ToFin(Fail: UiFault.MissingScope(field: nameof(Methods)));
        internal Fin<GhObjectList> NeedObjects([CallerMemberName] string name = "") =>
            Objects.ToFin(Fail: UiFault.MissingScope(field: nameof(Objects)));
        internal Fin<Skin> NeedSkin([CallerMemberName] string name = "") =>
            Skin.Map(static s => s.Effective).ToFin(Fail: UiFault.MissingScope(field: nameof(Skin)));
        internal Fin<CanvasSkin> NeedCanvasSkin([CallerMemberName] string name = "") =>
            Skin.ToFin(Fail: UiFault.MissingScope(field: nameof(Skin)));
    }

    public Fin<T> Use<T>(GrasshopperUiIntent<T> intent, CancellationToken cancellation = default) =>
        from valid in Optional(intent).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Use)), detail: "null intent"))
        from result in OnUiThread(
            cancellation: cancellation,
            run: () =>
                from scope in Scope.Resolve(policy: valid.Policy, cancellation: cancellation)
                from outcome in valid.RunWithRepaint(scope: scope)
                select Repaint(
                    scope: scope,
                    policy: GrasshopperUiPolicy.Read with { Repaint = Some(outcome.Repaint) },
                    value: outcome.Value))
        select result;

    internal static GrasshopperUiIntent<Snapshot<TDelta>> Mutate<TDelta>(
        Op op,
        Func<Scope, Fin<TDelta>> mutate,
        UndoStrategy undo,
        RepaintRequest? repaint = null) =>
            GhUi.Document(
                repaint: repaint,
                run: scope =>
                    from delta in Optional(mutate).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null delegate")).Bind(m => m(arg: scope))
                    from _ in RecordUndo(scope: scope, op: op, undo: undo)
                    select Snapshot.Of(payload: delta, ownerId: scope.Document.Map(d => d.Hash)));

    // Cancellation is re-checked between recording the manual undo entry and History.Do, so a cancel
    // cannot leave the document mutated without a matching undo entry.
    private static Fin<Unit> RecordUndo(Scope scope, Op op, UndoStrategy undo) =>
        undo switch {
            UndoStrategy.AutoCase => Fin.Succ(value: unit),
            UndoStrategy.ManualCase manual =>
                from document in scope.NeedDocument()
                from entry in Try.lift(f: () => manual.Record(arg: scope))
                    .Run()
                    .MapFail(error => UiFault.MutationRejected(op: op, detail: $"manual undo recording threw: {error.Message}"))
                from _cancel in scope.Cancellation.IsCancellationRequested
                    ? Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: "cancelled before undo recording; document mutated without undo entry"))
                    : Fin.Succ(value: unit)
                from committed in Try.lift(f: () => {
                    _ = scope.UndoGroup
                        .Map(bag => entry.Actions.Iter(action => bag.Add(action: action)))
                        .IfNone(() => Optional(entry)
                            .Filter(static e => e.Actions.Count > 0)
                            .Map(e => { document.Undo.Do(name: e.Name, actions: e.AsList()); return unit; })
                            .IfNone(unit));
                    return unit;
                }).Run().MapFail(error => UiFault.MutationRejected(op: op, detail: $"History.Do threw: {error.Message}"))
                select committed,
            _ => Fin.Fail<Unit>(error: UiFault.MutationRejected(op: op, detail: "unknown undo strategy")),
        };

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run, CancellationToken cancellation = default) =>
        Optional(run)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(OnUiThread)), detail: "null delegate"))
            .Bind(valid => (RhinoApp.IsOnMainThread, cancellation.IsCancellationRequested) switch {
                (_, true) => Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(OnUiThread)))),
                (true, _) => Protect(valid: valid),
                (false, _) => Marshal(valid: valid, cancellation: cancellation),
            });

    private static Fin<T> Marshal<T>(Func<Fin<T>> valid, CancellationToken cancellation) =>
        Try.lift<Fin<T>>(f: () => cancellation.IsCancellationRequested
            ? Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Marshal))))
            : Application.Instance.Invoke(func: () => Protect(valid: valid)))
            .Run()
            .MapFail(error => UiFault.ThreadMarshal(detail: $"Application.Invoke threw: {error.Message}"))
            .Bind(static result => result);

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Try.lift<Fin<T>>(f: valid).Run().MapFail(error => UiFault.ThreadMarshal(detail: $"{name}: {error.Message}")).Bind(static result => result);

    internal static Fin<Unit> DetachOnUiThread(System.Action run) =>
        Optional(run).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DetachOnUiThread)), detail: "null delegate"))
            .Bind(valid => RhinoApp.IsOnMainThread
                ? Protect(valid: () => { valid(); return Fin.Succ(value: unit); })
                : Try.lift(f: () => {
                    Application.Instance.AsyncInvoke(action: () => Protect(valid: () => { valid(); return Fin.Succ(value: unit); }).Ignore());
                    return unit;
                }).Run().MapFail(error => UiFault.ThreadMarshal(detail: $"AsyncInvoke threw: {error.Message}")));

    internal static T Repaint<T>(Scope scope, GrasshopperUiPolicy policy, T value) {
        _ = policy.RepaintOrNone.ApplyTo(scope: scope);
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static Rectangle ControlSpace(GhCanvas canvas, RectangleF bounds) =>
        Rectangle.Ceiling(canvas.Map(rectangle: bounds, from: CoordinateSystem.Content, to: CoordinateSystem.Control));
}

// Lightweight Eto panel host for DrawPlan outside the GH canvas paint cycle.
internal sealed class FloatingForm : Form {
    internal FloatingForm(Option<Window> owner) {
        ShowInTaskbar = false;
        Resizable = false;
        WindowStyle = WindowStyle.Default;
        Owner = owner.IfNone(() => Application.Instance.MainForm);
    }
}
