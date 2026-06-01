using System.Runtime.InteropServices;
using Rhino.DocObjects.Tables;
using IODirectory = System.IO.Directory;
using IOPath = System.IO.Path;

namespace Rasm.Rhino.Events;

// --- [TYPES] ------------------------------------------------------------------------------
public enum DeferralPolicy { Immediate, Idle }

public enum WatchPanelState { Shown, Hidden, Closed }

[Union]
public abstract partial record WatchPayload {
    private WatchPayload() { }

    public sealed record View(RhinoView Value) : WatchPayload;
    public sealed record Objects(Seq<Guid> Ids) : WatchPayload;
    public sealed record Selection(Seq<Guid> Ids, int Count) : WatchPayload;
    public sealed record Replace(Guid Old, Option<Guid> New) : WatchPayload;
    public sealed record Attributes(Option<Guid> Object) : WatchPayload;
    public sealed record LayerEvent(LayerTableEventType Event, int Index, Option<Layer> Old, Option<Layer> Next) : WatchPayload;
    public sealed record Viewport(Guid Id, uint ChangeCounter) : WatchPayload;
    public sealed record DisplayMode(Guid ViewportId, Guid Old, Guid Next) : WatchPayload;
    public sealed record PageView(uint SerialNumber, Option<Guid> OldActiveDetailId, Option<Guid> NewActiveDetailId) : WatchPayload;
    public sealed record Panel(Guid Id, WatchPanelState State) : WatchPayload;
    public sealed record Document(
        Option<string> FileName = default,
        Option<bool> Merge = default,
        Option<bool> Reference = default,
        Option<bool> ExportSelected = default,
        Option<double> Scale = default,
        Option<string> UserStringKey = default) : WatchPayload {
        internal static Document Open(DocumentOpenEventArgs args) =>
            new(FileName: Optional(args.FileName), Merge: Some(args.Merge), Reference: Some(args.Reference));

        internal static Document Save(DocumentSaveEventArgs args) =>
            new(FileName: Optional(args.FileName), ExportSelected: Some(args.ExportSelected));

        internal static Document Units(UnitsChangedWithScalingEventArgs args) =>
            new(Scale: Some(args.Scale));

        internal static Document UserString(RhinoDoc.UserStringChangedArgs args) =>
            new(UserStringKey: Optional(args.Key));
    }

    public Seq<Guid> ObjectIds =>
        this switch {
            Objects value => value.Ids,
            Selection value => value.Ids,
            Replace value => value.New.ToSeq().Add(value.Old),
            Attributes value => value.Object.ToSeq(),
            PageView value => value.OldActiveDetailId.ToSeq() + value.NewActiveDetailId.ToSeq(),
            _ => Seq<Guid>(),
        };
}

[SmartEnum<int>]
public sealed partial class WatchPhase {
    public static readonly WatchPhase
        ViewModified = new(key: 0, bind: View(h => RhinoView.Modified += h, h => RhinoView.Modified -= h)),
        ViewCreated = new(key: 1, bind: View(h => RhinoView.Create += h, h => RhinoView.Create -= h)),
        ViewDestroyed = new(key: 2, bind: View(h => RhinoView.Destroy += h, h => RhinoView.Destroy -= h)),
        ViewActivated = new(key: 3, bind: View(h => RhinoView.SetActive += h, h => RhinoView.SetActive -= h)),
        ViewRenamed = new(key: 4, bind: View(h => RhinoView.Rename += h, h => RhinoView.Rename -= h)),
        SelectionAdded = new(key: 5, bind: On<RhinoObjectSelectionEventArgs>(h => RhinoDoc.SelectObjects += h, h => RhinoDoc.SelectObjects -= h, static (a, doc) => Selection(a: a, doc: doc))),
        SelectionRemoved = new(key: 6, bind: On<RhinoObjectSelectionEventArgs>(h => RhinoDoc.DeselectObjects += h, h => RhinoDoc.DeselectObjects -= h, static (a, doc) => Selection(a: a, doc: doc))),
        SelectionCleared = new(key: 7, bind: On<RhinoDeselectAllObjectsEventArgs>(h => RhinoDoc.DeselectAllObjects += h, h => RhinoDoc.DeselectAllObjects -= h, static (a, doc) => Gate(eventDoc: a.Document, watched: doc, payload: new WatchPayload.Selection(Ids: Seq<Guid>(), Count: a.ObjectCount)))),
        ObjectAdded = new(key: 8, bind: On<RhinoObjectEventArgs>(h => RhinoDoc.AddRhinoObject += h, h => RhinoDoc.AddRhinoObject -= h, static (a, doc) => Object(a: a, doc: doc))),
        ObjectDeleted = new(key: 9, bind: On<RhinoObjectEventArgs>(h => RhinoDoc.DeleteRhinoObject += h, h => RhinoDoc.DeleteRhinoObject -= h, static (a, doc) => Object(a: a, doc: doc))),
        ObjectReplaced = new(key: 10, bind: On<RhinoReplaceObjectEventArgs>(h => RhinoDoc.ReplaceRhinoObject += h, h => RhinoDoc.ReplaceRhinoObject -= h, static (a, doc) => Gate(eventDoc: a.Document, watched: doc, payload: new WatchPayload.Replace(Old: a.ObjectId, New: Optional(a.NewRhinoObject).Map(static o => o.Id).Filter(static id => id != Guid.Empty))))),
        AttributesModified = new(key: 11, bind: On<RhinoModifyObjectAttributesEventArgs>(h => RhinoDoc.ModifyObjectAttributes += h, h => RhinoDoc.ModifyObjectAttributes -= h, static (a, doc) => Gate(eventDoc: a.Document, watched: doc, payload: new WatchPayload.Attributes(Object: Optional(a.RhinoObject).Map(static o => o.Id))))),
        LayerChanged = new(key: 12, bind: On<LayerTableEventArgs>(h => RhinoDoc.LayerTableEvent += h, h => RhinoDoc.LayerTableEvent -= h, static (a, doc) => Gate(eventDoc: a.Document, watched: doc, payload: new WatchPayload.LayerEvent(Event: a.EventType, Index: a.LayerIndex, Old: Optional(a.OldState), Next: Optional(a.NewState))))),
        ViewProjectionChanged = new(key: 13, bind: Projection(h => DisplayPipeline.ViewportProjectionChanged += h, h => DisplayPipeline.ViewportProjectionChanged -= h)),
        ViewDisplayModeChanged = new(key: 14, bind: On<DisplayModeChangedEventArgs>(h => DisplayPipeline.DisplayModeChanged += h, h => DisplayPipeline.DisplayModeChanged -= h, static (a, doc) => Optional(a.Viewport).Bind(viewport => Gate(eventDoc: a.RhinoDoc, watched: doc, payload: new WatchPayload.DisplayMode(ViewportId: viewport.Id, Old: a.OldDisplayModeId, Next: a.ChangedDisplayModeId))))),
        BeginOpen = new(key: 15, bind: On<DocumentOpenEventArgs>(h => RhinoDoc.BeginOpenDocument += h, h => RhinoDoc.BeginOpenDocument -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: WatchPayload.Document.Open(args: a)))),
        EndOpen = new(key: 16, bind: On<DocumentOpenEventArgs>(h => RhinoDoc.EndOpenDocument += h, h => RhinoDoc.EndOpenDocument -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: WatchPayload.Document.Open(args: a)))),
        BeginSave = new(key: 17, bind: On<DocumentSaveEventArgs>(h => RhinoDoc.BeginSaveDocument += h, h => RhinoDoc.BeginSaveDocument -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: WatchPayload.Document.Save(args: a)))),
        EndSave = new(key: 18, bind: On<DocumentSaveEventArgs>(h => RhinoDoc.EndSaveDocument += h, h => RhinoDoc.EndSaveDocument -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: WatchPayload.Document.Save(args: a)))),
        DocumentClosing = new(key: 19, bind: On<DocumentEventArgs>(h => RhinoDoc.CloseDocument += h, h => RhinoDoc.CloseDocument -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: new WatchPayload.Document()))),
        DocumentCreated = new(key: 20, bind: On<DocumentEventArgs>(h => RhinoDoc.NewDocument += h, h => RhinoDoc.NewDocument -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: new WatchPayload.Document()))),
        ActiveDocumentChanged = new(key: 21, bind: On<DocumentEventArgs>(h => RhinoDoc.ActiveDocumentChanged += h, h => RhinoDoc.ActiveDocumentChanged -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: new WatchPayload.Document()))),
        DocumentPropertiesChanged = new(key: 22, bind: On<DocumentEventArgs>(h => RhinoDoc.DocumentPropertiesChanged += h, h => RhinoDoc.DocumentPropertiesChanged -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: new WatchPayload.Document()))),
        UnitsChanged = new(key: 23, bind: On<UnitsChangedWithScalingEventArgs>(h => RhinoDoc.UnitsChangedWithScaling += h, h => RhinoDoc.UnitsChangedWithScaling -= h, static (a, doc) => Gate(serial: a.DocumentSerialNumber, watched: doc, payload: WatchPayload.Document.Units(args: a)))),
        UserStringChanged = new(key: 24, bind: On<RhinoDoc.UserStringChangedArgs>(h => RhinoDoc.UserStringChanged += h, h => RhinoDoc.UserStringChanged -= h, static (a, doc) => Gate(eventDoc: a.Document, watched: doc, payload: WatchPayload.Document.UserString(args: a)))),
        ObjectUndeleted = new(key: 25, bind: On<RhinoObjectEventArgs>(h => RhinoDoc.UndeleteRhinoObject += h, h => RhinoDoc.UndeleteRhinoObject -= h, static (a, doc) => Object(a: a, doc: doc))),
        ObjectPurged = new(key: 26, bind: On<RhinoObjectEventArgs>(h => RhinoDoc.PurgeRhinoObject += h, h => RhinoDoc.PurgeRhinoObject -= h, static (a, doc) => Object(a: a, doc: doc))),
        PanelShown = new(key: 27, bind: PanelShow(show: true)),
        PanelHidden = new(key: 28, bind: PanelShow(show: false)),
        PanelClosed = new(key: 29, bind: PanelClose()),
        PageSpace = new(key: 30, bind: On<PageViewSpaceChangeEventArgs>(h => RhinoPageView.PageViewSpaceChange += h, h => RhinoPageView.PageViewSpaceChange -= h, static (a, doc) => PageSpacePayload(args: a, watched: doc))),
        PageProperties = new(key: 31, bind: On<PageViewPropertiesChangeEventArgs>(h => RhinoPageView.PageViewPropertiesChange += h, h => RhinoPageView.PageViewPropertiesChange -= h, static (a, doc) => PagePropertiesPayload(args: a, watched: doc)));

    [UseDelegateFromConstructor] internal partial Subscription Bind(WatchTarget target, Func<WatchEnvelope, Fin<Unit>> deliver);

    internal static Fin<Subscription> Subscribe(RhinoDoc document, Seq<WatchPhase> phases, Func<WatchEvent, Fin<Unit>> sink) =>
        WatchBus.Subscribe(target: new WatchTarget.Document(Value: document), spec: new WatchSpec(Sink: sink, Phases: phases));
    private static Func<WatchTarget, Func<WatchEnvelope, Fin<Unit>>, Subscription> On<TArgs>(Action<EventHandler<TArgs>> subscribe, Action<EventHandler<TArgs>> unsubscribe, Func<TArgs, WatchTarget, Option<WatchEnvelope>> project) =>
        (target, deliver) => Subscription.Attach(active: true, subscribe: subscribe, unsubscribe: unsubscribe, handle: (TArgs a) =>
            project(arg1: a, arg2: target).Case switch { WatchEnvelope payload => deliver(arg: payload), _ => Fin.Succ(value: unit) });

    private static Func<WatchTarget, Func<WatchEnvelope, Fin<Unit>>, Subscription> View(Action<EventHandler<ViewEventArgs>> subscribe, Action<EventHandler<ViewEventArgs>> unsubscribe) =>
        On(subscribe: subscribe, unsubscribe: unsubscribe, project: static (ViewEventArgs a, WatchTarget target) => Optional(a.View).Bind(view => Gate(eventDoc: view.Document, watched: target, payload: new WatchPayload.View(Value: view))));

    private static Func<WatchTarget, Func<WatchEnvelope, Fin<Unit>>, Subscription> PanelShow(bool show) =>
        On<global::Rhino.UI.ShowPanelEventArgs>(
            subscribe: h => global::Rhino.UI.Panels.Show += h,
            unsubscribe: h => global::Rhino.UI.Panels.Show -= h,
            project: (a, target) => a.Show == show
                ? Gate(serial: a.DocumentSerialNumber, watched: target, payload: new WatchPayload.Panel(Id: a.PanelId, State: show ? WatchPanelState.Shown : WatchPanelState.Hidden))
                : Option<WatchEnvelope>.None);

    private static Func<WatchTarget, Func<WatchEnvelope, Fin<Unit>>, Subscription> PanelClose() =>
        On<global::Rhino.UI.PanelEventArgs>(
            subscribe: h => global::Rhino.UI.Panels.Closed += h,
            unsubscribe: h => global::Rhino.UI.Panels.Closed -= h,
            project: static (a, target) => Gate(serial: a.DocumentSerialNumber, watched: target, payload: new WatchPayload.Panel(Id: a.PanelId, State: WatchPanelState.Closed)));
    private static Func<WatchTarget, Func<WatchEnvelope, Fin<Unit>>, Subscription> Projection(Action<EventHandler<DrawEventArgs>> subscribe, Action<EventHandler<DrawEventArgs>> unsubscribe) =>
        (target, deliver) => {
            Atom<HashMap<Guid, uint>> seen = Atom(value: HashMap<Guid, uint>());
            return Subscription.Attach(active: true, subscribe: subscribe, unsubscribe: unsubscribe, handle: (DrawEventArgs a) =>
                (Admit(eventDoc: a.RhinoDoc, target: target).IsSome
                    ? Optional(a.Viewport).Bind(vp => Advanced(seen: seen, document: Optional(a.RhinoDoc), target: target, viewport: vp))
                    : Option<WatchEnvelope>.None)
                .Case switch { WatchEnvelope payload => deliver(arg: payload), _ => Fin.Succ(value: unit) });
        };

    private static Option<WatchEnvelope> Advanced(Atom<HashMap<Guid, uint>> seen, Option<RhinoDoc> document, WatchTarget target, RhinoViewport viewport) {
        Guid id = viewport.Id;
        uint counter = viewport.ChangeCounter;
        bool advanced = false;
        _ = seen.Swap(f: m => m.Find(key: id) switch {
            { IsSome: true, Case: uint prior } when prior == counter => m,
            _ => (advanced = true, m.AddOrUpdate(key: id, value: counter)).Item2,
        });
        return advanced
            ? document.Bind(doc => Admit(eventDoc: doc, target: target).Map(scope => new WatchEnvelope(DocumentSerialNumber: scope.Serial, Document: scope.Document, Payload: new WatchPayload.Viewport(Id: id, ChangeCounter: counter))))
            : Option<WatchEnvelope>.None;
    }

    private static Option<WatchEnvelope> Gate(RhinoDoc? eventDoc, WatchTarget watched, WatchPayload payload) =>
        Admit(eventDoc: eventDoc, target: watched).Map(scope => new WatchEnvelope(DocumentSerialNumber: scope.Serial, Document: scope.Document, Payload: payload));

    private static Option<WatchEnvelope> Gate(uint serial, WatchTarget watched, WatchPayload payload) =>
        Admit(serial: serial, target: watched).Map(scope => new WatchEnvelope(DocumentSerialNumber: scope.Serial, Document: scope.Document, Payload: payload));

    private static Option<(uint Serial, Option<RhinoDoc> Document)> Admit(RhinoDoc? eventDoc, WatchTarget target) =>
        eventDoc is RhinoDoc doc ? Admit(serial: doc.RuntimeSerialNumber, target: target) : Option<(uint Serial, Option<RhinoDoc> Document)>.None;

    private static Option<(uint Serial, Option<RhinoDoc> Document)> Admit(uint serial, WatchTarget target) =>
        (serial, target) switch {
            ( > 0, WatchTarget.Document watched) when Optional(watched.Value).Case is RhinoDoc doc && doc.RuntimeSerialNumber == serial =>
                Some((Serial: serial, Document: Optional(RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial)))),
            ( > 0, WatchTarget.AllDocuments) =>
                Some((Serial: serial, Document: Optional(RhinoDoc.FromRuntimeSerialNumber(serialNumber: serial)))),
            _ => Option<(uint Serial, Option<RhinoDoc> Document)>.None,
        };

    private static Option<WatchEnvelope> Selection(RhinoObjectSelectionEventArgs a, WatchTarget doc) =>
        Gate(eventDoc: a.Document, watched: doc, payload: new WatchPayload.Selection(Ids: toSeq(a.RhinoObjects).Map(static o => o.Id), Count: a.RhinoObjectCount));

    private static Option<WatchEnvelope> Object(RhinoObjectEventArgs a, WatchTarget doc) =>
        Gate(eventDoc: a.TheObject?.Document, watched: doc, payload: new WatchPayload.Objects(Ids: Seq(a.ObjectId)));

    private static Option<WatchEnvelope> PageSpacePayload(PageViewSpaceChangeEventArgs args, WatchTarget watched) =>
        Optional(args.PageView).Bind(page =>
            Gate(eventDoc: page.Document, watched: watched, payload: new WatchPayload.PageView(
                SerialNumber: page.RuntimeSerialNumber,
                OldActiveDetailId: OptionalDetail(value: args.OldActiveDetailId),
                NewActiveDetailId: OptionalDetail(value: args.NewActiveDetailId))));

    private static Option<WatchEnvelope> PagePropertiesPayload(PageViewPropertiesChangeEventArgs args, WatchTarget watched) =>
        Gate(serial: args.DocumentSerialNumber, watched: watched, payload: new WatchPayload.PageView(
            SerialNumber: args.PageViewSerialNumber,
            OldActiveDetailId: Option<Guid>.None,
            NewActiveDetailId: Option<Guid>.None));

    private static Option<Guid> OptionalDetail(Guid value) =>
        value == Guid.Empty ? Option<Guid>.None : Some(value);
}

[Union]
public abstract partial record WatchTarget {
    private WatchTarget() { }
    public sealed record Document(RhinoDoc Value) : WatchTarget;
    public sealed record AllDocuments : WatchTarget;
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct WatchEvent(WatchPhase Phase, uint DocumentSerialNumber, Option<RhinoDoc> Document, WatchPayload Payload) {
    public Option<RhinoView> View => Payload is WatchPayload.View value ? Some(value: value.Value) : Option<RhinoView>.None;
    public Option<WatchPayload.Panel> Panel => Payload is WatchPayload.Panel value ? Some(value: value) : Option<WatchPayload.Panel>.None;
    public Option<WatchPayload.PageView> PageView => Payload is WatchPayload.PageView value ? Some(value: value) : Option<WatchPayload.PageView>.None;
    public Seq<Guid> ObjectIds => Payload.ObjectIds;
}

public sealed record WatchSpec(
    Func<WatchEvent, Fin<Unit>> Sink,
    Seq<WatchPhase> Phases = default,
    DeferralPolicy Deferral = DeferralPolicy.Immediate);

internal readonly record struct WatchEnvelope(uint DocumentSerialNumber, Option<RhinoDoc> Document, WatchPayload Payload);

// --- [SERVICES] ---------------------------------------------------------------------------
public static class WatchBus {
    public static Fin<Subscription> Subscribe(WatchTarget target, WatchSpec spec) =>
        from activeTarget in Optional(target).ToFin(Fail: Op.Of(name: nameof(WatchBus)).InvalidInput())
        from activeSpec in Optional(spec).ToFin(Fail: Op.Of(name: nameof(WatchBus)).InvalidInput())
        from sink in Optional(activeSpec.Sink).ToFin(Fail: Op.Of(name: nameof(WatchBus)).InvalidInput())
        let phases = activeSpec.Phases.IsEmpty ? toSeq(WatchPhase.Items) : activeSpec.Phases.Distinct()
        select phases.Fold(
            Subscription.Nothing,
            (all, phase) => all | phase.Bind(
                target: activeTarget,
                deliver: payload => WatchIdle.Deliver(deferral: activeSpec.Deferral, run: () => sink(arg: new WatchEvent(
                    Phase: phase,
                    DocumentSerialNumber: payload.DocumentSerialNumber,
                    Document: payload.Document,
                    Payload: payload.Payload)))));

    public static Fin<Subscription> SubscribeFile(string path, TimeSpan debounce, TimeProvider clock, Func<Fin<Unit>> sink) {
        Op op = Op.Of(name: nameof(SubscribeFile));
        return from activePath in Optional(path)
                   .Filter(static value => !string.IsNullOrWhiteSpace(value: value))
                   .ToFin(Fail: op.InvalidInput())
               from activeClock in Optional(clock).ToFin(Fail: op.InvalidInput())
               from activeSink in Optional(sink).ToFin(Fail: op.InvalidInput())
               from _ in debounce > TimeSpan.Zero
                   ? Fin.Succ(value: unit)
                   : Fin.Fail<Unit>(error: op.InvalidInput())
               from sub in op.Catch(() => AttachFile(path: activePath, debounce: debounce, clock: activeClock, sink: activeSink, op: op))
               select sub;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Subscription owns and disposes the watcher after successful attachment.")]
    private static Fin<Subscription> AttachFile(string path, TimeSpan debounce, TimeProvider clock, Func<Fin<Unit>> sink, Op op) {
        string fullPath = IOPath.GetFullPath(path: path);
        string dir = IOPath.GetDirectoryName(path: fullPath) ?? string.Empty;
        string filter = IOPath.GetFileName(path: fullPath);
        if (string.IsNullOrWhiteSpace(value: dir) || !IODirectory.Exists(path: dir)) return Fin.Fail<Subscription>(error: op.InvalidInput());
        FileSystemWatcher watcher = new(path: dir, filter: filter) {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
        };
        try {
            Atom<DateTimeOffset> lastFired = Atom(value: DateTimeOffset.MinValue);
            Fin<Unit> Fire() {
                DateTimeOffset now = clock.GetUtcNow();
                bool accepted = false;
                _ = lastFired.Swap(f: last => {
                    accepted = now - last >= debounce;
                    return accepted ? now : last;
                });
                return accepted ? sink() : Fin.Succ(value: unit);
            }
            void OnChange(object sender, FileSystemEventArgs args) => _ = UI.RhinoUi.Protect(valid: Fire);
            void OnRename(object sender, RenamedEventArgs args) => _ = UI.RhinoUi.Protect(valid: Fire);
            FileSystemEventHandler change = OnChange;
            RenamedEventHandler rename = OnRename;
            watcher.Changed += change;
            watcher.Created += change;
            watcher.Deleted += change;
            watcher.Renamed += rename;
            watcher.EnableRaisingEvents = true;
            return Fin.Succ(value: Subscription.Watch(
                watcher: watcher,
                detachers: Seq(
                    () => watcher.Changed -= change,
                    () => watcher.Created -= change,
                    () => watcher.Deleted -= change,
                    () => watcher.Renamed -= rename)));
        } catch (IOException ex) {
            watcher.Dispose();
            return Fin.Fail<Subscription>(error: op.InvalidResult(detail: ex.Message));
        } catch (UnauthorizedAccessException ex) {
            watcher.Dispose();
            return Fin.Fail<Subscription>(error: op.InvalidResult(detail: ex.Message));
        }
    }
}

// --- [COMPOSITION] ------------------------------------------------------------------------
public sealed class Subscription : IDisposable {
    private Action? detach;
    private Subscription(Action? detach) => this.detach = detach;

    public static readonly Subscription Nothing = new(detach: null);
    public static Subscription Of(Action detach) => new(detach: detach);
    public static Subscription Of(Seq<Action> detachers) =>
        new(detach: () => detachers.Iter(static run =>
            _ = UI.RhinoUi.Protect(valid: () => { run(); return Fin.Succ(value: unit); })));

    public static Subscription operator |(Subscription a, Subscription b) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        return ReferenceEquals(objA: a, objB: Nothing) ? b
            : ReferenceEquals(objA: b, objB: Nothing) ? a
            : Of(Seq(a.Dispose, b.Dispose));
    }

    public void Dispose() {
        Action? captured = Interlocked.Exchange(location1: ref detach, value: null);
        _ = captured is null ? unit : UI.RhinoUi.Protect(valid: () => { captured(); return Fin.Succ(value: unit); });
    }
    public static Subscription Attach<TArgs>(bool active, Action<EventHandler<TArgs>> subscribe, Action<EventHandler<TArgs>> unsubscribe, Func<TArgs, Fin<Unit>> handle) {
        void Handle(object? sender, TArgs args) => _ = UI.RhinoUi.Protect(valid: () => handle(arg: args));
        EventHandler<TArgs> handler = Handle;   // single delegate instance for symmetric +=/-=
        _ = Op.SideWhen(active, () => subscribe(handler));
        return active ? Of(detach: () => unsubscribe(handler)) : Nothing;
    }
    public static Subscription Watch(FileSystemWatcher watcher, Seq<Action> detachers) =>
        Of(Seq(() => {
            _ = detachers.Iter(static run =>
                _ = UI.RhinoUi.Protect(valid: () => { run(); return Fin.Succ(value: unit); }));
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }));
}

internal sealed class EventDispatcher<TRaw, TEvent> {
    private readonly record struct Entry(Guid Id, Func<TEvent, bool> Filter, Action<TEvent> Sink, DeferralPolicy Deferral);

    private readonly Func<TRaw, (uint Serial, TEvent Event)> project;
    private readonly Action<uint, TEvent> prologue;
    private readonly Action<uint, Action> defer;
    private int hooked;
    private readonly Atom<LanguageExt.HashSet<int>> reentrant = Atom(value: LanguageExt.HashSet<int>.Empty);
    private readonly Atom<HashMap<uint, Seq<Entry>>> bySerial = Atom(value: HashMap<uint, Seq<Entry>>());

    internal EventDispatcher(
        Action<EventHandler<TRaw>> hook,
        Func<TRaw, (uint Serial, TEvent Event)> project,
        Action<uint, TEvent> prologue,
        Action<uint, Action> defer) {
        this.project = project;
        this.prologue = prologue;
        this.defer = defer;
        EnsureHook(hook: hook);
    }

    internal Subscription Register(uint serial, Func<TEvent, bool> filter, Action<TEvent> sink, DeferralPolicy deferral) {
        Entry entry = new(Id: Guid.NewGuid(), Filter: filter, Sink: sink, Deferral: deferral);
        _ = bySerial.Swap(f: map => map.AddOrUpdate(key: serial, Some: existing => existing.Add(value: entry), None: () => Seq(entry)));
        return Subscription.Of(detach: () => Detach(serial: serial, id: entry.Id));
    }

    internal Unit DropSerial(uint serial) {
        _ = bySerial.Swap(f: map => map.Remove(key: serial));
        return unit;
    }

    private void EnsureHook(Action<EventHandler<TRaw>> hook) =>
        _ = Interlocked.CompareExchange(location1: ref hooked, value: 1, comparand: 0) != 0
            ? unit
            : Op.Side(() => hook((_, raw) => OnEvent(raw: raw)));

    private void OnEvent(TRaw raw) {
        int tid = System.Environment.CurrentManagedThreadId;
        _ = reentrant.Value.Contains(key: tid)
            ? unit
            : UI.RhinoUi.Protect(valid: () => { _ = Dispatch(tid: tid, raw: raw); return Fin.Succ(value: unit); });
    }

    private Unit Dispatch(int tid, TRaw raw) {
        _ = reentrant.Swap(f: set => set.Add(key: tid));
        try {
            (uint serial, TEvent ev) = project(arg: raw);
            prologue(arg1: serial, arg2: ev);
            Seq<Entry> subs = bySerial.Value.Find(key: serial).IfNone(noneValue: Seq<Entry>());   // snapshot before iter
            return subs.IsEmpty
                ? unit
                : ignore(subs.Filter(s => s.Filter(arg: ev)).Iter(s => Deliver(serial: serial, entry: s, ev: ev)));
        } finally {
            _ = reentrant.Swap(f: set => set.Remove(key: tid));
        }
    }

    private void Deliver(uint serial, Entry entry, TEvent ev) {
        void Run() => _ = UI.RhinoUi.Protect(valid: () => { entry.Sink(obj: ev); return Fin.Succ(value: unit); });
        _ = entry.Deferral == DeferralPolicy.Idle ? Op.Side(() => defer(arg1: serial, arg2: Run)) : Op.Side(Run);
    }

    private Unit Detach(uint serial, Guid id) {
        _ = bySerial.Swap(f: map => map.Find(key: serial) switch {
            { IsSome: true, Case: Seq<Entry> existing } when existing.Filter(s => s.Id != id).IsEmpty => map.Remove(key: serial),
            { IsSome: true, Case: Seq<Entry> existing } => map.AddOrUpdate(key: serial, value: existing.Filter(s => s.Id != id)),
            _ => map,
        });
        return unit;
    }
}

internal static class WatchIdle {
    private static readonly Atom<Seq<Func<Fin<Unit>>>> Queue = Atom(value: Seq<Func<Fin<Unit>>>());
    private static int hooked;

    internal static Fin<Unit> Deliver(DeferralPolicy deferral, Func<Fin<Unit>> run) =>
        from valid in Optional(run).ToFin(Fail: Op.Of(name: nameof(WatchIdle)).InvalidInput())
        from delivered in deferral switch {
            DeferralPolicy.Idle => Enqueue(run: valid),
            _ => valid(),
        }
        select delivered;

    private static Fin<Unit> Enqueue(Func<Fin<Unit>> run) {
        EnsureHook();
        _ = Queue.Swap(f: current => current.Add(value: run));
        return Fin.Succ(value: unit);
    }

    private static void EnsureHook() {
        if (Interlocked.CompareExchange(location1: ref hooked, value: 1, comparand: 0) != 0) return;
        RhinoApp.Idle += static (_, _) => Drain();
    }

    private static void Drain() {
        Seq<Func<Fin<Unit>>> pending = Queue.Value;
        _ = Queue.Swap(f: current => toSeq(current.Skip(count: pending.Count)));
        _ = pending.Iter(static run => _ = UI.RhinoUi.Protect(valid: run));
    }
}
