using Foundation.CSharp.Analyzers.Contracts;
using Grasshopper2.Doc;
using GhDocument = Grasshopper2.Doc.Document;
using GhObjectList = Grasshopper2.Doc.ObjectList;
using Op = Rasm.Domain.Op;

namespace Rasm.Grasshopper.UI;

// --- [TYPES] -----------------------------------------------------------------------------
[Union]
public partial record UiEvent {
    private UiEvent() { }
    public sealed record PaintCase(CanvasPaintPhase Phase, Func<PaintScope, Fin<Unit>> Handler) : UiEvent;
    public sealed record DocumentChangedCase(Func<DocumentSnapshot, Fin<Unit>> Handler, TimeSpan PollInterval) : UiEvent;
    public sealed record SelectionChangedCase(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> Handler, TimeSpan PollInterval) : UiEvent;

    public static UiEvent Paint(CanvasPaintPhase phase, Func<PaintScope, Fin<Unit>> handler) =>
        new PaintCase(Phase: phase, Handler: handler);
    public static UiEvent DocumentChanged(Func<DocumentSnapshot, Fin<Unit>> handler, TimeSpan pollInterval = default) =>
        new DocumentChangedCase(Handler: handler, PollInterval: pollInterval == default ? TimeSpan.FromMilliseconds(value: 250) : pollInterval);
    public static UiEvent SelectionChanged(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler, TimeSpan pollInterval = default) =>
        new SelectionChangedCase(Handler: handler, PollInterval: pollInterval == default ? TimeSpan.FromMilliseconds(value: 250) : pollInterval);
}

public abstract record EventRequest<T> : GhUiRequest<T> {
    public sealed record Subscribe(UiEvent Event) : EventRequest<IDisposable> {
        internal override GrasshopperUiPolicy Policy => GrasshopperUiPolicy.Document();
        internal override Fin<IDisposable> Apply(GrasshopperUi.Scope scope) => Events.Subscribe(uiEvent: Event).Run(scope: scope);
    }
}

// --- [SERVICES] --------------------------------------------------------------------------
internal static partial class Events {
    internal static GrasshopperUiIntent<IDisposable> Subscribe(UiEvent uiEvent) =>
        uiEvent switch {
            UiEvent.PaintCase p => Paint.Hook(phase: p.Phase, paint: p.Handler),
            UiEvent.DocumentChangedCase d => SubscribeDocumentChange(handler: d.Handler, pollInterval: d.PollInterval),
            UiEvent.SelectionChangedCase s => SubscribeSelectionChange(handler: s.Handler, pollInterval: s.PollInterval),
            _ => IntentFactory.Document<IDisposable>(run: _ => Fin.Fail<IDisposable>(error: UiFault.InvalidInput(op: Op.Of(name: nameof(Subscribe)), detail: $"event kind not supported: {uiEvent.GetType().Name}"))),
        };

    // --- [OPERATIONS] ----------------------------------------------------------------------
    private static GrasshopperUiIntent<IDisposable> SubscribeDocumentChange(Func<DocumentSnapshot, Fin<Unit>> handler, TimeSpan pollInterval) =>
        IntentFactory.Document<IDisposable>(run: scope =>
            from doc in scope.NeedDocument()
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeDocumentChange)), detail: "null handler"))
            select (IDisposable)DocumentChangeWatcher.Attach(document: doc, objects: objs, handler: valid));

    private static GrasshopperUiIntent<IDisposable> SubscribeSelectionChange(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler, TimeSpan pollInterval) =>
        IntentFactory.Document<IDisposable>(run: scope =>
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSelectionChange)), detail: "null handler"))
            select (IDisposable)SelectionChangeWatcher.Attach(objects: objs, handler: valid));

    private sealed class DocumentChangeWatcher : IDisposable {
        private readonly GhDocument document;
        private readonly GhObjectList objects;
        private readonly Func<DocumentSnapshot, Fin<Unit>> handler;
        private readonly EventHandler<DocumentModifiedEventArgs> modified;
        private readonly EventHandler<DocumentStateEventArgs> state;
        private readonly EventHandler<AfterAddObjectEventArgs> added;
        private readonly EventHandler<AfterRemoveObjectEventArgs> removed;
        private readonly EventHandler<ObjectEventArgs> objectChanged;
        private DocumentChangeWatcher(GhDocument document, GhObjectList objects, Func<DocumentSnapshot, Fin<Unit>> handler) {
            this.document = document;
            this.objects = objects;
            this.handler = handler;
            modified = (_, _) => Publish();
            state = (_, _) => Publish();
            added = (_, _) => Publish();
            removed = (_, _) => Publish();
            objectChanged = (_, _) => Publish();
            document.ModifiedChanged += modified;
            document.StateChanged += state;
            objects.ObjectAdded += added;
            objects.ObjectRemoved += removed;
            objects.ObjectExpired += objectChanged;
            objects.ObjectEnabledChanged += objectChanged;
            objects.ObjectLayoutChanged += objectChanged;
            objects.ObjectDisplayChanged += objectChanged;
        }
        internal static DocumentChangeWatcher Attach(GhDocument document, GhObjectList objects, Func<DocumentSnapshot, Fin<Unit>> handler) =>
            new(document: document, objects: objects, handler: handler);
        private Unit Publish() {
            _ = GrasshopperUi.Protect(valid: () => handler(arg: UiRail.DocumentSnapshotOf(document: document, objects: objects)));
            return unit;
        }
        public void Dispose() {
            document.ModifiedChanged -= modified;
            document.StateChanged -= state;
            objects.ObjectAdded -= added;
            objects.ObjectRemoved -= removed;
            objects.ObjectExpired -= objectChanged;
            objects.ObjectEnabledChanged -= objectChanged;
            objects.ObjectLayoutChanged -= objectChanged;
            objects.ObjectDisplayChanged -= objectChanged;
        }
    }

    private sealed class SelectionChangeWatcher : IDisposable {
        private readonly GhObjectList objects;
        private readonly Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler;
        private readonly EventHandler<ObjectEventArgs> selectedChanged;
        private SelectionChangeWatcher(GhObjectList objects, Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) {
            this.objects = objects;
            this.handler = handler;
            selectedChanged = (_, _) => Publish();
            objects.ObjectSelectionChanged += selectedChanged;
        }
        internal static SelectionChangeWatcher Attach(GhObjectList objects, Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler) =>
            new(objects: objects, handler: handler);
        private Unit Publish() {
            Seq<DocumentObjectSnapshot> selected = toSeq(objects.SelectedObjects.Select(SnapshotObjectFor));
            _ = GrasshopperUi.Protect(valid: () => handler(arg: selected));
            return unit;
        }
        private static DocumentObjectSnapshot SnapshotObjectFor(IDocumentObject obj) =>
            new(Id: obj.InstanceId, Name: obj.Nomen.Name, DisplayName: obj.DisplayName,
                Selected: obj.Selected, Activity: obj.Activity.ToString(),
                Display: obj.Display.ToString(), Phase: obj.Phase.ToString(), State: obj.State.ToString(),
                Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);
        public void Dispose() =>
            objects.ObjectSelectionChanged -= selectedChanged;
    }
}
