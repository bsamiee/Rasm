using Rhino.Render;
using Rhino.Render.DataSources;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;

namespace Rasm.Rhino.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Provider<T> where T : class {
    internal Provider(Guid id) => Id = id;

    public Guid Id { get; }
}

public static class Provider {
    public static readonly Provider<RhinoSettings> RhinoSettings = new(ProviderIds.RhinoSettings);
    public static readonly Provider<Sun> Sun = new(ProviderIds.Sun);
    public static readonly Provider<Skylight> Skylight = new(ProviderIds.Skylight);
    public static readonly Provider<GroundPlane> GroundPlane = new(ProviderIds.GroundPlane);
    public static readonly Provider<LinearWorkflow> LinearWorkflow = new(ProviderIds.LinearWorkflow);
    public static readonly Provider<Dithering> Dithering = new(ProviderIds.Dithering);
    public static readonly Provider<RenderChannels> RenderChannels = new(ProviderIds.RenderChannels);
    public static readonly Provider<ICurrentEnvironment> CurrentEnvironment = new(ProviderIds.CurrentEnvironment);
    public static readonly Provider<RenderContentCollection> ContentDisplayCollection = new(ProviderIds.ContentDisplayCollection);
    public static readonly Provider<RenderContentCollection> ContentDatabase = new(ProviderIds.ContentDatabase);
    public static readonly Provider<RenderContentCollection> ContentSelectionForSetParams = new(ProviderIds.ContentSelectionForSetParams);
    public static readonly Provider<RdkSelectionNavigator> SelectionNavigator = new(ProviderIds.SelectionNavigator);
    public static readonly Provider<RdkEdit> RdkEdit = new(ProviderIds.RdkEdit);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionModels {
    public static IO<TResult> Read<T, TResult>(ICollapsibleSection section, Provider<T> provider, Func<T, IO<TResult>> read) where T : class =>
        Model(section).Bind(model => GetData(model, provider, forWrite: false)).Bind(read);

    public static IO<TResult> Write<T, TResult>(ICollapsibleSection section, Provider<T> provider, Func<T, IO<TResult>> edit) where T : class =>
        Model(section).Bind(model => DisposalOps.OnFailure(
            from data in GetData(model, provider, forWrite: true)
            from edited in edit(data)
            from _ in IO.lift(() => model.Commit(provider.Id))
            select edited,
            IO.lift(() => model.Discard(provider.Id))));

    public static IO<T> UndoRecord<T>(ICollapsibleSection section, string description, IO<T> body) =>
        Model(section).Bind(model => use(() => new UndoRecord(description, model)).Bind(_ => body).Bracket());

    private static IO<IRdkViewModel> Model(ICollapsibleSection section) =>
        IO.lift(() => Missing.Unless(section.ViewModel, nameof(ICollapsibleSection.ViewModel)));

    private static IO<T> GetData<T>(IRdkViewModel model, Provider<T> provider, bool forWrite) where T : class =>
        IO.lift(() => Missing.Unless((T?)model.GetData(provider.Id, forWrite, bAutoChangeBracket: true), nameof(IRdkViewModel.GetData)));
}
