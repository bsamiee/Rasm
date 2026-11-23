using Rhino.Render.DataSources;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;
using DataSource = Rhino.UI.Controls.DataSource;

namespace Rasm.Rhino.Render.Views;

// --- [MODELS] --------------------------------------------------------------------------

public sealed class ProviderId<T>(Guid id) where T : class {
    public Guid Id { get; } = id;
}

public static class Provider {
    public static readonly ProviderId<RhinoSettings> RhinoSettings = new(ProviderIds.RhinoSettings);
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class SectionModels {
    public static IO<TResult> Read<T, TResult>(ICollapsibleSection section, ProviderId<T> provider, Func<T, IO<TResult>> read) where T : class =>
        Model(section).Bind(model => GetData(model, provider, forWrite: false)).Bind(read);

    public static IO<TResult> Write<T, TResult>(ICollapsibleSection section, ProviderId<T> provider, Func<T, IO<TResult>> edit) where T : class =>
        Model(section).Bind(model =>
            (from data in GetData(model, provider, forWrite: true)
             from edited in edit(data)
             from _ in IO.lift(() => model.Commit(provider.Id))
             select edited)
            .Catch(error => IO.lift(() => model.Discard(provider.Id)) >> IO.fail<TResult>(error)));

    public static IO<T> UndoRecord<T>(ICollapsibleSection section, string description, IO<T> body) =>
        Model(section).Bind(model =>
            (from record in use(() => new UndoRecord(description, model))
             from value in body
             select value).Bracket());

    public static HostEvent<Guid> DataChanged(EtoCollapsibleSection section) =>
        Subscriptions.On<DataSource.EventArgs>(h => section.DataChanged += h, h => section.DataChanged -= h).Choose(static args => Some(args.DataType));

    private static IO<IRdkViewModel> Model(ICollapsibleSection section) =>
        Products.Of(() => section.ViewModel);

    private static IO<T> GetData<T>(IRdkViewModel model, ProviderId<T> provider, bool forWrite) where T : class =>
        IO.lift(() => Conversions.Required((T?)model.GetData(provider.Id, forWrite, bAutoChangeBracket: true)));
}
