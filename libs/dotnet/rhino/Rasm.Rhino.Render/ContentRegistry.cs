using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI.Controls;

namespace Rasm.Rhino.Render;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ContentTypeState(Guid TypeId, Option<string> InternalName, Guid RenderEngineId, Guid PlugInId);

public sealed record CollectionState(FilterContentByUsage Usage, Seq<Guid> Members, string SearchPattern, bool ForcedVaries, Seq<RenderContentKind> Kinds);

public sealed record PanelRegistration(Type Body, string Caption, RenderPanelType Kind, Guid RenderEngineId, bool AlwaysShow, bool InitialShow, RenderPanels.ExtraSidePanePosition Position);

public sealed record TabRegistration(Type Body, string Caption, Icon Icon, Guid RenderEngineId);

public sealed record EditorState(Guid CurrentRenderer, Option<Guid> RenderingViewport, Seq<Size> CustomSizes, bool CustomSizeIsPreset);

public sealed record SerializerCallbacks(
    Action<Error> Reject,
    Func<string, IO<RenderContent>> Read,
    Func<string, RenderContent, CreatePreviewEventArgs, IO<Unit>> Write);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ContentSerializer(string fileExtension, RenderContentKind contentKind, bool canRead, bool canWrite)
    : RenderContentSerializer(fileExtension, contentKind, canRead, canWrite) {
    protected abstract SerializerCallbacks Callbacks { get; }

    public sealed override RenderContent? Read(string pathToFile) =>
        Answers.Answer(Callbacks.Read(pathToFile).Map(static content => (RenderContent?)content), Callbacks.Reject, fallback: null);

    public sealed override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs previewArgs) =>
        Answers.Succeeded(Callbacks.Write(pathToFile, renderContent, previewArgs), Callbacks.Reject);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRegistry {
    // --- [CONTENT_TYPES]
    public static IO<Seq<ContentTypeState>> AvailableTypes { get; } =
        DisposalOps.Using(
            IO.lift(static () => toSeq(RenderContentType.GetAllAvailableTypes())),
            static types => IO.lift(() => types.Map(static row => ContentMapper.ToState(row)).Strict()));

    public static IO<Seq<Type>> RegisterContent(System.Reflection.Assembly assembly, Guid plugInId) =>
        Answers.Registered(RenderContent.RegisterContent, assembly, plugInId, nameof(RenderContent.RegisterContent));

    public static IO<RenderContent> CreateInDocument(RhinoDoc doc, Guid typeId, Option<(RenderContent Parent, string Slot)> parent) =>
        IO.lift(() => Missing.Unless(parent.Match(
            Some: parented => RenderContent.Create(doc, typeId, parented.Parent, parented.Slot),
            None: () => RenderContent.Create(doc, typeId)), nameof(RenderContent.Create)));

    public static IO<Unit> RegisterSerializer(RenderContentSerializer serializer, PlugIn owner) =>
        IO.lift(() => Refused.Unless(serializer.RegisterSerializer(owner.Id), nameof(RenderContentSerializer.RegisterSerializer)));

    // --- [COLLECTIONS]
    public static IO<CollectionState> Inspect(RenderContentCollection collection, RenderContentKindList kinds) =>
        IO.lift(() => new CollectionState(
            collection.GetFilterContentByUsage(),
            toSeq(collection).Map(static content => content.Id).Strict(),
            collection.GetSearchPattern(),
            collection.GetForcedVaries(),
            toSeq(Enum.GetValues<RenderContentKind>()).Filter(kind => (kind != RenderContentKind.None) && kinds.Contains(kind)).Strict()));

    // --- [PANELS]
    public static IO<Unit> RegisterPanels(RenderPanels panels, PlugIn owner, Seq<PanelRegistration> rows) =>
        rows.TraverseM(row => IO.lift(() => panels.RegisterPanel(owner, row.Kind, row.Body, row.RenderEngineId, row.Caption, row.AlwaysShow, row.InitialShow, row.Position))
                .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<Unit>(new Refused(nameof(RenderPanels.RegisterPanel)))))
            .As()
            .Map(static _ => unit);

    public static IO<Unit> RegisterTabs(RenderTabs tabs, PlugIn owner, Seq<TabRegistration> rows) =>
        rows.TraverseM(row => IO.lift(() => tabs.RegisterTab(owner, row.Body, row.RenderEngineId, row.Caption, row.Icon))
                .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<Unit>(new Refused(nameof(RenderTabs.RegisterTab)))))
            .As()
            .Map(static _ => unit);

    public static IO<Option<TBody>> Panel<TBody>(PlugIn owner, Guid renderSessionId) where TBody : class =>
        IO.lift(() => MissingGuid.Unless(typeof(TBody)).Map(_ => Optional(RenderPanels.FromRenderSessionId(owner, typeof(TBody), renderSessionId) as TBody)));

    public static IO<Option<TBody>> Tab<TBody>(PlugIn owner, Guid renderSessionId) where TBody : class =>
        IO.lift(() => MissingGuid.Unless(typeof(TBody)).Map(_ => Optional(RenderTabs.FromRenderSessionId(owner, typeof(TBody), renderSessionId) as TBody)));

    public static IO<Guid> SidePaneUiIdFromTab(object tab) =>
        IO.lift(() =>
            from keyed in MissingGuid.Unless(tab.GetType())
            from id in Answers.Required(RenderTabs.SidePaneUiIdFromTab(tab), nameof(RenderTabs.SidePaneUiIdFromTab))
            select id);

    // --- [EDITOR]
    public static IO<TValue> WithProvider<TValue>(IRdkViewModel model, Guid providerId, bool forWrite, bool autoChangeBracket, Func<object, IO<TValue>> body) =>
        from data in IO.lift(() => Missing.Unless(model.GetData(providerId, forWrite, autoChangeBracket), nameof(IRdkViewModel.GetData)))
        from value in forWrite
            ? DisposalOps.OnFailure(
                from result in body(data)
                from committed in IO.lift(() => model.Commit(providerId))
                select result,
                IO.lift(() => model.Discard(providerId)))
            : body(data)
        select value;

    public static IO<EditorState> Editor(RhinoSettings settings) =>
        IO.lift(() => new EditorState(
            settings.GetCurrentRenderer(),
            Optional(settings.RenderingView()).Map(static view => view.Viewport.Id),
            toSeq(settings.GetCustomRenderSizes()),
            settings.CustomImageSizeIsPreset));
}
