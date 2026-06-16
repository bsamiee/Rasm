using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Eto.Forms;
using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Rhino;
using Rhino.UI;
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
// Declaration order is the apply order: Solution runs before Display so a paired pass never degrades to the weaker single side.
[SmartEnum<int>]
public sealed partial class RepaintBoundarySide {
    public static readonly RepaintBoundarySide Solution = new(key: 0, apply: static doc => { _ = doc.Solution.Start(mode: SolutionMode.Regular); return unit; });
    public static readonly RepaintBoundarySide Display = new(key: 1, apply: static doc => { doc.Display.UpdateDisplay(); return unit; });

    [UseDelegateFromConstructor]
    internal partial Unit Apply(GhDocument doc);
}

[SkipUnionOps]
[Union]
public partial record RepaintRequest {
    private RepaintRequest() { }
    public sealed record NoneCase : RepaintRequest;
    public sealed record ObjectCase(Guid Id) : RepaintRequest;
    public sealed record RegionCase(RectangleF Bounds) : RepaintRequest;
    public sealed record CanvasCase : RepaintRequest;
    public sealed record ScheduledCase(Option<TimeSpan> Delay = default) : RepaintRequest;
    public sealed record BoundaryCase(ImmutableHashSet<RepaintBoundarySide> Sides) : RepaintRequest;

    public static readonly RepaintRequest None = new NoneCase();
    public static readonly RepaintRequest Canvas = new CanvasCase();
    public static readonly RepaintRequest Scheduled = new ScheduledCase(Delay: Option<TimeSpan>.None);
    public static readonly RepaintRequest Solution = new BoundaryCase(Sides: [RepaintBoundarySide.Solution]);
    public static readonly RepaintRequest Display = new BoundaryCase(Sides: [RepaintBoundarySide.Display]);
    public static readonly RepaintRequest SolutionAndDisplay = new BoundaryCase(Sides: [RepaintBoundarySide.Solution, RepaintBoundarySide.Display]);
    public static RepaintRequest Object(Guid id) => new ObjectCase(Id: id);
    public static RepaintRequest Region(RectangleF bounds) => new RegionCase(Bounds: bounds);
    public static RepaintRequest Delayed(TimeSpan delay) => new ScheduledCase(Delay: Some(delay));

    // Absorption lattice (top absorbs all): Boundary (set-union) > Canvas > Scheduled > Region(union) / Object(same-id).
    // Arm order is load-bearing: boundary-set escalation precedes canvas/scheduled collapse so a paired pass never degrades to a single side.
    public static RepaintRequest operator |(RepaintRequest left, RepaintRequest right) =>
        (left, right) switch {
            (NoneCase, _) => right,
            (_, NoneCase) => left,
            (BoundaryCase l, BoundaryCase r) => new BoundaryCase(Sides: l.Sides.Union(r.Sides)),
            (BoundaryCase, _) or (_, BoundaryCase) => left is BoundaryCase ? left : right,
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

    // Folds a batch through the absorption lattice; the empty batch is None because None is operator|'s identity.
    public static RepaintRequest Batch(Seq<RepaintRequest> requests) =>
        requests.Fold(None, static (absorbed, next) => absorbed | next);

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
            scheduledCase: static (s, sc) => s.Canvas.IfSome(canvas => Redraw(canvas: canvas, delay: sc.Delay, cancellation: s.Cancellation)),
            // Fold the membership in declaration (rank) order, invoking each present pass's apply column once.
            boundaryCase: static (s, b) => s.Document.IfSome(doc =>
                toSeq(RepaintBoundarySide.Items).Filter(side => b.Sides.Contains(side)).Fold(unit, (_, side) => side.Apply(doc: doc))));

    // GH2 Canvas exposes only arg-less ScheduleRedraw() (Eto-coalesced); a positive delay has no native entry, so it is
    // deferred by a one-shot timer marshalling the arg-less redraw to the UI thread, its outcome folded into the fault sink.
    private static Unit Redraw(GhCanvas canvas, Option<TimeSpan> delay, CancellationToken cancellation) =>
        cancellation.IsCancellationRequested ? unit
        : delay.Filter(static span => span > TimeSpan.Zero) is { IsSome: true, Case: TimeSpan wait }
            ? Defer(canvas: canvas, wait: wait, cancellation: cancellation)
            : Op.Side(canvas.ScheduleRedraw);

    private static Unit Defer(GhCanvas canvas, TimeSpan wait, CancellationToken cancellation) =>
        Op.Side(() => _ = Task.Delay(delay: wait, timeProvider: TimeProvider.System, cancellationToken: cancellation).ContinueWith(
            _ => ignore(GrasshopperUi.Handler(valid: () => GrasshopperUi.OnUiThread(
                run: () => { canvas.ScheduleRedraw(); return Fin.Succ(value: unit); },
                cancellation: cancellation))),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnRanToCompletion,
            TaskScheduler.Default));
}

// Teardown lifecycle for one detacher: Always repeats the detach on every Dispose; Once latches it to fire exactly
// once; Gated fires once only while a minted token still owns the live slot (compare-and-clear lease).
[SkipUnionOps]
[Union]
public partial record Teardown {
    private Teardown() { }
    public sealed record AlwaysCase : Teardown;
    public sealed record OnceCase(Atom<bool> Fired) : Teardown;
    public sealed record GatedCase(Atom<Option<Guid>> Owner, Guid Token) : Teardown;

    public static readonly Teardown Always = new AlwaysCase();
    public static Teardown Once() => new OnceCase(Fired: Atom(value: false));
    // Gated mints a fresh token over the supplied ownership cell; the first attach commits the token, detach fires
    // only while the cell still holds it, and the winning detach clears the cell so a stale teardown cannot fire.
    public static Teardown Gated(Atom<Option<Guid>> owner) => new GatedCase(Owner: owner, Token: Guid.NewGuid());

    // First-writer-wins: the token installs only into an unowned cell, so a second racing attach never clobbers the
    // first. A lost commit means the caller must release itself.
    internal bool CommitWon() => Switch(
        alwaysCase: static _ => true,
        onceCase: static _ => true,
        gatedCase: static g => {
            bool won = false;
            _ = g.Owner.Swap(current => (won = current.IsNone, won ? Some(g.Token) : current).Item2);
            return won;
        });

    // CAS-atomic gate: Once flips false->true and only the flipping caller fires; Gated clears the cell only while its
    // token still owns it and only that owner fires. Swap captures the pre-swap snapshot to report the winner.
    internal bool ShouldFire() => Switch(
        alwaysCase: static _ => true,
        onceCase: static o => {
            bool already = true;
            _ = o.Fired.Swap(current => (already = current, true).Item2);
            return !already;
        },
        gatedCase: static g => {
            bool owned = false;
            _ = g.Owner.Swap(current => (owned = current is { IsSome: true, Case: Guid held } && held == g.Token, owned ? Option<Guid>.None : current).Item2);
            return owned;
        });
}

[SkipUnionOps]
[Union]
public partial record Subscription : IDisposable {
    private Subscription() { }
    public sealed record AtomCase(System.Action Detach, bool MarshalToUi, Teardown Teardown) : Subscription;
    public sealed record CompositeCase(Seq<Subscription> Members) : Subscription;
    public sealed record EmptyCase : Subscription;

    public static readonly Subscription Empty = new EmptyCase();
    public static Subscription Atom(System.Action detach, bool marshalToUi = false, bool detachOnce = false) =>
        new AtomCase(
            Detach: detach,
            MarshalToUi: marshalToUi,
            Teardown: detachOnce ? Teardown.Once() : Teardown.Always);

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

    // Iterative drain (named exemption): a pathologically nested composite would overflow on recursion, so children
    // flatten onto an explicit stack; each per-child throw is captured so one failure never orphans the rest.
    protected virtual void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }

        Stack<Subscription> pending = new();
        pending.Push(this);
        while (pending.Count > 0) {
            switch (pending.Pop()) {
                case AtomCase atom:
                    // Each per-child detach Fin folds into the observable fault sink so a failed detach is recorded rather than discarded.
                    _ = GrasshopperUi.ObserveDetach(
                        atom.Teardown.ShouldFire()
                            ? atom.MarshalToUi
                                ? GrasshopperUi.DetachOnUiThread(run: atom.Detach)
                                : GrasshopperUi.Protect(valid: () => { atom.Detach(); return Fin.Succ(value: unit); })
                            : Fin.Succ(value: unit));
                    break;
                case CompositeCase composite:
                    // Push reversed so pop yields members in their stored LIFO detach order; nested composites flatten onto the same stack.
                    foreach (Subscription member in composite.Members.Rev()) {
                        pending.Push(member);
                    }
                    break;
                case EmptyCase:
                    break;
            }
        }
    }

    [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "Composite owns the pacer atom; caller disposes the returned subscription.")]
    internal static Subscription PaintPacer(Subscription paintHook, System.Action pacerRelease) =>
        paintHook | Atom(detach: pacerRelease, detachOnce: true, marshalToUi: true);

    internal static Subscription DisposeOnce(Subscription inner) =>
        inner switch {
            EmptyCase => Empty,
            AtomCase { Teardown: not Teardown.AlwaysCase } => inner,
            _ => Atom(detach: inner.Dispose, detachOnce: true),
        };

    // Token-gated subscription: attach mints a token over the ownership cell and commits it after attach succeeds;
    // detach fires once only while the cell still holds the token, then clears it so a stale teardown cannot hide a live successor.
    internal static Fin<Subscription> TokenGated(Atom<Option<Guid>> owner, System.Action inner, System.Action detach, bool marshalToUi = true) =>
        Optional(owner)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(TokenGated)), detail: "ownership cell is required"))
            .Bind(cell => {
                Teardown gate = Teardown.Gated(owner: cell);
                // A lost commit means a concurrent attach already owns the slot, so this acquisition releases itself immediately.
                return Bind(
                    attach: () => { inner(); if (!gate.CommitWon()) { detach(); } },
                    detach: detach,
                    marshalToUi: marshalToUi,
                    teardown: gate);
            });

    internal static Fin<Subscription> Bind(
        System.Action attach,
        System.Action detach,
        bool marshalToUi = false,
        bool detachOnce = false,
        Teardown? teardown = null) =>
        Try.lift(f: () => { attach(); return unit; }).Run()
            .Map(_ => (Subscription)new AtomCase(
                Detach: detach,
                MarshalToUi: marshalToUi,
                Teardown: teardown ?? (detachOnce ? Teardown.Once() : Teardown.Always)))
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

// Cross-intent undo accumulator: one boundary cell threads the grouped label and the immutable list of contributed
// action segments across the nested intent tree; Commit folds every segment into one fresh ActionList at the document boundary.
public sealed class UndoGroup {
    private readonly Atom<(VerbNoun Name, Seq<ActionList> Segments)> state;
    internal UndoGroup(string verb, string noun) => state = Atom(value: ((VerbNoun)(verb, noun), Seq<ActionList>()));
    public VerbNoun Name => state.Value.Name;
    internal Unit Add(ActionList list) =>
        ignore(state.Swap(current => current with { Segments = current.Segments.Add(list) }));
    // Nested labels concatenate via VerbNoun so History preserves grouped provenance.
    internal Unit Annotate(string verb, string noun) =>
        ignore(state.Swap(current => current with { Name = current.Name + (verb, noun) }));
    internal UndoEntry ToEntry() {
        (VerbNoun name, Seq<ActionList> segments) = state.Value;
        ActionList merged = segments.Fold(ActionList.Empty, static (acc, segment) => acc + segment);
        return new UndoEntry(Name: name, Actions: merged);
    }
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

[StructLayout(LayoutKind.Auto)]
public readonly record struct CanvasSkin(Skin Effective, Skin Lit, Skin Dim) {
    internal static Option<CanvasSkin> Of(GhCanvas? canvas) =>
        Optional(canvas).Map(static c => new CanvasSkin(Effective: c.Skin, Lit: c.SkinLit, Dim: c.SkinDim));
}

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

    internal static readonly Op Validation = Op.Of(name: nameof(Validation));

    public static UiFault Create(string message) => Create(op: Validation, message: message);
    public static UiFault Create(Op op, string message) => InvalidInput(op: op, detail: message);
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

[BoundaryAdapter]
public sealed partial record GrasshopperUi {
    private static readonly Atom<Seq<Error>> HandlerFaultSink = Atom(value: Seq<Error>());

    public static int HandlerFaultCount => HandlerFaultSink.Value.Count;

    public static Seq<Error> HandlerFaults => HandlerFaultSink.Value;

    public static Unit ClearHandlerFaults() {
        _ = HandlerFaultSink.Swap(static _ => Seq<Error>());
        return unit;
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

    // Host callbacks have no return rail; keep swallowed handler faults observable.
    internal static Fin<Unit> Handler(Func<Fin<Unit>> valid) =>
        ObserveDetach(Protect(valid: valid));

    // Folds an already-captured boundary Fin into the observable fault sink: a Succ passes through, a Fail appends to
    // HandlerFaultSink and resolves to Succ so a non-aborting sweep continues with the failure recorded.
    internal static Fin<Unit> ObserveDetach(Fin<Unit> outcome) =>
        outcome.Match(
            Succ: static _ => Fin.Succ(value: unit),
            Fail: static error => HandlerFaultSink.Swap(faults => faults.Add(value: error)) switch {
                _ => Fin.Succ(value: unit),
            });

    [StructLayout(LayoutKind.Auto)]
    public readonly record struct Scope(Option<GhEditor> Editor, Option<GhCanvas> Canvas, Option<GhDocument> Document, Option<GhDocumentMethods> Methods, Option<GhObjectList> Objects, Option<CanvasSkin> Skin, Option<UndoGroup> UndoGroup, CancellationToken Cancellation) {
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

        // ShowEditor always Form.Show()s and can SIGABRT during programmatic plugin load; headless singleton construction builds Canvas without realizing a window.
        // GH2 owns the process-static _instance; visible ops Show the window inside their dispatch body.
        private static GhEditor? AcquireEditor(bool openEditor) =>
            (GhEditor.Instance, openEditor) switch {
                (GhEditor current, _) => current,
                (null, true) => new GhEditor(),
                _ => null,
            };
    }

    internal static Fin<T> OnUiThread<T>(Func<Fin<T>> run, CancellationToken cancellation = default) =>
        Optional(run)
            .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(OnUiThread)), detail: "null delegate"))
            .Bind(valid => (RhinoApp.IsOnMainThread, cancellation.IsCancellationRequested) switch {
                (_, true) => Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(OnUiThread)))),
                (true, _) => Protect(valid: valid),
                (false, _) => Marshal(valid: valid, cancellation: cancellation),
            });

    // The cancellation token is observed twice — on the posting thread before enqueue and inside the posted continuation
    // at dispatch instant — so a callback enqueued against a target torn down between enqueue and dequeue short-circuits to Cancelled.
    private static Fin<T> Marshal<T>(Func<Fin<T>> valid, CancellationToken cancellation) =>
        from app in NeedApplication(op: Op.Of(name: nameof(Marshal)))
        from result in Try.lift<Fin<T>>(f: () => cancellation.IsCancellationRequested
                ? Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Marshal))))
                : app.Invoke(func: () => cancellation.IsCancellationRequested
                    ? Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Marshal))))
                    : Protect(valid: valid)))
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
                      from result in Try.lift<Fin<Unit>>(f: () => app.Invoke(func: () => Protect(valid: () => { valid(); return Fin.Succ(value: unit); })))
                          .Run()
                          .MapFail(error => UiFault.ThreadMarshal(detail: $"Application.Invoke threw: {error.Message}"))
                          .Bind(static result => result)
                      select result);

    internal static Fin<Application> NeedApplication(Op op) =>
        Optional(Application.Instance).ToFin(Fail: UiFault.ThreadMarshal(detail: $"{op}: Eto Application.Instance is not initialized"));

    // Post-commit repaint: the host paint/solution primitives fold their failure into the observable fault sink, so a
    // repaint-side-effect throw never retroactively fails the already-resolved intent value, which returns unconditionally.
    internal static T Repaint<T>(Scope scope, GrasshopperUiPolicy policy, T value) {
        _ = ObserveDetach(Protect(valid: () => policy.RepaintOrNone.ApplyTo(scope: scope) switch { _ => Fin.Succ(value: unit) }));
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
        _ = owner.Iter(window => Owner = window);
        this.UseRhinoStyle();
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class UiDocumentIdentity {
    private sealed class IdBox {
        internal readonly Guid Value = Guid.NewGuid();
    }

    private static readonly ConditionalWeakTable<GhDocument, IdBox> Ids = [];

    internal static Guid Of(GhDocument? document) =>
        document is null ? Guid.Empty : Ids.GetValue(document, static _ => new IdBox()).Value;
}

// Canonical bounded-recency cache for UI state. Value factories run outside the lock; evict owns discarded values.
// Capacity is a working-set bound per consumer, sized to the live recency window the producer can realistically hold:
//   fonts 128       — distinct (family, size, style) faces on screen at once across all rendered glyph runs.
//   brushes 256     — distinct fill sources (solid, gradient, texture) live across simultaneously painted objects.
//   textMeasure 1024 — distinct (text, font) measured layouts per paint pass; the largest hot churn surface.
//   dashStyle 4096  — distinct (dash-array, width-bucket) pen styles; wide because bucketing keeps near-dupes apart.
//   wireRoute 256   — distinct routed wire paths held between solves on a busy canvas.
//   wireDrawn 8     — recently drawn wire-geometry sets; a tiny per-frame window, only the latest few matter.
//   wireIndex 8     — recently resolved (source, target) endpoint index sets; same tiny per-frame window as wireDrawn.
internal sealed class BoundedCache<TKey, TValue> where TKey : notnull {
    private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> entries;
    private readonly LinkedList<KeyValuePair<TKey, TValue>> order = new();
    private readonly Lock gate = new();
    private readonly int capacity;
    private readonly Action<TValue>? evict;

    internal BoundedCache(int capacity, IEqualityComparer<TKey>? comparer = null, Action<TValue>? evict = null) {
        this.capacity = capacity;
        this.evict = evict;
        entries = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(capacity: capacity, comparer: comparer);
    }

    internal Option<TValue> Find(TKey key) {
        using (gate.EnterScope()) {
            return Touch(key: key, value: out TValue? cached)
                ? Some(cached)
                : Option<TValue>.None;
        }
    }

    internal TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory) {
        using (gate.EnterScope()) {
            if (Touch(key: key, value: out TValue? cached)) {
                return cached;
            }
        }

        TValue fresh = valueFactory(arg: key);
        using (gate.EnterScope()) {
            if (Touch(key: key, value: out TValue? raced)) {
                evict?.Invoke(fresh);
                return raced;
            }

            if (entries.Count >= capacity && order.First is LinkedListNode<KeyValuePair<TKey, TValue>> lru) {
                order.RemoveFirst();
                _ = entries.Remove(key: lru.Value.Key);
                evict?.Invoke(lru.Value.Value);
            }

            LinkedListNode<KeyValuePair<TKey, TValue>> node = new(value: new KeyValuePair<TKey, TValue>(key: key, value: fresh));
            order.AddLast(node: node);
            entries.Add(key: key, value: node);
            return fresh;
        }
    }

    internal Unit Record(TKey key, TValue value) {
        using (gate.EnterScope()) {
            if (entries.TryGetValue(key: key, value: out LinkedListNode<KeyValuePair<TKey, TValue>>? existing)) {
                order.Remove(node: existing);
                evict?.Invoke(existing.Value.Value);
                existing.Value = new KeyValuePair<TKey, TValue>(key: key, value: value);
                order.AddLast(node: existing);
                return unit;
            }

            if (entries.Count >= capacity && order.First is LinkedListNode<KeyValuePair<TKey, TValue>> lru) {
                order.RemoveFirst();
                _ = entries.Remove(key: lru.Value.Key);
                evict?.Invoke(lru.Value.Value);
            }

            LinkedListNode<KeyValuePair<TKey, TValue>> node = new(value: new KeyValuePair<TKey, TValue>(key: key, value: value));
            order.AddLast(node: node);
            entries.Add(key: key, value: node);
            return unit;
        }
    }

    internal Unit Invalidate(TKey key) {
        using (gate.EnterScope()) {
            if (entries.Remove(key: key, value: out LinkedListNode<KeyValuePair<TKey, TValue>>? node)) {
                order.Remove(node: node);
                evict?.Invoke(node.Value.Value);
            }
        }
        return unit;
    }

    private bool Touch(TKey key, [MaybeNullWhen(returnValue: false)] out TValue value) {
        if (entries.TryGetValue(key: key, value: out LinkedListNode<KeyValuePair<TKey, TValue>>? hit)) {
            order.Remove(node: hit);
            order.AddLast(node: hit);
            value = hit.Value.Value;
            return true;
        }

        value = default;
        return false;
    }
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
                ? UiFault.InvalidInput(op: UiFault.Validation, detail: "empty failure")
                : faults.Skip(1).Fold((Error)faults[0], static (acc, fault) => acc + fault)));
}
