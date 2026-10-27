using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.Render.Sessions;
using Rasm.RhinoCommon.Render.Settings;
using Rasm.RhinoCommon.UI;
using Rhino;
using Rhino.Render;
using Rhino.Render.PostEffects;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;

namespace Rasm.RhinoCommon.Render.Effects;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record EffectSource {
    public HostEvent<Unit> Changed =>
        Switch(
            document: static held => ValueStores.Signal(EventKind.DocumentPropertiesChanged.In(held.Doc.RuntimeSerialNumber), static _ => true),
            section: static held => ValueStores.Signal(
                Subscriptions.Host<Rhino.UI.Controls.DataSource.EventArgs>(handler => held.Host.DataChanged += handler, handler => held.Host.DataChanged -= handler),
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

    public sealed record Tuning(Guid Id, string Key, object Value) : EffectRequest {
        public static Tuning Of<TState>(Guid effect, TState state) where TState : notnull => new(effect, EffectList.StateKey, EffectList.Encoded(state));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class EffectList {
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
    public const string StateKey = "state";

    public static IO<TState> State<TState>(RenderSettings settings, Guid effect, TState @default) where TState : notnull =>
        Within(settings, (_, held) => IO.lift(() => Resolved(held, effect)).Bind(data => Recalled(Stored(data), @default)));

    public static IO<TState> State<TState>(EffectSource source, Guid effect, TState @default) where TState : notnull =>
        source.Switch(
            (Id: effect, Default: @default),
            document: static (wanted, held) => RenderSettingsSources.Read(held.Doc, scope => Within(scope.Settings, (_, entries) =>
                entries.Find(data => data.Id == wanted.Id).Match(
                    Some: data => Recalled(Stored(data), wanted.Default),
                    None: () => IO.pure(wanted.Default)))),
            section: static (wanted, held) => Hosted(held.Host)
                .Bind(effects => IO.lift(effects.Find(effect => effect.Id == wanted.Id).ToFin(Missing(wanted.Id))))
                .Bind(effect => Recalled(Parameter(effect), wanted.Default)));

    public static Fin<TState> State<TState>(Rimage image, Guid effect, TState @default) where TState : notnull =>
        image.State.Find(effect).Bind(static fields => fields.Find(StateKey)).Match(Some: Decoded<TState>, None: () => @default);

    public static IO<Unit> Tune(EffectSource source, Seq<EffectRequest.Tuning> tunings, object owner) =>
        source.Switch(
            (Requests: tunings, Owner: owner),
            document: static (state, held) => RenderSettingsSources.Edit(held.Doc, scope => Apply(scope.Settings, state.Requests.Map(static request => (EffectRequest)request)), state.Owner),
            section: static (state, held) => SectionModels.Write(held.Host, Provider.RdkRenderingPostEffects, data => IO.lift(() => Listed(data) switch {
                var effects => Callbacks.Each(state.Requests, (request, _) => effects.Find(effect => effect.Id == request.Id).ToFin(Missing(request.Id))
                    .Bind(effect => RhinoFault.Unless(effect.SetParam(request.Key, request.Value)))),
            }).Map(unit)));

    internal static string Encoded<TState>(TState state) where TState : notnull => JsonSerializer.Serialize(state, ArchivableDictionaries.Options);

    internal static Fin<TState> Decoded<TState>(string text) where TState : notnull =>
        ArchivableDictionaries.Decoded(typeof(TState).Name, () => JsonSerializer.Deserialize<TState>(text, ArchivableDictionaries.Options));

    private static IO<TState> Recalled<TState>(IO<Option<string>> stored, TState @default) where TState : notnull =>
        stored.Bind(text => IO.lift(text.Match(Some: Decoded<TState>, None: () => (Fin<TState>)@default)));

    // --- [SCOPE]
    private static IO<A> Within<A>(RenderSettings settings, Func<PostEffectCollection, Seq<PostEffectData>, IO<A>> body) =>
        (from effects in unitIO.Bind(_ => Products.Made(settings.PostEffects))
         from held in Products.Made<PostEffectData>(effects)
         from value in body(effects, held)
         select value).Bracket();

    private static IO<Seq<PostEffect>> Hosted(EtoPostEffectCollapsibleSection host) =>
        SectionModels.Read(host, Provider.RdkRenderingPostEffects, static effects => IO.lift(() => Listed(effects)));

    private static Seq<PostEffect> Listed(IDisposable effects) =>
        toSeq(Enum.GetValues<PostEffectType>()).Bind(type => toSeq(GetPostEffects(effects, type))).Strict();

    private static Fin<PostEffectData> Resolved(Seq<PostEffectData> held, Guid id) =>
        held.Find(data => data.Id == id).ToFin(Missing(id));

    private static RhinoFault Missing(Guid id) => new($"post effect {id}", RhinoReason.Missing);

    // --- [PARAMETERS]
    private static IO<Option<string>> Stored(PostEffectData data) =>
        IO.lift(() => Optional(data.GetParameter(StateKey))).Bind(static found =>
            IO.lift(() => found.Map(static variant => variant.ToString(CultureInfo.InvariantCulture)))
                .Finally(IO.lift(() => found.Iter(static variant => ((IDisposable)variant).Dispose()))));

    private static IO<Option<string>> Parameter(PostEffect effect) =>
        IO.lift(() => {
            object? value = null;
            return effect.GetParam(StateKey, ref value) ? Optional(value as string) : None;
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
            ordering: static (scope, ordering) => Callbacks.Each(ordering.Ids, (id, index) => RhinoFault.Unless(scope.Effects.MovePostEffectBefore(id, Guid.Empty), index)).Map(static _ => unit),
            tuning: static (scope, tuning) => Resolved(scope.Held, tuning.Id).Bind(data => RhinoFault.Unless(data.SetParameter(tuning.Key, tuning.Value))));

    private static Guid Selected(PostEffectCollection effects) =>
        Conversions.Found(effects.GetSelectedPostEffect(ToneMappingNode, out Guid id), id).Bind(Conversions.Present).IfNone(static () => PostEffectUuids.ToneMapper_Clamp);
}
