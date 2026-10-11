using Grasshopper2;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Rasm.Rhino.Events;

namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperEvents {
    // --- [SETTINGS]
    public static readonly HostEvent<EventArgs> SandBoxChanged =
        Subscriptions.Host<EventHandler, EventArgs>(typeof(Settings), static h => Settings.SandBoxChanged += h, static h => Settings.SandBoxChanged -= h, static handler => handler.Invoke);

    public static HostEvent<EventArgs> SettingChanged<T>(Settings.Setting<T> setting) =>
        Subscriptions.Host<EventArgs>(typeof(Settings), h => setting.Changed += h, h => setting.Changed -= h, setting.Name);

    // --- [SKIN]
    public static readonly HostEvent<EventArgs> SkinChanged =
        Subscriptions.Host(typeof(SkinServer), static () => Seq(
            Subscriptions.Host<EventHandler, EventArgs>(typeof(SkinServer), static h => SkinServer.CurrentChanged += h, static h => SkinServer.CurrentChanged -= h, static handler => handler.Invoke, nameof(SkinServer.CurrentChanged)),
            SettingChanged(Settings.DarkMode)));

    // --- [CANVAS]
    public static HostEvent<EventArgs> DocumentChanged(Canvas canvas) =>
        Subscriptions.Host<EventHandler, EventArgs>(typeof(Canvas), h => canvas.DocumentChanged += h, h => canvas.DocumentChanged -= h, static handler => handler.Invoke);

    public static HostEvent<EventArgs> DocumentModified(Canvas canvas) =>
        Subscriptions.Host<EventHandler, EventArgs>(typeof(Canvas), h => canvas.DocumentModified += h, h => canvas.DocumentModified -= h, static handler => handler.Invoke);

    public static HostEvent<ProjectionChangedEventArgs> ProjectionChanged(FlexControl control) =>
        Subscriptions.Host<ProjectionChangedEventArgs>(typeof(FlexControl), h => control.ProjectionChanged += h, h => control.ProjectionChanged -= h);

    public static HostEvent<WindowSelectionEventArgs> WindowSelection(FlexControl control) =>
        Subscriptions.Host<WindowSelectionEventArgs>(typeof(FlexControl), h => control.WindowSelection += h, h => control.WindowSelection -= h);

    public static HostEvent<MouseDwellEventArgs> MouseDwell(FlexControl control) =>
        Subscriptions.Host<MouseDwellEventArgs>(typeof(FlexControl), h => control.MouseDwell += h, h => control.MouseDwell -= h);

    public static HostEvent<ControlDrawEventArgs> FlexControlDraw(FlexControl control) =>
        Subscriptions.Host<ControlDrawEventArgs>(typeof(FlexControl), h => control.Draw += h, h => control.Draw -= h, nameof(FlexControl.Draw));

    public static HostEvent<PopulateContextMenuEventArgs> PopulateContextMenu(FlexControl control) =>
        Subscriptions.Host<PopulateContextMenuEventArgs>(typeof(FlexControl), h => control.PopulateContextMenu += h, h => control.PopulateContextMenu -= h);

    // --- [PAINT]
    public static HostEvent<CanvasBackgroundPaintEventArgs> BeforePaintBackground(Canvas canvas) =>
        Subscriptions.Host<CanvasBackgroundPaintEventArgs>(typeof(Canvas), h => canvas.BeforePaintBackground += h, h => canvas.BeforePaintBackground -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintBackground(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.AfterPaintBackground += h, h => canvas.AfterPaintBackground -= h);

    public static HostEvent<CanvasPaintEventArgs> BeforePaintGroups(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.BeforePaintGroups += h, h => canvas.BeforePaintGroups -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintGroups(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.AfterPaintGroups += h, h => canvas.AfterPaintGroups -= h);

    public static HostEvent<CanvasPaintEventArgs> BeforePaintWires(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.BeforePaintWires += h, h => canvas.BeforePaintWires -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintWires(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.AfterPaintWires += h, h => canvas.AfterPaintWires -= h);

    public static HostEvent<CanvasPaintEventArgs> BeforePaintObjects(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.BeforePaintObjects += h, h => canvas.BeforePaintObjects -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintObjects(Canvas canvas) =>
        Subscriptions.Host<CanvasPaintEventArgs>(typeof(Canvas), h => canvas.AfterPaintObjects += h, h => canvas.AfterPaintObjects -= h);

    // --- [DOCUMENTS]
    public static HostEvent<DocumentModifiedEventArgs> ModifiedChanged(Document document) =>
        Subscriptions.Host<DocumentModifiedEventArgs>(typeof(Document), h => document.ModifiedChanged += h, h => document.ModifiedChanged -= h);

    public static HostEvent<DocumentStateEventArgs> StateChanged(Document document) =>
        Subscriptions.Host<DocumentStateEventArgs>(typeof(Document), h => document.StateChanged += h, h => document.StateChanged -= h);

    public static HostEvent<BeforeAfterEventArgs<Document, IDocumentParent>> ParentChanged(Document document) =>
        Subscriptions.Host<BeforeAfterEventArgs<Document, IDocumentParent>>(typeof(Document), h => document.ParentChanged += h, h => document.ParentChanged -= h);

    // --- [OBJECTS]
    public static HostEvent<AfterAddObjectEventArgs> ObjectAdded(ObjectList objects) =>
        Subscriptions.Host<AfterAddObjectEventArgs>(typeof(ObjectList), h => objects.ObjectAdded += h, h => objects.ObjectAdded -= h);

    public static HostEvent<AfterRemoveObjectEventArgs> ObjectRemoved(ObjectList objects) =>
        Subscriptions.Host<AfterRemoveObjectEventArgs>(typeof(ObjectList), h => objects.ObjectRemoved += h, h => objects.ObjectRemoved -= h);

    public static HostEvent<ObjectEventArgs> ObjectSelectionChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(typeof(ObjectList), h => objects.ObjectSelectionChanged += h, h => objects.ObjectSelectionChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectExpired(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(typeof(ObjectList), h => objects.ObjectExpired += h, h => objects.ObjectExpired -= h);

    public static HostEvent<ObjectNameEventArgs> ObjectNameChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectNameEventArgs>(typeof(ObjectList), h => objects.ObjectNameChanged += h, h => objects.ObjectNameChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectEnabledChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(typeof(ObjectList), h => objects.ObjectEnabledChanged += h, h => objects.ObjectEnabledChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectRelevanceChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(typeof(ObjectList), h => objects.ObjectRelevanceChanged += h, h => objects.ObjectRelevanceChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectLayoutChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(typeof(ObjectList), h => objects.ObjectLayoutChanged += h, h => objects.ObjectLayoutChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectDisplayChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(typeof(ObjectList), h => objects.ObjectDisplayChanged += h, h => objects.ObjectDisplayChanged -= h);

    public static HostEvent<ObjectGuidEventArgs> ObjectInstanceIdChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectGuidEventArgs>(typeof(ObjectList), h => objects.ObjectInstanceIdChanged += h, h => objects.ObjectInstanceIdChanged -= h);

    public static HostEvent<ObjectDocumentEventArgs> DocumentChanged(IDocumentObject documentObject) =>
        Subscriptions.Host<ObjectDocumentEventArgs>(typeof(IDocumentObject), h => documentObject.DocumentChanged += h, h => documentObject.DocumentChanged -= h);

    public static HostEvent<ModularParameterEventArgs> ModularListShown(ModularList list) =>
        Subscriptions.Host<ModularParameterEventArgs>(typeof(ModularList), h => list.Shown += h, h => list.Shown -= h, nameof(ModularList.Shown));

    public static HostEvent<ModularParameterEventArgs> ModularListHidden(ModularList list) =>
        Subscriptions.Host<ModularParameterEventArgs>(typeof(ModularList), h => list.Hidden += h, h => list.Hidden -= h, nameof(ModularList.Hidden));

    // --- [VALUES]
    public static HostEvent<KeyValueEventArgs> Stored(KeyedValues store) =>
        Subscriptions.Host(typeof(KeyedValues), () => Seq(
            Subscriptions.Host<KeyValueEventArgs>(typeof(KeyedValues), h => store.ValueChanged += h, h => store.ValueChanged -= h, nameof(KeyedValues.ValueChanged)),
            Subscriptions.Host<KeyValueEventArgs>(typeof(KeyedValues), h => store.ValueDeleted += h, h => store.ValueDeleted -= h, nameof(KeyedValues.ValueDeleted))));

    // --- [SOLUTIONS]
    public static readonly HostEvent<EventArgs> EnabledChanged =
        Subscriptions.Host<EventHandler, EventArgs>(typeof(SolutionServer), static h => SolutionServer.EnabledChanged += h, static h => SolutionServer.EnabledChanged -= h, static handler => handler.Invoke);

    public static HostEvent<SolutionIdEventArgs> SolutionAboutToStart(SolutionServer server) =>
        Subscriptions.Host<SolutionIdEventArgs>(typeof(SolutionServer), h => server.SolutionAboutToStart += h, h => server.SolutionAboutToStart -= h);

    public static HostEvent<SolutionEventArgs> SolutionStarted(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(typeof(SolutionServer), h => server.SolutionStarted += h, h => server.SolutionStarted -= h);

    public static HostEvent<SolutionEventArgs> SolutionStopped(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(typeof(SolutionServer), h => server.SolutionStopped += h, h => server.SolutionStopped -= h);

    public static HostEvent<SolutionEventArgs> SolutionCancelled(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(typeof(SolutionServer), h => server.SolutionCancelled += h, h => server.SolutionCancelled -= h);

    public static HostEvent<SolutionEventArgs> SolutionCompleted(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(typeof(SolutionServer), h => server.SolutionCompleted += h, h => server.SolutionCompleted -= h);

    public static HostEvent<SolutionExceptionEventArgs> SolutionFaulted(SolutionServer server) =>
        Subscriptions.Host<SolutionExceptionEventArgs>(typeof(SolutionServer), h => server.SolutionFaulted += h, h => server.SolutionFaulted -= h);

    public static HostEvent<BeforeAfterEventArgs<Solution, SolutionPhase>> StateChanged(Solution solution) =>
        Subscriptions.Host<BeforeAfterEventArgs<Solution, SolutionPhase>>(typeof(Solution), h => solution.StateChanged += h, h => solution.StateChanged -= h);

    // --- [UNDO]
    public static HostEvent<UndoEventArgs> Undone(History history) =>
        Subscriptions.Host<UndoEventArgs>(typeof(History), h => history.Undone += h, h => history.Undone -= h);

    public static HostEvent<UndoEventArgs> Redone(History history) =>
        Subscriptions.Host<UndoEventArgs>(typeof(History), h => history.Redone += h, h => history.Redone -= h);

    public static HostEvent<UndoEventArgs> HistoryModified(History history) =>
        Subscriptions.Host<UndoEventArgs>(typeof(History), h => history.Modified += h, h => history.Modified -= h, nameof(History.Modified));

    public static HostEvent<UndoNodeEventArgs> NodeAdded(History history) =>
        Subscriptions.Host<UndoNodeEventArgs>(typeof(History), h => history.NodeAdded += h, h => history.NodeAdded -= h);

    public static HostEvent<UndoNodeEventArgs> NodeRemoved(History history) =>
        Subscriptions.Host<UndoNodeEventArgs>(typeof(History), h => history.NodeRemoved += h, h => history.NodeRemoved -= h);

    public static HostEvent<UndoNodeEventArgs> NodeMerged(History history) =>
        Subscriptions.Host<UndoNodeEventArgs>(typeof(History), h => history.NodeMerged += h, h => history.NodeMerged -= h);

    public static HostEvent<UndoNodeMovedEventArgs> NodeMoved(History history) =>
        Subscriptions.Host<UndoNodeMovedEventArgs>(typeof(History), h => history.NodeMoved += h, h => history.NodeMoved -= h);
}
