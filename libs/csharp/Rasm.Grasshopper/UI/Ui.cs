using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Eto.Drawing;
using Foundation.CSharp.Analyzers.Contracts;
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
using UndoAction = Grasshopper2.Undo.Action;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
[Union]
public partial record RepaintRequest {
    private RepaintRequest() { }
    public sealed record NoneCase : RepaintRequest;
    public sealed record ObjectCase(Guid Id) : RepaintRequest;
    public sealed record RegionCase(RectangleF Bounds) : RepaintRequest;
    public sealed record CanvasCase : RepaintRequest;
    public sealed record ScheduledCase : RepaintRequest;

    public static readonly RepaintRequest None = new NoneCase();
    public static readonly RepaintRequest Canvas = new CanvasCase();
    public static readonly RepaintRequest Scheduled = new ScheduledCase();
    public static RepaintRequest Object(Guid id) => new ObjectCase(Id: id);
    public static RepaintRequest Region(RectangleF bounds) => new RegionCase(Bounds: bounds);

    public static RepaintRequest operator |(RepaintRequest left, RepaintRequest right) =>
        (left, right) switch {
            (NoneCase, _) => right,
            (_, NoneCase) => left,
            (CanvasCase, _) or (_, CanvasCase) => Canvas,
            (ScheduledCase, _) or (_, ScheduledCase) => Scheduled,
            (RegionCase l, RegionCase r) => Region(bounds: RectangleF.Union(l.Bounds, r.Bounds)),
            (ObjectCase l, ObjectCase r) when l.Id == r.Id => left,
            _ => Canvas,
        };
    public static RepaintRequest BitwiseOr(RepaintRequest left, RepaintRequest right) => left | right;
}

// --- [ERRORS] ----------------------------------------------------------------------------
[Union]
public abstract partial record UiFault : Expected {
    private UiFault() : base() { }

    public sealed record MissingScopeCase(string Field) : UiFault {
        public override string Message => $"GrasshopperUi scope field '{Field}' required but absent.";
        public override string Category => "Scope";
    }
    public sealed record InvalidInputCase(Op Op, string Detail) : UiFault {
        public override string Message => $"Op '{Op}' rejected input: {Detail}.";
        public override string Category => "Input";
    }
    public sealed record MutationRejectedCase(Op Op, string Detail) : UiFault {
        public override string Message => $"Op '{Op}' rejected by Grasshopper2: {Detail}.";
        public override string Category => "Mutation";
    }
    public sealed record RhinoEditorCase(string Detail) : UiFault {
        public override string Message => $"Rhino editor operation failed: {Detail}.";
        public override string Category => "Editor";
    }
    public sealed record ResourceLeakedCase(string Detail) : UiFault {
        public override string Message => $"Resource teardown failed: {Detail}.";
        public override string Category => "Resource";
    }
    public sealed record ThreadMarshalCase(string Detail) : UiFault {
        public override string Message => $"UI-thread marshal failed: {Detail}.";
        public override string Category => "Thread";
    }
    public sealed record CancelledCase(Op Op) : UiFault {
        public override string Message => $"Op '{Op}' cancelled.";
        public override string Category => "Cancelled";
    }

    public static UiFault MissingScope(string field) => new MissingScopeCase(Field: field);
    public static UiFault InvalidInput(Op op, string detail) => new InvalidInputCase(Op: op, Detail: detail);
    public static UiFault MutationRejected(Op op, string detail) => new MutationRejectedCase(Op: op, Detail: detail);
    public static UiFault RhinoEditor(string detail) => new RhinoEditorCase(Detail: detail);
    public static UiFault ResourceLeaked(string detail) => new ResourceLeakedCase(Detail: detail);
    public static UiFault ThreadMarshal(string detail) => new ThreadMarshalCase(Detail: detail);
    public static UiFault Cancelled(Op op) => new CancelledCase(Op: op);
}

[Union]
internal partial record UndoStrategy {
    private UndoStrategy() { }
    public sealed record GhBuiltInCase : UndoStrategy;
    public sealed record ManualCase(Func<GrasshopperUi.Scope, UndoEntry> Record) : UndoStrategy;
    public sealed record NoneCase : UndoStrategy;

    public static readonly UndoStrategy GhBuiltIn = new GhBuiltInCase();
    public static readonly UndoStrategy None = new NoneCase();
    public static UndoStrategy Manual(Func<GrasshopperUi.Scope, UndoEntry> record) => new ManualCase(Record: record);
}

// --- [MODELS] ----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct Snapshot<T>(
    Option<Guid> OwnerId,
    DateTimeOffset CapturedAt,
    T Payload) {
    public Snapshot<TOut> Map<TOut>(Func<T, TOut> project) {
        ArgumentNullException.ThrowIfNull(argument: project);
        return new(OwnerId: OwnerId, CapturedAt: CapturedAt, Payload: project(arg: Payload));
    }
}

public static class Snapshot {
    public static Snapshot<T> Of<T>(T payload, Option<Guid> ownerId = default) =>
        new(OwnerId: ownerId, CapturedAt: DateTimeOffset.UtcNow, Payload: payload);
}

public readonly record struct DocumentMutationDelta(int Changed, DocumentSnapshot After);
public readonly record struct LayoutMoveDelta(Guid ObjectId, float Dx, float Dy, LayoutSnapshot After, Option<SnappingSnapshot> Snap);
public readonly record struct WireSplitDelta(bool Changed, WireSnapshot.ConnectedCase Wire, Option<Guid> Shout, Option<Guid> Listen);

public readonly record struct UndoEntry(string Verb, string Noun, Seq<UndoAction> Actions) {
    internal VerbNoun AsName() => (Verb, Noun);
    internal ActionList AsList() => new([.. Actions]);
}

public sealed class UndoGroup {
    private readonly List<UndoAction> actions = [];
    public string Verb { get; }
    public string Noun { get; }
    internal UndoGroup(string verb, string noun) {
        Verb = verb;
        Noun = noun;
    }
    public Unit Add(UndoAction action) {
        actions.Add(item: action);
        return unit;
    }
    internal UndoEntry ToEntry() => new(Verb: Verb, Noun: Noun, Actions: toSeq(actions));
    internal Fin<Unit> Commit(GhDocument document) =>
        Try.lift<Unit>(f: () => {
            UndoEntry entry = ToEntry();
            _ = Optional(entry)
                .Filter(static e => e.Actions.Count > 0)
                .IfSome(e => document.Undo.Do(name: e.AsName(), actions: e.AsList()));
            return unit;
        }).Run().MapFail(_ => UiFault.MutationRejected(op: Op.Of(name: nameof(Commit)), detail: "History.Do threw"));
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct GrasshopperUiPolicy(
    bool OpenEditor = false,
    bool RequireCanvas = false,
    bool RequireDocument = false,
    RepaintRequest? Repaint = null) {
    public static GrasshopperUiPolicy Read => default;
    public static readonly GrasshopperUiPolicy Empty;

    internal static GrasshopperUiPolicy Canvas(bool openEditor = false, RepaintRequest? repaint = null) =>
        new(OpenEditor: openEditor, RequireCanvas: true, Repaint: repaint);
    internal static GrasshopperUiPolicy Document(RepaintRequest? repaint = null) =>
        new(RequireCanvas: true, RequireDocument: true, Repaint: repaint);

    public static GrasshopperUiPolicy operator |(GrasshopperUiPolicy left, GrasshopperUiPolicy right) =>
        new(
            OpenEditor: left.OpenEditor || right.OpenEditor,
            RequireCanvas: left.RequireCanvas || right.RequireCanvas,
            RequireDocument: left.RequireDocument || right.RequireDocument,
            Repaint: (left.Repaint ?? RepaintRequest.None) | (right.Repaint ?? RepaintRequest.None));

    public static GrasshopperUiPolicy BitwiseOr(GrasshopperUiPolicy left, GrasshopperUiPolicy right) => left | right;
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
        new(run: scope => Run(scope: scope).Bind(value => bind(arg: value).Run(scope: scope)), policy: Policy | GrasshopperUiPolicy.Document(repaint: RepaintRequest.Canvas));
}

public abstract record GhUiRequest<T> {
    internal abstract GrasshopperUiPolicy Policy { get; }
    internal abstract Fin<T> Apply(GrasshopperUi.Scope scope);
}

public static class GhUi {
    public static GrasshopperUiIntent<T> Apply<T>(GhUiRequest<T> request) =>
        new(
            run: scope => Optional(request)
                .ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(Apply)), detail: "request is required"))
                .Bind(valid => valid.Apply(scope: scope)),
            policy: Optional(request).Map(static valid => valid.Policy).IfNone(GrasshopperUiPolicy.Read));
}

internal static class IntentFactory {
    internal static GrasshopperUiIntent<T> Read<T>(Func<GrasshopperUi.Scope, Fin<T>> run) =>
        new(run: run, policy: GrasshopperUiPolicy.Read);
    internal static GrasshopperUiIntent<T> Canvas<T>(Func<GrasshopperUi.Scope, Fin<T>> run, bool openEditor = false, RepaintRequest? repaint = null) =>
        new(run: run, policy: GrasshopperUiPolicy.Canvas(openEditor: openEditor, repaint: repaint));
    internal static GrasshopperUiIntent<T> Document<T>(Func<GrasshopperUi.Scope, Fin<T>> run, RepaintRequest? repaint = null) =>
        new(run: run, policy: GrasshopperUiPolicy.Document(repaint: repaint));
}

// --- [SERVICES] --------------------------------------------------------------------------
[BoundaryAdapter]
public sealed partial record GrasshopperUi {
    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct Scope(
        Option<GhEditor> Editor,
        Option<GhCanvas> Canvas,
        Option<GhDocument> Document,
        Option<GhDocumentMethods> Methods,
        Option<GhObjectList> Objects,
        Option<Skin> Skin,
        Option<UndoGroup> UndoGroup,
        CancellationToken Cancellation) {
        internal static Fin<Scope> Resolve(GrasshopperUiPolicy policy, CancellationToken cancellation, Option<UndoGroup> undo = default) =>
            cancellation.IsCancellationRequested
                ? Fin.Fail<Scope>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Resolve))))
                : ResolveInner(policy: policy, undo: undo, cancellation: cancellation);

        private static Fin<Scope> ResolveInner(GrasshopperUiPolicy policy, Option<UndoGroup> undo, CancellationToken cancellation) {
            GhEditor? editor = (GhEditor.Instance, policy.OpenEditor) switch {
                (GhEditor current, _) => current,
                (null, true) => GhEditor.ShowEditor(createVisible: true),
                _ => null,
            };
            GhCanvas? canvas = editor?.Canvas;
            GhDocument? document = canvas?.Document;
            Scope scope = new(
                Editor: Optional(editor),
                Canvas: Optional(canvas),
                Document: Optional(document),
                Methods: Optional(document?.Methods),
                Objects: Optional(document?.Objects),
                Skin: Optional(canvas?.Skin),
                UndoGroup: undo,
                Cancellation: cancellation);
            return (policy.RequireCanvas && scope.Canvas.IsNone, policy.RequireDocument && scope.Document.IsNone) switch {
                (true, _) => Fin.Fail<Scope>(error: UiFault.MissingScope(field: nameof(Canvas))),
                (_, true) => Fin.Fail<Scope>(error: UiFault.MissingScope(field: nameof(Document))),
                _ => Fin.Succ(value: scope),
            };
        }

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
            IntentFactory.Document<Snapshot<TDelta>>(
                repaint: repaint,
                run: scope =>
                    from delta in Optional(mutate).ToFin(Fail: UiFault.InvalidInput(op: op, detail: "null delegate")).Bind(m => m(arg: scope))
                    from _ in RecordUndo(scope: scope, op: op, undo: undo)
                    select Snapshot.Of<TDelta>(payload: delta, ownerId: scope.Document.Map(d => d.Hash)));

    internal static GrasshopperUiIntent<Snapshot<DocumentMutationDelta>> MutateDocument(
        Op op,
        Func<GhDocumentMethods, Fin<int>> mutate,
        RepaintRequest? repaint = null) =>
            Mutate<DocumentMutationDelta>(
                op: op,
                undo: UndoStrategy.GhBuiltIn,
                repaint: repaint ?? RepaintRequest.Canvas,
                mutate: scope =>
                    from methods in scope.NeedMethods()
                    from document in scope.NeedDocument()
                    from objects in scope.NeedObjects()
                    from changed in mutate(arg: methods).Bind(count => count >= 0
                        ? Fin.Succ(value: count)
                        : Fin.Fail<int>(error: UiFault.MutationRejected(op: op, detail: $"count={count}")))
                    select new DocumentMutationDelta(Changed: changed, After: UiRail.DocumentSnapshotOf(document: document, objects: objects)));

    private static Fin<Unit> RecordUndo(Scope scope, Op op, UndoStrategy undo) =>
        undo switch {
            UndoStrategy.NoneCase or UndoStrategy.GhBuiltInCase => Fin.Succ(value: unit),
            UndoStrategy.ManualCase manual =>
                from document in scope.NeedDocument()
                from committed in scope.UndoGroup.Match(
                    Some: bag => Try.lift<Unit>(f: () => {
                        UndoEntry entry = manual.Record(arg: scope);
                        _ = entry.Actions.Iter(action => bag.Add(action: action));
                        return unit;
                    }).Run().MapFail(_ => UiFault.MutationRejected(op: op, detail: "manual undo recording threw")),
                    None: () => Try.lift<Unit>(f: () => {
                        UndoEntry entry = manual.Record(arg: scope);
                        _ = Optional(entry)
                            .Filter(static e => e.Actions.Count > 0)
                            .IfSome(e => document.Undo.Do(name: e.AsName(), actions: e.AsList()));
                        return unit;
                    }).Run().MapFail(_ => UiFault.MutationRejected(op: op, detail: "History.Do threw")))
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

    private static Fin<T> Marshal<T>(Func<Fin<T>> valid, CancellationToken cancellation) {
        return Try.lift<Fin<T>>(f: () => {
            Fin<T> result = Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Marshal))));
            RhinoApp.InvokeAndWait(action: () => result = cancellation.IsCancellationRequested
                ? Fin.Fail<T>(error: UiFault.Cancelled(op: Op.Of(name: nameof(Marshal))))
                : Protect(valid: valid));
            return result;
        }).Run().MapFail(_ => UiFault.ThreadMarshal(detail: "InvokeAndWait threw")).Bind(static result => result);
    }

    internal static Fin<T> Protect<T>(Func<Fin<T>> valid, [CallerMemberName] string name = "") =>
        Try.lift<Fin<T>>(f: valid).Run().MapFail(_ => UiFault.ThreadMarshal(detail: name)).Bind(static result => result);

    private static T Repaint<T>(Scope scope, GrasshopperUiPolicy policy, T value) {
        _ = (policy.Repaint ?? RepaintRequest.None) switch {
            RepaintRequest.NoneCase => unit,
            RepaintRequest.CanvasCase => scope.Canvas.IfSome(static canvas => canvas.Invalidate()),
            RepaintRequest.ScheduledCase => scope.Canvas.IfSome(static canvas => canvas.ScheduleRedraw()),
            RepaintRequest.RegionCase region => scope.Canvas.IfSome(canvas => canvas.Invalidate(rect: ToIntRect(region.Bounds))),
            RepaintRequest.ObjectCase obj => scope.Canvas.Bind(canvas => scope.Objects.Map(objects => InvalidateObject(canvas: canvas, objects: objects, id: obj.Id))).IfNone(unit),
            _ => unit,
        };
        return value;
    }

    private static Unit InvalidateObject(GhCanvas canvas, GhObjectList objects, Guid id) =>
        Optional(objects.Find(instanceId: id)).Match(
            Some: obj => { canvas.Invalidate(rect: ToIntRect(obj.Attributes.AggregateBounds)); return unit; },
            None: () => { canvas.Invalidate(); return unit; });

    private static Eto.Drawing.Rectangle ToIntRect(RectangleF f) =>
        new(x: (int)Math.Floor(f.X), y: (int)Math.Floor(f.Y), width: (int)Math.Ceiling(f.Width), height: (int)Math.Ceiling(f.Height));

}

public static class GrasshopperUiIntentExtensions {
    public static GrasshopperUiIntent<T> Group<T>(this GrasshopperUiIntent<T> body, string verb, string noun) =>
        Optional(body).Match(
            Some: valid => IntentFactory.Document<T>(
                repaint: valid.Policy.Repaint,
                run: scope => {
                    UndoGroup bag = new(verb: verb, noun: noun);
                    GrasshopperUi.Scope grouped = scope with { UndoGroup = Some(bag) };
                    return from value in valid.Run(scope: grouped)
                           from document in scope.NeedDocument()
                           from committed in bag.Commit(document: document)
                           select value;
                }),
            None: () => IntentFactory.Document<T>(run: _ => Fin.Fail<T>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Group)), detail: "body is required"))));
}
