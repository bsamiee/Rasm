using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class WatchPhase {
    public static readonly WatchPhase ViewModified = new(0), ViewCreated = new(1), ViewDestroyed = new(2), ViewActivated = new(3),
        SelectionAdded = new(4), SelectionRemoved = new(5), SelectionCleared = new(6),
        ObjectAdded = new(7), ObjectDeleted = new(8), ObjectReplaced = new(9), AttributesModified = new(10), LayerChanged = new(11);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record WatchEvent {
    private WatchEvent() { }

    public sealed record ViewCase(WatchPhase Kind, RhinoView Value) : WatchEvent;
    public sealed record DocumentCase(WatchPhase Kind, RhinoDoc Doc) : WatchEvent;
    public sealed record ObjectsCase(WatchPhase Kind, RhinoDoc Doc, Seq<Guid> Objects) : WatchEvent;
    public sealed record SelectionCase(WatchPhase Kind, RhinoDoc Doc, Seq<Guid> Objects, int Count) : WatchEvent;
    public sealed record ReplaceCase(WatchPhase Kind, RhinoDoc Doc, Guid OldObject, Option<Guid> NewObject) : WatchEvent;
    public sealed record AttributesCase(WatchPhase Kind, RhinoDoc Doc, Option<Guid> Object) : WatchEvent;
    public sealed record LayerCase(WatchPhase Kind, RhinoDoc Doc, LayerTableEventType Event, int Index, Option<Layer> Old, Option<Layer> Next) : WatchEvent;

    public WatchPhase Phase =>
        this switch {
            ViewCase value => value.Kind,
            DocumentCase value => value.Kind,
            ObjectsCase value => value.Kind,
            SelectionCase value => value.Kind,
            ReplaceCase value => value.Kind,
            AttributesCase value => value.Kind,
            LayerCase value => value.Kind,
            _ => WatchPhase.ViewModified,
        };

    public Option<RhinoDoc> Document =>
        this switch {
            ViewCase value => Optional(value.Value.Document),
            DocumentCase value => Some(value.Doc),
            ObjectsCase value => Some(value.Doc),
            SelectionCase value => Some(value.Doc),
            ReplaceCase value => Some(value.Doc),
            AttributesCase value => Some(value.Doc),
            LayerCase value => Some(value.Doc),
            _ => Option<RhinoDoc>.None,
        };

    public Option<RhinoView> View =>
        this switch {
            ViewCase value => Some(value.Value),
            _ => Option<RhinoView>.None,
        };

    public Seq<Guid> ObjectIds =>
        this switch {
            ObjectsCase value => value.Objects,
            SelectionCase value => value.Objects,
            ReplaceCase value => value.NewObject.ToSeq().Add(value.OldObject),
            AttributesCase value => value.Object.ToSeq(),
            _ => Seq<Guid>(),
        };
}

public sealed record UiWatch(Func<WatchEvent, Fin<Unit>> OnEvent, Seq<WatchPhase> Phases) {
    public static UiWatch Of(Func<WatchEvent, Fin<Unit>> onEvent, params WatchPhase[] phases) =>
        new(OnEvent: onEvent, Phases: toSeq(phases).Distinct());

    // RhinoView events carry the live view; RhinoDoc events are gated by document runtime serial so a watch
    // bound to one document ignores parallel-document traffic. AddRhinoObject/DeleteRhinoObject lack a `.Document`
    // member (RhinoObjectEventArgs verified 9.0.26146) — the document is reached through `TheObject.Document`.
    internal Subscription Subscribe(RhinoDoc document) {
        uint serial = document.RuntimeSerialNumber;
        Func<WatchEvent, Fin<Unit>> sink = OnEvent;
        Seq<WatchPhase> phases = Phases;
        bool On(WatchPhase phase) => phases.IsEmpty || phases.Find(value => value == phase).IsSome;
        Fin<Unit> Gate(WatchEvent e) =>
            e.Document.Case switch {
                RhinoDoc doc when doc.RuntimeSerialNumber == serial => sink(arg: e),
                _ => Fin.Succ(value: unit),
            };
        Fin<Unit> View(WatchPhase phase, ViewEventArgs args) =>
            Optional(args.View).Case switch {
                RhinoView view => Gate(e: new WatchEvent.ViewCase(Kind: phase, Value: view)),
                _ => Fin.Succ(value: unit),
            };
        return new Subscription(detachers:
            Subscriptions.Attach<ViewEventArgs>(On(WatchPhase.ViewModified), h => RhinoView.Modified += h, h => RhinoView.Modified -= h, a => View(phase: WatchPhase.ViewModified, args: a))
            + Subscriptions.Attach<ViewEventArgs>(On(WatchPhase.ViewCreated), h => RhinoView.Create += h, h => RhinoView.Create -= h, a => View(phase: WatchPhase.ViewCreated, args: a))
            + Subscriptions.Attach<ViewEventArgs>(On(WatchPhase.ViewDestroyed), h => RhinoView.Destroy += h, h => RhinoView.Destroy -= h, a => View(phase: WatchPhase.ViewDestroyed, args: a))
            + Subscriptions.Attach<ViewEventArgs>(On(WatchPhase.ViewActivated), h => RhinoView.SetActive += h, h => RhinoView.SetActive -= h, a => View(phase: WatchPhase.ViewActivated, args: a))
            + Subscriptions.Attach<RhinoObjectSelectionEventArgs>(On(WatchPhase.SelectionAdded), h => RhinoDoc.SelectObjects += h, h => RhinoDoc.SelectObjects -= h, a => Gate(e: new WatchEvent.SelectionCase(Kind: WatchPhase.SelectionAdded, Doc: a.Document, Objects: toSeq(a.RhinoObjects).Map(static o => o.Id), Count: a.RhinoObjectCount)))
            + Subscriptions.Attach<RhinoObjectSelectionEventArgs>(On(WatchPhase.SelectionRemoved), h => RhinoDoc.DeselectObjects += h, h => RhinoDoc.DeselectObjects -= h, a => Gate(e: new WatchEvent.SelectionCase(Kind: WatchPhase.SelectionRemoved, Doc: a.Document, Objects: toSeq(a.RhinoObjects).Map(static o => o.Id), Count: a.RhinoObjectCount)))
            + Subscriptions.Attach<RhinoDeselectAllObjectsEventArgs>(On(WatchPhase.SelectionCleared), h => RhinoDoc.DeselectAllObjects += h, h => RhinoDoc.DeselectAllObjects -= h, a => Gate(e: new WatchEvent.SelectionCase(Kind: WatchPhase.SelectionCleared, Doc: a.Document, Objects: Seq<Guid>(), Count: a.ObjectCount)))
            + Subscriptions.Attach<RhinoObjectEventArgs>(On(WatchPhase.ObjectAdded), h => RhinoDoc.AddRhinoObject += h, h => RhinoDoc.AddRhinoObject -= h, a => Optional(a.TheObject?.Document).Case switch { RhinoDoc doc => Gate(e: new WatchEvent.ObjectsCase(Kind: WatchPhase.ObjectAdded, Doc: doc, Objects: Seq(a.ObjectId))), _ => Fin.Succ(value: unit) })
            + Subscriptions.Attach<RhinoObjectEventArgs>(On(WatchPhase.ObjectDeleted), h => RhinoDoc.DeleteRhinoObject += h, h => RhinoDoc.DeleteRhinoObject -= h, a => Optional(a.TheObject?.Document).Case switch { RhinoDoc doc => Gate(e: new WatchEvent.ObjectsCase(Kind: WatchPhase.ObjectDeleted, Doc: doc, Objects: Seq(a.ObjectId))), _ => Fin.Succ(value: unit) })
            + Subscriptions.Attach<RhinoReplaceObjectEventArgs>(On(WatchPhase.ObjectReplaced), h => RhinoDoc.ReplaceRhinoObject += h, h => RhinoDoc.ReplaceRhinoObject -= h, a => Gate(e: new WatchEvent.ReplaceCase(Kind: WatchPhase.ObjectReplaced, Doc: a.Document, OldObject: a.ObjectId, NewObject: Optional(a.NewRhinoObject).Map(static o => o.Id).Filter(static id => id != Guid.Empty))))
            + Subscriptions.Attach<RhinoModifyObjectAttributesEventArgs>(On(WatchPhase.AttributesModified), h => RhinoDoc.ModifyObjectAttributes += h, h => RhinoDoc.ModifyObjectAttributes -= h, a => Gate(e: new WatchEvent.AttributesCase(Kind: WatchPhase.AttributesModified, Doc: a.Document, Object: Optional(a.RhinoObject).Map(static o => o.Id))))
            + Subscriptions.Attach<LayerTableEventArgs>(On(WatchPhase.LayerChanged), h => RhinoDoc.LayerTableEvent += h, h => RhinoDoc.LayerTableEvent -= h, a => Gate(e: new WatchEvent.LayerCase(Kind: WatchPhase.LayerChanged, Doc: a.Document, Event: a.EventType, Index: a.LayerIndex, Old: Optional(a.OldState), Next: Optional(a.NewState)))));
    }
}

// --- [SERVICES] ---------------------------------------------------------------------------
// Shared detacher capsule: CAS-idempotent dispose runs every detacher once. Single home for the reactive
// lifecycle primitive (replaces Motion.Detacher + PanelOp.PanelSubscription; reused by the Watch/Panel rails).
internal sealed class Subscription(Seq<Action> detachers) : IDisposable {
    private int disposed;
    public static Subscription Empty { get; } = new(Seq<Action>());
    public void Dispose() => _ = Interlocked.Exchange(ref disposed, 1) == 1 ? unit : detachers.Iter(static detach => detach());
}

internal static class Subscriptions {
    // BOUNDARY ADAPTER — native static events need one delegate identity for symmetric +=/-=; the handler is
    // Protect-wrapped so a faulting sink never escapes the native dispatch. Gated-off yields an empty detacher set.
    internal static Seq<Action> Attach<TArgs>(bool active, Action<EventHandler<TArgs>> subscribe, Action<EventHandler<TArgs>> unsubscribe, Func<TArgs, Fin<Unit>> handle) {
        void Handle(object? sender, TArgs args) => _ = RhinoUi.Protect(valid: () => handle(arg: args));
        EventHandler<TArgs> handler = Handle;   // single delegate instance for symmetric +=/-=
        _ = Op.SideWhen(active, () => subscribe(handler));
        return active ? Seq(() => unsubscribe(handler)) : Seq<Action>();
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static partial class UiIntent {
    // Fire-and-forget: the caller owns the returned IDisposable (long-lived reactive overlays/HUDs/panels).
    public static UiIntent<IDisposable> Watch(UiWatch watch) =>
        OfScope(run: scope => Op.Of(name: nameof(Watch)).Need(watch).Map(valid => (IDisposable)valid.Subscribe(document: scope.Document)), interactive: false);

    // Scoped: subscribe, run the body, auto-detach on exit (mirrors PanelOp.Watch lifetime).
    public static UiIntent<T> Watch<T>(UiWatch watch, Func<Fin<T>> run) =>
        OfScope(run: scope =>
            from valid in Op.Of(name: nameof(Watch)).Need(watch)
            from body in Op.Of(name: nameof(Watch)).Need(run)
            from result in RhinoUi.Protect(valid: () => {
                Subscription box = valid.Subscribe(document: scope.Document);
                // BOUNDARY ADAPTER — native event detach must close after the scoped body exits (including throw).
                try { return body(); } finally { box.Dispose(); }
            })
            select result, interactive: false);
}
