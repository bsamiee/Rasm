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
    public sealed record ScheduledCase(Option<TimeSpan> Delay = default) : RepaintRequest;
    public sealed record SolutionCase : RepaintRequest;
    public sealed record DisplayCase : RepaintRequest;
    public sealed record SolutionAndDisplayCase : RepaintRequest;

    internal Unit ApplyTo(GrasshopperUi.Scope scope) =>
        Switch(state: scope,
            noneCase: static (_, _) => unit,
            objectCase: static (s, o) => s.Canvas.IfSome(canvas => s.Objects.Match(
                Some: objects => Optional(objects.Find(instanceId: o.Id))
                    .Map(target => { canvas.Invalidate(rect: GrasshopperUi.ControlSpace(canvas: canvas, bounds: target.Attributes.AggregateBounds)); return unit; })
                    .IfNone(() => { canvas.Invalidate(); return unit; }),
                None: () => { canvas.Invalidate(); return unit; })),
            regionCase: static (s, r) => s.Canvas.IfSome(canvas => canvas.Invalidate(rect: GrasshopperUi.ControlSpace(canvas: canvas, bounds: r.Bounds))),
            canvasCase: static (s, _) => s.Canvas.IfSome(static canvas => canvas.Invalidate()),
            scheduledCase: static (s, sc) => s.Canvas.IfSome(canvas => Redraw(canvas: canvas, delay: sc.Delay)),
            solutionCase: static (s, op) => s.Document.IfSome(static doc => { _ = doc.Solution.Start(mode: SolutionMode.Regular); return unit; }),
            displayCase: static (s, _) => s.Document.IfSome(static doc => { doc.Display.UpdateDisplay(); return unit; }),
            solutionAndDisplayCase: static (s, op) => s.Document.IfSome(static doc => {
                _ = doc.Solution.Start(mode: SolutionMode.Regular);
                doc.Display.UpdateDisplay();
                return unit;
            }));

    public static readonly RepaintRequest None = new NoneCase();
    public static readonly RepaintRequest Canvas = new CanvasCase();
    public static readonly RepaintRequest Scheduled = new ScheduledCase(Delay: Option<TimeSpan>.None);
    public static readonly RepaintRequest Solution = new SolutionCase();
    public static readonly RepaintRequest Display = new DisplayCase();
    public static readonly RepaintRequest SolutionAndDisplay = new SolutionAndDisplayCase();
    public static RepaintRequest Object(Guid id) => new ObjectCase(Id: id);
    public static RepaintRequest Region(RectangleF bounds) => new RegionCase(Bounds: bounds);
    public static RepaintRequest Delayed(TimeSpan delay) => new ScheduledCase(Delay: Some(delay));

    // Repaint absorption collapses intents before the single ApplyTo at GrasshopperUi.Use exit.
    public static RepaintRequest operator |(RepaintRequest left, RepaintRequest right) =>
        (left, right) switch {
            (NoneCase, _) => right,
            (_, NoneCase) => left,
            (SolutionAndDisplayCase, _) or (_, SolutionAndDisplayCase) => SolutionAndDisplay,
            (SolutionCase, DisplayCase) or (DisplayCase, SolutionCase) => SolutionAndDisplay,
            (SolutionCase, _) or (_, SolutionCase) => Solution,
            (DisplayCase, _) or (_, DisplayCase) => Display,
            (CanvasCase, _) or (_, CanvasCase) => Canvas,
            (ScheduledCase l, ScheduledCase r) => new ScheduledCase(
                Delay: (l.Delay, r.Delay) switch {
                    ( { IsSome: true, Case: TimeSpan ld }, { IsSome: true, Case: TimeSpan rd }) => Some(ld < rd ? ld : rd),
                    _ => l.Delay | r.Delay,
                }),
            (ScheduledCase, _) or (_, ScheduledCase) => Scheduled,
            (RegionCase l, RegionCase r) => Region(bounds: RectangleF.Union(l.Bounds, r.Bounds)),
            (ObjectCase l, ObjectCase r) when l.Id == r.Id => left,
            _ => Canvas,
        };

    private static Unit Redraw(GhCanvas canvas, Option<TimeSpan> delay) =>
        delay is { IsSome: true, Case: TimeSpan wait } && wait > TimeSpan.Zero
            ? Defer(canvas: canvas, wait: wait)
            : Op.Side(canvas.ScheduleRedraw);

    // BOUNDARY ADAPTER — GH2 exposes no delayed-redraw entry; a one-shot timer defers the arg-less ScheduleRedraw and
    // marshals it back to the UI thread (mirrors SolutionControl.Start's discarded fire-and-forget continuation).
    private static Unit Defer(GhCanvas canvas, TimeSpan wait) {
        _ = Task.Delay(delay: wait, timeProvider: TimeProvider.System, cancellationToken: CancellationToken.None).ContinueWith(
            _ => GrasshopperUi.OnUiThread(run: () => { canvas.ScheduleRedraw(); return Fin.Succ(value: unit); }, cancellation: CancellationToken.None).Ignore(),
            cancellationToken: CancellationToken.None,
            continuationOptions: TaskContinuationOptions.ExecuteSynchronously,
            scheduler: TaskScheduler.Default);
        return unit;
    }
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

    internal static System.Action GuardDetach(System.Action detach, bool detachOnce) =>
        detachOnce switch {
            false => detach,
            true => Latch(detach: detach),
        };

    // Atom.Swap is CAS-atomic: the first caller observes the prior false and fires detach, every later caller
    // observes true and no-ops — replaces the Interlocked.Exchange(ref gate) mutable-int gate.
    private static System.Action Latch(System.Action detach) {
        Atom<bool> fired = Prelude.Atom(value: false);
        return () => {
            bool already = true;
            _ = fired.Swap(current => (already = current, true).Item2);
            _ = already ? unit : Op.Side(detach);
        };
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
[StructLayout(LayoutKind.Auto)]
public readonly record struct DocumentMutationDelta(int Changed, DocumentSnapshot After, Seq<Guid> Created = default);
[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutMoveDelta(Guid ObjectId, float Dx, float Dy, LayoutSnapshot After, Option<SnappingSnapshot> Snap);
[StructLayout(LayoutKind.Auto)]
public readonly record struct LayoutArrangeDelta(Seq<LayoutMoveDelta> Moves) {
    public int Count => Moves.Count;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UndoEntry(VerbNoun Name, ActionList Actions);

internal sealed class UndoGroup {
    private readonly ActionList actions = ActionList.Empty;
    public VerbNoun Name { get; private set; }
    internal UndoGroup(string verb, string noun) => Name = (verb, noun);
    internal Unit Add(ActionList list) {
        actions.Add(list);
        return unit;
    }
    // Nested labels concatenate via VerbNoun so History preserves grouped provenance.
    internal Unit Annotate(string verb, string noun) {
        Name += (verb, noun);
        return unit;
    }
    internal UndoEntry ToEntry() => new(Name: Name, Actions: actions);
    internal Fin<Unit> Commit(GhDocument document) {
        UndoEntry entry = ToEntry();
        return UiRail.CommitActions(document: document, name: entry.Name, actions: entry.Actions);
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

    internal GrasshopperUiIntent(IUiOp<T> op)
        : this(
            execute: scope => Optional(op)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(GrasshopperUiIntent<>)), detail: "operation is required"))
                .Bind(valid => valid.Intent().RunWithRepaint(scope: scope)),
            policy: Optional(op).Map(static valid => valid.Intent().Policy).IfNone(GrasshopperUiPolicy.Read)) { }

    private GrasshopperUiIntent(Func<GrasshopperUi.Scope, Fin<IntentOutcome<T>>> execute, GrasshopperUiPolicy policy) {
        this.execute = execute;
        Policy = policy;
    }

    internal GrasshopperUiPolicy Policy { get; }
    internal Fin<T> Run(GrasshopperUi.Scope scope) => RunWithRepaint(scope: scope).Map(static outcome => outcome.Value);
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

    public GrasshopperUiIntent<TOut> BindFin<TOut>(Func<T, Fin<TOut>> bind, RepaintRequest? repaint = null) =>
        new(
            execute: scope => execute(arg: scope).Bind(outcome => bind(arg: outcome.Value).Map(value =>
                    new IntentOutcome<TOut>(
                        Value: value,
                        Repaint: Optional(repaint).Map(extra => outcome.Repaint | extra).IfNone(outcome.Repaint)))),
            policy: Policy);
}

// --- [ERRORS] -----------------------------------------------------------------------------
[SkipUnionOps]
[Union]
public abstract partial record UiFault : Expected, IValidationError<UiFault> {
    private UiFault() : base() { }

    public sealed record MissingScopeCase(string Field) : UiFault;
    public sealed record InvalidInputCase(Op Op, string Detail) : UiFault;
    public sealed record MutationRejectedCase(Op Op, string Detail) : UiFault;
    public sealed record GhEditorCase(Op Op, string Detail) : UiFault;
    public sealed record ThreadMarshalCase(string Detail) : UiFault;
    public sealed record CancelledCase(Op Op) : UiFault;

    public override string Message => Switch(
        missingScopeCase: static m => $"GrasshopperUi scope field '{m.Field}' required but absent.",
        invalidInputCase: static i => $"Op '{i.Op}' rejected input: {i.Detail}.",
        mutationRejectedCase: static r => $"Op '{r.Op}' rejected by Grasshopper2: {r.Detail}.",
        ghEditorCase: static e => $"Op '{e.Op}' editor operation failed: {e.Detail}.",
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
    public static UiFault GhEditor(Op op, string detail) => new GhEditorCase(Op: op, Detail: detail);
    public static UiFault ThreadMarshal(string detail) => new ThreadMarshalCase(Detail: detail);
    public static UiFault Cancelled(Op op) => new CancelledCase(Op: op);

    // IValidationError<UiFault> contract; Op-bearing overload preserves VO call-site provenance.
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

    // Shared Changed-callback gate keeps toolbar/menu/panel diagnostics aligned.
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
internal interface IUiOp<TResult> {
    public GrasshopperUiIntent<TResult> Intent();
}

public static class GhUi {
    public static GrasshopperUiIntent<CanvasResult> Canvas(CanvasOp op) => new(op: op);
    public static GrasshopperUiIntent<DocumentResult> Document(DocumentOp op) => new(op: op);
    public static GrasshopperUiIntent<EditorResult> Editor(EditorOp op) => new(op: op);
    public static GrasshopperUiIntent<LayoutResult> Layout(LayoutOp op) => new(op: op);
    public static GrasshopperUiIntent<WireResult> Wire(WireOp op) => new(op: op);
    public static GrasshopperUiIntent<CanvasChromeResult> CanvasChrome(CanvasChromeOp op) => new(op: op);
    public static GrasshopperUiIntent<Subscription> Event(UiEvent uiEvent) => new(op: uiEvent);

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

    // Fans N unit intents into one undo entry via Bind-fold over the single-body Group; empty folds to a no-op.
    public static GrasshopperUiIntent<Unit> Group(string verb, string noun, Seq<GrasshopperUiIntent<Unit>> body) =>
        Group(
            verb: verb,
            noun: noun,
            body: body.IsEmpty
                ? Read(run: static _ => Fin.Succ(value: unit))
                : body.Tail.Fold(
                    initialState: body.Head.IfNone(Read(run: static _ => Fin.Succ(value: unit))),
                    f: static (acc, next) => acc.Bind(_ => next)));

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

    public static int HandlerFaultCount => HandlerFaultSink.Value.Count;

    public static Seq<Error> HandlerFaults => HandlerFaultSink.Value;

    public static Unit ClearHandlerFaults() {
        _ = HandlerFaultSink.Swap(static _ => Seq<Error>());
        return unit;
    }

    // Host callbacks have no return rail; keep swallowed handler faults observable.
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
                }).Bind(scope => {
                    Validation<Seq<UiFault>, Unit> reqCanvas =
                        policy.RequireCanvas && scope.Canvas.IsNone
                            ? Fail<Seq<UiFault>, Unit>(Seq(UiFault.MissingScope(field: nameof(Canvas))))
                            : Success<Seq<UiFault>, Unit>(unit);
                    Validation<Seq<UiFault>, Unit> reqDocument =
                        policy.RequireDocument && scope.Document.IsNone
                            ? Fail<Seq<UiFault>, Unit>(Seq(UiFault.MissingScope(field: nameof(Document))))
                            : Success<Seq<UiFault>, Unit>(unit);
                    return (reqCanvas & reqDocument)
                        .ToFin()
                        .Map(_ => scope);
                });

        // ShowEditor always Form.Show()s and can SIGABRT during programmatic plugin load; headless singleton construction builds Canvas without realizing a window.
        // GH2 owns the process-static _instance; visible ops Show the window inside their dispatch body.
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

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run, CancellationToken cancellation = default) =>
        Optional(run)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(OnUiThread)), detail: "null delegate"))
            .Bind(valid => (RhinoApp.IsOnMainThread, cancellation.IsCancellationRequested) switch {
                (_, true) => Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(OnUiThread)))),
                (true, _) => Protect(valid: valid),
                (false, _) => Marshal(valid: valid, cancellation: cancellation),
            });

    private static Fin<T> Marshal<T>(Func<Fin<T>> valid, CancellationToken cancellation) =>
        from app in NeedApplication(op: Op.Of(name: nameof(Marshal)))
        from result in Try.lift<Fin<T>>(f: () => cancellation.IsCancellationRequested
                ? Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Marshal))))
                : app.Invoke(func: () => Protect(valid: valid)))
            .Run()
            .MapFail(error => UiFault.ThreadMarshal(detail: $"Application.Invoke threw: {error.Message}"))
            .Bind(static result => result)
        select result;

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Try.lift<Fin<T>>(f: valid).Run().MapFail(error => UiFault.ThreadMarshal(detail: $"{name}: {error.Message}")).Bind(static result => result);

    internal static Fin<Unit> DetachOnUiThread(System.Action run) =>
        Optional(run).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(DetachOnUiThread)), detail: "null delegate"))
            .Bind(valid =>
                RhinoApp.IsOnMainThread
                    ? Protect(valid: () => { valid(); return Fin.Succ(value: unit); })
                    : from app in NeedApplication(op: Op.Of(name: nameof(DetachOnUiThread)))
                      from result in Try.lift(f: () => {
                          app.AsyncInvoke(action: () => Handler(valid: () => Protect(valid: () => { valid(); return Fin.Succ(value: unit); })));
                          return unit;
                      }).Run().MapFail(error => UiFault.ThreadMarshal(detail: $"AsyncInvoke threw: {error.Message}"))
                      select result);

    internal static Fin<Application> NeedApplication(Op op) =>
        Optional(Application.Instance).ToFin(Fail: UiFault.ThreadMarshal(detail: $"{op}: Eto Application.Instance is not initialized"));

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
    internal FloatingForm(Application app, Option<Window> owner) {
        ShowInTaskbar = false;
        Resizable = false;
        WindowStyle = WindowStyle.Default;
        Owner = owner.IfNone(() => app.MainForm);
    }
}
