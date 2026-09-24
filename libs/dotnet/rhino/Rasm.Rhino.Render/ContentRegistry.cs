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
    Func<string, RenderContent, CreatePreviewEventArgs, IO<Unit>> Write,
    Option<Func<RhinoDoc, Seq<string>, RenderContentKind, RenderContentSerializer.LoadMultipleFlags, IO<Unit>>> LoadMultiple);

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class ContentSerializer(string fileExtension, RenderContentKind contentKind, bool canRead, bool canWrite)
    : RenderContentSerializer(fileExtension, contentKind, canRead, canWrite) {
    protected abstract SerializerCallbacks Callbacks { get; }

    public sealed override RenderContent? Read(string pathToFile) =>
        Answers.Answer(Callbacks.Read(pathToFile).Map(static content => (RenderContent?)content), Callbacks.Reject, fallback: null);

    public sealed override bool Write(string pathToFile, RenderContent renderContent, CreatePreviewEventArgs previewArgs) =>
        Answers.Succeeded(Callbacks.Write(pathToFile, renderContent, previewArgs), Callbacks.Reject);

    public sealed override bool CanLoadMultiple() =>
        Callbacks.LoadMultiple.IsSome;

    public sealed override bool LoadMultiple(RhinoDoc doc, IEnumerable<string> fileNames, RenderContentKind contentKind, LoadMultipleFlags flags) =>
        Answers.Succeeded(Callbacks.LoadMultiple.Map(load => load(doc, toSeq(fileNames), contentKind, flags)), Callbacks.Reject, () => base.LoadMultiple(doc, fileNames, contentKind, flags));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentRegistry {
    // --- [CONTENT_TYPES]
    public static IO<Seq<ContentTypeState>> AvailableTypes { get; } =
        Disposal.Using(
            IO.lift(static () => toSeq(RenderContentType.GetAllAvailableTypes())),
            static types => IO.lift(() => types.Map(static row => ContentMapper.ToState(row)).Strict()));

    public static IO<Seq<Type>> RegisterContent(System.Reflection.Assembly assembly, Guid plugInId) =>
        Answers.Registered(RenderContent.RegisterContent, assembly, plugInId, nameof(RenderContent.RegisterContent));

    public static IO<RenderContent> CreateInDocument(RhinoDoc doc, Guid typeId, Option<(RenderContent Parent, string Slot)> parent) =>
        from id in IO.lift(() => Answers.NonEmpty(typeId, nameof(RenderContent.Create)))
        from content in IO.lift(() => Missing.Unless(parent.Match(
                Some: parented => RenderContent.Create(doc, id, parented.Parent, parented.Slot),
                None: () => RenderContent.Create(doc, id)), nameof(RenderContent.Create)))
        select content;

    public static IO<Unit> RegisterSerializer(RenderContentSerializer serializer, Guid pluginId) =>
        from extension in IO.lift(() => Invalid.Unless(serializer.FileExtension.Length > 0, nameof(RenderContentSerializer.FileExtension)))
        from id in IO.lift(() => Answers.NonEmpty(pluginId, nameof(RenderContentSerializer.RegisterSerializer)))
        from registered in IO.lift(() => AlreadyRegistered.Unless(serializer.RegisterSerializer(id), serializer.FileExtension))
        select registered;

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
        rows.TraverseM(row =>
                from registrable in IO.lift(() => Registrable(row.Body, nameof(RenderPanels.RegisterPanel)))
                from registered in IO.lift(() => panels.RegisterPanel(owner, row.Kind, row.Body, row.RenderEngineId, row.Caption, row.AlwaysShow, row.InitialShow, row.Position))
                    .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<Unit>(new Refused(nameof(RenderPanels.RegisterPanel))))
                select registered)
            .As()
            .Map(static _ => unit);

    public static IO<Unit> RegisterTabs(RenderTabs tabs, PlugIn owner, Seq<TabRegistration> rows) =>
        rows.TraverseM(row =>
                from registrable in IO.lift(() => Registrable(row.Body, nameof(RenderTabs.RegisterTab)))
                from registered in IO.lift(() => tabs.RegisterTab(owner, row.Body, row.RenderEngineId, row.Caption, row.Icon))
                    .Catch(static error => error.HasException<ArgumentException>(), static _ => IO.fail<Unit>(new Refused(nameof(RenderTabs.RegisterTab))))
                select registered)
            .As()
            .Map(static _ => unit);

    public static IO<Option<TBody>> Panel<TBody>(PlugIn owner, Guid renderSessionId) where TBody : class =>
        from session in IO.lift(() => Session<TBody>(renderSessionId, nameof(RenderPanels.FromRenderSessionId)))
        from body in IO.lift(() => Optional(RenderPanels.FromRenderSessionId(owner, typeof(TBody), session) as TBody))
        select body;

    public static IO<Option<TBody>> Tab<TBody>(PlugIn owner, Guid renderSessionId) where TBody : class =>
        from session in IO.lift(() => Session<TBody>(renderSessionId, nameof(RenderTabs.FromRenderSessionId)))
        from body in IO.lift(() => Optional(RenderTabs.FromRenderSessionId(owner, typeof(TBody), session) as TBody))
        select body;

    public static IO<Guid> SidePaneUiIdFromTab(object tab) =>
        IO.lift(() =>
            from keyed in MissingGuid.Unless(tab.GetType())
            from id in Answers.NonEmpty(RenderTabs.SidePaneUiIdFromTab(tab), nameof(RenderTabs.SidePaneUiIdFromTab))
            select id);

    private static Fin<Guid> Session<TBody>(Guid renderSessionId, string member) =>
        from keyed in MissingGuid.Unless(typeof(TBody))
        from session in Invalid.Unless(renderSessionId != Guid.Empty, member)
        select renderSessionId;

    private static Fin<Unit> Registrable(Type body, string member) =>
        from keyed in MissingGuid.Unless(body)
        from creatable in Invalid.Unless(body.IsPublic && (body.GetConstructor(Type.EmptyTypes) is not null), member)
        select unit;

    // --- [EDITOR]
    public static IO<TValue> WithProvider<TValue>(IRdkViewModel model, Guid providerId, bool forWrite, bool autoChangeBracket, Func<object, IO<TValue>> body) =>
        from data in IO.lift(() => Missing.Unless(model.GetData(providerId, forWrite, autoChangeBracket), nameof(IRdkViewModel.GetData)))
        from value in forWrite
            ? GeometryOps.OnFailure(
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
