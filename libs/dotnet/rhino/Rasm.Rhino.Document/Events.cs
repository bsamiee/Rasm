using System.Drawing;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Input.Custom;
using Rhino.Render;
using Rhino.Runtime;
using Rhino.UI;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Document;

// --- [TYPES] ---------------------------------------------------------------------------
public enum EventCategory { Lifecycle = 0, Structure = 1, Selection = 2, Tables = 3, Screen = 4, Draw = 5, Panels = 6, Commands = 7, Application = 8, Render = 9 }

public enum ComponentTransition { Added = 0, Deleted = 1, Undeleted = 2, Modified = 3, Sorted = 4, Current = 5 }

public enum UndoRedoKind {
    BeforeBeginRecording = 0,
    BeginRecording = 1,
    BeforeEndRecording = 2,
    EndRecording = 3,
    BeginUndo = 4,
    EndUndo = 5,
    BeginRedo = 6,
    EndRedo = 7,
    PurgeRecord = 8,
    Other = 9,
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TableRowState {
    public sealed record Present(Guid Id, Option<string> Name) : TableRowState;

    public sealed record Deleted(Guid Id, Option<string> DeletedName) : TableRowState;
}

public sealed record RenderAssignment(bool IsLayer, Guid TargetId, Guid Previous, Guid Current);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record OptionState {
    public sealed record Toggle(bool Value) : OptionState;

    public sealed record Number(double Value) : OptionState;

    public sealed record Integer(int Value) : OptionState;

    public sealed record String(string Value) : OptionState;

    public sealed record Color(System.Drawing.Color Value) : OptionState;

    public sealed record List(int Index) : OptionState;
}

public sealed record OptionSelection(
    int Index,
    CommandLineOptionType OptionType,
    string EnglishName,
    string LocalName,
    Seq<string> ListOptions,
    Option<(string Off, string On)> ToggleValues,
    Option<OptionState> CurrentValue) {
    public static OptionSelection Read(CommandLineOption option, Option<OptionState> held) =>
        new(
            option.Index,
            option.OptionType,
            option.EnglishName,
            option.LocalName,
            option.OptionType == CommandLineOptionType.List ? toSeq(option.ListOptions(english: false)) : Seq<string>(),
            option.OptionType == CommandLineOptionType.Toggle ? Some(Toggles(option)) : Option<(string Off, string On)>.None,
            held || Current(option));

    private static Option<OptionState> Current(CommandLineOption option) =>
        option.OptionType switch {
            CommandLineOptionType.Toggle => Optional(option.CurrentToggleValue).Map<OptionState>(static on => new OptionState.Toggle(on)),
            CommandLineOptionType.Number => Some<OptionState>(new OptionState.Number(option.CurrentNumericValue)),
            CommandLineOptionType.List => Some<OptionState>(new OptionState.List(option.CurrentListOptionIndex)),
            CommandLineOptionType.Simple => Answers.Present(option.StringOptionValue).Map<OptionState>(static text => new OptionState.String(text)),
            _ => Option<OptionState>.None,
        };

    private static (string Off, string On) Toggles(CommandLineOption option) {
        option.ToggleValues(english: false, out string off, out string on);
        return (off, on);
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EventData {
    public sealed record Signal() : EventData;

    public sealed record Opened(Option<string> File, bool Merge, bool Reference) : EventData;

    public sealed record Saved(Option<string> File, bool ExportSelected) : EventData;

    public sealed record Active(Option<uint> Serial) : EventData;

    public sealed record UnitsScaled(double Scale) : EventData;

    public sealed record UserString(string Key) : EventData;

    public sealed record WorksessionEdit(uint ModelSerial, string File, RhinoDoc.WorksessionFileChangeKind Change) : EventData;

    public sealed record Object(Guid Id) : EventData;

    public sealed record Replaced(Guid Old, Option<Guid> New) : EventData;

    public sealed record AttributeEdit(Guid Id) : EventData;

    public sealed record TransformStarted(
        uint EventId,
        Transform Xform,
        bool Copies,
        Seq<(Guid Id, uint Serial)> Subjects,
        Seq<(Guid Id, uint Serial)> Grips,
        Seq<(Guid Id, uint Serial)> GripOwners) : EventData;

    public sealed record TransformEnded(uint EventId, Seq<(Guid Id, uint Serial)> Subjects) : EventData;

    public sealed record Selection(Seq<Guid> Ids, int ItemCount) : EventData;

    public sealed record SelectionCleared(int ItemCount) : EventData;

    public sealed record Component(TableKind Kind, int Index, ComponentTransition Transition, Option<TableRowState> Previous, Option<TableRowState> Current) : EventData;

    public sealed record ContentChange(TableKind Kind, RhinoDoc.RenderContentTableEventType Change, Option<RenderAssignment> Assignment) : EventData;

    public sealed record MappingChange(RhinoDoc.TextureMappingEventType Change, Option<Guid> Current) : EventData;

    public sealed record View(uint ViewSerial, Guid MainViewportId, bool IsPage) : EventData;

    public sealed record ViewportProjection(Guid ViewportId, uint ChangeCounter) : EventData;

    public sealed record DisplayMode(Guid ViewportId, Guid Old, Guid Next) : EventData;

    public sealed record Frame(Guid ViewportId, uint ChangeCounter, Option<uint> ViewSerial, Option<(Guid Id, uint Serial)> Subject) : EventData;

    public sealed record FrameBuffer() : EventData;

    public sealed record Panel(Guid PanelId, Option<bool> Shown) : EventData;

    public sealed record Command(Guid Id, string EnglishName, string LocalName, Option<string> HelpUrl, Option<string> PlugInName, bool IsHiddenFromUser, Result Result) : EventData;

    public sealed record UndoRedo(Guid CommandId, uint UndoSerial, UndoRedoKind Kind) : EventData;

    public sealed record CommandPrompt(string Prompt, string PromptDefault, Seq<OptionSelection> Options) : EventData;

    public sealed record LicenseState(bool CallingRhinoCommonAllowed) : EventData;

    public sealed record Appearance(bool RunningInDarkMode) : EventData;

    public sealed record ExceptionReport(string Source, Exception Exception) : EventData;

    public sealed record CloudLog(HostUtils.LogMessageType MessageType, string Class, string Description, string Text) : EventData;

    public sealed record Content(Guid ContentId, RenderContentChangeReason Reason) : EventData;

    public sealed record EnvironmentUsage(RenderSettings.EnvironmentUsage Usage) : EventData;

    public sealed record ContentEdit(Guid ContentId, RenderContent.ChangeContexts Context, Option<Guid> Old) : EventData;

    public sealed record ContentField(Guid ContentId, string Field, RenderContent.ChangeContexts Context) : EventData;

    public sealed record Preview(Bitmap Image, int Width, int Height, Utilities.PreviewQuality Quality) : EventData;

    public sealed record RenderProperty(int Context) : EventData;
}

public readonly record struct DocEvent(EventKind Kind, Option<uint> DocumentSerial, EventData Data);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EventScope {
    public sealed record Only(uint Serial) : EventScope;

    public sealed record Any() : EventScope;
}

[SmartEnum<string>]
public sealed partial class EventKind {
    // --- [LIFECYCLE]
    public static readonly EventKind BeginOpenDocument = new(
        nameof(BeginOpenDocument),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentOpenEventArgs>(sink, kind, static h => RhinoDoc.BeginOpenDocument += h, static h => RhinoDoc.BeginOpenDocument -= h, Events.Opened));

    public static readonly EventKind EndOpenDocument = new(
        nameof(EndOpenDocument),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentOpenEventArgs>(sink, kind, static h => RhinoDoc.EndOpenDocument += h, static h => RhinoDoc.EndOpenDocument -= h, Events.Opened));

    public static readonly EventKind EndOpenDocumentInitialViewUpdate = new(
        nameof(EndOpenDocumentInitialViewUpdate),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentOpenEventArgs>(
            sink, kind, static h => RhinoDoc.EndOpenDocumentInitialViewUpdate += h, static h => RhinoDoc.EndOpenDocumentInitialViewUpdate -= h, Events.Opened));

    public static readonly EventKind BeginSaveDocument = new(
        nameof(BeginSaveDocument),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentSaveEventArgs>(sink, kind, static h => RhinoDoc.BeginSaveDocument += h, static h => RhinoDoc.BeginSaveDocument -= h, Events.Saved));

    public static readonly EventKind EndSaveDocument = new(
        nameof(EndSaveDocument),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentSaveEventArgs>(sink, kind, static h => RhinoDoc.EndSaveDocument += h, static h => RhinoDoc.EndSaveDocument -= h, Events.Saved));

    public static readonly EventKind CloseDocument = new(
        nameof(CloseDocument),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentEventArgs>(sink, kind, static h => RhinoDoc.CloseDocument += h, static h => RhinoDoc.CloseDocument -= h, Events.Signal));

    public static readonly EventKind NewDocument = new(
        nameof(NewDocument),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentEventArgs>(sink, kind, static h => RhinoDoc.NewDocument += h, static h => RhinoDoc.NewDocument -= h, Events.Signal));

    public static readonly EventKind ActiveDocumentChanged = new(
        nameof(ActiveDocumentChanged),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentEventArgs>(
            sink, kind, static h => RhinoDoc.ActiveDocumentChanged += h, static h => RhinoDoc.ActiveDocumentChanged -= h,
            static args => (Answers.Present(args.DocumentSerialNumber), new EventData.Active(Answers.Present(args.DocumentSerialNumber)))));

    public static readonly EventKind DocumentPropertiesChanged = new(
        nameof(DocumentPropertiesChanged),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<DocumentEventArgs>(
            sink, kind, static h => RhinoDoc.DocumentPropertiesChanged += h, static h => RhinoDoc.DocumentPropertiesChanged -= h, Events.Signal));

    public static readonly EventKind UnitsChangedWithScaling = new(
        nameof(UnitsChangedWithScaling),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<UnitsChangedWithScalingEventArgs>(
            sink, kind, static h => RhinoDoc.UnitsChangedWithScaling += h, static h => RhinoDoc.UnitsChangedWithScaling -= h,
            static args => (Answers.Present(args.DocumentSerialNumber), new EventData.UnitsScaled(args.Scale))));

    public static readonly EventKind UserStringChanged = new(
        nameof(UserStringChanged),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<RhinoDoc.UserStringChangedArgs>(
            sink, kind, static h => RhinoDoc.UserStringChanged += h, static h => RhinoDoc.UserStringChanged -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.UserString(args.Key))));

    public static readonly EventKind WorksessionFileChanged = new(
        nameof(WorksessionFileChanged),
        EventCategory.Lifecycle,
        static (kind, sink) => Events.Host<RhinoDoc.WorksessionFileChangedEventArgs>(
            sink, kind, static h => RhinoDoc.WorksessionFileChanged += h, static h => RhinoDoc.WorksessionFileChanged -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.WorksessionEdit(args.WorksessionModelRuntimeSerialNumber, args.FilePath, args.ChangeKind))));

    // --- [STRUCTURE]
    public static readonly EventKind AddRhinoObject = new(
        nameof(AddRhinoObject),
        EventCategory.Structure,
        static (kind, sink) => Events.Filtered<RhinoObjectEventArgs>(sink, kind, static h => RhinoDoc.AddRhinoObject += h, static h => RhinoDoc.AddRhinoObject -= h, Events.Structure));

    public static readonly EventKind DeleteRhinoObject = new(
        nameof(DeleteRhinoObject),
        EventCategory.Structure,
        static (kind, sink) => Events.Filtered<RhinoObjectEventArgs>(sink, kind, static h => RhinoDoc.DeleteRhinoObject += h, static h => RhinoDoc.DeleteRhinoObject -= h, Events.Structure));

    public static readonly EventKind ReplaceRhinoObject = new(
        nameof(ReplaceRhinoObject),
        EventCategory.Structure,
        static (kind, sink) => Events.Host<RhinoReplaceObjectEventArgs>(
            sink, kind, static h => RhinoDoc.ReplaceRhinoObject += h, static h => RhinoDoc.ReplaceRhinoObject -= h,
            static args => (
                Answers.Present(args.DocumentSerialNumber),
                new EventData.Replaced(args.ObjectId, Optional(args.NewRhinoObject).Map(static replacement => replacement.Id)))));

    public static readonly EventKind UndeleteRhinoObject = new(
        nameof(UndeleteRhinoObject),
        EventCategory.Structure,
        static (kind, sink) => Events.Filtered<RhinoObjectEventArgs>(sink, kind, static h => RhinoDoc.UndeleteRhinoObject += h, static h => RhinoDoc.UndeleteRhinoObject -= h, Events.Structure));

    public static readonly EventKind PurgeRhinoObject = new(
        nameof(PurgeRhinoObject),
        EventCategory.Structure,
        static (kind, sink) => Events.Filtered<RhinoObjectEventArgs>(sink, kind, static h => RhinoDoc.PurgeRhinoObject += h, static h => RhinoDoc.PurgeRhinoObject -= h, Events.Structure));

    public static readonly EventKind ModifyObjectAttributes = new(
        nameof(ModifyObjectAttributes),
        EventCategory.Structure,
        static (kind, sink) => Events.Host<RhinoModifyObjectAttributesEventArgs>(
            sink, kind, static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h,
            static args => (Answers.Present(args.DocumentSerialNumber), new EventData.AttributeEdit(args.RhinoObject.Id))));

    public static readonly EventKind TransformObjects = new(nameof(TransformObjects), EventCategory.Structure, static (_, sink) => Events.Transforms(sink));

    // --- [SELECTION]
    public static readonly EventKind SelectObjects = new(
        nameof(SelectObjects),
        EventCategory.Selection,
        static (kind, sink) => Events.Host<RhinoObjectSelectionEventArgs>(sink, kind, static h => RhinoDoc.SelectObjects += h, static h => RhinoDoc.SelectObjects -= h, Events.Selection));

    public static readonly EventKind DeselectObjects = new(
        nameof(DeselectObjects),
        EventCategory.Selection,
        static (kind, sink) => Events.Host<RhinoObjectSelectionEventArgs>(sink, kind, static h => RhinoDoc.DeselectObjects += h, static h => RhinoDoc.DeselectObjects -= h, Events.Selection));

    public static readonly EventKind DeselectAllObjects = new(
        nameof(DeselectAllObjects),
        EventCategory.Selection,
        static (kind, sink) => Events.Host<RhinoDeselectAllObjectsEventArgs>(
            sink, kind, static h => RhinoDoc.DeselectAllObjects += h, static h => RhinoDoc.DeselectAllObjects -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.SelectionCleared(args.ObjectCount))));

    // --- [TABLES]
    public static readonly EventKind LayerTableEvent = new(
        nameof(LayerTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<LayerTableEventArgs>(
            sink, kind, static h => RhinoDoc.LayerTableEvent += h, static h => RhinoDoc.LayerTableEvent -= h,
            static args => Events.Table(args.Document, TableKind.Layers, args.LayerIndex, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind MaterialTableEvent = new(
        nameof(MaterialTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<MaterialTableEventArgs>(
            sink, kind, static h => RhinoDoc.MaterialTableEvent += h, static h => RhinoDoc.MaterialTableEvent -= h,
            static args => Events.Table(
                args.Document, TableKind.Materials, args.Index, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldSettings), () => Events.RowState(args.Document.Materials[args.Index]))));

    public static readonly EventKind GroupTableEvent = new(
        nameof(GroupTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<GroupTableEventArgs>(
            sink, kind, static h => RhinoDoc.GroupTableEvent += h, static h => RhinoDoc.GroupTableEvent -= h,
            static args => Events.Table(args.Document, TableKind.Groups, args.GroupIndex, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind LinetypeTableEvent = new(
        nameof(LinetypeTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<LinetypeTableEventArgs>(
            sink, kind, static h => RhinoDoc.LinetypeTableEvent += h, static h => RhinoDoc.LinetypeTableEvent -= h,
            static args => Events.Table(args.Document, TableKind.Linetypes, args.LinetypeIndex, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind LightTableEvent = new(
        nameof(LightTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<LightTableEventArgs>(
            sink, kind, static h => RhinoDoc.LightTableEvent += h, static h => RhinoDoc.LightTableEvent -= h,
            static args => Events.Table(
                args.Document, TableKind.Lights, args.LightIndex, TransitionMapper.ToTransition(args.EventType), () => Events.LightState(args.OldState), () => Events.LightState(args.NewState?.LightGeometry))));

    public static readonly EventKind DimensionStyleTableEvent = new(
        nameof(DimensionStyleTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<DimStyleTableEventArgs>(
            sink, kind, static h => RhinoDoc.DimensionStyleTableEvent += h, static h => RhinoDoc.DimensionStyleTableEvent -= h,
            static args => Events.Table(args.Document, TableKind.DimStyles, args.Index, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind InstanceDefinitionTableEvent = new(
        nameof(InstanceDefinitionTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<InstanceDefinitionTableEventArgs>(
            sink, kind, static h => RhinoDoc.InstanceDefinitionTableEvent += h, static h => RhinoDoc.InstanceDefinitionTableEvent -= h,
            static args => Events.Table(
                args.Document, TableKind.InstanceDefinitions, args.InstanceDefinitionIndex, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind SectionStyleTableEvent = new(
        nameof(SectionStyleTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<SectionStyleTableEventArgs>(
            sink, kind, static h => RhinoDoc.SectionStyleTableEvent += h, static h => RhinoDoc.SectionStyleTableEvent -= h,
            static args => Events.Table(args.Document, TableKind.SectionStyles, args.Index, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind MarkupTableEvent = new(
        nameof(MarkupTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<MarkupTableEventArgs>(
            sink, kind, static h => RhinoDoc.MarkupTableEvent += h, static h => RhinoDoc.MarkupTableEvent -= h,
            static args => Events.Table(args.Document, TableKind.Markups, args.Index, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind PageViewGroupTableEvent = new(
        nameof(PageViewGroupTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<PageViewGroupTableEventArgs>(
            sink, kind, static h => RhinoDoc.PageViewGroupTableEvent += h, static h => RhinoDoc.PageViewGroupTableEvent -= h,
            static args => Events.Table(
                args.Document, TableKind.PageViewGroups, args.PageViewGroupIndex, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind HatchPatternTableEvent = new(
        nameof(HatchPatternTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<HatchPatternTableEventArgs>(
            sink, kind, static h => RhinoDoc.HatchPatternTableEvent += h, static h => RhinoDoc.HatchPatternTableEvent -= h,
            static args => Events.Table(
                args.Document, TableKind.HatchPatterns, args.HatchPatternIndex, TransitionMapper.ToTransition(args.EventType), () => Events.RowState(args.OldState), () => Events.RowState(args.NewState))));

    public static readonly EventKind RenderMaterialsTableEvent = new(
        nameof(RenderMaterialsTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<RhinoDoc.RenderContentTableEventArgs>(
            sink, kind, static h => RhinoDoc.RenderMaterialsTableEvent += h, static h => RhinoDoc.RenderMaterialsTableEvent -= h,
            static args => Events.ContentTable(TableKind.RenderMaterials, args)));

    public static readonly EventKind RenderEnvironmentTableEvent = new(
        nameof(RenderEnvironmentTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<RhinoDoc.RenderContentTableEventArgs>(
            sink, kind, static h => RhinoDoc.RenderEnvironmentTableEvent += h, static h => RhinoDoc.RenderEnvironmentTableEvent -= h,
            static args => Events.ContentTable(TableKind.RenderEnvironments, args)));

    public static readonly EventKind RenderTextureTableEvent = new(
        nameof(RenderTextureTableEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<RhinoDoc.RenderContentTableEventArgs>(
            sink, kind, static h => RhinoDoc.RenderTextureTableEvent += h, static h => RhinoDoc.RenderTextureTableEvent -= h,
            static args => Events.ContentTable(TableKind.RenderTextures, args)));

    public static readonly EventKind TextureMappingEvent = new(
        nameof(TextureMappingEvent),
        EventCategory.Tables,
        static (kind, sink) => Events.Host<RhinoDoc.TextureMappingEventArgs>(
            sink, kind, static h => RhinoDoc.TextureMappingEvent += h, static h => RhinoDoc.TextureMappingEvent -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.MappingChange(args.EventType, Optional(args.NewMapping).Map(static mapping => mapping.Id)))));

    // --- [SCREEN]
    public static readonly EventKind RhinoViewCreate = new(
        nameof(RhinoViewCreate),
        EventCategory.Screen,
        static (kind, sink) => Events.Host<ViewEventArgs>(sink, kind, static h => RhinoView.Create += h, static h => RhinoView.Create -= h, Events.Screen));

    public static readonly EventKind RhinoViewDestroy = new(
        nameof(RhinoViewDestroy),
        EventCategory.Screen,
        static (kind, sink) => Events.Host<ViewEventArgs>(sink, kind, static h => RhinoView.Destroy += h, static h => RhinoView.Destroy -= h, Events.Screen));

    public static readonly EventKind RhinoViewSetActive = new(
        nameof(RhinoViewSetActive),
        EventCategory.Screen,
        static (kind, sink) => Events.Host<ViewEventArgs>(sink, kind, static h => RhinoView.SetActive += h, static h => RhinoView.SetActive -= h, Events.Screen));

    public static readonly EventKind RhinoViewRename = new(
        nameof(RhinoViewRename),
        EventCategory.Screen,
        static (kind, sink) => Events.Host<ViewEventArgs>(sink, kind, static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h, Events.Screen));

    public static readonly EventKind RhinoViewModified = new(
        nameof(RhinoViewModified),
        EventCategory.Screen,
        static (kind, sink) => Events.Host<ViewEventArgs>(sink, kind, static h => RhinoView.Modified += h, static h => RhinoView.Modified -= h, Events.Screen));

    public static readonly EventKind ViewportProjectionChanged = new(nameof(ViewportProjectionChanged), EventCategory.Screen, static (_, sink) => Events.Projections(sink));

    public static readonly EventKind DisplayModeChanged = new(
        nameof(DisplayModeChanged),
        EventCategory.Screen,
        static (kind, sink) => Events.Host<DisplayModeChangedEventArgs>(
            sink, kind, static h => DisplayPipeline.DisplayModeChanged += h, static h => DisplayPipeline.DisplayModeChanged -= h,
            static args => (DocumentHandles.Serial(args.RhinoDoc), new EventData.DisplayMode(args.Viewport.Id, args.OldDisplayModeId, args.ChangedDisplayModeId))));

    // --- [DRAW]
    public static readonly EventKind DrawForeground = new(
        nameof(DrawForeground),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawEventArgs>(sink, kind, static h => DisplayPipeline.DrawForeground += h, static h => DisplayPipeline.DrawForeground -= h, Events.Drawn));

    public static readonly EventKind DrawOverlay = new(
        nameof(DrawOverlay),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawEventArgs>(sink, kind, static h => DisplayPipeline.DrawOverlay += h, static h => DisplayPipeline.DrawOverlay -= h, Events.Drawn));

    public static readonly EventKind ObjectCulling = new(
        nameof(ObjectCulling),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<CullObjectEventArgs>(
            sink, kind, static h => DisplayPipeline.ObjectCulling += h, static h => DisplayPipeline.ObjectCulling -= h,
            static args => Events.Frame(args, Some((args.RhinoObject.Id, Serial: args.RhinoObjectSerialNumber)))));

    public static readonly EventKind InitFrameBuffer = new(
        nameof(InitFrameBuffer),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<InitFrameBufferEventArgs>(
            sink, kind, static h => DisplayPipeline.InitFrameBuffer += h, static h => DisplayPipeline.InitFrameBuffer -= h,
            static _ => (Option<uint>.None, new EventData.FrameBuffer())));

    public static readonly EventKind PreDrawObjects = new(
        nameof(PreDrawObjects),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawEventArgs>(sink, kind, static h => DisplayPipeline.PreDrawObjects += h, static h => DisplayPipeline.PreDrawObjects -= h, Events.Drawn));

    public static readonly EventKind PreDrawTransparentObjects = new(
        nameof(PreDrawTransparentObjects),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawEventArgs>(
            sink, kind, static h => DisplayPipeline.PreDrawTransparentObjects += h, static h => DisplayPipeline.PreDrawTransparentObjects -= h, Events.Drawn));

    public static readonly EventKind PreDrawObject = new(
        nameof(PreDrawObject),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawObjectEventArgs>(sink, kind, static h => DisplayPipeline.PreDrawObject += h, static h => DisplayPipeline.PreDrawObject -= h, Events.DrawnObject));

    public static readonly EventKind PostDrawObject = new(
        nameof(PostDrawObject),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawObjectEventArgs>(sink, kind, static h => DisplayPipeline.PostDrawObject += h, static h => DisplayPipeline.PostDrawObject -= h, Events.DrawnObject));

    public static readonly EventKind PostDrawObjects = new(
        nameof(PostDrawObjects),
        EventCategory.Draw,
        static (kind, sink) => Events.Host<DrawEventArgs>(sink, kind, static h => DisplayPipeline.PostDrawObjects += h, static h => DisplayPipeline.PostDrawObjects -= h, Events.Drawn));

    // --- [PANELS]
    public static readonly EventKind PanelsShow = new(
        nameof(PanelsShow),
        EventCategory.Panels,
        static (kind, sink) => Events.Host<ShowPanelEventArgs>(
            sink, kind, static h => Panels.Show += h, static h => Panels.Show -= h,
            static args => (Answers.Present(args.DocumentSerialNumber), new EventData.Panel(args.PanelId, Some(args.Show)))));

    public static readonly EventKind PanelsClosed = new(
        nameof(PanelsClosed),
        EventCategory.Panels,
        static (kind, sink) => Events.Host<PanelEventArgs>(
            sink, kind, static h => Panels.Closed += h, static h => Panels.Closed -= h,
            static args => (Answers.Present(args.DocumentSerialNumber), new EventData.Panel(args.PanelId, Option<bool>.None))));

    // --- [COMMANDS]
    public static readonly EventKind BeginCommand = new(
        nameof(BeginCommand),
        EventCategory.Commands,
        static (kind, sink) => Events.Host<CommandEventArgs>(sink, kind, static h => Command.BeginCommand += h, static h => Command.BeginCommand -= h, Events.Command));

    public static readonly EventKind EndCommand = new(
        nameof(EndCommand),
        EventCategory.Commands,
        static (kind, sink) => Events.Host<CommandEventArgs>(sink, kind, static h => Command.EndCommand += h, static h => Command.EndCommand -= h, Events.Command));

    public static readonly EventKind UndoRedo = new(
        nameof(UndoRedo),
        EventCategory.Commands,
        static (kind, sink) => Events.Host<UndoRedoEventArgs>(sink, kind, static h => Command.UndoRedo += h, static h => Command.UndoRedo -= h, Events.Undo));

    public static readonly EventKind CommandPromptChanged = new(
        nameof(CommandPromptChanged),
        EventCategory.Commands,
        static (kind, sink) => Events.Host<CommandPromptChangedEventArgs>(
            sink, kind, static h => RhinoApp.CommandPromptChanged += h, static h => RhinoApp.CommandPromptChanged -= h,
            static args => (
                Option<uint>.None,
                new EventData.CommandPrompt(args.Prompt, args.PromptDefault, Answers.Present(args.Options).Map(static option => OptionSelection.Read(option, Option<OptionState>.None)).Strict()))));

    public static readonly EventKind EscapeKeyPressed = new(
        nameof(EscapeKeyPressed),
        EventCategory.Commands,
        static (kind, sink) => Events.Host(sink, kind, static h => RhinoApp.EscapeKeyPressed += h, static h => RhinoApp.EscapeKeyPressed -= h, static () => new EventData.Signal()));

    // --- [APPLICATION]
    public static readonly EventKind LicenseStateChanged = new(
        nameof(LicenseStateChanged),
        EventCategory.Application,
        static (kind, sink) => Events.Host<LicenseStateChangedEventArgs>(
            sink, kind, static h => RhinoApp.LicenseStateChanged += h, static h => RhinoApp.LicenseStateChanged -= h,
            static args => (Option<uint>.None, new EventData.LicenseState(args.CallingRhinoCommonAllowed))));

    public static readonly EventKind ThemeChanged = new(
        nameof(ThemeChanged),
        EventCategory.Application,
        static (kind, sink) => Events.Host(sink, kind, static h => ThemeSettings.ThemeChanged += h, static h => ThemeSettings.ThemeChanged -= h, static () => new EventData.Appearance(HostUtils.RunningInDarkMode)));

    public static readonly EventKind OnExceptionReport = new(
        nameof(OnExceptionReport),
        EventCategory.Application,
        static (kind, sink) => Events.Attach<HostUtils.ExceptionReportDelegate>(
            static h => HostUtils.OnExceptionReport += h, static h => HostUtils.OnExceptionReport -= h,
            (source, exception) => sink.Send(() => Some(new DocEvent(kind, Option<uint>.None, new EventData.ExceptionReport(source, exception))))));

    public static readonly EventKind OnSendLogMessageToCloud = new(
        nameof(OnSendLogMessageToCloud),
        EventCategory.Application,
        static (kind, sink) => Events.Attach<HostUtils.SendLogMessageToCloudDelegate>(
            static h => HostUtils.OnSendLogMessageToCloud += h, static h => HostUtils.OnSendLogMessageToCloud -= h,
            (messageType, type, description, text) => sink.Send(() => Some(new DocEvent(kind, Option<uint>.None, new EventData.CloudLog(messageType, type, description, text))))));

    // --- [RENDER]
    public static readonly EventKind ContentAdded = new(
        nameof(ContentAdded),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(sink, kind, static h => RenderContent.ContentAdded += h, static h => RenderContent.ContentAdded -= h, Events.Content));

    public static readonly EventKind ContentRenamed = new(
        nameof(ContentRenamed),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(sink, kind, static h => RenderContent.ContentRenamed += h, static h => RenderContent.ContentRenamed -= h, Events.Content));

    public static readonly EventKind ContentDeleting = new(
        nameof(ContentDeleting),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(sink, kind, static h => RenderContent.ContentDeleting += h, static h => RenderContent.ContentDeleting -= h, Events.Content));

    public static readonly EventKind ContentDeleted = new(
        nameof(ContentDeleted),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(sink, kind, static h => RenderContent.ContentDeleted += h, static h => RenderContent.ContentDeleted -= h, Events.Content));

    public static readonly EventKind ContentReplacing = new(
        nameof(ContentReplacing),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(sink, kind, static h => RenderContent.ContentReplacing += h, static h => RenderContent.ContentReplacing -= h, Events.Content));

    public static readonly EventKind ContentReplaced = new(
        nameof(ContentReplaced),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(sink, kind, static h => RenderContent.ContentReplaced += h, static h => RenderContent.ContentReplaced -= h, Events.Content));

    public static readonly EventKind ContentUpdatePreview = new(
        nameof(ContentUpdatePreview),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(
            sink, kind, static h => RenderContent.ContentUpdatePreview += h, static h => RenderContent.ContentUpdatePreview -= h, Events.Content));

    public static readonly EventKind CurrentEnvironmentChanged = new(
        nameof(CurrentEnvironmentChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentEventArgs>(
            sink, kind, static h => RenderContent.CurrentEnvironmentChanged += h, static h => RenderContent.CurrentEnvironmentChanged -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.EnvironmentUsage(args.EnvironmentUsageEx))));

    public static readonly EventKind ContentChanged = new(
        nameof(ContentChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentChangedEventArgs>(
            sink, kind, static h => RenderContent.ContentChanged += h, static h => RenderContent.ContentChanged -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.ContentEdit(args.Content.Id, args.ChangeContext, Optional(args.OldContent).Map(static old => old.Id)))));

    public static readonly EventKind ContentFieldChanged = new(
        nameof(ContentFieldChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderContentFieldChangedEventArgs>(
            sink, kind, static h => RenderContent.ContentFieldChanged += h, static h => RenderContent.ContentFieldChanged -= h,
            static args => (DocumentHandles.Serial(args.Document), new EventData.ContentField(args.Content.Id, args.FieldName, args.ChangeContext))));

    public static readonly EventKind PreviewRendered = new(
        nameof(PreviewRendered),
        EventCategory.Render,
        static (kind, sink) => Events.Host<PreviewRenderedEventArgs>(
            sink, kind, static h => RenderContent.PreviewRendered += h, static h => RenderContent.PreviewRendered -= h,
            static args => (Option<uint>.None, new EventData.Preview(args.Bitmap, args.PreviewJobSignature.ImageWidth(), args.PreviewJobSignature.ImageHeight(), args.Quality))));

    public static readonly EventKind GroundPlaneChanged = new(
        nameof(GroundPlaneChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderPropertyChangedEvent>(sink, kind, static h => GroundPlane.Changed += h, static h => GroundPlane.Changed -= h, Events.RenderProperty));

    public static readonly EventKind SkylightChanged = new(
        nameof(SkylightChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderPropertyChangedEvent>(sink, kind, static h => Skylight.Changed += h, static h => Skylight.Changed -= h, Events.RenderProperty));

    public static readonly EventKind SunChanged = new(
        nameof(SunChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderPropertyChangedEvent>(sink, kind, static h => Sun.Changed += h, static h => Sun.Changed -= h, Events.RenderProperty));

    public static readonly EventKind SafeFrameChanged = new(
        nameof(SafeFrameChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderPropertyChangedEvent>(sink, kind, static h => SafeFrame.Changed += h, static h => SafeFrame.Changed -= h, Events.RenderProperty));

    public static readonly EventKind RenderChannelsChanged = new(
        nameof(RenderChannelsChanged),
        EventCategory.Render,
        static (kind, sink) => Events.Host<RenderPropertyChangedEvent>(
            sink, kind, static h => RenderChannels.Changed += h, static h => RenderChannels.Changed -= h, Events.RenderProperty));

    // --- [ATTACH]
    public EventCategory Category { get; }

    private readonly Func<EventKind, Events.Sink, IO<IDisposable>> attach;

    internal IO<IDisposable> Attach(Events.Sink sink) =>
        attach(this, sink);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Events {
    // --- [ATTACH]
    public static IO<IDisposable> Attach<THandler>(Action<THandler> subscribe, Action<THandler> unsubscribe, THandler handler) where THandler : Delegate =>
        GeometryOps.OnFailure(IO.lift(() => subscribe(handler)), IO.lift(() => unsubscribe(handler)))
            .Map<IDisposable>(_ => new Disposal(() => unsubscribe(handler)));

    public static IO<IDisposable> AttachAll(Seq<IO<IDisposable>> attach) =>
        Disposal.AcquireAll(attach).Map<IDisposable>(static attached => new Disposal(() => Disposal.Release(attached).Run()));

    public static IO<IDisposable> OnIdle(EventHandler handler) =>
        Attach(static h => RhinoApp.Idle += h, static h => RhinoApp.Idle -= h, handler);

    public static IO<IDisposable> Attach(EventKind kind, EventScope scope, Func<DocEvent, IO<Unit>> deliver, Action<Error> reject) =>
        kind.Attach(new Sink(scope, deliver, reject));

    public static IO<IDisposable> AttachCategory(EventCategory category, EventScope scope, Func<DocEvent, IO<Unit>> deliver, Action<Error> reject) =>
        AttachAll(toSeq(EventKind.Items).Filter(kind => kind.Category == category).Map(kind => Attach(kind, scope, deliver, reject)));

    internal static IO<IDisposable> Host<TArgs>(
        Sink sink,
        EventKind kind,
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<TArgs, (Option<uint> Serial, EventData Data)> project) =>
        Filtered(sink, kind, subscribe, unsubscribe, (_, args) => Some(project(args)));

    internal static IO<IDisposable> Host(Sink sink, EventKind kind, Action<EventHandler> subscribe, Action<EventHandler> unsubscribe, Func<EventData> project) =>
        Attach(subscribe, unsubscribe, new EventHandler((_, _) => sink.Send(() => Some(new DocEvent(kind, Option<uint>.None, project())))));

    internal static IO<IDisposable> Filtered<TArgs>(
        Sink sink,
        EventKind kind,
        Action<EventHandler<TArgs>> subscribe,
        Action<EventHandler<TArgs>> unsubscribe,
        Func<object?, TArgs, Option<(Option<uint> Serial, EventData Data)>> project) =>
        Attach(
            subscribe,
            unsubscribe,
            new EventHandler<TArgs>((sender, args) => sink.Send(() => project(sender, args).Map(row => new DocEvent(kind, row.Serial, row.Data)))));

    internal static IO<IDisposable> Transforms(Sink sink) =>
        IO.lift(static () => Atom(new TransformLog(HashMap<uint, (Option<uint> Serial, Seq<(Guid Id, uint Serial)> Objects)>(), None)))
            .Bind(pending => AttachAll(Seq(
                Filtered<RhinoTransformObjectsEventArgs>(
                    sink, EventKind.TransformObjects, static h => RhinoDoc.BeforeTransformObjects += h, static h => RhinoDoc.BeforeTransformObjects -= h,
                    (sender, args) => Started(pending, sender, args)),
                Filtered<RhinoAfterTransformObjectsEventArgs>(
                    sink, EventKind.TransformObjects, static h => RhinoDoc.AfterTransformObjects += h, static h => RhinoDoc.AfterTransformObjects -= h,
                    (_, args) => Ended(pending, args)))));

    internal static IO<IDisposable> Projections(Sink sink) =>
        IO.lift(static () => Atom((Last: Option<(Guid Viewport, Option<uint> Document, uint Counter)>.None, Repeat: false)))
            .Bind(last => Filtered<DrawEventArgs>(
                sink, EventKind.ViewportProjectionChanged, static h => DisplayPipeline.ViewportProjectionChanged += h, static h => DisplayPipeline.ViewportProjectionChanged -= h,
                (_, args) => Changed(last, args)));

    internal sealed record TransformLog(HashMap<uint, (Option<uint> Serial, Seq<(Guid Id, uint Serial)> Objects)> Pending, Option<(Option<uint> Serial, Seq<(Guid Id, uint Serial)> Objects)> Taken);

    internal sealed record Sink(EventScope Scope, Func<DocEvent, IO<Unit>> Deliver, Action<Error> Reject) {
        public void Send(Func<Option<DocEvent>> project) =>
            _ = Answers.Answer(IO.lift(project).Bind(projected => projected.Filter(Matches).Match(Some: Deliver, None: static () => IO.pure(unit))), Reject, unit);

        private bool Matches(DocEvent evt) =>
            Scope.Switch(evt, only: static (held, only) => held.DocumentSerial == Some(only.Serial), any: static (_, _) => true);
    }

    // --- [PROJECTIONS]
    internal static (Option<uint> Serial, EventData Data) Opened(DocumentOpenEventArgs args) =>
        (Answers.Present(args.DocumentSerialNumber), new EventData.Opened(Answers.Present(args.FileName), args.Merge, args.Reference));

    internal static (Option<uint> Serial, EventData Data) Saved(DocumentSaveEventArgs args) =>
        (Answers.Present(args.DocumentSerialNumber), new EventData.Saved(Answers.Present(args.FileName), args.ExportSelected));

    internal static (Option<uint> Serial, EventData Data) Signal(DocumentEventArgs args) =>
        (Answers.Present(args.DocumentSerialNumber), new EventData.Signal());

    internal static Option<(Option<uint> Serial, EventData Data)> Structure(object? sender, RhinoObjectEventArgs args) =>
        Some((
            DocumentHandles.Serial(sender as RhinoDoc) || Optional(args.TheObject).Bind(static subject => DocumentHandles.Serial(subject.Document)),
            (EventData)new EventData.Object(args.ObjectId)));

    internal static (Option<uint> Serial, EventData Data) Selection(RhinoObjectSelectionEventArgs args) =>
        (DocumentHandles.Serial(args.Document), new EventData.Selection(toSeq(args.RhinoObjects).Map(static subject => subject.Id).Strict(), args.RhinoObjectCount));

    internal static (Option<uint> Serial, EventData Data) Table(
        RhinoDoc? doc,
        TableKind kind,
        int index,
        ComponentTransition transition,
        Func<Option<TableRowState>> previous,
        Func<Option<TableRowState>> current) =>
        (DocumentHandles.Serial(doc), new EventData.Component(
            kind,
            index,
            transition,
            transition is ComponentTransition.Added or ComponentTransition.Sorted ? Option<TableRowState>.None : previous(),
            transition is ComponentTransition.Deleted or ComponentTransition.Sorted ? Option<TableRowState>.None : current()));

    private static Option<(Option<uint> Serial, EventData Data)> Started(
        Atom<TransformLog> pending,
        object? sender,
        RhinoTransformObjectsEventArgs args) {
        Seq<(Guid Id, uint Serial)> objects = Pairs(args.Objects);
        Option<uint> serial = DocumentHandles.Serial(sender as RhinoDoc) || toSeq(args.Objects).Head.Bind(static subject => DocumentHandles.Serial(subject.Document));
        _ = pending.Swap(log => new TransformLog(log.Pending.AddOrUpdate(args.TransformEventId, (serial, objects)), None));
        return Some((
            serial,
            (EventData)new EventData.TransformStarted(args.TransformEventId, args.Transform, args.ObjectsWillBeCopied, objects, Pairs(args.Grips), Pairs(args.GripOwners))));
    }

    private static Option<(Option<uint> Serial, EventData Data)> Ended(
        Atom<TransformLog> pending,
        RhinoAfterTransformObjectsEventArgs args) =>
        pending.Swap(log => new TransformLog(log.Pending.Remove(args.TransformEventId), log.Pending.Find(args.TransformEventId)))
            .Taken
            .Map(entry => (entry.Serial, (EventData)new EventData.TransformEnded(args.TransformEventId, entry.Objects)));

    private static Seq<(Guid Id, uint Serial)> Pairs(RhinoObject[] objects) =>
        toSeq(objects).Map(static subject => (subject.Id, Serial: subject.RuntimeSerialNumber)).Strict();

    // --- [ROWS]
    internal static Option<TableRowState> RowState(ModelComponent? row) =>
        Optional(row).Map<TableRowState>(static present => present.IsDeleted ? new TableRowState.Deleted(present.Id, Answers.Present(present.DeletedName)) : new TableRowState.Present(present.Id, Answers.Present(present.Name)));

    internal static Option<TableRowState> LightState(Light? light) =>
        Optional(light).Map<TableRowState>(static present => new TableRowState.Present(present.Id, Answers.Present(present.Name)));

    internal static (Option<uint> Serial, EventData Data) ContentTable(TableKind kind, RhinoDoc.RenderContentTableEventArgs args) =>
        (
            DocumentHandles.Serial(args.Document),
            new EventData.ContentChange(
                kind,
                args.EventType,
                Optional(args as RhinoDoc.RenderMaterialAssignmentChangedEventArgs)
                    .Map(static change => EventMapper.ToAssignment(change))));

    // --- [SCREEN]
    internal static (Option<uint> Serial, EventData Data) Screen(ViewEventArgs args) =>
        (DocumentHandles.Serial(args.View.Document), new EventData.View(args.View.RuntimeSerialNumber, args.View.MainViewport.Id, args.View is RhinoPageView));

    internal static (Option<uint> Serial, EventData Data) Drawn(DrawEventArgs args) =>
        Frame(args, Option<(Guid Id, uint Serial)>.None);

    internal static (Option<uint> Serial, EventData Data) DrawnObject(DrawObjectEventArgs args) =>
        Frame(args, Some((args.RhinoObject.Id, Serial: args.RhinoObject.RuntimeSerialNumber)));

    internal static (Option<uint> Serial, EventData Data) Frame(DrawEventArgs args, Option<(Guid Id, uint Serial)> subject) =>
        (
            DocumentHandles.Serial(args.RhinoDoc),
            new EventData.Frame(
                args.Viewport.Id,
                args.Viewport.ChangeCounter,
                Optional(args.Viewport.ParentView).Map(static view => view.RuntimeSerialNumber),
                subject));

    private static Option<(Option<uint> Serial, EventData Data)> Changed(
        Atom<(Option<(Guid Viewport, Option<uint> Document, uint Counter)> Last, bool Repeat)> last,
        DrawEventArgs args) {
        (Guid Viewport, Option<uint> Document, uint Counter) raised = (args.Viewport.Id, DocumentHandles.Serial(args.RhinoDoc), args.Viewport.ChangeCounter);
        return last.Swap(prior => (Some(raised), prior.Last == Some(raised))).Repeat
            ? Option<(Option<uint> Serial, EventData Data)>.None
            : Some((raised.Document, (EventData)new EventData.ViewportProjection(raised.Viewport, raised.Counter)));
    }

    // --- [COMMANDS]
    internal static (Option<uint> Serial, EventData Data) Command(CommandEventArgs args) =>
        (
            Answers.Present(args.DocumentRuntimeSerialNumber),
            new EventData.Command(
                args.CommandId,
                args.CommandEnglishName,
                args.CommandLocalName,
                Optional(args.CommandHelpURL),
                Answers.Present(args.CommandPluginName),
                args.CommandIsHiddenFromUser,
                args.CommandResult));

    internal static (Option<uint> Serial, EventData Data) Undo(UndoRedoEventArgs args) =>
        (
            Answers.Present(args.DocumentSerialNumber),
            new EventData.UndoRedo(
                args.CommandId,
                args.UndoSerialNumber,
                args switch {
                    { IsBeforeBeginRecording: true } => UndoRedoKind.BeforeBeginRecording,
                    { IsBeginRecording: true } => UndoRedoKind.BeginRecording,
                    { IsBeforeEndRecording: true } => UndoRedoKind.BeforeEndRecording,
                    { IsEndRecording: true } => UndoRedoKind.EndRecording,
                    { IsBeginUndo: true } => UndoRedoKind.BeginUndo,
                    { IsEndUndo: true } => UndoRedoKind.EndUndo,
                    { IsBeginRedo: true } => UndoRedoKind.BeginRedo,
                    { IsEndRedo: true } => UndoRedoKind.EndRedo,
                    { IsPurgeRecord: true } => UndoRedoKind.PurgeRecord,
                    _ => UndoRedoKind.Other,
                }));

    // --- [RENDER]
    internal static (Option<uint> Serial, EventData Data) Content(RenderContentEventArgs args) =>
        (DocumentHandles.Serial(args.Document), new EventData.Content(args.Content.Id, args.Reason));

    internal static (Option<uint> Serial, EventData Data) RenderProperty(RenderPropertyChangedEvent args) =>
        (DocumentHandles.Serial(args.Document), new EventData.RenderProperty(args.Context));
}

[Mapper(EnabledConversions = MappingConversionType.EnumToEnum, EnumMappingStrategy = EnumMappingStrategy.ByName, RequiredEnumMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class TransitionMapper {
    internal static partial ComponentTransition ToTransition(LayerTableEventType type);

    internal static partial ComponentTransition ToTransition(MaterialTableEventType type);

    internal static partial ComponentTransition ToTransition(GroupTableEventType type);

    internal static partial ComponentTransition ToTransition(LinetypeTableEventType type);

    internal static partial ComponentTransition ToTransition(LightTableEventType type);

    internal static partial ComponentTransition ToTransition(DimStyleTableEventType type);

    internal static partial ComponentTransition ToTransition(InstanceDefinitionTableEventType type);

    internal static partial ComponentTransition ToTransition(SectionStyleTableEventType type);

    internal static partial ComponentTransition ToTransition(MarkupTableEventType type);

    internal static partial ComponentTransition ToTransition(PageViewGroupTableEventType type);

    internal static partial ComponentTransition ToTransition(HatchPatternTableEventType type);
}

[Mapper]
internal static partial class EventMapper {
    [MapPropertyFromSource(nameof(RenderAssignment.TargetId), Use = nameof(TargetId))]
    [MapProperty(nameof(RhinoDoc.RenderMaterialAssignmentChangedEventArgs.OldRenderMaterial), nameof(RenderAssignment.Previous))]
    [MapProperty(nameof(RhinoDoc.RenderMaterialAssignmentChangedEventArgs.NewRenderMaterial), nameof(RenderAssignment.Current))]
    internal static partial RenderAssignment ToAssignment(RhinoDoc.RenderMaterialAssignmentChangedEventArgs change);

    private static Guid TargetId(RhinoDoc.RenderMaterialAssignmentChangedEventArgs change) =>
        change.IsLayer ? change.LayerId : change.ObjectId;
}
