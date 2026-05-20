using Eto.Forms;
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

// --- [SERVICES] --------------------------------------------------------------------------
public static partial class Events {
    public static GrasshopperUiIntent<IDisposable> Subscribe(UiEvent uiEvent) =>
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
            select (IDisposable)DocumentChangeWatcher.Attach(document: doc, objects: objs, handler: valid, pollInterval: pollInterval));

    private static GrasshopperUiIntent<IDisposable> SubscribeSelectionChange(Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler, TimeSpan pollInterval) =>
        IntentFactory.Document<IDisposable>(run: scope =>
            from objs in scope.NeedObjects()
            from valid in Optional(handler).ToFin(Fail: UiFault.InvalidInput(op: Op.Of(name: nameof(SubscribeSelectionChange)), detail: "null handler"))
            select (IDisposable)SelectionChangeWatcher.Attach(objects: objs, handler: valid, pollInterval: pollInterval));

    private sealed class DocumentChangeWatcher : IDisposable {
        private readonly UITimer timer;
        private readonly GhDocument document;
        private readonly GhObjectList objects;
        private readonly Func<DocumentSnapshot, Fin<Unit>> handler;
        private int lastModifications;
        private DocumentChangeWatcher(GhDocument document, GhObjectList objects, Func<DocumentSnapshot, Fin<Unit>> handler, TimeSpan pollInterval) {
            this.document = document;
            this.objects = objects;
            this.handler = handler;
            lastModifications = document.Modifications;
            timer = new UITimer { Interval = pollInterval.TotalSeconds };
            timer.Elapsed += OnTick;
            timer.Start();
        }
        internal static DocumentChangeWatcher Attach(GhDocument document, GhObjectList objects, Func<DocumentSnapshot, Fin<Unit>> handler, TimeSpan pollInterval) =>
            new(document: document, objects: objects, handler: handler, pollInterval: pollInterval);
        private void OnTick(object? sender, EventArgs args) {
            if (document.Modifications == lastModifications) return;
            lastModifications = document.Modifications;
            _ = GrasshopperUi.Protect(valid: () => handler(arg: Document.SnapshotOf(document: document, objects: objects)));
        }
        public void Dispose() {
            timer.Stop();
            timer.Elapsed -= OnTick;
            timer.Dispose();
        }
    }

    private sealed class SelectionChangeWatcher : IDisposable {
        private readonly UITimer timer;
        private readonly GhObjectList objects;
        private readonly Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler;
        private int lastSelectedCount;
        private SelectionChangeWatcher(GhObjectList objects, Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler, TimeSpan pollInterval) {
            this.objects = objects;
            this.handler = handler;
            lastSelectedCount = objects.SelectedCount;
            timer = new UITimer { Interval = pollInterval.TotalSeconds };
            timer.Elapsed += OnTick;
            timer.Start();
        }
        internal static SelectionChangeWatcher Attach(GhObjectList objects, Func<Seq<DocumentObjectSnapshot>, Fin<Unit>> handler, TimeSpan pollInterval) =>
            new(objects: objects, handler: handler, pollInterval: pollInterval);
        private void OnTick(object? sender, EventArgs args) {
            if (objects.SelectedCount == lastSelectedCount) return;
            lastSelectedCount = objects.SelectedCount;
            Seq<DocumentObjectSnapshot> selected = toSeq(objects.SelectedObjects.Select(SnapshotObjectFor));
            _ = GrasshopperUi.Protect(valid: () => handler(arg: selected));
        }
        private static DocumentObjectSnapshot SnapshotObjectFor(IDocumentObject obj) =>
            new(Id: obj.InstanceId, Name: obj.Nomen.Name, DisplayName: obj.DisplayName,
                Selected: obj.Selected, Activity: obj.Activity.ToString(),
                Display: obj.Display.ToString(), Phase: obj.Phase.ToString(), State: obj.State.ToString(),
                Bounds: obj.Attributes.Bounds, Pivot: obj.Attributes.Pivot);
        public void Dispose() {
            timer.Stop();
            timer.Elapsed -= OnTick;
            timer.Dispose();
        }
    }
}
