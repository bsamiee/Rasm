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
    public static RepaintRequest BitwiseOr(RepaintRequest left, RepaintRequest right) => left | right;
}

// Auto: caller commits via UiRail.CommitActions OR built-in produces no actions (Select/DeselectWire).
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

[SkipUnionOps]
[Union]
public partial record Subscription : IDisposable {
    private Subscription() { }
    public sealed record AtomCase(System.Action Detach, bool MarshalToUi) : Subscription;
    public sealed record CompositeCase(Seq<Subscription> Members) : Subscription;
    public sealed record EmptyCase : Subscription;

    public static readonly Subscription Empty = new EmptyCase();
    public static Subscription Atom(System.Action detach, bool marshalToUi = false) => new AtomCase(Detach: detach, MarshalToUi: marshalToUi);
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

    protected virtual void Dispose(bool disposing) {
        if (!disposing) {
            return;
        }
        _ = Switch(
            atomCase: static a => a.MarshalToUi
                ? GrasshopperUi.DetachOnUiThread(run: a.Detach)
                : GrasshopperUi.Protect(valid: () => { a.Detach(); return Fin.Succ(value: unit); }),
            compositeCase: static c => { _ = c.Members.Iter(static s => s.Dispose()); return Fin.Succ(value: unit); },
            emptyCase: static _ => Fin.Succ(value: unit));
    }

    internal static Fin<Subscription> Bind(System.Action attach, System.Action detach, bool marshalToUi = false) =>
        Try.lift(f: () => { attach(); return unit; }).Run()
            .Map(_ => (Subscription)new AtomCase(Detach: detach, MarshalToUi: marshalToUi))
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
    public static DocumentMutationReceipt CreatedFrom(Option<Guid> id) =>
        id.Map(CreatedObject).IfNone(None);
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

// Public bag contract exposes only the appender; Annotate and Commit are implementation details on UndoGroup.
public interface IUndoBag {
    public Unit Add(UndoAction action);
}

public readonly record struct UndoEntry(VerbNoun Name, Seq<UndoAction> Actions) {
    internal ActionList AsList() => new([.. Actions]);
}

internal sealed class UndoGroup : IUndoBag {
    private readonly List<UndoAction> actions = [];
    public VerbNoun Name { get; private set; }
    internal UndoGroup(string verb, string noun) => Name = (verb, noun);
    public Unit Add(UndoAction action) {
        actions.Add(item: action);
        return unit;
    }
    // Append a nested label via VerbNoun's native concatenation; the History panel sees nested provenance preserved in the single combined record.
    internal Unit Annotate(string verb, string noun) {
        Name += (verb, noun);
        return unit;
    }
    internal UndoEntry ToEntry() => new(Name: Name, Actions: toSeq(actions));
    internal Fin<Unit> Commit(GhDocument document) =>
        Try.lift(f: () => {
            UndoEntry entry = ToEntry();
            _ = Optional(entry)
                .Filter(static e => e.Actions.Count > 0)
                .IfSome(e => document.Undo.Do(name: e.Name, actions: e.AsList()));
            return unit;
        }).Run().MapFail(error => UiFault.MutationRejected(op: Op.Of(name: nameof(Commit)), detail: $"History.Do threw: {error.Message}"));
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
    public static GrasshopperUiPolicy BitwiseOr(GrasshopperUiPolicy left, GrasshopperUiPolicy right) => left | right;
}

internal static partial class GhUiPolicy {
    internal static GrasshopperUiPolicy ForCanvas(CanvasOp op) =>
        op switch {
            CanvasOp.SnapshotCase s => GrasshopperUiPolicy.Canvas(openEditor: s.OpenEditor),
            CanvasOp.InvalidateCase i => GrasshopperUiPolicy.Canvas(repaint: i.Repaint),
            CanvasOp.InstantiateCase => GrasshopperUiPolicy.Canvas(openEditor: true),
            CanvasOp.ViewCase or CanvasOp.SnapFeedbackCase => GrasshopperUiPolicy.Canvas(openEditor: true, repaint: RepaintRequest.Canvas),
            CanvasOp.InteractionCase i => i.Policy.Projection.IsSome
                ? GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas)
                : GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Canvas),
            CanvasOp.WindowSelectCase => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
            _ => GrasshopperUiPolicy.Canvas(),
        };

    internal static GrasshopperUiPolicy ForDocument(DocumentOp op) =>
        op switch {
            DocumentOp.QueryCase { Request: DocumentQuery.UniverseCase } => GrasshopperUiPolicy.Read,
            DocumentOp.MutateCase mutate => GrasshopperUiPolicy.Document(repaint: mutate.Policy.RepaintOrDefault),
            _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
        };

    internal static GrasshopperUiPolicy ForEditor(EditorOp op) =>
        op switch {
            EditorOp.ShowCase show => GrasshopperUiPolicy.Canvas(openEditor: show.Visible),
            EditorOp.EnsureVisibleCase or EditorOp.ShellCase => GrasshopperUiPolicy.Canvas(openEditor: true),
            _ => GrasshopperUiPolicy.Read,
        };

    internal static GrasshopperUiPolicy ForLayout(LayoutOp op) =>
        op.Switch(
            measureCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None),
            arrangeCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
            snapCase: static _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.None));

    internal static GrasshopperUiPolicy ForWire(WireOp op) =>
        op switch {
            WireOp.QueryCase => GrasshopperUiPolicy.Canvas(),
            WireOp.InstallShapeCase => GrasshopperUiPolicy.Read,
            WireOp.OverlayPenCase or WireOp.WirePaintObserveCase => GrasshopperUiPolicy.Canvas(repaint: RepaintRequest.Scheduled),
            _ => GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas),
        };

    internal static GrasshopperUiPolicy ForEvent(UiEvent uiEvent) =>
        uiEvent switch {
            UiEvent.PaintCase => GrasshopperUiPolicy.Canvas(),
            UiEvent.DocumentCase or UiEvent.SolutionCase or UiEvent.UndoCase => GrasshopperUiPolicy.Document(),
            _ => GrasshopperUiPolicy.Read,
        };

    internal static GrasshopperUiPolicy ForCanvasChrome(CanvasChromeOp op) =>
        op.Switch(
            tooltipCase: static t => ForTooltip(op: t.Op),
            floatingButtonCase: static _ => GrasshopperUiPolicy.Canvas(),
            interactionCase: static _ => GrasshopperUiPolicy.Canvas());

    internal static GrasshopperUiPolicy ForTooltip(TooltipOp op) =>
        op.Switch(
            showCase: static _ => GrasshopperUiPolicy.Canvas(),
            hideCase: static _ => GrasshopperUiPolicy.Read,
            invalidateCase: static _ => GrasshopperUiPolicy.Read,
            statusCase: static _ => GrasshopperUiPolicy.Read,
            layoutCase: static _ => GrasshopperUiPolicy.Read);
}

public sealed record GrasshopperUiIntent<T> {
    private readonly Func<GrasshopperUi.Scope, Fin<T>> run;
    internal GrasshopperUiIntent(Func<GrasshopperUi.Scope, Fin<T>> run, GrasshopperUiPolicy policy) {
        this.run = run;
        Policy = policy;
    }
    internal GrasshopperUiPolicy Policy { get; }
    internal Fin<T> Run(GrasshopperUi.Scope scope) => run(arg: scope);
    public GrasshopperUiIntent<TOut> Map<TOut>(Func<T, TOut> project) =>
        new(run: scope => Run(scope: scope).Map(project), policy: Policy);
    public GrasshopperUiIntent<TOut> Bind<TOut>(Func<T, GrasshopperUiIntent<TOut>> bind) =>
        new(
            run: scope =>
                from value in Run(scope: scope)
                from next in Optional(bind(arg: value))
                    .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Bind)), detail: "bind returned null"))
                let merged = Policy | next.Policy
                from resolved in merged == Policy
                    ? Fin.Succ(value: scope)
                    : GrasshopperUi.Scope.Resolve(policy: merged, cancellation: scope.Cancellation, undo: scope.UndoGroup)
                from result in next.Run(scope: resolved)
                select GrasshopperUi.Repaint(scope: resolved, policy: next.Policy, value: result),
            policy: Policy);
}

public abstract record GhUiRequest<T> {
    internal abstract GrasshopperUiPolicy Policy { get; }
    internal abstract Fin<T> Apply(GrasshopperUi.Scope scope);
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
    internal static Fin<float> AcceptFinite(this Op op, float value, string detail = "non-finite value", bool requirePositive = false, bool nonNegative = false) =>
        float.IsFinite(value)
        && (!requirePositive || value > 0f)
        && (!nonNegative || value >= 0f)
            ? Fin.Succ(value)
            : Fin.Fail<float>(error: UiFault.InvalidInput(op: op, detail: detail));

    // Native-boundary exception capsule. Wraps `Try.lift(f).Run().MapFail(e => UiFault.MutationRejected(op,
    // $"{what} threw: {e.Message}"))` so call sites stay declarative. Use for any GH2/Eto/Rhino mutation
    // that may throw native exceptions; the typed UiFault.MutationRejected case preserves the op + reason.
    [BoundaryAdapter]
    internal static Fin<T> Attempt<T>(this Op op, Func<T> body, string what = "") =>
        Optional(body)
            .ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null delegate"))
            .Bind(valid => Try.lift(f: valid).Run().MapFail(error => UiFault.MutationRejected(
                op: op,
                detail: string.IsNullOrEmpty(what)
                    ? $"native call threw: {error.Message}"
                    : $"{what} threw: {error.Message}")));

    // Side-effect variant of Attempt. Returns Fin<Unit> for actions that don't yield a value.
    [BoundaryAdapter]
    internal static Fin<Unit> Attempt(this Op op, System.Action body, string what = "") =>
        op.Attempt(body: () => { body(); return unit; }, what: what);

    // Parallel admission — folds multiple AcceptPoint/AcceptRect/AcceptFinite checks into one
    // Validation<Seq<UiFault>, T> before ToFin. Use when PaintStyle/CornerRadii/multi-field
    // validation repeats across a single payload.
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

    // Validation<Seq<UiFault>, T> → Fin<T> bridge — Seq's built-in monoid accumulates faults during
    // parallel validation; this collapses to Error.Many for single-error Fin contract while preserving
    // every accumulated UiFault's provenance. Use when CornerRadii/SpringConfig/Bounds/Navigate fold
    // multiple AcceptX calls via Apply.
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

    public static GrasshopperUiIntent<CanvasResult> Canvas(CanvasOp op) => Apply(request: new CanvasRequest.Run(Op: op));
    public static GrasshopperUiIntent<DocumentResult> Document(DocumentOp op) => Apply(request: new DocumentRequest.Run(Op: op));
    public static GrasshopperUiIntent<EditorResult> Editor(EditorOp op) => Apply(request: new EditorRequest.Run(Op: op));
    public static GrasshopperUiIntent<LayoutResult> Layout(LayoutOp op) => Apply(request: new LayoutRequest.Run(Op: op));
    public static GrasshopperUiIntent<WireResult> Wire(WireOp op) => Apply(request: new WireRequest.Run(Op: op));
    public static GrasshopperUiIntent<T> Input<T>(InputRequest<T> request) => Apply(request: request);
    public static GrasshopperUiIntent<T> Paint<T>(PaintRequest<T> request) => Apply(request: request);
    public static GrasshopperUiIntent<T> Motion<T>(MotionRequest<T> request) => Apply(request: request);
    public static GrasshopperUiIntent<CanvasChromeResult> CanvasChrome(CanvasChromeOp op) => Apply(request: new CanvasChromeRequest.Run(Op: op));
    public static GrasshopperUiIntent<CanvasChromeResult> Tooltip(TooltipOp op) => CanvasChrome(CanvasChromeOp.Tooltip(op: op));
    public static GrasshopperUiIntent<CanvasChromeResult> FloatingButton(FloatingButtonOp op) => CanvasChrome(CanvasChromeOp.FloatingButton(op: op));
    public static GrasshopperUiIntent<CanvasChromeResult> Interaction(InteractionOp op) => CanvasChrome(CanvasChromeOp.Interaction(op: op));
    public static GrasshopperUiIntent<Subscription> Event(UiEvent uiEvent) => Apply(request: new EventRequest.Run(Event: uiEvent));

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

[BoundaryAdapter]
public sealed partial record GrasshopperUi {
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Scope(Option<GhEditor> Editor, Option<GhCanvas> Canvas, Option<GhDocument> Document, Option<GhDocumentMethods> Methods, Option<GhObjectList> Objects, Option<Skin> Skin, Option<UndoGroup> UndoGroup, CancellationToken Cancellation) {
        internal static Fin<Scope> Resolve(GrasshopperUiPolicy policy, CancellationToken cancellation, Option<UndoGroup> undo = default) =>
            cancellation.IsCancellationRequested
                ? Fin.Fail<Scope>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Resolve))))
                : Fin.Succ(value: (GhEditor.Instance, policy.OpenEditor) switch {
                    (GhEditor current, _) => current,
                    (null, true) => GhEditor.ShowEditor(createVisible: true),
                    _ => null,
                }).Map(editor => {
                    GhCanvas? canvas = editor?.Canvas;
                    GhDocument? document = editor?.Documents.Current ?? canvas?.Document;
                    return new Scope(
                        Editor: Optional(editor),
                        Canvas: Optional(canvas),
                        Document: Optional(document),
                        Methods: Optional(document?.Methods),
                        Objects: Optional(document?.Objects),
                        Skin: Optional(canvas?.Skin),
                        UndoGroup: undo,
                        Cancellation: cancellation);
                }).Bind(scope => (policy.RequireCanvas && scope.Canvas.IsNone, policy.RequireDocument && scope.Document.IsNone) switch {
                    (true, _) => Fin.Fail<Scope>(error: UiFault.MissingScope(field: nameof(Canvas))),
                    (_, true) => Fin.Fail<Scope>(error: UiFault.MissingScope(field: nameof(Document))),
                    _ => Fin.Succ(value: scope),
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
            Skin.ToFin(Fail: UiFault.MissingScope(field: nameof(Skin)));
    }

    public Fin<T> Use<T>(GrasshopperUiIntent<T> intent, CancellationToken cancellation = default) =>
        from valid in Optional(intent).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Use)), detail: "null intent"))
        from result in OnUiThread(
            cancellation: cancellation,
            run: () =>
                from scope in Scope.Resolve(policy: valid.Policy, cancellation: cancellation)
                from value in valid.Run(scope: scope)
                select Repaint(scope: scope, policy: valid.Policy, value: value))
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

    // Two-phase commit: PrepareUndo captures pre-state on each Action; cancellation check between
    // prepare and Do() prevents the document from being mutated without an undo entry.
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
