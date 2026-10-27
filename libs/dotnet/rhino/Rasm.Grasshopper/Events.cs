using Grasshopper2;
using Grasshopper2.Components;
using Grasshopper2.Doc;
using Grasshopper2.UI.Canvas;
using Grasshopper2.UI.Flex;
using Grasshopper2.UI.Skinning;
using Grasshopper2.Undo;
using Rasm.RhinoCommon.Events;

namespace Rasm.Grasshopper;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GrasshopperEvents {
    // --- [SETTINGS]
    public static readonly HostEvent<EventArgs> SandBoxChanged =
        Subscriptions.Host<EventHandler, EventArgs>(static h => Settings.SandBoxChanged += h, static h => Settings.SandBoxChanged -= h, static handler => handler.Invoke);

    public static HostEvent<EventArgs> SettingChanged<T>(Settings.Setting<T> setting) =>
        Subscriptions.Host<EventArgs>(h => setting.Changed += h, h => setting.Changed -= h);

    // --- [SKIN]
    public static readonly HostEvent<EventArgs> SkinChanged =
        Subscriptions.Merged(
            Subscriptions.Host<EventHandler, EventArgs>(static h => SkinServer.CurrentChanged += h, static h => SkinServer.CurrentChanged -= h, static handler => handler.Invoke),
            SettingChanged(Settings.DarkMode));

    // --- [CANVAS]
    public static HostEvent<EventArgs> DocumentChanged(Canvas canvas) =>
        Subscriptions.Host<EventHandler, EventArgs>(h => canvas.DocumentChanged += h, h => canvas.DocumentChanged -= h, static handler => handler.Invoke);

    public static HostEvent<EventArgs> DocumentModified(Canvas canvas) =>
        Subscriptions.Host<EventHandler, EventArgs>(h => canvas.DocumentModified += h, h => canvas.DocumentModified -= h, static handler => handler.Invoke);

    public static HostEvent<ProjectionChangedEventArgs> ProjectionChanged(FlexControl control) =>
        Subscriptions.Host<ProjectionChangedEventArgs>(h => control.ProjectionChanged += h, h => control.ProjectionChanged -= h);

    public static HostEvent<WindowSelectionEventArgs> WindowSelection(FlexControl control) =>
        Subscriptions.Host<WindowSelectionEventArgs>(h => control.WindowSelection += h, h => control.WindowSelection -= h);

    public static HostEvent<MouseDwellEventArgs> MouseDwell(FlexControl control) =>
        Subscriptions.Host<MouseDwellEventArgs>(h => control.MouseDwell += h, h => control.MouseDwell -= h);

    public static HostEvent<ControlDrawEventArgs> FlexControlDraw(FlexControl control) =>
        Subscriptions.During<ControlDrawEventArgs>(h => control.Draw += h, h => control.Draw -= h);

    public static HostEvent<PopulateContextMenuEventArgs> PopulateContextMenu(FlexControl control) =>
        Subscriptions.During<PopulateContextMenuEventArgs>(h => control.PopulateContextMenu += h, h => control.PopulateContextMenu -= h);

    // --- [PAINT]
    public static HostEvent<CanvasBackgroundPaintEventArgs> BeforePaintBackground(Canvas canvas) =>
        Subscriptions.During<CanvasBackgroundPaintEventArgs>(h => canvas.BeforePaintBackground += h, h => canvas.BeforePaintBackground -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintBackground(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.AfterPaintBackground += h, h => canvas.AfterPaintBackground -= h);

    public static HostEvent<CanvasPaintEventArgs> BeforePaintGroups(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.BeforePaintGroups += h, h => canvas.BeforePaintGroups -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintGroups(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.AfterPaintGroups += h, h => canvas.AfterPaintGroups -= h);

    public static HostEvent<CanvasPaintEventArgs> BeforePaintWires(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.BeforePaintWires += h, h => canvas.BeforePaintWires -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintWires(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.AfterPaintWires += h, h => canvas.AfterPaintWires -= h);

    public static HostEvent<CanvasPaintEventArgs> BeforePaintObjects(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.BeforePaintObjects += h, h => canvas.BeforePaintObjects -= h);

    public static HostEvent<CanvasPaintEventArgs> AfterPaintObjects(Canvas canvas) =>
        Subscriptions.During<CanvasPaintEventArgs>(h => canvas.AfterPaintObjects += h, h => canvas.AfterPaintObjects -= h);

    // --- [DOCUMENTS]
    public static HostEvent<DocumentModifiedEventArgs> ModifiedChanged(Document document) =>
        Subscriptions.Host<DocumentModifiedEventArgs>(h => document.ModifiedChanged += h, h => document.ModifiedChanged -= h);

    public static HostEvent<DocumentStateEventArgs> StateChanged(Document document) =>
        Subscriptions.Host<DocumentStateEventArgs>(h => document.StateChanged += h, h => document.StateChanged -= h);

    public static HostEvent<BeforeAfterEventArgs<Document, IDocumentParent>> ParentChanged(Document document) =>
        Subscriptions.Host<BeforeAfterEventArgs<Document, IDocumentParent>>(h => document.ParentChanged += h, h => document.ParentChanged -= h);

    // --- [OBJECTS]
    public static HostEvent<AfterAddObjectEventArgs> ObjectAdded(ObjectList objects) =>
        Subscriptions.Host<AfterAddObjectEventArgs>(h => objects.ObjectAdded += h, h => objects.ObjectAdded -= h);

    public static HostEvent<AfterRemoveObjectEventArgs> ObjectRemoved(ObjectList objects) =>
        Subscriptions.Host<AfterRemoveObjectEventArgs>(h => objects.ObjectRemoved += h, h => objects.ObjectRemoved -= h);

    public static HostEvent<ObjectEventArgs> ObjectSelectionChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(h => objects.ObjectSelectionChanged += h, h => objects.ObjectSelectionChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectExpired(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(h => objects.ObjectExpired += h, h => objects.ObjectExpired -= h);

    public static HostEvent<ObjectNameEventArgs> ObjectNameChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectNameEventArgs>(h => objects.ObjectNameChanged += h, h => objects.ObjectNameChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectEnabledChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(h => objects.ObjectEnabledChanged += h, h => objects.ObjectEnabledChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectRelevanceChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(h => objects.ObjectRelevanceChanged += h, h => objects.ObjectRelevanceChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectLayoutChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(h => objects.ObjectLayoutChanged += h, h => objects.ObjectLayoutChanged -= h);

    public static HostEvent<ObjectEventArgs> ObjectDisplayChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectEventArgs>(h => objects.ObjectDisplayChanged += h, h => objects.ObjectDisplayChanged -= h);

    public static HostEvent<ObjectGuidEventArgs> ObjectInstanceIdChanged(ObjectList objects) =>
        Subscriptions.Host<ObjectGuidEventArgs>(h => objects.ObjectInstanceIdChanged += h, h => objects.ObjectInstanceIdChanged -= h);

    public static HostEvent<ObjectDocumentEventArgs> DocumentChanged(IDocumentObject documentObject) =>
        Subscriptions.Host<ObjectDocumentEventArgs>(h => documentObject.DocumentChanged += h, h => documentObject.DocumentChanged -= h);

    public static HostEvent<ModularParameterEventArgs> ModularListShown(ModularList list) =>
        Subscriptions.Host<ModularParameterEventArgs>(h => list.Shown += h, h => list.Shown -= h);

    public static HostEvent<ModularParameterEventArgs> ModularListHidden(ModularList list) =>
        Subscriptions.Host<ModularParameterEventArgs>(h => list.Hidden += h, h => list.Hidden -= h);

    // --- [VALUES]
    public static HostEvent<KeyValueEventArgs> Stored(KeyedValues store) =>
        Subscriptions.Merged(
            Subscriptions.Host<KeyValueEventArgs>(h => store.ValueChanged += h, h => store.ValueChanged -= h),
            Subscriptions.Host<KeyValueEventArgs>(h => store.ValueDeleted += h, h => store.ValueDeleted -= h));

    // --- [SOLUTIONS]
    public static readonly HostEvent<EventArgs> EnabledChanged =
        Subscriptions.Host<EventHandler, EventArgs>(static h => SolutionServer.EnabledChanged += h, static h => SolutionServer.EnabledChanged -= h, static handler => handler.Invoke);

    public static HostEvent<SolutionIdEventArgs> SolutionAboutToStart(SolutionServer server) =>
        Subscriptions.Host<SolutionIdEventArgs>(h => server.SolutionAboutToStart += h, h => server.SolutionAboutToStart -= h);

    public static HostEvent<SolutionEventArgs> SolutionStarted(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(h => server.SolutionStarted += h, h => server.SolutionStarted -= h);

    public static HostEvent<SolutionEventArgs> SolutionStopped(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(h => server.SolutionStopped += h, h => server.SolutionStopped -= h);

    public static HostEvent<SolutionEventArgs> SolutionCancelled(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(h => server.SolutionCancelled += h, h => server.SolutionCancelled -= h);

    public static HostEvent<SolutionEventArgs> SolutionCompleted(SolutionServer server) =>
        Subscriptions.Host<SolutionEventArgs>(h => server.SolutionCompleted += h, h => server.SolutionCompleted -= h);

    public static HostEvent<SolutionExceptionEventArgs> SolutionFaulted(SolutionServer server) =>
        Subscriptions.Host<SolutionExceptionEventArgs>(h => server.SolutionFaulted += h, h => server.SolutionFaulted -= h);

    public static HostEvent<BeforeAfterEventArgs<Solution, SolutionPhase>> StateChanged(Solution solution) =>
        Subscriptions.Host<BeforeAfterEventArgs<Solution, SolutionPhase>>(h => solution.StateChanged += h, h => solution.StateChanged -= h);

    // --- [UNDO]
    public static HostEvent<UndoEventArgs> Undone(History history) =>
        Subscriptions.Host<UndoEventArgs>(h => history.Undone += h, h => history.Undone -= h);

    public static HostEvent<UndoEventArgs> Redone(History history) =>
        Subscriptions.Host<UndoEventArgs>(h => history.Redone += h, h => history.Redone -= h);

    public static HostEvent<UndoEventArgs> HistoryModified(History history) =>
        Subscriptions.Host<UndoEventArgs>(h => history.Modified += h, h => history.Modified -= h);

    public static HostEvent<UndoNodeEventArgs> NodeAdded(History history) =>
        Subscriptions.Host<UndoNodeEventArgs>(h => history.NodeAdded += h, h => history.NodeAdded -= h);

    public static HostEvent<UndoNodeEventArgs> NodeRemoved(History history) =>
        Subscriptions.Host<UndoNodeEventArgs>(h => history.NodeRemoved += h, h => history.NodeRemoved -= h);

    public static HostEvent<UndoNodeEventArgs> NodeMerged(History history) =>
        Subscriptions.Host<UndoNodeEventArgs>(h => history.NodeMerged += h, h => history.NodeMerged -= h);

    public static HostEvent<UndoNodeMovedEventArgs> NodeMoved(History history) =>
        Subscriptions.Host<UndoNodeMovedEventArgs>(h => history.NodeMoved += h, h => history.NodeMoved -= h);
}
