using System.Globalization;
using System.Runtime.CompilerServices;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.Render.Settings;
using Rasm.Rhino.UI;
using Rasm.UI;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EffectSource {
    public HostEvent<Unit> Changed =>
        Switch(
            document: static held => ValueStore.Signal(EventKind.DocumentPropertiesChanged.In(held.Doc.RuntimeSerialNumber), static _ => true),
            section: static held => ValueStore.Signal(
                Subscriptions.Host<NativeDataSourceEventArgs>(
                    typeof(EtoCollapsibleSection), handler => held.Host.DataChanged += handler, handler => held.Host.DataChanged -= handler, nameof(EtoCollapsibleSection.DataChanged)),
                static args => args.DataType == ProviderIds.RdkRenderingPostEffects));

    public sealed record Document(RhinoDoc Doc) : EffectSource;

    public sealed record Section(EtoPostEffectCollapsibleSection Host) : EffectSource;
}

[Union]
public abstract partial record EffectEntry {
    private EffectEntry(Guid id) => Id = id;

    public Guid Id { get; }
    public bool Runs => Switch(listed: static listed => listed.On && listed.Shown, selectable: static selectable => selectable.Selected);

    public sealed record Listed(Guid Id, bool On, bool Shown) : EffectEntry(Id);

    public sealed record Selectable(Guid Id, bool Selected) : EffectEntry(Id);

    internal static EffectEntry Of(PostEffectData data, Guid selected) =>
        data.Type switch {
            PostEffectType.Early or PostEffectType.Late => new Listed(data.Id, data.On, data.Shown),
            PostEffectType.ToneMapping => new Selectable(data.Id, data.Id == selected),
        };

    internal static EffectEntry Of(PostEffect effect) =>
        effect.PostEffectType switch {
            PostEffectType.Early or PostEffectType.Late => new Listed(effect.Id, effect.On, effect.Shown),
            PostEffectType.ToneMapping => new Selectable(effect.Id, effect.IsSelected),
        };
}

[Union]
public abstract partial record EffectRequest {
    public sealed record Placing(EffectEntry Entry) : EffectRequest;

    public sealed record Ordering(Seq<Guid> Ids) : EffectRequest;

    public sealed record Tuning : EffectRequest {
        private Tuning(Guid id, string key, string text) => (Id, Key, Text) = (id, key, text);

        public Guid Id { get; }
        public string Key { get; }
        public string Text { get; }

        public static Seq<Tuning> Of<TState>(Guid effect, TState state) where TState : IStateRecord<TState> =>
            RecordEntries.Items<TState>().Choose(entry => entry.Capture(state).Map(text => new Tuning(effect, EntryKey.Name(entry.Path), text)));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectCollection {
    // --- [HOSTS]
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "RhinoDocumentId")]
    internal static extern uint RhinoDocumentId([UnsafeAccessorType("Rhino.Render.Rendering, RhinoCommon")] object rendering);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "GetPostEffects")]
    private static extern PostEffect[] GetPostEffects([UnsafeAccessorType("Rhino.Render.RenderingPostEffects, RhinoCommon")] object effects, PostEffectType type);

    // --- [ENTRIES]
    public static IO<Seq<EffectEntry>> Read(RenderSettings settings) =>
        Within(settings, static (effects, held) => IO.lift(() => {
            Guid selected = Selected(effects);
            return held.Map(data => EffectEntry.Of(data, selected)).Strict();
        }));

    public static IO<Seq<EffectEntry>> Read(EffectSource source) =>
        source.Switch(
            document: static held => RenderSettingsSources.Read(held.Doc, static scope => Read(scope.Settings)),
            section: static held => Hosted(held.Host).Map(static effects => effects.Map(EffectEntry.Of)));

    public static IO<Unit> Apply(RenderSettings settings, Seq<EffectRequest> requests) =>
        Within(settings, (effects, held) => IO.lift(() => Callbacks.Each(requests, (request, _) => Applied(effects, held, request))).Map(unit));

    // --- [STATE]
    public static IO<TState> State<TState>(RenderSettings settings, Guid effect) where TState : IStateRecord<TState> =>
        Within(settings, (_, held) => IO.lift(() => Resolved(held, effect)).Bind(data => Recalled<TState>(effect, name => Stored(data, name))));

    public static IO<TState> State<TState>(EffectSource source, Guid effect) where TState : IStateRecord<TState> =>
        source.Switch(
            effect,
            document: static (id, held) => RenderSettingsSources.Read(held.Doc, scope => Within(scope.Settings, (_, entries) =>
                entries.Find(data => data.Id == id).Match(
                    Some: data => Recalled<TState>(id, name => Stored(data, name)),
                    None: () => IO.pure(TState.Default)))),
            section: static (id, held) => Hosted(held.Host)
                .Bind(effects => IO.lift(effects.Find(effect => effect.Id == id).ToFin(new NotFound("post effect", $"{id}"))))
                .Bind(effect => Recalled<TState>(id, name => Parameter(effect, name))));

    public static IO<Unit> Tune(EffectSource source, Seq<EffectRequest.Tuning> tunings) =>
        source.Switch(
            tunings,
            document: static (requests, held) => RenderSettingsSources.Edit(held.Doc, scope => Apply(scope.Settings, requests.Map(static request => (EffectRequest)request))),
            section: static (requests, held) => SectionModels.Write(held.Host, Provider.RdkRenderingPostEffects, data => IO.lift(() => Listed(data) switch {
                var effects => Callbacks.Each(requests, (request, _) => effects.Find(effect => effect.Id == request.Id).ToFin(new NotFound("post effect", $"{request.Id}"))
                    .Bind(effect => Refused.Unless(effect.SetParam(request.Key, request.Text), nameof(PostEffect.SetParam)))),
            }).Map(unit)));

    private static IO<TState> Recalled<TState>(Guid effect, Func<string, IO<Option<string>>> stored) where TState : IStateRecord<TState> =>
        new FieldCodec<TState>(TState.Parameters, TState.Default).Recalled(EntryKey.TypeOwner(effect), path => stored(EntryKey.Name(path)));

    // --- [SCOPE]
    private static IO<A> Within<A>(RenderSettings settings, Func<PostEffectCollection, Seq<PostEffectData>, IO<A>> body) =>
        DisposalOps.Within(() => settings.PostEffects, effects => DisposalOps.Within(
            () => Conversions.Rows(effects).Strict(),
            held => body(effects, held),
            DisposalOps.Release));

    private static IO<Seq<PostEffect>> Hosted(EtoPostEffectCollapsibleSection host) =>
        SectionModels.Read(host, Provider.RdkRenderingPostEffects, static effects => IO.lift(() => Listed(effects)));

    private static Seq<PostEffect> Listed(IDisposable effects) =>
        toSeq(Enum.GetValues<PostEffectType>()).Bind(type => toSeq(GetPostEffects(effects, type))).Strict();

    private static Fin<PostEffectData> Resolved(Seq<PostEffectData> held, Guid id) =>
        held.Find(data => data.Id == id).ToFin(new NotFound("post effect", $"{id}"));

    // --- [PARAMETERS]
    private static IO<Option<string>> Stored(PostEffectData data, string name) =>
        DisposalOps.Within(
            () => Optional(data.GetParameter(name)),
            static found => IO.lift(() => found.Map(static variant => variant.ToString(CultureInfo.InvariantCulture))),
            static found => DisposalOps.Release(found.ToSeq().Map(static variant => (IDisposable)variant)));

    private static IO<Option<string>> Parameter(PostEffect effect, string name) =>
        IO.lift(() => {
            object? value = null;
            return effect.GetParam(name, ref value) ? Optional(value).Bind(static held => Optional(Convert.ToString(held, CultureInfo.InvariantCulture))) : None;
        });

    // --- [REQUESTS]
    private const PostEffectType ToneMappingNode = PostEffectType.ToneMapping + 1;

    private static Fin<Unit> Applied(PostEffectCollection effects, Seq<PostEffectData> held, EffectRequest request) =>
        request.Switch(
            (Effects: effects, Held: held),
            placing: static (scope, placing) => Resolved(scope.Held, placing.Entry.Id).Map(data => placing.Entry.Switch(
                (scope.Effects, Data: data),
                listed: static (target, listed) => {
                    target.Data.On = listed.On;
                    target.Data.Shown = listed.Shown;
                    return unit;
                },
                selectable: static (target, selectable) => {
                    if (selectable.Selected) target.Effects.SetSelectedPostEffect(ToneMappingNode, selectable.Id);
                    return unit;
                })),
            ordering: static (scope, ordering) => Callbacks.Each(
                ordering.Ids, id => scope.Effects.MovePostEffectBefore(id, Guid.Empty), nameof(PostEffectCollection.MovePostEffectBefore)),
            tuning: static (scope, tuning) => Resolved(scope.Held, tuning.Id)
                .Bind(data => Refused.Unless(data.SetParameter(tuning.Key, tuning.Text), nameof(PostEffectData.SetParameter))));

    private static Guid Selected(PostEffectCollection effects) =>
        Callbacks.Found(effects.GetSelectedPostEffect(ToneMappingNode, out Guid id), id).Bind(Conversions.Present).IfNone(static () => PostEffectUuids.ToneMapper_Clamp);
}
